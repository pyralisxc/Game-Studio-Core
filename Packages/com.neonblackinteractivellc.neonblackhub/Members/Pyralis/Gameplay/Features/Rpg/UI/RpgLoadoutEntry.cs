namespace NeonBlack.Gameplay.Features.Rpg.UI
{
    public readonly struct RpgLoadoutEntry
    {
        public RpgLoadoutEntry(string itemId, string title, string slotSummary, bool isEquipped)
        {
            ItemId = string.IsNullOrWhiteSpace(itemId) ? string.Empty : itemId.Trim();
            Title = string.IsNullOrWhiteSpace(title) ? ItemId : title.Trim();
            SlotSummary = slotSummary ?? string.Empty;
            IsEquipped = isEquipped;
        }

        public string ItemId { get; }
        public string Title { get; }
        public string SlotSummary { get; }
        public bool IsEquipped { get; }
    }
}
