using System;
using UnityEngine;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Presentation.Animation;

namespace NeonBlack.Gameplay.Features.Combat
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat | AuthoringCapability.Stats,
        ModuleId = "actor.status",
        Relevance = "Handles the application and management of combat status effects and stat modifiers on actors.",
        ProfileType = typeof(ActorStatusEffectProfile),
        RequiredInterfaces = new Type[]
        {
            typeof(IFeatureModuleRuntime),
            typeof(IActorStatusEffectReceiver),
            typeof(IDamageModifier)
        },
        SupportedLanes = new[]
        {
            ActorPresentationMode.Sprite2D,
            ActorPresentationMode.Billboard2_5D,
            ActorPresentationMode.Rigged3D
        },
        NativeSetup = new[]
        {
            "create ActorStatusEffectProfile",
            "create FeatureModuleDefinition",
            "assign runtime prefab with ActorStatusEffectFeatureRuntime",
            "assign profile asset",
            "add module to PawnDefinition.featureModules or enemy actor module list",
            "ensure HealthComponent and status modifier receivers are present on actors that consume status effects"
        },
        FirstProof = "Apply a status effect to an actor and verify it appears in the active effect list.",
        AssignmentFields = new[]
        {
            "FeatureModuleDefinition.moduleId",
            "FeatureModuleDefinition.runtimePrefab",
            "FeatureModuleDefinition.profileAsset",
            "FeatureModuleDefinition.supportedPresentationModes",
            "PawnDefinition.featureModules"
        },
        CustomizationMoments = new[]
        {
            "ActorStatusEffectProfile.startingEffects",
            "ActorStatusEffectProfile.allowRefreshExistingEffects",
            "ActorStatusEffectProfile.defaultShieldDamageReduction",
            "StatusEffectDefinition.effectKind",
            "StatusEffectDefinition.stackMode"
        })]
    public interface IActorStatusEffectReceiver
    {
        void ApplyStatusEffect(StatusEffectDefinition effectDefinition, GameObject source = null);
    }
}
