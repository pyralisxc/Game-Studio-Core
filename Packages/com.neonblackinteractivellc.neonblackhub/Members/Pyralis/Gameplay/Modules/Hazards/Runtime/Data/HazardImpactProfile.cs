using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions.Combat;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Modules.Hazards;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [AuthoringContract(
        Category = "Combat",
        CapabilityPath = "Combat/Actions/Hazard Impact Profile",
        Surface = AuthoringSurface.Goal,
        Summary = "Defines the damage, knockback, and status effects applied by a hazard on contact.",
        RequiredFields = new[] { nameof(effectId) },
        SetupSteps = new[] { "Set Damage and Tick Interval.", "Configure Targeting." },
        SuccessChecks = new[] { "Verify the hazard applies the correct damage and status effects to targets." },
        Tags = new[] { "capability:Combat", "runtime:Combat" }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Hazard Impact Profile", fileName = "HazardImpactProfile")]
    public class HazardImpactProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (string.IsNullOrWhiteSpace(effectId)) yield return RuntimeValidationIssue.Required("Effect Id is required.");
            if (tickInterval <= 0f) yield return RuntimeValidationIssue.Required("Tick Interval must be greater than zero.");
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
