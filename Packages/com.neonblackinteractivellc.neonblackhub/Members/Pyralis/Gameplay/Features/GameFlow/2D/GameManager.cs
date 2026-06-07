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

public interface IGameplaySessionFlow : IGameplayStateReader
{
    GameState CurrentState { get; }

    void AddGameStateChangedListener(UnityAction<GameState> listener);
    void RemoveGameStateChangedListener(UnityAction<GameState> listener);
    void RestartGame();
    void GoToMainMenu();
}

/// <summary>
/// Central game orchestrator for the current 2D score-loop runtime.
/// Uses explicit scene references plus the participant roster rather than
/// scanning the scene for runtime wiring.
/// </summary>
[AddComponentMenu("NeonBlack/Gameplay/Game Flow/2D Game Manager")]
[DefaultExecutionOrder(-20)]
public class GameManager : MonoBehaviour
    , IGameplayStateReader
    , IGameplaySessionFlow
    , IHazardOutcomeSink
{
    public static GameManager Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
    }

    [Header("System References")]
    [SerializeField, Tooltip("ParticipantScoreService colocated with this manager or explicitly assigned.")]
    private ParticipantScoreService scoreManager;

    [SerializeField, Tooltip("HazardSpawner for this scene.")]
    private HazardSpawner hazardSpawner;

    [SerializeField, Tooltip("Pickup spawner for this scene. Required.")]
    private CollectibleSpawner2D pickupSpawner;

    [SerializeField, Tooltip("DifficultyManager for this scene.")]
    private DifficultyManager difficultyManager;

    [SerializeField, Tooltip("Camera bounds provider for the 2D playfield, usually CinemachineCameraRigController.")]
    private MonoBehaviour cameraBoundsSource;

    [SerializeField, Tooltip("Scene transition service used by restart and main-menu navigation. SceneFader and SceneLoader implement ISceneNavigator.")]
    private MonoBehaviour sceneNavigatorSource;

    [SerializeField, Tooltip("Settings service saved before restart or menu navigation. SettingsManager implements IGameplaySettingsApplier.")]
    private MonoBehaviour settingsSource;

    [Header("Scene Names")]
    [SerializeField, Tooltip("Exact name of the main menu scene as listed in Build Settings.")]
    private string mainMenuSceneName = SceneNames.MainMenu;

    [Header("Levels")]
    [SerializeField, Tooltip("LevelRegistry asset. Required for random restart mode.")]
    private LevelRegistry levelRegistry;

    [Header("Player")]
    [SerializeField, Tooltip("Primary player GameObject reference.")]
    private GameObject player;

    [SerializeField, Tooltip("Primary 2D motor reference.")]
    private Motor2D primaryPlayerController;

    [SerializeField, Tooltip("Optional explicit player list for local multiplayer scenes. When empty, active roster pawns are used.")]
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
    private GameState _currentState;

    public GameState CurrentState => _currentState;
    public bool IsGameplayActive => _currentState == GameState.Playing;

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
        ISceneNavigator sceneNavigator = null,
        IGameplaySettingsApplier settings = null)
    {
        _participantRosterService = participantRosterService;
        _leaderboardService = leaderboardService;
        if (sceneNavigator != null)
            _sceneNavigator = sceneNavigator;
        if (settings != null)
            _settings = settings;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        scoreManager ??= GetComponent<ParticipantScoreService>();
        difficultyManager ??= GetComponent<DifficultyManager>();
        _cameraBoundsProvider = ResolveCameraBoundsProvider();
        ResolveSceneNavigator();
        ResolveSettings();

        if (player != null && primaryPlayerController == null)
            primaryPlayerController = ResolvePlayerController(player);

        RefreshTrackedPlayers(includeInactive: true);
        ConfigureRuntimeDependencies();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
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
        PlayerDied(primaryPlayerController);
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
        Motor2D deadPlayer = ResolvePlayerController(target);
        if (deadPlayer == null)
            deadPlayer = target != null ? target.GetComponentInParent<Motor2D>() : null;

        if (deadPlayer == null)
            return false;

        PlayerDied(deadPlayer);
        return true;
    }

    public void RestartGame()
    {
        ResolveSettings()?.Save();

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
        ResolveSettings()?.Save();
        LoadScene(mainMenuSceneName);
    }

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, deathAnimDuration));
        scoreManager?.SaveHighScore();
        pickupSpawner?.ClearAllCollectibles();
        hazardSpawner?.ClearAllHazards();
        _leaderboardService?.SubmitScore(scoreManager != null ? scoreManager.PointsCollected : 0);
        SetState(GameState.GameOver);
    }

    private void SetState(GameState state)
    {
        _currentState = state;
        OnGameStateChanged?.Invoke(state);
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

        if (primaryPlayerController == null && _trackedPlayerControllers.Count > 0)
            primaryPlayerController = _trackedPlayerControllers[0];

        if (player == null && primaryPlayerController != null)
            player = primaryPlayerController.gameObject;
    }

    private void RegisterRosterPlayers(bool includeInactive)
    {
        if (_participantRosterService == null)
        {
            if (player != null)
                RegisterTrackedPlayer(ResolvePlayerController(player), includeInactive);
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
        movement?.ConfigureRuntime(this, _cameraBoundsProvider);

        PlayerInputHandler inputHandler = controller.GetComponent<PlayerInputHandler>();
        inputHandler?.ConfigureRuntime(this);

        StillnessBonus2D stillnessBonus = controller.GetComponent<StillnessBonus2D>();
        stillnessBonus?.ConfigureRuntime(this, scoreManager);
    }

    private static Motor2D ResolvePlayerController(GameObject playerObject)
    {
        return playerObject != null
            ? playerObject.GetComponent<Motor2D>()
            : null;
    }

    private void ConfigureRuntimeDependencies()
    {
        pickupSpawner?.ConfigureRuntime(this, _cameraBoundsProvider);
        hazardSpawner?.ConfigureRuntime(this, _cameraBoundsProvider, this, pickupSpawner);
    }

    private ICameraBoundsProvider ResolveCameraBoundsProvider()
    {
        if (cameraBoundsSource is ICameraBoundsProvider provider)
            return provider;

        return null;
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

        ISceneNavigator navigator = ResolveSceneNavigator();
        if (navigator != null)
        {
            navigator.LoadScene(sceneName);
            return;
        }

        Debug.LogError("[GameManager] Scene Navigator Source is not configured. Assign SceneFader, SceneLoader, or another ISceneNavigator.", this);
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

    private IGameplaySettingsApplier ResolveSettings()
    {
        if (_settings != null)
            return _settings;

        if (settingsSource == null)
            return null;

        _settings = settingsSource as IGameplaySettingsApplier;
        if (_settings == null)
            _settings = settingsSource.GetComponent<IGameplaySettingsApplier>();

        return _settings;
    }
}
}
