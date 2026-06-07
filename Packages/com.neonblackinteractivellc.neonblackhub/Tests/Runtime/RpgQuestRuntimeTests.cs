using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class RpgQuestRuntimeTests
    {
        [Test]
        public void QuestService_ReportObjectiveProgress_CompletesObjectiveAndQuest()
        {
            QuestDefinition quest = CreateQuest(
                new QuestObjectiveDefinition("objective.collect", QuestObjectiveKind.CollectItem, "item.herb", 3));
            QuestService service = new QuestService();
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");

            Assert.That(service.TryStartQuest(owner, quest, out string startIssue), Is.True, startIssue);
            Assert.That(service.ReportObjectiveProgress(owner, quest, "objective.collect", 2, out QuestProgressState progress, out string issue), Is.True, issue);
            Assert.That(progress.GetObjectiveProgress("objective.collect"), Is.EqualTo(2));
            Assert.That(progress.Status, Is.EqualTo(QuestStatus.Active));

            Assert.That(service.ReportObjectiveProgress(owner, quest, "objective.collect", 1, out progress, out issue), Is.True, issue);
            Assert.That(progress.GetObjectiveProgress("objective.collect"), Is.EqualTo(3));
            Assert.That(progress.Status, Is.EqualTo(QuestStatus.Completed));

            Object.DestroyImmediate(quest);
        }

        [Test]
        public void QuestService_ReportObjectiveProgress_GrantsExperienceAndItemsOnce()
        {
            ProgressionService progression = new ProgressionService(null);
            InventoryService inventory = new InventoryService();
            QuestDefinition quest = CreateQuest(
                new QuestObjectiveDefinition("objective.talk", QuestObjectiveKind.TalkToNpc, "npc.elder", 1));
            quest.rewards = new[]
            {
                new QuestRewardDefinition
                {
                    experience = 25,
                    skillPoints = 2,
                    itemRewards = new[] { new QuestItemRewardDefinition("item.cape", 1) }
                }
            };
            QuestService service = new QuestService(progression, inventory);
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");

            Assert.That(service.TryStartQuest(owner, quest, out _), Is.True);
            Assert.That(service.ReportObjectiveProgress(owner, quest, "objective.talk", 1, out QuestProgressState progress, out string issue), Is.True, issue);
            Assert.That(progress.Status, Is.EqualTo(QuestStatus.Completed));
            Assert.That(progression.GetState(owner).Experience, Is.EqualTo(25));
            Assert.That(progression.GetState(owner).SkillPoints, Is.EqualTo(2));
            Assert.That(inventory.GetItemCount(owner, "item.cape"), Is.EqualTo(1));

            Assert.That(service.ReportObjectiveProgress(owner, quest, "objective.talk", 1, out progress, out issue), Is.False);
            Assert.That(progression.GetState(owner).Experience, Is.EqualTo(25));
            Assert.That(progression.GetState(owner).SkillPoints, Is.EqualTo(2));
            Assert.That(inventory.GetItemCount(owner, "item.cape"), Is.EqualTo(1));

            Object.DestroyImmediate(quest);
        }

        [Test]
        public void QuestService_ReportObjectiveProgress_TracksOwnersSeparately()
        {
            QuestDefinition quest = CreateQuest(
                new QuestObjectiveDefinition("objective.defeat", QuestObjectiveKind.DefeatActor, "enemy.bandit", 2));
            QuestService service = new QuestService();
            RpgOwnerKey firstOwner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
            RpgOwnerKey secondOwner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-2");

            Assert.That(service.TryStartQuest(firstOwner, quest, out _), Is.True);
            Assert.That(service.TryStartQuest(secondOwner, quest, out _), Is.True);
            Assert.That(service.ReportObjectiveProgress(firstOwner, quest, "objective.defeat", 2, out QuestProgressState firstProgress, out string firstIssue), Is.True, firstIssue);

            QuestProgressState secondProgress = service.GetProgress(secondOwner, quest.QuestId);
            Assert.That(firstProgress.Status, Is.EqualTo(QuestStatus.Completed));
            Assert.That(secondProgress.Status, Is.EqualTo(QuestStatus.Active));
            Assert.That(secondProgress.GetObjectiveProgress("objective.defeat"), Is.EqualTo(0));

            Object.DestroyImmediate(quest);
        }

        [Test]
        public void QuestService_TryStartQuest_RejectsCompletedNonRepeatableQuest()
        {
            QuestDefinition quest = CreateQuest(
                new QuestObjectiveDefinition("objective.score", QuestObjectiveKind.EarnScore, "score.session", 10));
            QuestService service = new QuestService();
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");

            Assert.That(service.TryStartQuest(owner, quest, out _), Is.True);
            Assert.That(service.ReportObjectiveProgress(owner, quest, "objective.score", 10, out QuestProgressState progress, out string issue), Is.True, issue);
            Assert.That(progress.Status, Is.EqualTo(QuestStatus.Completed));

            Assert.That(service.TryStartQuest(owner, quest, out string restartIssue), Is.False);
            Assert.That(restartIssue, Does.Contain("already completed"));

            Object.DestroyImmediate(quest);
        }

        private static QuestDefinition CreateQuest(params QuestObjectiveDefinition[] objectives)
        {
            QuestDefinition quest = ScriptableObject.CreateInstance<QuestDefinition>();
            quest.questId = "quest.test";
            quest.displayName = "Test Quest";
            quest.objectives = objectives;
            quest.rewards = new[] { new QuestRewardDefinition { experience = 1 } };
            return quest;
        }
    }
}
