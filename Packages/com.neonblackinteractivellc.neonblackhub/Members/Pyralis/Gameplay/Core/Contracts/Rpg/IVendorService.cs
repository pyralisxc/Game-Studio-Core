using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Core.Contracts.Rpg
{
    /// <summary>
    /// Service for managing vendor transactions.
    /// </summary>
    public interface IVendorService
    {
        bool TryBuy(RpgOwnerKey owner, IVendorDefinition vendor, string offerId, int quantity, out VendorTransactionResult result, out string issue);
        bool TrySell(RpgOwnerKey owner, IVendorDefinition vendor, string offerId, int quantity, out VendorTransactionResult result, out string issue);
    }
}