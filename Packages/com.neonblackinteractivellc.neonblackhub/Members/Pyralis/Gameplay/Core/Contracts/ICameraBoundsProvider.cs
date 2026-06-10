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

    [AuthoringContract(Capability = AuthoringCapability.Camera, Relevance = "Provides world-space boundaries for camera framing and containment.", Axioms = AuthoringWorldAxiom.BoundedSpace)]
public interface ICameraBoundsProvider
{
        bool TryGetCameraBounds2D(float margin, out CameraBounds2D bounds);
    }
}
