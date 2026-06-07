namespace NeonBlack.Gameplay.Core.Rpg
{
    public readonly struct QuestItemReward
    {
        public QuestItemReward(string itemId, int quantity)
        {
            ItemId = string.IsNullOrWhiteSpace(itemId) ? string.Empty : itemId.Trim();
            Quantity = quantity < 1 ? 1 : quantity;
        }

        public string ItemId { get; }
        public int Quantity { get; }
        public bool IsValid => !string.IsNullOrWhiteSpace(ItemId) && Quantity > 0;
    }
}
