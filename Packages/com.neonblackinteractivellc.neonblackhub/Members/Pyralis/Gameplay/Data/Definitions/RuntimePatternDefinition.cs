using System.Collections.Generic;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions
{
    public enum RuntimePatternPresentationLane
    {
        Any,
        Sprite2D,
        Billboard2_5D,
        Rigged3D,
        TabletopNoPawn,
        UiMenu,
        CameraCursor,
        Networked
    }

    [System.Flags]
    public enum RuntimePatternFirstProofRequirement
    {
        None = 0,
        SpawnPoints = 1 << 0,
        CameraRig = 1 << 1,
        CameraBounds2D = 1 << 2,
        PlayerInputManager = 1 << 3,
        GameplayStateService = 1 << 4,
        CameraBoundsService = 1 << 5,
        ScoreService = 1 << 6,
        HudOrMenuSurface = 1 << 7,
        ProjectileOrHitboxSource = 1 << 8,
        EnemyOrNpcSpawner = 1 << 9,
        TabletopRuntimeContract = 1 << 10,
        SelectionSurface = 1 << 11
    }

    /// <summary>
    /// Authoring asset for one reusable runtime setup recipe.
    /// Patterns are composable; they describe capability expectations rather than exclusive genres.
    /// </summary>
    [CreateAssetMenu(menuName = "NeonBlack/Definitions/Runtime Pattern Definition", fileName = "RuntimePatternDefinition", order = 40)]
    public class RuntimePatternDefinition : ScriptableObject
    {
        public string patternId = "pattern.runtime";
        public string displayName = "Runtime Pattern";

        [TextArea(2, 5)]
        public string description = string.Empty;

        public RuntimeCapabilityFamily capabilityFamily = RuntimeCapabilityFamily.PlatformCore;
        public ParticipantEmbodimentRequirement participantEmbodiment = ParticipantEmbodimentRequirement.NoneRequired;
        public RuntimeControlSurface[] supportedControlSurfaces = { RuntimeControlSurface.Pawn };
        public string[] requiredRuntimeSystems = System.Array.Empty<string>();
        public string[] optionalRuntimeSystems = System.Array.Empty<string>();
        public RuntimePatternPresentationLane[] presentationLanes = { RuntimePatternPresentationLane.Any };
        public RuntimePatternFirstProofRequirement firstProofRequirements = RuntimePatternFirstProofRequirement.None;
        public RuntimePatternDefinition[] recommendedCompanionPatterns = System.Array.Empty<RuntimePatternDefinition>();
        public RuntimePatternDefinition[] cautionaryCompanionPatterns = System.Array.Empty<RuntimePatternDefinition>();

        [TextArea(2, 6)]
        public string setupNotes = string.Empty;

        public void Sanitize()
        {
            patternId = patternId != null ? patternId.Trim() : string.Empty;
            displayName = displayName != null ? displayName.Trim() : string.Empty;
            description ??= string.Empty;
            setupNotes ??= string.Empty;
            supportedControlSurfaces ??= System.Array.Empty<RuntimeControlSurface>();
            requiredRuntimeSystems ??= System.Array.Empty<string>();
            optionalRuntimeSystems ??= System.Array.Empty<string>();
            presentationLanes ??= new[] { RuntimePatternPresentationLane.Any };
            if (presentationLanes.Length == 0)
                presentationLanes = new[] { RuntimePatternPresentationLane.Any };
            recommendedCompanionPatterns ??= System.Array.Empty<RuntimePatternDefinition>();
            cautionaryCompanionPatterns ??= System.Array.Empty<RuntimePatternDefinition>();
        }

        public bool SupportsControlSurface(RuntimeControlSurface surface)
        {
            if (supportedControlSurfaces == null)
                return false;

            for (int i = 0; i < supportedControlSurfaces.Length; i++)
            {
                if (supportedControlSurfaces[i] == surface)
                    return true;
            }

            return false;
        }

        public bool SupportsPresentationLane(RuntimePatternPresentationLane lane)
        {
            if (lane == RuntimePatternPresentationLane.Any)
                return true;

            if (presentationLanes == null)
                return false;

            for (int i = 0; i < presentationLanes.Length; i++)
            {
                RuntimePatternPresentationLane candidate = presentationLanes[i];
                if (candidate == RuntimePatternPresentationLane.Any || candidate == lane)
                    return true;
            }

            return false;
        }

        public bool RequiresFirstProof(RuntimePatternFirstProofRequirement requirement)
        {
            return requirement != RuntimePatternFirstProofRequirement.None
                && (firstProofRequirements & requirement) == requirement;
        }

        public bool Recommends(RuntimePatternDefinition pattern)
        {
            return ContainsPattern(recommendedCompanionPatterns, pattern);
        }

        public bool ConflictsWith(RuntimePatternDefinition pattern)
        {
            return ContainsPattern(cautionaryCompanionPatterns, pattern);
        }

        public List<string> GetValidationIssues()
        {
            List<string> issues = new List<string>();

            if (string.IsNullOrWhiteSpace(patternId))
                issues.Add("Pattern id is required.");

            if (string.IsNullOrWhiteSpace(displayName))
                issues.Add("Display name is required.");

            if (IsDefaultPlaceholder())
                issues.Add("Runtime pattern still uses default placeholder values. Choose a real capability family, embodiment, control surfaces, description, and setup notes before adding it to a setup profile.");

            if (string.IsNullOrWhiteSpace(description))
                issues.Add("Description is required. It appears in setup inspectors as beginner guidance.");

            if (string.IsNullOrWhiteSpace(setupNotes))
                issues.Add("Setup notes are required. Write the concrete Unity steps a beginner should follow for this pattern.");

            if (supportedControlSurfaces == null || supportedControlSurfaces.Length == 0)
                issues.Add("At least one supported control surface is required.");

            if (presentationLanes == null || presentationLanes.Length == 0)
                issues.Add("At least one presentation/runtime lane is required. Use Any only when the pattern is genuinely lane-agnostic.");

            if (participantEmbodiment == ParticipantEmbodimentRequirement.RequiredPawn
                && !SupportsControlSurface(RuntimeControlSurface.Pawn))
            {
                issues.Add("A pattern that requires a pawn must include the Pawn control surface.");
            }

            if (participantEmbodiment == ParticipantEmbodimentRequirement.NonPawnSurfaceRequired
                && !HasNonPawnControlSurface())
            {
                issues.Add("A pattern that requires a non-pawn participant surface must include at least one non-pawn control surface.");
            }

            return issues;
        }

        private bool IsDefaultPlaceholder()
        {
            return string.Equals(patternId, "pattern.runtime", System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(displayName, "Runtime Pattern", System.StringComparison.OrdinalIgnoreCase)
                || capabilityFamily == RuntimeCapabilityFamily.PlatformCore;
        }

        private bool HasNonPawnControlSurface()
        {
            if (supportedControlSurfaces == null)
                return false;

            for (int i = 0; i < supportedControlSurfaces.Length; i++)
            {
                if (supportedControlSurfaces[i] != RuntimeControlSurface.Pawn)
                    return true;
            }

            return false;
        }

        private static bool ContainsPattern(RuntimePatternDefinition[] patterns, RuntimePatternDefinition pattern)
        {
            if (patterns == null || pattern == null)
                return false;

            for (int i = 0; i < patterns.Length; i++)
            {
                RuntimePatternDefinition candidate = patterns[i];
                if (candidate == null)
                    continue;

                if (ReferenceEquals(candidate, pattern))
                    return true;

                if (!string.IsNullOrWhiteSpace(candidate.patternId)
                    && string.Equals(candidate.patternId, pattern.patternId, System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
