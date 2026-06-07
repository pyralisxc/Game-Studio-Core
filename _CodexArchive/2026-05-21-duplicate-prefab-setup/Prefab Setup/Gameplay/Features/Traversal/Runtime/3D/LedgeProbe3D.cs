using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Traversal
{
/// <summary>
/// Reusable ledge-probe settings and detection helper for 3D traversal surfaces.
/// Owns only overlap-box ledge detection; climb, hang, and traversal state stay in the caller.
/// </summary>
[System.Serializable]
public class LedgeProbe3D
{
    [Tooltip("Height above the player root where the ledge probe is cast (local Y). Should be around hand/chest height.")]
    [SerializeField] private float probeHeight = 1.4f;

    [Tooltip("Half-extents of the overlap box used to detect ClimbZone triggers at hand height.")]
    [SerializeField] private Vector3 probeHalfExtents = new Vector3(0.3f, 0.15f, 0.3f);

    [Tooltip("Layers the ledge probe checks against. Include whichever layer your ClimbZone GameObjects are on.")]
    [SerializeField] private LayerMask probeLayer = Physics.DefaultRaycastLayers;

    public Vector3 GetProbeCenter(Transform root)
    {
        return root.position + Vector3.up * probeHeight;
    }

    public IClimbZone FindClimbZone(Transform root, float verticalVelocity)
    {
        if (root == null)
            return null;

        Vector3 probeCenter = GetProbeCenter(root);
        Collider[] hits = Physics.OverlapBox(
            probeCenter,
            probeHalfExtents,
            Quaternion.identity,
            probeLayer,
            QueryTriggerInteraction.Collide);

        foreach (Collider col in hits)
        {
            ClimbZone zone = col.GetComponent<ClimbZone>();
            if (zone == null)
                continue;

            if (zone.RequireApproachFromBelow && probeCenter.y > col.transform.position.y)
                continue;

            if (verticalVelocity < zone.MaxFallSpeed)
                continue;

            return zone;
        }

        return null;
    }
}
}
