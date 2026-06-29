using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Data.Definitions.Combat;
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
            WeaponData defaultWeapon,
            ref float cooldownTimer,
            float defaultCooldown)
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

            WeaponData resolvedWeapon = action.weapon != null ? action.weapon : defaultWeapon;
            cooldownTimer = ResolveActionCooldown(action, resolvedWeapon, defaultCooldown);

            Runtime.Motor.ResetMoveToIdle();
            Runtime.Motor.IsActing = true;
            _actingTimer = Mathf.Max(resolvedWeapon != null ? resolvedWeapon.hitDelay + resolvedWeapon.hitDuration : hitDelay + hitDuration, 0.05f);
            UpdateActionState();

            TriggerCombatAnimation(action, inputType);
            ActivateHitBoxForZone(action.hitBoxZone, resolvedWeapon ?? defaultWeapon, action.hitBoxZone);

            return true;
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

        private static float ResolveActionCooldown(CombatActionDefinition action, WeaponData resolvedWeapon, float defaultCooldown)
        {
            if (action != null && action.cooldownOverride >= 0f)
                return action.cooldownOverride;

            if (resolvedWeapon != null && resolvedWeapon.attackCooldown > 0f)
                return resolvedWeapon.attackCooldown;

            return defaultCooldown;
        }
    }
}
