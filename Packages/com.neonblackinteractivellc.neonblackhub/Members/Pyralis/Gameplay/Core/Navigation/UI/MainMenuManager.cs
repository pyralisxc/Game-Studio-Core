using UnityEngine;
using UnityEngine.UI;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Core.Navigation
{
    /// <summary>
    /// Main menu controller for a panel-driven gameplay menu scene.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.UI,
        Relevance = "Main menu controller for a panel-driven gameplay menu scene.",
        AssignmentFields = new[] { "mainPanel", "settingsPanel", "newGameButton", "exitButton" },
        FirstProof = "Menu buttons correctly navigate between panels or trigger scene transitions."
    )]
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject creditsPanel;
        [SerializeField] private GameObject coopPanel;

        [Header("Main Menu Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button coopButton;
        [SerializeField] private Button exitButton;

        [Header("Settings Panel")]
        [SerializeField] private Button settingsBackButton;

        [Header("Credits Panel")]
        [SerializeField] private Button creditsBackButton;

        [Header("Co-op Panel")]
        [SerializeField] private Button coopHostButton;
        [SerializeField] private Button coopJoinButton;
        [SerializeField] private Button coopBackButton;

        [Header("Scene Names")]
        [SerializeField] private string gameSceneName = "Opening";

        [Header("Runtime Services")]
        [SerializeField, Tooltip("Scene transition service used by play/load/exit buttons. SceneFader and SceneLoader implement ISceneNavigator.")]
        private MonoBehaviour sceneNavigatorSource;

        private ISceneNavigator _sceneNavigator;

        private void Start()
        {
            ResolveRuntimeServices();

            if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGame);
            if (loadGameButton != null) loadGameButton.onClick.AddListener(OnLoadGame);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettings);
            if (creditsButton != null) creditsButton.onClick.AddListener(OnCredits);
            if (coopButton != null) coopButton.onClick.AddListener(OnCoop);
            if (exitButton != null) exitButton.onClick.AddListener(OnExit);

            if (settingsBackButton != null) settingsBackButton.onClick.AddListener(OnBackToMain);
            if (creditsBackButton != null) creditsBackButton.onClick.AddListener(OnBackToMain);
            if (coopBackButton != null) coopBackButton.onClick.AddListener(OnBackToMain);
            if (coopHostButton != null) coopHostButton.onClick.AddListener(OnCoopHost);
            if (coopJoinButton != null) coopJoinButton.onClick.AddListener(OnCoopJoin);

            ShowPanel(mainPanel);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnNewGame()
        {
            LoadConfiguredGameScene();
        }

        private void OnLoadGame()
        {
            Debug.Log("[MainMenu] Load Game pressed - wire up your save system here.");
            LoadConfiguredGameScene();
        }

        private void OnSettings()
        {
            ShowPanel(settingsPanel);
        }

        private void OnCredits()
        {
            ShowPanel(creditsPanel);
        }

        private void OnCoop()
        {
            ShowPanel(coopPanel);
        }

        private void OnCoopHost()
        {
            Debug.Log("[MainMenu] Host Co-op - wire up your multiplayer system here.");
            LoadConfiguredGameScene();
        }

        private void OnCoopJoin()
        {
            Debug.Log("[MainMenu] Join Co-op - wire up your multiplayer system here.");
        }

        private void OnBackToMain()
        {
            ShowPanel(mainPanel);
        }

        private void OnExit()
        {
            ISceneNavigator navigator = ResolveSceneNavigator();
            if (navigator != null)
            {
                navigator.QuitGame();
                return;
            }

            Debug.LogError("[MainMenu] Scene Navigator Source is not configured. Assign SceneFader, SceneLoader, or another ISceneNavigator.", this);
        }

        private void ShowPanel(GameObject target)
        {
            if (mainPanel != null) mainPanel.SetActive(mainPanel == target);
            if (settingsPanel != null) settingsPanel.SetActive(settingsPanel == target);
            if (creditsPanel != null) creditsPanel.SetActive(creditsPanel == target);
            if (coopPanel != null) coopPanel.SetActive(coopPanel == target);
        }

        public void SetGameSceneName(string sceneName)
        {
            if (!string.IsNullOrWhiteSpace(sceneName))
            {
                gameSceneName = sceneName;
            }
        }

        public void SetSceneNavigator(ISceneNavigator sceneNavigator)
        {
            _sceneNavigator = sceneNavigator;
        }

        private void LoadConfiguredGameScene()
        {
            if (string.IsNullOrWhiteSpace(gameSceneName))
            {
                Debug.LogError("[MainMenu] Game Scene Name is blank.", this);
                return;
            }

            ISceneNavigator navigator = ResolveSceneNavigator();
            if (navigator == null)
            {
                Debug.LogError("[MainMenu] Scene Navigator Source is not configured. Assign SceneFader, SceneLoader, or another ISceneNavigator.", this);
                return;
            }

            navigator.LoadScene(gameSceneName);
        }

        private void ResolveRuntimeServices()
        {
            ResolveSceneNavigator();
        }

        private ISceneNavigator ResolveSceneNavigator()
        {
            if (_sceneNavigator != null)
                return _sceneNavigator;

            if (sceneNavigatorSource == null)
                return null;

            _sceneNavigator = sceneNavigatorSource as ISceneNavigator;
            if (_sceneNavigator == null)
                _sceneNavigator = sceneNavigatorSource.GetComponent<ISceneNavigator>();

            return _sceneNavigator;
        }
    }
}
