using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Hazards;
using UnityEngine;
using UnityEngine.Events;

namespace NeonBlack.Gameplay.Features.Zones
{
    /// <summary>
    /// Trigger volume that repeatedly damages overlapping actors. This can still
    /// run from local fallback values, but the preferred path is a shared
    /// HazardImpactProfile so 2D and 3D hazards use the same authored payload.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class DamageZone : MonoBehaviour
    {
        [Header("Profile")]
        [SerializeField] private HazardImpactProfile impactProfile;
        [Header("Fallback Damage")]
        [SerializeField] private float damagePerTick = 10f;
        [SerializeField, Min(0.05f)] private float tickInterval = 0.5f;
        [SerializeField] private float knockbackForce = 0f;

        [Header("Fallback Targeting")]
        [SerializeField] private DamageTarget targeting = DamageTarget.All;

        [Header("Events")]
        public UnityEvent<GameObject> OnTargetEntered;
        public UnityEvent<GameObject> OnTargetExited;

        public enum DamageTarget
        {
            PlayerOnly,
            EnemyOnly,
            All
        }

        private readonly Dictionary<HealthComponent, float> _targets = new Dictionary<HealthComponent, float>();

        private void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;
            impactProfile?.Sanitize();
        }

        private void OnTriggerEnter(Collider other)
        {
            HealthComponent health = other.GetComponentInParent<HealthComponent>();
            if (health == null || _targets.ContainsKey(health) || !IsValidTarget(health))
                return;

            _targets[health] = 0f;
            OnTargetEntered?.Invoke(health.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            HealthComponent health = other.GetComponentInParent<HealthComponent>();
            if (health != null && _targets.Remove(health))
                OnTargetExited?.Invoke(health.gameObject);
        }

        private void Update()
        {
            if (_targets.Count == 0)
                return;

            List<HealthComponent> expired = null;
            float activeTickInterval = impactProfile != null ? impactProfile.tickInterval : tickInterval;

            foreach (HealthComponent health in _targets.Keys)
            {
                if (health == null || health.IsDead)
                {
                    expired ??= new List<HealthComponent>();
                    expired.Add(health);
                    continue;
                }

                _targets[health] -= Time.deltaTime;
                if (_targets[health] > 0f)
                    continue;

                _targets[health] = activeTickInterval;

                if (impactProfile != null)
                {
                    HazardImpactUtility.TryApplyImpact(health.gameObject, impactProfile, gameObject, health.transform.position);
                    continue;
                }

                health.TakeDamage(damagePerTick, health.transform.position, gameObject);
                if (knockbackForce > 0f)
                {
                    KnockbackReceiver knockback = health.GetComponent<KnockbackReceiver>() ?? health.GetComponentInParent<KnockbackReceiver>();
                    if (knockback != null)
                        knockback.ApplyKnockback(Vector3.up * knockbackForce);
                }
            }

            if (expired == null)
                return;

            for (int i = 0; i < expired.Count; i++)
                _targets.Remove(expired[i]);
        }

        private bool IsValidTarget(HealthComponent health)
        {
            if (impactProfile != null)
                return HazardImpactUtility.IsValidTarget(health, impactProfile.targeting);

            return targeting switch
            {
                DamageTarget.PlayerOnly => health.faction == Faction.Player,
                DamageTarget.EnemyOnly => health.faction == Faction.Enemy,
                _ => true
            };
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box == null)
                return;

            Gizmos.color = new Color(1f, 0.15f, 0f, 0.18f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);

            Gizmos.color = new Color(1f, 0.15f, 0f, 0.7f);
            Gizmos.DrawWireCube(box.center, box.size);
        }
#endif
    }
}
