using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Modules.Input
{
    public partial class PlayerInputHandler
    {
        private void OnJumpPerformed(InputAction.CallbackContext ctx)
        {
            if (ctx.control.device is Gamepad && !_gamepadEnabled) return;
            if (ctx.control.device is Keyboard && !_editorKeyboardInput) return;
            if (!IsGameplayActive()) return;
            if (!TryDispatchGameplayAction(GameplayInputActionRole.Jump))
                _movementInputReceiver?.Jump();
        }

        private void OnDashPerformed(InputAction.CallbackContext ctx)
        {
            if (ctx.control.device is Gamepad && !_gamepadEnabled) return;
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
            _combatRequestReceiver?.TryHandleCombatCommand(new ActorCombatCommand(ActorCombatCommandKind.PrimaryAttack, gameObject));
        }

        private void OnKickPerformed(InputAction.CallbackContext ctx)
        {
            if (ctx.control.device is Gamepad && !_gamepadEnabled) return;
            if (ctx.control.device is Keyboard && !_editorKeyboardInput) return;
            if (!IsGameplayActive()) return;
            _combatRequestReceiver?.TryHandleCombatCommand(new ActorCombatCommand(ActorCombatCommandKind.SecondaryAttack, gameObject));
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
    }
}
