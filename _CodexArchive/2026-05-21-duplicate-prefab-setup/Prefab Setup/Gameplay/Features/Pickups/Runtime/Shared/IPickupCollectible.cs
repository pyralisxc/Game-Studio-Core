using UnityEngine;

namespace NeonBlack.Gameplay.Features.Pickups
{
    public interface IPickupCollectible
    {
        int FeedbackScoreValue { get; }
        void CollectBy(GameObject collector);
        bool RemoveFromPlay();
    }
}
