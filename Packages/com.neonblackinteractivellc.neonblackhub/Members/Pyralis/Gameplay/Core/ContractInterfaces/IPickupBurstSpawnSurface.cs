using UnityEngine;

namespace NeonBlack.Gameplay.Core.Contracts
{
    public interface IPickupBurstSpawnSurface
    {
        void SpawnCollectiblesAt(Vector2 center, int count, float radius = 0.5f);
    }
}
