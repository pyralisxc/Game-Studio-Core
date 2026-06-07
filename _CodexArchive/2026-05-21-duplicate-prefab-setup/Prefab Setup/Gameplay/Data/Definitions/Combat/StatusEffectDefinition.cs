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

    [CreateAssetMenu(menuName = "NeonBlack/Gameplay/Combat/Status Effect", fileName = "StatusEffectDefinition")]
    public class StatusEffectDefinition : ScriptableObject
    {
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
                effectId = name;
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
