using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Rpg;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions.Rpg
{
    [CreateAssetMenu(menuName = "NeonBlack/RPG/Item Catalog", fileName = "ItemCatalogDefinition")]
    public class ItemCatalogDefinition : ScriptableObject, IItemCatalog
    {
        public string catalogId = "items.default";
        public string displayName = "Default Item Catalog";
        public ItemDefinition[] items = System.Array.Empty<ItemDefinition>();

        public void Sanitize()
        {
            catalogId = !string.IsNullOrWhiteSpace(catalogId) ? catalogId.Trim() : catalogId;
            displayName = !string.IsNullOrWhiteSpace(displayName) ? displayName.Trim() : catalogId;
            items ??= System.Array.Empty<ItemDefinition>();
        }

        public bool TryGetItem(string itemId, out ItemDefinition item)
        {
            string normalizedItemId = Normalize(itemId);
            if (string.IsNullOrEmpty(normalizedItemId) || items == null)
            {
                item = null;
                return false;
            }

            for (int i = 0; i < items.Length; i++)
            {
                ItemDefinition candidate = items[i];
                if (candidate != null && Normalize(candidate.itemId) == normalizedItemId)
                {
                    item = candidate;
                    return true;
                }
            }

            item = null;
            return false;
        }

        public bool TryGetMaxStackSize(string itemId, out int maxStackSize)
        {
            if (TryGetItem(itemId, out ItemDefinition item))
            {
                maxStackSize = item != null ? Mathf.Max(1, item.maxStackSize) : 0;
                return true;
            }

            maxStackSize = 0;
            return false;
        }

        public List<string> GetValidationIssues()
        {
            List<string> issues = new List<string>();

            if (string.IsNullOrWhiteSpace(catalogId))
                issues.Add("Item catalog id is required.");

            if (string.IsNullOrWhiteSpace(displayName))
                issues.Add("Display name is required.");

            HashSet<string> itemIds = new HashSet<string>();
            if (items == null)
                return issues;

            for (int i = 0; i < items.Length; i++)
            {
                ItemDefinition item = items[i];
                if (item == null)
                {
                    issues.Add($"Items[{i}] is null.");
                    continue;
                }

                string normalizedItemId = Normalize(item.itemId);
                if (!string.IsNullOrEmpty(normalizedItemId) && !itemIds.Add(normalizedItemId))
                    issues.Add($"Item id `{normalizedItemId}` is assigned more than once.");

                List<string> itemIssues = item.GetValidationIssues();
                for (int issueIndex = 0; issueIndex < itemIssues.Count; issueIndex++)
                    issues.Add($"Item `{item.itemId}`: {itemIssues[issueIndex]}");
            }

            return issues;
        }

        private void OnValidate()
        {
            Sanitize();
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
