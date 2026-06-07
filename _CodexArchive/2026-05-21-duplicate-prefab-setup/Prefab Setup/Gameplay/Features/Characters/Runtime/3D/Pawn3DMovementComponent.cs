using System.Linq;
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
    ///   â€¢ Attach on the same root as <see cref="Motor3D"/>.
    ///   â€¢ Tune movement, jump, and gravity fields in the Inspector.
    ///   â€¢ Assign the ground layer mask to match your terrain layer.
    /// </summary>
    [AddComponentMenu("NeonBlack/Gameplay/3D/Pawn 3D Movement Component")]
    [RequireComponent(typeof(CharacterController))]
    public sealed class Pawn3DMovementComponent : MonoBehaviour, IPawnMotor, IMovementModule
    {
        // â”€â”€ Movement â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        [Header("Movement")]
        [Tooltip("ThreeD = 2.5D brawler (X/Z). TwoD = side-scroller (X only). TopDown = bird's-eye (X/Z, no gravity).")]
        [SerializeField] private MovementMode movementMode        = MovementMode.ThreeD;
        [Tooltip("TopDown only: enable gravity and jumping (Hades-style). Uncheck for Zelda/Pokemon style.")]
        [SerializeField] private bool  topDownAllowJump;
        [SerializeField] private float walkSpeed                  = 5f;
        [SerializeField] private float sprintSpeed                = 10f;
        [SerializeField] private float crouchSpeed                = 2.5f;
        [Tooltip("Depth (W/S) speed multiplier. 0.6 compensates for a 30Â° pitch camera.")]
        [Range(0.1f, 1f)]
        [SerializeField] private float depthSpeedMultiplier       = 0.6f;
        [Tooltip("Seconds to reach full speed from a standstill.")]
        [SerializeField] private float accelerationTime           = 0.08f;
        [Tooltip("Seconds to decelerate to a stop.")]
        [SerializeField] private float decelerationTime           = 0.05f;

        // â”€â”€ Jump & Gravity â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        [Header("Jump & Gravity")]
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

        // â”€â”€ Land Impact â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        [Header("Land Impact")]
        [Tooltip("Minimum downward speed at impact that triggers a land squash. 0 = always.")]
        [SerializeField] private float landSquashThreshold        = 5f;
        [Tooltip("Seconds movement is slowed after landing. 0 = disabled.")]
        [SerializeField] private float landSlowDuration           = 0.2f;
        [Tooltip("Speed multiplier during the landing slow window.")]
        [Range(0f, 1f)]
        [SerializeField] private float landSlowMultiplier         = 0.3f;

        // â”€â”€ Ground Check â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        [Header("Ground Check")]
        [Tooltip("Layers that count as ground.")]
        [SerializeField] private LayerMask groundLayer            = Physics.DefaultRaycastLayers;
        [SerializeField] private float groundCheckRadius          = 0.2f;
        [SerializeField] private float groundProbeExtraDistance   = 0.08f;

        // â”€â”€ Dodge â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        [Header("Dodge")]
        [SerializeField] private float dodgeDistance              = 3f;
        [SerializeField] private float dodgeDuration              = 0.4f;
        [SerializeField] private float dodgeCooldown              = 0.8f;
        [SerializeField] private float rollCooldown               = 1.2f;

        // â”€â”€ Slope Slide â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        [Header("Slope Slide")]
        [Range(5f, 80f)]
        [SerializeField] private float slideAngle                 = 45f;
        [SerializeField] private float slideSpeed                 = 8f;
        [Range(0f, 1f)]
        [SerializeField] private float slideSteering              = 0.5f;
        [SerializeField] private float slideBlendTime             = 0.3f;

        // â”€â”€ Power Slide â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        [Header("Power Slide")]
        [SerializeField] private float  powerSlideDamage          = 20f;
        [SerializeField] private float  powerSlideKnockback       = 6f;
        [SerializeField] private float  powerSlideDistance        = 4f;
        [SerializeField] private float  powerSlideDuration        = 0.45f;
        [SerializeField] private float  powerSlideCooldown        = 1f;
        [Tooltip("HitBox zone name activated during the slide. Must match Zone Name exactly (case-sensitive).")]
        [SerializeField] private string powerSlideHitBoxZone      = "Kick";

        // â”€â”€ Wall Slide â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        [Header("Wall Slide")]
        [SerializeField] private float wallSlideGravityMultiplier = 0.15f;
        [SerializeField] private float wallSlideFallSpeedCap      = -4f;

        // â”€â”€ Crouch â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        [Header("Crouch")]
        [SerializeField] private float   normalHeight             = 2f;
        [SerializeField] private float   crouchHeight             = 1f;
        [SerializeField] private Vector3 normalCenter             = new Vector3(0f, 1f, 0f);
        [SerializeField] private Vector3 crouchCenter             = new Vector3(0f, 0.5f, 0f);

        // â”€â”€ Component references â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        private CharacterController _controller;
        private IActorKnockbackController _knockback;
        private IPawnCombatMovementContext _combat;
        private Camera              _cam;
        private float               _capsuleSkin;
        private float               _externalSpeedMultiplier = 1f;

        // â”€â”€ Movement model â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        private readonly BrawlerMovementModel _model        = new BrawlerMovementModel();
        private          MovementPhysicsFrame _physicsFrame = MovementPhysicsFrame.Default;
        private          MovementConfig       _config;

        // â”€â”€ IMovementModule â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        public float MoveSpeed  => _model.State.IsSprinting ? sprintSpeed : walkSpeed;
        public bool  IsGrounded => _model.State.IsGrounded;

        // â”€â”€ Exposed state (consumed by traversal and presentation modules) â”€â”€â”€ //
        /// <summary>Read-only snapshot of current movement state.</summary>
        public MovementState State              => _model.State;
        public int           MaxJumps           => maxJumps;
        public float         DodgeDuration      => _config.DodgeDuration;
        public LayerMask     GroundLayer        => groundLayer;
        public float         LandSquashThreshold => landSquashThreshold;
        public string        PowerSlideHitBoxZone => powerSlideHitBoxZone;
        public float         PowerSlideDamage   => powerSlideDamage;
        public float         PowerSlideKnockback => powerSlideKnockback;

        // â”€â”€ Unity lifecycle â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        private void Awake()
        {
            _controller  = GetComponent<CharacterController>();
            _knockback   = GetComponents<MonoBehaviour>().OfType<IActorKnockbackController>().FirstOrDefault();
            _combat      = GetComponents<MonoBehaviour>().OfType<IPawnCombatMovementContext>().FirstOrDefault();
            _cam         = Camera.main;
            _capsuleSkin = Mathf.Max(_controller.skinWidth, 0.01f);

            // Use CharacterController live values as source-of-truth so existing
            // configs remain stable without matching serialized field defaults.
            normalHeight = _controller.height;
            normalCenter = _controller.center;
            if (crouchHeight >= normalHeight)
                crouchHeight = Mathf.Max(0.5f, normalHeight * 0.5f);
            crouchCenter = normalCenter - Vector3.up * ((normalHeight - crouchHeight) * 0.5f);

            _config = BuildConfig();
            _model.Configure(_config);
        }

        // â”€â”€ Per-frame API (called by Motor3D) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        /// <summary>Reset the physics frame accumulator. Call at the top of each Update.</summary>
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
        public void ApplyMovement(Vector3 modelVelocity)
        {
            if (!_controller.enabled) return;

            _knockback.Tick(Time.deltaTime);
            CollisionFlags flags = _controller.Move((modelVelocity + _knockback.Velocity) * Time.deltaTime);

            bool byCollision = (flags & CollisionFlags.Below) != 0;
            bool byProbe     = false;
            if (!byCollision && modelVelocity.y <= 0f)
            {
                float   radius = Mathf.Clamp(groundCheckRadius, 0.02f, _controller.radius * 0.95f);
                Vector3 origin = GetGroundProbeOrigin();
                byProbe = Physics.SphereCast(origin, radius, Vector3.down, out _,
                    Mathf.Max(groundProbeExtraDistance, 0.02f), groundLayer, QueryTriggerInteraction.Ignore);
            }
            _physicsFrame.GroundedByCollision = byCollision;
            _physicsFrame.GroundedByProbe     = byProbe;
        }

        /// <summary>
        /// Feed a CharacterController surface contact into this frame's physics accumulator.
        /// Call from <see cref="Motor3D.OnControllerColliderHit"/>.
        /// </summary>
        public void NotifyColliderHit(ControllerColliderHit hit)
        {
            if (hit.normal.y > 0.1f)
            {
                _physicsFrame.GroundNormal = hit.normal;
            }
            else if (hit.normal.y >= -0.1f)
            {
                Vector3 moveDir = new Vector3(_model.State.VelocityX, 0f, _model.State.VelocityZ);
                if (moveDir.sqrMagnitude > 0.01f && Vector3.Dot(moveDir.normalized, -hit.normal) > 0.3f)
                    _physicsFrame.HasWallContact = true;
            }
        }

        // â”€â”€ Crouch â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        /// <summary>Enter or exit crouch. Respects ceiling clearance when standing up.</summary>
        public void SetCrouch(bool crouch)
        {
            if (crouch)
            {
                _model.SetCrouching(true);
                _controller.height = crouchHeight;
                _controller.center = crouchCenter;
                return;
            }
            if (!CanStandUp()) return;
            _model.SetCrouching(false);
            _controller.height = normalHeight;
            _controller.center = normalCenter;
        }

        private bool CanStandUp()
        {
            Vector3 center = transform.TransformPoint(normalCenter);
            float   radius = Mathf.Max(_controller.radius - _capsuleSkin, 0.01f);
            float   half   = Mathf.Max(normalHeight * 0.5f - radius, 0f);
            return !Physics.CheckCapsule(
                center + Vector3.up * half, center - Vector3.up * half,
                radius, groundLayer, QueryTriggerInteraction.Ignore);
        }

        // â”€â”€ Dodge & power slide â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        /// <summary>Request a dodge roll. Returns true if the model accepted it.</summary>
        public bool TryStartDodge(Vector2 moveInput) => _model.TryStartDodge(moveInput);

        /// <summary>Request a power slide. Returns true if the model accepted it.</summary>
        public bool TryStartPowerSlide() => _model.TryStartPowerSlide();

        // â”€â”€ Traversal notifications (called by Pawn3DTraversalComponent) â”€â”€â”€â”€ //
        public void NotifyClimbStart(float cooldown) => _model.NotifyClimbStart(cooldown);
        public void NotifyClimbEnd()                 => _model.NotifyClimbEnd();
        public void NotifyHangStart()                => _model.NotifyHangStart(maxJumps);
        public void NotifyHangEnd()                  => _model.NotifyHangEnd();
        public void SetVelocityY(float vy)           => _model.SetVelocityY(vy);

        // â”€â”€ Motor state mutations (called by Motor3D / presentation) â”€â”€â”€â”€â”€â”€â”€ //
        public void TriggerKnockBack()               => _model.TriggerKnockBack();
        public void SetActing(bool acting)           => _model.SetActing(acting);

        // â”€â”€ IMovementModule (AI / network locomotion) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
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

        public void SetMovementEnabled(bool enabled) => _controller.enabled = enabled;

        public void SetExternalSpeedMultiplier(float multiplier)
        {
            _externalSpeedMultiplier = Mathf.Max(multiplier, 0f);
            _config = BuildConfig();
            _model.Configure(_config);
        }

        // â”€â”€ IPawnMotor â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        public void ApplyMovementProfile(PawnProfileApplicationContext context, PawnMovementProfile profile)
        {
            if (profile == null) return;
            movementMode         = profile.movementMode;
            walkSpeed            = profile.walkSpeed;
            sprintSpeed          = profile.sprintSpeed;
            crouchSpeed          = profile.crouchSpeed;
            depthSpeedMultiplier = profile.depthSpeedMultiplier;
            _config = BuildConfig();
            _model.Configure(_config);
        }

        /// <summary>Apply traversal tuning (jump, dodge, gravity) from a profile.</summary>
        public void ApplyTraversalProfile(PawnTraversalProfile profile)
        {
            if (profile == null) return;
            jumpHeight    = profile.jumpHeight;
            gravity       = profile.gravity;
            dodgeDistance = profile.dodgeDistance;
            dodgeDuration = profile.dodgeDuration;
            dodgeCooldown = profile.dodgeCooldown;
            _config = BuildConfig();
            _model.Configure(_config);
        }

        // â”€â”€ Helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        private Vector3 GetGroundProbeOrigin()
        {
            Bounds b = _controller.bounds;
            return new Vector3(b.center.x, b.min.y + 0.02f, b.center.z);
        }

        private MovementInput BuildMovementInput(FrameInput fi) => new MovementInput
        {
            Move                       = fi.Move,
            SprintHeld                 = fi.SprintHeld,
            JumpPressed                = fi.JumpPressed,
            JumpReleased               = fi.JumpReleased,
            AttackTimerActive          = _combat != null && _combat.AttackTimer > 0f,
            KickTimerActive            = _combat != null && _combat.KickTimer > 0f,
            AttackMoveMultiplier       = _combat?.AttackMoveMultiplier ?? 1f,
            AerialAttackMoveMultiplier = _combat?.AerialAttackMoveMultiplier ?? 1f,
            CameraRight                = _cam != null ? _cam.transform.right : Vector3.right,
        };

        private MovementConfig BuildConfig() => new MovementConfig
        {
            MovementMode               = movementMode,
            TopDownAllowJump           = topDownAllowJump,
            WalkSpeed                  = walkSpeed * _externalSpeedMultiplier,
            SprintSpeed                = sprintSpeed * _externalSpeedMultiplier,
            CrouchSpeed                = crouchSpeed * _externalSpeedMultiplier,
            DepthSpeedMultiplier       = depthSpeedMultiplier,
            AccelerationTime           = accelerationTime,
            DecelerationTime           = decelerationTime,
            JumpHeight                 = jumpHeight,
            Gravity                    = gravity,
            CoyoteTime                 = coyoteTime,
            JumpBufferTime             = jumpBufferTime,
            JumpCutMultiplier          = jumpCutMultiplier,
            MaxJumps                   = maxJumps,
            LandSquashThreshold        = landSquashThreshold,
            LandSlowDuration           = landSlowDuration,
            LandSlowMultiplier         = landSlowMultiplier,
            SlideAngle                 = slideAngle,
            SlideSpeed                 = slideSpeed,
            SlideSteering              = slideSteering,
            SlideBlendTime             = slideBlendTime,
            WallSlideGravityMultiplier = wallSlideGravityMultiplier,
            WallSlideFallSpeedCap      = wallSlideFallSpeedCap,
            DodgeDistance              = dodgeDistance,
            DodgeDuration              = dodgeDuration,
            DodgeCooldown              = dodgeCooldown,
            RollCooldown               = rollCooldown,
            PowerSlideDistance         = powerSlideDistance,
            PowerSlideDuration         = powerSlideDuration,
            PowerSlideCooldown         = powerSlideCooldown,
            ClimbCooldown              = 0f, // owned by Pawn3DTraversalComponent
        };

        // â”€â”€ Debug gizmos â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying || _controller == null) return;
            Bounds  b      = _controller.bounds;
            Vector3 origin = new Vector3(b.center.x, b.min.y + 0.02f, b.center.z);
            float   radius = Mathf.Clamp(groundCheckRadius, 0.02f, _controller.radius * 0.95f);
            Gizmos.color = _model.State.IsGrounded ? new Color(0f, 1f, 0f, 0.4f) : new Color(1f, 0f, 0f, 0.4f);
            Gizmos.DrawSphere(origin, radius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin, origin + Vector3.down * Mathf.Max(groundProbeExtraDistance, 0.02f));
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(new Vector3(b.center.x, b.min.y, b.center.z), 0.03f);
        }
#endif
    }
}
