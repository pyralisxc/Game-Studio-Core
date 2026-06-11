using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Core.Contracts.Rpg
{
    /// <summary>
    /// Service for managing actor equipment.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Inventory,
        Relevance = "Interface for equipping and unequipping items in specific slots.",
        ExpertAdvice = "ApplyEquipmentEffects can be used to inject item stats into an actor's StatSheet. Slots are defined by IEquipmentSlot.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/rpg"
    )]
    public interface IEquipmentService
{
        bool TryEquip(RpgOwnerKey owner, IEquipmentSlot slot, IEquippableItem item, out string issue);
        bool TryUnequip(RpgOwnerKey owner, string slotId, out IEquippableItem removedItem);
        bool TryGetEquippedItem(RpgOwnerKey owner, string slotId, out IEquippableItem item);
        string GetEquippedItemId(RpgOwnerKey owner, string slotId);
        void ApplyEquipmentEffects(RpgOwnerKey owner, StatSheet statSheet);
    }
}