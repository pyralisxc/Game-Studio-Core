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

        public PyralisRuntimeFeatureServicePolicy WithLoadedSceneEvidence(
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

        public static PyralisRuntimeFeatureServicePolicy ResolveWithLoadedSceneEvidence(SessionDefinition sessionDefinition)
        {
            return Resolve(sessionDefinition).WithLoadedSceneEvidence(
                HasLoadedSceneComponent<PawnCombatBehaviour>()
                || HasLoadedSceneComponent<PawnCombatBehaviour2D>(),
                HasLoadedSceneComponent<EnemyAI>()
                || HasLoadedSceneComponent<BattleManager>(),
                HasLoadedSceneComponentInNamespace("NeonBlack.Gameplay.Features.Rpg"),
                HasLoadedSceneComponent<GameManager>()
                || HasLoadedSceneComponentInNamespace("NeonBlack.Gameplay.Features.GameFlow"),
                HasLoadedSceneComponent<ParticipantScoreService>()
                || HasLoadedSceneComponent<LeaderboardManager>()
                || HasLoadedSceneComponent<StillnessBonus2D>()
                || HasLoadedSceneComponent<CollectibleFeedback2D>(),
                HasLoadedSceneComponent<ParticipantFeedbackService>()
                || HasLoadedSceneComponentInNamespace("NeonBlack.Gameplay.Features.Feedback"));
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

        private static bool HasLoadedSceneComponent<T>() where T : Component
        {
            // Loaded-scene evidence keeps visible scene-authored services active without creating a second route truth.
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

        private static bool HasLoadedSceneComponentInNamespace(string namespacePrefix)
        {
            // Namespace scans are coarse scene evidence; contracts and authored definitions remain the primary route signal.
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
