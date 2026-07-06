using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Animation;
using NeonBlack.Gameplay.Data.Interactions;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Interaction
{
    [AddComponentMenu("NeonBlack/Gameplay/Interaction/Collectibles/Actor Pickup Collector Feature 2D")]
    [AuthoringContract(
        StableId = "feature.actor.interaction.collectibles.2d",
        Category = "Inventory",
        Surface = AuthoringSurface.Profile,
        Summary = "Allows 2D actors to detect and collect pickups using 2D collider overlap detection.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/actor-pickups",
        RequiredFields = new[]
        {
            "ActorPickupCollector2D.pickupProfile"
        },
        RequiredComponentNames = new[] { "UnityEngine.Collider2D" },
        RequiredInterfaces = new[] { typeof(IActorInteractionHandler) },
        SetupSteps = new[]
        {
            "add ActorPickupCollector2D to the actor root",
            "assign PickupProfile"
        },
        SuccessChecks = new[] { "Walk a 2D actor into a pickup and verify it is collected." },
        Tags = new[] { "capability:Inventory", "lane:Interaction" },
        Selectable = false
    )]
    public class ActorPickupCollector2D : GameplayTickBehaviour, IActorInteractionHandler, IRuntimeValidationProvider
{
        private const int BufferSize = 16;
        [SerializeField] private PickupProfile pickupProfile;

        private readonly Collider2D[] _overlapBuffer = new Collider2D[BufferSize];

        private ActorInteractionContext _context;
        private IActorFeedbackPublisher _feedbackPublisher;
        protected override GameplayTickDomain TickDomain => GameplayTickDomain.Interaction;
        protected override bool UsesGameplayTick => true;

        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (pickupProfile == null)
            {
                yield return RuntimeValidationIssue.Required("Pickup Profile is required for 2D pickup collection.");
                yield break;
            }

            foreach (RuntimeValidationIssue issue in pickupProfile.GetRuntimeValidationIssues())
                yield return issue;

            if (pickupProfile.enableInteractionCollect && pickupProfile.interactionRadius <= 0f)
                yield return RuntimeValidationIssue.Required("Interaction Radius must be greater than zero when 2D Interaction Collect is enabled.");

            if (pickupProfile.enableAutoCollect && pickupProfile.collectibleLayers.value == 0)
                yield return RuntimeValidationIssue.Required("2D Collectible Layers is set to Nothing while Auto Collect is enabled.");

            if (GetComponent<Collider2D>() == null)
                yield return RuntimeValidationIssue.Required("Collider2D is required on the actor for 2D pickup overlap checks.");
        }

        private void Awake()
        {
            Initialize();
        }

        protected override void OnGameplayTick(in GameplayTickContext context)
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
