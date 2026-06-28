using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Animation;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Definitions.Combat
{
    public enum StatusEffectStackMode
    {
        Ignore,
        RefreshDuration,
        StackDuration,
        StackMagnitude
    }

    public enum StatusEffectKind
    {
        Stun,
        Slow,
        SpeedBoost,
        DamageOverTime,
        HealOverTime,
        Poison,
        Burn,
        Shield,
        Armor,
        ArmorBreak,
        DamageBoost,
        KnockbackBoost,
        RegenBoost
    }

    [AuthoringContract(
        Category = "Combat",
        CapabilityPath = "Combat/Actions/Status Effect Definition",
        Surface = AuthoringSurface.Goal,
        Summary = "Defines a status effect (buff or debuff) that can be applied to actors.",
        RequiredFields = new[] { nameof(effectId), nameof(displayName), nameof(duration) },
        SetupSteps = new[] { "Set Effect Kind and Duration.", "Configure stack mode." },
        SuccessChecks = new[] { "Apply the effect to an actor and verify its magnitude and duration match the definition." },
        Tags = new[] { "capability:Combat", "runtime:Combat" }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Combat/Status Effect", fileName = "StatusEffectDefinition")]
    public class StatusEffectDefinition : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (string.IsNullOrWhiteSpace(effectId)) yield return PyralisRuntimeValidationIssue.Required("Effect Id is required.");
            if (duration < 0f) yield return PyralisRuntimeValidationIssue.Required("Duration cannot be negative.");
            if (tickInterval <= 0f) yield return PyralisRuntimeValidationIssue.Required("Tick Interval must be greater than zero.");
        }

        public string effectId = "status.effect";
        public string displayName = "Status Effect";
        public StatusEffectKind effectKind = StatusEffectKind.Stun;
        public StatusEffectStackMode stackMode = StatusEffectStackMode.RefreshDuration;
        public int maxStacks = 1;
        public float duration = 1f;
        public float magnitude = 1f;
        public float tickInterval = 0.5f;
        public ActorAnimationSignal applySignal = ActorAnimationSignal.Custom;
        public string customAnimationKey = "StatusEffect";

        public void Sanitize()
        {
            if (string.IsNullOrWhiteSpace(effectId))
            {
                effectId = !string.IsNullOrWhiteSpace(name) ? name : "status.effect";
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = effectId;
            }

            maxStacks = Mathf.Max(1, maxStacks);
            duration = Mathf.Max(0f, duration);
            magnitude = Mathf.Max(0f, magnitude);
            tickInterval = Mathf.Max(0.05f, tickInterval);
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
