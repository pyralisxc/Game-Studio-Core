using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Contracts.Rpg;

namespace NeonBlack.Gameplay.Core.Rpg
{
    [AuthoringContract(
        ModuleId = "rpg.quest",
        Capability = AuthoringCapability.Dialogue,
        Lane = "RPG",
        NativeSetup = new[]
        {
            "create QuestDefinitions",
            "define QuestObjectives",
            "link quest start/end to Dialogue or Hub interaction"
        },
        AssignmentFields = new[] { "_progression", "_inventory" },
        FirstProof = "Quests can be started and objectives can be progressed."
    )]
    public sealed class QuestService : IQuestService
{
        private readonly ProgressionService _progression;
        private readonly InventoryService _inventory;
        private readonly Dictionary<RpgOwnerKey, Dictionary<string, QuestProgressRecord>> _progress =
            new Dictionary<RpgOwnerKey, Dictionary<string, QuestProgressRecord>>();

        public QuestService(ProgressionService progression = null, InventoryService inventory = null)
        {
            _progression = progression;
            _inventory = inventory;
        }

        public bool TryStartQuest(RpgOwnerKey owner, IQuestDefinition quest, out string issue)
        {
            if (!ValidateQuestRequest(owner, quest, out string questId, out issue))
                return false;

            Dictionary<string, QuestProgressRecord> ownerProgress = GetOrCreateOwnerProgress(owner);
            if (ownerProgress.TryGetValue(questId, out QuestProgressRecord existing))
            {
                if (existing.Status == QuestStatus.Completed && !quest.Repeatable)
                {
                    issue = $"Quest `{questId}` is already completed.";
                    return false;
                }

                if (existing.Status == QuestStatus.Active)
                {
                    issue = string.Empty;
                    return true;
                }
            }

            ownerProgress[questId] = CreateRecord(quest);
            issue = string.Empty;
            return true;
        }

        public bool ReportObjectiveProgress(
            RpgOwnerKey owner,
            IQuestDefinition quest,
            string objectiveId,
            int amount,
            out QuestProgressState progress,
            out string issue)
        {
            progress = default;
            if (!ValidateQuestRequest(owner, quest, out string questId, out issue))
                return false;

            string normalizedObjectiveId = Normalize(objectiveId);
            if (string.IsNullOrEmpty(normalizedObjectiveId) || !quest.TryGetObjective(normalizedObjectiveId, out QuestObjective objective))
            {
                issue = $"Quest objective `{normalizedObjectiveId}` could not be found.";
                return false;
            }

            if (amount <= 0)
            {
                issue = "Objective progress amount must be positive.";
                return false;
            }

            Dictionary<string, QuestProgressRecord> ownerProgress = GetOrCreateOwnerProgress(owner);
            if (!ownerProgress.TryGetValue(questId, out QuestProgressRecord record))
            {
                issue = $"Quest `{questId}` has not been started.";
                return false;
            }

            if (record.Status == QuestStatus.Completed)
            {
                issue = $"Quest `{questId}` is already completed.";
                progress = ToState(questId, record);
                return false;
            }

            int current = record.ObjectiveProgress.TryGetValue(normalizedObjectiveId, out int existing) ? existing : 0;
            int next = Math.Min(objective.RequiredQuantity, current + amount);
            record.ObjectiveProgress[normalizedObjectiveId] = next;

            if (AllObjectivesComplete(quest, record))
            {
                record.Status = QuestStatus.Completed;
                GrantRewards(owner, quest, record);
            }

            progress = ToState(questId, record);
            issue = string.Empty;
            return true;
        }

        public QuestProgressState GetProgress(RpgOwnerKey owner, string questId)
        {
            string normalizedQuestId = Normalize(questId);
            if (!owner.IsValid || string.IsNullOrEmpty(normalizedQuestId))
                return new QuestProgressState(normalizedQuestId, QuestStatus.NotStarted, null);

            return _progress.TryGetValue(owner, out Dictionary<string, QuestProgressRecord> ownerProgress)
                && ownerProgress.TryGetValue(normalizedQuestId, out QuestProgressRecord record)
                    ? ToState(normalizedQuestId, record)
                    : new QuestProgressState(normalizedQuestId, QuestStatus.NotStarted, null);
        }

        public RpgQuestSnapshot[] GetSnapshot(RpgOwnerKey owner)
        {
            if (!owner.IsValid || !_progress.TryGetValue(owner, out Dictionary<string, QuestProgressRecord> ownerProgress))
                return Array.Empty<RpgQuestSnapshot>();

            List<string> questIds = new List<string>(ownerProgress.Keys);
            questIds.Sort(StringComparer.Ordinal);
            RpgQuestSnapshot[] snapshot = new RpgQuestSnapshot[questIds.Count];
            for (int i = 0; i < questIds.Count; i++)
            {
                QuestProgressRecord record = ownerProgress[questIds[i]];
                snapshot[i] = new RpgQuestSnapshot(
                    questIds[i],
                    record.Status,
                    record.RewardsGranted,
                    CreateObjectiveSnapshot(record));
            }

            return snapshot;
        }

        public void RestoreSnapshot(RpgOwnerKey owner, RpgQuestSnapshot[] snapshot)
        {
            if (!owner.IsValid)
                return;

            Dictionary<string, QuestProgressRecord> ownerProgress = GetOrCreateOwnerProgress(owner);
            ownerProgress.Clear();

            RpgQuestSnapshot[] safeSnapshot = snapshot ?? Array.Empty<RpgQuestSnapshot>();
            for (int i = 0; i < safeSnapshot.Length; i++)
            {
                if (!safeSnapshot[i].IsValid)
                    continue;

                QuestProgressRecord record = new QuestProgressRecord
                {
                    Status = safeSnapshot[i].Status,
                    RewardsGranted = safeSnapshot[i].RewardsGranted
                };

                RpgQuestObjectiveSnapshot[] objectives = safeSnapshot[i].Objectives ?? Array.Empty<RpgQuestObjectiveSnapshot>();
                for (int objectiveIndex = 0; objectiveIndex < objectives.Length; objectiveIndex++)
                {
                    if (objectives[objectiveIndex].IsValid)
                        record.ObjectiveProgress[objectives[objectiveIndex].ObjectiveId] = objectives[objectiveIndex].Progress;
                }

                ownerProgress[safeSnapshot[i].QuestId] = record;
            }
        }

        private void GrantRewards(RpgOwnerKey owner, IQuestDefinition quest, QuestProgressRecord record)
        {
            if (record.RewardsGranted)
                return;

            QuestReward[] rewards = quest.Rewards ?? Array.Empty<QuestReward>();
            for (int rewardIndex = 0; rewardIndex < rewards.Length; rewardIndex++)
            {
                QuestReward reward = rewards[rewardIndex];
                if (reward.Experience > 0)
                    _progression?.AddExperience(owner, reward.Experience);

                if (reward.SkillPoints > 0)
                    _progression?.GrantSkillPoints(owner, reward.SkillPoints);

                QuestItemReward[] itemRewards = reward.ItemRewards ?? Array.Empty<QuestItemReward>();
                for (int itemIndex = 0; itemIndex < itemRewards.Length; itemIndex++)
                {
                    QuestItemReward itemReward = itemRewards[itemIndex];
                    if (itemReward.IsValid)
                        _inventory?.TryAddItem(owner, itemReward.ItemId, itemReward.Quantity, out _);
                }
            }

            record.RewardsGranted = true;
        }

        private static bool AllObjectivesComplete(IQuestDefinition quest, QuestProgressRecord record)
        {
            QuestObjective[] objectives = quest.Objectives ?? Array.Empty<QuestObjective>();
            if (objectives.Length == 0)
                return false;

            for (int i = 0; i < objectives.Length; i++)
            {
                QuestObjective objective = objectives[i];
                if (!record.ObjectiveProgress.TryGetValue(objective.ObjectiveId, out int progress) || progress < objective.RequiredQuantity)
                    return false;
            }

            return true;
        }

        private static QuestProgressRecord CreateRecord(IQuestDefinition quest)
        {
            QuestProgressRecord record = new QuestProgressRecord { Status = QuestStatus.Active };
            QuestObjective[] objectives = quest.Objectives ?? Array.Empty<QuestObjective>();
            for (int i = 0; i < objectives.Length; i++)
                record.ObjectiveProgress[objectives[i].ObjectiveId] = 0;

            return record;
        }

        private Dictionary<string, QuestProgressRecord> GetOrCreateOwnerProgress(RpgOwnerKey owner)
        {
            if (_progress.TryGetValue(owner, out Dictionary<string, QuestProgressRecord> ownerProgress))
                return ownerProgress;

            ownerProgress = new Dictionary<string, QuestProgressRecord>(StringComparer.Ordinal);
            _progress[owner] = ownerProgress;
            return ownerProgress;
        }

        private static QuestProgressState ToState(string questId, QuestProgressRecord record)
        {
            return new QuestProgressState(questId, record.Status, record.ObjectiveProgress);
        }

        private static RpgQuestObjectiveSnapshot[] CreateObjectiveSnapshot(QuestProgressRecord record)
        {
            List<string> objectiveIds = new List<string>(record.ObjectiveProgress.Keys);
            objectiveIds.Sort(StringComparer.Ordinal);
            RpgQuestObjectiveSnapshot[] snapshot = new RpgQuestObjectiveSnapshot[objectiveIds.Count];
            for (int i = 0; i < objectiveIds.Count; i++)
                snapshot[i] = new RpgQuestObjectiveSnapshot(objectiveIds[i], record.ObjectiveProgress[objectiveIds[i]]);

            return snapshot;
        }

        private static bool ValidateQuestRequest(RpgOwnerKey owner, IQuestDefinition quest, out string questId, out string issue)
        {
            questId = quest != null ? Normalize(quest.QuestId) : string.Empty;
            if (!owner.IsValid)
            {
                issue = "A valid RPG owner is required.";
                return false;
            }

            if (quest == null || string.IsNullOrEmpty(questId))
            {
                issue = "A valid quest definition is required.";
                return false;
            }

            issue = string.Empty;
            return true;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private sealed class QuestProgressRecord
        {
            public QuestStatus Status;
            public bool RewardsGranted;
            public readonly Dictionary<string, int> ObjectiveProgress = new Dictionary<string, int>(StringComparer.Ordinal);
        }
    }
}
