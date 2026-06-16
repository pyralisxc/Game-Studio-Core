using NeonBlack.Gameplay.Features.Characters;

namespace NeonBlack.Gameplay.Characters
{
    public sealed partial class Pawn3DMovementComponent
    {
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
