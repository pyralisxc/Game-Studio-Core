using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Glue.SceneFlow.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonBlack.Gameplay.Glue.SceneFlow.Arcade2D
{
    public partial class ArcadeGameFlowController
    {
        [Header("Scene Names")]
        [SerializeField, Tooltip("Exact name of the main menu scene as listed in Build Settings.")]
        private string mainMenuSceneName = SceneNames.MainMenu;

        [Header("Levels")]
        [SerializeField, Tooltip("LevelRegistry asset. Required for random restart mode.")]
        private LevelRegistry levelRegistry;

        private ISceneNavigator _sceneNavigator;
        private IGameplaySettingsApplier _settings;

        public void RestartGame()
        {
            _settings?.Save();

            string sceneToLoad;
            if (LevelSession.IsRandom && levelRegistry != null)
            {
                LevelData next = levelRegistry.GetRandom();
                sceneToLoad = next != null ? next.sceneName : SceneManager.GetActiveScene().name;
                LevelSession.ChosenSceneName = sceneToLoad;
            }
            else if (!string.IsNullOrEmpty(LevelSession.ChosenSceneName))
            {
                sceneToLoad = LevelSession.ChosenSceneName;
            }
            else
            {
                sceneToLoad = SceneManager.GetActiveScene().name;
            }

            LoadScene(sceneToLoad);
        }

        public void GoToMainMenu()
        {
            _settings?.Save();
            LoadScene(mainMenuSceneName);
        }

        public void SetSceneNavigator(ISceneNavigator sceneNavigator)
        {
            _sceneNavigator = sceneNavigator;
        }

        public void SetSettings(IGameplaySettingsApplier settings)
        {
            _settings = settings;
        }

        private void LoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("[ArcadeGameFlowController] Scene name is blank.", this);
                return;
            }

            if (_sceneNavigator != null)
            {
                _sceneNavigator.LoadScene(sceneName);
                return;
            }

            Debug.LogError("[ArcadeGameFlowController] Scene Navigator is not injected. Ensure ISceneNavigator is registered in the LifetimeScope.", this);
        }
    }
}
