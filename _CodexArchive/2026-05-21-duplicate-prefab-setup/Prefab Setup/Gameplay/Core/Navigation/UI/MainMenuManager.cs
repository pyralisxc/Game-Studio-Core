using UnityEngine;
using UnityEngine.UI;
using NeonBlack.Gameplay.Core.Runtime;

namespace NeonBlack.Gameplay.Core.Navigation
{
    /// <summary>
    /// Main menu controller for a panel-driven gameplay menu scene.
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject coopPanel;

        [Header("Main Menu Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button coopButton;
        [SerializeField] private Button exitButton;

        [Header("Settings Panel")]
        [SerializeField] private Button settingsBackButton;

        [Header("Co-op Panel")]
        [SerializeField] private Button coopHostButton;
        [SerializeField] private Button coopJoinButton;
        [SerializeField] private Button coopBackButton;

        [Header("Scene Names")]
        [SerializeField] private string gameSceneName = "Opening";

        private void Start()
        {
            newGameButton.onClick.AddListener(OnNewGame);
            loadGameButton.onClick.AddListener(OnLoadGame);
            settingsButton.onClick.AddListener(OnSettings);
            coopButton.onClick.AddListener(OnCoop);
            exitButton.onClick.AddListener(OnExit);

            if (settingsBackButton != null) settingsBackButton.onClick.AddListener(OnBackToMain);
            if (coopBackButton != null) coopBackButton.onClick.AddListener(OnBackToMain);
            if (coopHostButton != null) coopHostButton.onClick.AddListener(OnCoopHost);
            if (coopJoinButton != null) coopJoinButton.onClick.AddListener(OnCoopJoin);

            ShowPanel(mainPanel);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnNewGame()
        {
            SceneLoader.Instance.LoadScene(gameSceneName);
        }

        private void OnLoadGame()
        {
            Debug.Log("[MainMenu] Load Game pressed - wire up your save system here.");
            SceneLoader.Instance.LoadScene(gameSceneName);
        }

        private void OnSettings()
        {
            ShowPanel(settingsPanel);
        }

        private void OnCoop()
        {
            ShowPanel(coopPanel);
        }

        private void OnCoopHost()
        {
            Debug.Log("[MainMenu] Host Co-op - wire up your multiplayer system here.");
            SceneLoader.Instance.LoadScene(gameSceneName);
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
            SceneLoader.Instance.QuitGame();
        }

        private void ShowPanel(GameObject target)
        {
            if (mainPanel != null) mainPanel.SetActive(mainPanel == target);
            if (settingsPanel != null) settingsPanel.SetActive(settingsPanel == target);
            if (coopPanel != null) coopPanel.SetActive(coopPanel == target);
        }

        public void SetGameSceneName(string sceneName)
        {
            if (!string.IsNullOrWhiteSpace(sceneName))
            {
                gameSceneName = sceneName;
            }
        }
    }
}
