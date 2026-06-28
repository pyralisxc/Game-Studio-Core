using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Data.Definitions.Combat;
using NeonBlack.Gameplay.Modules.Combat;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Combat
{
    public partial class PawnCombatBehaviour2D
    {
        private bool ExecuteSequenceAction(
            PawnComboProcessor.ComboRuntimeState state,
            CombatSequenceDefinition sequence,
            CombatInputType inputType,
            WeaponData fallbackWeapon,
            string fallbackZoneName,
            ref float cooldownTimer,
            float fallbackCooldown)
        {
            if (Runtime.Motor == null)
                return false;

            if (!_comboProcessor.TryExecuteAction(
                state,
                sequence,
                comboResetTime,
                combatWindow,
                ref _combatTimer,
                Runtime.Motor.IsActing,
                cooldownTimer,
                out int _,
                out CombatActionDefinition action))
            {
                return false;
            }

            WeaponData resolvedWeapon = action.weapon != null ? action.weapon : fallbackWeapon;
            cooldownTimer = ResolveActionCooldown(action, resolvedWeapon, fallbackCooldown);

            Runtime.Motor.ResetMoveToIdle();
            Runtime.Motor.IsActing = true;
            _actingTimer = Mathf.Max(resolvedWeapon != null ? resolvedWeapon.hitDelay + resolvedWeapon.hitDuration : hitDelay + hitDuration, 0.05f);
            UpdateActionState();

            TriggerCombatAnimation(action, inputType);
            ActivateHitBoxForZone(action.fallbackHitBoxZone, resolvedWeapon ?? fallbackWeapon, action.fallbackHitBoxZone);

            return true;
        }

        private void ExecuteFallbackAttack()
        {
            if (Runtime.Motor == null || !CombatActionStateMachine.CanStartActionFrom(Runtime.Motor.IsActing, _attackTimer))
                return;

            _attackTimer = attackCooldown;
            _combatTimer = combatWindow;
            _attackCount = (_attackCount % 3) + 1;
            Runtime.Motor.ResetMoveToIdle();
            Runtime.Motor.IsActing = true;
            _actingTimer = Mathf.Max(attackWeapon != null ? attackWeapon.hitDelay + attackWeapon.hitDuration : hitDelay + hitDuration, 0.05f);
            UpdateActionState();
            PublishCombatResult(new ActorCombatResult(
                ActorCombatResultKind.AttackStarted,
                gameObject,
                animationSignal: ActorAnimationSignal.AttackPrimary,
                step: _attackCount));
            ActivateHitBoxForZone("Punch", attackWeapon);
        }

        private void ExecuteFallbackKick()
        {
            if (Runtime.Motor == null || !CombatActionStateMachine.CanStartActionFrom(Runtime.Motor.IsActing, _kickTimer))
                return;

            _kickTimer = kickCooldown;
            _combatTimer = combatWindow;
            _kickCount = (_kickCount % 3) + 1;
            Runtime.Motor.ResetMoveToIdle();
            Runtime.Motor.IsActing = true;
            _actingTimer = Mathf.Max(kickWeapon != null ? kickWeapon.hitDelay + kickWeapon.hitDuration : hitDelay + hitDuration, 0.05f);
            UpdateActionState();
            PublishCombatResult(new ActorCombatResult(
                ActorCombatResultKind.AttackStarted,
                gameObject,
                animationSignal: ActorAnimationSignal.AttackSecondary,
                step: _kickCount));
            ActivateHitBoxForZone("Kick", kickWeapon);
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
            return inputType == CombatInputType.Secondary
                ? ActorAnimationSignal.AttackSecondary
                : ActorAnimationSignal.AttackPrimary;
        }

        private static float ResolveActionCooldown(CombatActionDefinition action, WeaponData resolvedWeapon, float fallbackCooldown)
        {
            if (action != null && action.cooldownOverride >= 0f)
                return action.cooldownOverride;

            if (resolvedWeapon != null && resolvedWeapon.attackCooldown > 0f)
                return resolvedWeapon.attackCooldown;

            return fallbackCooldown;
        }
    }
}
