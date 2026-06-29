using UnityEngine;

namespace NeonBlack.Gameplay.Core.Contracts
{
    public readonly struct CameraBounds2D
    {
        public CameraBounds2D(Camera camera, Vector3 center, float halfWidth, float halfHeight)
        {
            Camera = camera;
            Center = center;
            HalfWidth = Mathf.Max(0f, halfWidth);
            HalfHeight = Mathf.Max(0f, halfHeight);
        }

        public Camera Camera { get; }
        public Vector3 Center { get; }
        public float HalfWidth { get; }
        public float HalfHeight { get; }
        public bool IsValid => Camera != null && HalfWidth > 0f && HalfHeight > 0f;
    }

    public readonly struct PlayfieldBounds2D
    {
        public PlayfieldBounds2D(Vector2 min, Vector2 max, bool allowScreenWrap)
        {
            Min = min;
            Max = max;
            AllowScreenWrap = allowScreenWrap;
        }

        public Vector2 Min { get; }
        public Vector2 Max { get; }
        public bool AllowScreenWrap { get; }
        public Vector2 Center => (Min + Max) * 0.5f;
        public float HalfWidth => Mathf.Max(0f, (Max.x - Min.x) * 0.5f);
        public float HalfHeight => Mathf.Max(0f, (Max.y - Min.y) * 0.5f);
        public bool IsValid => Max.x > Min.x && Max.y > Min.y;
    }

    public interface ICameraBoundsProvider
    {
        bool TryGetCameraBounds2D(float margin, out CameraBounds2D bounds);
    }

    public interface IPlayfieldBoundsProvider
    {
        bool TryGetPlayfieldBounds2D(float margin, out PlayfieldBounds2D bounds);
    }
}
