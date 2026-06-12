using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Animation;

namespace NeonBlack.Gameplay.Features.Composition
{
    [AuthoringContract(
        ModuleId = "actor.pickups.2d",
        Capability = AuthoringCapability.Inventory,
        Relevance = "Allows actors to detect and collect pickups (items, resources) in 2D space.",
        ProfileType = typeof(PickupFeatureProfile),
        RequiredInterfaces = new[] { typeof(IFeatureModuleRuntime), typeof(IActorInteractionHandler) },
        ExpertAdvice = "Implementations should define how the actor reacts to nearest interactables or auto-collects pickups.",
        SupportedLanes = new[] { ActorPresentationMode.Sprite2D },
        UnsupportedLanes = new[] { ActorPresentationMode.Billboard2_5D, ActorPresentationMode.ThirdPerson3D },
        UnsupportedLaneMessage = "Pickups 2D should use Sprite2D-only setup when collection is authored on 2D pawns.",
        ConsumedRoles = new[] { "Interact" },
        NativeSetup = new[]
        {
            "create PickupFeatureProfile",
            "create FeatureModuleDefinition",
            "assign runtime prefab with ActorPickupCollectorFeature2D",
            "assign profile asset",
            "add module to PawnDefinition.featureModules",
            "bind Interact in InputProfile"
        },
        FirstProof = "Walk into a pickup object and verify it is collected/removed.",
        FirstProofTargetId = "proof.custom-object-effect",
        DocumentationURL = "https://docs.neonblack.com/pyralis/actor-interaction",
        AssignmentFields = new[]
        {
            "FeatureModuleDefinition.moduleId",
            "FeatureModuleDefinition.runtimePrefab",
            "FeatureModuleDefinition.profileAsset",
            "FeatureModuleDefinition.supportedPresentationModes",
            "PawnDefinition.featureModules",
            "InputProfile.gameplayActions"
        },
        CustomizationMoments = new[]
        {
            "PickupFeatureProfile.enableAutoCollect",
            "PickupFeatureProfile.enableInteractionCollect",
            "PickupFeatureProfile.interactionRadius",
            "PickupFeatureProfile.collectibleLayers",
            "PickupFeatureProfile.preferNearestPickup"
        }
    )]
    [AuthoringContract(
        ModuleId = "actor.pickups.3d",
        Capability = AuthoringCapability.Inventory,
        Relevance = "Allows actors to detect and collect pickups (items, resources) in 3D space.",
        ProfileType = typeof(PickupFeatureProfile),
        RequiredInterfaces = new[] { typeof(IFeatureModuleRuntime), typeof(IActorInteractionHandler) },
        SupportedLanes = new[] { ActorPresentationMode.Billboard2_5D, ActorPresentationMode.ThirdPerson3D },
        UnsupportedLanes = new[] { ActorPresentationMode.Sprite2D },
        UnsupportedLaneMessage = "Pickups 3D should target Billboard2.5D and/or Rigged3D presentation modes.",
        ConsumedRoles = new[] { "Interact" },
        NativeSetup = new[]
        {
            "create PickupFeatureProfile",
            "create FeatureModuleDefinition",
            "assign runtime prefab with ActorPickupCollectorFeature3D",
            "assign profile asset",
            "add module to PawnDefinition.featureModules",
            "bind Interact in InputProfile"
        },
        FirstProof = "Walk into a pickup object and verify it is collected/removed.",
        FirstProofTargetId = "proof.custom-object-effect",
        DocumentationURL = "https://docs.neonblack.com/pyralis/actor-interaction",
        AssignmentFields = new[]
        {
            "FeatureModuleDefinition.moduleId",
            "FeatureModuleDefinition.runtimePrefab",
            "FeatureModuleDefinition.profileAsset",
            "FeatureModuleDefinition.supportedPresentationModes",
            "PawnDefinition.featureModules",
            "InputProfile.gameplayActions"
        },
        CustomizationMoments = new[]
        {
            "PickupFeatureProfile.enableAutoCollect",
            "PickupFeatureProfile.enableInteractionCollect",
            "PickupFeatureProfile.overlapRadius3D",
            "PickupFeatureProfile.collectibleLayers3D",
            "PickupFeatureProfile.preferNearestPickup"
        }
    )]
    public interface IActorInteractionHandler
    {
        bool TryHandleInteraction(ActorFeatureContext context);
    }
}
