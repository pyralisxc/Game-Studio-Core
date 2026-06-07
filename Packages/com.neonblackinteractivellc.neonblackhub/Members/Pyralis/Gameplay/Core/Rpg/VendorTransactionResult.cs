namespace NeonBlack.Gameplay.Core.Rpg
{
    public readonly struct VendorTransactionResult
    {
        public VendorTransactionResult(string vendorId, string offerId, string itemId, string currencyItemId, int quantity, int totalPrice)
        {
            VendorId = Normalize(vendorId);
            OfferId = Normalize(offerId);
            ItemId = Normalize(itemId);
            CurrencyItemId = Normalize(currencyItemId);
            Quantity = quantity < 0 ? 0 : quantity;
            TotalPrice = totalPrice < 0 ? 0 : totalPrice;
        }

        public string VendorId { get; }
        public string OfferId { get; }
        public string ItemId { get; }
        public string CurrencyItemId { get; }
        public int Quantity { get; }
        public int TotalPrice { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
