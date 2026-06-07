using TMPro;
using NeonBlack.Gameplay.Features.Scoring;
using NeonBlack.Gameplay.Features.Settings;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VContainer;

namespace NeonBlack.Gameplay.Features.GameFlow
{
/// <summary>
/// Manages all UI panels: HUD (live score + time) and Game Over screen.
/// Subscribes to GameManager.OnGameStateChanged and ScoreManager.OnPointsChanged.
/// Setup: Attach to a GameObject on the UI Canvas.
/// Wire all panel GameObjects, TMP labels, and Buttons in the Inspector.
/// Canvas must be Screen Space - Overlay with a PhysicsRaycaster for button input.
/// </summary>
[DefaultExecutionOrder(-10)] // Subscribes to GameManager events in Start; GameManager must be ready.
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField, Tooltip("HUD panel shown while the player is alive.")]
    private GameObject _hudPanel;
    [SerializeField, Tooltip("Panel shown on the game over screen.")]
    private GameObject _gameOverPanel;

    [Header("HUD Labels")]
    [SerializeField, Tooltip("TMP label displaying the current score.")]
    private TextMeshProUGUI _scoreLabel;
    [SerializeField, Tooltip("TMP label displaying survival time. Updated every frame during play.")]
    private TextMeshProUGUI _timeLabel;

    [Header("Game Over Labels")]
    [SerializeField, Tooltip("TMP label showing the final score on the game over screen.")]
    private TextMeshProUGUI _finalScoreLabel;
    [SerializeField, Tooltip("TMP label showing the all-time high score on the game over screen.")]
    private TextMeshProUGUI _highScoreLabel;

    [Header("Game Over Buttons")]
    [SerializeField, Tooltip("Restart button on the game over panel.")]
    private Button _restartButton;
    [SerializeField, Tooltip("Main Menu button on the game over panel.")]
    private Button _mainMenuButton;

    [Header("Settings")]
    [SerializeField, Tooltip("Gear/settings button shown on the HUD.")]
    private Button _settingsButton;
    [SerializeField, Tooltip("SettingsScreen component to open when the settings button is pressed.")]
    private SettingsScreen _settingsScreen;

    [Header("HUD Format")]
    [SerializeField, Tooltip("Prefix shown before the formatted time value in the HUD (e.g. 'Time: ').")]
    private string _timePrefix = "Time: ";
    [SerializeField, Tooltip("Prefix shown before the score count in the HUD (e.g. 'Points: ').")]
    [FormerlySerializedAs("_crumbsPrefix")]
    private string _scorePrefix = "Points: ";

    private ParticipantScoreService _scoreService;
    private bool  _showingHUD;
    private float _timeUpdateTimer;
    // Pre-allocated buffer for zero-GC time string writes.
    // Layout: [prefix chars][m digit(s)][':'][s tens][s ones]['.'][d]
    // Rebuilt each tick by writing directly into the char array â€” no string allocations.
    private char[] _timeBuffer;
    private int    _timePrefixLen;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        // Pre-allocate the time display buffer once.
        // Max layout: "Time: 99:59.9" = prefix(6) + 2 + 1 + 2 + 1 + 1 = 13 chars max.
        // Allocate generously; unused tail chars are never written.
        _timePrefixLen = _timePrefix != null ? _timePrefix.Length : 0;
        _timeBuffer = new char[_timePrefixLen + 16];
        if (_timePrefix != null)
            for (int i = 0; i < _timePrefixLen; i++)
                _timeBuffer[i] = _timePrefix[i];
    }

    private void Start()
    {
        _restartButton?.onClick.AddListener(OnRestartClicked);
        _mainMenuButton?.onClick.AddListener(OnMainMenuClicked);
        _settingsButton?.onClick.AddListener(OnSettingsClicked);

        // Subscribe in Start so all Awakes have run and GameManager.Instance is guaranteed set
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged.AddListener(OnGameStateChanged);
        }
        else
            Debug.LogError("[UIManager] GameManager.Instance is null in Start.", this);

        if (_scoreService != null)
        {
            _scoreService.OnPointsChanged.AddListener(OnScoreUpdated);
            // GameManager.Start() fired ResetScore() (and OnPointsChanged) before we subscribed.
            // Push the current values now so labels are correct from frame one.
            OnScoreUpdated(_scoreService.PointsCollected);
        }

        // Sync panels to whatever state already exists
        GameState startState = GameManager.Instance != null ? GameManager.Instance.CurrentState : GameState.Playing;
        ShowPanel(startState);
        // Force the time label immediately so it shows "0:00.0" and not stale text from the previous run.
        _timeUpdateTimer = 0f;
        if (_timeLabel != null && _scoreService != null)
        {
            float t = _scoreService.SurvivalTime;
            int   m = Mathf.FloorToInt(t / 60f);
            int   s = Mathf.FloorToInt(t % 60f);
            _timeLabel.SetText(_timeBuffer, 0, WriteTimeToBuffer(m, s, 0));
        }
    }

    [Inject]
    private void Construct(ParticipantScoreService scoreService = null)
    {
        _scoreService = scoreService;
    }

    private void Update()
    {
        // Rebuild the time string every 0.1 s so tenths-of-a-second feel responsive,
        // without allocating a string every single frame.
        if (GameManager.Instance != null
            && GameManager.Instance.CurrentState == GameState.Playing
            && _timeLabel != null
            && _scoreService != null)
        {
            _timeUpdateTimer -= Time.deltaTime;
            if (_timeUpdateTimer <= 0f)
            {
                _timeUpdateTimer = 0.1f;
                float t = _scoreService.SurvivalTime;
                int   m = Mathf.FloorToInt(t / 60f);
                int   s = Mathf.FloorToInt(t % 60f);
                int   d = Mathf.FloorToInt((t % 1f) * 10f);
                _timeLabel.SetText(_timeBuffer, 0, WriteTimeToBuffer(m, s, d));
            }
        }
    }

    /// <summary>
    /// Writes m:ss.d into _timeBuffer (after the prefix) with zero allocations.
    /// Returns the total char count written (prefix + time chars).
    /// </summary>
    private int WriteTimeToBuffer(int m, int s, int d)
    {
        int i = _timePrefixLen;
        // Minutes (1â€“2 digits)
        if (m >= 10) _timeBuffer[i++] = (char)('0' + m / 10);
        _timeBuffer[i++] = (char)('0' + m % 10);
        _timeBuffer[i++] = ':';
        // Seconds (always 2 digits)
        _timeBuffer[i++] = (char)('0' + s / 10);
        _timeBuffer[i++] = (char)('0' + s % 10);
        _timeBuffer[i++] = '.';
        // Tenths
        _timeBuffer[i++] = (char)('0' + d);
        return i;
    }

    private void OnDestroy()
    {
        // Guard: GameManager may have already cleared its Instance in its own OnDestroy.
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged.RemoveListener(OnGameStateChanged);

        _scoreService?.OnPointsChanged.RemoveListener(OnScoreUpdated);

        _restartButton?.onClick.RemoveListener(OnRestartClicked);
        _mainMenuButton?.onClick.RemoveListener(OnMainMenuClicked);
        _settingsButton?.onClick.RemoveListener(OnSettingsClicked);
    }

    public void OnGameStateChanged(GameState state)
    {
        ShowPanel(state);
    }

    public void OnScoreUpdated(int score)
    {
        if (_scoreLabel != null)
            _scoreLabel.text = $"{_scorePrefix}{score}";
    }

    private void ShowPanel(GameState state)
    {
        _timeUpdateTimer = 0f; // reset so time label refreshes immediately on game start/restart
        _showingHUD = state == GameState.Playing || state == GameState.Dead;
        _hudPanel?.SetActive(_showingHUD);
        _gameOverPanel?.SetActive(state == GameState.GameOver);

        // Dismiss the settings panel if it is open when game over triggers â€” prevents UI overlap.
        if (state == GameState.GameOver)
            _settingsScreen?.Close();

        // Hide settings button when not on HUD (e.g. game over screen)
        if (_settingsButton != null)
            _settingsButton.gameObject.SetActive(_showingHUD);

        if (state == GameState.GameOver && _scoreService != null)
        {
            int points       = _scoreService.PointsCollected;
            float survivedSecs = _scoreService.SurvivalTime;
            string time      = $"{Mathf.FloorToInt(survivedSecs / 60f)}:{Mathf.FloorToInt(survivedSecs % 60f):00}";
            int bestPoints   = _scoreService.HighScorePoints;
            float bestSecs   = _scoreService.HighScoreTime;
            string bestTime  = $"{Mathf.FloorToInt(bestSecs / 60f)}:{Mathf.FloorToInt(bestSecs % 60f):00}";

            if (_finalScoreLabel != null)
                _finalScoreLabel.text = $"You collected {points} point{(points == 1 ? "" : "s")}\nin {time}";
            if (_highScoreLabel != null)
                _highScoreLabel.text = bestPoints > 0
                    ? $"Best: {bestPoints} point{(bestPoints == 1 ? "" : "s")} in {bestTime}"
                    : "No best run yet";
        }
    }

    private void OnRestartClicked()  { GameManager.Instance?.RestartGame(); }
    private void OnMainMenuClicked() { GameManager.Instance?.GoToMainMenu(); }
    private void OnSettingsClicked() { _settingsScreen?.Open(); }
}
}
