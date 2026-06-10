using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Contracts.Rpg;

namespace NeonBlack.Gameplay.Core.Rpg
{
    [AuthoringContract(
        ModuleId = "rpg.equipment",
        Capability = AuthoringCapability.Inventory,
        Relevance = "Manages RPG equipment slots and loadouts for actors, allowing items to be equipped and unequipped.",
        Lane = "RPG",
        RequiredInterfaces = new[] { typeof(IEquipmentSlot) },
        NativeSetup = new[]
        {
            "define EquipmentSlotDefinitions",
            "tag items as Equippable",
            "configure equipment visual mapping"
        },
        FirstProof = "Equip an item to an actor and verify its stats or visuals update accordingly."
    )]
    public sealed class EquipmentService : IEquipmentService
{
        private const string EquipmentModifierSourcePrefix = "equipment:";
        private readonly Dictionary<RpgOwnerKey, Dictionary<string, IEquippableItem>> _loadouts =
            new Dictionary<RpgOwnerKey, Dictionary<string, IEquippableItem>>();

        public bool TryEquip(RpgOwnerKey owner, IEquipmentSlot slot, IEquippableItem item, out string issue)
        {
            if (!owner.IsValid)
            {
                issue = "A valid RPG owner is required.";
                return false;
            }

            if (slot == null || string.IsNullOrWhiteSpace(slot.SlotId))
            {
                issue = "A valid equipment slot is required.";
                return false;
            }

            if (item == null || string.IsNullOrWhiteSpace(item.ItemId))
            {
                issue = "A valid equippable item is required.";
                return false;
            }

            string slotId = Normalize(slot.SlotId);
            if (!item.CanEquipInSlot(slotId))
            {
                issue = $"Item `{item.ItemId}` cannot be equipped in slot `{slotId}`.";
                return false;
            }

            Dictionary<string, IEquippableItem> loadout = GetOrCreateLoadout(owner);
            loadout[slotId] = item;
            issue = string.Empty;
            return true;
        }

        public bool TryUnequip(RpgOwnerKey owner, string slotId, out IEquippableItem removedItem)
        {
            removedItem = null;
            string normalizedSlotId = Normalize(slotId);
            if (!owner.IsValid || string.IsNullOrEmpty(normalizedSlotId))
                return false;

            if (!_loadouts.TryGetValue(owner, out Dictionary<string, IEquippableItem> loadout))
                return false;

            if (!loadout.TryGetValue(normalizedSlotId, out removedItem))
                return false;

            loadout.Remove(normalizedSlotId);
            return true;
        }

        public bool TryGetEquippedItem(RpgOwnerKey owner, string slotId, out IEquippableItem item)
        {
            item = null;
            string normalizedSlotId = Normalize(slotId);
            return owner.IsValid
                && !string.IsNullOrEmpty(normalizedSlotId)
                && _loadouts.TryGetValue(owner, out Dictionary<string, IEquippableItem> loadout)
                && loadout.TryGetValue(normalizedSlotId, out item)
                && item != null;
        }

        public string GetEquippedItemId(RpgOwnerKey owner, string slotId)
        {
            return TryGetEquippedItem(owner, slotId, out IEquippableItem item)
                ? item.ItemId
                : string.Empty;
        }

        public RpgEquipmentSnapshot[] GetSnapshot(RpgOwnerKey owner)
        {
            if (!owner.IsValid || !_loadouts.TryGetValue(owner, out Dictionary<string, IEquippableItem> loadout))
                return Array.Empty<RpgEquipmentSnapshot>();

            List<string> slotIds = new List<string>(loadout.Keys);
            slotIds.Sort(StringComparer.Ordinal);
            List<RpgEquipmentSnapshot> snapshot = new List<RpgEquipmentSnapshot>(slotIds.Count);
            for (int i = 0; i < slotIds.Count; i++)
            {
                if (loadout.TryGetValue(slotIds[i], out IEquippableItem item) && item != null)
                    snapshot.Add(new RpgEquipmentSnapshot(slotIds[i], item.ItemId));
            }

            return snapshot.ToArray();
        }

        public void RestoreSnapshot(RpgOwnerKey owner, RpgEquipmentSnapshot[] snapshot, Func<string, IEquippableItem> equippableResolver = null)
        {
            if (!owner.IsValid)
                return;

            Dictionary<string, IEquippableItem> loadout = GetOrCreateLoadout(owner);
            loadout.Clear();

            if (equippableResolver == null)
                return;

            RpgEquipmentSnapshot[] safeSnapshot = snapshot ?? Array.Empty<RpgEquipmentSnapshot>();
            for (int i = 0; i < safeSnapshot.Length; i++)
            {
                if (!safeSnapshot[i].IsValid)
                    continue;

                IEquippableItem item = equippableResolver(safeSnapshot[i].ItemId);
                if (item != null && item.CanEquipInSlot(safeSnapshot[i].SlotId))
                    loadout[safeSnapshot[i].SlotId] = item;
            }
        }

        public void ApplyEquipmentEffects(RpgOwnerKey owner, StatSheet statSheet)
        {
            if (!owner.IsValid || statSheet == null)
                return;

            statSheet.RemoveModifiersFromSource(EquipmentModifierSourcePrefix + owner);
            if (!_loadouts.TryGetValue(owner, out Dictionary<string, IEquippableItem> loadout))
                return;

            string sourceId = EquipmentModifierSourcePrefix + owner;
            foreach (IEquippableItem item in loadout.Values)
            {
                if (item == null)
                    continue;

                StatModifier[] modifiers = item.CreateStatModifiers(sourceId);
                for (int i = 0; i < modifiers.Length; i++)
                    statSheet.AddModifier(modifiers[i]);
            }
        }

        private Dictionary<string, IEquippableItem> GetOrCreateLoadout(RpgOwnerKey owner)
        {
            if (_loadouts.TryGetValue(owner, out Dictionary<string, IEquippableItem> loadout))
                return loadout;

            loadout = new Dictionary<string, IEquippableItem>();
            _loadouts[owner] = loadout;
            return loadout;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
