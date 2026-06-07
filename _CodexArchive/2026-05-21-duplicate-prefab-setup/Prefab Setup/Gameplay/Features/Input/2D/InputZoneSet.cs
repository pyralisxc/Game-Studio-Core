using UnityEngine;

namespace NeonBlack.Gameplay.Features.Input
{
/// <summary>
/// Defines movement dead zone polygons in world space (XY plane).
/// The active 2D motor is hard-blocked from walking or dashing into any polygon in this set.
/// Stores separate configs for Portrait and Landscape orientation.
///
/// Setup:
///   1. Right-click in Project â†’ Create â†’ NeonBlack â†’ Gameplay â†’ Input â†’ Input Zone Set.
///   2. Wire this asset into the current 2D motor controller's "Input Zones" field.
///   3. Select the asset in the Project window to see and edit zone polygons
///      directly in the Scene view with draggable handles.
/// </summary>
[CreateAssetMenu(fileName = "InputZoneSet", menuName = "NeonBlack/Gameplay/Input/Input Zone Set")]
public class InputZoneSet : ScriptableObject
{
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // Inner Types
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>
    /// A polygon in world space (XY plane).
    /// Points wind in any order â€” the crossing-number test handles both CW and CCW.
    /// Works for any convex OR concave simple polygon.
    /// Edit by selecting this asset in the Project window and dragging handles in the Scene view.
    /// </summary>
    [System.Serializable]
    public class ScreenPolygon
    {
        [Tooltip("Polygon vertices in world space (XY).\n" +
                 "Select this InputZoneSet asset in the Project window to see and drag the handles.")]
        public Vector2[] points = new Vector2[0];

        private Rect _bounds;
        private bool _boundsValid;

        /// <summary>Returns true if <paramref name="p"/> is inside the polygon (crossing-number / even-odd test).</summary>
        public bool ContainsPoint(Vector2 p)
        {
            if (points == null || points.Length < 3) return false;

            // AABB pre-check: O(1) fast-reject for points outside the bounding box.
            if (!_boundsValid) ComputeBounds();
            if (!_bounds.Contains(p)) return false;

            int crossings = 0;
            int n = points.Length;
            for (int i = 0; i < n; i++)
            {
                Vector2 a = points[i];
                Vector2 b = points[(i + 1) % n];
                if ((a.y <= p.y && b.y > p.y) || (b.y <= p.y && a.y > p.y))
                {
                    float t = (p.y - a.y) / (b.y - a.y);
                    if (p.x < a.x + t * (b.x - a.x))
                        crossings++;
                }
            }
            return (crossings & 1) == 1;
        }

        /// <summary>Call after editing points to force bounds recomputation on next use.</summary>
        public void InvalidateBounds() => _boundsValid = false;

        private void ComputeBounds()
        {
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;
            foreach (Vector2 pt in points)
            {
                if (pt.x < minX) minX = pt.x;
                if (pt.x > maxX) maxX = pt.x;
                if (pt.y < minY) minY = pt.y;
                if (pt.y > maxY) maxY = pt.y;
            }
            _bounds      = new Rect(minX, minY, maxX - minX, maxY - minY);
            _boundsValid = true;
        }
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // Inspector Fields
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Header("Portrait â€” Movement Dead Zones")]
    [Tooltip("World-space polygons. The active 2D motor is hard-blocked from walking or dashing into any of these areas.\n" +
             "Select this asset in the Project window to drag and reshape them in the Scene view.")]
    public ScreenPolygon[] portrait = new ScreenPolygon[0];

    [Header("Landscape â€” Movement Dead Zones")]
    [Tooltip("Same as Portrait but applied when the device is in landscape orientation.")]
    public ScreenPolygon[] landscape = new ScreenPolygon[0];

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // Public API
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>Returns the active dead zone array for the current orientation.</summary>
    public ScreenPolygon[] ActiveZones => Screen.height >= Screen.width ? portrait : landscape;

    /// <summary>True if <paramref name="worldPos"/> is inside any dead zone for the current orientation.</summary>
    public bool IsInAnyDeadZone(Vector2 worldPos)
    {
        foreach (ScreenPolygon zone in ActiveZones)
            if (zone.ContainsPoint(worldPos)) return true;
        return false;
    }

    private void OnValidate()
    {
        // Invalidate cached AABB whenever polygon points are edited in the Inspector.
        if (portrait  != null) foreach (var z in portrait)  z?.InvalidateBounds();
        if (landscape != null) foreach (var z in landscape) z?.InvalidateBounds();
    }
}
}
