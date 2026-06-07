using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Presentation.Animation;

namespace NeonBlack.Gameplay.Editor
{
    public sealed class CombatAuthoringContractProvider : IAuthoringContractProvider
    {
        public IReadOnlyList<PyralisAuthoringContract> GetAuthoringContracts()
        {
            return new[]
            {
                new PyralisAuthoringContract(
                    stableId: "feature.actor.combat.reaction",
                    moduleId: "actor.combat.reaction",
                    displayName: "Actor Combat Reaction",
                    authoringCategory: "Combat",
                    requiredProfileType: typeof(ActorCombatReactionProfile),
                    requiredRuntimeInterfaceNames: new[]
                    {
                        typeof(IFeatureModuleRuntime).FullName,
                        typeof(NeonBlack.Gameplay.Features.Combat.IActorGuardFeature).FullName,
                        typeof(IDamageModifier).FullName
                    },
                    supportedPresentationModes: new[]
                    {
                        ActorPresentationMode.Sprite2D,
                        ActorPresentationMode.Billboard2_5D,
                        ActorPresentationMode.Rigged3D
                    },
                    consumedActionRoles: new[] { "Guard" },
                    nativeSetup: new[]
                    {
                        "create ActorCombatReactionProfile",
                        "create FeatureModuleDefinition",
                        "assign runtime prefab with ActorCombatReactionFeatureRuntime",
                        "assign profile asset",
                        "add module to PawnDefinition.featureModules or enemy actor module list",
                        "ensure HealthComponent and IActorReactionResponder are present on the actor root"
                    },
                    firstProofTargetId: "proof.npc-enemy-behavior",
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
                        "ActorCombatReactionProfile.enableGuard",
                        "ActorCombatReactionProfile.blockDamageReduction",
                        "ActorCombatReactionProfile.blockFrontalAngle",
                        "ActorCombatReactionProfile.enableParry",
                        "ActorCombatReactionProfile.staggerDamageThreshold"
                    }),
                new PyralisAuthoringContract(
                    stableId: "feature.actor.status",
                    moduleId: "actor.status",
                    displayName: "Actor Status Effects",
                    authoringCategory: "Combat",
                    requiredProfileType: typeof(ActorStatusEffectProfile),
                    requiredRuntimeInterfaceNames: new[]
                    {
                        typeof(IFeatureModuleRuntime).FullName,
                        typeof(NeonBlack.Gameplay.Features.Combat.IActorStatusEffectReceiver).FullName,
                        typeof(IDamageModifier).FullName
                    },
                    supportedPresentationModes: new[]
                    {
                        ActorPresentationMode.Sprite2D,
                        ActorPresentationMode.Billboard2_5D,
                        ActorPresentationMode.Rigged3D
                    },
                    nativeSetup: new[]
                    {
                        "create ActorStatusEffectProfile",
                        "create FeatureModuleDefinition",
                        "assign runtime prefab with ActorStatusEffectFeatureRuntime",
                        "assign profile asset",
                        "add module to PawnDefinition.featureModules or enemy actor module list",
                        "ensure HealthComponent and status modifier receivers are present on actors that consume status effects"
                    },
                    firstProofTargetId: "proof.custom-object-effect",
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
                        "ActorStatusEffectProfile.startingEffects",
                        "ActorStatusEffectProfile.allowRefreshExistingEffects",
                        "ActorStatusEffectProfile.defaultShieldDamageReduction",
                        "StatusEffectDefinition.effectKind",
                        "StatusEffectDefinition.stackMode"
                    })
            };
        }
    }
}
