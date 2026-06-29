using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Modules.Hazards;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using UnityEngine.Events;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Hazards.Zones
{
    [AuthoringContract(
        Category = "Combat, Puzzle",
        CapabilityPath = "Combat/Actions/Damage Zone2D",
        Surface = AuthoringSurface.Goal,
        Summary = "2D trigger volume that repeatedly damages overlapping actors.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/combat/hazards",
        RequiredFields = new[] { nameof(impactProfile) },
        SetupSteps = new[] { "Place on a 2D volume.", "Assign Collider2D (Awake forces Is Trigger).", "Assign Hazard Impact Profile for shared damage, knockback, targeting, and status effects." },
        SuccessChecks = new[] { "Walk an actor into the zone and verify it takes repeated damage." },
        Tags = new[] { "capability:Combat", "capability:Puzzle", "axiom:Dimensions2D" }
    )]
    [RequireComponent(typeof(Collider2D))]
    [AddComponentMenu("NeonBlack/Gameplay/Zones/Damage Zone 2D")]
    public partial class DamageZone2D : GameplayTickBehaviour, IRuntimeValidationProvider
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
