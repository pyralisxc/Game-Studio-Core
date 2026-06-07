namespace NeonBlack.Gameplay.Core.Rpg
{
    public interface IEquippableItem
    {
        string ItemId { get; }
        bool CanEquipInSlot(string slotId);
        StatModifier[] CreateStatModifiers(string sourceId);
    }
}
