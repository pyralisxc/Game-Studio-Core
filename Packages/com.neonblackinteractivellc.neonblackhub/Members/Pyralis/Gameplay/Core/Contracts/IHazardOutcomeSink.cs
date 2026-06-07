using UnityEngine;

namespace NeonBlack.Gameplay.Core.Contracts
{
    public interface IHazardOutcomeSink
    {
        bool TryHandleHazardImpact(GameObject target, GameObject source, Vector3 hitPoint);
    }
}
