using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using UnityEngine.UI;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Scoring
{
    /// <summary>
    /// Page-swap leaderboard screen. Hides the main menu page and shows the leaderboard page,
    /// matching the pattern used by SettingsScreen.
    /// </summary>
    [AuthoringContract(
        Category = "U I",
        CapabilityPath = "UI/HUD/Leaderboard Screen",
        Surface = AuthoringSurface.Goal,
        Summary = "UI screen for displaying top scores from a leaderboard service.",
        RequiredFields = new[] { nameof(_leaderboardPage), nameof(_rowContainer), nameof(_rowPrefab) },
        SetupSteps = new[] 
        { 
            "Wire Leaderboard Page.",
            "Assign Row Container and Row Prefab with Rank/Name/Score labels.",
            "Assign Main Menu Page and Back Button only when this screen owns menu-page return navigation.",
            "Assign Status Label when the page should show loading, empty, or unavailable states."
        },
        SuccessChecks = new[] { "Open the leaderboard in the menu and verify the 'Fetching scores...' status appears." },
        Tags = new[] { "capability:UI" }
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Scoring/Leaderboard Screen")]
    public class LeaderboardScreen : MonoBehaviour, IRuntimeValidationProvider
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (_leaderboardPage == null) yield return RuntimeValidationIssue.Required("Leaderboard Page is unassigned.");
            if (_rowContainer == null) yield return RuntimeValidationIssue.Required("Row Container is unassigned.");
            if (_rowPrefab == null) yield return RuntimeValidationIssue.Required("Row Prefab is unassigned.");
            else if (_rowPrefab.GetComponentsInChildren<TextMeshProUGUI>(true).Length < 3)
                yield return RuntimeValidationIssue.Required("Row Prefab needs at least 3 TMP labels for Rank, Name, and Score.");

            if (_backButton == null)
                yield return RuntimeValidationIssue.Recommended("Back Button is unassigned. This is valid when another route closes the leaderboard.");
            if (_statusLabel == null)
                yield return RuntimeValidationIssue.Recommended("Status Label is unassigned. Loading, empty, and unavailable messages will be hidden.");
        }
        [Header("Pages")]
        [SerializeField, Tooltip("Root GameObject of the main menu content. Hidden while leaderboard is open.")]
        private GameObject _mainMenuPage;

        [SerializeField, Tooltip("Root GameObject of the leaderboard content. Starts inactive.")]
        private GameObject _leaderboardPage;

        [Header("References")]
        [SerializeField, Tooltip("Button that returns to the main menu page.")]
        private Button _backButton;

        [SerializeField, Tooltip("Content transform of the ScrollView where rows are instantiated.")]
        private Transform _rowContainer;

        [SerializeField, Tooltip("Prefab with 3 TMP children: Rank, Name, Score.")]
        private GameObject _rowPrefab;

        [SerializeField, Tooltip("Label shown while fetching or when no scores exist.")]
        private TextMeshProUGUI _statusLabel;

        private bool _isOpen;
        private UnityEngine.Events.UnityAction _onBack;
        private ILeaderboardService _leaderboardService;

        public void ConfigureRuntime(ILeaderboardService leaderboardService)
        {
            if (leaderboardService != null)
                _leaderboardService = leaderboardService;
        }

        private void Start()
        {
            _leaderboardPage?.SetActive(false);
            _onBack = Close;
            _backButton?.onClick.AddListener(_onBack);
        }

        private void OnDestroy()
        {
            // OnDestroy can fire before Start if the object is destroyed while inactive.
            if (_onBack != null)
                _backButton?.onClick.RemoveListener(_onBack);
        }

        public void Open()
        {
            if (_isOpen)
                return;

            _isOpen = true;
            _mainMenuPage?.SetActive(false);
            _leaderboardPage?.SetActive(true);
            _ = RefreshAsync();
        }

        public void Close()
        {
            if (!_isOpen)
                return;

            _isOpen = false;
            _leaderboardPage?.SetActive(false);
            _mainMenuPage?.SetActive(true);
        }

        private async Task RefreshAsync()
        {
            ClearRows();
            SetStatus("Fetching scores...");

            if (_leaderboardService == null)
            {
                SetStatus("Leaderboard unavailable.");
                return;
            }

            List<LeaderboardEntry> entries = await _leaderboardService.GetTopScoresAsync();

            if (entries == null || entries.Count == 0)
            {
                SetStatus("No scores yet - play and set a record!");
                return;
            }

            SetStatus(string.Empty);

            foreach (LeaderboardEntry entry in entries)
            {
                if (_rowPrefab == null || _rowContainer == null)
                    break;

                GameObject row = Instantiate(_rowPrefab, _rowContainer);
                TextMeshProUGUI[] labels = row.GetComponentsInChildren<TextMeshProUGUI>();
                if (labels.Length >= 3)
                {
                    labels[0].text = "#" + (entry.Rank + 1);
                    labels[1].text = string.IsNullOrEmpty(entry.PlayerName) ? "-" : entry.PlayerName;
                    labels[2].text = ((int)entry.Score).ToString();
                }
            }
        }

        private void ClearRows()
        {
            if (_rowContainer == null)
                return;

            foreach (Transform child in _rowContainer)
                Destroy(child.gameObject);
        }

        private void SetStatus(string message)
        {
            if (_statusLabel != null)
                _statusLabel.text = message;
        }
    }
}
