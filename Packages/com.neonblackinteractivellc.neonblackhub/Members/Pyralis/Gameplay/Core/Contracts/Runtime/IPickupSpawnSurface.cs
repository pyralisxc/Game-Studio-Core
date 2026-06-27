using UnityEngine;

namespace NeonBlack.Gameplay.Core.Contracts
{
    public interface IPickupSpawnSurface
    {
        bool CanAcceptRuntimeSpawns { get; }
        bool TryGetSpawnPosition(out Vector2 position);
    }
}
