using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;

namespace NeonBlack.Gameplay.Editor
{
    /// <summary>
    /// Mirrors FeatureModuleDefinition validation for legacy editor callers without duplicating contract rules.
    /// </summary>
    public static class PyralisFeatureModuleContractValidator
    {
        public static List<string> GetValidationIssues(FeatureModuleDefinition definition)
        {
            List<string> issues = new List<string>();
            if (definition == null) return issues;

            foreach (PyralisRuntimeValidationIssue issue in definition.GetRuntimeValidationIssues())
            {
                if (issue != null && !string.IsNullOrWhiteSpace(issue.Message))
                    issues.Add(issue.Message);
            }

            return issues;
        }
    }
}
