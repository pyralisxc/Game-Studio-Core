using System.Collections.Generic;

using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Modules.Hazards
{
    public partial class Hazard
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (_data == null)
                yield return RuntimeValidationIssue.Required("Hazard Data is unassigned.");
            if (_shadowRenderer == null)
                yield return RuntimeValidationIssue.Required("Shadow Renderer is unassigned.");
            if (_hitColliders == null || _hitColliders.Count == 0)
                yield return RuntimeValidationIssue.Required("Hit Colliders list is empty.");

            if (_outlineRenderer != null
                && _shadowRenderer != null
                && _outlineRenderer.gameObject == _shadowRenderer.gameObject)
            {
                yield return RuntimeValidationIssue.Required("Outline and Shadow renderers are on the same GameObject.");
            }

            if (_data != null && _data.enableExplosion)
            {
                if (_explosionEffect == null)
                    yield return RuntimeValidationIssue.Required("Explosive hazard needs an Explosion Effect child.");
                if (!Runtime.HasRootRigidbody2D)
                    yield return RuntimeValidationIssue.Required("Explosive hazard needs a Kinematic Rigidbody2D on root.");
            }

            if (_data != null && HasAudioFeedback(_data) && GetComponent<UnityEngine.AudioSource>() == null)
                yield return RuntimeValidationIssue.Required("HazardData assigns audio clips, but the hazard prefab root has no AudioSource.");

            if (_data != null && _data.hazardType == HazardData.HazardType.Crossing && _laneRenderer == null)
                yield return RuntimeValidationIssue.Required("Crossing hazard needs a Lane Renderer.");
        }

        private static bool HasAudioFeedback(HazardData data)
        {
            return data.slamImpactClip != null
                || data.bounceClip != null
                || data.crossingEntryClip != null
                || data.crossingTravelClip != null
                || data.crossingExitClip != null
                || data.explosionClip != null;
        }
    }
}
