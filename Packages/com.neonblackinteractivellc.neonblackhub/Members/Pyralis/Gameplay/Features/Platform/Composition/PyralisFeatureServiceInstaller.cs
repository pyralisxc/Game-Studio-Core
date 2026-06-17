using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Enemies;
using NeonBlack.Gameplay.Features.Feedback;
using NeonBlack.Gameplay.Features.GameFlow;
using NeonBlack.Gameplay.Features.Rpg.Runtime;
using NeonBlack.Gameplay.Features.Scoring;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace NeonBlack.Gameplay.Core.Runtime
{
    internal static class PyralisFeatureServiceInstaller
    {
        public static void RegisterFeatureServices(
            IContainerBuilder builder,
            PyralisRuntimeFeatureServicePolicy featureServices,
            Component scopeRoot,
            ItemCatalogDefinition itemCatalog,
            ProgressionCurveDefinition progressionCurve)
        {
            if (featureServices.UsesCombatServices)
                RegisterCombatServices(builder);

            if (featureServices.UsesEnemyServices)
                RegisterEnemyServices(builder, FindServiceInHierarchy<BattleManager>(scopeRoot));

            if (featureServices.UsesRpgServices)
                RegisterRpgServices(builder, itemCatalog, progressionCurve);

            if (featureServices.UsesGameFlowServices)
            {
                RegisterGameFlowServices(
                    builder,
                    FindLoadedSceneComponent<GameManager>() ?? FindServiceInHierarchy<GameManager>(scopeRoot));
            }

            if (featureServices.UsesScoringServices)
            {
                RegisterScoringServices(
                    builder,
                    FindLoadedSceneComponent<ParticipantScoreService>()
                    ?? FindServiceInHierarchy<ParticipantScoreService>(scopeRoot),
                    FindLoadedSceneComponent<LeaderboardManager>()
                    ?? FindServiceInHierarchy<LeaderboardManager>(scopeRoot));
            }

            if (featureServices.UsesFeedbackServices)
            {
                RegisterFeedbackServices(
                    builder,
                    FindLoadedSceneComponent<ParticipantFeedbackService>()
                    ?? FindServiceInHierarchy<ParticipantFeedbackService>(scopeRoot));
            }
        }

        public static void RegisterCombatServices(IContainerBuilder builder)
        {
            builder.Register<PawnComboProcessor>(Lifetime.Transient);
            builder.Register<PawnDamageHandler>(Lifetime.Transient);
        }

        public static void RegisterEnemyServices(IContainerBuilder builder, BattleManager battleManager)
        {
            builder.Register<EnemyDetectionService>(Lifetime.Singleton);
            builder.Register<EnemyCombatProcessor>(Lifetime.Singleton);
            RegisterComponent(builder, battleManager);
        }

        public static void RegisterRpgServices(
            IContainerBuilder builder,
            ItemCatalogDefinition itemCatalog,
            ProgressionCurveDefinition progressionCurve)
        {
            PyralisRpgServiceInstaller.Register(builder, itemCatalog, progressionCurve);
        }

        public static void RegisterGameFlowServices(IContainerBuilder builder, GameManager gameManager)
        {
            RegisterComponent(builder, gameManager);
        }

        public static void RegisterScoringServices(
            IContainerBuilder builder,
            ParticipantScoreService participantScoreService,
            LeaderboardManager leaderboardManager)
        {
            RegisterComponent(builder, participantScoreService);
            RegisterComponent(builder, leaderboardManager);
        }

        public static void RegisterFeedbackServices(IContainerBuilder builder, ParticipantFeedbackService participantFeedbackService)
        {
            RegisterComponent(builder, participantFeedbackService);
        }

        private static void RegisterComponent<T>(IContainerBuilder builder, T component)
            where T : UnityEngine.Component
        {
            if (component == null)
                return;

            builder.RegisterComponent(component).AsSelf().AsImplementedInterfaces();
        }

        private static T FindServiceInHierarchy<T>(Component scopeRoot) where T : class
        {
            return scopeRoot != null ? scopeRoot.GetComponentInChildren<T>(true) : null;
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
    }
}
