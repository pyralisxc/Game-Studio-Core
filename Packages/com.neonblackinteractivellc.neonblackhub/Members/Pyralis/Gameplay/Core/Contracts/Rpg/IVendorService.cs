using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Core.Contracts.Rpg
{
    /// <summary>
    /// Service for managing vendor transactions.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Vendors,
        Relevance = "Interface for buying and selling items with NPC vendors.",
        ExpertAdvice = "Ensure the participant has enough currency before calling TryBuy. The service returns a VendorTransactionResult indicating success or failure reasons.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/vendors"
    )]
    public interface IVendorService
{
        bool TryBuy(RpgOwnerKey owner, IVendorDefinition vendor, string offerId, int quantity, out VendorTransactionResult result, out string issue);
        bool TrySell(RpgOwnerKey owner, IVendorDefinition vendor, string offerId, int quantity, out VendorTransactionResult result, out string issue);
    }
}