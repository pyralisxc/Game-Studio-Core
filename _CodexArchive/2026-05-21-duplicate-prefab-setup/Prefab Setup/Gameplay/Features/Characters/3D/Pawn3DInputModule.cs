using UnityEngine;
using UnityEngine.InputSystem;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Config;
using NeonBlack.Gameplay.Features.Input;
using NeonBlack.Gameplay.Characters;

namespace NeonBlack.Gameplay.Features.Characters
{
    /// <summary>
    /// Owns all Input System action binding for a 3D pawn.
    /// Produces a <see cref="FrameInput"/> snapshot each frame for the
    /// <see cref="Motor3D"/> coordinator to distribute to sub-modules.
    ///
    /// This component has no knowledge of movement, traversal, or presentation â€”
    /// it is purely responsible for translating hardware input into a clean data struct.
    ///
    /// Setup:
    ///   â€¢ Attach on the same root as <see cref="Motor3D"/>.
    ///   â€¢ Assign the InputSystem_Actions.inputactions asset, or let the session
    ///     configure it at runtime via <see cref="SetInputConfig"/>.
    /// </summary>
    [AddComponentMenu("NeonBlack/Gameplay/3D/Pawn 3D Input Module")]
    public sealed class Pawn3DInputModule : MonoBehaviour, IPawnInputModule
    {
        [Header("Input")]
        [Tooltip("Drag the InputSystem_Actions.inputactions asset here.")]
        [SerializeField] private InputActionAsset inputActions;
        [Tooltip("Optional shared input config. Overrides Input Actions when assigned.")]
        [SerializeField] private InputConfig inputConfig;

        // â”€â”€ Bound actions â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
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

        // â”€â”€ One-frame flags â€” set by callbacks, cleared after CollectFrameInput â”€ //
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

        /// <summary>True after <see cref="BindActions"/> completes successfully.</summary>
        public bool IsBound { get; private set; }

        // â”€â”€ Unity lifecycle â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
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

        // â”€â”€ Binding â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
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

            _playerMap        = asset.FindActionMap("Player",     throwIfNotFound: true);
            _moveAction       = _playerMap.FindAction("Move",     throwIfNotFound: true);
            _lookAction       = _playerMap.FindAction("Look",     throwIfNotFound: false);
            _jumpAction       = _playerMap.FindAction("Jump",     throwIfNotFound: true);
            _attackAction     = _playerMap.FindAction("Attack",   throwIfNotFound: true);
            _kickAction       = _playerMap.FindAction("Kick",     throwIfNotFound: true);
            _interactAction   = _playerMap.FindAction("Interact", throwIfNotFound: true);
            _sprintAction     = _playerMap.FindAction("Sprint",   throwIfNotFound: true);
            _crouchAction     = _playerMap.FindAction("Crouch",   throwIfNotFound: true);
            _previousAction   = _playerMap.FindAction("Previous", throwIfNotFound: false);
            _nextAction       = _playerMap.FindAction("Next",     throwIfNotFound: false);
            _rollAction       = _playerMap.FindAction("Roll",     throwIfNotFound: false);
            _blockAction      = _playerMap.FindAction("Block",    throwIfNotFound: false);
            _lookAroundAction = _playerMap.FindAction("LookAround", throwIfNotFound: false);
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
            if (inputConfig?.actions != null)                        return inputConfig.actions;
            if (GameplayRuntimeContext.DefaultInputActions != null)  return GameplayRuntimeContext.DefaultInputActions;
            if (_playerInput != null && _playerInput.actions != null) return _playerInput.actions;
            return inputActions;
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

        // â”€â”€ Frame snapshot â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
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

        // â”€â”€ Input callbacks â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
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

        // â”€â”€ Public API â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        /// <summary>Swap the input config at runtime â€” e.g. for per-participant bindings.</summary>
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
            if (profile.actions != null)
                SetInputActions(profile.actions);

            ParticipantInputProfileUtility.ApplyToPlayerInput(_playerInput, profile);
            BindActions();
        }
    }
}
