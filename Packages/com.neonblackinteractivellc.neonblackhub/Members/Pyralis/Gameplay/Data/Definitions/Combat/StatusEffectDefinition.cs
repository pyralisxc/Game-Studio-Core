using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
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
        Capability = AuthoringCapability.Combat,
        Relevance = "Defines a status effect (buff or debuff) that can be applied to actors.",
        NativeSetup = new[] { "Create Asset.", "Set Effect Kind and Duration.", "Configure stack mode." },
        AssignmentFields = new[] { nameof(effectId), nameof(displayName), nameof(duration) },
        FirstProof = "Apply the effect to an actor and verify its magnitude and duration match the definition.",
        ExpertAdvice = "Use tickInterval for effects that apply over time (e.g., Poison, Heal)."
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Combat/Status Effect", fileName = "StatusEffectDefinition")]
    public class StatusEffectDefinition : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (string.IsNullOrWhiteSpace(effectId)) yield return "Effect Id is required.";
            if (duration < 0f) yield return "Duration cannot be negative.";
            if (tickInterval <= 0f) yield return "Tick Interval must be greater than zero.";
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
