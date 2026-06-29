using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Character
{
    public sealed partial class Pawn2DMovementComponent
    {
        private void TickTopDownNoGravityMovement(float fixedDeltaTime)
        {
            Vector2 velocity = model.Tick(BuildMotorInput(), fixedDeltaTime);
            Vector2 newPos = rb2d.position + velocity * fixedDeltaTime;

            if (TryGetMovementBounds(out MovementBounds2D bounds))
                newPos = ApplyTopDownBounds(newPos, bounds);

            newPos = ApplyInputDeadZones(newPos);

            rb2d.MovePosition(newPos);
        }
    }
}
