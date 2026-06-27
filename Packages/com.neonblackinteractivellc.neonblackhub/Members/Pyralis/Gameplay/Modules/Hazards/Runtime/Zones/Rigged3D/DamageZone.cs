using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Modules.Hazards;
using UnityEngine;
using UnityEngine.Events;

namespace NeonBlack.Gameplay.Modules.Hazards.Zones
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
        Proof = "Walk an actor into the zone and verify it takes repeated damage.",
        ExpertAdvice = "Do not set Tick Interval too low. Ensure target actors expose an actor health contract. Use Hazard Impact Profile if you want this hazard to behave identically to others.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/combat/hazards",
        CapabilityPath = "Combat/Actions/Damage Zone"
    )]
    [RequireComponent(typeof(BoxCollider))]
    public partial class DamageZone : MonoBehaviour, IRuntimeValidationProvider
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

        private readonly DamageZoneTargetRuntime _targets = new DamageZoneTargetRuntime();

        private void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;
            impactProfile?.Sanitize();
        }

        private void OnTriggerEnter(Collider other)
        {
            IActorHealthState health = other.GetComponentInParent<IActorHealthState>();
            if (health == null || !IsValidTarget(health) || !_targets.AddTarget(health))
                return;

            OnTargetEntered?.Invoke(((Component)health).gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            IActorHealthState health = other.GetComponentInParent<IActorHealthState>();
            if (!_targets.RemoveTarget(health))
                return;

            OnTargetExited?.Invoke(((Component)health).gameObject);
        }

        private void Update()
        {
            if (!_targets.HasTargets)
                return;

            _targets.Tick(gameObject, transform, impactProfile, damagePerTick, tickInterval, knockbackForce);
        }

        private bool IsValidTarget(IActorHealthState health)
        {
            if (impactProfile != null)
                return HazardImpactUtility.IsValidTarget(health, impactProfile.targeting);

            return targeting switch
            {
                DamageTarget.PlayerOnly => health.Faction == Faction.Player,
                DamageTarget.EnemyOnly => health.Faction == Faction.Enemy,
                _ => true
            };
        }

    }
}
