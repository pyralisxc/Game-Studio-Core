using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Data.Interactions;
using NeonBlack.Gameplay.Core.Types.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Interaction
{
    [AddComponentMenu("NeonBlack/Gameplay/Interaction/Actor Interaction Component")]
    [AuthoringContract(
        Capability = AuthoringCapability.Puzzle | AuthoringCapability.Input,
        ModuleId = "actor.interaction",
        Relevance = "Receives interaction input and delegates it to IActorInteractionHandler sibling components.",
        Lane = "Interaction",
        ProfileType = typeof(InteractionProfile),
        RequiredInterfaces = new[] { typeof(IActorInteractionRequestReceiver) },
        RequiredInterfaceNames = new[] { "NeonBlack.Gameplay.Core.Contracts.IActorInteractionInputReceiver2D" }, // Required for Sprite2D lane
        AssignmentFields = new[] { nameof(interactionProfile) },
        Proof = "Verify that TryHandleInteraction triggers one of the attached IActorInteractionHandlers.",
        NativeSetup = new[]
        {
            "Create InteractionProfile.",
            "Add ActorInteractionComponent to the actor root.",
            "Assign InteractionProfile.",
            "Add IActorInteractionHandler components for interactable behaviors."
        },
        ExpertAdvice = "Sprite2D actors usually also need ActorInteractionInputBridge2D on the actor root so pawn input can reach this feature.",
        CustomizationMoments = new[]
        {
            "InteractionProfile.enableInteraction",
            "interactionCooldown",
            "triggerInteractAnimationWhenUnhandled"
        },
        ConsumedRoles = new[] { "Interact" },
        CapabilityPath = "Interaction/Runtime/Actor Interaction Component"
    )]
    public class ActorInteractionComponent : MonoBehaviour, IActorInteractionRequestReceiver
    {
        [SerializeField] private InteractionProfile interactionProfile;
        private ActorInteractionContext _context;
        private IActorInteractionHandler[] _handlers;
        private float _cooldownTimer;

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            if (_cooldownTimer > 0f)
                _cooldownTimer -= Time.deltaTime;
        }

        private void Initialize()
        {
            _context = ActorInteractionContext.FromActor(gameObject);
            interactionProfile?.Sanitize();
            _handlers = GetComponents<IActorInteractionHandler>();
        }

        private void OnDestroy()
        {
            _context = null;
            _handlers = null;
            _cooldownTimer = 0f;
        }

        public bool TryHandleInteraction()
        {
            _context ??= ActorInteractionContext.FromActor(gameObject);

            if (_context == null || interactionProfile == null || !interactionProfile.enableInteraction || _cooldownTimer > 0f)
                return false;

            if (_handlers != null)
            {
                for (int i = 0; i < _handlers.Length; i++)
                {
                    if (_handlers[i] == null || ReferenceEquals(_handlers[i], this))
                        continue;

                    if (_handlers[i].TryHandleInteraction(_context))
                    {
                        StartCooldown();
                        return true;
                    }
                }
            }

            if (interactionProfile.triggerInteractAnimationWhenUnhandled)
            {
                _context.Animation?.TriggerSignal(ActorAnimationSignal.Interact);
                StartCooldown();
            }

            return interactionProfile.triggerInteractAnimationWhenUnhandled;
        }

        private void StartCooldown()
        {
            _cooldownTimer = Mathf.Max(interactionProfile != null ? interactionProfile.interactionCooldown : 0f, 0f);
        }
    }
}
