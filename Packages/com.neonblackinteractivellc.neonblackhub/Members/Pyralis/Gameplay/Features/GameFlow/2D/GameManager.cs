using System.Collections;
using System.Collections.Generic;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Features.Hazards;
using NeonBlack.Gameplay.Features.Input;
using NeonBlack.Gameplay.Features.Pickups;
using NeonBlack.Gameplay.Core.Navigation;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Features.Scoring;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using VContainer;

namespace NeonBlack.Gameplay.Features.GameFlow
{
public enum GameState { Playing, Dead, GameOver }

public interface IGameplaySessionFlow
{
    GameState CurrentState { get; }

    void AddGameStateChangedListener(UnityAction<GameState> listener);
    void RemoveGameStateChangedListener(UnityAction<GameState> listener);
    void RestartGame();
    void GoToMainMenu();
}

/// <summary>
/// Central game orchestrator for the current 2D score-loop runtime.
/// </summary>
[AuthoringContract(
    Capability = AuthoringCapability.Setup | AuthoringCapability.Session,
    Relevance = "2D arcade flow orchestrator; coordinates scoring, difficulty, hazards, pickups, and arcade states while SessionStateService owns shared gameplay-active state.",
    Axioms = AuthoringWorldAxiom.Dimensions2D,
    RequiredInterfaces = new[] { typeof(IGameplaySessionFlow), typeof(IHazardOutcomeSink) },
    RequiredComponents = new[] { typeof(GameManager) },
    NativeSetup = new[] 
    { 
        "Add GameManager to the scene.",
        "Wire system references (Score, Hazards, Pickups, etc.).",
        "For participant-spawned pawns, let the roster provide active controllers. Use Player Controllers only for standalone compatibility scenes."
    },
    FirstProof = "Start the game and verify the session initializes and transitions to the Playing state."
,
        AssignmentFields = new[] { nameof(scoreManager), nameof(hazardSpawner), nameof(pickupSpawner), nameof(difficultyManager), nameof(playerControllers) },
        ExpertAdvice = "The GameManager is the 2D arcade orchestrator. SessionStateService remains the normal IGameplayStateReader for movement/input/spawner activity. Prefer participant roster pawns for active players; use explicit Player Controllers only for hand-authored standalone compatibility scenes.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/gameflow")]
[AddComponentMenu("NeonBlack/Gameplay/Game Flow/2D Game Manager")]
[DefaultExecutionOrder(-20)]
public class GameManager : MonoBehaviour,
    IGameplaySessionFlow,
    IHazardOutcomeSink
{
    [Header("System References")]
    [SerializeField, Tooltip("ParticipantScoreService colocated with this manager or explicitly assigned.")]
    private ParticipantScoreService scoreManager;

    [SerializeField, Tooltip("HazardSpawner for this scene.")]
    private HazardSpawner hazardSpawner;

    [SerializeField, Tooltip("Pickup spawner for this scene. Required.")]
    private CollectibleSpawner2D pickupSpawner;

    [SerializeField, Tooltip("DifficultyManager for this scene.")]
    private DifficultyManager difficultyManager;

    [Header("Scene Names")]
    [SerializeField, Tooltip("Exact name of the main menu scene as listed in Build Settings.")]
    private string mainMenuSceneName = SceneNames.MainMenu;

    [Header("Levels")]
    [SerializeField, Tooltip("LevelRegistry asset. Required for random restart mode.")]
    private LevelRegistry levelRegistry;

    [Header("Standalone Compatibility")]
    [SerializeField, Tooltip("Optional explicit 2D controller list for standalone scenes that do not use participant spawning. Participant-spawned scenes should leave this empty and use the roster.")]
    private Motor2D[] playerControllers;

    [SerializeField, Tooltip("Seconds to wait for the death animation before hiding the player.")]
    private float deathAnimDuration = 0.5f;

    [Header("Events")]
    public UnityEvent<GameState> OnGameStateChanged;

    private readonly List<Motor2D> _trackedPlayerControllers = new List<Motor2D>(8);
    private readonly Dictionary<Motor2D, Vector3> _playerStartPositions = new Dictionary<Motor2D, Vector3>();
    private ParticipantRosterService _participantRosterService;
    private ILeaderboardService _leaderboardService;
    private ICameraBoundsProvider _cameraBoundsProvider;
    private ISceneNavigator _sceneNavigator;
    private IGameplaySettingsApplier _settings;
    private IGameplayStateReader _gameplayStateReader;
    private SessionStateService _sessionStateService;
    private GameState _currentState;
    private Motor2D _primaryPlayerController;
    private readonly ArcadeFlowStateReader _standaloneStateReader = new ArcadeFlowStateReader();

    public GameState CurrentState => _currentState;
    public void AddGameStateChangedListener(UnityAction<GameState> listener)
    {
        if (listener != null)
            OnGameStateChanged.AddListener(listener);
    }

    public void RemoveGameStateChangedListener(UnityAction<GameState> listener)
    {
        if (listener != null)
            OnGameStateChanged.RemoveListener(listener);
    }

    [Inject]
    private void Construct(
        ParticipantRosterService participantRosterService = null,
        ILeaderboardService leaderboardService = null,
        ICameraBoundsProvider cameraBoundsProvider = null,
        ISceneNavigator sceneNavigator = null,
        IGameplaySettingsApplier settings = null,
        IGameplayStateReader gameplayStateReader = null,
        SessionStateService sessionStateService = null)
    {
        _participantRosterService = participantRosterService;
        _leaderboardService = leaderboardService;
        if (cameraBoundsProvider != null)
            _cameraBoundsProvider = cameraBoundsProvider;
        if (sceneNavigator != null)
            _sceneNavigator = sceneNavigator;
        if (settings != null)
            _settings = settings;
        if (gameplayStateReader != null)
            _gameplayStateReader = gameplayStateReader;
        if (sessionStateService != null)
            _sessionStateService = sessionStateService;
    }

    private void Awake()
    {
        scoreManager ??= GetComponent<ParticipantScoreService>();
        difficultyManager ??= GetComponent<DifficultyManager>();

        RefreshTrackedPlayers(includeInactive: true);
        ConfigureRuntimeDependencies();
    }

    private void OnDestroy()
    {
    }

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        if (scoreManager == null)
        {
            Debug.LogError("[GameManager] scoreManager is not assigned in the Inspector.", this);
            return;
        }

        if (difficultyManager == null)
        {
            Debug.LogError("[GameManager] difficultyManager is not assigned in the Inspector.", this);
            return;
        }

        if (pickupSpawner == null)
        {
            Debug.LogError("[GameManager] pickupSpawner is not assigned in the Inspector.", this);
            return;
        }

        if (hazardSpawner == null)
        {
            Debug.LogError("[GameManager] hazardSpawner is not assigned in the Inspector.", this);
            return;
        }

        RefreshTrackedPlayers(includeInactive: true);
        ConfigureRuntimeDependencies();

        for (int i = 0; i < _trackedPlayerControllers.Count; i++)
        {
            Motor2D playerController = _trackedPlayerControllers[i];
            if (playerController == null)
                continue;

            if (_playerStartPositions.TryGetValue(playerController, out Vector3 startPosition))
            {
                playerController.gameObject.SetActive(true);
                playerController.ResetForRound(startPosition);
            }
        }

        scoreManager.ResetScore();
        difficultyManager.ResetDifficulty();
        pickupSpawner.SpawnInitialCollectibles();
        hazardSpawner.StartSpawning();
        SetState(GameState.Playing);
    }

    public void PlayerDied()
    {
        PlayerDied(_primaryPlayerController);
    }

    public void PlayerDied(Motor2D deadPlayer)
    {
        if (_currentState != GameState.Playing || deadPlayer == null || deadPlayer.IsDead)
            return;

        deadPlayer.PlayDeathAnimation();
        RefreshTrackedPlayers(includeInactive: true);

        if (!AreAllTrackedPlayersDead())
            return;

        hazardSpawner?.StopSpawning();
        difficultyManager?.StopDifficulty();
        scoreManager?.StopTimer();
        SetState(GameState.Dead);
        StartCoroutine(GameOverRoutine());
    }

    public bool TryHandleHazardImpact(GameObject target, GameObject source, Vector3 hitPoint)
    {
        Motor2D deadPlayer = target != null ? target.GetComponentInParent<Motor2D>() : null;

        if (deadPlayer == null)
            return false;

        PlayerDied(deadPlayer);
        return true;
    }

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

    private static readonly Dictionary<float, WaitForSecondsRealtime> _realtimeWaitPool = new Dictionary<float, WaitForSecondsRealtime>();

    private static WaitForSecondsRealtime GetWaitRealtime(float seconds)
    {
        seconds = (float)System.Math.Round(seconds, 2);
        if (!_realtimeWaitPool.TryGetValue(seconds, out var wait))
        {
            wait = new WaitForSecondsRealtime(seconds);
            _realtimeWaitPool[seconds] = wait;
        }
        return wait;
    }

    private IEnumerator GameOverRoutine()
    {
        yield return GetWaitRealtime(Mathf.Max(0.1f, deathAnimDuration));
        scoreManager?.SaveHighScore();
        pickupSpawner?.ClearAllCollectibles();
        hazardSpawner?.ClearAllHazards();
        _leaderboardService?.SubmitScore(scoreManager != null ? scoreManager.PointsCollected : 0);
        SetState(GameState.GameOver);
    }

    private void SetState(GameState state)
    {
        _currentState = state;
        ApplySessionPhase(state);
        OnGameStateChanged?.Invoke(state);
    }

    private void ApplySessionPhase(GameState state)
    {
        if (_sessionStateService == null)
            return;

        _sessionStateService.SetPhase(state == GameState.Playing
            ? SessionStateService.SessionPhase.Gameplay
            : SessionStateService.SessionPhase.Results);
    }

    private bool AreAllTrackedPlayersDead()
    {
        bool foundAnyPlayer = false;
        for (int i = 0; i < _trackedPlayerControllers.Count; i++)
        {
            Motor2D playerController = _trackedPlayerControllers[i];
            if (playerController == null)
                continue;

            foundAnyPlayer = true;
            if (!playerController.IsDead)
                return false;
        }

        return foundAnyPlayer;
    }

    private void RefreshTrackedPlayers(bool includeInactive)
    {
        _trackedPlayerControllers.Clear();

        if (playerControllers != null && playerControllers.Length > 0)
        {
            for (int i = 0; i < playerControllers.Length; i++)
                RegisterTrackedPlayer(playerControllers[i], includeInactive);
        }
        else
        {
            RegisterRosterPlayers(includeInactive);
        }

        _primaryPlayerController = _trackedPlayerControllers.Count > 0 ? _trackedPlayerControllers[0] : null;
    }

    private void RegisterRosterPlayers(bool includeInactive)
    {
        if (_participantRosterService == null)
        {
            return;
        }

        for (int i = 0; i < _participantRosterService.Participants.Count; i++)
        {
            ParticipantHandle participant = _participantRosterService.Participants[i];
            if (participant?.PawnInstance == null)
                continue;

            if (!includeInactive && !participant.PawnInstance.activeInHierarchy)
                continue;

            RegisterTrackedPlayer(participant.PawnInstance.GetComponent<Motor2D>(), includeInactive);
        }
    }

    private void RegisterTrackedPlayer(Motor2D controller, bool includeInactive)
    {
        if (controller == null || _trackedPlayerControllers.Contains(controller))
            return;

        if (!includeInactive && !controller.gameObject.activeInHierarchy)
            return;

        _trackedPlayerControllers.Add(controller);
        if (!_playerStartPositions.ContainsKey(controller))
            _playerStartPositions[controller] = controller.transform.position;

        Pawn2DMovementComponent movement = controller.GetComponent<Pawn2DMovementComponent>();
        movement?.ConfigureRuntime(ResolveGameplayStateReader(), _cameraBoundsProvider);

        PlayerInputHandler inputHandler = controller.GetComponent<PlayerInputHandler>();
        inputHandler?.ConfigureRuntime(ResolveGameplayStateReader());

        StillnessBonus2D stillnessBonus = controller.GetComponent<StillnessBonus2D>();
        stillnessBonus?.ConfigureRuntime(ResolveGameplayStateReader(), scoreManager);
    }

    private void ConfigureRuntimeDependencies()
    {
        IGameplayStateReader stateReader = ResolveGameplayStateReader();
        pickupSpawner?.ConfigureRuntime(stateReader, _cameraBoundsProvider);
        hazardSpawner?.ConfigureRuntime(stateReader, _cameraBoundsProvider, this, pickupSpawner);
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
            Debug.LogError("[GameManager] Scene name is blank.", this);
            return;
        }

        if (_sceneNavigator != null)
        {
            _sceneNavigator.LoadScene(sceneName);
            return;
        }

        Debug.LogError("[GameManager] Scene Navigator is not injected. Ensure ISceneNavigator is registered in the LifetimeScope.", this);
    }

    private IGameplayStateReader ResolveGameplayStateReader()
    {
        if (_gameplayStateReader != null)
            return _gameplayStateReader;

        if (_sessionStateService != null)
            return _sessionStateService;

        _standaloneStateReader.Owner = this;
        return _standaloneStateReader;
    }

    private sealed class ArcadeFlowStateReader : IGameplayStateReader
    {
        public GameManager Owner { get; set; }
        public bool IsGameplayActive => Owner != null && Owner.CurrentState == GameState.Playing;
    }
}
}
