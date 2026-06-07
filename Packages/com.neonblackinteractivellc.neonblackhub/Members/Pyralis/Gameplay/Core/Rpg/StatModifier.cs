namespace NeonBlack.Gameplay.Core.Rpg
{
    public readonly struct StatModifier
    {
        public StatModifier(string statId, float value, string sourceId)
        {
            StatId = string.IsNullOrWhiteSpace(statId) ? string.Empty : statId.Trim();
            Value = value;
            SourceId = string.IsNullOrWhiteSpace(sourceId) ? string.Empty : sourceId.Trim();
        }

        public string StatId { get; }
        public float Value { get; }
        public string SourceId { get; }
        public bool IsValid => !string.IsNullOrWhiteSpace(StatId);
    }
}
