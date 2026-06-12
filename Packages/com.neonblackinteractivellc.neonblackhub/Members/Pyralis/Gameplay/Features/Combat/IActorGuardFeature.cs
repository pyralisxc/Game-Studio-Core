using System;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Presentation.Animation;

namespace NeonBlack.Gameplay.Features.Combat
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat,
        ModuleId = "actor.combat.reaction",
        Relevance = "Enables combat reactions like guarding, blocking, and parrying to reduce incoming damage.",
        ExpertAdvice = "Requires ActorCombatReactionProfile. Ensure frontal angle is tuned for different actor orientations.",
        ProfileType = typeof(ActorCombatReactionProfile),
        RequiredInterfaces = new Type[]
        {
            typeof(IFeatureModuleRuntime),
            typeof(IActorGuardFeature),
            typeof(IDamageModifier)
        },
        SupportedLanes = new[]
        {
            ActorPresentationMode.Sprite2D,
            ActorPresentationMode.Billboard2_5D,
            ActorPresentationMode.ThirdPerson3D
        },
        ConsumedRoles = new[] { "Guard" },
        NativeSetup = new[]
        {
            "create ActorCombatReactionProfile",
            "create FeatureModuleDefinition",
            "assign runtime prefab with ActorCombatReactionFeatureRuntime",
            "assign profile asset",
            "add module to PawnDefinition.featureModules or enemy actor module list",
            "ensure HealthComponent and IActorReactionResponder are present on the actor root"
        },
        FirstProof = "Hold the Guard button and verify the actor's IsGuarding state becomes true.",
        FirstProofTargetId = "proof.npc-enemy-behavior",
        DocumentationURL = "https://docs.neonblack.com/pyralis/actor-guard",
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
            "ActorCombatReactionProfile.enableGuard",
            "ActorCombatReactionProfile.blockDamageReduction",
            "ActorCombatReactionProfile.blockFrontalAngle",
            "ActorCombatReactionProfile.enableParry",
            "ActorCombatReactionProfile.staggerDamageThreshold"
        })]
    public interface IActorGuardFeature
    {
        bool IsGuarding { get; }
        float BlockDamageReduction { get; }
        float BlockFrontalAngle { get; }
        void BeginGuard();
        void EndGuard();
    }
}
