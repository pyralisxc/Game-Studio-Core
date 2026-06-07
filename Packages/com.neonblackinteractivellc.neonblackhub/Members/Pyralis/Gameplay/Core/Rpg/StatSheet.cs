using System.Collections.Generic;

namespace NeonBlack.Gameplay.Core.Rpg
{
    public sealed class StatSheet
    {
        private readonly Dictionary<string, float> _baseValues = new Dictionary<string, float>();
        private readonly List<StatModifier> _modifiers = new List<StatModifier>();

        public void SetBaseValue(string statId, float value)
        {
            string key = Normalize(statId);
            if (string.IsNullOrEmpty(key))
                return;

            _baseValues[key] = value;
        }

        public float GetBaseValue(string statId)
        {
            return _baseValues.TryGetValue(Normalize(statId), out float value) ? value : 0f;
        }

        public void AddModifier(StatModifier modifier)
        {
            if (!modifier.IsValid)
                return;

            _modifiers.Add(modifier);
        }

        public int RemoveModifiersFromSource(string sourceId)
        {
            string normalizedSource = Normalize(sourceId);
            if (string.IsNullOrEmpty(normalizedSource))
                return 0;

            int removed = 0;
            for (int i = _modifiers.Count - 1; i >= 0; i--)
            {
                if (Normalize(_modifiers[i].SourceId) != normalizedSource)
                    continue;

                _modifiers.RemoveAt(i);
                removed++;
            }

            return removed;
        }

        public float GetValue(string statId)
        {
            string key = Normalize(statId);
            if (string.IsNullOrEmpty(key))
                return 0f;

            float value = GetBaseValue(key);
            for (int i = 0; i < _modifiers.Count; i++)
            {
                if (Normalize(_modifiers[i].StatId) == key)
                    value += _modifiers[i].Value;
            }

            return value;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
