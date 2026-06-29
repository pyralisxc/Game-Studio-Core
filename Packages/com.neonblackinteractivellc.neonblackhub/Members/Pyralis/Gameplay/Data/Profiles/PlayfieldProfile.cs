using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Enums;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Defines gameplay-space rules independent from camera framing.
    /// </summary>
    [AuthoringContract(
        StableId = "playfield.profile",
        Category = "Movement, Setup",
        CapabilityPath = "Movement/Profiles/Playfield Profile",
        Surface = AuthoringSurface.Profile,
        Summary = "Project-window creation path for movement space, bounds, wrap, and arena-depth rules.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/core",
        RequiredFields = new[] { nameof(movementMode), nameof(minBounds), nameof(maxBounds) },
        PrerequisiteStableIds = new[] { "mode.definition" },
        RouteStage = "Game Mode Asset",
        RouteOrder = 35,
        SetupDomain = "Playfield",
        ProofTarget = "PlayfieldProfile defines the active movement bounds for the mode.",
        NativeActionKind = AuthoringActionKind.CreateAsset,
        SuccessChecks = new[] { "Verify that actors are clamped to the defined bounds in-game." },
        Tags = new[] { "capability:Movement", "capability:Setup", "runtime:CharacterPawnGameplay" },
        Selectable = false
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Playfield Profile", fileName = "PlayfieldProfile", order = -80)]
    public class PlayfieldProfile : ScriptableObject, IRuntimeValidationProvider, IPlayfieldBoundsProvider
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (minBounds.x > maxBounds.x) yield return RuntimeValidationIssue.Required("Min X bound should not exceed Max X bound.");
            if (minBounds.y > maxBounds.y) yield return RuntimeValidationIssue.Required("Min Y bound should not exceed Max Y bound.");
            if (minDepth > maxDepth) yield return RuntimeValidationIssue.Required("Min Depth should not exceed Max Depth.");
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
