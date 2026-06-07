using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Presentation.Animation;

namespace NeonBlack.Gameplay.Editor
{
    public sealed class PickupsAuthoringContractProvider : IAuthoringContractProvider
    {
        public IReadOnlyList<PyralisAuthoringContract> GetAuthoringContracts()
        {
            return new[]
            {
                new PyralisAuthoringContract(
                    stableId: "feature.actor.pickups.2d",
                    moduleId: "actor.pickups.2d",
                    displayName: "Actor Pickups 2D",
                    authoringCategory: "Pickups",
                    requiredProfileType: typeof(PickupFeatureProfile),
                    requiredRuntimeInterfaceNames: new[]
                    {
                        typeof(IFeatureModuleRuntime).FullName,
                        typeof(IActorInteractionHandler).FullName
                    },
                    supportedPresentationModes: new[]
                    {
                        ActorPresentationMode.Sprite2D
                    },
                    unsupportedPresentationModes: new[]
                    {
                        ActorPresentationMode.Billboard2_5D,
                        ActorPresentationMode.Rigged3D
                    },
                    unsupportedLaneMessage: "Pickups 2D should use Sprite2D-only setup when collection is authored on 2D pawns.",
                    consumedActionRoles: new[] { "Interact" },
                    nativeSetup: new[]
                    {
                        "create PickupFeatureProfile",
                        "create FeatureModuleDefinition",
                        "assign runtime prefab with ActorPickupCollectorFeature2D",
                        "assign profile asset",
                        "add module to PawnDefinition.featureModules",
                        "bind Interact in InputProfile"
                    },
                    firstProofTargetId: "proof.custom-object-effect",
                    confidence: PyralisAuthoringConfidence.Explicit,
                    assignmentFields: new[]
                    {
                        "FeatureModuleDefinition.moduleId",
                        "FeatureModuleDefinition.runtimePrefab",
                        "FeatureModuleDefinition.profileAsset",
                        "FeatureModuleDefinition.supportedPresentationModes",
                        "PawnDefinition.featureModules",
                        "InputProfile.gameplayActions"
                    },
                    customizationMoments: new[]
                    {
                        "PickupFeatureProfile.enableAutoCollect",
                        "PickupFeatureProfile.enableInteractionCollect",
                        "PickupFeatureProfile.interactionRadius",
                        "PickupFeatureProfile.collectibleLayers",
                        "PickupFeatureProfile.preferNearestPickup"
                    }),
                new PyralisAuthoringContract(
                    stableId: "feature.actor.pickups.3d",
                    moduleId: "actor.pickups.3d",
                    displayName: "Actor Pickups 3D",
                    authoringCategory: "Pickups",
                    requiredProfileType: typeof(PickupFeatureProfile),
                    requiredRuntimeInterfaceNames: new[]
                    {
                        typeof(IFeatureModuleRuntime).FullName,
                        typeof(IActorInteractionHandler).FullName
                    },
                    supportedPresentationModes: new[]
                    {
                        ActorPresentationMode.Billboard2_5D,
                        ActorPresentationMode.Rigged3D
                    },
                    unsupportedPresentationModes: new[]
                    {
                        ActorPresentationMode.Sprite2D
                    },
                    unsupportedLaneMessage: "Pickups 3D should target Billboard2.5D and/or Rigged3D presentation modes.",
                    consumedActionRoles: new[] { "Interact" },
                    nativeSetup: new[]
                    {
                        "create PickupFeatureProfile",
                        "create FeatureModuleDefinition",
                        "assign runtime prefab with ActorPickupCollectorFeature3D",
                        "assign profile asset",
                        "add module to PawnDefinition.featureModules",
                        "bind Interact in InputProfile"
                    },
                    firstProofTargetId: "proof.custom-object-effect",
                    confidence: PyralisAuthoringConfidence.Explicit,
                    assignmentFields: new[]
                    {
                        "FeatureModuleDefinition.moduleId",
                        "FeatureModuleDefinition.runtimePrefab",
                        "FeatureModuleDefinition.profileAsset",
                        "FeatureModuleDefinition.supportedPresentationModes",
                        "PawnDefinition.featureModules",
                        "InputProfile.gameplayActions"
                    },
                    customizationMoments: new[]
                    {
                        "PickupFeatureProfile.enableAutoCollect",
                        "PickupFeatureProfile.enableInteractionCollect",
                        "PickupFeatureProfile.overlapRadius3D",
                        "PickupFeatureProfile.collectibleLayers3D",
                        "PickupFeatureProfile.preferNearestPickup"
                    })
            };
        }
    }
}
