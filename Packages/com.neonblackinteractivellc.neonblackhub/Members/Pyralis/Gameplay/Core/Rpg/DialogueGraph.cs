using System;

namespace NeonBlack.Gameplay.Core.Rpg
{
    public readonly struct DialogueGraph : IDialogueGraph
    {
        public DialogueGraph(string graphId, string startNodeId, DialogueNode[] nodes)
        {
            GraphId = Normalize(graphId);
            StartNodeId = Normalize(startNodeId);
            Nodes = nodes ?? Array.Empty<DialogueNode>();
        }

        public string GraphId { get; }
        public string StartNodeId { get; }
        public DialogueNode[] Nodes { get; }
        public bool IsValid => !string.IsNullOrWhiteSpace(GraphId) && !string.IsNullOrWhiteSpace(StartNodeId);

        public bool TryGetNode(string nodeId, out DialogueNode node)
        {
            string normalizedNodeId = Normalize(nodeId);
            if (string.IsNullOrEmpty(normalizedNodeId))
            {
                node = default;
                return false;
            }

            for (int i = 0; i < Nodes.Length; i++)
            {
                if (Nodes[i].NodeId == normalizedNodeId)
                {
                    node = Nodes[i];
                    return true;
                }
            }

            node = default;
            return false;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
