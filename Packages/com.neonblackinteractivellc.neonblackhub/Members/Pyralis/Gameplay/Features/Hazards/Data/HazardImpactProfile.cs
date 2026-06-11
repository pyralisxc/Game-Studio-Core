using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Hazards;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat,
        Relevance = "Defines the damage, knockback, and status effects applied by a hazard on contact.",
        NativeSetup = new[] { "Create Asset.", "Set Damage and Tick Interval.", "Configure Targeting." },
        AssignmentFields = new[] { nameof(effectId), nameof(damagePerTick) },
        FirstProof = "Verify the hazard applies the correct damage and status effects to targets.",
        ExpertAdvice = "Use destroyCollectiblesOnContact for obstacle hazards that should 'eat' powerups."
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Hazard Impact Profile", fileName = "HazardImpactProfile")]
    public class HazardImpactProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (string.IsNullOrWhiteSpace(effectId)) yield return "Effect Id is required.";
            if (tickInterval <= 0f) yield return "Tick Interval must be greater than zero.";
        }

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
                effectId = !string.IsNullOrWhiteSpace(name) ? name : "hazard.impact";

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
