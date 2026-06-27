namespace NeonBlack.Gameplay.Modules.Rpg.Runtime
{
    public interface IEquippableItem
    {
        string ItemId { get; }
        bool CanEquipInSlot(string slotId);
        StatModifier[] CreateStatModifiers(string sourceId);
    }
}
