using System;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    public class PawnComboProcessor
    {
        public sealed class ComboRuntimeState
        {
            public CombatSequenceDefinition Sequence;
            public CombatActionDefinition ActiveAction;
            public int CurrentIndex = -1;
            public float Timer;
            public bool AllowNextBranch;
            public bool WaitingForHitConfirm;

            public void Reset()
            {
                ActiveAction = null;
                CurrentIndex = -1;
                Timer = 0f;
                AllowNextBranch = false;
                WaitingForHitConfirm = false;
            }
        }

        private readonly ComboRuntimeState _primaryState = new ComboRuntimeState();
        private readonly ComboRuntimeState _secondaryState = new ComboRuntimeState();
        private readonly ComboRuntimeState _aerialState = new ComboRuntimeState();
        private ComboRuntimeState _currentSequenceState;

        public ComboRuntimeState PrimaryState => _primaryState;
        public ComboRuntimeState SecondaryState => _secondaryState;
        public ComboRuntimeState AerialState => _aerialState;
        public ComboRuntimeState CurrentSequenceState => _currentSequenceState;

        public void Tick(float deltaTime, float comboResetTime)
        {
            TickState(_primaryState, deltaTime, comboResetTime);
            TickState(_secondaryState, deltaTime, comboResetTime);
            TickState(_aerialState, deltaTime, comboResetTime);
        }

        private void TickState(ComboRuntimeState state, float deltaTime, float comboResetTime)
        {
            if (state == null || state.Timer <= 0f)
                return;

            state.Timer -= deltaTime;
            if (state.Timer > 0f)
                return;

            state.Reset();
            if (_currentSequenceState == state)
                _currentSequenceState = null;
        }

        public bool TryExecuteAction(
            ComboRuntimeState state,
            CombatSequenceDefinition sequence,
            float comboResetTime,
            float combatWindow,
            ref float combatTimer,
            bool isActing,
            float cooldownTimer,
            out int nextIndex,
            out CombatActionDefinition action)
        {
            nextIndex = -1;
            action = null;

            if (sequence == null || sequence.actions == null || sequence.actions.Length == 0)
                return false;

            bool canBranch = state.CurrentIndex >= 0 && state.Timer > 0f && state.AllowNextBranch;
            nextIndex = canBranch ? state.CurrentIndex + 1 : 0;
            if (nextIndex >= sequence.actions.Length)
                nextIndex = sequence.restartFromFirstActionWhenBranchFails ? 0 : sequence.actions.Length - 1;

            action = sequence.actions[nextIndex];
            if (action == null)
                return false;

            if (isActing || cooldownTimer > 0f)
                return false;

            combatTimer = combatWindow;
            state.Sequence = sequence;
            state.ActiveAction = action;
            state.CurrentIndex = nextIndex;
            state.Timer = action.comboWindow > 0f ? action.comboWindow : comboResetTime;
            state.AllowNextBranch = !action.requiresHitConfirmForNextBranch;
            state.WaitingForHitConfirm = action.requiresHitConfirmForNextBranch;

            _currentSequenceState = state;

            if (action.finisherResetsCombo || (nextIndex >= sequence.actions.Length - 1 && sequence.resetAfterFinalAction))
            {
                state.AllowNextBranch = false;
                state.WaitingForHitConfirm = false;
                state.CurrentIndex = -1;
            }

            return true;
        }

        public void HandleHitConfirmed(float comboResetTime, Action<int, bool> onComboUpdate)
        {
            if (_currentSequenceState == null || !_currentSequenceState.WaitingForHitConfirm)
                return;

            _currentSequenceState.WaitingForHitConfirm = false;
            _currentSequenceState.AllowNextBranch = true;
            _currentSequenceState.Timer = Mathf.Max(_currentSequenceState.Timer, comboResetTime);
            
            if (_currentSequenceState.ActiveAction != null)
            {
                onComboUpdate?.Invoke(_currentSequenceState.ActiveAction.comboStep, _currentSequenceState.ActiveAction.finisherResetsCombo);
            }
        }

        public void ResetPrimary() => _primaryState.Reset();
        public void ResetSecondary() => _secondaryState.Reset();
        public void ResetAerial() => _aerialState.Reset();
    }
}
