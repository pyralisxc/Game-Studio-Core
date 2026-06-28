using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NeonBlack.Gameplay.Modules.Combat;
using NeonBlack.Gameplay.Modules.Enemies;
using NeonBlack.Gameplay.Modules.Feedback;
using NeonBlack.Gameplay.Glue.SceneFlow.Arcade2D;
using NeonBlack.Gameplay.Data.Rpg;
using NeonBlack.Gameplay.Modules.Rpg.Runtime;
using NeonBlack.Gameplay.Modules.Scoring;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace NeonBlack.Gameplay.Glue.ServiceRegistration
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
                PyralisCombatServiceInstaller.Register(builder, scopeRoot);

            if (featureServices.UsesEnemyServices)
                PyralisEnemyServiceInstaller.Register(builder);

            if (featureServices.UsesRpgServices)
                RegisterRpgServices(builder, itemCatalog, progressionCurve);

            if (featureServices.UsesGameFlowServices)
            {
                RegisterGameFlowServices(
                    builder,
                    PyralisRuntimeSceneSearch.Find<GameManager>() ?? FindServiceInHierarchy<GameManager>(scopeRoot));
            }

            if (featureServices.UsesScoringServices)
                PyralisScoringServiceInstaller.Register(builder, scopeRoot);

            if (featureServices.UsesFeedbackServices)
                PyralisFeedbackServiceInstaller.Register(builder, scopeRoot);
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
    }
}
