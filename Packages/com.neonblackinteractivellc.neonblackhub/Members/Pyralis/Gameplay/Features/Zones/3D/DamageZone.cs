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
    [AuthoringContract(
        Capability = AuthoringCapability.Combat | AuthoringCapability.Puzzle,
        Axioms = AuthoringWorldAxiom.Dimensions3D,
        Relevance = "3D trigger volume that repeatedly damages overlapping actors.",
        NativeSetup = new[] 
        { 
            "Place on a 3D volume.",
            "Assign BoxCollider (Awake forces Is Trigger).",
            "Assign Hazard Impact Profile for shared data, or use fallback fields."
        },
        AssignmentFields = new[] { nameof(impactProfile), nameof(damagePerTick), nameof(tickInterval), nameof(knockbackForce) },
        FirstProof = "Walk an actor into the zone and verify it takes repeated damage.",
        ExpertAdvice = "Do not set Tick Interval too low. Ensure target actors have a HealthComponent. Use Hazard Impact Profile if you want this hazard to behave identically to others.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/combat/hazards"
    )]
    [RequireComponent(typeof(BoxCollider))]
    public class DamageZone : MonoBehaviour, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box == null)
                yield return "BoxCollider is required for 3D trigger damage.";
            else if (!box.isTrigger)
                yield return "BoxCollider is not set to Is Trigger. Awake will force it on.";

            if (impactProfile == null && damagePerTick <= 0f)
                yield return "Fallback Damage Per Tick must be greater than zero when Impact Profile is empty.";

            if (tickInterval <= 0f)
                yield return "Tick Interval must be greater than zero.";
        }
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
            GetComponent<BoxCollider>().isTrigger = true;
            impactProfile?.Sanitize();
        }

        private void OnTriggerEnter(Collider other)
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

        private void OnTriggerExit(Collider other)
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

            float activeTickInterval = impactProfile != null ? impactProfile.tickInterval : tickInterval;

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

                state.timer = activeTickInterval;
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
