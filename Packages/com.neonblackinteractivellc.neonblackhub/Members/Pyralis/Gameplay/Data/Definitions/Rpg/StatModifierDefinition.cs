using System;

namespace NeonBlack.Gameplay.Data.Definitions.Rpg
{
    [Serializable]
    public struct StatModifierDefinition
    {
        public string statId;
        public float value;

        public StatModifierDefinition(string statId, float value)
        {
            this.statId = string.IsNullOrWhiteSpace(statId) ? string.Empty : statId.Trim();
            this.value = value;
        }

        public string StatId => string.IsNullOrWhiteSpace(statId) ? string.Empty : statId.Trim();
        public float Value => value;
        public bool IsValid => !string.IsNullOrWhiteSpace(StatId);

        public void Sanitize()
        {
            statId = StatId;
        }
    }
}
