using System;
using System.Collections.Generic;
using System.Linq;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions.Rpg
{
    [AuthoringContract(
        ModuleId = "rpg.quest.definition",
        Capability = AuthoringCapability.Dialogue,
        Lane = "RPG",
        AssignmentFields = new[] { nameof(questId), nameof(displayName), nameof(objectives), nameof(rewards) },
        FirstProof = "Proof that the quest can be tracked and rewards are correctly defined."
    )]
    [CreateAssetMenu(menuName = "NeonBlack/RPG/Quest", fileName = "QuestDefinition")]
    public class QuestDefinition : ScriptableObject, IQuestDefinition, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            return GetValidationIssues();
        }

        public string questId = "quest.new";
        public string displayName = "New Quest";
        public bool repeatable;
        public QuestObjectiveDefinition[] objectives = Array.Empty<QuestObjectiveDefinition>();
        public QuestRewardDefinition[] rewards = Array.Empty<QuestRewardDefinition>();

        public string QuestId => string.IsNullOrWhiteSpace(questId) ? string.Empty : questId.Trim();
        public bool Repeatable => repeatable;
        public QuestObjectiveDefinition[] ObjectiveDefinitions => objectives ?? Array.Empty<QuestObjectiveDefinition>();
        public QuestRewardDefinition[] RewardDefinitions => rewards ?? Array.Empty<QuestRewardDefinition>();

        public QuestObjective[] Objectives => ObjectiveDefinitions
            .Where(objective => objective.IsValid)
            .Select(objective => objective.CreateRuntimeObjective())
            .ToArray();

        public QuestReward[] Rewards => RewardDefinitions
            .Select(reward => reward.CreateRuntimeReward())
            .Where(reward => !reward.IsEmpty)
            .ToArray();

        public void Sanitize()
        {
            questId = QuestId;
            displayName = !string.IsNullOrWhiteSpace(displayName) ? displayName.Trim() : questId;
            objectives = ObjectiveDefinitions;
            rewards = RewardDefinitions;

            for (int i = 0; i < objectives.Length; i++)
                objectives[i].Sanitize();

            for (int i = 0; i < rewards.Length; i++)
                rewards[i].Sanitize();
        }

        public bool TryGetObjectiveDefinition(string objectiveId, out QuestObjectiveDefinition objective)
        {
            string normalizedObjectiveId = Normalize(objectiveId);
            if (string.IsNullOrEmpty(normalizedObjectiveId))
            {
                objective = default;
                return false;
            }

            QuestObjectiveDefinition[] definitions = ObjectiveDefinitions;
            for (int i = 0; i < definitions.Length; i++)
            {
                if (definitions[i].ObjectiveId == normalizedObjectiveId)
                {
                    objective = definitions[i];
                    return true;
                }
            }

            objective = default;
            return false;
        }

        public bool TryGetObjective(string objectiveId, out QuestObjective objective)
        {
            if (!TryGetObjectiveDefinition(objectiveId, out QuestObjectiveDefinition definition))
            {
                objective = default;
                return false;
            }

            objective = definition.CreateRuntimeObjective();
            return true;
        }

        public List<string> GetValidationIssues()
        {
            List<string> issues = new List<string>();

            if (string.IsNullOrWhiteSpace(questId))
                issues.Add("Quest id is required.");

            if (string.IsNullOrWhiteSpace(displayName))
                issues.Add("Display name is required.");

            QuestObjectiveDefinition[] objectiveDefinitions = ObjectiveDefinitions;
            if (objectiveDefinitions.Length == 0)
                issues.Add("At least one objective is required.");

            HashSet<string> objectiveIds = new HashSet<string>();
            for (int i = 0; i < objectiveDefinitions.Length; i++)
            {
                QuestObjectiveDefinition objective = objectiveDefinitions[i];
                string objectiveId = objective.ObjectiveId;
                if (string.IsNullOrWhiteSpace(objectiveId))
                {
                    issues.Add($"Objectives[{i}] Objective id is required.");
                    continue;
                }

                if (!objectiveIds.Add(objectiveId))
                    issues.Add($"Quest objective `{objectiveId}` is assigned more than once.");

                if (string.IsNullOrWhiteSpace(objective.TargetId))
                    issues.Add($"Quest objective `{objectiveId}` Target id is required.");

                if (objective.requiredQuantity < 1)
                    issues.Add($"Quest objective `{objectiveId}` required quantity must be at least 1.");
            }

            QuestRewardDefinition[] rewardDefinitions = RewardDefinitions;
            if (rewardDefinitions.Length == 0 || rewardDefinitions.All(reward => reward.IsEmpty))
                issues.Add("At least one reward is required.");

            for (int rewardIndex = 0; rewardIndex < rewardDefinitions.Length; rewardIndex++)
            {
                QuestRewardDefinition reward = rewardDefinitions[rewardIndex];
                if (reward.experience < 0)
                    issues.Add($"Rewards[{rewardIndex}] experience cannot be negative.");

                if (reward.skillPoints < 0)
                    issues.Add($"Rewards[{rewardIndex}] skill points cannot be negative.");

                QuestItemRewardDefinition[] itemRewards = reward.ItemRewards;
                for (int itemIndex = 0; itemIndex < itemRewards.Length; itemIndex++)
                {
                    if (string.IsNullOrWhiteSpace(itemRewards[itemIndex].ItemId))
                        issues.Add($"Rewards[{rewardIndex}] Item Rewards[{itemIndex}] Item id is required.");

                    if (itemRewards[itemIndex].quantity < 1)
                        issues.Add($"Rewards[{rewardIndex}] Item Rewards[{itemIndex}] quantity must be at least 1.");
                }
            }

            return issues;
        }

        private void OnValidate()
        {
            Sanitize();
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
