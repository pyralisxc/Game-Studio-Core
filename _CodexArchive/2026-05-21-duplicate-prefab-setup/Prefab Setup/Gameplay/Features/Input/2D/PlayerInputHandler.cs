using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Features.Settings;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Input;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using UnityEngine.InputSystem;
using NeonBlack.Gameplay.Features.GameFlow;
using UnityEngine.UI;

namespace NeonBlack.Gameplay.Features.Input
{
/// <summary>
/// Reads input and feeds movement direction to Motor2D.
/// Input priority: Gamepad â†’ Keyboard â†’ Virtual Joystick.
///
/// DASH: Tap anywhere in the right half of the screen (or left when controls are swapped).
///       Also triggered by Player/Jump action (Space / Gamepad South / Gamepad A).
///       Touch detection uses raw Touchscreen input; hardware uses an InputAction.performed callback.
///
/// Setup: Attach to the Player GameObject alongside Motor2D.
///   1. Drag InputSystem_Actions.inputactions (Assets root) into _inputActions.
///   2. Wire VirtualJoystick, LeftZone, RightZone, and Canvas in the Inspector.
///   3. Optionally add PlayerInput so local multiplayer can pair devices per player.
///   4. The "Player/Move" action handles gamepad leftStick + WASD/arrows.
///      Gamepad dpad is supplemented via a raw read when it is not in the asset.
///   5. The "Player/Jump" action (Space + buttonSouth) drives dash.
/// </summary>
[RequireComponent(typeof(Motor2D))]
[DefaultExecutionOrder(-10)] // Must Awake before SettingsManager.Start() calls PushToInputHandler.
public class PlayerInputHandler : MonoBehaviour, IInputSettingsReceiver, IPawnInputModule
{
    public static PlayerInputHandler Instance { get; private set; }

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

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // Runtime
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private Motor2D _controller;
    private PlayerInput         _playerInput;
    private InputAction         _moveAction;
    private InputAction         _dashAction;
    private InputAction         _attackAction;
    private InputAction         _kickAction;
    private InputAction         _interactAction;
    private InputAction         _blockAction;
    private IPawnCombatInputReceiver2D _combatInputReceiver;
    private IActorInteractionInputReceiver2D _interactionInputReceiver;
    private IActorGuardInputReceiver2D _guardInputReceiver;
    private Vector2             _lastNonZeroDir = Vector2.right;
    private RectTransform       _dashZone; // assigned by ApplySettings â€” right zone by default

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // Unity Lifecycle
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        _controller = GetComponent<Motor2D>();
        _playerInput = GetComponent<PlayerInput>();
        _combatInputReceiver = GetComponent<IPawnCombatInputReceiver2D>();
        _interactionInputReceiver = GetComponent<IActorInteractionInputReceiver2D>();
        _guardInputReceiver = GetComponent<IActorGuardInputReceiver2D>();

        // Register with SettingsManager so it can push settings without a concrete type reference.
        // SettingsManager.DefaultExecutionOrder = -40, so Instance is guaranteed set by now.
        SettingsManager.Instance?.RegisterInputReceiver(this);

        if (_playerInput != null && _playerInput.actions != null)
            _inputActions = _playerInput.actions;

        if (_inputActions != null)
        {
            InputActionMap playerMap = _inputActions.FindActionMap("Player", throwIfNotFound: false);
            if (playerMap != null)
            {
                _moveAction = playerMap.FindAction("Move", throwIfNotFound: false);
                _dashAction = playerMap.FindAction("Jump", throwIfNotFound: false); // Space + Gamepad South
                _attackAction = playerMap.FindAction("Attack", throwIfNotFound: false);
                _kickAction = playerMap.FindAction("Kick", throwIfNotFound: false);
                _interactAction = playerMap.FindAction("Interact", throwIfNotFound: false);
                _blockAction = playerMap.FindAction("Block", throwIfNotFound: false);
            }
            else
                Debug.LogError("[PlayerInputHandler] 'Player' action map not found in _inputActions.", this);
        }
        else
            Debug.LogError("[PlayerInputHandler] _inputActions is not assigned. Drag InputSystem_Actions.inputactions into the Inspector.", this);
    }

    private void OnEnable()
    {
        _moveAction?.Enable();
        _dashAction?.Enable();
        _attackAction?.Enable();
        _kickAction?.Enable();
        _interactAction?.Enable();
        _blockAction?.Enable();
        if (_dashAction != null) _dashAction.performed += OnDashPerformed;
        if (_attackAction != null) _attackAction.performed += OnAttackPerformed;
        if (_kickAction != null) _kickAction.performed += OnKickPerformed;
        if (_interactAction != null) _interactAction.performed += OnInteractPerformed;
        if (_blockAction != null) { _blockAction.performed += OnBlockPerformed; _blockAction.canceled += OnBlockCanceled; }
    }

    private void OnDisable()
    {
        if (_dashAction != null) _dashAction.performed -= OnDashPerformed;
        if (_attackAction != null) _attackAction.performed -= OnAttackPerformed;
        if (_kickAction != null) _kickAction.performed -= OnKickPerformed;
        if (_interactAction != null) _interactAction.performed -= OnInteractPerformed;
        if (_blockAction != null) { _blockAction.performed -= OnBlockPerformed; _blockAction.canceled -= OnBlockCanceled; }
        _moveAction?.Disable();
        _dashAction?.Disable();
        _attackAction?.Disable();
        _kickAction?.Disable();
        _interactAction?.Disable();
        _blockAction?.Disable();
    }

    private void Start()
    {
        // Zone panels use an alpha-0 Image as their raycast surface. But joystick and
        // dash detection both use RectTransformUtility (not EventSystem raycasts), so
        // the panels don't need Raycast Target. Disabling it prevents them from blocking
        // taps on buttons (Restart, Main Menu, Settings) that overlap the same screen area.
        DisableZoneRaycast(_leftZone);
        DisableZoneRaycast(_rightZone);
    }

    private static void DisableZoneRaycast(RectTransform zone)
    {
        if (zone == null) return;
        var img = zone.GetComponent<Image>();
        if (img != null) img.raycastTarget = false;
    }

    private void OnDestroy()
    {
        SettingsManager.Instance?.UnregisterInputReceiver(this);
        if (Instance == this) Instance = null;
    }

    public void SetInputActions(InputActionAsset inputActions, bool rebindActions = true)
    {
        if (inputActions == null)
            return;

        _playerInput ??= GetComponent<PlayerInput>();

        if (rebindActions)
        {
            _moveAction?.Disable();
            if (_dashAction != null)
                _dashAction.performed -= OnDashPerformed;
            if (_attackAction != null)
                _attackAction.performed -= OnAttackPerformed;
            if (_kickAction != null)
                _kickAction.performed -= OnKickPerformed;
            if (_interactAction != null)
                _interactAction.performed -= OnInteractPerformed;
            if (_blockAction != null) { _blockAction.performed -= OnBlockPerformed; _blockAction.canceled -= OnBlockCanceled; }
            _dashAction?.Disable();
            _attackAction?.Disable();
            _kickAction?.Disable();
            _interactAction?.Disable();
            _blockAction?.Disable();
        }

        _inputActions = inputActions;
        if (_playerInput != null && _playerInput.actions != inputActions)
            _playerInput.actions = inputActions;

        InputActionMap playerMap = _inputActions.FindActionMap("Player", throwIfNotFound: false);
        if (playerMap == null)
            return;

        _moveAction = playerMap.FindAction("Move", throwIfNotFound: false);
        _dashAction = playerMap.FindAction("Jump", throwIfNotFound: false);
        _attackAction = playerMap.FindAction("Attack", throwIfNotFound: false);
        _kickAction = playerMap.FindAction("Kick", throwIfNotFound: false);
        _interactAction = playerMap.FindAction("Interact", throwIfNotFound: false);
        _blockAction = playerMap.FindAction("Block", throwIfNotFound: false);

        if (isActiveAndEnabled && rebindActions)
        {
            _moveAction?.Enable();
            _dashAction?.Enable();
            _attackAction?.Enable();
            _kickAction?.Enable();
            _interactAction?.Enable();
            _blockAction?.Enable();
            if (_dashAction != null)
                _dashAction.performed += OnDashPerformed;
            if (_attackAction != null)
                _attackAction.performed += OnAttackPerformed;
            if (_kickAction != null)
                _kickAction.performed += OnKickPerformed;
            if (_interactAction != null)
                _interactAction.performed += OnInteractPerformed;
            if (_blockAction != null) { _blockAction.performed += OnBlockPerformed; _blockAction.canceled += OnBlockCanceled; }
        }
    }

    public void ApplyInputProfile(PawnProfileApplicationContext context, InputProfile inputProfile)
    {
        if (inputProfile == null)
            return;

        _controller ??= GetComponent<Motor2D>();
        _playerInput ??= GetComponent<PlayerInput>();
        inputProfile.Sanitize();
        if (inputProfile.actions != null)
            SetInputActions(inputProfile.actions);

        ParticipantInputProfileUtility.ApplyToPlayerInput(_playerInput, inputProfile);
        _gamepadEnabled = inputProfile.supportsGamepad;
        _editorKeyboardInput = inputProfile.supportsKeyboardMouse;
        _joystickEnabled = inputProfile.touchFriendly;
    }

    private void Update()
    {
        if (_controller == null)
            return;

        if (_controller.IsDead)
        {
            _controller.MoveDirection = Vector2.zero;
            return;
        }

        bool isPlaying = GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing;

        // Gate the VirtualJoystick component so its own Update() doesn't run outside gameplay.
        // OnDisable() on VirtualJoystick calls Hide(), cleaning up any in-flight touch state.
        if (_joystick != null && _joystick.enabled != isPlaying)
            _joystick.enabled = isPlaying;

        if (!isPlaying)
        {
            _controller.MoveDirection = Vector2.zero;
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

        // â”€â”€ Gamepad (leftStick via action + dpad raw fallback) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
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
                _controller.MoveDirection = Vector2.ClampMagnitude(gamepadRaw, 1f);
                return;
            }
        }

        // â”€â”€ Keyboard (WASD / arrows via action) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        if (_editorKeyboardInput && activeIsKeyboard && hardwareRaw.sqrMagnitude > 0.01f)
        {
            _controller.MoveDirection = hardwareRaw.normalized;
            return;
        }

        // â”€â”€ Virtual Joystick fallback â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        if (_joystickEnabled && _joystick != null)
        {
            Vector2 joy = _joystick.Direction;
            if (joy.magnitude > _joystickDeadzone)
            {
                _controller.MoveDirection = joy;
                return;
            }
        }

        _controller.MoveDirection = Vector2.zero;
    }

    private void LateUpdate()
    {
        if (_controller.MoveDirection.sqrMagnitude > 0.01f)
            _lastNonZeroDir = _controller.MoveDirection;
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // Dash
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>Fired by the Player/Jump InputAction (Space or Gamepad South).</summary>
    private void OnDashPerformed(InputAction.CallbackContext ctx)
    {
        // Respect the gamepad/keyboard enabled toggles.
        if (ctx.control.device is Gamepad && !_gamepadEnabled)  return;
        if (ctx.control.device is Keyboard && !_editorKeyboardInput) return;
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;
        TriggerDash();
    }

    private void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.control.device is Gamepad && !_gamepadEnabled) return;
        if (ctx.control.device is Keyboard && !_editorKeyboardInput) return;
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;
        _combatInputReceiver?.HandlePrimaryAttackInput();
    }

    private void OnKickPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.control.device is Gamepad && !_gamepadEnabled) return;
        if (ctx.control.device is Keyboard && !_editorKeyboardInput) return;
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;
        _combatInputReceiver?.HandleSecondaryAttackInput();
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.control.device is Gamepad && !_gamepadEnabled) return;
        if (ctx.control.device is Keyboard && !_editorKeyboardInput) return;
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;
        _interactionInputReceiver?.HandleInteractionInput();
    }

    private void OnBlockPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.control.device is Gamepad && !_gamepadEnabled) return;
        if (ctx.control.device is Keyboard && !_editorKeyboardInput) return;
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;
        _guardInputReceiver?.HandleGuardStartInput();
    }

    private void OnBlockCanceled(InputAction.CallbackContext ctx)
    {
        if (ctx.control.device is Gamepad && !_gamepadEnabled) return;
        if (ctx.control.device is Keyboard && !_editorKeyboardInput) return;
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;
        _guardInputReceiver?.HandleGuardEndInput();
    }

    private void TriggerDash()
    {
        Vector2 dir = _controller.MoveDirection.sqrMagnitude > 0.01f
            ? _controller.MoveDirection
            : _lastNonZeroDir;
        _controller.TryDash(dir);
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

        return Gamepad.current;
    }

    /// <summary>
    /// Checks every Playing frame for a new touch inside the dash zone and fires TriggerDash.
    /// The joystick and dash zones cover opposite halves of the screen so there is no ambiguity
    /// â€” no touch ID tracking needed.
    /// </summary>
    private void DetectDashZoneTap()
    {
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

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // Public API
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>Called by SettingsManager to push updated values. Shows the correct dash button side.</summary>
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
