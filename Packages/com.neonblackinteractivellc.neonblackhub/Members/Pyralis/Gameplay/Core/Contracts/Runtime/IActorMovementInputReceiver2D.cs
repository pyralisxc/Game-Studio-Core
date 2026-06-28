using UnityEngine;

namespace NeonBlack.Gameplay.Core.Contracts
{
    public interface IActorMovementInputReceiver2D
    {
        Vector2 MoveDirection { get; set; }
        bool IsDead { get; }
        void Jump();
        bool TryDash(Vector2 direction);
    }
}
