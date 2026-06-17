using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Enemies;
using NeonBlack.Gameplay.Features.Feedback;
using NeonBlack.Gameplay.Features.GameFlow;
using NeonBlack.Gameplay.Features.Pickups;
using NeonBlack.Gameplay.Features.Scoring;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonBlack.Gameplay.Core.Runtime
{
    internal readonly struct PyralisRuntimeFeatureServicePolicy
    {
        public PyralisRuntimeFeatureServicePolicy(
            bool usesCombatServices,
            bool usesEnemyServices,
            bool usesRpgServices,
            bool usesGameFlowServices,
            bool usesScoringServices,
            bool usesFeedbackServices)
        {
            UsesCombatServices = usesCombatServices;
            UsesEnemyServices = usesEnemyServices;
            UsesRpgServices = usesRpgServices;
            UsesGameFlowServices = usesGameFlowServices;
            UsesScoringServices = usesScoringServices;
            UsesFeedbackServices = usesFeedbackServices;
        }

        public bool UsesCombatServices { get; }
        public bool UsesEnemyServices { get; }
        public bool UsesRpgServices { get; }
        public bool UsesGameFlowServices { get; }
        public bool UsesScoringServices { get; }
        public bool UsesFeedbackServices { get; }

        public PyralisRuntimeFeatureServicePolicy WithCompatibilityEvidence(
            bool usesCombatServices,
            bool usesEnemyServices,
            bool usesRpgServices,
            bool usesGameFlowServices,
            bool usesScoringServices,
            bool usesFeedbackServices)
        {
            return new PyralisRuntimeFeatureServicePolicy(
                UsesCombatServices || usesCombatServices,
                UsesEnemyServices || usesEnemyServices,
                UsesRpgServices || usesRpgServices,
                UsesGameFlowServices || usesGameFlowServices,
                UsesScoringServices || usesScoringServices,
                UsesFeedbackServices || usesFeedbackServices);
        }

        public static PyralisRuntimeFeatureServicePolicy Resolve(SessionDefinition sessionDefinition)
        {
            bool usesCombat = false;
            bool usesEnemy = false;
            bool usesRpg = false;
            bool usesGameFlow = false;
            bool usesScoring = false;
            bool usesFeedback = false;

            GameModeDefinition mode = sessionDefinition != null ? sessionDefinition.defaultGameMode : null;
            if (mode != null)
            {
                usesCombat |= mode.enableCombat || mode.enableHazards;
                usesGameFlow |= mode.enablePickups || mode.enableHazards || mode.enableScore || mode.enableRespawn;
                usesScoring |= mode.enablePickups || mode.enableScore;
                usesFeedback |= mode.enableCombat || mode.enablePickups || mode.enableHazards || mode.enableScore;
                AppendModuleSignals(
                    mode.requiredFeatureModules,
                    ref usesCombat,
                    ref usesEnemy,
                    ref usesRpg,
                    ref usesGameFlow,
                    ref usesScoring,
                    ref usesFeedback);
            }

            ParticipantDefinition[] participants = sessionDefinition != null ? sessionDefinition.defaultParticipants : null;
            if (participants != null)
            {
                for (int i = 0; i < participants.Length; i++)
                {
                    PawnDefinition pawn = participants[i] != null ? participants[i].defaultPawn : null;
                    if (pawn == null)
                        continue;

                    usesCombat |= pawn.combatProfile != null && pawn.combatProfile.enableCombat;
                    AppendModuleSignals(
                        pawn.featureModules,
                        ref usesCombat,
                        ref usesEnemy,
                        ref usesRpg,
                        ref usesGameFlow,
                        ref usesScoring,
                        ref usesFeedback);
                }
            }

            return new PyralisRuntimeFeatureServicePolicy(
                usesCombat,
                usesEnemy,
                usesRpg,
                usesGameFlow,
                usesScoring,
                usesFeedback);
        }

        public static PyralisRuntimeFeatureServicePolicy ResolveWithCompatibilityEvidence(SessionDefinition sessionDefinition)
        {
            return Resolve(sessionDefinition).WithCompatibilityEvidence(
                HasCompatibilitySceneComponent<PawnCombatBehaviour>()
                || HasCompatibilitySceneComponent<PawnCombatBehaviour2D>(),
                HasCompatibilitySceneComponent<EnemyAI>()
                || HasCompatibilitySceneComponent<BattleManager>(),
                HasCompatibilitySceneComponentInNamespace("NeonBlack.Gameplay.Features.Rpg"),
                HasCompatibilitySceneComponent<GameManager>()
                || HasCompatibilitySceneComponentInNamespace("NeonBlack.Gameplay.Features.GameFlow"),
                HasCompatibilitySceneComponent<ParticipantScoreService>()
                || HasCompatibilitySceneComponent<LeaderboardManager>()
                || HasCompatibilitySceneComponent<StillnessBonus2D>()
                || HasCompatibilitySceneComponent<CollectibleFeedback2D>(),
                HasCompatibilitySceneComponent<ParticipantFeedbackService>()
                || HasCompatibilitySceneComponentInNamespace("NeonBlack.Gameplay.Features.Feedback"));
        }

        private static void AppendModuleSignals(
            FeatureModuleDefinition[] modules,
            ref bool usesCombat,
            ref bool usesEnemy,
            ref bool usesRpg,
            ref bool usesGameFlow,
            ref bool usesScoring,
            ref bool usesFeedback)
        {
            if (modules == null)
                return;

            for (int i = 0; i < modules.Length; i++)
            {
                FeatureModuleDefinition module = modules[i];
                if (module == null)
                    continue;

                ResolvedAuthoringContract contract = ResolvedAuthoringContractRegistry.FindByModuleId(module.moduleId);
                if (contract != null)
                {
                    AppendContractSignals(
                        contract,
                        ref usesCombat,
                        ref usesEnemy,
                        ref usesRpg,
                        ref usesGameFlow,
                        ref usesScoring,
                        ref usesFeedback);
                }
            }
        }

        private static void AppendContractSignals(
            ResolvedAuthoringContract contract,
            ref bool usesCombat,
            ref bool usesEnemy,
            ref bool usesRpg,
            ref bool usesGameFlow,
            ref bool usesScoring,
            ref bool usesFeedback)
        {
            usesCombat |= contract.Capability.HasFlag(AuthoringCapability.Combat)
                || contract.Capability.HasFlag(AuthoringCapability.CombatState)
                || contract.Capability.HasFlag(AuthoringCapability.CombatSensors)
                || contract.Capability.HasFlag(AuthoringCapability.RangedFlow);
            usesEnemy |= contract.Capability.HasFlag(AuthoringCapability.TacticsAggressive)
                || contract.Capability.HasFlag(AuthoringCapability.TacticsDefensive);
            usesRpg |= contract.Capability.HasFlag(AuthoringCapability.Rpg)
                || contract.Capability.HasFlag(AuthoringCapability.Inventory)
                || contract.Capability.HasFlag(AuthoringCapability.Quests)
                || contract.Capability.HasFlag(AuthoringCapability.Dialogue)
                || contract.Capability.HasFlag(AuthoringCapability.Vendors)
                || contract.Capability.HasFlag(AuthoringCapability.SkillTree)
                || contract.Capability.HasFlag(AuthoringCapability.Progression)
                || contract.Capability.HasFlag(AuthoringCapability.Stats);
            usesGameFlow |= contract.Capability.HasFlag(AuthoringCapability.Rules)
                || contract.Capability.HasFlag(AuthoringCapability.Participants);
            usesScoring |= contract.Capability.HasFlag(AuthoringCapability.Scoring)
                || contract.Capability.HasFlag(AuthoringCapability.Rules);
            usesFeedback |= contract.Capability.HasFlag(AuthoringCapability.UI)
                || contract.Capability.HasFlag(AuthoringCapability.VFX)
                || contract.Capability.HasFlag(AuthoringCapability.Animation);
        }

        private static bool HasCompatibilitySceneComponent<T>() where T : Component
        {
            // Compatibility evidence keeps hand-authored existing scenes alive while feature contracts
            // become the primary activation path. Do not treat these scans as new route truth.
            return FindLoadedSceneComponent<T>() != null;
        }

        private static T FindLoadedSceneComponent<T>() where T : Component
        {
            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded)
                    continue;

                GameObject[] roots = scene.GetRootGameObjects();
                for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
                {
                    if (roots[rootIndex] == null)
                        continue;

                    T component = roots[rootIndex].GetComponentInChildren<T>(true);
                    if (component != null)
                        return component;
                }
            }

            return null;
        }

        private static bool HasCompatibilitySceneComponentInNamespace(string namespacePrefix)
        {
            // Namespace scans are compatibility evidence for older scene-authored feature stacks.
            // Prefer policy/contract activation for new feature services.
            if (string.IsNullOrWhiteSpace(namespacePrefix))
                return false;

            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded)
                    continue;

                GameObject[] roots = scene.GetRootGameObjects();
                for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
                {
                    GameObject root = roots[rootIndex];
                    if (root == null)
                        continue;

                    MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
                    for (int i = 0; i < behaviours.Length; i++)
                    {
                        Type type = behaviours[i] != null ? behaviours[i].GetType() : null;
                        if (type != null
                            && type.Namespace != null
                            && type.Namespace.StartsWith(namespacePrefix, StringComparison.Ordinal))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
