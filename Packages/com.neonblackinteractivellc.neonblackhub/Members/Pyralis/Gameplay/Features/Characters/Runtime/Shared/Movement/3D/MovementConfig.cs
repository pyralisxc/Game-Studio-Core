using NeonBlack.Gameplay.Core.Enums;

namespace NeonBlack.Gameplay.Features.Characters
{
    /// <summary>
    /// All tuning parameters consumed by <see cref="BrawlerMovementModel"/>.
    /// Filled by <see cref="Pawn3DMovementComponent"/> from serialized fields or profile data.
    /// Plain C# struct with no UnityEngine.Object dependency.
    /// </summary>
    public struct MovementConfig
    {
        public MovementMode MovementMode;
        public bool TopDownAllowJump;

        public bool AllowJump;
        public bool AllowDodge;
        public bool AllowCrouch;
        public bool AllowPowerSlide;

        public float WalkSpeed;
        public float SprintSpeed;
        public float CrouchSpeed;
        public float DepthSpeedMultiplier;

        public float AccelerationTime;
        public float DecelerationTime;

        public float JumpHeight;
        public float Gravity;
        public float CoyoteTime;
        public float JumpBufferTime;
        public float JumpCutMultiplier;
        public int MaxJumps;

        public float LandSquashThreshold;
        public float LandSlowDuration;
        public float LandSlowMultiplier;

        public float SlideAngle;
        public float SlideSpeed;
        public float SlideSteering;
        public float SlideBlendTime;

        public float WallSlideGravityMultiplier;
        public float WallSlideFallSpeedCap;

        public float DodgeDistance;
        public float DodgeDuration;
        public float DodgeCooldown;
        public float RollCooldown;

        public float PowerSlideDistance;
        public float PowerSlideDuration;
        public float PowerSlideCooldown;

        public float ClimbCooldown;
    }
}