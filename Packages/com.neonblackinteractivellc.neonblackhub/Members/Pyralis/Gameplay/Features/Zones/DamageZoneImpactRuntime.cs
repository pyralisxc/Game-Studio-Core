using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Combat;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Zones
{
    internal struct DamageZoneTargetState
    {
        public HealthComponent health;
        public IActorStatusEffectReceiver statusReceiver;
        public KnockbackReceiver knockback;
        public float timer;
    }

    internal static class DamageZoneImpactRuntime
    {
        public static void ApplyProfileImpact(
            GameObject source,
            Transform sourceTransform,
            DamageZoneTargetState state,
            HazardImpactProfile profile)
        {
            if (profile.damagePerTick > 0f)
                state.health.TakeDamage(profile.damagePerTick, state.health.transform.position, source);

            if (profile.knockbackForce > 0f && state.knockback != null)
            {
                Vector3 delta = state.health.transform.position - sourceTransform.position;
                delta.z = 0f;
                Vector3 fallback = profile.useUpwardKnockback ? Vector3.up : Vector3.right;
                Vector3 direction = delta.sqrMagnitude > 0.0001f ? delta.normalized : fallback;
                state.knockback.ApplyKnockback(direction * profile.knockbackForce);
            }

            if (state.statusReceiver == null || profile.statusEffects == null)
                return;

            for (int i = 0; i < profile.statusEffects.Length; i++)
            {
                if (profile.statusEffects[i] != null)
                    state.statusReceiver.ApplyStatusEffect(profile.statusEffects[i], source);
            }
        }
    }
}
