using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Composition;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Pickups
{
    [AddComponentMenu("NeonBlack/Gameplay/Pickups/Actor Pickup Collector Feature 3D")]
    [AuthoringContract(
        ModuleId = "actor.pickups.3d",
        Capability = AuthoringCapability.Inventory,
        Relevance = "Allows 3D actors to detect and collect pickups using spherical overlap detection.",
        Lane = "Pickups",
        ProfileType = typeof(PickupFeatureProfile),
        RequiredInterfaces = new[] { typeof(IFeatureModuleRuntime), typeof(IActorInteractionHandler) },
        RequiredComponentNames = new[] { "UnityEngine.Collider" }, // Or CharacterController
        SupportedLanes = new[] { ActorPresentationMode.Billboard2_5D, ActorPresentationMode.Rigged3D },
        UnsupportedLanes = new[] { ActorPresentationMode.Sprite2D },
        NativeSetup = new[]
        {
            "create PickupFeatureProfile",
            "create FeatureModuleDefinition",
            "assign runtime prefab with ActorPickupCollectorFeature3D",
            "assign profile asset",
            "add module to PawnDefinition.featureModules"
        },
        FirstProof = "Walk a 3D actor into a pickup and verify it is collected.",
        AssignmentFields = new[]
        {
            "FeatureModuleDefinition.moduleId",
            "FeatureModuleDefinition.runtimePrefab",
            "FeatureModuleDefinition.profileAsset"
        },
        CustomizationMoments = new[]
        {
            "PickupFeatureProfile.enableAutoCollect",
            "PickupFeatureProfile.enableInteractionCollect",
            "PickupFeatureProfile.collectibleLayers3D"
        }
    )]
    public class ActorPickupCollectorFeature3D : MonoBehaviour, IFeatureModuleRuntime, IActorInteractionHandler
{
        private const int BufferSize = 16;
        [SerializeField] private PickupFeatureProfile pickupProfile;

        private readonly Collider[] _overlapBuffer = new Collider[BufferSize];
        private ActorFeatureContext _context;
        private IActorFeedbackPublisher _feedbackPublisher;

        public string ModuleId => "actor.pickups.3d";

        private void Update()
        {
            if (_context == null || pickupProfile == null || !pickupProfile.enableAutoCollect)
                return;

            TryCollectNearbyPickup();
        }

        public void InitializeFeature(FeatureRuntimeInitializationContext initializationContext)
        {
            ActorFeatureContext context = initializationContext != null ? initializationContext.ActorContext : null;
            FeatureModuleDefinition definition = initializationContext != null ? initializationContext.Definition : null;
            _context = context;
            pickupProfile = initializationContext != null
                ? initializationContext.GetProfile<PickupFeatureProfile>(definition != null ? definition.profileAsset : null)
                : pickupProfile;
            pickupProfile?.Sanitize();
            _feedbackPublisher = _context != null && _context.ActorObject != null
                ? _context.ActorObject.GetComponent<IActorFeedbackPublisher>()
                : null;
        }

        public void ShutdownFeature()
        {
            _context = null;
            _feedbackPublisher = null;
        }

        public bool TryHandleInteraction(ActorFeatureContext context)
        {
            return pickupProfile != null
                && pickupProfile.enableInteractionCollect
                && TryCollectNearbyPickup(onlyNearest: true);
        }

        private bool TryCollectNearbyPickup(bool onlyNearest = false)
        {
            if (_context == null || pickupProfile == null)
                return false;

            int hitCount = Physics.OverlapSphereNonAlloc(
                _context.ActorTransform.position,
                pickupProfile.overlapRadius3D,
                _overlapBuffer,
                pickupProfile.collectibleLayers3D,
                QueryTriggerInteraction.Collide);

            IPickupCollectible bestCollectible = null;
            float bestDistanceSq = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                IPickupCollectible collectible = _overlapBuffer[i] != null
                    ? _overlapBuffer[i].GetComponent<IPickupCollectible>()
                    : null;
                if (collectible == null)
                    continue;

                if (!onlyNearest || !pickupProfile.preferNearestPickup)
                {
                    collectible.CollectBy(_context.ActorObject);
                    _feedbackPublisher?.PublishScore(collectible.FeedbackScoreValue);
                    return true;
                }

                float distanceSq = (_overlapBuffer[i].transform.position - _context.ActorTransform.position).sqrMagnitude;
                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                bestCollectible = collectible;
            }

            if (bestCollectible == null)
                return false;

            bestCollectible.CollectBy(_context.ActorObject);
            _feedbackPublisher?.PublishScore(bestCollectible.FeedbackScoreValue);
            return true;
        }
    }
}
