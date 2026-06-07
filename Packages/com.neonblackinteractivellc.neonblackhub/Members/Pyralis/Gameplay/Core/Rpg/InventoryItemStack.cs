namespace NeonBlack.Gameplay.Core.Rpg
{
    public readonly struct InventoryItemStack
    {
        public InventoryItemStack(string itemId, int quantity)
        {
            ItemId = string.IsNullOrWhiteSpace(itemId) ? string.Empty : itemId.Trim();
            Quantity = quantity < 0 ? 0 : quantity;
        }

        public string ItemId { get; }
        public int Quantity { get; }
        public bool IsValid => !string.IsNullOrWhiteSpace(ItemId) && Quantity > 0;
    }
}
