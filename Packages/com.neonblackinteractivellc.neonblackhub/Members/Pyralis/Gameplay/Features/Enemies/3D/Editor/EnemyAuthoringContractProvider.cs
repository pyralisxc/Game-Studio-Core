using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Features.Enemies;
using NeonBlack.Gameplay.Presentation.Animation;

namespace NeonBlack.Gameplay.Editor
{
    public sealed class EnemyReactionAuthoringContractProvider : IAuthoringContractProvider
    {
        public IReadOnlyList<PyralisAuthoringContract> GetAuthoringContracts()
        {
            return new[]
            {
                new PyralisAuthoringContract(
                    stableId: "feature.enemy.reaction",
                    moduleId: "enemy.reaction",
                    displayName: "Enemy Reaction",
                    authoringCategory: "Enemies",
                    requiredProfileType: typeof(EnemyReactionProfile),
                    requiredRuntimeInterfaceNames: new[]
                    {
                        typeof(IFeatureModuleRuntime).FullName,
                        typeof(IEnemyReactionState).FullName
                    },
                    supportedPresentationModes: new[]
                    {
                        ActorPresentationMode.Billboard2_5D,
                        ActorPresentationMode.Rigged3D
                    },
                    nativeSetup: new[]
                    {
                        "create EnemyReactionProfile",
                        "create FeatureModuleDefinition",
                        "assign runtime prefab with EnemyReactionFeatureRuntime",
                        "assign profile asset",
                        "add module to FeatureModuleDefinition array on enemy actor",
                        "ensure HealthComponent and Health/animation sources are present for reaction pathways"
                    },
                    firstProofTargetId: "proof.npc-enemy-behavior",
                    confidence: PyralisAuthoringConfidence.Explicit,
                    assignmentFields: new[]
                    {
                        "FeatureModuleDefinition.moduleId",
                        "FeatureModuleDefinition.runtimePrefab",
                        "FeatureModuleDefinition.profileAsset",
                        "FeatureModuleDefinition.supportedPresentationModes"
                    },
                    customizationMoments: new[]
                    {
                        "EnemyReactionProfile.enableReactions",
                        "EnemyReactionProfile.hurtLockDuration",
                        "EnemyReactionProfile.staggerDamageThreshold",
                        "EnemyReactionProfile.hitPauseDuration",
                        "EnemyReactionProfile.cameraShakeIntensity",
                        "FeatureModuleDefinition.supportedPresentationModes"
                    })
            };
        }
    }

    public sealed class EnemyAmbientAuthoringContractProvider : IAuthoringContractProvider
    {
        public IReadOnlyList<PyralisAuthoringContract> GetAuthoringContracts()
        {
            return new[]
            {
                new PyralisAuthoringContract(
                    stableId: "feature.enemy.ambient",
                    moduleId: "enemy.ambient",
                    displayName: "Enemy Ambient",
                    authoringCategory: "Enemies",
                    requiredProfileType: typeof(EnemyAmbientFeatureProfile),
                    requiredRuntimeInterfaceNames: new[]
                    {
                        typeof(IFeatureModuleRuntime).FullName
                    },
                    supportedPresentationModes: new[]
                    {
                        ActorPresentationMode.Billboard2_5D,
                        ActorPresentationMode.Rigged3D
                    },
                    nativeSetup: new[]
                    {
                        "create EnemyAmbientFeatureProfile",
                        "create FeatureModuleDefinition",
                        "assign runtime prefab with EnemyAmbientFeatureRuntime",
                        "assign profile asset",
                        "add module to FeatureModuleDefinition array on enemy actor",
                        "assign EnemyAI or compatible enemy runtime host"
                    },
                    firstProofTargetId: "proof.npc-enemy-behavior",
                    confidence: PyralisAuthoringConfidence.Explicit,
                    assignmentFields: new[]
                    {
                        "FeatureModuleDefinition.moduleId",
                        "FeatureModuleDefinition.runtimePrefab",
                        "FeatureModuleDefinition.profileAsset",
                        "FeatureModuleDefinition.supportedPresentationModes"
                    },
                    customizationMoments: new[]
                    {
                        "EnemyAmbientFeatureProfile.enableAmbientLookAround",
                        "EnemyAmbientFeatureProfile.lookAroundInterval",
                        "EnemyAmbientFeatureProfile.requirePatrolState",
                        "EnemyAmbientFeatureProfile.suppressDuringReactionLock",
                        "FeatureModuleDefinition.supportedPresentationModes"
                    })
            };
        }
    }
}
