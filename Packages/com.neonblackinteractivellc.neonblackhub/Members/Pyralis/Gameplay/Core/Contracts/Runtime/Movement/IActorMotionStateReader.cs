using UnityEngine;

namespace NeonBlack.Gameplay.Core.Contracts
{
    public interface IActorMotionStateReader
    {
        Vector3 MotionVelocity { get; }
    }
}
