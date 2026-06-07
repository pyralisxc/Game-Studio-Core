using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Characters;

namespace NeonBlack.Gameplay.Features.Traversal
{
    public interface IActorTraversalFeature
    {
        float ShimmyVelocityX { get; }
        void ProbeTraversal();
        bool HandleHangFrame(FrameInput frameInput);
        void TriggerClimbUp();
        void TryLedgeGrab(IClimbZone zone, float maxVelocityY = 0f);
        void SetClimbZone(IClimbZone zone);
        void ClearClimbZone();
    }
}
