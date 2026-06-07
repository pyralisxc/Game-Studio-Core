using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Features.Interaction;
using NeonBlack.Gameplay.Presentation.Animation;

namespace NeonBlack.Gameplay.Editor
{
    public sealed class InteractionAuthoringContractProvider : IAuthoringContractProvider
    {
        public IReadOnlyList<PyralisAuthoringContract> GetAuthoringContracts()
        {
            return new[]
            {
                new PyralisAuthoringContract(
                    stableId: "feature.actor.interaction",
                    moduleId: "actor.interaction",
                    displayName: "Actor Interaction",
                    authoringCategory: "Interaction",
                    requiredProfileType: typeof(InteractionFeatureProfile),
                    requiredRuntimeInterfaceNames: new[]
                    {
                        typeof(IFeatureModuleRuntime).FullName,
                        typeof(IActorInteractionFeature).FullName
                    },
                    supportedPresentationModes: System.Array.Empty<ActorPresentationMode>(),
                    unsupportedPresentationModes: System.Array.Empty<ActorPresentationMode>(),
                    consumedActionRoles: new[] { "Interact" },
                    nativeSetup: new[]
                    {
                        "create InteractionFeatureProfile",
                        "create FeatureModuleDefinition",
                        "assign runtime prefab with interaction feature runtime",
                        "assign profile asset",
                        "add module to PawnDefinition.featureModules",
                        "bind Interact in InputProfile"
                    },
                    firstProofTargetId: "proof.action-selection",
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
                        "InteractionFeatureProfile.enableInteraction",
                        "InteractionFeatureProfile.interactionCooldown",
                        "InputProfile Interact binding"
                    })
            };
        }
    }
}
