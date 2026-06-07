namespace NeonBlack.Gameplay.Core.Rpg
{
    public readonly struct VendorOffer
    {
        public VendorOffer(
            string offerId,
            string displayName,
            string itemId,
            string currencyItemId,
            int buyPrice,
            int sellPrice,
            bool canBuy,
            bool canSell)
        {
            OfferId = Normalize(offerId);
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? OfferId : displayName.Trim();
            ItemId = Normalize(itemId);
            CurrencyItemId = Normalize(currencyItemId);
            BuyPrice = buyPrice < 0 ? 0 : buyPrice;
            SellPrice = sellPrice < 0 ? 0 : sellPrice;
            CanBuy = canBuy;
            CanSell = canSell;
        }

        public string OfferId { get; }
        public string DisplayName { get; }
        public string ItemId { get; }
        public string CurrencyItemId { get; }
        public int BuyPrice { get; }
        public int SellPrice { get; }
        public bool CanBuy { get; }
        public bool CanSell { get; }
        public bool IsValid => !string.IsNullOrWhiteSpace(OfferId) && !string.IsNullOrWhiteSpace(ItemId);

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
