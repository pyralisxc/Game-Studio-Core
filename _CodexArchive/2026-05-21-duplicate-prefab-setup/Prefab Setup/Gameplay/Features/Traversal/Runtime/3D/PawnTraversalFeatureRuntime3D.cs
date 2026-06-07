using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Features.Composition;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Traversal
{
    [AddComponentMenu("NeonBlack/Gameplay/Traversal/Pawn Traversal Feature Runtime 3D")]
    [RequireComponent(typeof(Pawn3DTraversalComponent))]
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
            traversalProfile = initializationContext.GetProfile<PawnTraversalProfile>(definition != null ? definition.profileAsset : null);
            if (traversalProfile != null && _traversal != null)
            {
                traversalProfile.Sanitize();
                _traversal.ApplyTraversalProfile(
                    new PawnProfileApplicationContext(
                        initializationContext.ActorObject,
                        initializationContext.PawnDefinition,
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
