using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Glue.Participants;
using NeonBlack.Gameplay.Glue.Session;
using NeonBlack.Gameplay.Modules.Character;
using NeonBlack.Gameplay.Modules.Combat;
using TMPro;
using UnityEngine;
using VContainer;

namespace NeonBlack.Gameplay.Glue.Spawning
{
    /// <summary>
    /// Participant-owned respawn coordinator. Pawn identity and instantiation stay with
    /// ParticipantSpawnService; this component only handles death timing, lives, and revive feedback.
    /// </summary>
    public class PlayerSpawner : GameplayTickBehaviour
    {
        [Header("Participant")]
        [Tooltip("Seat index to track. -1 follows the primary registered participant.")]
        [SerializeField] private int targetSeatIndex = -1;
        [SerializeField] private ParticipantSpawnService participantSpawnService;
        [SerializeField] private ParticipantRosterService rosterService;

        [Header("Timing")]
        [SerializeField] private float respawnDelay = 3f;
        [SerializeField] private float respawnShield = 2f;

        [Header("Lives (0 = infinite)")]
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

        protected override GameplayTickDomain TickDomain => GameplayTickDomain.Spawning;
        protected override bool UsesGameplayTick => IsRespawning || _shieldActive;

        private HealthComponent _health;
        private GameObject _currentPawn;
        private GameObject _countdownCanvas;
        private TextMeshProUGUI _countdownLabel;
        private bool _rosterSubscribed;
        private float _respawnTimer;
        private bool _respawnCountdownVisible;
        private bool _shieldActive;
        private float _shieldTimer;
        private Renderer[] _shieldRenderers = System.Array.Empty<Renderer>();

        private void Awake()
        {
            LivesRemaining = startingLives;
        }

        [Inject]
        private void Construct(
            ParticipantSpawnService injectedSpawnService = null,
            IParticipantRoster injectedRoster = null)
        {
            if (injectedSpawnService != null)
                participantSpawnService = injectedSpawnService;
            if (injectedRoster is ParticipantRosterService concreteRoster)
                rosterService = concreteRoster;
        }

        private void Start()
        {
            SubscribeRoster();
            SubscribeToPawn(ResolveTrackedPawn());
        }

        private void OnEnable()
        {
            SubscribeRoster();
        }

        private void OnDisable()
        {
            UnsubscribeRoster();
            UnsubscribeFromPawn();
        }

        private void OnDestroy()
        {
            UnsubscribeRoster();
            UnsubscribeFromPawn();
            DestroyCountdownUI();
        }

        private void SubscribeRoster()
        {
            if (_rosterSubscribed || rosterService == null)
                return;

            rosterService.ParticipantRegistered += HandleParticipantRegistered;
            rosterService.ParticipantRemoved += HandleParticipantRemoved;
            _rosterSubscribed = true;
        }

        private void UnsubscribeRoster()
        {
            if (!_rosterSubscribed || rosterService == null)
                return;

            rosterService.ParticipantRegistered -= HandleParticipantRegistered;
            rosterService.ParticipantRemoved -= HandleParticipantRemoved;
            _rosterSubscribed = false;
        }

        private void HandleParticipantRegistered(ParticipantHandle participant)
        {
            if (!IsTrackedParticipant(participant))
                return;

            SubscribeToPawn(participant.PawnInstance);
        }

        private void HandleParticipantRemoved(ParticipantHandle participant)
        {
            if (!IsTrackedParticipant(participant))
                return;

            UnsubscribeFromPawn();
        }

        private void SubscribeToPawn(GameObject pawn)
        {
            UnsubscribeFromPawn();
            _currentPawn = pawn;
            if (pawn == null)
                return;

            _health = pawn.GetComponentInChildren<HealthComponent>();
            if (_health == null)
            {
                Debug.LogWarning("[PlayerSpawner] Tracked participant pawn has no HealthComponent; respawn cannot listen for death.", this);
                return;
            }

            _health.OnDeath.AddListener(HandlePawnDeath);
        }

        private void UnsubscribeFromPawn()
        {
            if (_health != null)
                _health.OnDeath.RemoveListener(HandlePawnDeath);

            _health = null;
            _currentPawn = null;
        }

        private void HandlePawnDeath()
        {
            if (IsGameOver || IsRespawning)
                return;

            if (startingLives > 0)
            {
                LivesRemaining--;
                if (LivesRemaining <= 0)
                {
                    IsGameOver = true;
                    OnGameOver?.Invoke();
                    DisablePawn(_currentPawn);
                    return;
                }
            }

            BeginRespawn();
        }

        private void BeginRespawn()
        {
            IsRespawning = true;
            _respawnTimer = Mathf.Max(0f, respawnDelay);
            _respawnCountdownVisible = showCountdown && _respawnTimer > 0f;
            DisablePawn(_currentPawn);

            if (_respawnCountdownVisible)
            {
                BuildCountdownUI();
                _countdownLabel.gameObject.SetActive(true);
                UpdateCountdownLabel();
            }
        }

        protected override void OnGameplayTick(in GameplayTickContext context)
        {
            if (IsRespawning)
                TickRespawn(context.DeltaTime);

            if (_shieldActive)
                TickRespawnShield(context.DeltaTime);
        }

        private void TickRespawn(float deltaTime)
        {
            if (_respawnTimer > 0f)
            {
                _respawnTimer = Mathf.Max(0f, _respawnTimer - deltaTime);
                if (_respawnCountdownVisible)
                    UpdateCountdownLabel();

                if (_respawnTimer > 0f)
                    return;
            }

            if (_respawnCountdownVisible && _countdownLabel != null)
                _countdownLabel.gameObject.SetActive(false);

            _respawnCountdownVisible = false;
            CompleteRespawn();
        }

        private void UpdateCountdownLabel()
        {
            if (_countdownLabel == null)
                return;

            _countdownLabel.text = string.Format(countdownFormat, Mathf.Ceil(_respawnTimer));
        }

        private void CompleteRespawn()
        {
            if (!IsRespawning)
                return;

            OnBeforeRespawn?.Invoke();

            ParticipantHandle participant = ResolveTrackedParticipant();
            if (participant == null)
            {
                Debug.LogWarning("[PlayerSpawner] Respawn requested, but no tracked participant is registered.", this);
            }
            else if (participantSpawnService == null)
            {
                Debug.LogWarning("[PlayerSpawner] Respawn requested, but ParticipantSpawnService is not assigned or injected.", this);
            }
            else
            {
                GameObject pawn = participantSpawnService.SpawnParticipantPawn(participant);
                if (pawn != null)
                {
                    SubscribeToPawn(pawn);
                    RevivePawn(pawn);
                }
            }

            IsRespawning = false;
            OnAfterRespawn?.Invoke();
        }

        private void RevivePawn(GameObject pawn)
        {
            if (pawn == null)
                return;

            Motor3D motor = pawn.GetComponent<Motor3D>();
            if (motor != null)
                motor.enabled = true;

            CharacterController characterController = pawn.GetComponent<CharacterController>();
            if (characterController != null)
                characterController.enabled = true;

            KnockbackReceiver knockbackReceiver = pawn.GetComponent<KnockbackReceiver>();
            if (knockbackReceiver != null)
            {
                knockbackReceiver.enabled = true;
                knockbackReceiver.ClearKnockback();
            }

            HealthComponent hp = pawn.GetComponentInChildren<HealthComponent>();
            if (hp != null)
            {
                hp.FullHeal();
                if (respawnHpFraction < 1f)
                    hp.SetCurrentHealth(hp.MaxHealth * respawnHpFraction);

                if (respawnShield > 0f)
                    BeginRespawnShield(pawn, hp);
            }

            foreach (Renderer renderer in pawn.GetComponentsInChildren<Renderer>())
                renderer.enabled = true;
        }

        private void DisablePawn(GameObject pawn)
        {
            if (pawn == null)
                return;

            Motor3D motor = pawn.GetComponent<Motor3D>();
            if (motor != null)
                motor.enabled = false;

            CharacterController characterController = pawn.GetComponent<CharacterController>();
            if (characterController != null)
                characterController.enabled = false;

            KnockbackReceiver knockbackReceiver = pawn.GetComponent<KnockbackReceiver>();
            if (knockbackReceiver != null)
            {
                knockbackReceiver.ClearKnockback();
                knockbackReceiver.enabled = false;
            }
        }

        private void BeginRespawnShield(GameObject pawn, HealthComponent hp)
        {
            hp.ForceIFrames(respawnShield);

            _shieldTimer = 0f;
            _shieldActive = true;
            _shieldRenderers = pawn != null
                ? pawn.GetComponentsInChildren<Renderer>()
                : System.Array.Empty<Renderer>();
        }

        private void TickRespawnShield(float deltaTime)
        {
            _shieldTimer += deltaTime;
            if (_shieldTimer < respawnShield)
            {
                bool visible = Mathf.FloorToInt(_shieldTimer / 0.12f) % 2 == 0;
                foreach (Renderer renderer in _shieldRenderers)
                {
                    if (renderer != null)
                        renderer.enabled = visible;
                }

                return;
            }

            foreach (Renderer renderer in _shieldRenderers)
            {
                if (renderer != null)
                    renderer.enabled = true;
            }

            _shieldActive = false;
            _shieldTimer = 0f;
            _shieldRenderers = System.Array.Empty<Renderer>();
        }

        private ParticipantHandle ResolveTrackedParticipant()
        {
            if (rosterService == null)
                return null;

            if (targetSeatIndex >= 0)
                return rosterService.TryGetParticipantBySeat(targetSeatIndex, out ParticipantHandle participant)
                    ? participant
                    : null;

            return rosterService.TryGetPrimaryParticipant(out ParticipantHandle primary)
                ? primary
                : null;
        }

        private GameObject ResolveTrackedPawn()
        {
            return ResolveTrackedParticipant()?.PawnInstance;
        }

        private bool IsTrackedParticipant(ParticipantHandle participant)
        {
            if (participant == null)
                return false;

            return targetSeatIndex < 0 || participant.SeatIndex == targetSeatIndex;
        }

        private void BuildCountdownUI()
        {
            if (_countdownLabel != null)
                return;

            GameObject canvasObject = new GameObject("[PlayerSpawner] RespawnCountdownCanvas");
            canvasObject.transform.SetParent(transform, false);
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

        public void ForceRespawn()
        {
            if (!IsRespawning && !IsGameOver)
                BeginRespawn();
        }
    }
}
