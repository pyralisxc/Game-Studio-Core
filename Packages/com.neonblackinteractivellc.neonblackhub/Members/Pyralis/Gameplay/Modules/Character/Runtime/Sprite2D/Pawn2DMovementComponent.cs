using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Modules.Actor.Composition;
using UnityEngine;
using VContainer;

namespace NeonBlack.Gameplay.Modules.Character
{
    [AuthoringContract(
        Capability = AuthoringCapability.Movement, 
        CapabilityPath = "Movement/Sprite2D/Movement Component",
        PriorityValueOverride = 50,
        Relevance = "Tunable 2D movement module supporting top-down and side-view modes. PlayfieldProfile owns normal movement bounds; camera-visible bounds are an explicit arcade option.",
        Axioms = AuthoringWorldAxiom.Dimensions2D,
        RoleTags = new[] { "Movement2D", "TopDownNoGravity", "SideViewGravity" },
        NativeSetup = new[] 
        { 
            "Add Rigidbody2D and Collider2D.",
            "Keep on the same root as Motor2D.",
            "Set Movement Style to SideViewGravity only for platformer-style gravity and ground checks."
        },
        AssignmentFields = new[] { nameof(movementStyle), nameof(moveSpeed), nameof(dashEnabled), nameof(dashSpeed), nameof(dashCooldown), nameof(jumpEnabled), nameof(jumpVelocity), nameof(groundLayer), nameof(inputZones) },
        ProofTargetId = "proof.1p-pawn-movement",
        Proof = "Pawn responds to input in the scene. For top-down routes, verify Move drives X/Y on the map plane; for side-view routes, verify the ground check hits the correct layer.",
        RuntimeFamilies = new[] { RuntimeCapabilityFamily.CharacterPawnGameplay },
        ExpertAdvice = "Top-down/no-gravity route: keep Movement Style as TopDownNoGravity so Rigidbody2D stays Kinematic and Move drives X/Y. If Jump should visually hop without physics gravity, install a TopDownHop feature module that consumes the Jump action. Side-view/gravity route: set Movement Style to SideViewGravity and enable Jump for Dynamic Rigidbody2D vertical motion. Leave camera-visible movement bounds off unless the camera view itself is the legal play area."
    )]
[AddComponentMenu("NeonBlack/Gameplay/Modules/Character/Sprite2D/Pawn 2D Movement Component")]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PolygonCollider2D))]
    public sealed partial class Pawn2DMovementComponent : MonoBehaviour, IPawnMotor, IMovementModule, IPawnLocomotionStateReader, IActorReactionResponder, IActorMovementModifierReceiver, IPawnRuntimeServicesReceiver, IRuntimeValidationProvider
    {
        private readonly Motor2DModel model = new Motor2DModel();
        private readonly Collider2D[] groundCheckHits = new Collider2D[8];

        private Rigidbody2D rb2d;
        private ICameraBoundsProvider cameraBoundsProvider;
        private IPlayfieldBoundsProvider playfieldBoundsProvider;
        private IGameplayStateReader gameplayStateReader;
        private Vector2 moveDirection;
        private bool facingRight = true;
        private bool combatActionLocked;
        private bool statusActionLocked;
        private bool movementEnabled = true;
        private bool missingRuntimeServicesLogged;
        private float reactionLockTimer;
        private float statusMoveSpeedMultiplier = 1f;
        private bool jumpQueued;
        private bool isGrounded = true;

        private float TotalMargin => spriteRadius + edgePadding;

        public Vector2 MoveDirection
        {
            get => moveDirection;
            set => moveDirection = value;
        }

        public Vector2 CurrentVelocity => model.State.CurrentVelocity;
        public PawnLocomotionState LocomotionState => PawnLocomotionStateMachine.Resolve(model.State, movementEnabled, IsGrounded);
        public bool FacingRight => facingRight;
        public bool IsDashing => model.State.IsDashing;
        public bool IsDead => model.State.IsDead;
        public float DashCooldownRemaining => Mathf.Max(0f, model.State.DashCooldownTimer);
        public bool IsActionLocked => combatActionLocked || statusActionLocked || reactionLockTimer > 0f;
        public float MoveSpeed => moveSpeed * statusMoveSpeedMultiplier;
        public bool IsGrounded => EffectiveMovementStyle != Pawn2DMovementStyle.SideViewGravity || isGrounded;
        public bool MovementEnabled => movementEnabled;
        public bool JumpEnabled => jumpEnabled;
        public Pawn2DMovementStyle EffectiveMovementStyle => movementStyle;
        public bool RuntimeGrounded => isGrounded;
        public bool RuntimeJumpQueued => jumpQueued;
        public Object RuntimeGameplayStateSource => gameplayStateReader as Object;
        public Object RuntimeCameraBoundsSource => cameraBoundsProvider as Object;
        public Object RuntimePlayfieldBoundsSource => playfieldBoundsProvider as Object;

        public bool TryGetRuntimeGameplayActive(out bool isGameplayActive)
        {
            isGameplayActive = gameplayStateReader != null && gameplayStateReader.IsGameplayActive;
            return gameplayStateReader != null;
        }

        public bool TryGetRuntimeCameraBounds(out CameraBounds2D bounds)
        {
            return TryGetCameraBounds(out bounds) && bounds.IsValid;
        }

        private void Awake()
        {
            rb2d = GetComponent<Rigidbody2D>();
            rb2d.bodyType = RigidbodyType2D.Kinematic;
            rb2d.gravityScale = 0f;
            gameplayStateReader = gameplayStateSource as IGameplayStateReader;
            model.Configure(BuildMotorConfig());
            ConfigureRigidbodyForMovementMode();
        }

        [Inject]
        private void Construct(IGameplayStateReader stateReader = null)
        {
            gameplayStateReader ??= stateReader;
        }

        public void ConfigureRuntime(
            IGameplayStateReader stateReader,
            ICameraBoundsProvider boundsProvider,
            IPlayfieldBoundsProvider playfieldProvider = null)
        {
            if (stateReader != null)
                gameplayStateReader = stateReader;
            if (boundsProvider != null)
                cameraBoundsProvider = boundsProvider;
            if (playfieldProvider != null)
                playfieldBoundsProvider = playfieldProvider;
        }

        public void ApplyRuntimeServices(PawnRuntimeServicesContext context)
        {
            ConfigureRuntime(context.GameplayStateReader, context.CameraBoundsProvider, context.PlayfieldBoundsProvider);
        }

        private void Start()
        {
            ClampPositionToBounds();
        }

        private void FixedUpdate()
        {
            if (!HasRequiredRuntimeServices())
                return;
            if (!gameplayStateReader.IsGameplayActive)
                return;
            if (!movementEnabled || model.State.IsDead)
                return;

            if (EffectiveMovementStyle == Pawn2DMovementStyle.SideViewGravity)
            {
                TickSideViewGravityMovement();
                return;
            }

            TickTopDownNoGravityMovement();
        }

        private void Update()
        {
            if (reactionLockTimer > 0f)
            {
                reactionLockTimer = Mathf.Max(0f, reactionLockTimer - Time.deltaTime);
                if (reactionLockTimer <= 0f)
                    moveDirection = Vector2.zero;
            }

            if (moveDirection.x > 0.05f)
                facingRight = true;
            else if (moveDirection.x < -0.05f)
                facingRight = false;
        }

        public void Move(Vector2 input)
        {
            moveDirection = input;
        }

        public void Jump()
        {
            if (jumpEnabled && !IsActionLocked && !model.State.IsDead)
                jumpQueued = true;
        }

        public void SetMovementEnabled(bool enabled)
        {
            movementEnabled = enabled;
            if (!enabled)
                moveDirection = Vector2.zero;
        }

        public bool TryDash(Vector2 direction) => model.TryDash(direction);

        public void ResetForRound(Vector3 position)
        {
            model.ResetForRound();
            combatActionLocked = false;
            statusActionLocked = false;
            reactionLockTimer = 0f;
            moveDirection = Vector2.zero;
            transform.position = position;

            if (rb2d != null)
            {
                rb2d.linearVelocity = Vector2.zero;
                rb2d.angularVelocity = 0f;
                rb2d.position = position;
                ConfigureRigidbodyForMovementMode();
            }
        }

        public void NotifyDeath()
        {
            if (model.State.IsDead)
                return;

            model.NotifyDead();
            moveDirection = Vector2.zero;
        }

        public void ResetMoveToIdle()
        {
            moveDirection = Vector2.zero;
        }

        public void SetActionLock(bool locked)
        {
            combatActionLocked = locked;
            if (locked)
                moveDirection = Vector2.zero;
        }

        public void ApplyReactionLock(float duration)
        {
            reactionLockTimer = Mathf.Max(reactionLockTimer, duration);
            moveDirection = Vector2.zero;
        }

        public void ClearReactionLock()
        {
            reactionLockTimer = 0f;
        }

        public void SetStatusMoveSpeedMultiplier(float multiplier)
        {
            statusMoveSpeedMultiplier = Mathf.Max(multiplier, 0f);
            model.Configure(BuildMotorConfig());
        }

        public void SetStatusActionLock(bool locked)
        {
            statusActionLocked = locked;
            if (locked)
                moveDirection = Vector2.zero;
        }

        public void ApplyMovementProfile(PawnProfileApplicationContext context, PawnMovementProfile profile)
        {
            if (profile == null)
                return;

            moveSpeed = profile.walkSpeed;
            acceleration = profile.acceleration;
            deceleration = profile.deceleration;
            screenWrap = profile.allowScreenWrap;
            dashEnabled = profile.allow2DDash;
            dashSpeed = profile.dashSpeed;
            dashDuration = profile.dashDuration;
            dashCooldown = profile.dashCooldown;
            movementStyle = profile.Effective2DMovementStyle;
            jumpEnabled = profile.allow2DJump;
            jumpVelocity = profile.jumpVelocity2D;
            gravityScale = profile.gravityScale2D;
            model.Configure(BuildMotorConfig());
            ConfigureRigidbodyForMovementMode();
        }

        private bool HasRequiredRuntimeServices()
        {
            if (gameplayStateReader != null)
                return true;

            if (!missingRuntimeServicesLogged)
            {
                missingRuntimeServicesLogged = true;
                Debug.LogError("[Pawn2DMovementComponent] Missing gameplay state service. Configure a gameplay state reader or let GameplaySessionBootstrap initialize this pawn through ParticipantSpawnService.", this);
            }

            return false;
        }

        private Motor2DInput BuildMotorInput() => new Motor2DInput
{
            MoveDirection = IsActionLocked ? Vector2.zero : moveDirection
        };

        private Motor2DConfig BuildMotorConfig() => new Motor2DConfig
        {
            MoveSpeed = moveSpeed * statusMoveSpeedMultiplier,
            Acceleration = acceleration,
            Deceleration = deceleration,
            StopThreshold = stopThreshold,
            DashEnabled = dashEnabled,
            DashSpeed = dashSpeed * statusMoveSpeedMultiplier,
            DashDuration = dashDuration,
            DashCooldown = dashCooldown
        };

    }
}
