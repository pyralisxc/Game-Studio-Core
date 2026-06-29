using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Input;
using NeonBlack.Gameplay.Data.Profiles;

namespace NeonBlack.Gameplay.Data.Participants
{
    public interface IPawnTraversalModule
    {
        float ShimmyVelocityX { get; }

        void ApplyTraversalProfile(PawnProfileApplicationContext context, PawnTraversalProfile traversalProfile);
        bool HandleHangFrame(FrameInput frameInput, float deltaTime);
        void ProbeLedge();
        void HandleInteract();
        void TriggerClimbUp();
        void TryLedgeGrab(IClimbZone zone, float maxVelocityY = 0f);
        void SetClimbZone(IClimbZone zone);
        void ClearClimbZone();
    }
}
