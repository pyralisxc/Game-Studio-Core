using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Modules.Hazards;
using UnityEngine;
using UnityEngine.Events;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Hazards.Zones
{
    /// <summary>
    /// Trigger volume that repeatedly applies an authored hazard impact profile
    /// to overlapping actors.
    /// </summary>
    [AuthoringContract(
        Category = "Combat, Puzzle",
        CapabilityPath = "Combat/Actions/Damage Zone",
        Surface = AuthoringSurface.Goal,
        Summary = "3D trigger volume that repeatedly damages overlapping actors.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/combat/hazards",
        RequiredFields = new[] { nameof(impactProfile) },
        SetupSteps = new[] 
        { 
            "Place on a 3D volume.",
            "Assign BoxCollider (Awake forces Is Trigger).",
            "Assign Hazard Impact Profile for shared damage, knockback, targeting, and status effects."
        },
        SuccessChecks = new[] { "Walk an actor into the zone and verify it takes repeated damage." },
        Tags = new[] { "capability:Combat", "capability:Puzzle", "axiom:Dimensions3D" }
    )]
    [RequireComponent(typeof(BoxCollider))]
    public partial class DamageZone : GameplayTickBehaviour, IRuntimeValidationProvider
    {
        [Header("Profile")]
        [SerializeField] private HazardImpactProfile impactProfile;

        [Header("Events")]
        public UnityEvent<GameObject> OnTargetEntered;
        public UnityEvent<GameObject> OnTargetExited;

        private readonly DamageZoneTargetRuntime _targets = new DamageZoneTargetRuntime();

        protected override GameplayTickDomain TickDomain => GameplayTickDomain.Hazards;
        protected override bool UsesGameplayTick => true;

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

        protected override void OnGameplayTick(in GameplayTickContext context)
        {
            if (!_targets.HasTargets)
                return;

            if (impactProfile == null)
                return;

            _targets.Tick(gameObject, transform, impactProfile, context.DeltaTime);
        }

        private bool IsValidTarget(IActorHealthState health)
        {
            return impactProfile != null && HazardImpactUtility.IsValidTarget(health, impactProfile.targeting);
        }

    }
}
