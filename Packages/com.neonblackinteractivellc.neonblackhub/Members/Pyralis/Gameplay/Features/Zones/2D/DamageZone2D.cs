using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Hazards;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using UnityEngine.Events;

namespace NeonBlack.Gameplay.Features.Zones
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat, 
        Relevance = "Inspector Add Component path for a 2D hazard or damage trigger.",
        AssignmentFields = new[] { "impactProfile", "damagePerTick", "tickInterval" },
        FirstProof = "Place the zone on a pawn and verify it takes damage over time.",
        NativeSetup = new[] { "Add Component", "Configure Collider2D as Trigger" }
    )]
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

        private struct TargetState
        {
            public HealthComponent health;
            public float timer;
        }

        private readonly List<TargetState> _targets = new List<TargetState>(8);
        private readonly HashSet<HealthComponent> _targetLookup = new HashSet<HealthComponent>();

        private void Awake()
        {
            Collider2D collider = GetComponent<Collider2D>();
            collider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            HealthComponent health = other.GetComponentInParent<HealthComponent>();
            if (health == null || _targetLookup.Contains(health) || !IsValidTarget(health))
                return;

            _targetLookup.Add(health);
            _targets.Add(new TargetState { health = health, timer = 0f });
            OnTargetEntered?.Invoke(health.gameObject);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            HealthComponent health = other.GetComponentInParent<HealthComponent>();
            if (health == null || !_targetLookup.Remove(health))
                return;

            for (int i = 0; i < _targets.Count; i++)
            {
                if (_targets[i].health == health)
                {
                    _targets.RemoveAt(i);
                    break;
                }
            }
            OnTargetExited?.Invoke(health.gameObject);
        }

        private void Update()
        {
            if (_targets.Count == 0)
                return;

            float interval = impactProfile != null ? impactProfile.tickInterval : tickInterval;

            for (int i = _targets.Count - 1; i >= 0; i--)
            {
                TargetState state = _targets[i];
                HealthComponent health = state.health;

                if (health == null || health.IsDead)
                {
                    _targetLookup.Remove(health);
                    _targets.RemoveAt(i);
                    continue;
                }

                state.timer -= Time.deltaTime;
                if (state.timer > 0f)
                {
                    _targets[i] = state;
                    continue;
                }

                state.timer = interval;
                _targets[i] = state;

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
        }

        private bool IsValidTarget(HealthComponent health)
        {
            return impactProfile != null
                ? HazardImpactUtility.IsValidTarget(health, impactProfile.targeting)
                : HazardImpactUtility.IsValidTarget(health, targeting);
        }
    }
}
