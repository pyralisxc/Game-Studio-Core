using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Core.Contracts.Rpg
{
    /// <summary>
    /// Service for managing participant inventories.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Inventory,
        Relevance = "Core interface for adding, removing, and querying items in a participant's inventory.",
        ExpertAdvice = "Use RpgOwnerKey to identify which actor's inventory is being modified. This service handles item stacking and issue reporting.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/inventory"
    )]
    public interface IInventoryService
{
        bool TryAddItem(RpgOwnerKey owner, string itemId, int quantity, out string issue);
        bool TryRemoveItem(RpgOwnerKey owner, string itemId, int quantity, out string issue);
        int GetItemCount(RpgOwnerKey owner, string itemId);
        bool ContainsItem(RpgOwnerKey owner, string itemId, int minimumQuantity = 1);
        InventoryItemStack[] GetItems(RpgOwnerKey owner);
        // Snapshot methods omitted for simplicity or included if needed for save system
    }
}