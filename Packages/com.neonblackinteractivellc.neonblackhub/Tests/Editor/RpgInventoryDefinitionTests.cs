using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NUnit.Framework;
using System.Linq;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public sealed class RpgInventoryDefinitionTests
    {
        [Test]
        public void ItemDefinition_GetValidationIssues_RequiresStableIdAndDisplayName()
        {
            ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();
            item.itemId = string.Empty;
            item.displayName = string.Empty;

            System.Collections.Generic.List<string> issues = item.GetValidationIssues();

            Assert.That(issues.Any(issue => issue.Contains("Item stable id")), Is.True);
            Assert.That(issues.Any(issue => issue.Contains("Display name")), Is.True);

            Object.DestroyImmediate(item);
        }

        [Test]
        public void ItemDefinition_GetValidationIssues_RejectsInvalidStackSize()
        {
            ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();
            item.itemId = "item.bad";
            item.displayName = "Bad Item";
            item.maxStackSize = 0;

            Assert.That(item.GetValidationIssues().Any(issue => issue.Contains("Max stack size")), Is.True);

            Object.DestroyImmediate(item);
        }

        [Test]
        public void ItemDefinition_Sanitize_TrimsTagsAndRemovesEmptyTags()
        {
            ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();
            item.itemId = " item.herb ";
            item.displayName = " Herb ";
            item.category = " Resource ";
            item.tags = new[] { " healing ", "", " quest " };

            item.Sanitize();

            Assert.That(item.itemId, Is.EqualTo("item.herb"));
            Assert.That(item.displayName, Is.EqualTo("Herb"));
            Assert.That(item.category, Is.EqualTo("Resource"));
            Assert.That(item.tags, Is.EqualTo(new[] { "healing", "quest" }));

            Object.DestroyImmediate(item);
        }

        [Test]
        public void ItemCatalogDefinition_GetValidationIssues_FlagsDuplicateItemIds()
        {
            ItemDefinition first = ScriptableObject.CreateInstance<ItemDefinition>();
            first.itemId = "item.coin";
            first.displayName = "Coin";
            ItemDefinition second = ScriptableObject.CreateInstance<ItemDefinition>();
            second.itemId = "item.coin";
            second.displayName = "Other Coin";
            ItemCatalogDefinition catalog = ScriptableObject.CreateInstance<ItemCatalogDefinition>();
            catalog.items = new[] { first, second };

            Assert.That(catalog.GetValidationIssues().Any(issue => issue.Contains("assigned more than once")), Is.True);

            Object.DestroyImmediate(catalog);
            Object.DestroyImmediate(second);
            Object.DestroyImmediate(first);
        }

        [Test]
        public void ItemCatalogDefinition_TryGetItem_FindsByStableId()
        {
            ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();
            item.itemId = "item.key";
            item.displayName = "Key";
            ItemCatalogDefinition catalog = ScriptableObject.CreateInstance<ItemCatalogDefinition>();
            catalog.items = new[] { item };

            bool found = catalog.TryGetItem("item.key", out ItemDefinition result);

            Assert.That(found, Is.True);
            Assert.That(result, Is.SameAs(item));

            Object.DestroyImmediate(catalog);
            Object.DestroyImmediate(item);
        }
    }
}
