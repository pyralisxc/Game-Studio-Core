using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions.Combat;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Hazards.Zones
{
    internal struct DamageZoneTargetState
    {
        public IActorHealthState health;
        public Component healthComponent;
        public IActorStatusEffectSink statusReceiver;
        public IActorKnockbackController knockback;
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
                state.health.TakeDamage(profile.damagePerTick, state.healthComponent.transform.position, source);

            if (profile.knockbackForce > 0f && state.knockback != null)
            {
                Vector3 delta = state.healthComponent.transform.position - sourceTransform.position;
                delta.z = 0f;
                Vector3 defaultDirection = profile.useUpwardKnockback ? Vector3.up : Vector3.right;
                Vector3 direction = delta.sqrMagnitude > 0.0001f ? delta.normalized : defaultDirection;
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

    internal sealed class DamageZoneTargetRuntime
    {
        private readonly List<DamageZoneTargetState> _targets = new List<DamageZoneTargetState>(8);
        private readonly HashSet<IActorHealthState> _targetLookup = new HashSet<IActorHealthState>();

        public bool HasTargets => _targets.Count > 0;

        public bool AddTarget(IActorHealthState health)
        {
            if (health == null || _targetLookup.Contains(health))
                return false;

            Component healthComponent = health as Component;
            if (healthComponent == null)
                return false;

            _targetLookup.Add(health);
            _targets.Add(new DamageZoneTargetState
            {
                health = health,
                healthComponent = healthComponent,
                statusReceiver = healthComponent.GetComponent<IActorStatusEffectSink>() ?? healthComponent.GetComponentInParent<IActorStatusEffectSink>(),
                knockback = healthComponent.GetComponent<IActorKnockbackController>() ?? healthComponent.GetComponentInParent<IActorKnockbackController>(),
                timer = 0f
            });
            return true;
        }

        public bool RemoveTarget(IActorHealthState health)
        {
            if (health == null || !_targetLookup.Remove(health))
                return false;

            for (int i = 0; i < _targets.Count; i++)
            {
                if (_targets[i].health == health)
                {
                    _targets.RemoveAt(i);
                    break;
                }
            }

            return true;
        }

        public void Tick(
            GameObject source,
            Transform sourceTransform,
            HazardImpactProfile impactProfile,
            float deltaTime)
        {
            if (_targets.Count == 0 || impactProfile == null)
                return;

            float interval = impactProfile.tickInterval;

            for (int i = _targets.Count - 1; i >= 0; i--)
            {
                DamageZoneTargetState state = _targets[i];
                IActorHealthState health = state.health;

                if (health == null || health.IsDead)
                {
                    _targetLookup.Remove(health);
                    _targets.RemoveAt(i);
                    continue;
                }

                state.timer -= deltaTime;
                if (state.timer > 0f)
                {
                    _targets[i] = state;
                    continue;
                }

                state.timer = interval;
                _targets[i] = state;

                DamageZoneImpactRuntime.ApplyProfileImpact(source, sourceTransform, state, impactProfile);
            }
        }
    }
}
