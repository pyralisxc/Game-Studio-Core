using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Animation;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Modules.Character;
using NeonBlack.Gameplay.Modules.Actor.Composition;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Traversal
{
    [AddComponentMenu("NeonBlack/Gameplay/Traversal/Pawn Traversal Feature Runtime 3D")]
    [RequireComponent(typeof(Pawn3DTraversalComponent))]
    [AuthoringContract(
        ModuleId = "actor.traversal.3d",
        Capability = AuthoringCapability.Traversal,
        Lane = "Traversal",
        Relevance = "Optional 3D traversal feature runtime for advanced ledge hanging, climbing, and shimmying.",
        ProfileType = typeof(PawnTraversalProfile),
        RequiredInterfaces = new[] { typeof(IFeatureModuleRuntime), typeof(IActorTraversalFeature) },
        RequiredInterfaceNames = new[] { "NeonBlack.Gameplay.Modules.Actor.Composition.IActorInteractionHandler" },
        RequiredComponentNames = new[] { "NeonBlack.Gameplay.Modules.Character.Motor3D", "NeonBlack.Gameplay.Modules.Traversal.Pawn3DTraversalComponent" },
        SupportedLanes = new[] { ActorPresentationMode.Billboard2_5D, ActorPresentationMode.ThirdPerson3D },
        UnsupportedLanes = new[] { ActorPresentationMode.Sprite2D },
        NativeSetup = new[]
        {
            "Create a PawnTraversalProfile asset for traversal tuning.",
            "Create or assign a FeatureModuleDefinition with module id actor.traversal.3d.",
            "Assign a feature runtime prefab that contains PawnTraversalFeatureRuntime3D.",
            "Keep Motor3D, Pawn3DMovementComponent, and Pawn3DTraversalComponent on the pawn root as explicit sibling components.",
            "Register the module in the PawnDefinition featureModules list."
        },
        ExpertAdvice = "Adjust ledge detection offsets in the profile to match your character's physical height. ActorFeatureHost installs this optional module around the explicit traversal sibling; it should not be the only owner of base 3D movement.",
        Proof = "Character successfully grabs a ledge marked with an IClimbZone when jumping toward it.",
        AssignmentFields = new[] { nameof(traversalProfile) },
        DocumentationURL = "https://docs.neonblack.com/pyralis/traversal/3d",
        CustomizationMoments = new[]
        {
            "PawnTraversalProfile.maxSlopeAngle",
            "PawnTraversalProfile.jumpImpulse",
            "Pawn3DTraversalComponent.ledgeDetectionOffset"
        }
    )]
    public class PawnTraversalFeatureRuntime3D : MonoBehaviour, IFeatureModuleRuntime, IActorTraversalFeature, IActorInteractionHandler
{
        [SerializeField] private PawnTraversalProfile traversalProfile;
        private ActorFeatureContext _context;
        private Pawn3DTraversalComponent _traversal;

        public string ModuleId => "actor.traversal.3d";
        public float ShimmyVelocityX => _traversal != null ? _traversal.ShimmyVelocityX : 0f;

        private void Awake()
        {
            _traversal = GetComponent<Pawn3DTraversalComponent>();
        }

        public void InitializeFeature(FeatureRuntimeInitializationContext initializationContext)
        {
            ActorFeatureContext context = initializationContext != null ? initializationContext.ActorContext : null;
            FeatureModuleDefinition definition = initializationContext != null ? initializationContext.Definition : null;
            _context = context;
            _traversal ??= GetComponent<Pawn3DTraversalComponent>();
            traversalProfile = initializationContext != null
                ? initializationContext.GetProfile<PawnTraversalProfile>(definition != null ? definition.profileAsset : null)
                : null;
            if (traversalProfile != null && _traversal != null)
            {
                traversalProfile.Sanitize();
                _traversal.ApplyTraversalProfile(
                    initializationContext.BuildPawnProfileApplicationContext(),
                    traversalProfile);
            }
        }

        public void ShutdownFeature()
        {
            _context = null;
        }

        public void ProbeTraversal() => _traversal?.ProbeLedge();
        public bool HandleHangFrame(FrameInput frameInput) => _traversal != null && _traversal.HandleHangFrame(frameInput);
        public void TriggerClimbUp() => _traversal?.TriggerClimbUp();
        public void TryLedgeGrab(IClimbZone zone, float maxVelocityY = 0f) => _traversal?.TryLedgeGrab(zone, maxVelocityY);
        public void SetClimbZone(IClimbZone zone) => _traversal?.SetClimbZone(zone);
        public void ClearClimbZone() => _traversal?.ClearClimbZone();

        public bool TryHandleInteraction(ActorFeatureContext context)
        {
            return _traversal != null && _traversal.TryHandleTraversalInteraction();
        }
    }
}
