using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Modules.Character
{
    public interface IPawnTraversalModule
    {
        float ShimmyVelocityX { get; }

        void ApplyTraversalProfile(PawnProfileApplicationContext context, PawnTraversalProfile traversalProfile);
        bool HandleHangFrame(FrameInput frameInput);
        void ProbeLedge();
        void HandleInteract();
        void TriggerClimbUp();
        void TryLedgeGrab(IClimbZone zone, float maxVelocityY = 0f);
        void SetClimbZone(IClimbZone zone);
        void ClearClimbZone();
    }
}
