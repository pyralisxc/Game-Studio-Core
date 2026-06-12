using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Core.Config;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Input;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace NeonBlack.Gameplay.Features.Input
{
/// <summary>
/// Reads input and feeds movement direction to Motor2D.
/// Input priority: Gamepad -> Keyboard -> Virtual Joystick.
///
/// DASH: Tap in the authored dash zone for touch setups, or add a Dash row to InputProfile
///       when hardware input should trigger dash. Leave Dash absent for games where this
///       2D controller should not dash from keyboard/gamepad input.
///
/// Setup: Attach to the Player GameObject alongside Motor2D.
///   1. Drag InputSystem_Actions.inputactions (Assets root) into _inputActions.
///   2. Wire VirtualJoystick, LeftZone, RightZone, and Canvas in the Inspector.
///   3. Optionally add PlayerInput so local multiplayer can pair devices per player.
///   4. The "Player/Move" action handles gamepad leftStick + WASD/arrows.
///      Gamepad dpad is supplemented via a raw read when it is not in the asset.
///   5. The InputProfile Jump Action drives side-view 2D jump when the movement component enables it.
///   6. The InputProfile Dash Action drives hardware dash when one is assigned.
/// </summary>
[AddComponentMenu("NeonBlack/Gameplay/Input/2D Player Input Handler")]
[RequireComponent(typeof(Motor2D))]
[DefaultExecutionOrder(-10)] // Register before settings services push input values during Start().
public class PlayerInputHandler : MonoBehaviour, IInputSettingsReceiver, IPawnInputModule
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

    [SerializeField, Tooltip("Gameplay state provider that controls when player input is accepted. GameManager implements IGameplayStateReader, or assign a custom session state component.")]
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

    private Motor2D _controller;
    private PlayerInput         _playerInput;
    private InputAction         _moveAction;
    private InputAction         _jumpAction;
    private InputAction         _dashAction;
    private InputAction         _attackAction;
    private InputAction         _kickAction;
    private InputAction         _interactAction;
    private InputAction         _blockAction;
    private IPawnCombatInputReceiver2D _combatInputReceiver;
    private IActorInteractionInputReceiver2D _interactionInputReceiver;
    private IActorGuardInputReceiver2D _guardInputReceiver;
    private IActorGameplayActionReceiver[] _gameplayActionReceivers;
    private IGameplayStateReader _gameplayStateReader;
    private IInputSettingsRegistrar _inputSettingsRegistrar;
    private Vector2             _lastNonZeroDir = Vector2.right;
    private RectTransform       _dashZone; // assigned by ApplySettings; right zone by default
    private bool                _loggedMissingGameplayState;
    private InputProfile        _inputProfile;

    // Unity Lifecycle

    private void Awake()
    {
        _controller = GetComponent<Motor2D>();
        _playerInput = GetComponent<PlayerInput>();
        _combatInputReceiver = GetComponent<IPawnCombatInputReceiver2D>();
        _interactionInputReceiver = GetComponent<IActorInteractionInputReceiver2D>();
        _guardInputReceiver = GetComponent<IActorGuardInputReceiver2D>();
        _gameplayActionReceivers = GetComponentsInChildren<IActorGameplayActionReceiver>(true);

        ResolveInputSettingsRegistrar()?.RegisterInputReceiver(this);

        if (_playerInput != null && _playerInput.actions != null)
        {
            _inputActions = _playerInput.actions;
            if (GameplayRuntimeContext.DefaultInputProfile != null && GameplayRuntimeContext.DefaultInputProfile.actions == _inputActions)
                _inputProfile = GameplayRuntimeContext.DefaultInputProfile;
        }
        else if (GameplayRuntimeContext.DefaultInputActions != null)
        {
            _inputProfile = GameplayRuntimeContext.DefaultInputProfile;
            _inputActions ??= GameplayRuntimeContext.DefaultInputActions;
        }

        if (_inputActions != null)
            BindActions();
        else
            Debug.LogError("[PlayerInputHandler] No InputActionAsset is assigned. Assign one in InputProfile or on PlayerInput.", this);
    }

    private void OnEnable()
    {
        _moveAction?.Enable();
        _jumpAction?.Enable();
        _dashAction?.Enable();
        _attackAction?.Enable();
        _kickAction?.Enable();
        _interactAction?.Enable();
        _blockAction?.Enable();
        if (_jumpAction != null) _jumpAction.performed += OnJumpPerformed;
        if (_dashAction != null) _dashAction.performed += OnDashPerformed;
        if (_attackAction != null) _attackAction.performed += OnAttackPerformed;
        if (_kickAction != null) _kickAction.performed += OnKickPerformed;
        if (_interactAction != null) _interactAction.performed += OnInteractPerformed;
        if (_blockAction != null) { _blockAction.performed += OnBlockPerformed; _blockAction.canceled += OnBlockCanceled; }
    }

    private void OnDisable()
    {
        if (_jumpAction != null) _jumpAction.performed -= OnJumpPerformed;
        if (_dashAction != null) _dashAction.performed -= OnDashPerformed;
        if (_attackAction != null) _attackAction.performed -= OnAttackPerformed;
        if (_kickAction != null) _kickAction.performed -= OnKickPerformed;
        if (_interactAction != null) _interactAction.performed -= OnInteractPerformed;
        if (_blockAction != null) { _blockAction.performed -= OnBlockPerformed; _blockAction.canceled -= OnBlockCanceled; }
        _moveAction?.Disable();
        _jumpAction?.Disable();
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

    public void SetInputActions(InputActionAsset inputActions, bool rebindActions = true)
    {
        SetInputActions(inputActions, _inputProfile, rebindActions);
    }

    public void SetInputActions(InputActionAsset inputActions, InputProfile inputProfile, bool rebindActions = true)
    {
        if (inputActions == null)
            return;

        _playerInput ??= GetComponent<PlayerInput>();

        if (rebindActions)
        {
            _moveAction?.Disable();
            if (_jumpAction != null)
                _jumpAction.performed -= OnJumpPerformed;
            if (_dashAction != null)
                _dashAction.performed -= OnDashPerformed;
            if (_attackAction != null)
                _attackAction.performed -= OnAttackPerformed;
            if (_kickAction != null)
                _kickAction.performed -= OnKickPerformed;
            if (_interactAction != null)
                _interactAction.performed -= OnInteractPerformed;
            if (_blockAction != null) { _blockAction.performed -= OnBlockPerformed; _blockAction.canceled -= OnBlockCanceled; }
            _jumpAction?.Disable();
            _dashAction?.Disable();
            _attackAction?.Disable();
            _kickAction?.Disable();
            _interactAction?.Disable();
            _blockAction?.Disable();
        }

        _inputActions = inputActions;
        _inputProfile = inputProfile;
        if (_playerInput != null && _playerInput.actions != inputActions)
            _playerInput.actions = inputActions;

        if (!BindActions())
            return;

        if (isActiveAndEnabled && rebindActions)
        {
            _moveAction?.Enable();
            _jumpAction?.Enable();
            _dashAction?.Enable();
            _attackAction?.Enable();
            _kickAction?.Enable();
            _interactAction?.Enable();
            _blockAction?.Enable();
            if (_jumpAction != null)
                _jumpAction.performed += OnJumpPerformed;
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
        _inputProfile = inputProfile;
        if (inputProfile.actions != null)
            SetInputActions(inputProfile.actions, inputProfile);
        else if (_inputActions != null)
            BindActions();

        ParticipantInputProfileUtility.ApplyToPlayerInput(_playerInput, inputProfile);
        _gamepadEnabled = inputProfile.supportsGamepad;
        _editorKeyboardInput = inputProfile.supportsKeyboardMouse;
        _joystickEnabled = inputProfile.touchFriendly;
    }

    private bool BindActions()
    {
        InputActionMap actionMap = ParticipantInputProfileUtility.FindGameplayActionMap(_inputActions, _inputProfile);
        if (actionMap == null)
        {
            string mapName = _inputProfile != null && !string.IsNullOrWhiteSpace(_inputProfile.primaryActionMap)
                ? _inputProfile.primaryActionMap
                : "Player";
            Debug.LogError($"[PlayerInputHandler] '{mapName}' action map not found in the assigned InputActionAsset.", this);
            return false;
        }

        string moveActionName = ParticipantInputProfileUtility.GetActionName(_inputProfile, GameplayInputActionRole.Move);
        string jumpActionName = ParticipantInputProfileUtility.GetActionName(_inputProfile, GameplayInputActionRole.Jump);
        string dashActionName = ParticipantInputProfileUtility.GetActionName(_inputProfile, GameplayInputActionRole.Dash);
        string attackActionName = ParticipantInputProfileUtility.GetActionName(_inputProfile, GameplayInputActionRole.AttackPrimary);
        string secondaryAttackActionName = ParticipantInputProfileUtility.GetActionName(_inputProfile, GameplayInputActionRole.AttackSecondary);
        string interactActionName = ParticipantInputProfileUtility.GetActionName(_inputProfile, GameplayInputActionRole.Interact);
        string blockActionName = ParticipantInputProfileUtility.GetActionName(_inputProfile, GameplayInputActionRole.Block);

        _moveAction = ParticipantInputProfileUtility.FindAction(actionMap, _inputProfile, GameplayInputActionRole.Move);
        _jumpAction = ParticipantInputProfileUtility.FindAction(actionMap, _inputProfile, GameplayInputActionRole.Jump);
        _dashAction = ParticipantInputProfileUtility.FindAction(actionMap, _inputProfile, GameplayInputActionRole.Dash);
        _attackAction = ParticipantInputProfileUtility.FindAction(actionMap, _inputProfile, GameplayInputActionRole.AttackPrimary);
        _kickAction = ParticipantInputProfileUtility.FindAction(actionMap, _inputProfile, GameplayInputActionRole.AttackSecondary);
        _interactAction = ParticipantInputProfileUtility.FindAction(actionMap, _inputProfile, GameplayInputActionRole.Interact);
        _blockAction = ParticipantInputProfileUtility.FindAction(actionMap, _inputProfile, GameplayInputActionRole.Block);

        if (_moveAction == null)
            ParticipantInputProfileUtility.LogMissingAction(this, nameof(PlayerInputHandler), _inputProfile, "Move", moveActionName);
        if (_jumpAction == null && ParticipantInputProfileUtility.HasRequiredBinding(_inputProfile, GameplayInputActionRole.Jump))
            ParticipantInputProfileUtility.LogMissingAction(this, nameof(PlayerInputHandler), _inputProfile, "Jump", jumpActionName);
        if (_dashAction == null && ParticipantInputProfileUtility.HasRequiredBinding(_inputProfile, GameplayInputActionRole.Dash))
            ParticipantInputProfileUtility.LogMissingAction(this, nameof(PlayerInputHandler), _inputProfile, "Dash", dashActionName);
        if (_attackAction == null && ParticipantInputProfileUtility.HasRequiredBinding(_inputProfile, GameplayInputActionRole.AttackPrimary))
            ParticipantInputProfileUtility.LogMissingAction(this, nameof(PlayerInputHandler), _inputProfile, "Primary Attack", attackActionName);
        if (_kickAction == null && ParticipantInputProfileUtility.HasRequiredBinding(_inputProfile, GameplayInputActionRole.AttackSecondary))
            ParticipantInputProfileUtility.LogMissingAction(this, nameof(PlayerInputHandler), _inputProfile, "Secondary Attack", secondaryAttackActionName);
        if (_interactAction == null && ParticipantInputProfileUtility.HasRequiredBinding(_inputProfile, GameplayInputActionRole.Interact))
            ParticipantInputProfileUtility.LogMissingAction(this, nameof(PlayerInputHandler), _inputProfile, "Interact", interactActionName);
        if (_blockAction == null && ParticipantInputProfileUtility.HasRequiredBinding(_inputProfile, GameplayInputActionRole.Block))
            ParticipantInputProfileUtility.LogMissingAction(this, nameof(PlayerInputHandler), _inputProfile, "Block", blockActionName);

        return true;
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

        bool isPlaying = IsGameplayActive();

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

        if (_editorKeyboardInput && activeIsKeyboard && hardwareRaw.sqrMagnitude > 0.01f)
        {
            _controller.MoveDirection = hardwareRaw.normalized;
            return;
        }

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

    // Jump and Dash

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.control.device is Gamepad && !_gamepadEnabled) return;
        if (ctx.control.device is Keyboard && !_editorKeyboardInput) return;
        if (!IsGameplayActive()) return;
        if (!TryDispatchGameplayAction(GameplayInputActionRole.Jump))
            _controller.Jump();
    }

    /// <summary>Fired by the InputProfile Dash Action when one is assigned.</summary>
    private void OnDashPerformed(InputAction.CallbackContext ctx)
    {
        // Respect the gamepad/keyboard enabled toggles.
        if (ctx.control.device is Gamepad && !_gamepadEnabled)  return;
        if (ctx.control.device is Keyboard && !_editorKeyboardInput) return;
        if (!IsGameplayActive()) return;
        if (!TryDispatchGameplayAction(GameplayInputActionRole.Dash))
            TriggerDash();
    }

    private bool TryDispatchGameplayAction(GameplayInputActionRole role)
    {
        string actionKey = role.ToString();
        RefreshGameplayActionReceivers();
        if (_gameplayActionReceivers == null)
            return false;

        for (int i = 0; i < _gameplayActionReceivers.Length; i++)
        {
            IActorGameplayActionReceiver receiver = _gameplayActionReceivers[i];
            if (receiver == null)
                continue;

            if (ReferenceEquals(receiver, this))
                continue;

            if (receiver.TryHandleGameplayAction(actionKey))
                return true;
        }

        return false;
    }

    private void RefreshGameplayActionReceivers()
    {
        _gameplayActionReceivers = GetComponentsInChildren<IActorGameplayActionReceiver>(true);
    }

    private void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.control.device is Gamepad && !_gamepadEnabled) return;
        if (ctx.control.device is Keyboard && !_editorKeyboardInput) return;
        if (!IsGameplayActive()) return;
        _combatInputReceiver?.HandlePrimaryAttackInput();
    }

    private void OnKickPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.control.device is Gamepad && !_gamepadEnabled) return;
        if (ctx.control.device is Keyboard && !_editorKeyboardInput) return;
        if (!IsGameplayActive()) return;
        _combatInputReceiver?.HandleSecondaryAttackInput();
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.control.device is Gamepad && !_gamepadEnabled) return;
        if (ctx.control.device is Keyboard && !_editorKeyboardInput) return;
        if (!IsGameplayActive()) return;
        _interactionInputReceiver?.HandleInteractionInput();
    }

    private void OnBlockPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.control.device is Gamepad && !_gamepadEnabled) return;
        if (ctx.control.device is Keyboard && !_editorKeyboardInput) return;
        if (!IsGameplayActive()) return;
        _guardInputReceiver?.HandleGuardStartInput();
    }

    private void OnBlockCanceled(InputAction.CallbackContext ctx)
    {
        if (ctx.control.device is Gamepad && !_gamepadEnabled) return;
        if (ctx.control.device is Keyboard && !_editorKeyboardInput) return;
        if (!IsGameplayActive()) return;
        _guardInputReceiver?.HandleGuardEndInput();
    }

    private bool IsGameplayActive()
    {
        if (_gameplayStateReader != null)
            return _gameplayStateReader.IsGameplayActive;

        if (!_loggedMissingGameplayState)
        {
            _loggedMissingGameplayState = true;
            Debug.LogError("[PlayerInputHandler] Gameplay State Source is not configured. Assign a component that implements IGameplayStateReader or let the scene session configure it.", this);
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
