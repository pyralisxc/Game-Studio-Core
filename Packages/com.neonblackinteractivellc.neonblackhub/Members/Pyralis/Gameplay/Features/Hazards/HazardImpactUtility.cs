using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Pickups;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Hazards
{
    public static class HazardImpactUtility
    {
        public static bool IsValidTarget(HealthComponent health, HazardTargetMode targeting)
        {
            if (health == null)
                return false;
            return targeting switch
            {
                HazardTargetMode.PlayerOnly => health.faction == Faction.Player,
                HazardTargetMode.EnemyOnly => health.faction == Faction.Enemy,
                _ => true
            };
        }

        public static bool TryApplyImpact(GameObject target, HazardImpactProfile profile, GameObject source, Vector3 hitPoint)
        {
            if (target == null || profile == null)
                return false;

            profile.Sanitize();

            HealthComponent health = target.GetComponentInParent<HealthComponent>();
            if (health == null || !IsValidTarget(health, profile.targeting))
                return false;

            if (profile.damagePerTick > 0f)
                health.TakeDamage(profile.damagePerTick, hitPoint, source);

            if (profile.knockbackForce > 0f)
            {
                KnockbackReceiver knockback = health.GetComponent<KnockbackReceiver>() ?? health.GetComponentInParent<KnockbackReceiver>();
                if (knockback != null)
                    knockback.ApplyKnockback(GetKnockbackDirection(health.transform.position, source != null ? source.transform.position : hitPoint, profile.useUpwardKnockback) * profile.knockbackForce);
            }

            IActorStatusEffectReceiver statusReceiver = health.GetComponent<IActorStatusEffectReceiver>() ?? health.GetComponentInParent<IActorStatusEffectReceiver>();
            if (statusReceiver != null && profile.statusEffects != null)
            {
                for (int i = 0; i < profile.statusEffects.Length; i++)
                {
                    if (profile.statusEffects[i] != null)
                        statusReceiver.ApplyStatusEffect(profile.statusEffects[i], source);
                }
            }

            if (profile.destroyCollectiblesOnContact)
            {
                IPickupCollectible collectible = target.GetComponentInParent<IPickupCollectible>();
                collectible?.RemoveFromPlay();
            }

            return true;
        }

        private static Vector3 GetKnockbackDirection(Vector3 targetPosition, Vector3 sourcePosition, bool useUpwardFallback)
        {
            Vector3 delta = targetPosition - sourcePosition;
            delta.z = 0f;
            if (delta.sqrMagnitude <= 0.0001f)
                return useUpwardFallback ? Vector3.up : Vector3.right;

            return delta.normalized;
        }
    }
}
