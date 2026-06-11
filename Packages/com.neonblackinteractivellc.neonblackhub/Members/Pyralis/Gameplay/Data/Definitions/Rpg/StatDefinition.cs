using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions.Rpg
{
    [AuthoringContract(
        Capability = AuthoringCapability.Stats,
        Relevance = "Defines a reusable RPG stat (e.g., Strength, Wisdom, Health).",
        NativeSetup = new[] { "Create Asset.", "Set Stat Id and Display Name.", "Choose Category." },
        AssignmentFields = new[] { nameof(statId), nameof(displayName), nameof(category) },
        FirstProof = "Verify the stat is correctly displayed in character profiles and modified by equipment.",
        ExpertAdvice = "Use categories to group related stats (e.g., 'Primary', 'Combat', 'Social') in UI and tools."
    )]
    [CreateAssetMenu(menuName = "NeonBlack/RPG/Stat Definition", fileName = "StatDefinition")]
    public class StatDefinition : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            return GetValidationIssues();
        }

        public string statId = "stat.new";
        public string displayName = "New Stat";
        public string category = "General";
        public float defaultValue;

        [TextArea(2, 5)]
        public string notes = string.Empty;

        public void Sanitize()
        {
            statId = !string.IsNullOrWhiteSpace(statId) ? statId.Trim() : statId;
            displayName = !string.IsNullOrWhiteSpace(displayName) ? displayName.Trim() : statId;
            category = !string.IsNullOrWhiteSpace(category) ? category.Trim() : "General";
        }

        public List<string> GetValidationIssues()
        {
            List<string> issues = new List<string>();

            if (string.IsNullOrWhiteSpace(statId))
                issues.Add("Stat stable id is required.");

            if (string.IsNullOrWhiteSpace(displayName))
                issues.Add("Display name is required.");

            if (string.IsNullOrWhiteSpace(category))
                issues.Add("Category is required so RPG tools can group stats.");

            return issues;
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
