using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class RpgInventoryRuntimeTests
    {
        [Test]
        public void InventoryService_AddItem_StacksByOwnerAndItem()
        {
            InventoryService service = new InventoryService();
            RpgOwnerKey firstOwner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
            RpgOwnerKey secondOwner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-2");

            Assert.That(service.TryAddItem(firstOwner, "item.coin", 3, out string firstIssue), Is.True, firstIssue);
            Assert.That(service.TryAddItem(firstOwner, "item.coin", 2, out string secondIssue), Is.True, secondIssue);
            Assert.That(service.TryAddItem(secondOwner, "item.coin", 7, out string thirdIssue), Is.True, thirdIssue);

            Assert.That(service.GetItemCount(firstOwner, "item.coin"), Is.EqualTo(5));
            Assert.That(service.GetItemCount(secondOwner, "item.coin"), Is.EqualTo(7));
        }

        [Test]
        public void InventoryService_RemoveItem_RejectsMissingQuantity()
        {
            InventoryService service = new InventoryService();
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
            Assert.That(service.TryAddItem(owner, "item.potion", 1, out _), Is.True);

            bool removed = service.TryRemoveItem(owner, "item.potion", 2, out string issue);

            Assert.That(removed, Is.False);
            Assert.That(issue, Does.Contain("Not enough"));
            Assert.That(service.GetItemCount(owner, "item.potion"), Is.EqualTo(1));
        }

        [Test]
        public void InventoryService_TryAddItem_RejectsInvalidOwnerItemAndQuantity()
        {
            InventoryService service = new InventoryService();
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");

            Assert.That(service.TryAddItem(default, "item.coin", 1, out string ownerIssue), Is.False);
            Assert.That(ownerIssue, Does.Contain("valid RPG owner"));

            Assert.That(service.TryAddItem(owner, "", 1, out string itemIssue), Is.False);
            Assert.That(itemIssue, Does.Contain("Item id"));

            Assert.That(service.TryAddItem(owner, "item.coin", 0, out string quantityIssue), Is.False);
            Assert.That(quantityIssue, Does.Contain("positive"));
        }

        [Test]
        public void InventoryService_GetItems_ReturnsStableSnapshot()
        {
            InventoryService service = new InventoryService();
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
            Assert.That(service.TryAddItem(owner, "item.b", 2, out _), Is.True);
            Assert.That(service.TryAddItem(owner, "item.a", 1, out _), Is.True);

            InventoryItemStack[] stacks = service.GetItems(owner);

            Assert.That(stacks.Length, Is.EqualTo(2));
            Assert.That(stacks[0].ItemId, Is.EqualTo("item.a"));
            Assert.That(stacks[0].Quantity, Is.EqualTo(1));
            Assert.That(stacks[1].ItemId, Is.EqualTo("item.b"));
            Assert.That(stacks[1].Quantity, Is.EqualTo(2));
        }

        [Test]
        public void InventoryService_TryAddItem_RejectsStackLimitOverflowWhenCatalogProvided()
        {
            ItemDefinition potion = ScriptableObject.CreateInstance<ItemDefinition>();
            potion.itemId = "item.potion";
            potion.displayName = "Potion";
            potion.maxStackSize = 3;

            ItemCatalogDefinition catalog = ScriptableObject.CreateInstance<ItemCatalogDefinition>();
            catalog.items = new[] { potion };

            InventoryService service = new InventoryService(catalog);
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");

            Assert.That(service.TryAddItem(owner, "item.potion", 3, out string addIssue), Is.True, addIssue);
            Assert.That(service.TryAddItem(owner, "item.potion", 1, out string overflowIssue), Is.False);
            Assert.That(overflowIssue, Does.Contain("stack limit"));
            Assert.That(service.GetItemCount(owner, "item.potion"), Is.EqualTo(3));

            Object.DestroyImmediate(catalog);
            Object.DestroyImmediate(potion);
        }
    }
}
