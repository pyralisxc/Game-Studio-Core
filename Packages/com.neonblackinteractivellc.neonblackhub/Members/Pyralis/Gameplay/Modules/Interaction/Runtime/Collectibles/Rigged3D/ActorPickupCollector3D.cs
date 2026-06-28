using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Animation;
using NeonBlack.Gameplay.Data.Interactions;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Interaction
{
    [AddComponentMenu("NeonBlack/Gameplay/Interaction/Collectibles/Actor Pickup Collector Feature 3D")]
    [AuthoringContract(
        ModuleId = "actor.interaction.collectibles.3d",
        Capability = AuthoringCapability.Inventory,
        Relevance = "Allows 3D actors to detect and collect pickups using spherical overlap detection.",
        Lane = "Interaction",
        ProfileType = typeof(PickupProfile),
        RequiredInterfaces = new[] { typeof(IActorInteractionHandler) },
        RequiredComponentNames = new[] { "UnityEngine.Collider" }, // Or CharacterController
        SupportedLanes = new[] { ActorPresentationMode.Billboard2_5D, ActorPresentationMode.ThirdPerson3D },
        UnsupportedLanes = new[] { ActorPresentationMode.Sprite2D },
        NativeSetup = new[]
        {
            "add ActorPickupCollector3D to the actor root",
            "assign PickupProfile"
        },
        Proof = "Walk a 3D actor into a pickup and verify it is collected.",
        AssignmentFields = new[]
        {
            "ActorPickupCollector3D.pickupProfile"
        },
        CustomizationMoments = new[]
        {
            "PickupProfile.enableAutoCollect",
            "PickupProfile.enableInteractionCollect",
            "PickupProfile.collectibleLayers3D"
        }
    )]
    public class ActorPickupCollector3D : MonoBehaviour, IActorInteractionHandler
{
        private const int BufferSize = 16;
        [SerializeField] private PickupProfile pickupProfile;

        private readonly Collider[] _overlapBuffer = new Collider[BufferSize];
        private ActorInteractionContext _context;
        private IActorFeedbackPublisher _feedbackPublisher;

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            if (_context == null || pickupProfile == null || !pickupProfile.enableAutoCollect)
                return;

            TryCollectNearbyPickup();
        }

        private void Initialize()
        {
            _context = ActorInteractionContext.FromActor(gameObject);
            pickupProfile?.Sanitize();
            _feedbackPublisher = _context != null && _context.ActorObject != null
                ? _context.ActorObject.GetComponent<IActorFeedbackPublisher>()
                : null;
        }

        private void OnDestroy()
        {
            _context = null;
            _feedbackPublisher = null;
        }

        public bool TryHandleInteraction(ActorInteractionContext context)
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
