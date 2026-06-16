using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Enums;
using NeonBlack.Gameplay.Features.Combat;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Enemies
{
    /// <summary>
    /// Patrol, chase, and attack state-machine AI for 2.5D enemies.
    /// Decomposed into specific modules for movement, combat, detection, and animation.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.TacticsAggressive | AuthoringCapability.Steering3D, 
        Priority = AuthoringPriority.Primary,
        Lane = "AI",
        Relevance = "Canonical 3D/2.5D AI controller; handles patrol, detection, and attack states.",
        AssignmentFields = new[] { nameof(moveSpeed), nameof(enemyFeatureProfile), nameof(patrolPoints) },
        FirstProofTargetId = "proof.npc-enemy-behavior",
        FirstProof = "Place enemy and player in scene. Verify enemy enters 'Chase' state when player enters detection range.",
        NativeSetup = new[] { "Add EnemyAI to 3D actor.", "Assign EnemyFeatureProfile.", "Configure Detection Module ranges." },
        ExpertAdvice = "EnemyAI separates 'Tactics' and 'Steering'. Use 'EnemyFeatureProfile' to define shared stats like Aggro Range and Attack Cooldowns.",
        Axioms = AuthoringWorldAxiom.Realtime | AuthoringWorldAxiom.Dimensions3D,
        DocumentationURL = "https://docs.neonblack.com/pyralis/enemy-ai"
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Enemies/Enemy AI")]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(EnemyMovementModule))]
    [RequireComponent(typeof(EnemyDetectionModule))]
    [RequireComponent(typeof(EnemyCombatModule))]
    [RequireComponent(typeof(EnemyAnimationModule))]
    public partial class EnemyAI : MonoBehaviour, IActorMovementModifierReceiver, IActorCombatModifierReceiver, IEnemyActorState
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

        private EnemyActorRuntimeReferences _runtime;
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

        public EnemyMovementModule MovementModule => _runtime?.MovementModule;
        public EnemyDetectionModule DetectionModule => _runtime?.DetectionModule;
        public EnemyCombatModule CombatModule => _runtime?.CombatModule;
        public EnemyAnimationModule AnimationModule => _runtime?.AnimationModule;
        public MovementMode MovementMode => movementMode;
        public float MoveSpeed => moveSpeed;
        public float StatusMoveSpeedMultiplier => _statusMoveSpeedMultiplier;
        public Camera PresentationCamera => presentationCamera;
        public Transform VisualRoot => visualRoot;
        public bool SpriteDefaultFacesRight => spriteDefaultFacesRight;
        public float WaypointTolerance => waypointTolerance;

        private void Awake()
        {
            _runtime = EnemyActorRuntimeReferences.Resolve(gameObject);
            _spawnPos = transform.position;
            
            _states[EnemyState.Patrol] = new PatrolState();
            _states[EnemyState.Chase] = new ChaseState();
            _states[EnemyState.Attack] = new AttackState();

            ApplyFeatureProfile(enemyFeatureProfile);

            _runtime.ConfigureBillboard(transform, visualRoot, presentationCamera, spriteDefaultFacesRight);

            _runtime.HealthComponent?.OnDeath.AddListener(OnDeath);
            _runtime.HealthComponent?.OnDamaged.AddListener(OnHit);

            InitializeFeatureModules();
        }

        private void Start()
        {
            _states[_state].OnEnter(this);
        }

        private void Update()
        {
            if (_state == EnemyState.Dead) return;

            MovementModule.Tick(Time.deltaTime);
            CombatModule.Tick(Time.deltaTime);

            if ((_reactionState != null && _reactionState.IsReactionLocked) || _statusActionLocked)
            {
                UpdateAnimator();
                MovementModule.ApplyStationaryMotion(Time.deltaTime);
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
            bool isMoving = _state == EnemyState.Chase || (_state == EnemyState.Patrol && _runtime.Controller.velocity.sqrMagnitude > 0.05f);
            AnimationModule.UpdateMovement(isMoving, MovementModule.IsGrounded);
        }

        private void OnDeath()
        {
            if (_state != EnemyState.Dead)
                _states[_state].OnExit(this);

            _state = EnemyState.Dead;
            AnimationModule.TriggerDeath();
            _runtime.Controller.enabled = false;
            CombatModule.DisableAllHitBoxes();
        }

        private void OnHit(float damage)
        {
            AnimationModule.TriggerHurt();
            if (_state == EnemyState.Patrol && DetectionModule.HorizontalDistance(movementMode) < DetectionModule.LeashRange)
                ChangeState(EnemyState.Chase);
        }

        public void SetStatusMoveSpeedMultiplier(float multiplier) => _statusMoveSpeedMultiplier = Mathf.Max(multiplier, 0f);
        public void SetStatusActionLock(bool locked) => _statusActionLocked = locked;
        public void SetOutgoingDamageMultiplier(float multiplier) => CombatModule.SetOutgoingDamageMultiplier(multiplier);
        public void SetOutgoingKnockbackMultiplier(float multiplier) => CombatModule.SetOutgoingKnockbackMultiplier(multiplier);

        public void SetPresentationCamera(Camera camera)
        {
            presentationCamera = camera;
            _runtime?.SetPresentationCamera(camera);
        }

    }
}
