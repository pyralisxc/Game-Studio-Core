using NeonBlack.Gameplay.Core.Rpg;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions.Rpg
{
    [System.Serializable]
    public struct VendorOfferDefinition
    {
        public string offerId;
        public string displayName;
        public string itemId;
        public string currencyItemId;
        [Min(0)] public int buyPrice;
        [Min(0)] public int sellPrice;
        public bool canBuy;
        public bool canSell;

        public VendorOfferDefinition(string offerId, string displayName, string itemId, string currencyItemId, int buyPrice, int sellPrice, bool canBuy, bool canSell)
        {
            this.offerId = offerId;
            this.displayName = displayName;
            this.itemId = itemId;
            this.currencyItemId = currencyItemId;
            this.buyPrice = buyPrice;
            this.sellPrice = sellPrice;
            this.canBuy = canBuy;
            this.canSell = canSell;
        }

        public string OfferId => Normalize(offerId);
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? OfferId : displayName.Trim();
        public string ItemId => Normalize(itemId);
        public string CurrencyItemId => Normalize(currencyItemId);

        public void Sanitize()
        {
            offerId = OfferId;
            displayName = DisplayName;
            itemId = ItemId;
            currencyItemId = CurrencyItemId;
            buyPrice = Mathf.Max(0, buyPrice);
            sellPrice = Mathf.Max(0, sellPrice);
        }

        public VendorOffer CreateRuntimeOffer()
        {
            return new VendorOffer(OfferId, DisplayName, ItemId, CurrencyItemId, buyPrice, sellPrice, canBuy, canSell);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
