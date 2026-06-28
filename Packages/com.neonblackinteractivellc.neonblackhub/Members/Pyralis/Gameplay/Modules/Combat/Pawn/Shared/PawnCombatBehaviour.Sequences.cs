using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Data.Definitions.Combat;
using NeonBlack.Gameplay.Modules.Combat;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Animation;

namespace NeonBlack.Gameplay.Modules.Combat
{
    public partial class PawnCombatBehaviour
    {
        private void PerformAerialAttack()
        {
            if (maxAerialAttacks > 0 && _aerialAttackCount >= maxAerialAttacks)
                return;

            if (aerialSequence != null && aerialSequence.actions != null && aerialSequence.actions.Length > 0)
            {
                if (ExecuteSequenceAction(_comboProcessor.AerialState, aerialSequence, CombatInputType.Aerial, WeaponModule.AerialWeapon, aerialHitBoxZone, ref _aerialTimer, attackCooldown))
                    _aerialAttackCount++;
                return;
            }

            if (_aerialTimer > 0f)
                return;

            _aerialAttackCount++;
            _aerialTimer = attackCooldown;
            PublishCombatResult(new ActorCombatResult(
                ActorCombatResultKind.AttackStarted,
                gameObject,
                animationSignal: ActorAnimationSignal.AttackAerial,
                step: _aerialAttackCount));
            ActivateHitBoxForZone(aerialHitBoxZone, WeaponModule.AerialWeapon);
        }

        private bool ExecuteSequenceAction(
            PawnComboProcessor.ComboRuntimeState state,
            CombatSequenceDefinition sequence,
            CombatInputType inputType,
            WeaponData fallbackWeapon,
            string fallbackZoneName,
            ref float cooldownTimer,
            float fallbackCooldown)
        {
            if (_comboProcessor.TryExecuteAction(
                state,
                sequence,
                comboResetTime,
                combatWindow,
                ref _combatTimer,
                Motor.IsActing,
                cooldownTimer,
                out int _,
                out CombatActionDefinition action))
            {
                WeaponData resolvedWeapon = action.weapon != null ? action.weapon : fallbackWeapon;
                cooldownTimer = ResolveActionCooldown(action, resolvedWeapon, fallbackCooldown);
                Motor.IsActing = true;
                UpdateActionState();
                Motor.ResetMoveToIdle();
                TriggerCombatAnimation(action, inputType);
                ActivateHitBoxForZone(action.fallbackHitBoxZone, resolvedWeapon ?? fallbackWeapon ?? WeaponModule.ActiveWeapon, action.fallbackHitBoxZone);
                return true;
            }

            return false;
        }

        private void ExecuteFallbackAttack()
        {
            if (Motor == null || !CombatActionStateMachine.CanStartActionFrom(Motor.IsActing, _attackTimer))
                return;

            _attackTimer = attackCooldown;
            Motor.IsActing = true;
            _combatTimer = combatWindow;
            UpdateActionState();
            _attackCount = (_attackCount % 3) + 1;

            Motor.ResetMoveToIdle();
            PublishCombatResult(new ActorCombatResult(
                ActorCombatResultKind.AttackStarted,
                gameObject,
                animationSignal: ActorAnimationSignal.AttackPrimary,
                step: _attackCount));

            ActivateHitBoxForZone("Punch", WeaponModule.AttackWeapon);
        }

        private void ExecuteFallbackKick()
        {
            if (Motor == null || !CombatActionStateMachine.CanStartActionFrom(Motor.IsActing, _kickTimer))
                return;

            _kickTimer = kickCooldown;
            Motor.IsActing = true;
            _combatTimer = combatWindow;
            UpdateActionState();
            _kickCount = (_kickCount % 3) + 1;

            Motor.ResetMoveToIdle();
            PublishCombatResult(new ActorCombatResult(
                ActorCombatResultKind.AttackStarted,
                gameObject,
                animationSignal: ActorAnimationSignal.AttackSecondary,
                step: _kickCount));

            ActivateHitBoxForZone("Kick", WeaponModule.KickWeapon);
        }

        private void TriggerCombatAnimation(CombatActionDefinition action, CombatInputType inputType)
        {
            ActorAnimationSignal signal = action != null ? action.animationSignal : ResolveDefaultSignal(inputType);
            int comboStep = action != null ? action.comboStep : 1;

            PublishCombatResult(new ActorCombatResult(
                ActorCombatResultKind.AttackStarted,
                gameObject,
                animationSignal: signal,
                step: comboStep,
                isFinisher: action != null && action.finisherResetsCombo));
        }

        private static ActorAnimationSignal ResolveDefaultSignal(CombatInputType inputType)
        {
            return inputType switch
            {
                CombatInputType.Secondary => ActorAnimationSignal.AttackSecondary,
                CombatInputType.Aerial => ActorAnimationSignal.AttackAerial,
                _ => ActorAnimationSignal.AttackPrimary,
            };
        }

        private float ResolveActionCooldown(CombatActionDefinition action, WeaponData resolvedWeapon, float fallbackCooldown)
        {
            if (action != null && action.cooldownOverride >= 0f)
                return action.cooldownOverride;
            if (resolvedWeapon != null && resolvedWeapon.attackCooldown > 0f)
                return resolvedWeapon.attackCooldown;
            return fallbackCooldown;
        }
    }
}
