using UnityEngine;

namespace NeonBlack.Gameplay.Features.Input
{
    /// <summary>
    /// Defines movement dead zone polygons in world space on the XY plane.
    /// The active 2D motor can block walking or dashing into any polygon in this set.
    /// </summary>
    [CreateAssetMenu(fileName = "InputZoneSet", menuName = "NeonBlack/Input/Input Zone Set")]
    public class InputZoneSet : ScriptableObject
    {
        /// <summary>
        /// A polygon in world space on the XY plane.
        /// The crossing-number test supports clockwise or counter-clockwise winding.
        /// </summary>
        [System.Serializable]
        public class ScreenPolygon
        {
            [Tooltip("Polygon vertices in world space on the XY plane. Select this InputZoneSet asset to edit handles in the Scene view.")]
            public Vector2[] points = new Vector2[0];

            private Rect _bounds;
            private bool _boundsValid;

            public bool ContainsPoint(Vector2 p)
            {
                if (points == null || points.Length < 3)
                    return false;

                if (!_boundsValid)
                    ComputeBounds();
                if (!_bounds.Contains(p))
                    return false;

                int crossings = 0;
                int pointCount = points.Length;
                for (int i = 0; i < pointCount; i++)
                {
                    Vector2 a = points[i];
                    Vector2 b = points[(i + 1) % pointCount];
                    if ((a.y <= p.y && b.y > p.y) || (b.y <= p.y && a.y > p.y))
                    {
                        float t = (p.y - a.y) / (b.y - a.y);
                        if (p.x < a.x + t * (b.x - a.x))
                            crossings++;
                    }
                }

                return (crossings & 1) == 1;
            }

            public void InvalidateBounds()
            {
                _boundsValid = false;
            }

            private void ComputeBounds()
            {
                float minX = float.MaxValue;
                float maxX = float.MinValue;
                float minY = float.MaxValue;
                float maxY = float.MinValue;

                foreach (Vector2 point in points)
                {
                    if (point.x < minX)
                        minX = point.x;
                    if (point.x > maxX)
                        maxX = point.x;
                    if (point.y < minY)
                        minY = point.y;
                    if (point.y > maxY)
                        maxY = point.y;
                }

                _bounds = new Rect(minX, minY, maxX - minX, maxY - minY);
                _boundsValid = true;
            }
        }

        [Header("Portrait Movement Dead Zones")]
        [Tooltip("World-space polygons. Select this asset in the Project window to drag and reshape them in the Scene view.")]
        public ScreenPolygon[] portrait = new ScreenPolygon[0];

        [Header("Landscape Movement Dead Zones")]
        [Tooltip("Same as Portrait, but applied when the device is in landscape orientation.")]
        public ScreenPolygon[] landscape = new ScreenPolygon[0];

        public ScreenPolygon[] ActiveZones => Screen.height >= Screen.width ? portrait : landscape;

        public bool IsInAnyDeadZone(Vector2 worldPos)
        {
            foreach (ScreenPolygon zone in ActiveZones)
            {
                if (zone != null && zone.ContainsPoint(worldPos))
                    return true;
            }

            return false;
        }

        private void OnValidate()
        {
            InvalidateZones(portrait);
            InvalidateZones(landscape);
        }

        private static void InvalidateZones(ScreenPolygon[] zones)
        {
            if (zones == null)
                return;

            foreach (ScreenPolygon zone in zones)
                zone?.InvalidateBounds();
        }
    }
}
