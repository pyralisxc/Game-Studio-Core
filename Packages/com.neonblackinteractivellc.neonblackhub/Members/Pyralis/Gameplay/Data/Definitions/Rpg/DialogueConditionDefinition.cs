using System;
using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Data.Definitions.Rpg
{
    [Serializable]
    public struct DialogueConditionDefinition
    {
        public DialogueConditionKind kind;
        public string targetId;
        public string comparisonValue;
        public int requiredQuantity;
        public bool expected;

        public string TargetId => Normalize(targetId);
        public string ComparisonValue => Normalize(comparisonValue);
        public int RequiredQuantity => requiredQuantity < 1 ? 1 : requiredQuantity;

        public void Sanitize()
        {
            targetId = TargetId;
            comparisonValue = ComparisonValue;
            requiredQuantity = RequiredQuantity;
        }

        public DialogueCondition CreateRuntimeCondition()
        {
            return new DialogueCondition(kind, TargetId, ComparisonValue, RequiredQuantity, expected);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
