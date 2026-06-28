using NeonBlack.Gameplay.Data.Rpg;
namespace NeonBlack.Gameplay.Modules.Rpg.Runtime
{
    public interface IDialogueGraphResolver
    {
        bool TryGetDialogueGraph(string graphId, out IDialogueGraph graph);
    }
}
