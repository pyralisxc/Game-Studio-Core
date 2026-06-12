using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Composition;

namespace NeonBlack.Gameplay.Features.Interaction
{
    [AuthoringContract(
        Capability = AuthoringCapability.Puzzle | AuthoringCapability.Input,
        ModuleId = "actor.interaction",
        ProfileType = typeof(InteractionFeatureProfile),
        RequiredInterfaces = new[] { typeof(IFeatureModuleRuntime), typeof(IActorInteractionFeature) },
        ConsumedRoles = new[] { "Interact" },
        AssignmentFields = new[] { "InteractionFeatureProfile.enableInteraction", "InteractionFeatureProfile.interactionCooldown" },
        FirstProof = "Press the interact button and verify the interaction handler is triggered.",
        FirstProofTargetId = "proof.action-selection",
        NativeSetup = new[]
        {
            "create InteractionFeatureProfile",
            "create FeatureModuleDefinition",
            "assign runtime prefab with interaction feature runtime",
            "assign profile asset",
            "add module to PawnDefinition.featureModules",
            "bind Interact in InputProfile"
        },
        CustomizationMoments = new[]
        {
            "InteractionFeatureProfile.enableInteraction",
            "InteractionFeatureProfile.interactionCooldown",
            "InputProfile Interact binding"
        }
    )]
    public interface IActorInteractionFeature
    {
        bool TryHandleInteraction();
    }
}
