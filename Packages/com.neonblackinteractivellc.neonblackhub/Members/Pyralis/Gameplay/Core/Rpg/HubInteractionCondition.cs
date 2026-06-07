namespace NeonBlack.Gameplay.Core.Rpg
{
    public readonly struct HubInteractionCondition
    {
        public HubInteractionCondition(HubConditionKind kind, string targetId, string comparisonValue, int requiredQuantity, bool expected)
        {
            Kind = kind;
            TargetId = Normalize(targetId);
            ComparisonValue = Normalize(comparisonValue);
            RequiredQuantity = requiredQuantity < 1 ? 1 : requiredQuantity;
            Expected = expected;
        }

        public HubConditionKind Kind { get; }
        public string TargetId { get; }
        public string ComparisonValue { get; }
        public int RequiredQuantity { get; }
        public bool Expected { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
