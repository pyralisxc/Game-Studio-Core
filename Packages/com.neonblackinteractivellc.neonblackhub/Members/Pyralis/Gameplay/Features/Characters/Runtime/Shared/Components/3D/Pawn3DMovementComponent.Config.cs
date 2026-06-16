using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Core.Enums;
using UnityEngine;

namespace NeonBlack.Gameplay.Characters
{
    public sealed partial class Pawn3DMovementComponent
    {
        [Header("Movement")]
        [Tooltip("ThreeD = 2.5D brawler (X/Z). TwoD = side-scroller (X only). TopDown = bird's-eye (X/Z, no gravity).")]
        [SerializeField] private MovementMode movementMode = MovementMode.ThreeD;
        [Tooltip("TopDown only: enable gravity and jumping (Hades-style). Uncheck for Zelda/Pokemon style.")]
        [SerializeField] private bool topDownAllowJump;
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeed = 10f;
        [SerializeField] private float crouchSpeed = 2.5f;
        [Tooltip("Depth (W/S) speed multiplier. 0.6 compensates for a 30 degrees pitch camera.")]
        [Range(0.1f, 1f)]
        [SerializeField] private float depthSpeedMultiplier = 0.6f;
        [Tooltip("Seconds to reach full speed from a standstill.")]
        [SerializeField] private float accelerationTime = 0.08f;
        [Tooltip("Seconds to decelerate to a stop.")]
        [SerializeField] private float decelerationTime = 0.05f;

        [Header("Jump & Gravity")]
        [SerializeField] private bool allowJump = true;
        [SerializeField] private float jumpHeight = 3f;
        [SerializeField] private float gravity = -20f;
        [Tooltip("Seconds after leaving ground where a jump is still allowed (coyote time).")]
        [SerializeField] private float coyoteTime = 0.12f;
        [Tooltip("Seconds a jump press is stored before landing (jump buffer).")]
        [SerializeField] private float jumpBufferTime = 0.12f;
        [Tooltip("Velocity multiplier when jump is released early. Lower = shorter hops.")]
        [Range(0f, 1f)]
        [SerializeField] private float jumpCutMultiplier = 0.4f;
        [Tooltip("Total jumps allowed before landing. 2 = double jump.")]
        [SerializeField] private int maxJumps = 2;

        [Header("Land Impact")]
        [Tooltip("Minimum downward speed at impact that triggers a land squash. 0 = always.")]
        [SerializeField] private float landSquashThreshold = 5f;
        [Tooltip("Seconds movement is slowed after landing. 0 = disabled.")]
        [SerializeField] private float landSlowDuration = 0.2f;
        [Tooltip("Speed multiplier during the landing slow window.")]
        [Range(0f, 1f)]
        [SerializeField] private float landSlowMultiplier = 0.3f;

        [Header("Ground Check")]
        [Tooltip("Layers that count as ground.")]
        [SerializeField] private LayerMask groundLayer = Physics.DefaultRaycastLayers;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private float groundProbeExtraDistance = 0.08f;

        [Header("Scene References")]
        [Tooltip("Camera used to convert input into camera-relative movement. Leave empty for world-axis movement.")]
        [SerializeField] private Camera movementCamera;

        [Header("Dodge")]
        [SerializeField] private bool allowDodge;
        [SerializeField] private float dodgeDistance = 3f;
        [SerializeField] private float dodgeDuration = 0.4f;
        [SerializeField] private float dodgeCooldown = 0.8f;
        [SerializeField] private float rollCooldown = 1.2f;

        [Header("Slope Slide")]
        [Range(5f, 80f)]
        [SerializeField] private float slideAngle = 45f;
        [SerializeField] private float slideSpeed = 8f;
        [Range(0f, 1f)]
        [SerializeField] private float slideSteering = 0.5f;
        [SerializeField] private float slideBlendTime = 0.3f;

        [Header("Power Slide")]
        [SerializeField] private bool allowPowerSlide = true;
        [SerializeField] private float powerSlideDamage = 20f;
        [SerializeField] private float powerSlideKnockback = 6f;
        [SerializeField] private float powerSlideDistance = 4f;
        [SerializeField] private float powerSlideDuration = 0.45f;
        [SerializeField] private float powerSlideCooldown = 1f;
        [Tooltip("HitBox zone name activated during the slide. Must match Zone Name exactly (case-sensitive).")]
        [SerializeField] private string powerSlideHitBoxZone = "Kick";

        [Header("Wall Slide")]
        [SerializeField] private float wallSlideGravityMultiplier = 0.15f;
        [SerializeField] private float wallSlideFallSpeedCap = -4f;

        [Header("Crouch")]
        [SerializeField] private bool allowCrouch = true;
        [SerializeField] private float normalHeight = 2f;
        [SerializeField] private float crouchHeight = 1f;
        [SerializeField] private Vector3 normalCenter = new Vector3(0f, 1f, 0f);
        [SerializeField] private Vector3 crouchCenter = new Vector3(0f, 0.5f, 0f);

        private MovementConfig BuildConfig() => new MovementConfig
        {
            MovementMode = movementMode,
            TopDownAllowJump = topDownAllowJump,
            AllowJump = allowJump,
            AllowDodge = allowDodge,
            AllowCrouch = allowCrouch,
            AllowPowerSlide = allowPowerSlide,
            WalkSpeed = walkSpeed * _externalSpeedMultiplier,
            SprintSpeed = sprintSpeed * _externalSpeedMultiplier,
            CrouchSpeed = crouchSpeed * _externalSpeedMultiplier,
            DepthSpeedMultiplier = depthSpeedMultiplier,
            AccelerationTime = accelerationTime,
            DecelerationTime = decelerationTime,
            JumpHeight = jumpHeight,
            Gravity = gravity,
            CoyoteTime = coyoteTime,
            JumpBufferTime = jumpBufferTime,
            JumpCutMultiplier = jumpCutMultiplier,
            MaxJumps = maxJumps,
            LandSquashThreshold = landSquashThreshold,
            LandSlowDuration = landSlowDuration,
            LandSlowMultiplier = landSlowMultiplier,
            SlideAngle = slideAngle,
            SlideSpeed = slideSpeed,
            SlideSteering = slideSteering,
            SlideBlendTime = slideBlendTime,
            WallSlideGravityMultiplier = wallSlideGravityMultiplier,
            WallSlideFallSpeedCap = wallSlideFallSpeedCap,
            DodgeDistance = dodgeDistance,
            DodgeDuration = dodgeDuration,
            DodgeCooldown = dodgeCooldown,
            RollCooldown = rollCooldown,
            PowerSlideDistance = powerSlideDistance,
            PowerSlideDuration = powerSlideDuration,
            PowerSlideCooldown = powerSlideCooldown,
            ClimbCooldown = 0f, // owned by Pawn3DTraversalComponent
        };
    }
}
