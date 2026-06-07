using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Hazards;
using UnityEngine;
using UnityEngine.Events;

namespace NeonBlack.Gameplay.Features.Zones
{
    [RequireComponent(typeof(Collider2D))]
    [AddComponentMenu("NeonBlack/Gameplay/Zones/Damage Zone 2D")]
    public class DamageZone2D : MonoBehaviour
    {
        [Header("Profile")]
        [SerializeField] private HazardImpactProfile impactProfile;
        [Header("Fallback Damage")]
        [SerializeField] private float damagePerTick = 10f;
        [SerializeField, Min(0.05f)] private float tickInterval = 0.5f;
        [SerializeField] private float knockbackForce = 0f;
        [SerializeField] private HazardTargetMode targeting = HazardTargetMode.All;

        [Header("Events")]
        public UnityEvent<GameObject> OnTargetEntered;
        public UnityEvent<GameObject> OnTargetExited;

        private readonly Dictionary<HealthComponent, float> _targets = new Dictionary<HealthComponent, float>();
        private readonly List<HealthComponent> _targetSnapshot = new List<HealthComponent>(8);
        private readonly List<HealthComponent> _expiredTargets = new List<HealthComponent>(4);

        private void Awake()
        {
            Collider2D collider = GetComponent<Collider2D>();
            collider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            HealthComponent health = other.GetComponentInParent<HealthComponent>();
            if (health == null || _targets.ContainsKey(health) || !IsValidTarget(health))
                return;

            _targets[health] = 0f;
            OnTargetEntered?.Invoke(health.gameObject);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            HealthComponent health = other.GetComponentInParent<HealthComponent>();
            if (health != null && _targets.Remove(health))
                OnTargetExited?.Invoke(health.gameObject);
        }

        private void Update()
        {
            if (_targets.Count == 0)
                return;

            float interval = impactProfile != null ? impactProfile.tickInterval : tickInterval;

            _targetSnapshot.Clear();
            _expiredTargets.Clear();
            foreach (HealthComponent health in _targets.Keys)
                _targetSnapshot.Add(health);

            for (int i = 0; i < _targetSnapshot.Count; i++)
            {
                HealthComponent health = _targetSnapshot[i];
                if (health == null || health.IsDead)
                {
                    _expiredTargets.Add(health);
                    continue;
                }

                float remaining = _targets[health] - Time.deltaTime;
                if (remaining > 0f)
                {
                    _targets[health] = remaining;
                    continue;
                }

                _targets[health] = interval;
                if (impactProfile != null)
                    HazardImpactUtility.TryApplyImpact(health.gameObject, impactProfile, gameObject, health.transform.position);
                else
                {
                    health.TakeDamage(damagePerTick, health.transform.position, gameObject);
                    if (knockbackForce > 0f)
                    {
                        KnockbackReceiver knockback = health.GetComponent<KnockbackReceiver>() ?? health.GetComponentInParent<KnockbackReceiver>();
                        knockback?.ApplyKnockback(Vector3.up * knockbackForce);
                    }
                }
            }

            for (int i = 0; i < _expiredTargets.Count; i++)
                _targets.Remove(_expiredTargets[i]);
        }

        private bool IsValidTarget(HealthComponent health)
        {
            return impactProfile != null
                ? HazardImpactUtility.IsValidTarget(health, impactProfile.targeting)
                : HazardImpactUtility.IsValidTarget(health, targeting);
        }
    }
}
