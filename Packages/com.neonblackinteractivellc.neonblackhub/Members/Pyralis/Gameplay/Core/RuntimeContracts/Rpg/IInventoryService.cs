using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Core.Contracts.Rpg
{
    /// <summary>
    /// Service for managing participant inventories.
    /// </summary>
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
