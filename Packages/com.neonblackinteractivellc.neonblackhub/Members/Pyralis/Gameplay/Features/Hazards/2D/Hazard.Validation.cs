using System.Collections.Generic;

namespace NeonBlack.Gameplay.Features.Hazards
{
    public partial class Hazard
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (_data == null)
                yield return "Hazard Data is unassigned.";
            if (_shadowRenderer == null)
                yield return "Shadow Renderer is unassigned.";
            if (_hitColliders == null || _hitColliders.Count == 0)
                yield return "Hit Colliders list is empty.";

            if (_outlineRenderer != null
                && _shadowRenderer != null
                && _outlineRenderer.gameObject == _shadowRenderer.gameObject)
            {
                yield return "Outline and Shadow renderers are on the same GameObject.";
            }

            if (_data != null && _data.enableExplosion)
            {
                if (_explosionEffect == null)
                    yield return "Explosive hazard needs an Explosion Effect child.";
                if (!Runtime.HasRootRigidbody2D)
                    yield return "Explosive hazard needs a Kinematic Rigidbody2D on root.";
            }

            if (_data != null && _data.hazardType == HazardData.HazardType.Crossing && _laneRenderer == null)
                yield return "Crossing hazard needs a Lane Renderer.";
        }
    }
}
