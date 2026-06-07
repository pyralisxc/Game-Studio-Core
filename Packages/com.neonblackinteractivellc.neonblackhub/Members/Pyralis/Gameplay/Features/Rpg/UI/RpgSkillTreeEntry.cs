namespace NeonBlack.Gameplay.Features.Rpg.UI
{
    public readonly struct RpgSkillTreeEntry
    {
        public RpgSkillTreeEntry(string nodeId, string title, int cost, int unlockCount, bool repeatable, string prerequisiteSummary, bool canUnlock)
        {
            NodeId = string.IsNullOrWhiteSpace(nodeId) ? string.Empty : nodeId.Trim();
            Title = string.IsNullOrWhiteSpace(title) ? NodeId : title.Trim();
            Cost = cost < 0 ? 0 : cost;
            UnlockCount = unlockCount < 0 ? 0 : unlockCount;
            Repeatable = repeatable;
            PrerequisiteSummary = prerequisiteSummary ?? string.Empty;
            CanUnlock = canUnlock;
        }

        public string NodeId { get; }
        public string Title { get; }
        public int Cost { get; }
        public int UnlockCount { get; }
        public bool Repeatable { get; }
        public string PrerequisiteSummary { get; }
        public bool CanUnlock { get; }
        public bool IsUnlocked => UnlockCount > 0;
    }
}
