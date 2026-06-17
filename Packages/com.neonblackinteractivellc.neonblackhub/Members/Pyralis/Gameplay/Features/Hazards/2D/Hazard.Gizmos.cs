using UnityEngine;

namespace NeonBlack.Gameplay.Features.Hazards
{
    public partial class Hazard
    {
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (CrossingStart != CrossingEnd)
            {
                UnityEditor.Handles.color = new Color(1f, 0.85f, 0f, 0.9f);
                UnityEditor.Handles.DrawLine(CrossingStart, CrossingEnd);
                UnityEditor.Handles.DrawSolidDisc(CrossingStart, Vector3.forward, 0.12f);
                UnityEditor.Handles.DrawSolidDisc(CrossingEnd, Vector3.forward, 0.12f);
            }

            if (_data == null)
                return;

            if (_data.enableExplosion && _data.explosionTrigger == HazardData.ExplosionTrigger.OnProximity)
            {
                Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.15f);
                Gizmos.DrawSphere(transform.position, _data.explosionProximityRadius);
                Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.85f);
                Gizmos.DrawWireSphere(transform.position, _data.explosionProximityRadius);
            }

            if (_data.enableTargeting && _data.lockOnRadius > 0f)
            {
                Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.25f);
                Gizmos.DrawWireSphere(transform.position, _data.lockOnRadius);
            }

            if (_data.destroysNearbyCollectibles)
            {
                Vector2 size = GetPrimaryHitColliderSize();
                float radius = Mathf.Max(size.x, size.y) * 0.5f * _data.collectibleDestroyRadiusScale;
                Gizmos.color = new Color(0.6f, 0.4f, 0f, 0.2f);
                Gizmos.DrawWireSphere(transform.position, radius);
            }
        }
#endif
    }
}
