namespace NeonBlack.Gameplay.Modules.Rpg.Runtime
{
    public interface IItemCatalog
    {
        bool TryGetMaxStackSize(string itemId, out int maxStackSize);
    }
}
