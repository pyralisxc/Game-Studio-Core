using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Composition;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Pickups
{
    [AddComponentMenu("NeonBlack/Gameplay/Pickups/Actor Pickup Collector Feature 2D")]
    public class ActorPickupCollectorFeature2D : MonoBehaviour, IFeatureModuleRuntime, IActorInteractionHandler
    {
        private const int BufferSize = 16;
        [SerializeField] private PickupFeatureProfile pickupProfile;

        private readonly Collider2D[] _overlapBuffer = new Collider2D[BufferSize];

        private ActorFeatureContext _context;
        private IActorFeedbackPublisher _feedbackPublisher;
        public string ModuleId => "actor.pickups.2d";

        private void Update()
        {
            if (_context == null || pickupProfile == null || !pickupProfile.enableAutoCollect)
                return;

            TryCollectOverlappingPickup();
        }

        public void InitializeFeature(FeatureRuntimeInitializationContext initializationContext)
        {
            ActorFeatureContext context = initializationContext != null ? initializationContext.ActorContext : null;
            FeatureModuleDefinition definition = initializationContext != null ? initializationContext.Definition : null;
            _context = context;
            pickupProfile = initializationContext.GetProfile<PickupFeatureProfile>(definition != null ? definition.profileAsset : null);
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

            int hitCount = Physics2D.OverlapCircleNonAlloc(
                transform.position,
                pickupProfile.interactionRadius,
                _overlapBuffer,
                pickupProfile.collectibleLayers);

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
