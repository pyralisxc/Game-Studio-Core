namespace NeonBlack.Gameplay.Data.Rpg
{
    public interface IEquippableItem
    {
        string ItemId { get; }
        bool CanEquipInSlot(string slotId);
        StatModifier[] CreateStatModifiers(string sourceId);
    }
}
