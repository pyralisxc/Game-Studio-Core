using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions.Rpg
{
    [AuthoringContract(
        ModuleId = "rpg.progression.curve",
        Capability = AuthoringCapability.Stats,
        Lane = "RPG",
        AssignmentFields = new[] { nameof(curveId), nameof(displayName), nameof(levelExperienceThresholds), nameof(skillPointGrants) },
        FirstProof = "Proof that the curve correctly resolves levels from experience points."
    )]
    [CreateAssetMenu(menuName = "NeonBlack/RPG/Progression Curve", fileName = "ProgressionCurveDefinition")]
    public class ProgressionCurveDefinition : ScriptableObject, IProgressionCurve, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            return GetValidationIssues();
        }

        public string curveId = "progression.default";
        public string displayName = "Default Progression";
        public int[] levelExperienceThresholds = { 0 };
        public int[] skillPointGrants = { 0 };

        public void Sanitize()
        {
            curveId = !string.IsNullOrWhiteSpace(curveId) ? curveId.Trim() : curveId;
            displayName = !string.IsNullOrWhiteSpace(displayName) ? displayName.Trim() : curveId;

            if (levelExperienceThresholds == null || levelExperienceThresholds.Length == 0)
                levelExperienceThresholds = new[] { 0 };

            if (skillPointGrants == null || skillPointGrants.Length == 0)
                skillPointGrants = new[] { 0 };

            for (int i = 0; i < levelExperienceThresholds.Length; i++)
                levelExperienceThresholds[i] = Mathf.Max(0, levelExperienceThresholds[i]);

            for (int i = 0; i < skillPointGrants.Length; i++)
                skillPointGrants[i] = Mathf.Max(0, skillPointGrants[i]);
        }

        public List<string> GetValidationIssues()
        {
            List<string> issues = new List<string>();

            if (string.IsNullOrWhiteSpace(curveId))
                issues.Add("Progression curve id is required.");

            if (string.IsNullOrWhiteSpace(displayName))
                issues.Add("Display name is required.");

            if (levelExperienceThresholds == null || levelExperienceThresholds.Length == 0)
            {
                issues.Add("At least one level threshold is required.");
                return issues;
            }

            if (levelExperienceThresholds[0] != 0)
                issues.Add("Level 1 threshold must be 0 XP.");

            for (int i = 0; i < levelExperienceThresholds.Length; i++)
            {
                if (levelExperienceThresholds[i] < 0)
                    issues.Add($"Level {i + 1} threshold cannot be negative.");

                if (i > 0 && levelExperienceThresholds[i] < levelExperienceThresholds[i - 1])
                    issues.Add($"Level {i + 1} threshold must be greater than or equal to the previous level threshold.");
            }

            if (skillPointGrants == null)
                return issues;

            for (int i = 0; i < skillPointGrants.Length; i++)
            {
                if (skillPointGrants[i] < 0)
                    issues.Add($"Skill point grant for level {i + 1} cannot be negative.");
            }

            return issues;
        }

        public int ResolveLevel(int experience)
        {
            int clampedExperience = Mathf.Max(0, experience);
            int level = 1;

            if (levelExperienceThresholds == null || levelExperienceThresholds.Length == 0)
                return level;

            for (int i = 0; i < levelExperienceThresholds.Length; i++)
            {
                if (clampedExperience >= Mathf.Max(0, levelExperienceThresholds[i]))
                    level = i + 1;
            }

            return level;
        }

        public int GetSkillPointGrantForLevel(int level)
        {
            if (level < 1 || skillPointGrants == null || skillPointGrants.Length < level)
                return 0;

            return Mathf.Max(0, skillPointGrants[level - 1]);
        }

        public void SetTestThresholds(int[] thresholds)
        {
            levelExperienceThresholds = thresholds;
        }

        public void SetTestSkillPointGrants(int[] grants)
        {
            skillPointGrants = grants;
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
