using System;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Presentation.Animation;

namespace NeonBlack.Gameplay.Features.Enemies
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat,
        ModuleId = "enemy.reaction",
        ProfileType = typeof(EnemyReactionProfile),
        RequiredInterfaces = new[] { typeof(IFeatureModuleRuntime), typeof(IEnemyReactionState) },
        SupportedLanes = new[] { ActorPresentationMode.Billboard2_5D, ActorPresentationMode.ThirdPerson3D },
        FirstProof = "Verify that IsReactionLocked is true when the enemy is staggered or hit.",
        NativeSetup = new[]
        {
            "create EnemyReactionProfile",
            "create FeatureModuleDefinition",
            "assign runtime prefab with EnemyReactionFeatureRuntime",
            "assign profile asset",
            "add module to FeatureModuleDefinition array on enemy actor",
            "ensure HealthComponent and Health/animation sources are present for reaction pathways"
        },
        AssignmentFields = new[]
        {
            "FeatureModuleDefinition.moduleId",
            "FeatureModuleDefinition.runtimePrefab",
            "FeatureModuleDefinition.profileAsset",
            "FeatureModuleDefinition.supportedPresentationModes"
        },
        CustomizationMoments = new[]
        {
            "EnemyReactionProfile.enableReactions",
            "EnemyReactionProfile.hurtLockDuration",
            "EnemyReactionProfile.staggerDamageThreshold",
            "EnemyReactionProfile.hitPauseDuration",
            "EnemyReactionProfile.cameraShakeIntensity",
            "FeatureModuleDefinition.supportedPresentationModes"
        }
    )]
    public interface IEnemyReactionState
    {
        bool IsReactionLocked { get; }
    }
}
