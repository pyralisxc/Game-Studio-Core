using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NUnit.Framework;
using System.Linq;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public sealed class RpgQuestDefinitionTests
    {
        [Test]
        public void QuestDefinition_GetValidationIssues_FlagsDuplicateObjectiveIds()
        {
            QuestDefinition quest = ScriptableObject.CreateInstance<QuestDefinition>();
            quest.questId = "quest.test";
            quest.displayName = "Test";
            quest.objectives = new[]
            {
                new QuestObjectiveDefinition("objective.same", QuestObjectiveKind.CollectItem, "item.herb", 1),
                new QuestObjectiveDefinition("objective.same", QuestObjectiveKind.DefeatActor, "enemy.bandit", 1)
            };
            quest.rewards = new[] { new QuestRewardDefinition { experience = 1 } };

            Assert.That(quest.GetValidationIssues().Any(issue => issue.Contains("assigned more than once")), Is.True);

            Object.DestroyImmediate(quest);
        }

        [Test]
        public void QuestDefinition_GetValidationIssues_FlagsMissingRewards()
        {
            QuestDefinition quest = ScriptableObject.CreateInstance<QuestDefinition>();
            quest.questId = "quest.test";
            quest.displayName = "Test";
            quest.objectives = new[] { new QuestObjectiveDefinition("objective.talk", QuestObjectiveKind.TalkToNpc, "npc.elder", 1) };
            quest.rewards = System.Array.Empty<QuestRewardDefinition>();

            Assert.That(quest.GetValidationIssues().Any(issue => issue.Contains("reward")), Is.True);

            Object.DestroyImmediate(quest);
        }

        [Test]
        public void QuestDefinition_GetValidationIssues_FlagsInvalidObjectivePayload()
        {
            QuestDefinition quest = ScriptableObject.CreateInstance<QuestDefinition>();
            quest.questId = "quest.test";
            quest.displayName = "Test";
            quest.objectives = new[] { new QuestObjectiveDefinition("objective.zone", QuestObjectiveKind.ReachZone, "", 1) };
            quest.rewards = new[] { new QuestRewardDefinition { experience = 1 } };

            Assert.That(quest.GetValidationIssues().Any(issue => issue.Contains("Target id")), Is.True);

            Object.DestroyImmediate(quest);
        }

        [Test]
        public void QuestDefinition_TryGetObjectiveDefinition_FindsObjectiveById()
        {
            QuestDefinition quest = ScriptableObject.CreateInstance<QuestDefinition>();
            quest.objectives = new[] { new QuestObjectiveDefinition("objective.root", QuestObjectiveKind.ProjectEvent, "event.started", 1) };

            Assert.That(quest.TryGetObjectiveDefinition("objective.root", out QuestObjectiveDefinition objective), Is.True);
            Assert.That(objective.targetId, Is.EqualTo("event.started"));

            Object.DestroyImmediate(quest);
        }
    }
}
