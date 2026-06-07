namespace NeonBlack.Gameplay.Core.Rpg
{
    public interface IItemCatalog
    {
        bool TryGetMaxStackSize(string itemId, out int maxStackSize);
    }
}
