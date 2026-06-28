using NeonBlack.Gameplay.Data.Profiles;

namespace NeonBlack.Gameplay.Data.Participants
{
    public interface IPawnTraversalMovementController
    {
        bool IsGrounded { get; }
        bool IsCrouching { get; }
        bool IsClimbing { get; }
        bool IsHanging { get; }
        bool IsActing { get; }
        float VelocityY { get; }
        float JumpBufferCounter { get; }
        float ClimbTimer { get; }

        void ApplyTraversalProfile(PawnTraversalProfile traversalProfile);
        void NotifyClimbStart(float cooldown);
        void NotifyClimbEnd();
        void NotifyHangStart();
        void NotifyHangEnd();
        void SetVelocityY(float velocityY);
    }
}
