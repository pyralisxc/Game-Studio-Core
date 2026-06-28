namespace NeonBlack.Gameplay.Data.Rpg
{
    public interface IDialogueGraph
    {
        string GraphId { get; }
        string StartNodeId { get; }
        DialogueNode[] Nodes { get; }
        bool TryGetNode(string nodeId, out DialogueNode node);
    }
}
