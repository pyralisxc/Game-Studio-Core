using System;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NeonBlack.Gameplay.Features.Rpg.UI;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class RpgLoadoutPanelPresenterRuntimeTests
    {
        [Test]
        public void RpgLoadoutPanelPresenter_ShowInteractionResult_ListsEquippableItems()
        {
            GameObject root = new GameObject("Loadout Panel");
            EquipmentSlotDefinition weaponSlot = CreateSlot("slot.weapon", "Weapon");
            EquippableItemDefinition sword = CreateEquippable("item.sword", "Iron Sword", "slot.weapon");
            try
            {
                RpgLoadoutPanelPresenter presenter = root.AddComponent<RpgLoadoutPanelPresenter>();
                RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
                presenter.ConfigureForTests(owner, new EquipmentService(), new IEquipmentSlot[] { weaponSlot }, new IEquippableItem[] { sword });

                Assert.That(presenter.ShowInteractionResult(Result()), Is.True, presenter.LastIssue);

                Assert.That(presenter.Entries.Length, Is.EqualTo(1));
                Assert.That(presenter.Entries[0].ItemId, Is.EqualTo("item.sword"));
                Assert.That(presenter.Entries[0].Title, Is.EqualTo("Iron Sword"));
                Assert.That(presenter.Entries[0].SlotSummary, Is.EqualTo("Weapon"));
                Assert.That(presenter.Entries[0].IsEquipped, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(sword);
                UnityEngine.Object.DestroyImmediate(weaponSlot);
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void RpgLoadoutPanelPresenter_EquipAndUnequipSelectedItem_UpdatesLoadout()
        {
            GameObject root = new GameObject("Loadout Panel");
            EquipmentSlotDefinition weaponSlot = CreateSlot("slot.weapon", "Weapon");
            EquippableItemDefinition sword = CreateEquippable("item.sword", "Iron Sword", "slot.weapon");
            try
            {
                EquipmentService service = new EquipmentService();
                RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
                RpgLoadoutPanelPresenter presenter = root.AddComponent<RpgLoadoutPanelPresenter>();
                presenter.ConfigureForTests(owner, service, new IEquipmentSlot[] { weaponSlot }, new IEquippableItem[] { sword });
                presenter.ShowInteractionResult(Result());

                Assert.That(presenter.EquipSelectedItem(), Is.True, presenter.LastIssue);

                Assert.That(service.GetEquippedItemId(owner, "slot.weapon"), Is.EqualTo("item.sword"));
                Assert.That(presenter.Entries[0].IsEquipped, Is.True);

                Assert.That(presenter.UnequipSelectedItem(), Is.True, presenter.LastIssue);

                Assert.That(service.GetEquippedItemId(owner, "slot.weapon"), Is.Empty);
                Assert.That(presenter.Entries[0].IsEquipped, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(sword);
                UnityEngine.Object.DestroyImmediate(weaponSlot);
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static HubInteractionResult Result()
        {
            return new HubInteractionResult(
                HubInteractionStatus.Selected,
                string.Empty,
                default,
                PlayerPanelRoute.Loadout,
                string.Empty,
                string.Empty,
                string.Empty,
                Array.Empty<HubNotificationPayload>());
        }

        private static EquipmentSlotDefinition CreateSlot(string slotId, string displayName)
        {
            EquipmentSlotDefinition slot = ScriptableObject.CreateInstance<EquipmentSlotDefinition>();
            slot.slotId = slotId;
            slot.displayName = displayName;
            return slot;
        }

        private static EquippableItemDefinition CreateEquippable(string itemId, string displayName, string slotId)
        {
            EquippableItemDefinition item = ScriptableObject.CreateInstance<EquippableItemDefinition>();
            item.itemId = itemId;
            item.displayName = displayName;
            item.allowedSlotIds = new[] { slotId };
            return item;
        }
    }
}
