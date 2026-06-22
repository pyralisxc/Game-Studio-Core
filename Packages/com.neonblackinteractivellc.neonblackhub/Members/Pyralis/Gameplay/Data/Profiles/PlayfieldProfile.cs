using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Enums;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Defines gameplay-space rules independent from camera framing.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Movement | AuthoringCapability.Setup,
        Relevance = "Project-window creation path for movement space, bounds, wrap, and arena-depth rules.",
        AssignmentFields = new[] { nameof(movementMode), nameof(minBounds), nameof(maxBounds) },
        FirstProof = "Verify that actors are clamped to the defined bounds in-game.",
        ExpertAdvice = "The Playfield defines the physical boundaries of the simulation. Use 'Clamp To Bounds' for arena-style games to keep participants within the playable area.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/core",
        CapabilityPath = "Movement/Profiles/Playfield Profile",
        RuntimeFamilies = new[] { RuntimeCapabilityFamily.CharacterPawnGameplay }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Playfield Profile", fileName = "PlayfieldProfile", order = -80)]
    public class PlayfieldProfile : ScriptableObject, IRuntimeValidationProvider, IPlayfieldBoundsProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (minBounds.x > maxBounds.x) yield return PyralisRuntimeValidationIssue.Required("Min X bound should not exceed Max X bound.");
            if (minBounds.y > maxBounds.y) yield return PyralisRuntimeValidationIssue.Required("Min Y bound should not exceed Max Y bound.");
            if (minDepth > maxDepth) yield return PyralisRuntimeValidationIssue.Required("Min Depth should not exceed Max Depth.");
        }

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

        public bool TryGetPlayfieldBounds2D(float margin, out PlayfieldBounds2D bounds)
        {
            if (!clampToBounds && !allowScreenWrap)
            {
                bounds = default;
                return false;
            }

            Sanitize();

            Vector2 min = minBounds + Vector2.one * Mathf.Max(0f, margin);
            Vector2 max = maxBounds - Vector2.one * Mathf.Max(0f, margin);
            if (min.x >= max.x)
            {
                float centerX = (minBounds.x + maxBounds.x) * 0.5f;
                min.x = centerX;
                max.x = centerX;
            }

            if (min.y >= max.y)
            {
                float centerY = (minBounds.y + maxBounds.y) * 0.5f;
                min.y = centerY;
                max.y = centerY;
            }

            bounds = new PlayfieldBounds2D(min, max, allowScreenWrap);
            return bounds.IsValid;
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
