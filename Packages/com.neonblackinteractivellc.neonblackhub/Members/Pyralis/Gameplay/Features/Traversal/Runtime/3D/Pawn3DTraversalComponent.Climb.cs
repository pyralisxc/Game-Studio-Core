using System.Collections;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Traversal
{
    public sealed partial class Pawn3DTraversalComponent
    {
        private void PerformClimb(IClimbZone zone)
        {
            if (!EnsureDependencies())
                return;

            if (!allowClimb || zone == null || _movement.State.ClimbTimer > 0f || _movement.State.IsActing)
                return;

            _movement.NotifyClimbStart(climbCooldown);
            _animationDriver?.TriggerSignal(
                zone.TraversalType == ClimbTraversalType.Side
                    ? ActorAnimationSignal.SideClimb
                    : ActorAnimationSignal.ForwardClimb);

            _activeClimb = StartCoroutine(ExecuteClimb(zone));
        }

        private IEnumerator ExecuteClimb(IClimbZone zone)
        {
            _activeClimbZone = zone;
            zone.DisableTemporarily();
            Vector3 startPos = transform.position;
            _controller.enabled = false;

            float elapsed = 0f;
            float duration = Mathf.Max(zone.ClimbDuration, 0.05f);
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                transform.position = zone.SamplePath(t, startPos);
                yield return null;
            }

            transform.position = zone.ClimbTargetPosition;
            CleanupClimb(zone, triggerAnimation: true);
        }

        private void CleanupClimb(IClimbZone zone, bool triggerAnimation)
        {
            if (_controller != null)
                _controller.enabled = true;

            zone?.EnableAfterClimb();
            _movement?.NotifyClimbEnd();
            if (triggerAnimation)
                _animationDriver?.TriggerSignal(ActorAnimationSignal.ClimbEnd);

            _activeClimb = null;
            _activeClimbZone = null;
        }
    }
}
