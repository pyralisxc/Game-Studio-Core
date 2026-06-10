using NeonBlack.Gameplay.Core.Enums;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Defines gameplay-space rules independent from camera framing.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Camera | AuthoringCapability.Setup, 
        Relevance = "Project-window creation path for movement space, bounds, wrap, and arena-depth rules.",
        AssignmentFields = new[] { nameof(movementMode), nameof(minBounds), nameof(maxBounds) },
        FirstProof = "Verify that actors are clamped to the defined bounds in-game.",
        NativeSetup = new[] { "Create Asset" }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Playfield Profile", fileName = "PlayfieldProfile", order = -80)]
    public class PlayfieldProfile : ScriptableObject
    {
        public MovementMode movementMode = MovementMode.ThreeD;
        [Header("Bounds")]
        public bool clampToBounds = false;
        public Vector2 minBounds = new Vector2(-8f, -4f);
        public Vector2 maxBounds = new Vector2(8f, 4f);
        public bool allowScreenWrap = false;

        [Header("Depth / Arena")]
        public bool useDepthAxis = true;
        public float minDepth = -3f;
        public float maxDepth = 3f;
        public bool lockArenaUntilWaveClear = false;

        public void Sanitize()
        {
            if (minBounds.x > maxBounds.x)
            {
                float swap = minBounds.x;
                minBounds.x = maxBounds.x;
                maxBounds.x = swap;
            }
            if (minBounds.y > maxBounds.y)
            {
                float swap = minBounds.y;
                minBounds.y = maxBounds.y;
                maxBounds.y = swap;
            }
            if (minDepth > maxDepth)
            {
                float swap = minDepth;
                minDepth = maxDepth;
                maxDepth = swap;
            }
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
