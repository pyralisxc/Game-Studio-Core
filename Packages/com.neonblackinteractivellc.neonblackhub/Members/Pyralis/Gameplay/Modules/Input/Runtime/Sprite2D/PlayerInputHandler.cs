using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Modules.Input;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace NeonBlack.Gameplay.Modules.Input
{
/// <summary>
/// Lower-level input reader used by <see cref="Motor2DInputAdapter"/>.
/// New 2D player pawns should add Motor2DInputAdapter unless they intentionally
/// need a custom direct-input route.
///
/// Reads input and feeds movement direction to Motor2D.
/// Input priority: Gamepad -> Keyboard -> Virtual Joystick.
///
/// DASH: Tap in the authored dash zone for touch setups, or add a Dash row to InputProfile
///       when hardware input should trigger dash. Leave Dash absent for games where this
///       2D controller should not dash from keyboard/gamepad input.
///
    /// Setup: new 2D player pawns should add Motor2DInputAdapter alongside Motor2D.
    /// ParticipantDefinition.inputProfile supplies the InputActionAsset and action rows.
    /// Optional direct scene pawns can still assign _inputActions locally for experiments.
    /// Add PlayerInput only when local device pairing is part of the proof.
/// </summary>
[AddComponentMenu("NeonBlack/Gameplay/Modules/Input/Sprite2D/Advanced Player Input Handler")]
[DefaultExecutionOrder(-10)] // Register before settings services push input values during Start().
public partial class PlayerInputHandler : MonoBehaviour, IInputSettingsReceiver, IPawnInputModule, IPawnRuntimeServicesReceiver
{
    [Header("Input Actions")]
    [SerializeField, Tooltip("Assign InputSystem_Actions.inputactions from the Assets root.\n" +
        "Provides Player/Move (gamepad leftStick + WASD) and Player/Jump (Space + buttonSouth) actions.")]
    private InputActionAsset _inputActions;

    [Header("References")]
    [SerializeField, Tooltip("The VirtualJoystick component on the JoystickContainer in the Canvas.")]
    private VirtualJoystick _joystick;
    [SerializeField, Tooltip("RectTransform panel covering the LEFT half of the screen.\n" +
        "Assigned to the joystick or dash zone depending on the Swap Controls setting.")]
    private RectTransform _leftZone;
    [SerializeField, Tooltip("RectTransform panel covering the RIGHT half of the screen.\n" +
        "Assigned to the dash zone or joystick depending on the Swap Controls setting.")]
    private RectTransform _rightZone;
    [SerializeField, Tooltip("The Canvas containing the input UI. Required for zone hit-testing on non-overlay canvases.")]
    private Canvas _canvas;

    [SerializeField, Tooltip("Gameplay state provider that controls when player input is accepted. SessionStateService normally supplies IGameplayStateReader; assign this only for standalone custom state.")]
    private MonoBehaviour _gameplayStateSource;

    [SerializeField, Tooltip("Settings service that pushes joystick/gamepad deadzone and swap-controls values. SettingsManager implements IInputSettingsRegistrar.")]
    private MonoBehaviour _settingsRegistrarSource;

    [Header("Input Modes")]
    [SerializeField, Tooltip("Enable the virtual joystick.\nDisable to test without touch input.")]
    private bool _joystickEnabled = true;
    [SerializeField, Tooltip("WASD / Arrow keys work in the Editor and PC builds. Safe to leave on.")]
    private bool _editorKeyboardInput = true;
    [SerializeField, Tooltip("Enable gamepad support (left stick or d-pad).")]
    private bool _gamepadEnabled = true;

    [Header("Joystick Settings")]
    [SerializeField, Tooltip("How far the thumb must push the joystick before movement registers.\n" +
        "0.0 = reacts to the tiniest nudge (may drift on a resting thumb).\n" +
        "0.1 = default sweet spot.\n" +
        "0.3 = requires a firm push before any movement starts.")]
    [Range(0f, 0.5f)]
    private float _joystickDeadzone = 0.1f;

    [Header("Gamepad")]
    [SerializeField, Tooltip("Minimum stick deflection before input registers.\n" +
        "0.2 = default, handles typical stick drift.")]
    [Range(0f, 0.5f)]
    private float _gamepadDeadzone = 0.2f;

    // Runtime

    private IActorMovementInputReceiver2D _movementInputReceiver;
    private PlayerInput         _playerInput;
    private InputAction         _moveAction;
    private InputAction         _jumpAction;
    private InputAction         _dashAction;
    private InputAction         _attackAction;
    private InputAction         _kickAction;
    private InputAction         _interactAction;
    private InputAction         _blockAction;
    private IActorCombatRequestReceiver _combatRequestReceiver;
    private IActorInteractionInputReceiver2D _interactionInputReceiver;
    private IActorGuardInputReceiver2D _guardInputReceiver;
    private IActorGameplayActionReceiver[] _gameplayActionReceivers;
    private IGameplayStateReader _gameplayStateReader;
    private IInputSettingsRegistrar _inputSettingsRegistrar;
    private Vector2             _lastNonZeroDir = Vector2.right;
    private RectTransform       _dashZone; // assigned by ApplySettings; right zone by default
    private bool                _loggedMissingGameplayState;
    private bool                _loggedMissingInputActions;
    private bool                _receivedParticipantInputProfile;
    private InputProfile        _inputProfile;

    // Unity Lifecycle

    private void Awake()
    {
        _movementInputReceiver = GetComponent<IActorMovementInputReceiver2D>();
        _playerInput = GetComponent<PlayerInput>();
        _combatRequestReceiver = GetComponent<IActorCombatRequestReceiver>();
        _interactionInputReceiver = GetComponent<IActorInteractionInputReceiver2D>();
        _guardInputReceiver = GetComponent<IActorGuardInputReceiver2D>();
        _gameplayActionReceivers = GetComponentsInChildren<IActorGameplayActionReceiver>(true);

        ResolveInputSettingsRegistrar()?.RegisterInputReceiver(this);

        if (_playerInput != null && _playerInput.actions != null)
        {
            _inputActions = _playerInput.actions;
        }

        if (_inputActions != null)
            BindActions();
    }

    private void OnEnable()
    {
        EnableAndSubscribeActions();
    }

    private void OnDisable()
    {
        DisableAndUnsubscribeActions();
    }

    private void Start()
    {
        // Zone panels use an alpha-0 Image as their raycast surface. But joystick and
        // dash detection both use RectTransformUtility (not EventSystem raycasts), so
        // the panels don't need Raycast Target. Disabling it prevents them from blocking
        // taps on buttons (Restart, Main Menu, Settings) that overlap the same screen area.
        DisableZoneRaycast(_leftZone);
        DisableZoneRaycast(_rightZone);
        ReportMissingInputActionsIfNeeded();
    }

    private static void DisableZoneRaycast(RectTransform zone)
    {
        if (zone == null) return;
        var img = zone.GetComponent<Image>();
        if (img != null) img.raycastTarget = false;
    }

    private void OnDestroy()
    {
        ResolveInputSettingsRegistrar()?.UnregisterInputReceiver(this);
    }

    [Inject]
    private void Construct(IGameplayStateReader gameplayStateReader = null)
    {
        if (gameplayStateReader != null)
            _gameplayStateReader = gameplayStateReader;
    }

    public void ConfigureRuntime(IGameplayStateReader gameplayStateReader)
    {
        if (gameplayStateReader != null)
            _gameplayStateReader = gameplayStateReader;
    }

    public void ApplyRuntimeServices(PawnRuntimeServicesContext context)
    {
        ConfigureRuntime(context.GameplayStateReader);
    }

    public void ConfigureRuntime(IGameplayStateReader gameplayStateReader, IInputSettingsRegistrar inputSettingsRegistrar)
    {
        ConfigureRuntime(gameplayStateReader);
        if (inputSettingsRegistrar != null)
        {
            ResolveInputSettingsRegistrar()?.UnregisterInputReceiver(this);
            _inputSettingsRegistrar = inputSettingsRegistrar;
            _inputSettingsRegistrar.RegisterInputReceiver(this);
        }
    }

    private void Update()
    {
        if (_movementInputReceiver == null)
            return;

        if (_movementInputReceiver.IsDead)
        {
            _movementInputReceiver.MoveDirection = Vector2.zero;
            return;
        }

        bool isPlaying = IsGameplayActive();

        // Gate the VirtualJoystick component so its own Update() doesn't run outside gameplay.
        // OnDisable() on VirtualJoystick calls Hide(), cleaning up any in-flight touch state.
        if (_joystick != null && _joystick.enabled != isPlaying)
            _joystick.enabled = isPlaying;

        if (!isPlaying)
        {
            _movementInputReceiver.MoveDirection = Vector2.zero;
            return;
        }

        // Tap-to-dash: detect touches in the dash zone before movement logic runs.
        // Called here so early returns below (gamepad/keyboard/joystick) don't skip it.
        DetectDashZoneTap();

        // Determine which device is currently driving the Move action.
        // activeControl is null when no hardware input is occurring.
        bool activeIsGamepad  = _moveAction?.activeControl?.device is Gamepad;
        bool activeIsKeyboard = _moveAction?.activeControl?.device is Keyboard;
        Vector2 hardwareRaw   = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;

        Gamepad assignedGamepad = GetAssignedGamepad();
        if (_gamepadEnabled && assignedGamepad != null)
        {
            Vector2 gamepadRaw = activeIsGamepad ? hardwareRaw : Vector2.zero;

            // D-Pad is not always included in authored Move bindings, so supplement it with
            // the specific gamepad paired to this PlayerInput when one exists.
            if (gamepadRaw.sqrMagnitude < 0.01f)
                gamepadRaw = assignedGamepad.dpad.ReadValue();

            if (gamepadRaw.sqrMagnitude > _gamepadDeadzone * _gamepadDeadzone)
            {
                _movementInputReceiver.MoveDirection = Vector2.ClampMagnitude(gamepadRaw, 1f);
                return;
            }
        }

        if (_editorKeyboardInput && activeIsKeyboard && hardwareRaw.sqrMagnitude > 0.01f)
        {
            _movementInputReceiver.MoveDirection = hardwareRaw.normalized;
            return;
        }

        if (_joystickEnabled && _joystick != null)
        {
            Vector2 joy = _joystick.Direction;
            if (joy.magnitude > _joystickDeadzone)
            {
                _movementInputReceiver.MoveDirection = joy;
                return;
            }
        }

        _movementInputReceiver.MoveDirection = Vector2.zero;
    }

    private void LateUpdate()
    {
        if (_movementInputReceiver != null && _movementInputReceiver.MoveDirection.sqrMagnitude > 0.01f)
            _lastNonZeroDir = _movementInputReceiver.MoveDirection;
    }

    private bool IsGameplayActive()
    {
        if (_gameplayStateReader != null)
            return _gameplayStateReader.IsGameplayActive;

        if (!_loggedMissingGameplayState)
        {
            _loggedMissingGameplayState = true;
            Debug.LogWarning("[PlayerInputHandler] Gameplay State Source is not configured yet. GameplaySessionBootstrap normally supplies SessionStateService during participant spawn; direct scene pawns can assign an IGameplayStateReader source.", this);
        }

        return false;
    }

    private IInputSettingsRegistrar ResolveInputSettingsRegistrar()
    {
        if (_inputSettingsRegistrar != null)
            return _inputSettingsRegistrar;

        if (_settingsRegistrarSource == null)
            return null;

        _inputSettingsRegistrar = _settingsRegistrarSource as IInputSettingsRegistrar;
        if (_inputSettingsRegistrar == null)
            _inputSettingsRegistrar = _settingsRegistrarSource.GetComponent<IInputSettingsRegistrar>();

        return _inputSettingsRegistrar;
    }

    private void TriggerDash()
    {
        if (_movementInputReceiver == null)
            return;

        Vector2 dir = _movementInputReceiver.MoveDirection.sqrMagnitude > 0.01f
            ? _movementInputReceiver.MoveDirection
            : _lastNonZeroDir;
        _movementInputReceiver.TryDash(dir);
    }

    private Gamepad GetAssignedGamepad()
    {
        if (_playerInput != null)
        {
            for (int i = 0; i < _playerInput.devices.Count; i++)
            {
                if (_playerInput.devices[i] is Gamepad gamepad)
                    return gamepad;
            }
        }

        if (_moveAction?.activeControl?.device is Gamepad activeGamepad)
            return activeGamepad;

        return null;
    }

    /// <summary>
    /// Checks every Playing frame for a new touch inside the dash zone and fires TriggerDash.
    /// The joystick and dash zones cover opposite halves of the screen so there is no ambiguity
    /// </summary>
    private void DetectDashZoneTap()
    {
        if (!_joystickEnabled) return;
        if (_dashZone == null) return;
        var touchscreen = Touchscreen.current;
        if (touchscreen == null) return;

        Camera cam = _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? _canvas.worldCamera : null;

        foreach (var touch in touchscreen.touches)
        {
            if (touch.phase.ReadValue() != UnityEngine.InputSystem.TouchPhase.Began) continue;
            Vector2 screenPos = touch.position.ReadValue();
            if (RectTransformUtility.RectangleContainsScreenPoint(_dashZone, screenPos, cam))
            {
                TriggerDash();
                return;
            }
        }
    }

    // Public API

    /// <summary>Called by the input settings service to push updated values. Shows the correct dash button side.</summary>
    public void ApplySettings(float joystickDeadzone, bool swapControls, float gamepadDeadzone = 0.2f)
    {
        _joystickDeadzone = joystickDeadzone;
        _gamepadDeadzone  = gamepadDeadzone;

        // swapControls = false (default): joystick on left, dash tap zone on right.
        // swapControls = true:            joystick on right, dash tap zone on left.
        _dashZone = swapControls ? _leftZone : _rightZone;
        _joystick?.SetActivationZone(swapControls ? _rightZone : _leftZone);
    }
}
}
