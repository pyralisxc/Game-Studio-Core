using System;

namespace NeonBlack.Gameplay.Core.Rpg
{
    public readonly struct DialogueChoice
    {
        public DialogueChoice(string choiceId, string text, string nextNodeId, DialogueCondition[] conditions, DialogueEffect[] effects)
        {
            ChoiceId = Normalize(choiceId);
            Text = string.IsNullOrWhiteSpace(text) ? ChoiceId : text.Trim();
            NextNodeId = Normalize(nextNodeId);
            Conditions = conditions ?? Array.Empty<DialogueCondition>();
            Effects = effects ?? Array.Empty<DialogueEffect>();
        }

        public string ChoiceId { get; }
        public string Text { get; }
        public string NextNodeId { get; }
        public DialogueCondition[] Conditions { get; }
        public DialogueEffect[] Effects { get; }
        public bool IsValid => !string.IsNullOrWhiteSpace(ChoiceId);

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
