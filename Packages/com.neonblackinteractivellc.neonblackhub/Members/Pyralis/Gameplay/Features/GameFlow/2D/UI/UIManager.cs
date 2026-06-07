using TMPro;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Settings;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VContainer;

namespace NeonBlack.Gameplay.Features.GameFlow
{
/// <summary>
/// Manages all UI panels: HUD (live score + time) and Game Over screen.
/// Subscribes to an explicit gameplay session flow and session score service.
/// Setup: Attach to a GameObject on the UI Canvas.
/// Wire all panel GameObjects, TMP labels, and Buttons in the Inspector.
/// Canvas must be Screen Space - Overlay with a PhysicsRaycaster for button input.
/// </summary>
[DefaultExecutionOrder(-10)]
public class UIManager : MonoBehaviour
{
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

    [Header("Runtime Services")]
    [SerializeField, Tooltip("Session flow that drives HUD/game-over state and restart/menu commands. GameManager implements IGameplaySessionFlow, or assign a custom session flow.")]
    private MonoBehaviour _gameplaySessionSource;

    [SerializeField, Tooltip("Score service for HUD labels and game-over totals. ParticipantScoreService implements ISessionScoreService, or assign a custom score service.")]
    private MonoBehaviour _scoreServiceSource;

    [Header("HUD Format")]
    [SerializeField, Tooltip("Prefix shown before the formatted time value in the HUD (e.g. 'Time: ').")]
    private string _timePrefix = "Time: ";
    [SerializeField, Tooltip("Prefix shown before the score count in the HUD (e.g. 'Points: ').")]
    [FormerlySerializedAs("_crumbsPrefix")]
    private string _scorePrefix = "Points: ";

    private ISessionScoreService _scoreService;
    private IGameplaySessionFlow _gameplaySession;
    private bool  _showingHUD;
    private float _timeUpdateTimer;
    // Pre-allocated buffer for zero-GC time string writes.
    // Layout: [prefix chars][m digit(s)][':'][s tens][s ones]['.'][d]
    // Rebuilt each tick by writing directly into the char array with no string allocations.
    private char[] _timeBuffer;
    private int    _timePrefixLen;

    private void Awake()
    {
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
        ResolveRuntimeServices();

        _restartButton?.onClick.AddListener(OnRestartClicked);
        _mainMenuButton?.onClick.AddListener(OnMainMenuClicked);
        _settingsButton?.onClick.AddListener(OnSettingsClicked);

        if (_gameplaySession != null)
            _gameplaySession.AddGameStateChangedListener(OnGameStateChanged);
        else if (RequiresGameplaySessionFlow())
            Debug.LogError("[UIManager] Gameplay Session Source is not configured. Assign a component that implements IGameplaySessionFlow.", this);

        if (_scoreService != null)
        {
            _scoreService.AddPointsChangedListener(OnScoreUpdated);
            // The session may reset score before we subscribe.
            // Push the current values now so labels are correct from frame one.
            OnScoreUpdated(_scoreService.PointsCollected);
        }

        // Sync panels to whatever state already exists
        GameState startState = _gameplaySession != null ? _gameplaySession.CurrentState : GameState.Playing;
        ShowPanel(startState);
        // Force the time label immediately so it shows "0:00.0" and not stale text from the previous run.
        _timeUpdateTimer = 0f;
        if (_gameplaySession != null && _timeLabel != null && _scoreService != null)
        {
            float t = _scoreService.SurvivalTime;
            int   m = Mathf.FloorToInt(t / 60f);
            int   s = Mathf.FloorToInt(t % 60f);
            _timeLabel.SetText(_timeBuffer, 0, WriteTimeToBuffer(m, s, 0));
        }
    }

    [Inject]
    private void Construct(ISessionScoreService scoreService = null, IGameplaySessionFlow gameplaySession = null)
    {
        if (scoreService != null)
            _scoreService = scoreService;
        if (gameplaySession != null)
            _gameplaySession = gameplaySession;
    }

    private void Update()
    {
        // Rebuild the time string every 0.1 s so tenths-of-a-second feel responsive,
        // without allocating a string every single frame.
        if (_gameplaySession != null
            && _gameplaySession.IsGameplayActive
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
        _gameplaySession?.RemoveGameStateChangedListener(OnGameStateChanged);

        _scoreService?.RemovePointsChangedListener(OnScoreUpdated);

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
        ResolveRuntimeServices();

        _timeUpdateTimer = 0f; // reset so time label refreshes immediately on game start/restart
        _showingHUD = state == GameState.Playing || state == GameState.Dead;
        if (_hudPanel != null)
            _hudPanel.SetActive(_showingHUD);
        if (_gameOverPanel != null)
            _gameOverPanel.SetActive(state == GameState.GameOver);

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

    private void OnRestartClicked()  { _gameplaySession?.RestartGame(); }
    private void OnMainMenuClicked() { _gameplaySession?.GoToMainMenu(); }
    private void OnSettingsClicked() { _settingsScreen?.Open(); }

    private bool RequiresGameplaySessionFlow()
    {
        return _gameOverPanel != null
            || _finalScoreLabel != null
            || _highScoreLabel != null
            || _restartButton != null
            || _mainMenuButton != null
            || _settingsButton != null
            || _settingsScreen != null;
    }

    private ISessionScoreService ResolveScoreService()
    {
        if (_scoreService == null && _scoreServiceSource != null)
        {
            _scoreService = _scoreServiceSource as ISessionScoreService;
            if (_scoreService == null)
                _scoreService = _scoreServiceSource.GetComponent<ISessionScoreService>();
        }

        return _scoreService;
    }

    private void ResolveRuntimeServices()
    {
        if (_gameplaySession == null && _gameplaySessionSource != null)
        {
            _gameplaySession = _gameplaySessionSource as IGameplaySessionFlow;
            if (_gameplaySession == null)
                _gameplaySession = _gameplaySessionSource.GetComponent<IGameplaySessionFlow>();
        }

        ResolveScoreService();
    }
}
}
