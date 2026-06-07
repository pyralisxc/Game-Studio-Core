using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [System.Serializable]
    public class RuntimeCapabilitySelection
    {
        public RuntimeCapabilityFamily capabilityFamily = RuntimeCapabilityFamily.CharacterPawnGameplay;
        public RuntimePatternDefinition patternDefinition;
        public bool requiredForFirstProof = true;

        [TextArea(1, 4)]
        public string creatorNotes = string.Empty;

        public void Sanitize()
        {
            creatorNotes ??= string.Empty;

            if (patternDefinition != null)
                capabilityFamily = patternDefinition.capabilityFamily;
        }
    }

    /// <summary>
     /// Describes one composed game setup by selecting compatible runtime pattern recipes.
     /// </summary>
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Game Setup Profile", fileName = "GameSetupProfile", order = -100)]
    public class GameSetupProfile : ScriptableObject
    {
        public string setupName = "Game Setup";

        [TextArea(2, 5)]
        public string summary = string.Empty;

        [Tooltip("Creator-facing runtime choices. Add rows from the inspector dropdown and assign a matching RuntimePatternDefinition when the setup needs detailed recipe guidance.")]
        public RuntimeCapabilitySelection[] runtimeCapabilities = System.Array.Empty<RuntimeCapabilitySelection>();

        [Tooltip("Resolved runtime recipe assets used by validators and route analysis. Prefer editing Runtime Capabilities above.")]
        public RuntimePatternDefinition[] runtimePatterns = System.Array.Empty<RuntimePatternDefinition>();

        [TextArea(2, 6)]
        public string setupNotes = string.Empty;

        public void Sanitize()
        {
            setupName = setupName != null ? setupName.Trim() : string.Empty;
            summary ??= string.Empty;
            setupNotes ??= string.Empty;
            runtimeCapabilities ??= System.Array.Empty<RuntimeCapabilitySelection>();
            for (int i = 0; i < runtimeCapabilities.Length; i++)
                runtimeCapabilities[i]?.Sanitize();

            SyncRuntimePatternsFromCapabilities();
            runtimePatterns ??= System.Array.Empty<RuntimePatternDefinition>();
        }

        public bool HasPattern(string patternId)
        {
            if (string.IsNullOrWhiteSpace(patternId) || runtimePatterns == null)
                return false;

            for (int i = 0; i < runtimePatterns.Length; i++)
            {
                RuntimePatternDefinition pattern = runtimePatterns[i];
                if (pattern == null || string.IsNullOrWhiteSpace(pattern.patternId))
                    continue;

                if (string.Equals(pattern.patternId, patternId, System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public List<string> GetValidationIssues()
        {
            List<string> issues = new List<string>();

            if (string.IsNullOrWhiteSpace(setupName))
                issues.Add("Setup name is required.");

            if ((runtimeCapabilities == null || runtimeCapabilities.Length == 0)
                && (runtimePatterns == null || runtimePatterns.Length == 0))
            {
                issues.Add("Choose a Runtime Capability family, then click Add Capability to create the first setup row.");
                return issues;
            }

            AppendRuntimeCapabilityIssues(issues);

            if (runtimePatterns == null || runtimePatterns.Length == 0)
            {
                issues.Add("Select a RuntimePatternDefinition for at least one runtime capability so Authoring can explain the setup recipe.");
                return issues;
            }

            HashSet<string> patternIds = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < runtimePatterns.Length; i++)
            {
                RuntimePatternDefinition pattern = runtimePatterns[i];
                if (pattern == null)
                {
                    issues.Add($"Runtime Patterns[{i}] is null.");
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(pattern.patternId) && !patternIds.Add(pattern.patternId))
                    issues.Add($"Runtime pattern `{pattern.patternId}` is assigned more than once.");

                List<string> patternIssues = pattern.GetValidationIssues();
                for (int issueIndex = 0; issueIndex < patternIssues.Count; issueIndex++)
                    issues.Add($"Runtime pattern `{GetPatternLabel(pattern)}`: {patternIssues[issueIndex]}");
            }

            AppendCompanionIssues(issues);
            return issues;
        }

        private void AppendRuntimeCapabilityIssues(List<string> issues)
        {
            if (!HasRuntimeCapabilityPatterns())
                return;

            HashSet<RuntimeCapabilityFamily> families = new HashSet<RuntimeCapabilityFamily>();
            for (int i = 0; i < runtimeCapabilities.Length; i++)
            {
                RuntimeCapabilitySelection capability = runtimeCapabilities[i];
                if (capability == null)
                {
                    issues.Add($"Runtime Capabilities[{i}] is empty.");
                    continue;
                }

                if (!families.Add(capability.capabilityFamily))
                    issues.Add($"Runtime capability `{capability.capabilityFamily}` is selected more than once.");

                if (capability.patternDefinition == null)
                    issues.Add($"Runtime capability `{capability.capabilityFamily}` needs a RuntimePatternDefinition recipe.");
                else if (capability.patternDefinition.capabilityFamily != capability.capabilityFamily)
                    issues.Add($"Runtime capability `{capability.capabilityFamily}` references pattern `{GetPatternLabel(capability.patternDefinition)}` with family `{capability.patternDefinition.capabilityFamily}`.");
            }
        }

        private void SyncRuntimePatternsFromCapabilities()
        {
            if (runtimeCapabilities == null || runtimeCapabilities.Length == 0)
                return;

            List<RuntimePatternDefinition> patterns = new List<RuntimePatternDefinition>();
            HashSet<string> patternIds = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < runtimeCapabilities.Length; i++)
            {
                RuntimePatternDefinition pattern = runtimeCapabilities[i]?.patternDefinition;
                if (pattern == null)
                    continue;

                string id = !string.IsNullOrWhiteSpace(pattern.patternId)
                    ? pattern.patternId
                    : pattern.name;
                if (patternIds.Add(id))
                    patterns.Add(pattern);
            }

            runtimePatterns = patterns.ToArray();
        }

        private bool HasRuntimeCapabilityPatterns()
        {
            if (runtimeCapabilities == null)
                return false;

            for (int i = 0; i < runtimeCapabilities.Length; i++)
            {
                if (runtimeCapabilities[i]?.patternDefinition != null)
                    return true;
            }

            return false;
        }

        private void AppendCompanionIssues(List<string> issues)
        {
            if (runtimePatterns == null)
                return;

            for (int i = 0; i < runtimePatterns.Length; i++)
            {
                RuntimePatternDefinition first = runtimePatterns[i];
                if (first == null)
                    continue;

                for (int j = i + 1; j < runtimePatterns.Length; j++)
                {
                    RuntimePatternDefinition second = runtimePatterns[j];
                    if (second == null)
                        continue;

                    if (first.ConflictsWith(second))
                        issues.Add($"Runtime pattern `{GetPatternLabel(first)}` cautions against `{GetPatternLabel(second)}`.");

                    if (second.ConflictsWith(first))
                        issues.Add($"Runtime pattern `{GetPatternLabel(second)}` cautions against `{GetPatternLabel(first)}`.");
                }
            }
        }

        private static string GetPatternLabel(RuntimePatternDefinition pattern)
        {
            if (pattern == null)
                return "<null>";

            if (!string.IsNullOrWhiteSpace(pattern.displayName))
                return pattern.displayName;

            if (!string.IsNullOrWhiteSpace(pattern.patternId))
                return pattern.patternId;

            return pattern.name;
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
