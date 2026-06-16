using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Enums;
using NeonBlack.Gameplay.Features.Characters;
using UnityEngine;

namespace NeonBlack.Gameplay.Characters
{
    /// <summary>
    /// Movement module for a 3D pawn. Owns the <see cref="BrawlerMovementModel"/>,
    /// drives the <see cref="CharacterController"/>, and manages crouch capsule resizing.
    ///
    /// Implements <see cref="IPawnMotor"/> so <see cref="PawnRoot"/> discovers it without a wrapper.
    /// Implements <see cref="IMovementModule"/> for AI and network locomotion entry points.
    ///
    /// Setup:
    ///    Attach on the same root as <see cref="Motor3D"/>.
    ///    Tune movement, jump, and gravity fields in the Inspector.
    ///    Assign the ground layer mask to match your terrain layer.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Movement,
        PriorityValueOverride = 50,
        Relevance = "Core 3D movement motor; handles walking, jumping, gravity, and ground detection.",
        Axioms = AuthoringWorldAxiom.Realtime | AuthoringWorldAxiom.Dimensions3D,
        NativeSetup = new[] { "Attach to a Pawn root with CharacterController.", "Assign Ground Layer mask." },
        AssignmentFields = new[] { nameof(movementMode), nameof(groundLayer), nameof(walkSpeed), nameof(jumpHeight) },
        FirstProof = "Verify the pawn can walk and jump in Play Mode.",
        ExpertAdvice = "Movement Component uses CharacterController.Move(). Ensure the Ground Layer mask does not include the 'Player' layer to prevent the pawn from trying to ground itself on its own collider."
    )]
[AddComponentMenu("NeonBlack/Gameplay/3D/Pawn 3D Movement Component")]
    [RequireComponent(typeof(CharacterController))]
    public sealed partial class Pawn3DMovementComponent : MonoBehaviour, IPawnMotor, IMovementModule
    {
        //  Movement  //
        [Header("Movement")]
        [Tooltip("ThreeD = 2.5D brawler (X/Z). TwoD = side-scroller (X only). TopDown = bird's-eye (X/Z, no gravity).")]
        [SerializeField] private MovementMode movementMode        = MovementMode.ThreeD;
        [Tooltip("TopDown only: enable gravity and jumping (Hades-style). Uncheck for Zelda/Pokemon style.")]
        [SerializeField] private bool  topDownAllowJump;
        [SerializeField] private float walkSpeed                  = 5f;
        [SerializeField] private float sprintSpeed                = 10f;
        [SerializeField] private float crouchSpeed                = 2.5f;
        [Tooltip("Depth (W/S) speed multiplier. 0.6 compensates for a 30 degrees pitch camera.")]
        [Range(0.1f, 1f)]
        [SerializeField] private float depthSpeedMultiplier       = 0.6f;
        [Tooltip("Seconds to reach full speed from a standstill.")]
        [SerializeField] private float accelerationTime           = 0.08f;
        [Tooltip("Seconds to decelerate to a stop.")]
        [SerializeField] private float decelerationTime           = 0.05f;

        //  Jump & Gravity  //
        [Header("Jump & Gravity")]
        [SerializeField] private bool allowJump = true;
        [SerializeField] private float jumpHeight                 = 3f;
        [SerializeField] private float gravity                    = -20f;
        [Tooltip("Seconds after leaving ground where a jump is still allowed (coyote time).")]
        [SerializeField] private float coyoteTime                 = 0.12f;
        [Tooltip("Seconds a jump press is stored before landing (jump buffer).")]
        [SerializeField] private float jumpBufferTime             = 0.12f;
        [Tooltip("Velocity multiplier when jump is released early. Lower = shorter hops.")]
        [Range(0f, 1f)]
        [SerializeField] private float jumpCutMultiplier          = 0.4f;
        [Tooltip("Total jumps allowed before landing. 2 = double jump.")]
        [SerializeField] private int   maxJumps                   = 2;

        //  Land Impact  //
        [Header("Land Impact")]
        [Tooltip("Minimum downward speed at impact that triggers a land squash. 0 = always.")]
        [SerializeField] private float landSquashThreshold        = 5f;
        [Tooltip("Seconds movement is slowed after landing. 0 = disabled.")]
        [SerializeField] private float landSlowDuration           = 0.2f;
        [Tooltip("Speed multiplier during the landing slow window.")]
        [Range(0f, 1f)]
        [SerializeField] private float landSlowMultiplier         = 0.3f;

        //  Ground Check  //
        [Header("Ground Check")]
        [Tooltip("Layers that count as ground.")]
        [SerializeField] private LayerMask groundLayer            = Physics.DefaultRaycastLayers;
        [SerializeField] private float groundCheckRadius          = 0.2f;
        [SerializeField] private float groundProbeExtraDistance   = 0.08f;

        [Header("Scene References")]
        [Tooltip("Camera used to convert input into camera-relative movement. Leave empty for world-axis movement.")]
        [SerializeField] private Camera movementCamera;

        //  Dodge  //
        [Header("Dodge")]
        [SerializeField] private bool allowDodge;
        [SerializeField] private float dodgeDistance              = 3f;
        [SerializeField] private float dodgeDuration              = 0.4f;
        [SerializeField] private float dodgeCooldown              = 0.8f;
        [SerializeField] private float rollCooldown               = 1.2f;

        //  Slope Slide  //
        [Header("Slope Slide")]
        [Range(5f, 80f)]
        [SerializeField] private float slideAngle                 = 45f;
        [SerializeField] private float slideSpeed                 = 8f;
        [Range(0f, 1f)]
        [SerializeField] private float slideSteering              = 0.5f;
        [SerializeField] private float slideBlendTime             = 0.3f;

        //  Power Slide  //
        [Header("Power Slide")]
        [SerializeField] private bool allowPowerSlide = true;
        [SerializeField] private float  powerSlideDamage          = 20f;
        [SerializeField] private float  powerSlideKnockback       = 6f;
        [SerializeField] private float  powerSlideDistance        = 4f;
        [SerializeField] private float  powerSlideDuration        = 0.45f;
        [SerializeField] private float  powerSlideCooldown        = 1f;
        [Tooltip("HitBox zone name activated during the slide. Must match Zone Name exactly (case-sensitive).")]
        [SerializeField] private string powerSlideHitBoxZone      = "Kick";

        //  Wall Slide  //
        [Header("Wall Slide")]
        [SerializeField] private float wallSlideGravityMultiplier = 0.15f;
        [SerializeField] private float wallSlideFallSpeedCap      = -4f;

        //  Crouch  //
        [Header("Crouch")]
        [SerializeField] private bool allowCrouch = true;
        [SerializeField] private float   normalHeight             = 2f;
        [SerializeField] private float   crouchHeight             = 1f;
        [SerializeField] private Vector3 normalCenter             = new Vector3(0f, 1f, 0f);
        [SerializeField] private Vector3 crouchCenter             = new Vector3(0f, 0.5f, 0f);

        //  Component references  //
        private Pawn3DMovementRuntimeReferences _runtime;
        private Camera              _cam;
        private float               _capsuleSkin;
        private float               _externalSpeedMultiplier = 1f;

        //  Movement model  //
        private readonly BrawlerMovementModel _model        = new BrawlerMovementModel();
        private          MovementPhysicsFrame _physicsFrame = MovementPhysicsFrame.Default;
        private          MovementConfig       _config;

        //  IMovementModule  //
        public float MoveSpeed  => _model.State.IsSprinting ? sprintSpeed : walkSpeed;
        public bool  IsGrounded => _model.State.IsGrounded;

        //  Exposed state (consumed by traversal and presentation modules)  //
        /// <summary>Read-only snapshot of current movement state.</summary>
        public MovementState State              => _model.State;
        public int           MaxJumps           => maxJumps;
        public float         DodgeDuration      => _config.DodgeDuration;
        public LayerMask     GroundLayer        => groundLayer;
        public float         LandSquashThreshold => landSquashThreshold;
        public string        PowerSlideHitBoxZone => powerSlideHitBoxZone;
        public float         PowerSlideDamage   => powerSlideDamage;
        public float         PowerSlideKnockback => powerSlideKnockback;

        //  Unity lifecycle  //
        private void Awake()
        {
            _runtime = Pawn3DMovementRuntimeReferences.Capture(gameObject);
            _cam         = movementCamera;
            _capsuleSkin = Mathf.Max(_runtime.Controller.skinWidth, 0.01f);

            // Use CharacterController live values as source-of-truth so existing
            // configs remain stable without matching serialized field defaults.
            normalHeight = _runtime.Controller.height;
            normalCenter = _runtime.Controller.center;
            if (crouchHeight >= normalHeight)
                crouchHeight = Mathf.Max(0.5f, normalHeight * 0.5f);
            crouchCenter = normalCenter - Vector3.up * ((normalHeight - crouchHeight) * 0.5f);

            _config = BuildConfig();
            _model.Configure(_config);
        }

        //  Per-frame API (called by Motor3D)  //
        /// <summary>Reset the physics frame accumulator before recording a fresh CharacterController move.</summary>
        public void ResetPhysicsFrame() => _physicsFrame = MovementPhysicsFrame.Default;

        /// <summary>
        /// Tick the movement model and return the world-space velocity for this frame.
        /// Pass the result to <see cref="ApplyMovement"/> after traversal checks.
        /// </summary>
        public Vector3 Tick(FrameInput fi) =>
            _model.Tick(BuildMovementInput(fi), _physicsFrame, Time.deltaTime);

        /// <summary>
        /// Apply model velocity + knockback via CharacterController and record
        /// this frame's physics results for the next <see cref="Tick"/> call.
        /// </summary>
        //  Dodge & power slide  //
        /// <summary>Request a dodge roll. Returns true if the model accepted it.</summary>
        public bool TryStartDodge(Vector2 moveInput) => _model.TryStartDodge(moveInput);

        /// <summary>Request a power slide. Returns true if the model accepted it.</summary>
        public bool TryStartPowerSlide() => _model.TryStartPowerSlide();

        //  Traversal notifications (called by Pawn3DTraversalComponent)  //
        public void NotifyClimbStart(float cooldown) => _model.NotifyClimbStart(cooldown);
        public void NotifyClimbEnd()                 => _model.NotifyClimbEnd();
        public void NotifyHangStart()                => _model.NotifyHangStart(maxJumps);
        public void NotifyHangEnd()                  => _model.NotifyHangEnd();
        public void SetVelocityY(float vy)           => _model.SetVelocityY(vy);

        //  Motor state mutations (called by Motor3D / presentation)  //
        public void TriggerKnockBack()               => _model.TriggerKnockBack();
        public void SetActing(bool acting)           => _model.SetActing(acting);

        //  IMovementModule (AI / network locomotion)  //
        public void Move(Vector2 input)
        {
            var fi = new FrameInput { Move = input };
            ApplyMovement(_model.Tick(BuildMovementInput(fi), _physicsFrame, Time.deltaTime));
        }

        public void Jump()
        {
            var fi = new FrameInput { JumpPressed = true };
            _model.Tick(BuildMovementInput(fi), _physicsFrame, Time.deltaTime);
        }

        public void SetMovementEnabled(bool enabled) => _runtime.Controller.enabled = enabled;

        public void SetExternalSpeedMultiplier(float multiplier)
        {
            _externalSpeedMultiplier = Mathf.Max(multiplier, 0f);
            _config = BuildConfig();
            _model.Configure(_config);
        }

        public void SetMovementCamera(Camera camera)
        {
            movementCamera = camera;
            _cam = camera;
        }

        //  IPawnMotor  //
        private MovementInput BuildMovementInput(FrameInput fi) => new MovementInput
        {
            Move                       = fi.Move,
            MoveWorld                  = ResolvePlanarMove(fi.Move),
            SprintHeld                 = fi.SprintHeld,
            JumpPressed                = fi.JumpPressed,
            JumpReleased               = fi.JumpReleased,
            AttackTimerActive          = _runtime.Combat != null && _runtime.Combat.AttackTimer > 0f,
            KickTimerActive            = _runtime.Combat != null && _runtime.Combat.KickTimer > 0f,
            AttackMoveMultiplier       = _runtime.Combat?.AttackMoveMultiplier ?? 1f,
            AerialAttackMoveMultiplier = _runtime.Combat?.AerialAttackMoveMultiplier ?? 1f,
            CameraRight                = _cam != null ? _cam.transform.right : Vector3.right,
        };

        private Vector3 ResolvePlanarMove(Vector2 move)
        {
            if (move.sqrMagnitude <= 0f)
                return Vector3.zero;

            if (movementMode == MovementMode.TwoD)
                return Vector3.right * Mathf.Clamp(move.x, -1f, 1f);

            Vector3 right = Vector3.right;
            Vector3 forward = Vector3.forward;
            if (_cam != null)
            {
                right = Vector3.ProjectOnPlane(_cam.transform.right, Vector3.up);
                forward = Vector3.ProjectOnPlane(_cam.transform.forward, Vector3.up);
                if (right.sqrMagnitude <= 0.0001f)
                    right = Vector3.right;
                if (forward.sqrMagnitude <= 0.0001f)
                    forward = Vector3.forward;
            }

            right.Normalize();
            forward.Normalize();
            Vector3 planarMove = right * move.x + forward * (move.y * depthSpeedMultiplier);
            return planarMove.sqrMagnitude > 1f ? planarMove.normalized : planarMove;
        }

    }
}
