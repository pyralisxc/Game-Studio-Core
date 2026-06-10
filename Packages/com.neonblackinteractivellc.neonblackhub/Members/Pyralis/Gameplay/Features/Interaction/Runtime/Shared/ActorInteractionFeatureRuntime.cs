using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Features.Composition;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Interaction
{
    [AddComponentMenu("NeonBlack/Gameplay/Interaction/Actor Interaction Feature Runtime")]
    [AuthoringContract(
        Capability = AuthoringCapability.Puzzle | AuthoringCapability.Input,
        ModuleId = "actor.interaction",
        Lane = "Interaction",
        ProfileType = typeof(InteractionFeatureProfile),
        RequiredInterfaces = new[] { typeof(IFeatureModuleRuntime), typeof(IActorInteractionFeature) },
        RequiredInterfaceNames = new[] { "NeonBlack.Gameplay.Features.Characters.IActorInteractionInputReceiver2D" }, // Required for Sprite2D lane
        AssignmentFields = new[] { nameof(interactionProfile) },
        FirstProof = "Verify that TryHandleInteraction triggers one of the attached IActorInteractionHandlers.",
        NativeSetup = new[]
        {
            "create InteractionFeatureProfile",
            "create FeatureModuleDefinition",
            "assign runtime prefab with ActorInteractionFeatureRuntime",
            "assign profile asset",
            "add module to PawnDefinition.featureModules"
        },
        CustomizationMoments = new[]
        {
            "InteractionFeatureProfile.enableInteraction",
            "interactionCooldown",
            "triggerInteractAnimationWhenUnhandled"
        },
        ConsumedRoles = new[] { "Interact" }
    )]
    public class ActorInteractionFeatureRuntime : MonoBehaviour, IFeatureModuleRuntime, IActorInteractionFeature
{
        [SerializeField] private InteractionFeatureProfile interactionProfile;
        private ActorFeatureContext _context;
        private IActorInteractionHandler[] _handlers;
        private float _cooldownTimer;

        public string ModuleId => "actor.interaction";

        private void Update()
        {
            if (_cooldownTimer > 0f)
                _cooldownTimer -= Time.deltaTime;
        }

        public void InitializeFeature(FeatureRuntimeInitializationContext initializationContext)
        {
            ActorFeatureContext context = initializationContext != null ? initializationContext.ActorContext : null;
            FeatureModuleDefinition definition = initializationContext != null ? initializationContext.Definition : null;
            _context = context;
            interactionProfile = initializationContext != null
                ? initializationContext.GetProfile<InteractionFeatureProfile>(definition != null ? definition.profileAsset : null)
                : interactionProfile;
            interactionProfile?.Sanitize();
            _handlers = GetComponents<IActorInteractionHandler>();
        }

        public void ShutdownFeature()
        {
            _context = null;
            _handlers = null;
            _cooldownTimer = 0f;
        }

        public bool TryHandleInteraction()
        {
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
