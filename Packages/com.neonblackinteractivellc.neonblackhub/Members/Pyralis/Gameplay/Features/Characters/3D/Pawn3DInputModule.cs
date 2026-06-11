using UnityEngine;
using UnityEngine.InputSystem;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Config;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Input;
using NeonBlack.Gameplay.Characters;

namespace NeonBlack.Gameplay.Features.Characters
{
    /// <summary>
    /// Owns all Input System action binding for a 3D pawn.
    /// Produces a <see cref="FrameInput"/> snapshot each frame for the
    /// <see cref="Motor3D"/> coordinator to distribute to sub-modules.
    ///
    /// This component has no knowledge of movement, traversal, or presentation 
    /// it is purely responsible for translating hardware input into a clean data struct.
    ///
    /// Setup:
    ///    Attach on the same root as <see cref="Motor3D"/>.
    ///    Assign the InputSystem_Actions.inputactions asset, or let the session
    ///     configure it at runtime via <see cref="SetInputConfig"/>.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Input,
        Relevance = "Translates Unity Input System actions into Pawn-readable FrameInput data.",
        Axioms = AuthoringWorldAxiom.Realtime | AuthoringWorldAxiom.Dimensions3D,
        NativeSetup = new[] { "Attach to the same root as Motor3D.", "Assign an InputActionAsset." },
        AssignmentFields = new[] { nameof(inputActions), nameof(inputConfig) },
        FirstProof = "Verify movement and actions respond in Play Mode with the assigned Input Asset.",
        ExpertAdvice = "Converts hardware signals into FrameInput. It uses the InputProfile to find action names. Ensure your InputActionAsset has the 'Player' map (or as defined in your profile).",
        DocumentationURL = "https://docs.neonblack.com/pyralis/input"
    )]
[AddComponentMenu("NeonBlack/Gameplay/3D/Pawn 3D Input Module")]
    public sealed class Pawn3DInputModule : MonoBehaviour, IPawnInputModule
    {
        [Header("Input")]
        [Tooltip("Drag the InputSystem_Actions.inputactions asset here.")]
        [SerializeField] private InputActionAsset inputActions;
        [Tooltip("Optional shared input config. Overrides Input Actions when assigned.")]
        [SerializeField] private InputConfig inputConfig;

        //  Bound actions  //
        private InputActionMap _playerMap;
        private InputAction    _moveAction;
        private InputAction    _lookAction;
        private InputAction    _jumpAction;
        private InputAction    _attackAction;
        private InputAction    _kickAction;
        private InputAction    _interactAction;
        private InputAction    _sprintAction;
        private InputAction    _crouchAction;
        private InputAction    _previousAction;
        private InputAction    _nextAction;
        private InputAction    _rollAction;
        private InputAction    _blockAction;
        private InputAction    _lookAroundAction;

        //  One-frame flags  set by callbacks, cleared after CollectFrameInput  //
        private bool _jumpPressed;
        private bool _jumpReleased;
        private bool _crouchPressed;
        private bool _crouchReleased;
        private bool _rollPressed;
        private bool _attackPressed;
        private bool _kickPressed;
        private bool _interactPressed;
        private bool _blockPressed;
        private bool _blockReleased;
        private bool _lookAroundPressed;
        private bool _lookAroundReleased;
        private int  _weaponCycleDelta;
        private PlayerInput _playerInput;
        private InputProfile _inputProfile;

        /// <summary>True after <see cref="BindActions"/> completes successfully.</summary>
        public bool IsBound { get; private set; }

        //  Unity lifecycle  //
        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            BindActions();
        }

        private void OnEnable()
        {
            if (!IsBound) BindActions();
            _playerMap?.Enable();
            SubscribeCallbacks();
        }

        private void OnDisable()
        {
            UnsubscribeCallbacks();
            _playerMap?.Disable();
        }

        //  Binding  //
        private void BindActions()
        {
            if (IsBound)
            {
                UnsubscribeCallbacks();
                _playerMap?.Disable();
                IsBound = false;
            }

            InputActionAsset asset = ResolveAsset();
            if (asset == null)
            {
                Debug.LogError("[Pawn3DInputModule] No InputActionAsset assigned.", this);
                enabled = false;
                return;
            }

            _playerMap = ParticipantInputProfileUtility.FindGameplayActionMap(asset, _inputProfile);
            if (_playerMap == null)
            {
                string mapName = _inputProfile != null && !string.IsNullOrWhiteSpace(_inputProfile.primaryActionMap)
                    ? _inputProfile.primaryActionMap
                    : "Player";
                Debug.LogError($"[Pawn3DInputModule] '{mapName}' action map not found in the assigned InputActionAsset.", this);
                enabled = false;
                return;
            }

            string moveActionName = ParticipantInputProfileUtility.GetActionName(_inputProfile, GameplayInputActionRole.Move);
            string lookActionName = ParticipantInputProfileUtility.GetActionName(_inputProfile, GameplayInputActionRole.Look);
            string jumpActionName = ParticipantInputProfileUtility.GetActionName(_inputProfile, GameplayInputActionRole.Jump);
            string attackActionName = ParticipantInputProfileUtility.GetActionName(_inputProfile, GameplayInputActionRole.AttackPrimary);
            string secondaryAttackActionName = ParticipantInputProfileUtility.GetActionName(_inputProfile, GameplayInputActionRole.AttackSecondary);
            string interactActionName = ParticipantInputProfileUtility.GetActionName(_inputProfile, GameplayInputActionRole.Interact);
            string sprintActionName = ParticipantInputProfileUtility.GetActionName(_inputProfile, GameplayInputActionRole.Sprint);
            string crouchActionName = ParticipantInputProfileUtility.GetActionName(_inputProfile, GameplayInputActionRole.Crouch);
            string previousActionName = ParticipantInputProfileUtility.GetActionName(_inputProfile, GameplayInputActionRole.Previous);
            string nextActionName = ParticipantInputProfileUtility.GetActionName(_inputProfile, GameplayInputActionRole.Next);
            string rollActionName = ParticipantInputProfileUtility.GetActionName(_inputProfile, GameplayInputActionRole.Roll);
            string blockActionName = ParticipantInputProfileUtility.GetActionName(_inputProfile, GameplayInputActionRole.Block);
            string lookAroundActionName = ParticipantInputProfileUtility.GetActionName(_inputProfile, GameplayInputActionRole.LookAround);

            _moveAction       = ParticipantInputProfileUtility.FindAction(_playerMap, _inputProfile, GameplayInputActionRole.Move);
            _lookAction       = ParticipantInputProfileUtility.FindAction(_playerMap, _inputProfile, GameplayInputActionRole.Look);
            _jumpAction       = ParticipantInputProfileUtility.FindAction(_playerMap, _inputProfile, GameplayInputActionRole.Jump);
            _attackAction     = ParticipantInputProfileUtility.FindAction(_playerMap, _inputProfile, GameplayInputActionRole.AttackPrimary);
            _kickAction       = ParticipantInputProfileUtility.FindAction(_playerMap, _inputProfile, GameplayInputActionRole.AttackSecondary);
            _interactAction   = ParticipantInputProfileUtility.FindAction(_playerMap, _inputProfile, GameplayInputActionRole.Interact);
            _sprintAction     = ParticipantInputProfileUtility.FindAction(_playerMap, _inputProfile, GameplayInputActionRole.Sprint);
            _crouchAction     = ParticipantInputProfileUtility.FindAction(_playerMap, _inputProfile, GameplayInputActionRole.Crouch);
            _previousAction   = ParticipantInputProfileUtility.FindAction(_playerMap, _inputProfile, GameplayInputActionRole.Previous);
            _nextAction       = ParticipantInputProfileUtility.FindAction(_playerMap, _inputProfile, GameplayInputActionRole.Next);
            _rollAction       = ParticipantInputProfileUtility.FindAction(_playerMap, _inputProfile, GameplayInputActionRole.Roll);
            _blockAction      = ParticipantInputProfileUtility.FindAction(_playerMap, _inputProfile, GameplayInputActionRole.Block);
            _lookAroundAction = ParticipantInputProfileUtility.FindAction(_playerMap, _inputProfile, GameplayInputActionRole.LookAround);

            LogMissingRequiredAction("Move", moveActionName, _moveAction);
            LogMissingRequiredAction(GameplayInputActionRole.Jump, "Jump", jumpActionName, _jumpAction);
            LogMissingRequiredAction(GameplayInputActionRole.AttackPrimary, "Primary Attack", attackActionName, _attackAction);
            LogMissingRequiredAction(GameplayInputActionRole.AttackSecondary, "Secondary Attack", secondaryAttackActionName, _kickAction);
            LogMissingRequiredAction(GameplayInputActionRole.Interact, "Interact", interactActionName, _interactAction);
            LogMissingRequiredAction(GameplayInputActionRole.Sprint, "Sprint", sprintActionName, _sprintAction);
            LogMissingRequiredAction(GameplayInputActionRole.Crouch, "Crouch", crouchActionName, _crouchAction);
            IsBound = true;

            if (isActiveAndEnabled)
            {
                _playerMap?.Enable();
                SubscribeCallbacks();
            }
        }

        private InputActionAsset ResolveAsset()
        {
            _playerInput ??= GetComponent<PlayerInput>();
            
            // Priority 1: PlayerInput. This is critical for local multi-device isolation.
            // When PlayerInput is used, Unity creates a per-user instance of the asset.
            if (_playerInput != null && _playerInput.actions != null) 
                return _playerInput.actions;
                
            // Priority 2: Local inspector override (mostly for testing/special cases).
            if (inputActions != null) 
                return inputActions;

            // Priority 3: Config-based override.
            if (inputConfig?.actions != null) 
                return inputConfig.actions;

            // Priority 4: Global session default (Compatibility fallback).
            if (GameplayRuntimeContext.DefaultInputActions != null)
            {
                _inputProfile ??= GameplayRuntimeContext.DefaultInputProfile;
                return GameplayRuntimeContext.DefaultInputActions;
            }

            return null;
        }

        private void LogMissingRequiredAction(string actionRole, string actionName, InputAction action)
        {
            if (action != null)
                return;

            ParticipantInputProfileUtility.LogMissingAction(this, nameof(Pawn3DInputModule), _inputProfile, actionRole, actionName);
        }

        private void LogMissingRequiredAction(GameplayInputActionRole role, string actionRole, string actionName, InputAction action)
        {
            if (action != null || !ParticipantInputProfileUtility.HasRequiredBinding(_inputProfile, role))
                return;

            ParticipantInputProfileUtility.LogMissingAction(this, nameof(Pawn3DInputModule), _inputProfile, actionRole, actionName);
        }

        private void SubscribeCallbacks()
        {
            if (_jumpAction      != null) { _jumpAction.performed      += OnJump;            _jumpAction.canceled      += OnJumpReleased;         }
            if (_attackAction    != null)   _attackAction.performed    += OnAttack;
            if (_kickAction      != null)   _kickAction.performed      += OnKick;
            if (_interactAction  != null)   _interactAction.performed  += OnInteract;
            if (_crouchAction    != null) { _crouchAction.performed    += OnCrouchPerformed; _crouchAction.canceled    += OnCrouchCanceled;       }
            if (_previousAction  != null)   _previousAction.performed  += OnPrevious;
            if (_nextAction      != null)   _nextAction.performed      += OnNext;
            if (_rollAction      != null)   _rollAction.performed      += OnRoll;
            if (_blockAction     != null) { _blockAction.performed     += OnBlockPerformed;  _blockAction.canceled     += OnBlockCanceled;        }
            if (_lookAroundAction != null) { _lookAroundAction.performed += OnLookAroundPerformed; _lookAroundAction.canceled += OnLookAroundCanceled; }
        }

        private void UnsubscribeCallbacks()
        {
            if (_jumpAction      != null) { _jumpAction.performed      -= OnJump;            _jumpAction.canceled      -= OnJumpReleased;         }
            if (_attackAction    != null)   _attackAction.performed    -= OnAttack;
            if (_kickAction      != null)   _kickAction.performed      -= OnKick;
            if (_interactAction  != null)   _interactAction.performed  -= OnInteract;
            if (_crouchAction    != null) { _crouchAction.performed    -= OnCrouchPerformed; _crouchAction.canceled    -= OnCrouchCanceled;       }
            if (_previousAction  != null)   _previousAction.performed  -= OnPrevious;
            if (_nextAction      != null)   _nextAction.performed      -= OnNext;
            if (_rollAction      != null)   _rollAction.performed      -= OnRoll;
            if (_blockAction     != null) { _blockAction.performed     -= OnBlockPerformed;  _blockAction.canceled     -= OnBlockCanceled;        }
            if (_lookAroundAction != null) { _lookAroundAction.performed -= OnLookAroundPerformed; _lookAroundAction.canceled -= OnLookAroundCanceled; }
        }

        //  Frame snapshot  //
        /// <summary>
        /// Snapshot all current input values into a <see cref="FrameInput"/> and clear
        /// the one-frame button flags. Call exactly once per Update from <see cref="Motor3D"/>.
        /// </summary>
        public FrameInput CollectFrameInput()
        {
            var fi = new FrameInput
            {
                Move               = _moveAction?.ReadValue<Vector2>()   ?? Vector2.zero,
                Look               = _lookAction?.ReadValue<Vector2>()   ?? Vector2.zero,
                SprintHeld         = (_sprintAction?.ReadValue<float>()  ?? 0f) > 0.5f,
                JumpPressed        = _jumpPressed,
                JumpReleased       = _jumpReleased,
                CrouchPressed      = _crouchPressed,
                CrouchReleased     = _crouchReleased,
                RollPressed        = _rollPressed,
                AttackPressed      = _attackPressed,
                KickPressed        = _kickPressed,
                InteractPressed    = _interactPressed,
                BlockPressed       = _blockPressed,
                BlockReleased      = _blockReleased,
                LookAroundPressed  = _lookAroundPressed,
                LookAroundReleased = _lookAroundReleased,
                WeaponCycleDelta   = _weaponCycleDelta,
            };
            ClearOneFrameFlags();
            return fi;
        }

        private void ClearOneFrameFlags()
        {
            _jumpPressed = _jumpReleased = false;
            _crouchPressed = _crouchReleased = false;
            _rollPressed = _attackPressed = _kickPressed = _interactPressed = false;
            _blockPressed = _blockReleased = false;
            _lookAroundPressed = _lookAroundReleased = false;
            _weaponCycleDelta = 0;
        }

        //  Input callbacks  //
        private void OnJump(InputAction.CallbackContext _)               => _jumpPressed        = true;
        private void OnJumpReleased(InputAction.CallbackContext _)       => _jumpReleased       = true;
        private void OnAttack(InputAction.CallbackContext _)             => _attackPressed      = true;
        private void OnKick(InputAction.CallbackContext _)               => _kickPressed        = true;
        private void OnInteract(InputAction.CallbackContext _)           => _interactPressed    = true;
        private void OnCrouchPerformed(InputAction.CallbackContext _)    => _crouchPressed      = true;
        private void OnCrouchCanceled(InputAction.CallbackContext _)     => _crouchReleased     = true;
        private void OnRoll(InputAction.CallbackContext _)               => _rollPressed        = true;
        private void OnBlockPerformed(InputAction.CallbackContext _)     => _blockPressed       = true;
        private void OnBlockCanceled(InputAction.CallbackContext _)      => _blockReleased      = true;
        private void OnLookAroundPerformed(InputAction.CallbackContext _) => _lookAroundPressed  = true;
        private void OnLookAroundCanceled(InputAction.CallbackContext _)  => _lookAroundReleased = true;
        private void OnPrevious(InputAction.CallbackContext _)           => _weaponCycleDelta   = -1;
        private void OnNext(InputAction.CallbackContext _)               => _weaponCycleDelta   = +1;

        //  Public API  //
        /// <summary>Swap the input config at runtime  e.g. for per-participant bindings.</summary>
        public void SetInputConfig(InputConfig config, bool overrideExisting = true)
        {
            if (config?.actions == null) return;
            if (!overrideExisting && inputConfig?.actions != null) return;
            inputConfig  = config;
            inputActions = config.actions;
            BindActions();
        }

        /// <summary>Swap the raw InputActionAsset at runtime.</summary>
        public void SetInputActions(InputActionAsset actions, bool overrideExisting = true)
        {
            if (actions == null) return;
            if (!overrideExisting && inputActions != null) return;
            inputConfig  = null;
            inputActions = actions;
            BindActions();
        }

        public void ApplyInputProfile(PawnProfileApplicationContext context, InputProfile profile)
        {
            if (profile == null)
                return;

            _playerInput ??= GetComponent<PlayerInput>();
            profile.Sanitize();
            _inputProfile = profile;
            if (profile.actions != null)
                SetInputActions(profile.actions);

            ParticipantInputProfileUtility.ApplyToPlayerInput(_playerInput, profile);
            BindActions();
        }
    }
}
