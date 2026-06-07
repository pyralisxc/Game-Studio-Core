namespace NeonBlack.Gameplay.Core.Rpg
{
    public sealed class VendorService
    {
        private readonly InventoryService _inventory;

        public VendorService(InventoryService inventory = null)
        {
            _inventory = inventory;
        }

        public bool TryBuy(RpgOwnerKey owner, IVendorDefinition vendor, string offerId, int quantity, out VendorTransactionResult result, out string issue)
        {
            result = default;
            if (!ValidateRequest(owner, vendor, offerId, quantity, out VendorOffer offer, out issue))
                return false;

            if (!offer.CanBuy)
            {
                issue = $"Vendor offer `{offer.OfferId}` cannot be bought.";
                return false;
            }

            if (_inventory == null)
            {
                issue = "Inventory service is required for vendor purchases.";
                return false;
            }

            int totalPrice = offer.BuyPrice * quantity;
            if (totalPrice > 0 && !_inventory.TryRemoveItem(owner, offer.CurrencyItemId, totalPrice, out issue))
                return false;

            if (!_inventory.TryAddItem(owner, offer.ItemId, quantity, out issue))
            {
                if (totalPrice > 0)
                    _inventory.TryAddItem(owner, offer.CurrencyItemId, totalPrice, out _);
                return false;
            }

            result = new VendorTransactionResult(vendor.VendorId, offer.OfferId, offer.ItemId, offer.CurrencyItemId, quantity, totalPrice);
            issue = string.Empty;
            return true;
        }

        public bool TrySell(RpgOwnerKey owner, IVendorDefinition vendor, string offerId, int quantity, out VendorTransactionResult result, out string issue)
        {
            result = default;
            if (!ValidateRequest(owner, vendor, offerId, quantity, out VendorOffer offer, out issue))
                return false;

            if (!offer.CanSell)
            {
                issue = $"Vendor offer `{offer.OfferId}` cannot be sold.";
                return false;
            }

            if (_inventory == null)
            {
                issue = "Inventory service is required for vendor sales.";
                return false;
            }

            if (!_inventory.TryRemoveItem(owner, offer.ItemId, quantity, out issue))
                return false;

            int totalPrice = offer.SellPrice * quantity;
            if (totalPrice > 0 && !_inventory.TryAddItem(owner, offer.CurrencyItemId, totalPrice, out issue))
            {
                _inventory.TryAddItem(owner, offer.ItemId, quantity, out _);
                return false;
            }

            result = new VendorTransactionResult(vendor.VendorId, offer.OfferId, offer.ItemId, offer.CurrencyItemId, quantity, totalPrice);
            issue = string.Empty;
            return true;
        }

        private static bool ValidateRequest(RpgOwnerKey owner, IVendorDefinition vendor, string offerId, int quantity, out VendorOffer offer, out string issue)
        {
            offer = default;
            string normalizedOfferId = Normalize(offerId);
            if (!owner.IsValid)
            {
                issue = "A valid RPG owner is required.";
                return false;
            }

            if (vendor == null || string.IsNullOrWhiteSpace(vendor.VendorId))
            {
                issue = "A valid vendor definition is required.";
                return false;
            }

            if (string.IsNullOrEmpty(normalizedOfferId) || !vendor.TryGetOffer(normalizedOfferId, out offer) || !offer.IsValid)
            {
                issue = $"Vendor offer `{normalizedOfferId}` could not be found.";
                return false;
            }

            if (quantity <= 0)
            {
                issue = "Vendor transaction quantity must be positive.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(offer.CurrencyItemId) && (offer.BuyPrice > 0 || offer.SellPrice > 0))
            {
                issue = $"Vendor offer `{offer.OfferId}` needs a currency item id.";
                return false;
            }

            issue = string.Empty;
            return true;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
