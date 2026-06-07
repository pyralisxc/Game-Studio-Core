using System;

namespace NeonBlack.Gameplay.Core.Rpg
{
    public readonly struct DialogueNode
    {
        public DialogueNode(
            string nodeId,
            DialogueNodeKind kind,
            string speakerId,
            string lineText,
            DialogueChoice[] choices,
            DialogueEffect[] effects,
            string nextNodeId)
        {
            NodeId = Normalize(nodeId);
            Kind = kind;
            SpeakerId = Normalize(speakerId);
            LineText = lineText ?? string.Empty;
            Choices = choices ?? Array.Empty<DialogueChoice>();
            Effects = effects ?? Array.Empty<DialogueEffect>();
            NextNodeId = Normalize(nextNodeId);
        }

        public string NodeId { get; }
        public DialogueNodeKind Kind { get; }
        public string SpeakerId { get; }
        public string LineText { get; }
        public DialogueChoice[] Choices { get; }
        public DialogueEffect[] Effects { get; }
        public string NextNodeId { get; }
        public bool IsValid => !string.IsNullOrWhiteSpace(NodeId);

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
