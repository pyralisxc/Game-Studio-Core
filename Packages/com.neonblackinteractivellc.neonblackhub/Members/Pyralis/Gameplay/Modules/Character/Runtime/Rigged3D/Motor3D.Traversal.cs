using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Modules.Character
{
    public partial class Motor3D
    {
        public void TryLedgeGrab(IClimbZone zone, float maxVelocityY = 0f)
        {
            if (TraversalFeature != null) TraversalFeature.TryLedgeGrab(zone, maxVelocityY);
            else Traversal.TryLedgeGrab(zone, maxVelocityY);
        }

        public void SetClimbZone(IClimbZone zone)
        {
            if (TraversalFeature != null) TraversalFeature.SetClimbZone(zone);
            else Traversal.SetClimbZone(zone);
        }

        public void ClearClimbZone()
        {
            if (TraversalFeature != null) TraversalFeature.ClearClimbZone();
            else Traversal.ClearClimbZone();
        }
    }
}
