using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    public sealed partial class Pawn2DMovementComponent
    {
        private void TickTopDownNoGravityMovement()
        {
            Vector2 velocity = model.Tick(BuildMotorInput(), Time.fixedDeltaTime);
            Vector2 newPos = rb2d.position + velocity * Time.fixedDeltaTime;

            if (TryGetMovementBounds(out MovementBounds2D bounds))
                newPos = ApplyTopDownBounds(newPos, bounds);

            newPos = ApplyInputDeadZones(newPos);

            rb2d.MovePosition(newPos);
        }
    }
}
