using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Input;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    public sealed partial class Pawn2DMovementComponent
    {
        private readonly struct MovementBounds2D
        {
            public MovementBounds2D(Vector2 center, float halfWidth, float halfHeight, bool allowScreenWrap)
            {
                Center = center;
                HalfWidth = Mathf.Max(0f, halfWidth);
                HalfHeight = Mathf.Max(0f, halfHeight);
                AllowScreenWrap = allowScreenWrap;
            }

            public Vector2 Center { get; }
            public float HalfWidth { get; }
            public float HalfHeight { get; }
            public bool AllowScreenWrap { get; }
            public bool IsValid => HalfWidth > 0f && HalfHeight > 0f;
        }

        private bool TryGetMovementBounds(out MovementBounds2D bounds)
        {
            if (playfieldBoundsProvider != null
                && playfieldBoundsProvider.TryGetPlayfieldBounds2D(TotalMargin, out PlayfieldBounds2D playfieldBounds)
                && playfieldBounds.IsValid)
            {
                bounds = new MovementBounds2D(
                    playfieldBounds.Center,
                    playfieldBounds.HalfWidth,
                    playfieldBounds.HalfHeight,
                    playfieldBounds.AllowScreenWrap);
                return true;
            }

            if (useCameraVisibleBoundsForMovement
                && TryGetCameraBounds(out CameraBounds2D cameraBounds)
                && cameraBounds.IsValid)
            {
                bounds = new MovementBounds2D(
                    cameraBounds.Center,
                    cameraBounds.HalfWidth,
                    cameraBounds.HalfHeight,
                    screenWrap);
                return true;
            }

            bounds = default;
            return false;
        }

        private bool TryGetCameraBounds(out CameraBounds2D bounds)
        {
            if (cameraBoundsProvider != null && cameraBoundsProvider.TryGetCameraBounds2D(TotalMargin, out bounds))
                return true;

            bounds = default;
            return false;
        }

        private Vector2 ApplyTopDownBounds(Vector2 newPos, MovementBounds2D bounds)
        {
            Vector2 centrePos = newPos + spriteRadiusOffset;
            Vector2 boundsCenter = bounds.Center;

            if (screenWrap || bounds.AllowScreenWrap)
            {
                if (centrePos.x > boundsCenter.x + bounds.HalfWidth) centrePos.x = boundsCenter.x - bounds.HalfWidth;
                if (centrePos.x < boundsCenter.x - bounds.HalfWidth) centrePos.x = boundsCenter.x + bounds.HalfWidth;
                if (centrePos.y > boundsCenter.y + bounds.HalfHeight) centrePos.y = boundsCenter.y - bounds.HalfHeight;
                if (centrePos.y < boundsCenter.y - bounds.HalfHeight) centrePos.y = boundsCenter.y + bounds.HalfHeight;
            }
            else
            {
                centrePos.x = Mathf.Clamp(centrePos.x, boundsCenter.x - bounds.HalfWidth, boundsCenter.x + bounds.HalfWidth);
                centrePos.y = Mathf.Clamp(centrePos.y, boundsCenter.y - bounds.HalfHeight, boundsCenter.y + bounds.HalfHeight);
            }

            return centrePos - spriteRadiusOffset;
        }

        private Vector2 ApplyInputDeadZones(Vector2 newPos)
        {
            if (inputZones == null || !inputZones.IsInAnyDeadZone(newPos))
                return newPos;

            Vector2 currentPos = rb2d.position;
            Vector2 slideX = new Vector2(newPos.x, currentPos.y);
            if (!inputZones.IsInAnyDeadZone(slideX))
            {
                newPos = slideX;
            }
            else
            {
                Vector2 slideY = new Vector2(currentPos.x, newPos.y);
                newPos = !inputZones.IsInAnyDeadZone(slideY) ? slideY : currentPos;
            }

            if (model.State.IsDashing)
                model.CancelDash();

            return newPos;
        }

        private void ClampPositionToBounds()
        {
            if (rb2d == null || !TryGetMovementBounds(out MovementBounds2D bounds))
                return;

            Vector2 boundsCenter = bounds.Center;
            Vector2 pivotPos = rb2d.position;
            Vector2 centrePos = pivotPos + spriteRadiusOffset;
            Vector2 clampedCentre;
            clampedCentre.x = Mathf.Clamp(centrePos.x, boundsCenter.x - bounds.HalfWidth, boundsCenter.x + bounds.HalfWidth);
            clampedCentre.y = Mathf.Clamp(centrePos.y, boundsCenter.y - bounds.HalfHeight, boundsCenter.y + bounds.HalfHeight);
            rb2d.MovePosition(clampedCentre - spriteRadiusOffset);
        }

        private void ClampSideViewPositionToHorizontalBounds(MovementBounds2D bounds)
        {
            Vector2 boundsCenter = bounds.Center;
            Vector2 pivotPos = rb2d.position;
            Vector2 centrePos = pivotPos + spriteRadiusOffset;
            float clampedX = Mathf.Clamp(centrePos.x, boundsCenter.x - bounds.HalfWidth, boundsCenter.x + bounds.HalfWidth);
            if (Mathf.Approximately(clampedX, centrePos.x))
                return;

            rb2d.position = new Vector2(clampedX - spriteRadiusOffset.x, pivotPos.y);
            Vector2 velocity = rb2d.linearVelocity;
            velocity.x = 0f;
            rb2d.linearVelocity = velocity;
        }
    }
}
