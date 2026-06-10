using System.Collections;
using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Enums;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Presentation.Visuals;
using UnityEngine;
using VContainer;

namespace NeonBlack.Gameplay.Features.Enemies
{
    /// <summary>
    /// Patrol, chase, and attack state-machine AI for 2.5D enemies.
    /// Decomposed into specific modules for movement, combat, detection, and animation.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Combat | AuthoringCapability.Movement, 
        Relevance = "Inspector Add Component path for enemy AI behavior.",
        AssignmentFields = new[] { "aggroRange", "moveSpeed", "enemyFeatureProfile" },
        FirstProof = "Place the enemy in a scene with a player and verify it aggros and attacks when in range.",
        NativeSetup = new[] { "Add Component", "Assign HitBox Zones", "Configure Attack Sequence" }
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Enemies/Enemy AI")]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(EnemyMovementModule))]
    [RequireComponent(typeof(EnemyDetectionModule))]
    [RequireComponent(typeof(EnemyCombatModule))]
    [RequireComponent(typeof(EnemyAnimationModule))]
    public class EnemyAI : MonoBehaviour, IActorMovementModifierReceiver, IActorCombatModifierReceiver, IEnemyActorState
    {
        public enum EnemyState { Patrol, Chase, Attack, Dead }

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float waypointTolerance = 0.4f;
        [SerializeField] private MovementMode movementMode = MovementMode.ThreeD;

        [Header("Patrol Points")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float randomPatrolDistance = 4f;

        [Header("Visuals")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private bool spriteDefaultFacesRight = true;
        [SerializeField] private Camera presentationCamera;

        [Header("Profiles")]
        [SerializeField] private EnemyFeatureProfile enemyFeatureProfile;

        private CharacterController _controller;
        private EnemyMovementModule _movementModule;
        private EnemyDetectionModule _detectionModule;
        private EnemyCombatModule _combatModule;
        private EnemyAnimationModule _animationModule;
        private ActorFeatureHost _featureHost;
        private IEnemyReactionState _reactionState;

        private EnemyState _state = EnemyState.Patrol;
        private Vector3 _spawnPos;
        private int _patrolIndex;
        private Vector3 _randomPatrolTarget;
        private bool _hasRandomTarget;
        private bool _statusActionLocked;
        private float _statusMoveSpeedMultiplier = 1f;

        private readonly Dictionary<EnemyState, IEnemyAIState> _states = new Dictionary<EnemyState, IEnemyAIState>();

        public bool IsPatrolling => _state == EnemyState.Patrol;
        public bool IsChasing => _state == EnemyState.Chase;
        public bool IsAttacking => _state == EnemyState.Attack;

        public EnemyMovementModule MovementModule => _movementModule;
        public EnemyDetectionModule DetectionModule => _detectionModule;
        public EnemyCombatModule CombatModule => _combatModule;
        public EnemyAnimationModule AnimationModule => _animationModule;
        public MovementMode MovementMode => movementMode;
        public float MoveSpeed => moveSpeed;
        public float StatusMoveSpeedMultiplier => _statusMoveSpeedMultiplier;
        public Camera PresentationCamera => presentationCamera;
        public Transform VisualRoot => visualRoot;
        public bool SpriteDefaultFacesRight => spriteDefaultFacesRight;
        public float WaypointTolerance => waypointTolerance;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _movementModule = GetComponent<EnemyMovementModule>();
            _detectionModule = GetComponent<EnemyDetectionModule>();
            _combatModule = GetComponent<EnemyCombatModule>();
            _animationModule = GetComponent<EnemyAnimationModule>();
            _featureHost = GetComponent<ActorFeatureHost>();

            _spawnPos = transform.position;
            
            _states[EnemyState.Patrol] = new PatrolState();
            _states[EnemyState.Chase] = new ChaseState();
            _states[EnemyState.Attack] = new AttackState();

            ApplyFeatureProfile(enemyFeatureProfile);
            
            var billboard = GetComponent<BillboardFacing3D>();
            if (billboard != null)
            {
                billboard.Configure(
                    visualRoot != null ? visualRoot : transform,
                    visualRoot,
                    GetComponentInChildren<SpriteRenderer>(),
                    presentationCamera,
                    BillboardFacing3D.FacingMode.YAxisOnly,
                    spriteDefaultFacesRight);
            }

            GetComponent<HealthComponent>().OnDeath.AddListener(OnDeath);
            GetComponent<HealthComponent>().OnDamaged.AddListener(OnHit);

            InitializeFeatureModules();
        }

        private void Start()
        {
            _states[_state].OnEnter(this);
        }

        private void Update()
        {
            if (_state == EnemyState.Dead) return;

            _movementModule.Tick(Time.deltaTime);
            _combatModule.Tick(Time.deltaTime);

            if ((_reactionState != null && _reactionState.IsReactionLocked) || _statusActionLocked)
            {
                UpdateAnimator();
                _movementModule.ApplyStationaryMotion(Time.deltaTime);
                return;
            }

            _states[_state].OnUpdate(this, Time.deltaTime);

            UpdateAnimator();
        }

        public void ChangeState(EnemyState newState)
        {
            if (_state == newState) return;

            _states[_state].OnExit(this);
            _state = newState;
            _states[_state].OnEnter(this);
        }

        public Vector3 GetPatrolTarget()
        {
            if (patrolPoints != null && patrolPoints.Length > 0) return patrolPoints[_patrolIndex].position;
            if (!_hasRandomTarget)
            {
                float offset = Random.value > 0.5f ? randomPatrolDistance : -randomPatrolDistance;
                _randomPatrolTarget = _spawnPos + new Vector3(offset, 0f, 0f);
                _hasRandomTarget = true;
            }
            return _randomPatrolTarget;
        }

        public void AdvancePatrol()
        {
            if (patrolPoints != null && patrolPoints.Length > 0) _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
            else _hasRandomTarget = false;
        }

        private void UpdateAnimator()
        {
            bool isMoving = _state == EnemyState.Chase || (_state == EnemyState.Patrol && _controller.velocity.sqrMagnitude > 0.05f);
            _animationModule.UpdateMovement(isMoving, _movementModule.IsGrounded);
        }

        private void OnDeath()
        {
            if (_state != EnemyState.Dead)
                _states[_state].OnExit(this);

            _state = EnemyState.Dead;
            _animationModule.TriggerDeath();
            _controller.enabled = false;
            _combatModule.DisableAllHitBoxes();
        }

        private void OnHit(float damage)
        {
            _animationModule.TriggerHurt();
            if (_state == EnemyState.Patrol && _detectionModule.HorizontalDistance(movementMode) < _detectionModule.LeashRange)
                ChangeState(EnemyState.Chase);
        }

        public void SetStatusMoveSpeedMultiplier(float multiplier) => _statusMoveSpeedMultiplier = Mathf.Max(multiplier, 0f);
        public void SetStatusActionLock(bool locked) => _statusActionLocked = locked;
        public void SetOutgoingDamageMultiplier(float multiplier) => _combatModule.SetOutgoingDamageMultiplier(multiplier);
        public void SetOutgoingKnockbackMultiplier(float multiplier) => _combatModule.SetOutgoingKnockbackMultiplier(multiplier);

        public void SetPresentationCamera(Camera camera)
        {
            presentationCamera = camera;
            GetComponent<BillboardFacing3D>()?.SetCameraOverride(camera);
        }

        private void ApplyFeatureProfile(EnemyFeatureProfile profile)
        {
            if (profile == null) return;
            if (profile.combatProfile != null) _combatModule.ApplyCombatProfile(profile.combatProfile);
        }

        private void InitializeFeatureModules()
        {
            FeatureModuleDefinition[] definitions = enemyFeatureProfile != null ? enemyFeatureProfile.featureModules : null;
            if (definitions == null || definitions.Length == 0) return;
            _featureHost ??= gameObject.AddComponent<ActorFeatureHost>();
            _featureHost.InitializeFeatures(BuildFeatureContext(), definitions);
            _featureHost.TryGetInstalledFeature(out _reactionState);
        }

        private ActorFeatureContext BuildFeatureContext()
        {
            return new ActorFeatureContext(
                gameObject,
                health: GetComponent<HealthComponent>(),
                animation: GetComponent<ActorAnimationDriver>(),
                knockback: GetComponent<KnockbackReceiver>(),
                enemyActorState: this,
                presentationMode: GetComponent<ActorAnimationDriver>() != null ? GetComponent<ActorAnimationDriver>().PresentationMode : ActorPresentationMode.Billboard2_5D,
                authoredProfiles: new ScriptableObject[] { enemyFeatureProfile });
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_detectionModule == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _detectionModule.AggroRange);
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, _detectionModule.LeashRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _combatModule != null ? _combatModule.MinAttackRange : 1f);
        }
#endif
    }
}
