using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Enums;
using NeonBlack.Gameplay.Core.Types.Input;
using NeonBlack.Gameplay.Data.Participants;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Character
{
    /// <summary>
    /// Movement module for a 3D pawn. Owns the <see cref="Rigged3DMovementModel"/>,
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
        Category = "Movement",
        CapabilityPath = "Movement/Traversal/Pawn3D Movement Component",
        Surface = AuthoringSurface.Goal,
        Summary = "Core 3D movement motor; handles walking, jumping, gravity, and ground detection.",
        RequiredFields = new[] { nameof(movementMode), nameof(groundLayer), nameof(walkSpeed), nameof(jumpHeight) },
        SetupSteps = new[] { "Attach to a Pawn root with CharacterController.", "Assign Ground Layer mask." },
        SuccessChecks = new[] { "Verify the pawn can walk and jump in Play Mode." },
        Tags = new[] { "capability:Movement", "axiom:Realtime", "axiom:Dimensions3D" }
    )]
[AddComponentMenu("NeonBlack/Gameplay/Modules/Character/Rigged3D/Pawn 3D Movement Component")]
    [RequireComponent(typeof(CharacterController))]
    public sealed partial class Pawn3DMovementComponent : MonoBehaviour, IPawnMotor, IMovementModule, IPawnLocomotionStateReader, IPawnTraversalMovementController
    {
        //  Component references  //
        private Pawn3DMovementRuntimeReferences _runtime;
        private Camera              _cam;
        private float               _capsuleSkin;
        private float               _externalSpeedMultiplier = 1f;

        //  Movement model  //
        private readonly Rigged3DMovementModel _model        = new Rigged3DMovementModel();
        private          MovementPhysicsFrame _physicsFrame = MovementPhysicsFrame.Default;
        private          MovementConfig       _config;

        //  IMovementModule  //
        public float MoveSpeed  => _model.State.IsSprinting ? sprintSpeed : walkSpeed;
        public bool  IsGrounded => _model.State.IsGrounded;
        public bool IsCrouching => _model.State.IsCrouching;
        public bool IsClimbing => _model.State.IsClimbing;
        public bool IsHanging => _model.State.IsHanging;
        public bool IsActing => _model.State.IsActing;
        public float VelocityY => _model.State.VelocityY;
        public float JumpBufferCounter => _model.State.JumpBufferCounter;
        public float ClimbTimer => _model.State.ClimbTimer;

        //  Exposed state (consumed by traversal and presentation modules)  //
        /// <summary>Read-only snapshot of current movement state.</summary>
        public MovementState State              => _model.State;
        public PawnLocomotionState LocomotionState => _model.LocomotionState;
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
        public Vector3 Tick(FrameInput fi, float deltaTime) =>
            _model.Tick(BuildMovementInput(fi), _physicsFrame, deltaTime);

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
        public void Move(Vector2 input, float deltaTime)
        {
            var fi = new FrameInput { Move = input };
            ApplyMovement(_model.Tick(BuildMovementInput(fi), _physicsFrame, deltaTime), deltaTime);
        }

        public void Jump(float deltaTime)
        {
            var fi = new FrameInput { JumpPressed = true };
            _model.Tick(BuildMovementInput(fi), _physicsFrame, deltaTime);
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
