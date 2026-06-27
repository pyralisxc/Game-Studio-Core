using System.Collections;
using NeonBlack.Gameplay.Glue.Session;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Modules.Character;
using NeonBlack.Gameplay.Modules.Hazards;
using NeonBlack.Gameplay.Modules.Pickups;
using NeonBlack.Gameplay.Modules.Scoring;
using UnityEngine;
using UnityEngine.Events;

namespace NeonBlack.Gameplay.Glue.SceneFlow.Arcade2D
{
/// <summary>
/// Central game orchestrator for the current 2D score-loop runtime.
/// </summary>
[AuthoringContract(
    Capability = AuthoringCapability.Setup | AuthoringCapability.Session,
    Relevance = "2D arcade flow orchestrator; coordinates scoring, difficulty, hazards, pickups, and arcade states while SessionStateService owns shared gameplay-active state.",
    Axioms = AuthoringWorldAxiom.Dimensions2D,
    RequiredInterfaces = new[] { typeof(IGameplaySessionFlow), typeof(IHazardOutcomeSink) },
    NativeSetup = new[] 
    { 
        "Wire system references (Score, Hazards, Pickups, etc.).",
        "For participant-spawned pawns, let the roster provide active controllers. Use explicit Player Controllers only for intentionally standalone scene-authored tests."
    },
    Proof = "Start the game and verify the session initializes and transitions to the Playing state.",
    AssignmentFields = new[] { nameof(scoreManager), nameof(hazardSpawner), nameof(pickupSpawner), nameof(difficultyManager) },
    ExpertAdvice = "The GameManager is the 2D arcade orchestrator. SessionStateService remains the normal IGameplayStateReader for movement/input/spawner activity. Prefer participant roster pawns for active players; use explicit Player Controllers only for intentionally standalone scene-authored tests.",
    DocumentationURL = "https://docs.neonblack.com/pyralis/gameflow",
    CapabilityPath = "Core Setup/Session/Game Manager",
    Surface = AuthoringContractSurface.SetupOnly
)]
[AddComponentMenu("NeonBlack/Gameplay/Glue/SceneFlow/Arcade2D/Game Manager")]
[DefaultExecutionOrder(-20)]
public partial class GameManager : MonoBehaviour,
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

    [SerializeField, Tooltip("Seconds to wait for the death animation before hiding the player.")]
    private float deathAnimDuration = 0.5f;

    [Header("Events")]
    public UnityEvent<GameState> OnGameStateChanged;

    private ILeaderboardService _leaderboardService;
    private GameState _currentState;

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

    private void Awake()
    {
        scoreManager ??= GetComponent<ParticipantScoreService>();
        difficultyManager ??= GetComponent<DifficultyManager>();

        RefreshTrackedPlayers(includeInactive: true);
        ConfigureRuntimeDependencies();
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

}
}
