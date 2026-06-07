using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Features.Feedback;
using NeonBlack.Gameplay.Presentation.Animation;

namespace NeonBlack.Gameplay.Editor
{
    public sealed class FeedbackAuthoringContractProvider : IAuthoringContractProvider
    {
        public IReadOnlyList<PyralisAuthoringContract> GetAuthoringContracts()
        {
            return new[]
            {
                new PyralisAuthoringContract(
                    stableId: "feature.actor.feedback",
                    moduleId: "actor.feedback",
                    displayName: "Actor Feedback",
                    authoringCategory: "Feedback",
                    requiredProfileType: typeof(ActorFeedbackProfile),
                    requiredRuntimeInterfaceNames: new[]
                    {
                        typeof(IFeatureModuleRuntime).FullName,
                        typeof(IActorFeedbackPublisher).FullName
                    },
                    supportedPresentationModes: new[]
                    {
                        ActorPresentationMode.Sprite2D,
                        ActorPresentationMode.Billboard2_5D,
                        ActorPresentationMode.Rigged3D
                    },
                    nativeSetup: new[]
                    {
                        "create ActorFeedbackProfile",
                        "create FeatureModuleDefinition",
                        "assign runtime prefab with ActorFeedbackFeatureRuntime",
                        "assign profile asset",
                        "add module to PawnDefinition.featureModules or enemy actor module list",
                        "add at least one IActorFeedbackReceiver in the actor hierarchy"
                    },
                    firstProofTargetId: "proof.ui-hud-menu",
                    confidence: PyralisAuthoringConfidence.Explicit,
                    assignmentFields: new[]
                    {
                        "FeatureModuleDefinition.moduleId",
                        "FeatureModuleDefinition.runtimePrefab",
                        "FeatureModuleDefinition.profileAsset",
                        "FeatureModuleDefinition.supportedPresentationModes",
                        "PawnDefinition.featureModules"
                    },
                    customizationMoments: new[]
                    {
                        "ActorFeedbackProfile.publishDamageEvents",
                        "ActorFeedbackProfile.publishHealingEvents",
                        "ActorFeedbackProfile.publishDeathEvents",
                        "ActorFeedbackProfile.publishStatusEvents",
                        "IActorFeedbackReceiver.HandleFeedbackEvent"
                    })
            };
        }
    }
}
