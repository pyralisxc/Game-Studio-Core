namespace NeonBlack.Gameplay.Data.Rpg
{
    public interface IItemCatalog
    {
        bool TryGetMaxStackSize(string itemId, out int maxStackSize);
    }
}
