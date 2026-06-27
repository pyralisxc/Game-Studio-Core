namespace NeonBlack.Gameplay.Modules.Rpg.Runtime
{
    public interface IDialogueGraph
    {
        string GraphId { get; }
        string StartNodeId { get; }
        DialogueNode[] Nodes { get; }
        bool TryGetNode(string nodeId, out DialogueNode node);
    }
}
