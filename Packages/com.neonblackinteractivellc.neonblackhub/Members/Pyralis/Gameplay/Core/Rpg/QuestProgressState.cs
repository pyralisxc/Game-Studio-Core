using System;
using System.Collections.Generic;

namespace NeonBlack.Gameplay.Core.Rpg
{
    public readonly struct QuestProgressState
    {
        private readonly Dictionary<string, int> _objectiveProgress;

        public QuestProgressState(string questId, QuestStatus status, Dictionary<string, int> objectiveProgress)
        {
            QuestId = string.IsNullOrWhiteSpace(questId) ? string.Empty : questId.Trim();
            Status = status;
            _objectiveProgress = objectiveProgress == null
                ? new Dictionary<string, int>(StringComparer.Ordinal)
                : new Dictionary<string, int>(objectiveProgress, StringComparer.Ordinal);
        }

        public string QuestId { get; }
        public QuestStatus Status { get; }

        public int GetObjectiveProgress(string objectiveId)
        {
            string normalizedObjectiveId = Normalize(objectiveId);
            return !string.IsNullOrEmpty(normalizedObjectiveId)
                && _objectiveProgress != null
                && _objectiveProgress.TryGetValue(normalizedObjectiveId, out int progress)
                    ? progress
                    : 0;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
