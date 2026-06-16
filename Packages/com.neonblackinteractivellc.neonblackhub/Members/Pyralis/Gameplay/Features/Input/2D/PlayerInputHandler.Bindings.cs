using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Characters;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Features.Input
{
    public partial class PlayerInputHandler
    {
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
                DisableAndUnsubscribeActions();

            _inputActions = inputActions;
            _inputProfile = inputProfile;
            if (_playerInput != null && _playerInput.actions != inputActions)
                _playerInput.actions = inputActions;

            if (!BindActions())
                return;

            if (isActiveAndEnabled && rebindActions)
                EnableAndSubscribeActions();
        }

        private void EnableAndSubscribeActions()
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
            if (_blockAction != null)
            {
                _blockAction.performed += OnBlockPerformed;
                _blockAction.canceled += OnBlockCanceled;
            }
        }

        private void DisableAndUnsubscribeActions()
        {
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
            if (_blockAction != null)
            {
                _blockAction.performed -= OnBlockPerformed;
                _blockAction.canceled -= OnBlockCanceled;
            }

            _moveAction?.Disable();
            _jumpAction?.Disable();
            _dashAction?.Disable();
            _attackAction?.Disable();
            _kickAction?.Disable();
            _interactAction?.Disable();
            _blockAction?.Disable();
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
            else
                ReportMissingInputActionsIfNeeded();

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

        private void ReportMissingInputActionsIfNeeded()
        {
            if (_inputActions != null || _loggedMissingInputActions)
                return;

            _loggedMissingInputActions = true;
            Debug.LogWarning("[PlayerInputHandler] No InputActionAsset is assigned yet. For participant-spawned pawns, assign Actions on the controlling ParticipantDefinition InputProfile; direct scene pawns can assign Actions on PlayerInput.", this);
        }
    }
}
