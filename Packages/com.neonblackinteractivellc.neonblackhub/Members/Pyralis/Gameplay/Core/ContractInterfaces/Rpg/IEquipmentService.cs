using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Core.Contracts.Rpg
{
    /// <summary>
    /// Service for managing actor equipment.
    /// </summary>
    public interface IEquipmentService
{
        bool TryEquip(RpgOwnerKey owner, IEquipmentSlot slot, IEquippableItem item, out string issue);
        bool TryUnequip(RpgOwnerKey owner, string slotId, out IEquippableItem removedItem);
        bool TryGetEquippedItem(RpgOwnerKey owner, string slotId, out IEquippableItem item);
        string GetEquippedItemId(RpgOwnerKey owner, string slotId);
        void ApplyEquipmentEffects(RpgOwnerKey owner, StatSheet statSheet);
    }
}
