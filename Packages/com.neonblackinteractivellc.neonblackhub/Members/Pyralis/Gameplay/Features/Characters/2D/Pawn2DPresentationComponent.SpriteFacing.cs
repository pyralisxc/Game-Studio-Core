using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    public sealed partial class Pawn2DPresentationComponent
    {
        private void TickSpriteFacingAndTintLane()
        {
            if (spriteRenderer == null || movement == null)
                return;
            if (movement.IsDead)
                return;

            if (IsMovingForPresentation())
                movingHoldTimer = idleDelay;
            else
                movingHoldTimer -= Time.deltaTime;
            bool moving = movingHoldTimer > 0f;

            if (animator == null)
                spriteRenderer.color = moving ? movingTint : idleTint;

            bool facingRight = movement.FacingRight;
            animationDriver?.SetFacing(facingRight);

            if (movement.MoveDirection.x > 0.05f)
            {
                spriteRenderer.flipX = !spriteDefaultFacesRight;
            }
            else if (movement.MoveDirection.x < -0.05f)
            {
                spriteRenderer.flipX = spriteDefaultFacesRight;
            }
        }
    }
}
