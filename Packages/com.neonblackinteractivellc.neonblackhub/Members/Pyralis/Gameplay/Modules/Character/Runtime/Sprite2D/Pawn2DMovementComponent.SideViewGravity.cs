using NeonBlack.Gameplay.Data.Profiles;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Character
{
    public sealed partial class Pawn2DMovementComponent
    {
        private void TickSideViewGravityMovement(float fixedDeltaTime)
        {
            UpdateGroundedState();

            Vector2 velocity = rb2d.linearVelocity;
            float targetX = (IsActionLocked ? 0f : moveDirection.x) * MoveSpeed;
            float rate = Mathf.Abs(targetX) > 0.01f ? acceleration : deceleration;
            velocity.x = rate > 0f
                ? Mathf.MoveTowards(velocity.x, targetX, rate * fixedDeltaTime)
                : targetX;

            if (jumpQueued && isGrounded)
            {
                velocity.y = jumpVelocity;
                isGrounded = false;
            }

            jumpQueued = false;

            if (velocity.y < -maxFallSpeed)
                velocity.y = -maxFallSpeed;

            rb2d.linearVelocity = velocity;

            if (TryGetMovementBounds(out MovementBounds2D bounds))
                ClampSideViewPositionToHorizontalBounds(bounds);
        }

        private void UpdateGroundedState()
        {
            if (!jumpEnabled)
            {
                isGrounded = true;
                return;
            }

            Vector2 checkPosition = (Vector2)transform.position + groundCheckOffset;
            ContactFilter2D groundFilter = new ContactFilter2D
            {
                useLayerMask = true,
                useTriggers = false,
                layerMask = groundLayer
            };
            int hitCount = Physics2D.OverlapCircle(checkPosition, groundCheckRadius, groundFilter, groundCheckHits);
            isGrounded = false;
            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = groundCheckHits[i];
                if (hit == null || hit.transform.IsChildOf(transform))
                    continue;

                isGrounded = true;
                break;
            }
        }

        private void ConfigureRigidbodyForMovementMode()
        {
            if (rb2d == null)
                return;

            rb2d.bodyType = EffectiveMovementStyle == Pawn2DMovementStyle.SideViewGravity
                ? RigidbodyType2D.Dynamic
                : RigidbodyType2D.Kinematic;
            rb2d.gravityScale = EffectiveMovementStyle == Pawn2DMovementStyle.SideViewGravity
                ? gravityScale
                : 0f;
            rb2d.freezeRotation = true;

            if (EffectiveMovementStyle == Pawn2DMovementStyle.TopDownNoGravity)
                rb2d.linearVelocity = Vector2.zero;
        }
    }
}
