using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace NeonBlack.Gameplay.Features.Scoring
{
/// <summary>
/// Page-swap leaderboard screen. Hides the main menu page and shows the leaderboard page,
/// matching the pattern used by SettingsScreen.
///
/// Setup:
///   1. In your Canvas create two root child GameObjects:
///        - MainMenuPage     â€” the page that holds your title, play/settings/leaderboard buttons.
///        - LeaderboardPage  â€” starts INACTIVE. Contains the scroll view and back button.
///   2. Inside LeaderboardPage, add a ScrollView â€” wire its Content transform to _rowContainer.
///   3. Create a "LeaderboardRow" prefab: a HorizontalLayoutGroup with three
///      TextMeshProUGUI children in order: Rank Â· Name Â· Score.
///   4. Attach this component to any persistent GameObject (e.g. the Canvas).
///   5. Wire all Inspector fields.
///   6. Call Open() from MainMenuController's leaderboard button.
/// </summary>
[AddComponentMenu("NeonBlack/Gameplay/Scoring/Leaderboard Screen")]
public class LeaderboardScreen : MonoBehaviour
{
    [Header("Pages")]
    [SerializeField, Tooltip("Root GameObject of the main menu content. Hidden while leaderboard is open.")]
    private GameObject _mainMenuPage;
    [SerializeField, Tooltip("Root GameObject of the leaderboard content. Starts inactive.")]
    private GameObject _leaderboardPage;

    [Header("References")]
    [SerializeField, Tooltip("Button that returns to the main menu page.")]
    private Button          _backButton;
    [SerializeField, Tooltip("Content transform of the ScrollView â€” rows are instantiated here.")]
    private Transform       _rowContainer;
    [SerializeField, Tooltip("Prefab with 3 TMP children: Rank, Name, Score.")]
    private GameObject      _rowPrefab;
    [SerializeField, Tooltip("Label shown while fetching or when no scores exist.")]
    private TextMeshProUGUI _statusLabel;

    private bool _isOpen;
    private UnityEngine.Events.UnityAction _onBack;
    private ILeaderboardService _leaderboardService;

    [Inject]
    private void Construct(ILeaderboardService leaderboardService = null)
    {
        _leaderboardService = leaderboardService;
    }

    // â”€â”€ Lifecycle â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private void Start()
    {
        _leaderboardPage?.SetActive(false);
        _onBack = Close;
        _backButton?.onClick.AddListener(_onBack);
    }

    private void OnDestroy()
    {
        // Guard: OnDestroy can fire before Start if the object is destroyed while
        // inactive, leaving _onBack null and causing a NullReferenceException.
        if (_onBack != null) _backButton?.onClick.RemoveListener(_onBack);
    }

    // â”€â”€ Public API â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public void Open()
    {
        if (_isOpen) return;
        _isOpen = true;
        _mainMenuPage?.SetActive(false);
        _leaderboardPage?.SetActive(true);
        _ = RefreshAsync();
    }

    public void Close()
    {
        if (!_isOpen) return;
        _isOpen = false;
        _leaderboardPage?.SetActive(false);
        _mainMenuPage?.SetActive(true);
    }

    // â”€â”€ Internal â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private async Task RefreshAsync()
    {
        ClearRows();
        SetStatus("Fetching scores\u2026");

        if (_leaderboardService == null)
        {
            SetStatus("Leaderboard unavailable.");
            return;
        }

        List<LeaderboardEntry> entries = await _leaderboardService.GetTopScoresAsync();

        if (entries == null || entries.Count == 0)
        {
            SetStatus("No scores yet \u2014 play and set a record!");
            return;
        }

        SetStatus(string.Empty);

        foreach (LeaderboardEntry entry in entries)
        {
            if (_rowPrefab == null || _rowContainer == null) break;
            GameObject row = Instantiate(_rowPrefab, _rowContainer);
            // Expects 3 TMP labels as children in order: rank, name, score.
            TextMeshProUGUI[] labels = row.GetComponentsInChildren<TextMeshProUGUI>();
            if (labels.Length >= 3)
            {
                labels[0].text = $"#{entry.Rank + 1}";
                labels[1].text = string.IsNullOrEmpty(entry.PlayerName) ? "\u2014" : entry.PlayerName;
                labels[2].text = ((int)entry.Score).ToString();
            }
        }
    }

    private void ClearRows()
    {
        if (_rowContainer == null) return;
        foreach (Transform child in _rowContainer)
            Destroy(child.gameObject);
    }

    private void SetStatus(string msg)
    {
        if (_statusLabel != null) _statusLabel.text = msg;
    }
}
}
