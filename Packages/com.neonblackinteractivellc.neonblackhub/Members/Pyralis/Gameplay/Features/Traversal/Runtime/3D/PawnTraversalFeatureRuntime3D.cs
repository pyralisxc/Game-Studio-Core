using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Features.Traversal;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Traversal
{
    [AddComponentMenu("NeonBlack/Gameplay/Traversal/Pawn Traversal Feature Runtime 3D")]
    [RequireComponent(typeof(Pawn3DTraversalComponent))]
    [AuthoringContract(
        ModuleId = "actor.traversal.3d",
        Capability = AuthoringCapability.Traversal,
        Lane = "Traversal",
        Relevance = "Enables advanced 3D world navigation including ledge hanging, climbing, and shimmying.",
        ProfileType = typeof(PawnTraversalProfile),
        RequiredInterfaces = new[] { typeof(IFeatureModuleRuntime), typeof(IActorTraversalFeature) },
        RequiredInterfaceNames = new[] { "NeonBlack.Gameplay.Features.Characters.IActorInteractionHandler" },
        RequiredComponentNames = new[] { "NeonBlack.Gameplay.Features.Characters.Motor3D", "NeonBlack.Gameplay.Features.Traversal.Pawn3DTraversalComponent" },
        SupportedLanes = new[] { ActorPresentationMode.Billboard2_5D, ActorPresentationMode.ThirdPerson3D },
        UnsupportedLanes = new[] { ActorPresentationMode.Sprite2D },
        NativeSetup = new[]
        {
            "Create a PawnTraversalProfile asset for movement tuning.",
            "Add PawnTraversalFeatureRuntime3D to the Pawn's runtime prefab.",
            "Ensure Pawn3DTraversalComponent is also present for physics handling.",
            "Register the module in the PawnDefinition feature list."
        },
        ExpertAdvice = "Adjust ledge detection offsets in the profile to match your character's physical height. Use the 'ProbeTraversal' method to check for climbable surfaces during movement states.",
        FirstProof = "Character successfully grabs a ledge marked with an IClimbZone when jumping toward it.",
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
                    new PawnProfileApplicationContext(
                        initializationContext != null ? initializationContext.ActorObject : null,
                        initializationContext != null ? initializationContext.PawnDefinition : null,
                        context != null ? context.Participant : null),
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
