using System.Collections;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Runtime;
using NeonBlack.Gameplay.Characters;
using TMPro;
using UnityEngine;
using VContainer;

namespace NeonBlack.Gameplay.Features.Respawn
{
/// <summary>
/// Handles player death and respawning.
///
/// Setup:
///   1. Add this component to any empty GameObject in the scene (e.g. "PlayerSpawner").
///   2. Drag your Player prefab into Player Prefab, or drag the existing Player scene object into Current Player.
///      If Current Player is set the spawner reuses that object. If only Player Prefab is set the spawner instantiates a fresh copy.
///   3. Add one or more Spawn Points. Leave empty to spawn at this object's own position.
///   4. Tune the timing and optional respawn shield fields.
///
/// When participant infrastructure is present, the spawner tracks one authored seat and respawns that
/// participant's pawn instead of assuming a single global player.
/// </summary>
public class PlayerSpawner : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Drag the player prefab here. Used to spawn a fresh copy if no tracked scene object is set.")]
    [SerializeField] private GameObject playerPrefab;

    [Tooltip("Drag the scene player object here to reuse it on respawn instead of instantiating.")]
    [SerializeField] private GameObject currentPlayer;

    [Header("Spawn Points")]
    [Tooltip("Possible spawn locations. The spawner picks the first point by default, or randomly if Randomise Spawn Point is on.")]
    [SerializeField] private Transform[] spawnPoints;
    [Tooltip("Pick a random spawn point each time instead of always using the first one.")]
    [SerializeField] private bool randomiseSpawnPoint;

    [Header("Timing")]
    [Tooltip("Seconds between the player dying and being revived.")]
    [SerializeField] private float respawnDelay = 3f;
    [Tooltip("If > 0, the player is invincible for this many seconds after respawning.")]
    [SerializeField] private float respawnShield = 2f;

    [Header("Lives (0 = infinite)")]
    [Tooltip("Number of lives the player starts with. 0 means unlimited.")]
    [SerializeField] private int startingLives;

    [Header("Respawn Countdown")]
    [SerializeField] private bool showCountdown = true;
    [SerializeField] private string countdownFormat = "Respawning in {0:0}...";
    [SerializeField] private float countdownFontSize = 48f;
    [SerializeField] private Color countdownColor = Color.white;

    [Header("Respawn HP")]
    [Range(0.01f, 1f)]
    [SerializeField] private float respawnHpFraction = 1f;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent OnBeforeRespawn;
    public UnityEngine.Events.UnityEvent OnAfterRespawn;
    public UnityEngine.Events.UnityEvent OnGameOver;

    public int LivesRemaining { get; private set; }
    public bool IsRespawning { get; private set; }
    public bool IsGameOver { get; private set; }

    private HealthComponent _health;
    private GameObject _countdownCanvas;
    private TextMeshProUGUI _countdownLabel;

    [Header("Participant Infrastructure (optional)")]
    [Tooltip("When present, respawn uses ParticipantSpawnService instead of raw Instantiate().")]
    [SerializeField] private ParticipantSpawnService participantSpawnService;
    [Tooltip("When present, respawn resolves the tracked participant from the active roster instead of assuming one player.")]
    [SerializeField] private ParticipantRosterService rosterService;
    [Tooltip("Seat index this spawner should track when participant infrastructure is active.")]
    [SerializeField] private int targetSeatIndex;
    private IPlayerProvider _playerProvider;

    private void Awake()
    {
        LivesRemaining = startingLives;
        ResolveParticipantServices();
        currentPlayer = ResolveTrackedPlayer();

        if (currentPlayer == null)
        {
            if (playerPrefab != null)
            {
                currentPlayer = Instantiate(playerPrefab, GetSpawnPosition(), Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("[PlayerSpawner] No player reference found. Assign Player Prefab or Current Player in the Inspector.");
                return;
            }
        }

        SubscribeToPlayer(currentPlayer);
    }

    [Inject]
    private void Construct(
        ParticipantSpawnService injectedSpawnService = null,
        IParticipantRoster injectedRoster = null,
        IPlayerProvider playerProvider = null)
    {
        participantSpawnService ??= injectedSpawnService;
        rosterService ??= injectedRoster as ParticipantRosterService;
        _playerProvider = playerProvider;
    }

    private void OnDestroy()
    {
        UnsubscribeFromPlayer();
        DestroyCountdownUI();
    }

    private void ResolveParticipantServices()
    {
        if (participantSpawnService == null)
        {
            if (GameplayPlatformContext.TryResolve(out ParticipantSpawnService resolvedSpawnService))
                participantSpawnService = resolvedSpawnService;
            else
                participantSpawnService = ResolveHierarchyComponent<ParticipantSpawnService>();
        }

        if (rosterService == null)
        {
            if (GameplayPlatformContext.TryResolve(out ParticipantRosterService resolvedRosterService))
                rosterService = resolvedRosterService;
            else if (GameplayPlatformContext.TryResolve(out IParticipantRoster resolvedRoster))
                rosterService = resolvedRoster as ParticipantRosterService;

            rosterService ??= ResolveHierarchyComponent<ParticipantRosterService>();
        }

        if (_playerProvider == null)
        {
            if (GameplayPlatformContext.TryResolve(out IPlayerProvider resolvedPlayerProvider))
                _playerProvider = resolvedPlayerProvider;
            else
                _playerProvider = rosterService;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.9f);
        if (spawnPoints != null)
        {
            foreach (Transform point in spawnPoints)
            {
                if (point == null)
                    continue;

                Gizmos.DrawWireSphere(point.position, 0.35f);
                Gizmos.DrawLine(transform.position, point.position);
            }
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, 0.35f);
        }
    }
#endif

    private void SubscribeToPlayer(GameObject player)
    {
        UnsubscribeFromPlayer();
        currentPlayer = player;
        if (player == null)
            return;

        _health = player.GetComponentInChildren<HealthComponent>();
        if (_health == null)
        {
            Debug.LogWarning("[PlayerSpawner] Player has no HealthComponent and death cannot be detected.");
            return;
        }

        _health.OnDeath.AddListener(HandlePlayerDeath);
    }

    private void UnsubscribeFromPlayer()
    {
        if (_health != null)
            _health.OnDeath.RemoveListener(HandlePlayerDeath);

        _health = null;
    }

    private void HandlePlayerDeath()
    {
        if (IsGameOver)
            return;

        if (startingLives > 0)
        {
            LivesRemaining--;
            if (LivesRemaining <= 0)
            {
                IsGameOver = true;
                OnGameOver?.Invoke();
                DisablePlayer(currentPlayer);
                return;
            }
        }

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        IsRespawning = true;
        currentPlayer = ResolveTrackedPlayer();
        DisablePlayer(currentPlayer);

        if (showCountdown)
        {
            BuildCountdownUI();
            float timer = respawnDelay;
            _countdownLabel.gameObject.SetActive(true);
            while (timer > 0f)
            {
                _countdownLabel.text = string.Format(countdownFormat, Mathf.Ceil(timer));
                yield return null;
                timer -= Time.deltaTime;
            }
            _countdownLabel.gameObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(respawnDelay);
        }

        OnBeforeRespawn?.Invoke();

        Vector3 spawnPos = GetSpawnPosition();
        ParticipantHandle trackedParticipant = ResolveTrackedParticipant();

        if (trackedParticipant != null)
        {
            currentPlayer = SpawnPlayer(spawnPos, trackedParticipant);
            SubscribeToPlayer(currentPlayer);
        }
        else if (playerPrefab != null && currentPlayer == null)
        {
            currentPlayer = SpawnPlayer(spawnPos, null);
            SubscribeToPlayer(currentPlayer);
        }
        else if (currentPlayer != null)
        {
            CharacterController controller = currentPlayer.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
                currentPlayer.transform.position = spawnPos;
                controller.enabled = true;
            }
            else
            {
                currentPlayer.transform.position = spawnPos;
            }

            RevivePlayer(currentPlayer);
        }
        else
        {
            Debug.LogWarning("[PlayerSpawner] Respawn was requested but no tracked player or prefab could be resolved.", this);
        }

        IsRespawning = false;
        OnAfterRespawn?.Invoke();
    }

    private void RevivePlayer(GameObject player)
    {
        if (player == null)
            return;

        Motor3D motor = player.GetComponent<Motor3D>();
        if (motor != null)
            motor.enabled = true;

        CharacterController characterController = player.GetComponent<CharacterController>();
        if (characterController != null)
            characterController.enabled = true;

        KnockbackReceiver knockbackReceiver = player.GetComponent<KnockbackReceiver>();
        if (knockbackReceiver != null)
        {
            knockbackReceiver.enabled = true;
            knockbackReceiver.ClearKnockback();
        }

        HealthComponent hp = player.GetComponentInChildren<HealthComponent>();
        if (hp != null)
        {
            hp.FullHeal();
            if (respawnHpFraction < 1f)
                hp.SetCurrentHealth(hp.MaxHealth * respawnHpFraction);

            if (respawnShield > 0f)
                StartCoroutine(ApplyRespawnShield(hp));
        }

        foreach (Renderer renderer in player.GetComponentsInChildren<Renderer>())
            renderer.enabled = true;
    }

    private void DisablePlayer(GameObject player)
    {
        if (player == null)
            return;

        Motor3D motor = player.GetComponent<Motor3D>();
        if (motor != null)
            motor.enabled = false;

        CharacterController characterController = player.GetComponent<CharacterController>();
        if (characterController != null)
            characterController.enabled = false;

        KnockbackReceiver knockbackReceiver = player.GetComponent<KnockbackReceiver>();
        if (knockbackReceiver != null)
        {
            knockbackReceiver.ClearKnockback();
            knockbackReceiver.enabled = false;
        }
    }

    private IEnumerator ApplyRespawnShield(HealthComponent hp)
    {
        hp.ForceIFrames(respawnShield);

        float elapsed = 0f;
        Renderer[] renderers = currentPlayer != null
            ? currentPlayer.GetComponentsInChildren<Renderer>()
            : new Renderer[0];

        while (elapsed < respawnShield)
        {
            elapsed += Time.deltaTime;
            bool visible = Mathf.FloorToInt(elapsed / 0.12f) % 2 == 0;
            foreach (Renderer renderer in renderers)
                renderer.enabled = visible;
            yield return null;
        }

        foreach (Renderer renderer in renderers)
            renderer.enabled = true;
    }

    private void BuildCountdownUI()
    {
        if (_countdownLabel != null)
            return;

        GameObject canvasObject = new GameObject("[PlayerSpawner] RespawnCountdownCanvas");
        DontDestroyOnLoad(canvasObject);
        _countdownCanvas = canvasObject;
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        GameObject textObject = new GameObject("CountdownLabel");
        textObject.transform.SetParent(canvasObject.transform, false);
        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = countdownFontSize;
        label.color = countdownColor;
        label.text = string.Empty;

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.35f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.35f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(700f, 130f);
        rectTransform.anchoredPosition = Vector2.zero;

        textObject.SetActive(false);
        _countdownLabel = label;
    }

    private void DestroyCountdownUI()
    {
        if (_countdownCanvas == null)
            return;

        if (Application.isPlaying)
            Destroy(_countdownCanvas);
        else
            DestroyImmediate(_countdownCanvas);

        _countdownCanvas = null;
        _countdownLabel = null;
    }

    private Vector3 GetSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int index = randomiseSpawnPoint ? Random.Range(0, spawnPoints.Length) : 0;
            if (spawnPoints[index] != null)
                return spawnPoints[index].position;
        }

        return transform.position;
    }

    private GameObject SpawnPlayer(Vector3 position, ParticipantHandle participant)
    {
        ResolveParticipantServices();

        if (participantSpawnService != null && participant != null)
        {
            participantSpawnService.SetSpawnPoints(spawnPoints);
            GameObject pawn = participantSpawnService.SpawnParticipantPawn(participant);
            if (pawn != null)
            {
                pawn.transform.position = position;
                return pawn;
            }

            Debug.LogWarning($"[PlayerSpawner] Participant pawn spawn failed for seat {participant.SeatIndex}. Check the participant PawnDefinition and pawn prefab before respawning.", this);
        }

        if (playerPrefab == null)
        {
            Debug.LogWarning("[PlayerSpawner] Cannot spawn player because no participant pawn was resolved and Player Prefab is empty.", this);
            return null;
        }

        return Instantiate(playerPrefab, position, Quaternion.identity);
    }

    private ParticipantHandle ResolveTrackedParticipant()
    {
        ResolveParticipantServices();
        if (rosterService == null)
            return null;

        // If a specific seat is targeted, we ONLY return that seat.
        if (targetSeatIndex >= 0)
        {
            for (int i = 0; i < rosterService.Participants.Count; i++)
            {
                ParticipantHandle participant = rosterService.Participants[i];
                if (participant != null && participant.SeatIndex == targetSeatIndex)
                    return participant;
            }
            return null;
        }

        // If targetSeatIndex is -1 (default/unconfigured), we fall back to the primary participant.
        return rosterService.Participants.Count > 0 ? rosterService.Participants[0] : null;
    }

    private GameObject ResolveTrackedPlayer()
    {
        ParticipantHandle trackedParticipant = ResolveTrackedParticipant();
        if (trackedParticipant?.PawnInstance != null)
            return trackedParticipant.PawnInstance;

        if (currentPlayer != null)
            return currentPlayer;

        if (_playerProvider != null)
        {
            GameObject providedPlayer = _playerProvider.GetPlayerGameObject();
            if (providedPlayer != null)
                return providedPlayer;
        }

        return null;
    }

    public void ForceRespawn()
    {
        if (IsRespawning)
            return;

        StopAllCoroutines();
        StartCoroutine(RespawnRoutine());
    }

    public void AddLives(int count) => LivesRemaining += count;

    public void SetPlayerPrefab(GameObject prefab, bool overrideExisting = true)
    {
        if (prefab == null)
            return;

        if (!overrideExisting && playerPrefab != null)
            return;

        playerPrefab = prefab;
    }

    private T ResolveHierarchyComponent<T>() where T : Component
    {
        T component = GetComponentInParent<T>(true);
        if (component != null)
            return component;

        Transform root = transform.root;
        return root != null ? root.GetComponentInChildren<T>(true) : GetComponentInChildren<T>(true);
    }
}
}
