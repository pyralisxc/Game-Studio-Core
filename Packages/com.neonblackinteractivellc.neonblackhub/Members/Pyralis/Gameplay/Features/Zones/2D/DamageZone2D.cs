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
        Capability = AuthoringCapability.Combat | AuthoringCapability.Puzzle, 
        Axioms = AuthoringWorldAxiom.Dimensions2D,
        Relevance = "2D trigger volume that repeatedly damages overlapping actors.",
        AssignmentFields = new[] { nameof(impactProfile), nameof(damagePerTick), nameof(tickInterval), nameof(knockbackForce), nameof(targeting) },
        FirstProof = "Walk an actor into the zone and verify it takes repeated damage.",
        NativeSetup = new[] { "Place on a 2D volume.", "Assign Collider2D (Awake forces Is Trigger).", "Assign Hazard Impact Profile or use fallback fields." },
        ExpertAdvice = "Use for floor spikes, poison gas, or area-of-effect hazards. Set Tick Interval to 0.5s for standard 'lava' feel. Ensure actors have a Rigidbody2D to trigger 2D physics events.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/combat/hazards"
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
            public IActorStatusEffectReceiver statusReceiver;
            public KnockbackReceiver knockback;
            public float timer;
        }

        private readonly List<TargetState> _targets = new List<TargetState>(8);
        private readonly HashSet<HealthComponent> _targetLookup = new HashSet<HealthComponent>();

        private void Awake()
        {
            Collider2D collider = GetComponent<Collider2D>();
            collider.isTrigger = true;
            impactProfile?.Sanitize();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            HealthComponent health = other.GetComponentInParent<HealthComponent>();
            if (health == null || _targetLookup.Contains(health) || !IsValidTarget(health))
                return;

            _targetLookup.Add(health);
            
            _targets.Add(new TargetState 
            { 
                health = health, 
                statusReceiver = health.GetComponent<IActorStatusEffectReceiver>() ?? health.GetComponentInParent<IActorStatusEffectReceiver>(),
                knockback = health.GetComponent<KnockbackReceiver>() ?? health.GetComponentInParent<KnockbackReceiver>(),
                timer = 0f 
            });
            
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
                {
                    ApplyImpactRefined(state, impactProfile);
                    continue;
                }

                health.TakeDamage(damagePerTick, health.transform.position, gameObject);
                if (knockbackForce > 0f && state.knockback != null)
                {
                    state.knockback.ApplyKnockback(Vector3.up * knockbackForce);
                }
            }
        }

        private void ApplyImpactRefined(TargetState state, HazardImpactProfile profile)
        {
            if (profile.damagePerTick > 0f)
                state.health.TakeDamage(profile.damagePerTick, state.health.transform.position, gameObject);

            if (profile.knockbackForce > 0f && state.knockback != null)
            {
                Vector3 delta = state.health.transform.position - transform.position;
                delta.z = 0f;
                Vector3 dir = delta.sqrMagnitude > 0.0001f ? delta.normalized : (profile.useUpwardKnockback ? Vector3.up : Vector3.right);
                state.knockback.ApplyKnockback(dir * profile.knockbackForce);
            }

            if (state.statusReceiver != null && profile.statusEffects != null)
            {
                for (int i = 0; i < profile.statusEffects.Length; i++)
                {
                    if (profile.statusEffects[i] != null)
                        state.statusReceiver.ApplyStatusEffect(profile.statusEffects[i], gameObject);
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
