using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Hazards;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [CreateAssetMenu(menuName = "NeonBlack/Gameplay/Profiles/Hazard Impact Profile", fileName = "HazardImpactProfile")]
    public class HazardImpactProfile : ScriptableObject
    {
        public string effectId = "hazard.impact";
        public HazardTargetMode targeting = HazardTargetMode.All;
        public float damagePerTick = 10f;
        public float tickInterval = 0.5f;
        public float knockbackForce = 0f;
        public bool useUpwardKnockback = true;
        public bool destroyCollectiblesOnContact = false;
        public StatusEffectDefinition[] statusEffects;

        public void Sanitize()
        {
            if (string.IsNullOrWhiteSpace(effectId))
                effectId = name;

            damagePerTick = Mathf.Max(0f, damagePerTick);
            tickInterval = Mathf.Max(0.05f, tickInterval);
            knockbackForce = Mathf.Max(0f, knockbackForce);
            if (statusEffects == null)
                statusEffects = System.Array.Empty<StatusEffectDefinition>();
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
