using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Animation;
using NeonBlack.Gameplay.Data.Interactions;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Interaction
{
    [AddComponentMenu("NeonBlack/Gameplay/Interaction/Collectibles/Actor Pickup Collector Feature 2D")]
    [AuthoringContract(
        ModuleId = "actor.interaction.collectibles.2d",
        Capability = AuthoringCapability.Inventory,
        Relevance = "Allows 2D actors to detect and collect pickups using 2D collider overlap detection.",
        ExpertAdvice = "Optimized for Sprite2D pawns. Uses 2D collider overlap for zero-effort collection setup.",
        Lane = "Interaction",
        ProfileType = typeof(PickupProfile),
        RequiredInterfaces = new[] { typeof(IActorInteractionHandler) },
        RequiredComponentNames = new[] { "UnityEngine.Collider2D" },
        SupportedLanes = new[] { ActorPresentationMode.Sprite2D },
        UnsupportedLanes = new[] { ActorPresentationMode.Billboard2_5D, ActorPresentationMode.ThirdPerson3D },
        ConsumedRoles = new[] { "Interact" },
        NativeSetup = new[]
        {
            "add ActorPickupCollector2D to the actor root",
            "assign PickupProfile"
        },
        Proof = "Walk a 2D actor into a pickup and verify it is collected.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/actor-pickups",
        AssignmentFields = new[]
        {
            "ActorPickupCollector2D.pickupProfile"
        },
        CustomizationMoments = new[]
        {
            "PickupProfile.enableAutoCollect",
            "PickupProfile.enableInteractionCollect",
            "PickupProfile.collectibleLayers"
        }
    )]
    public class ActorPickupCollector2D : MonoBehaviour, IActorInteractionHandler
{
        private const int BufferSize = 16;
        [SerializeField] private PickupProfile pickupProfile;

        private readonly Collider2D[] _overlapBuffer = new Collider2D[BufferSize];

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

            TryCollectOverlappingPickup();
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
            if (_context == null || pickupProfile == null || !pickupProfile.enableInteractionCollect)
                return false;

            IPickupCollectible collectible = FindNearestPickupInRange();
            if (collectible == null)
                return false;

            collectible.CollectBy(_context.ActorObject);
            _feedbackPublisher?.PublishScore(collectible.FeedbackScoreValue);
            return true;
        }

        private bool TryCollectOverlappingPickup()
        {
            Collider2D collectorCollider = _context != null && _context.ActorObject != null
                ? _context.ActorObject.GetComponent<Collider2D>()
                : null;
            if (collectorCollider == null)
                return false;

            ContactFilter2D filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = pickupProfile.collectibleLayers,
                useTriggers = true
            };

            int hitCount = collectorCollider.Overlap(filter, _overlapBuffer);
            for (int i = 0; i < hitCount; i++)
            {
                IPickupCollectible collectible = _overlapBuffer[i] != null
                    ? _overlapBuffer[i].GetComponent<IPickupCollectible>()
                    : null;
                if (collectible == null)
                    continue;

                collectible.CollectBy(_context.ActorObject);
                _feedbackPublisher?.PublishScore(collectible.FeedbackScoreValue);
                return true;
            }

            return false;
        }

        private IPickupCollectible FindNearestPickupInRange()
        {
            if (pickupProfile.interactionRadius <= 0f)
                return null;

            ContactFilter2D filter = new ContactFilter2D { useTriggers = true };
            filter.SetLayerMask(pickupProfile.collectibleLayers);
            int hitCount = Physics2D.OverlapCircle(
                transform.position,
                pickupProfile.interactionRadius,
                filter,
                _overlapBuffer);

            IPickupCollectible bestCollectible = null;
            float bestDistanceSq = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = _overlapBuffer[i];
                IPickupCollectible collectible = hit != null ? hit.GetComponent<IPickupCollectible>() : null;
                if (collectible == null)
                    continue;

                if (!pickupProfile.preferNearestPickup)
                    return collectible;

                float distanceSq = (((Component)collectible).transform.position - transform.position).sqrMagnitude;
                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                bestCollectible = collectible;
            }

            return bestCollectible;
        }
    }
}
