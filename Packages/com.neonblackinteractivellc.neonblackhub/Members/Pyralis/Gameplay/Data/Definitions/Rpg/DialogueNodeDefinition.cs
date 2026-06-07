using System;
using System.Linq;
using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Data.Definitions.Rpg
{
    [Serializable]
    public struct DialogueNodeDefinition
    {
        public string nodeId;
        public DialogueNodeKind kind;
        public string speakerId;
        public string lineText;
        public string nextNodeId;
        public DialogueChoiceDefinition[] choices;
        public DialogueEffectDefinition[] effects;

        public DialogueNodeDefinition(string nodeId, DialogueNodeKind kind, string speakerId, string lineText, string nextNodeId)
        {
            this.nodeId = Normalize(nodeId);
            this.kind = kind;
            this.speakerId = Normalize(speakerId);
            this.lineText = lineText ?? string.Empty;
            this.nextNodeId = Normalize(nextNodeId);
            choices = Array.Empty<DialogueChoiceDefinition>();
            effects = Array.Empty<DialogueEffectDefinition>();
        }

        public string NodeId => Normalize(nodeId);
        public string SpeakerId => Normalize(speakerId);
        public string NextNodeId => Normalize(nextNodeId);
        public DialogueChoiceDefinition[] Choices => choices ?? Array.Empty<DialogueChoiceDefinition>();
        public DialogueEffectDefinition[] Effects => effects ?? Array.Empty<DialogueEffectDefinition>();

        public void Sanitize()
        {
            nodeId = NodeId;
            speakerId = SpeakerId;
            lineText ??= string.Empty;
            nextNodeId = NextNodeId;
            choices = Choices;
            effects = Effects;

            for (int i = 0; i < choices.Length; i++)
                choices[i].Sanitize();

            for (int i = 0; i < effects.Length; i++)
                effects[i].Sanitize();
        }

        public DialogueNode CreateRuntimeNode()
        {
            return new DialogueNode(
                NodeId,
                kind,
                SpeakerId,
                lineText,
                Choices.Select(choice => choice.CreateRuntimeChoice()).ToArray(),
                Effects.Select(effect => effect.CreateRuntimeEffect()).ToArray(),
                NextNodeId);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
