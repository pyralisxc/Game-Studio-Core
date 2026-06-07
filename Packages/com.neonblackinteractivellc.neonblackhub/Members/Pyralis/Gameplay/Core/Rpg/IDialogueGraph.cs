namespace NeonBlack.Gameplay.Core.Rpg
{
    public interface IDialogueGraph
    {
        string GraphId { get; }
        string StartNodeId { get; }
        DialogueNode[] Nodes { get; }
        bool TryGetNode(string nodeId, out DialogueNode node);
    }
}
