using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Data.Definitions.Combat;
using NeonBlack.Gameplay.Modules.Combat;
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
                Runtime.Motor.IsActionLocked,
                cooldownTimer,
                out int _,
                out CombatActionDefinition action))
            {
                return false;
            }

            WeaponData resolvedWeapon = action.weapon != null ? action.weapon : fallbackWeapon;
            cooldownTimer = ResolveActionCooldown(action, resolvedWeapon, fallbackCooldown);

            Runtime.Motor.ResetMoveToIdle();
            Runtime.Motor.SetActionLock(true);
            _actingTimer = Mathf.Max(resolvedWeapon != null ? resolvedWeapon.hitDelay + resolvedWeapon.hitDuration : hitDelay + hitDuration, 0.05f);
            UpdateActionState();

            TriggerCombatAnimation(action, inputType);
            ActivateHitBoxForZone(action.fallbackHitBoxZone, resolvedWeapon ?? fallbackWeapon, action.fallbackHitBoxZone);

            return true;
        }

        private void ExecuteFallbackAttack()
        {
            if (Runtime.Motor == null || !CombatActionStateMachine.CanStartActionFrom(Runtime.Motor.IsActionLocked, _attackTimer))
                return;

            _attackTimer = attackCooldown;
            _combatTimer = combatWindow;
            _attackCount = (_attackCount % 3) + 1;
            Runtime.Motor.ResetMoveToIdle();
            Runtime.Motor.SetActionLock(true);
            _actingTimer = Mathf.Max(attackWeapon != null ? attackWeapon.hitDelay + attackWeapon.hitDuration : hitDelay + hitDuration, 0.05f);
            UpdateActionState();
            Runtime.AnimationDriver?.SetIntSignal(ActorAnimationSignal.AttackPrimary, _attackCount);
            Runtime.AnimationDriver?.TriggerSignal(ActorAnimationSignal.AttackPrimary, intValue: _attackCount);
            ActivateHitBoxForZone("Punch", attackWeapon);
        }

        private void ExecuteFallbackKick()
        {
            if (Runtime.Motor == null || !CombatActionStateMachine.CanStartActionFrom(Runtime.Motor.IsActionLocked, _kickTimer))
                return;

            _kickTimer = kickCooldown;
            _combatTimer = combatWindow;
            _kickCount = (_kickCount % 3) + 1;
            Runtime.Motor.ResetMoveToIdle();
            Runtime.Motor.SetActionLock(true);
            _actingTimer = Mathf.Max(kickWeapon != null ? kickWeapon.hitDelay + kickWeapon.hitDuration : hitDelay + hitDuration, 0.05f);
            UpdateActionState();
            Runtime.AnimationDriver?.SetIntSignal(ActorAnimationSignal.AttackSecondary, _kickCount);
            Runtime.AnimationDriver?.TriggerSignal(ActorAnimationSignal.AttackSecondary, intValue: _kickCount);
            ActivateHitBoxForZone("Kick", kickWeapon);
        }

        private void TriggerCombatAnimation(CombatActionDefinition action, CombatInputType inputType)
        {
            ActorAnimationSignal signal = action != null ? action.animationSignal : ResolveDefaultSignal(inputType);
            int comboStep = action != null ? action.comboStep : 1;

            Runtime.AnimationDriver?.SetIntSignal(signal, comboStep);
            Runtime.AnimationDriver?.TriggerSignal(signal, intValue: comboStep);

            if (action != null && action.finisherResetsCombo)
                Runtime.AnimationDriver?.TriggerCustom("ComboFinisher", intValue: comboStep);
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
