using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Runtime;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Features.Input;
using UnityEngine;
using VContainer;

namespace NeonBlack.Gameplay.Features.Characters
{
    [AuthoringContract(
        Capability = AuthoringCapability.Movement, 
        PriorityValueOverride = 50,
        Relevance = "Tunable 2D movement module supporting top-down and side-view modes. PlayfieldProfile owns normal movement bounds; camera-visible bounds are an explicit arcade option.",
        Axioms = AuthoringWorldAxiom.Dimensions2D,
        NativeSetup = new[] 
        { 
            "Add Rigidbody2D and Collider2D.",
            "Keep on the same root as Motor2D.",
            "Configure LayerMasks for ground check if Jump is enabled."
        },
        AssignmentFields = new[] { nameof(moveSpeed), nameof(dashEnabled), nameof(dashSpeed), nameof(dashCooldown), nameof(jumpEnabled), nameof(jumpVelocity), nameof(groundLayer), nameof(inputZones) },
        FirstProofTargetId = "proof.1p-pawn-movement",
        FirstProof = "Pawn responds to input in the scene. Use the Scene View to verify the Ground Check raycast (if side-view) is hitting the correct layer.",
        ExpertAdvice = "Top-down/no-gravity route: leave Jump Enabled off; this component configures Rigidbody2D as Kinematic and Move drives X/Y. Side-view/gravity route: enable Jump; this component configures Rigidbody2D as Dynamic and lets gravity own vertical motion. Leave camera-visible movement bounds off unless the camera view itself is the legal play area."
    )]
[AddComponentMenu("NeonBlack/Gameplay/Characters/2D/Pawn 2D Movement Component")]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PolygonCollider2D))]
    public sealed partial class Pawn2DMovementComponent : MonoBehaviour, IPawnMotor, IMovementModule, IActorReactionResponder, IActorMovementModifierReceiver, IPawnRuntimeServicesReceiver, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (moveSpeed <= 0f)
                yield return "Move Speed must be greater than zero.";
            if (dashEnabled)
            {
                if (dashSpeed <= 0f) yield return "Dash Speed must be greater than zero when dash is enabled.";
                if (dashCooldown <= 0f) yield return "Dash Cooldown must be greater than zero when dash is enabled.";
            }
            if (jumpEnabled)
            {
                if (jumpVelocity <= 0f) yield return "Jump Velocity must be greater than zero when side-view jump is enabled.";
                if (gravityScale <= 0f) yield return "Gravity Scale must be greater than zero when side-view jump is enabled.";
            }
            if (useCameraVisibleBoundsForMovement && cameraBoundsProvider == null)
                yield return "Use Camera Visible Bounds For Movement is enabled, but no camera bounds provider has been supplied by the session. Assign GameplaySessionBootstrap.cameraRigController; prefer PlayfieldProfile for normal legal movement bounds.";
        }
        [Header("Movement - Speed")]
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField, Range(0f, 50f)] private float acceleration = 20f;
        [SerializeField, Range(0f, 50f)] private float deceleration = 25f;
        [SerializeField, Range(0f, 1f)] private float stopThreshold = 0.01f;

        [Header("Movement - Bounds")]
        [SerializeField] private float edgePadding = 0.05f;
        [SerializeField, Min(0f)] private float spriteRadius = 0.32f;
        [SerializeField] private Vector2 spriteRadiusOffset = Vector2.zero;
        [SerializeField] private bool showBoundsGizmo = true;
        [SerializeField] private bool screenWrap = false;
        [SerializeField, Tooltip("Use visible camera bounds as movement bounds when no PlayfieldProfile bounds are active. Leave off for normal authored movement; PlayfieldProfile owns legal movement space.")]
        private bool useCameraVisibleBoundsForMovement = false;
        [SerializeField, Tooltip("Optional gameplay state reader. When empty, the scene orchestrator should configure this component before play.")]
        private MonoBehaviour gameplayStateSource;

        [Header("Dash")]
        [SerializeField] private bool dashEnabled = true;
        [SerializeField] private float dashSpeed = 12f;
        [SerializeField, Range(0.05f, 0.5f)] private float dashDuration = 0.15f;
        [SerializeField, Range(0.1f, 3f)] private float dashCooldown = 0.8f;

        [Header("Side View Jump")]
        [SerializeField, Tooltip("Enable for side-view/platformer 2D pawns. Off means top-down/no-gravity movement with a Kinematic Rigidbody2D; on means side-view gravity with a Dynamic Rigidbody2D.")]
        private bool jumpEnabled = false;
        [SerializeField, Tooltip("Initial upward velocity applied when Jump is requested while grounded.")]
        private float jumpVelocity = 8f;
        [SerializeField, Tooltip("Gravity scale used while side-view jumping is enabled.")]
        private float gravityScale = 3f;
        [SerializeField, Tooltip("Maximum downward speed while side-view jumping is enabled.")]
        private float maxFallSpeed = 20f;
        [SerializeField, Tooltip("Layers this pawn treats as walkable ground for side-view jumping.")]
        private LayerMask groundLayer = Physics2D.DefaultRaycastLayers;
        [SerializeField, Tooltip("Ground check offset from the pawn root. Move it to the feet after the visual/collider are in place.")]
        private Vector2 groundCheckOffset = new Vector2(0f, -0.5f);
        [SerializeField, Min(0.01f), Tooltip("Radius used by the side-view ground check.")]
        private float groundCheckRadius = 0.12f;

        [Header("Dead Zones")]
        [SerializeField] private InputZoneSet inputZones;

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
        public bool FacingRight => facingRight;
        public bool IsDashing => model.State.IsDashing;
        public bool IsDead => model.State.IsDead;
        public float DashCooldownRemaining => Mathf.Max(0f, model.State.DashCooldownTimer);
        public bool IsActionLocked => combatActionLocked || statusActionLocked || reactionLockTimer > 0f;
        public float MoveSpeed => moveSpeed * statusMoveSpeedMultiplier;
        public bool IsGrounded => !jumpEnabled || isGrounded;
        public bool MovementEnabled => movementEnabled;
        public bool JumpEnabled => jumpEnabled;
        public Pawn2DMovementStyle EffectiveMovementStyle =>
            jumpEnabled ? Pawn2DMovementStyle.SideViewGravity : Pawn2DMovementStyle.TopDownNoGravity;
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

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!showBoundsGizmo)
                return;

            Vector3 centre = transform.position + (Vector3)spriteRadiusOffset;
            UnityEditor.Handles.color = new Color(0f, 1f, 0.4f, 0.35f);
            UnityEditor.Handles.DrawSolidDisc(centre, Vector3.back, spriteRadius);
            UnityEditor.Handles.color = new Color(0f, 1f, 0.4f, 1f);
            UnityEditor.Handles.DrawWireDisc(centre, Vector3.back, spriteRadius);
            UnityEditor.Handles.DrawSolidDisc(centre, Vector3.back, 0.02f);

            float total = spriteRadius + edgePadding;
            UnityEditor.Handles.color = new Color(1f, 0.6f, 0f, 0.6f);
            UnityEditor.Handles.DrawWireDisc(centre, Vector3.back, total);

            if (jumpEnabled)
            {
                Vector3 groundCheck = transform.position + (Vector3)groundCheckOffset;
                UnityEditor.Handles.color = isGrounded
                    ? new Color(0.2f, 0.8f, 1f, 0.8f)
                    : new Color(1f, 0.35f, 0.15f, 0.8f);
                UnityEditor.Handles.DrawWireDisc(groundCheck, Vector3.back, groundCheckRadius);
            }

            UnityEditor.EditorGUI.BeginChangeCheck();
            Vector3 newCentre = UnityEditor.Handles.PositionHandle(centre, Quaternion.identity);
            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                UnityEditor.Undo.RecordObject(this, "Move Sprite Radius Offset");
                spriteRadiusOffset = newCentre - transform.position;
            }
        }
#endif
    }
}
