using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Animation;
using NeonBlack.Gameplay.Data.Interactions;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Interaction
{
    [AddComponentMenu("NeonBlack/Gameplay/Interaction/Collectibles/Actor Pickup Collector Feature 3D")]
    [AuthoringContract(
        StableId = "feature.actor.interaction.collectibles.3d",
        Category = "Inventory",
        Surface = AuthoringSurface.Profile,
        Summary = "Allows 3D actors to detect and collect pickups using spherical overlap detection.",
        RequiredFields = new[]
        {
            "ActorPickupCollector3D.pickupProfile"
        },
        RequiredComponentNames = new[] { "UnityEngine.Collider" },
        RequiredInterfaces = new[] { typeof(IActorInteractionHandler) },
        SetupSteps = new[]
        {
            "add ActorPickupCollector3D to the actor root",
            "assign PickupProfile"
        },
        SuccessChecks = new[] { "Walk a 3D actor into a pickup and verify it is collected." },
        Tags = new[] { "capability:Inventory", "lane:Interaction" },
        Selectable = false
    )]
    public class ActorPickupCollector3D : GameplayTickBehaviour, IActorInteractionHandler, IRuntimeValidationProvider
{
        private const int BufferSize = 16;
        [SerializeField] private PickupProfile pickupProfile;

        private readonly Collider[] _overlapBuffer = new Collider[BufferSize];
        private ActorInteractionContext _context;
        private IActorFeedbackPublisher _feedbackPublisher;
        protected override GameplayTickDomain TickDomain => GameplayTickDomain.Interaction;
        protected override bool UsesGameplayTick => true;

        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (pickupProfile == null)
            {
                yield return RuntimeValidationIssue.Required("Pickup Profile is required for 3D pickup collection.");
                yield break;
            }

            foreach (RuntimeValidationIssue issue in pickupProfile.GetRuntimeValidationIssues())
                yield return issue;

            if (pickupProfile.enableAutoCollect && pickupProfile.collectibleLayers3D.value == 0)
                yield return RuntimeValidationIssue.Required("3D Collectible Layers is set to Nothing while Auto Collect is enabled.");

            if (pickupProfile.enableAutoCollect && pickupProfile.overlapRadius3D <= 0f)
                yield return RuntimeValidationIssue.Required("3D Overlap Radius must be greater than zero when Auto Collect is enabled.");

            if (GetComponent<Collider>() == null)
                yield return RuntimeValidationIssue.Required("Collider is required on the actor for 3D pickup overlap checks.");
        }

        private void Awake()
        {
            Initialize();
        }

        protected override void OnGameplayTick(in GameplayTickContext context)
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
