using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NeonBlack.Gameplay.Glue.SceneFlow.Arcade2D;
using NeonBlack.Gameplay.Data.Rpg;
using NeonBlack.Gameplay.Modules.Rpg.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace NeonBlack.Gameplay.Glue.ServiceRegistration
{
    internal static class FeatureServiceInstaller
    {
        public static void RegisterFeatureServices(
            IContainerBuilder builder,
            RuntimeFeatureServicePolicy featureServices,
            Component scopeRoot,
            ItemCatalogDefinition itemCatalog,
            ProgressionCurveDefinition progressionCurve)
        {
            if (featureServices.UsesCombatServices)
                CombatServiceInstaller.Register(builder, scopeRoot);

            if (featureServices.UsesEnemyServices)
                EnemyServiceInstaller.Register(builder);

            if (featureServices.UsesRpgServices)
                RegisterRpgServices(builder, itemCatalog, progressionCurve);

            if (featureServices.UsesGameFlowServices)
            {
                RegisterGameFlowServices(
                    builder,
                    RuntimeSceneSearch.Find<ArcadeGameFlowController>() ?? FindServiceInHierarchy<ArcadeGameFlowController>(scopeRoot));
            }

            if (featureServices.UsesScoringServices)
                ScoringServiceInstaller.Register(builder, scopeRoot);

            if (featureServices.UsesFeedbackServices)
                FeedbackServiceInstaller.Register(builder, scopeRoot);
        }

        public static void RegisterRpgServices(
            IContainerBuilder builder,
            ItemCatalogDefinition itemCatalog,
            ProgressionCurveDefinition progressionCurve)
        {
            RpgRuntimeServices services = RpgServiceInstaller.Register(builder, itemCatalog, progressionCurve);
            ConfigureRpgUi(services);
        }

        public static void RegisterGameFlowServices(IContainerBuilder builder, ArcadeGameFlowController gameManager)
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

        private static void ConfigureRpgUi(RpgRuntimeServices services)
        {
            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded)
                    continue;

                foreach (GameObject root in scene.GetRootGameObjects())
                    ConfigureRpgUiRoot(root, services);
            }
        }

        private static void ConfigureRpgUiRoot(GameObject root, RpgRuntimeServices services)
        {
            RpgDialoguePanelPresenter[] dialoguePanels = root.GetComponentsInChildren<RpgDialoguePanelPresenter>(true);
            for (int i = 0; i < dialoguePanels.Length; i++)
                dialoguePanels[i].ConfigureRuntime(services.DialogueService);

            RpgLoadoutPanelPresenter[] loadoutPanels = root.GetComponentsInChildren<RpgLoadoutPanelPresenter>(true);
            for (int i = 0; i < loadoutPanels.Length; i++)
                loadoutPanels[i].ConfigureRuntime(services.EquipmentService);

            RpgQuestBoardPanelPresenter[] questPanels = root.GetComponentsInChildren<RpgQuestBoardPanelPresenter>(true);
            for (int i = 0; i < questPanels.Length; i++)
                questPanels[i].ConfigureRuntime(services.QuestService);

            RpgSkillTreePanelPresenter[] skillPanels = root.GetComponentsInChildren<RpgSkillTreePanelPresenter>(true);
            for (int i = 0; i < skillPanels.Length; i++)
                skillPanels[i].ConfigureRuntime(services.ProgressionService, services.SkillTreeService);

            RpgVendorPanelPresenter[] vendorPanels = root.GetComponentsInChildren<RpgVendorPanelPresenter>(true);
            for (int i = 0; i < vendorPanels.Length; i++)
                vendorPanels[i].ConfigureRuntime(services.VendorService);

            HubInteractionHudPresenter hudPresenter = root.GetComponentInChildren<HubInteractionHudPresenter>(true);
            HubInteractionSceneController[] hubControllers = root.GetComponentsInChildren<HubInteractionSceneController>(true);
            for (int i = 0; i < hubControllers.Length; i++)
                hubControllers[i].ConfigureRuntime(services.HubInteractionService, hudPresenter);
        }
    }
}
