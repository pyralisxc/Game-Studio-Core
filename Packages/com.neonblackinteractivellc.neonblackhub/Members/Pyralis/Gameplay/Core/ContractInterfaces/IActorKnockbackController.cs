using UnityEngine;

namespace NeonBlack.Gameplay.Core.Contracts
{
    public interface IActorKnockbackController
    {
        Vector3 Velocity { get; }
        void Tick(float deltaTime);
        void ApplyKnockback(Vector3 forceVector);
        void ClearKnockback();
    }
}
