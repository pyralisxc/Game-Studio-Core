using UnityEngine;

namespace NeonBlack.Gameplay.Core.Contracts
{
    public interface IDamageNumberSink
    {
        void Spawn(float amount, Vector3 worldPos, bool isCritical = false);
        void SpawnHeal(float amount, Vector3 worldPos);
    }
}
