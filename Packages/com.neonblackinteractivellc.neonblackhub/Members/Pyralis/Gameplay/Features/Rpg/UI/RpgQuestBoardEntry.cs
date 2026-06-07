using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Features.Rpg.UI
{
    public readonly struct RpgQuestBoardEntry
    {
        public RpgQuestBoardEntry(string questId, string title, QuestStatus status, bool repeatable)
        {
            QuestId = string.IsNullOrWhiteSpace(questId) ? string.Empty : questId.Trim();
            Title = string.IsNullOrWhiteSpace(title) ? QuestId : title.Trim();
            Status = status;
            Repeatable = repeatable;
        }

        public string QuestId { get; }
        public string Title { get; }
        public QuestStatus Status { get; }
        public bool Repeatable { get; }
        public bool CanStart => Status == QuestStatus.NotStarted || Status == QuestStatus.Completed && Repeatable;
    }
}
