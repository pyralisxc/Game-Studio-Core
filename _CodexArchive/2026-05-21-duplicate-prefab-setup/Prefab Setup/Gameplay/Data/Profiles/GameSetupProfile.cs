using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Describes one composed game setup by selecting compatible runtime pattern recipes.
    /// </summary>
    [CreateAssetMenu(menuName = "NeonBlack/Gameplay/Profiles/Game Setup Profile", fileName = "GameSetupProfile")]
    public class GameSetupProfile : ScriptableObject
    {
        public string setupName = "Game Setup";

        [TextArea(2, 5)]
        public string summary = string.Empty;

        public RuntimePatternDefinition[] runtimePatterns = System.Array.Empty<RuntimePatternDefinition>();

        [TextArea(2, 6)]
        public string setupNotes = string.Empty;

        public void Sanitize()
        {
            setupName = setupName != null ? setupName.Trim() : string.Empty;
            summary ??= string.Empty;
            setupNotes ??= string.Empty;
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

            if (runtimePatterns == null || runtimePatterns.Length == 0)
            {
                issues.Add("At least one runtime pattern should be assigned.");
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
