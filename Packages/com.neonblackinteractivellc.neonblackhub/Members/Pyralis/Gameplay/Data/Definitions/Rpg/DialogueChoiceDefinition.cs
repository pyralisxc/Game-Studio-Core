using System;
using System.Linq;
using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Data.Definitions.Rpg
{
    [Serializable]
    public struct DialogueChoiceDefinition
    {
        public string choiceId;
        public string text;
        public string nextNodeId;
        public DialogueConditionDefinition[] conditions;
        public DialogueEffectDefinition[] effects;

        public string ChoiceId => Normalize(choiceId);
        public string Text => string.IsNullOrWhiteSpace(text) ? ChoiceId : text.Trim();
        public string NextNodeId => Normalize(nextNodeId);
        public DialogueConditionDefinition[] Conditions => conditions ?? Array.Empty<DialogueConditionDefinition>();
        public DialogueEffectDefinition[] Effects => effects ?? Array.Empty<DialogueEffectDefinition>();

        public void Sanitize()
        {
            choiceId = ChoiceId;
            text = Text;
            nextNodeId = NextNodeId;
            conditions = Conditions;
            effects = Effects;

            for (int i = 0; i < conditions.Length; i++)
                conditions[i].Sanitize();

            for (int i = 0; i < effects.Length; i++)
                effects[i].Sanitize();
        }

        public DialogueChoice CreateRuntimeChoice()
        {
            return new DialogueChoice(
                ChoiceId,
                Text,
                NextNodeId,
                Conditions.Select(condition => condition.CreateRuntimeCondition()).ToArray(),
                Effects.Select(effect => effect.CreateRuntimeEffect()).ToArray());
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
