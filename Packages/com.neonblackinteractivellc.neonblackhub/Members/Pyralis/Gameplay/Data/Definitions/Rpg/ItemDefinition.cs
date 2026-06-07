using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions.Rpg
{
    [CreateAssetMenu(menuName = "NeonBlack/RPG/Item Definition", fileName = "ItemDefinition")]
    public class ItemDefinition : ScriptableObject
    {
        public string itemId = "item.new";
        public string displayName = "New Item";
        public string category = "General";
        public string rarity = "Common";
        public int maxStackSize = 1;
        public string[] tags = System.Array.Empty<string>();

        [TextArea(2, 5)]
        public string notes = string.Empty;

        public void Sanitize()
        {
            itemId = !string.IsNullOrWhiteSpace(itemId) ? itemId.Trim() : itemId;
            displayName = !string.IsNullOrWhiteSpace(displayName) ? displayName.Trim() : displayName;
            category = !string.IsNullOrWhiteSpace(category) ? category.Trim() : "General";
            rarity = !string.IsNullOrWhiteSpace(rarity) ? rarity.Trim() : "Common";
            maxStackSize = Mathf.Max(1, maxStackSize);
            tags = tags == null
                ? System.Array.Empty<string>()
                : tags.Select(tag => string.IsNullOrWhiteSpace(tag) ? string.Empty : tag.Trim())
                    .Where(tag => !string.IsNullOrWhiteSpace(tag))
                    .Distinct()
                    .ToArray();
        }

        public List<string> GetValidationIssues()
        {
            List<string> issues = new List<string>();

            if (string.IsNullOrWhiteSpace(itemId))
                issues.Add("Item stable id is required.");

            if (string.IsNullOrWhiteSpace(displayName))
                issues.Add("Display name is required.");

            if (string.IsNullOrWhiteSpace(category))
                issues.Add("Category is required so inventory tools can group items.");

            if (maxStackSize < 1)
                issues.Add("Max stack size must be at least 1.");

            return issues;
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
