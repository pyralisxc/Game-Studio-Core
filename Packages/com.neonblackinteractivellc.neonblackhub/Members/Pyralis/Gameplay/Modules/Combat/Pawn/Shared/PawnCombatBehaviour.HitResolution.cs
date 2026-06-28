using NeonBlack.Gameplay.Modules.Combat;
using NeonBlack.Gameplay.Data.Definitions.Combat;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Combat
{
    public partial class PawnCombatBehaviour
    {
        private void ActivateHitBoxForZone(string defaultZoneName, WeaponData weapon, string explicitZoneName = null)
        {
            if (weapon != null && (weapon.weaponType == WeaponType.Ranged || weapon.weaponType == WeaponType.Thrown) && weapon.projectileDefinition != null)
            {
                ProjectileModule?.FireProjectile(
                    weapon,
                    Motor?.FacingRight ?? true,
                    DamageModule != null ? DamageModule.DamageHandler.OutgoingDamageMultiplier : 1.0f,
                    DamageModule != null ? DamageModule.DamageHandler.OutgoingKnockbackMultiplier : 1.0f);
                return;
            }

            string zoneName = !string.IsNullOrEmpty(explicitZoneName)
                ? explicitZoneName
                : (weapon != null && !string.IsNullOrEmpty(weapon.hitBoxZone) ? weapon.hitBoxZone : defaultZoneName);

            float damage = DamageModule != null
                ? DamageModule.GetModifiedDamage(weapon != null ? weapon.damage : 10f)
                : 10f;
            float knockback = DamageModule != null
                ? DamageModule.GetModifiedKnockback(weapon != null ? weapon.knockbackForce : 5f)
                : 5f;
            float delay = weapon != null ? weapon.hitDelay : 0.1f;
            float duration = weapon != null ? weapon.hitDuration : 0.15f;

            HitBoxModule?.SyncHitBoxSides(Motor?.FacingRight ?? true);
            HitBoxModule?.ActivateHitBox(zoneName, damage, knockback, delay, duration);
        }

        private void HandleHitConfirmed(GameObject _)
        {
            _comboProcessor.HandleHitConfirmed(comboResetTime, (step, isFinisher) =>
            {
                PublishCombatResult(new ActorCombatResult(
                    ActorCombatResultKind.ComboConfirmed,
                    gameObject,
                    customAnimationKey: "ComboConfirm",
                    step: step,
                    isFinisher: isFinisher));
                FeedbackPublisher?.PublishCombo(step);
                if (isFinisher)
                    FeedbackPublisher?.PublishFinisher(step);
            });
        }

        private void SubscribeHitBoxes()
        {
            if (HitBoxModule == null || HitBoxModule.HitBoxZones == null)
                return;

            foreach (HitBoxSlot slot in HitBoxModule.HitBoxZones)
            {
                if (slot?.hitBox != null)
                    slot.hitBox.HitConfirmed += HandleHitConfirmed;
            }
        }

        private void UnsubscribeHitBoxes()
        {
            if (HitBoxModule == null || HitBoxModule.HitBoxZones == null)
                return;

            foreach (HitBoxSlot slot in HitBoxModule.HitBoxZones)
            {
                if (slot?.hitBox != null)
                    slot.hitBox.HitConfirmed -= HandleHitConfirmed;
            }
        }
    }
}
