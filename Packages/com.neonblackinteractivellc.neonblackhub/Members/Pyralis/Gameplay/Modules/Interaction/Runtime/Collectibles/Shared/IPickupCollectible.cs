using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Interaction
{
    public interface IPickupCollectible : IRemovableFromPlay
    {
        int FeedbackScoreValue { get; }
        void CollectBy(GameObject collector);
    }
}
