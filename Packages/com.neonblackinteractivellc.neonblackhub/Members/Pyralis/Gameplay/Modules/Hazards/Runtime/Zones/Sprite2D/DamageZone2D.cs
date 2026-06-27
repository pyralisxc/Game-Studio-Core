using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Modules.Hazards;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using UnityEngine.Events;

namespace NeonBlack.Gameplay.Modules.Hazards.Zones
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat | AuthoringCapability.Puzzle, 
        Axioms = AuthoringWorldAxiom.Dimensions2D,
        Relevance = "2D trigger volume that repeatedly damages overlapping actors.",
        AssignmentFields = new[] { nameof(impactProfile), nameof(damagePerTick), nameof(tickInterval), nameof(knockbackForce), nameof(targeting) },
        Proof = "Walk an actor into the zone and verify it takes repeated damage.",
        NativeSetup = new[] { "Place on a 2D volume.", "Assign Collider2D (Awake forces Is Trigger).", "Assign Hazard Impact Profile or use fallback fields." },
        ExpertAdvice = "Use for floor spikes, poison gas, or area-of-effect hazards. Set Tick Interval to 0.5s for standard 'lava' feel. Ensure actors have a Rigidbody2D to trigger 2D physics events.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/combat/hazards",
        CapabilityPath = "Combat/Actions/Damage Zone2D"
    )]
[RequireComponent(typeof(Collider2D))]
    [AddComponentMenu("NeonBlack/Gameplay/Zones/Damage Zone 2D")]
    public partial class DamageZone2D : MonoBehaviour, IRuntimeValidationProvider
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

        private readonly DamageZoneTargetRuntime _targets = new DamageZoneTargetRuntime();

        private void Awake()
        {
            Collider2D collider = GetComponent<Collider2D>();
            collider.isTrigger = true;
            impactProfile?.Sanitize();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            IActorHealthState health = other.GetComponentInParent<IActorHealthState>();
            if (health == null || !IsValidTarget(health) || !_targets.AddTarget(health))
                return;

            OnTargetEntered?.Invoke(((Component)health).gameObject);
        }

        private void OnTriggerExit2D(Collider2D other)
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
            return impactProfile != null
                ? HazardImpactUtility.IsValidTarget(health, impactProfile.targeting)
                : HazardImpactUtility.IsValidTarget(health, targeting);
        }
    }
}
