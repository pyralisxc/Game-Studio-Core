using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Contracts.Rpg;

namespace NeonBlack.Gameplay.Core.Rpg
{
    [AuthoringContract(
        ModuleId = "rpg.inventory",
        Capability = AuthoringCapability.Inventory,
        Lane = "RPG",
        RequiredInterfaces = new[] { typeof(IItemCatalog) },
        NativeSetup = new[]
        {
            "create ItemCatalogDefinition",
            "define ItemDefinitions",
            "assign catalog to InventoryService"
        },
        AssignmentFields = new[] { "_catalog" },
        FirstProof = "Items can be added to and removed from a participant's inventory."
    )]
    public sealed class InventoryService : IInventoryService
{
        private readonly IItemCatalog _catalog;
        private readonly Dictionary<RpgOwnerKey, Dictionary<string, int>> _inventories = new Dictionary<RpgOwnerKey, Dictionary<string, int>>();

        public InventoryService(IItemCatalog catalog = null)
        {
            _catalog = catalog;
        }

        public bool TryAddItem(RpgOwnerKey owner, string itemId, int quantity, out string issue)
        {
            if (!ValidateRequest(owner, itemId, quantity, out string normalizedItemId, out issue))
                return false;

            Dictionary<string, int> inventory = GetOrCreateInventory(owner);
            int current = inventory.TryGetValue(normalizedItemId, out int existing) ? existing : 0;
            int next = current + quantity;

            if (_catalog != null && _catalog.TryGetMaxStackSize(normalizedItemId, out int maxStackSize) && maxStackSize > 0 && next > maxStackSize)
            {
                issue = $"Adding {quantity} `{normalizedItemId}` would exceed the stack limit of {maxStackSize}.";
                return false;
            }

            inventory[normalizedItemId] = next;
            issue = string.Empty;
            return true;
        }

        public bool TryRemoveItem(RpgOwnerKey owner, string itemId, int quantity, out string issue)
        {
            if (!ValidateRequest(owner, itemId, quantity, out string normalizedItemId, out issue))
                return false;

            if (!_inventories.TryGetValue(owner, out Dictionary<string, int> inventory)
                || !inventory.TryGetValue(normalizedItemId, out int current)
                || current < quantity)
            {
                issue = $"Not enough `{normalizedItemId}` to remove {quantity}.";
                return false;
            }

            int next = current - quantity;
            if (next == 0)
                inventory.Remove(normalizedItemId);
            else
                inventory[normalizedItemId] = next;

            issue = string.Empty;
            return true;
        }

        public int GetItemCount(RpgOwnerKey owner, string itemId)
        {
            string normalizedItemId = Normalize(itemId);
            if (!owner.IsValid || string.IsNullOrEmpty(normalizedItemId))
                return 0;

            return _inventories.TryGetValue(owner, out Dictionary<string, int> inventory)
                && inventory.TryGetValue(normalizedItemId, out int quantity)
                    ? quantity
                    : 0;
        }

        public bool ContainsItem(RpgOwnerKey owner, string itemId, int minimumQuantity = 1)
        {
            int required = minimumQuantity < 1 ? 1 : minimumQuantity;
            return GetItemCount(owner, itemId) >= required;
        }

        public InventoryItemStack[] GetItems(RpgOwnerKey owner)
        {
            if (!owner.IsValid || !_inventories.TryGetValue(owner, out Dictionary<string, int> inventory))
                return Array.Empty<InventoryItemStack>();

            List<string> itemIds = new List<string>(inventory.Keys);
            itemIds.Sort(StringComparer.Ordinal);
            InventoryItemStack[] stacks = new InventoryItemStack[itemIds.Count];
            for (int i = 0; i < itemIds.Count; i++)
                stacks[i] = new InventoryItemStack(itemIds[i], inventory[itemIds[i]]);

            return stacks;
        }

        public RpgInventoryItemSnapshot[] GetSnapshot(RpgOwnerKey owner)
        {
            InventoryItemStack[] stacks = GetItems(owner);
            RpgInventoryItemSnapshot[] snapshot = new RpgInventoryItemSnapshot[stacks.Length];
            for (int i = 0; i < stacks.Length; i++)
                snapshot[i] = new RpgInventoryItemSnapshot(stacks[i].ItemId, stacks[i].Quantity);

            return snapshot;
        }

        public void RestoreSnapshot(RpgOwnerKey owner, RpgInventoryItemSnapshot[] snapshot)
        {
            if (!owner.IsValid)
                return;

            Dictionary<string, int> inventory = GetOrCreateInventory(owner);
            inventory.Clear();

            RpgInventoryItemSnapshot[] safeSnapshot = snapshot ?? Array.Empty<RpgInventoryItemSnapshot>();
            for (int i = 0; i < safeSnapshot.Length; i++)
            {
                if (!safeSnapshot[i].IsValid)
                    continue;

                inventory[safeSnapshot[i].ItemId] = inventory.TryGetValue(safeSnapshot[i].ItemId, out int current)
                    ? current + safeSnapshot[i].Quantity
                    : safeSnapshot[i].Quantity;
            }
        }

        private Dictionary<string, int> GetOrCreateInventory(RpgOwnerKey owner)
        {
            if (_inventories.TryGetValue(owner, out Dictionary<string, int> inventory))
                return inventory;

            inventory = new Dictionary<string, int>(StringComparer.Ordinal);
            _inventories[owner] = inventory;
            return inventory;
        }

        private static bool ValidateRequest(RpgOwnerKey owner, string itemId, int quantity, out string normalizedItemId, out string issue)
        {
            normalizedItemId = Normalize(itemId);
            if (!owner.IsValid)
            {
                issue = "A valid RPG owner is required.";
                return false;
            }

            if (string.IsNullOrEmpty(normalizedItemId))
            {
                issue = "Item id is required.";
                return false;
            }

            if (quantity <= 0)
            {
                issue = "Item quantity must be positive.";
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
