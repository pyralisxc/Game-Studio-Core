using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class RpgEquipmentRuntimeTests
    {
        [Test]
        public void EquipmentService_EquipItem_AssignsItemToOwnerSlot()
        {
            EquipmentSlotDefinition weaponSlot = CreateSlot("slot.weapon");
            EquippableItemDefinition sword = CreateEquippable("item.sword", "slot.weapon");
            EquipmentService service = new EquipmentService();
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");

            bool equipped = service.TryEquip(owner, weaponSlot, sword, out string issue);

            Assert.That(equipped, Is.True, issue);
            Assert.That(service.TryGetEquippedItem(owner, "slot.weapon", out IEquippableItem equippedItem), Is.True);
            Assert.That(equippedItem, Is.SameAs(sword));

            Object.DestroyImmediate(sword);
            Object.DestroyImmediate(weaponSlot);
        }

        [Test]
        public void EquipmentService_EquipItem_ReplacesPreviousItemInSameSlot()
        {
            EquipmentSlotDefinition weaponSlot = CreateSlot("slot.weapon");
            EquippableItemDefinition sword = CreateEquippable("item.sword", "slot.weapon");
            EquippableItemDefinition axe = CreateEquippable("item.axe", "slot.weapon");
            EquipmentService service = new EquipmentService();
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");

            Assert.That(service.TryEquip(owner, weaponSlot, sword, out _), Is.True);
            Assert.That(service.TryEquip(owner, weaponSlot, axe, out string issue), Is.True, issue);

            Assert.That(service.GetEquippedItemId(owner, "slot.weapon"), Is.EqualTo("item.axe"));

            Object.DestroyImmediate(axe);
            Object.DestroyImmediate(sword);
            Object.DestroyImmediate(weaponSlot);
        }

        [Test]
        public void EquipmentService_EquipItem_RejectsSlotMismatch()
        {
            EquipmentSlotDefinition weaponSlot = CreateSlot("slot.weapon");
            EquippableItemDefinition cape = CreateEquippable("item.cape", "slot.cape");
            EquipmentService service = new EquipmentService();
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");

            bool equipped = service.TryEquip(owner, weaponSlot, cape, out string issue);

            Assert.That(equipped, Is.False);
            Assert.That(issue, Does.Contain("slot.weapon"));

            Object.DestroyImmediate(cape);
            Object.DestroyImmediate(weaponSlot);
        }

        [Test]
        public void EquipmentService_ApplyEquipmentEffects_AddsAndRemovesStatModifiers()
        {
            EquipmentSlotDefinition capeSlot = CreateSlot("slot.cape");
            EquippableItemDefinition cape = CreateEquippable("item.cape", "slot.cape");
            cape.statModifiers = new[] { new StatModifierDefinition("stat.wisdom", 3f) };
            EquipmentService service = new EquipmentService();
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
            StatSheet sheet = new StatSheet();
            sheet.SetBaseValue("stat.wisdom", 5f);

            Assert.That(service.TryEquip(owner, capeSlot, cape, out _), Is.True);
            service.ApplyEquipmentEffects(owner, sheet);

            Assert.That(sheet.GetValue("stat.wisdom"), Is.EqualTo(8f));

            Assert.That(service.TryUnequip(owner, "slot.cape", out _), Is.True);
            service.ApplyEquipmentEffects(owner, sheet);

            Assert.That(sheet.GetValue("stat.wisdom"), Is.EqualTo(5f));

            Object.DestroyImmediate(cape);
            Object.DestroyImmediate(capeSlot);
        }

        private static EquipmentSlotDefinition CreateSlot(string slotId)
        {
            EquipmentSlotDefinition slot = ScriptableObject.CreateInstance<EquipmentSlotDefinition>();
            slot.slotId = slotId;
            slot.displayName = slotId;
            return slot;
        }

        private static EquippableItemDefinition CreateEquippable(string itemId, string slotId)
        {
            EquippableItemDefinition item = ScriptableObject.CreateInstance<EquippableItemDefinition>();
            item.itemId = itemId;
            item.displayName = itemId;
            item.allowedSlotIds = new[] { slotId };
            return item;
        }
    }
}
