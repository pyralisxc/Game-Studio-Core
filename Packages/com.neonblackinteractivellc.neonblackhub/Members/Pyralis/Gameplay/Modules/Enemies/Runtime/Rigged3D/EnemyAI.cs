using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Enums;
using UnityEngine;
using UnityEngine.Serialization;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Enemies
{
    /// <summary>
    /// Patrol, chase, and attack state-machine AI for 2.5D enemies.
    /// Decomposed into specific modules for movement, combat, detection, and animation.
    /// </summary>
    [AuthoringContract(
        StableId = "enemy.ai.3d",
        Category = "Tactics Aggressive, Steering3 D",
        CapabilityPath = "Movement/Traversal/Enemy AI",
        Surface = AuthoringSurface.Goal,
        Summary = "Canonical 3D/2.5D AI controller; handles patrol, detection, and attack states.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/enemy-ai",
        RequiredFields = new[] { nameof(enemyProfile) },
        SetupSteps = new[] { "Add EnemyAI to 3D actor.", "Assign EnemyProfile.", "Configure Detection Module ranges." },
        SuccessChecks = new[] { "Place enemy and player in scene. Verify enemy enters 'Chase' state when player enters detection range." },
        Tags = new[] { "capability:TacticsAggressive", "capability:Steering3D", "axiom:Realtime", "axiom:Dimensions3D", "lane:AI", "priority:Primary" }
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Enemies/Enemy AI")]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(EnemyMovementModule))]
    [RequireComponent(typeof(EnemyDetectionModule))]
    [RequireComponent(typeof(EnemyAnimationModule))]
    public partial class EnemyAI : GameplayTickBehaviour, IActorMovementModifierReceiver, IActorCombatModifierReceiver, IEnemyActorState
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
        [FormerlySerializedAs("enemyFeatureProfile")]
        [SerializeField] private EnemyProfile enemyProfile;

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
        public IActorCombatRequestReceiver CombatRequests => _runtime?.CombatRequestReceiver;
        public IActorCombatTacticalState CombatTactics => _runtime?.CombatTacticalState;
        public IActorCombatModifierReceiver CombatModifiers => _runtime?.CombatModifierReceiver;
        public EnemyAnimationModule AnimationModule => _runtime?.AnimationModule;
        public MovementMode MovementMode => movementMode;
        public float MoveSpeed => moveSpeed;
        public float StatusMoveSpeedMultiplier => _statusMoveSpeedMultiplier;
        public Camera PresentationCamera => presentationCamera;
        public Transform VisualRoot => visualRoot;
        public bool SpriteDefaultFacesRight => spriteDefaultFacesRight;
        public float WaypointTolerance => waypointTolerance;
        protected override GameplayTickDomain TickDomain => GameplayTickDomain.Enemies;
        protected override bool UsesGameplayTick => true;

        private void Awake()
        {
            _runtime = EnemyActorRuntimeReferences.Resolve(gameObject);
            _spawnPos = transform.position;
            
            _states[EnemyState.Patrol] = new PatrolState();
            _states[EnemyState.Chase] = new ChaseState();
            _states[EnemyState.Attack] = new AttackState();

            ApplyProfile(enemyProfile);

            _runtime.ConfigureBillboard(transform, visualRoot, presentationCamera, spriteDefaultFacesRight);

            if (_runtime.Health != null)
            {
                _runtime.Health.Died += OnDeath;
                _runtime.Health.Damaged += OnHit;
            }

            ResolveDirectCapabilities();
        }

        private void OnDestroy()
        {
            if (_runtime?.Health == null)
                return;

            _runtime.Health.Died -= OnDeath;
            _runtime.Health.Damaged -= OnHit;
        }

        private void Start()
        {
            _states[_state].OnEnter(this);
        }

        protected override void OnGameplayTick(in GameplayTickContext context)
        {
            if (_state == EnemyState.Dead) return;

            MovementModule.Tick(context.DeltaTime);

            if ((_reactionState != null && _reactionState.IsReactionLocked) || _statusActionLocked)
            {
                UpdateAnimator();
                MovementModule.ApplyStationaryMotion(context.DeltaTime);
                return;
            }

            _states[_state].OnUpdate(this, context.DeltaTime);

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
            CombatTactics?.DisableAllHitBoxes();
        }

        private void OnHit(float damage)
        {
            AnimationModule.TriggerHurt();
            if (_state == EnemyState.Patrol && DetectionModule.HorizontalDistance(movementMode) < DetectionModule.LeashRange)
                ChangeState(EnemyState.Chase);
        }

        public void SetStatusMoveSpeedMultiplier(float multiplier) => _statusMoveSpeedMultiplier = Mathf.Max(multiplier, 0f);
        public void SetStatusActionLock(bool locked) => _statusActionLocked = locked;
        public void SetOutgoingDamageMultiplier(float multiplier) => CombatModifiers?.SetOutgoingDamageMultiplier(multiplier);
        public void SetOutgoingKnockbackMultiplier(float multiplier) => CombatModifiers?.SetOutgoingKnockbackMultiplier(multiplier);

        public void SetPresentationCamera(Camera camera)
        {
            presentationCamera = camera;
            _runtime?.SetPresentationCamera(camera);
        }

    }
}
