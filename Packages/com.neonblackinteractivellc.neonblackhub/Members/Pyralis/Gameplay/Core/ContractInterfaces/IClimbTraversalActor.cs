using UnityEngine;

namespace NeonBlack.Gameplay.Core.Contracts
{
    public interface IClimbTraversalActor
    {
        Vector3 CurrentVelocity { get; }
        void TryLedgeGrab(IClimbZone zone, float maxVelocityY = 0f);
        void SetClimbZone(IClimbZone zone);
        void ClearClimbZone();
    }
}
