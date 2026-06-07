using System;
using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Data.Definitions.Rpg
{
    [Serializable]
    public struct DialogueEffectDefinition
    {
        public DialogueEffectKind kind;
        public string targetId;
        public string secondaryTargetId;
        public int quantity;
        public bool boolValue;

        public string TargetId => Normalize(targetId);
        public string SecondaryTargetId => Normalize(secondaryTargetId);
        public int Quantity => quantity < 1 ? 1 : quantity;

        public void Sanitize()
        {
            targetId = TargetId;
            secondaryTargetId = SecondaryTargetId;
            quantity = Quantity;
        }

        public DialogueEffect CreateRuntimeEffect()
        {
            return new DialogueEffect(kind, TargetId, SecondaryTargetId, Quantity, boolValue);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
