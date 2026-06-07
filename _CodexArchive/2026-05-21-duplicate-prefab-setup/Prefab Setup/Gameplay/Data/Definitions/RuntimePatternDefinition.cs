using System.Collections.Generic;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions
{
    /// <summary>
    /// Authoring asset for one reusable runtime setup recipe.
    /// Patterns are composable; they describe capability expectations rather than exclusive genres.
    /// </summary>
    [CreateAssetMenu(menuName = "NeonBlack/Gameplay/Definitions/Runtime Pattern Definition", fileName = "RuntimePatternDefinition")]
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

            if (supportedControlSurfaces == null || supportedControlSurfaces.Length == 0)
                issues.Add("At least one supported control surface is required.");

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
