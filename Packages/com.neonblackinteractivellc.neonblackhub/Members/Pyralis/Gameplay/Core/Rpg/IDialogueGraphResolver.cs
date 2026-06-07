namespace NeonBlack.Gameplay.Core.Rpg
{
    public interface IDialogueGraphResolver
    {
        bool TryGetDialogueGraph(string graphId, out IDialogueGraph graph);
    }
}
