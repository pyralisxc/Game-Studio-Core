namespace NeonBlack.Gameplay.Core.Rpg
{
    public readonly struct DialogueEffect
    {
        public DialogueEffect(DialogueEffectKind kind, string targetId, string secondaryTargetId, int quantity, bool boolValue)
        {
            Kind = kind;
            TargetId = Normalize(targetId);
            SecondaryTargetId = Normalize(secondaryTargetId);
            Quantity = quantity < 1 ? 1 : quantity;
            BoolValue = boolValue;
        }

        public DialogueEffectKind Kind { get; }
        public string TargetId { get; }
        public string SecondaryTargetId { get; }
        public int Quantity { get; }
        public bool BoolValue { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
