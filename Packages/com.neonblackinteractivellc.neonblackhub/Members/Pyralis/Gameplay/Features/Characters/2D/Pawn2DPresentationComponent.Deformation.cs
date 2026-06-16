using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    public sealed partial class Pawn2DPresentationComponent
    {
        private void TickDeformationLane()
        {
            if (movement.IsDead)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, baseScale, squashSnapSpeed * Time.deltaTime);
                return;
            }

            if (squashStretchEnabled)
                TickSquashStretchLane();

            if (tiltEnabled)
                TickTiltLane();
        }

        private void TickTiltLane()
        {
            Vector2 velocity = movement.CurrentVelocity;
            float targetAngle = 0f;
            float speed = velocity.magnitude;
            if (speed > PresentationVelocityThreshold)
            {
                float velAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
                float lean = -Mathf.Sin(velAngle * Mathf.Deg2Rad);
                float speedT = Mathf.Clamp01(speed / Mathf.Max(0.01f, movement.MoveSpeed));
                targetAngle = lean * maxTiltAngle * speedT;

                if (spriteRenderer != null && spriteRenderer.flipX)
                    targetAngle = -targetAngle;
            }

            currentTiltAngle = Mathf.MoveTowards(currentTiltAngle, targetAngle, tiltSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, 0f, currentTiltAngle);
        }

        private void TickSquashStretchLane()
        {
            Vector3 targetScale = baseScale;
            Vector2 velocity = movement.CurrentVelocity;
            float speed = velocity.magnitude;
            if (speed > DeformationVelocityThreshold)
            {
                float t = Mathf.Clamp01(speed / Mathf.Max(0.01f, movement.MoveSpeed));
                float stretch = Mathf.Lerp(1f, stretchAmount, t);
                bool horizontal = Mathf.Abs(velocity.x) >= Mathf.Abs(velocity.y);
                targetScale = horizontal
                    ? new Vector3(baseScale.x * stretch, baseScale.y / stretch, baseScale.z)
                    : new Vector3(baseScale.x / stretch, baseScale.y * stretch, baseScale.z);
            }

            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, squashSnapSpeed * Time.deltaTime);
        }
    }
}
