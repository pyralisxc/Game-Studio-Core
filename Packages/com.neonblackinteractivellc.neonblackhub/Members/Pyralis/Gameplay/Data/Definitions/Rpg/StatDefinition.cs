using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Definitions.Rpg
{
    [AuthoringContract(
        Category = "Stats",
        CapabilityPath = "RPG/Stats/Definitions/Stat Definition",
        Surface = AuthoringSurface.Goal,
        Summary = "Defines a reusable RPG stat (e.g., Strength, Wisdom, Health).",
        RequiredFields = new[] { nameof(statId), nameof(displayName), nameof(category) },
        SetupSteps = new[] { "Set Stat Id and Display Name.", "Choose Category." },
        SuccessChecks = new[] { "Verify the stat is correctly displayed in character profiles and modified by equipment." },
        Tags = new[] { "capability:Stats", "runtime:CharacterPawnGameplay" }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/RPG/Stat Definition", fileName = "StatDefinition")]
    public class StatDefinition : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            return PyralisRuntimeValidationIssueUtility.FromLocalValidationMessages(GetValidationIssues(), this);
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
