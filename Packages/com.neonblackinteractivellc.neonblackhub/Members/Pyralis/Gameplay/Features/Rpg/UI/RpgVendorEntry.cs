namespace NeonBlack.Gameplay.Features.Rpg.UI
{
    public readonly struct RpgVendorEntry
    {
        public RpgVendorEntry(string offerId, string title, string itemId, string priceText, bool canBuy, bool canSell)
        {
            OfferId = string.IsNullOrWhiteSpace(offerId) ? string.Empty : offerId.Trim();
            Title = string.IsNullOrWhiteSpace(title) ? OfferId : title.Trim();
            ItemId = string.IsNullOrWhiteSpace(itemId) ? string.Empty : itemId.Trim();
            PriceText = priceText ?? string.Empty;
            CanBuy = canBuy;
            CanSell = canSell;
        }

        public string OfferId { get; }
        public string Title { get; }
        public string ItemId { get; }
        public string PriceText { get; }
        public bool CanBuy { get; }
        public bool CanSell { get; }
    }
}
