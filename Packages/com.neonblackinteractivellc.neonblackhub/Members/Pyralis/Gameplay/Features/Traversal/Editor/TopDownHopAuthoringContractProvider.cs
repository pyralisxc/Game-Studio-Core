using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Features.Traversal;
using NeonBlack.Gameplay.Presentation.Animation;

namespace NeonBlack.Gameplay.Editor
{
    public sealed class TopDownHopAuthoringContractProvider : IAuthoringContractProvider
    {
        public IReadOnlyList<PyralisAuthoringContract> GetAuthoringContracts()
        {
            return new[]
            {
                new PyralisAuthoringContract(
                    stableId: "feature.actor.traversal.topdown-hop",
                    moduleId: "actor.traversal.topdown-hop",
                    displayName: "Top Down Hop",
                    authoringCategory: "Traversal",
                    requiredProfileType: typeof(TopDownHopProfile),
                    requiredRuntimeInterfaceNames: new[]
                    {
                        typeof(IFeatureModuleRuntime).FullName,
                        typeof(IActorGameplayActionReceiver).FullName
                    },
                    supportedPresentationModes: new[]
                    {
                        ActorPresentationMode.Sprite2D,
                        ActorPresentationMode.Billboard2_5D
                    },
                    unsupportedPresentationModes: new[]
                    {
                        ActorPresentationMode.Rigged3D
                    },
                    unsupportedLaneMessage: "Rigged3D actors should use the 3D traversal jump path instead of the top-down visual-hop module.",
                    consumedActionRoles: new[] { "Jump" },
                    nativeSetup: new[]
                    {
                        "create TopDownHopProfile",
                        "create FeatureModuleDefinition",
                        "assign runtime prefab with TopDownHopFeatureRuntime",
                        "assign profile asset",
                        "add module to PawnDefinition.featureModules",
                        "bind Jump in InputProfile"
                    },
                    firstProofTargetId: "proof.1p-pawn-movement",
                    confidence: PyralisAuthoringConfidence.Explicit,
                    assignmentFields: new[]
                    {
                        "FeatureModuleDefinition.moduleId",
                        "FeatureModuleDefinition.runtimePrefab",
                        "FeatureModuleDefinition.profileAsset",
                        "PawnDefinition.featureModules",
                        "InputProfile.gameplayActions"
                    },
                    customizationMoments: new[]
                    {
                        "TopDownHopProfile.actionRole",
                        "duration",
                        "height",
                        "cooldown",
                        "TopDownHopFeatureRuntime.visualTransform"
                    }),
                new PyralisAuthoringContract(
                    stableId: "feature.actor.traversal.3d",
                    moduleId: "actor.traversal.3d",
                    displayName: "3D Actor Traversal",
                    authoringCategory: "Traversal",
                    requiredProfileType: typeof(PawnTraversalProfile),
                    requiredRuntimeInterfaceNames: new[]
                    {
                        typeof(IFeatureModuleRuntime).FullName,
                        typeof(IActorTraversalFeature).FullName
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
                    unsupportedLaneMessage: "Sprite2D actors should use the 2D movement or top-down hop traversal path instead of the 3D traversal module.",
                    consumedActionRoles: new[] { "Jump", "Interact" },
                    nativeSetup: new[]
                    {
                        "create PawnTraversalProfile",
                        "create FeatureModuleDefinition",
                        "assign runtime prefab with PawnTraversalFeatureRuntime3D",
                        "assign profile asset",
                        "add module to PawnDefinition.featureModules",
                        "bind Jump or Interact in InputProfile"
                    },
                    firstProofTargetId: "proof.npc-enemy-behavior",
                    confidence: PyralisAuthoringConfidence.Explicit,
                    assignmentFields: new[]
                    {
                        "FeatureModuleDefinition.moduleId",
                        "FeatureModuleDefinition.runtimePrefab",
                        "FeatureModuleDefinition.profileAsset",
                        "PawnDefinition.featureModules",
                        "InputProfile.gameplayActions"
                    },
                    customizationMoments: new[]
                    {
                        "PawnTraversalProfile.allowClimb",
                        "PawnTraversalProfile.allowHang",
                        "PawnTraversalProfile.allowDodge",
                        "PawnTraversalProfile.jumpHeight",
                        "Pawn3DTraversalComponent traversal probes and climb zones"
                    })
            };
        }
    }
}
