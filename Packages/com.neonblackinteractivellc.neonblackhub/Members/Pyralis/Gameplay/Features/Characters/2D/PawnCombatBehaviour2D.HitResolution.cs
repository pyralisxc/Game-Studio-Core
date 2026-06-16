using System.Collections;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Composition;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    public partial class PawnCombatBehaviour2D
    {
        private void ActivateHitBoxForZone(string defaultZoneName, WeaponData weapon, string explicitZoneName = null)
        {
            if (weapon != null
                && (weapon.weaponType == WeaponType.Ranged || weapon.weaponType == WeaponType.Thrown)
                && weapon.projectileDefinition != null)
            {
                FireProjectile(weapon);
                return;
            }

            string zoneName = !string.IsNullOrEmpty(explicitZoneName)
                ? explicitZoneName
                : weapon != null && !string.IsNullOrEmpty(weapon.hitBoxZone)
                    ? weapon.hitBoxZone
                    : defaultZoneName;

            HitBox2D box = GetZoneByName(zoneName) ?? GetZoneByName(defaultZoneName);
            if (box == null)
                return;

            float damage = (weapon != null ? weapon.damage : baseDamage) * _outgoingDamageMultiplier;
            float knockback = (weapon != null ? weapon.knockbackForce : baseKnockback) * _outgoingKnockbackMultiplier;
            float delay = weapon != null ? weapon.hitDelay : hitDelay;
            float duration = weapon != null ? weapon.hitDuration : hitDuration;

            SyncHitBoxSides();

            StartCoroutine(HitBoxTimingRoutine(box, damage, knockback, delay, duration));
        }

        private IEnumerator HitBoxTimingRoutine(HitBox2D box, float damage, float knockback, float delay, float duration)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            box.ConfigureDamage(damage, knockback);
            box.Fire(duration);
        }

        private void FireProjectile(WeaponData weapon)
        {
            if (weapon == null || weapon.projectileDefinition == null)
                return;

            ProjectileLauncher2D launcher = ResolveProjectileLauncher();
            if (launcher == null)
            {
                Debug.LogWarning($"{nameof(PawnCombatBehaviour2D)} needs a {nameof(ProjectileLauncher2D)} to fire ranged weapon `{weapon.weaponName}` through the authored projectile path.", this);
                return;
            }

            bool facingRight = Runtime.Motor == null || Runtime.Motor.FacingRight;
            Vector3 spawnPos = projectileSpawnPoint != null
                ? projectileSpawnPoint.position
                : transform.position + Vector3.up * 0.25f + (facingRight ? Vector3.right : Vector3.left) * 0.5f;

            Vector3 forward = facingRight ? Vector3.right : Vector3.left;
            ProjectileFireRequest request = new ProjectileFireRequest(
                weapon.projectileDefinition,
                weapon.fireModeDefinition,
                spawnPos,
                forward,
                gameObject,
                Runtime.Health != null ? Runtime.Health.faction : Faction.Neutral,
                damageMultiplier: _outgoingDamageMultiplier,
                knockbackMultiplier: _outgoingKnockbackMultiplier);

            launcher.Fire(request);
        }

        private ProjectileLauncher2D ResolveProjectileLauncher()
        {
            ProjectileLauncher2D launcher = Runtime.ResolveProjectileLauncher(transform, projectileLauncher);
            projectileLauncher = launcher;
            return launcher;
        }

        private HitBox2D GetZoneByName(string zoneName)
        {
            if (hitBoxZones == null || string.IsNullOrEmpty(zoneName))
                return null;

            foreach (HitBoxSlot2D slot in hitBoxZones)
            {
                if (slot != null && slot.zoneName == zoneName)
                    return slot.hitBox;
            }

            return null;
        }

        private void SyncHitBoxSides()
        {
            if (Runtime.Motor == null || hitBoxZones == null)
                return;

            foreach (HitBoxSlot2D slot in hitBoxZones)
                slot?.MirrorToSide(transform, Runtime.Motor.FacingRight);
        }

        private void HandleHitConfirmed(GameObject _)
        {
            _comboProcessor.HandleHitConfirmed(comboResetTime, (step, isFinisher) =>
            {
                Runtime.AnimationDriver?.TriggerCustom("ComboConfirm", intValue: step);
                Runtime.FeedbackPublisher?.PublishCombo(step);
                if (isFinisher)
                    Runtime.FeedbackPublisher?.PublishFinisher(step);
            });
        }

        private void CacheHitBoxOffsets()
        {
            if (hitBoxZones == null)
                return;

            foreach (HitBoxSlot2D slot in hitBoxZones)
            {
                slot.absOffsetX = slot.hitBox != null
                    ? Mathf.Max(Mathf.Abs(slot.hitBox.transform.position.x - transform.position.x), 0.25f)
                    : 0.25f;
            }
        }

        private void SubscribeHitBoxes()
        {
            if (hitBoxZones == null)
                return;

            foreach (HitBoxSlot2D slot in hitBoxZones)
            {
                if (slot?.hitBox != null)
                    slot.hitBox.HitConfirmed += HandleHitConfirmed;
            }
        }

        private void UnsubscribeHitBoxes()
        {
            if (hitBoxZones == null)
                return;

            foreach (HitBoxSlot2D slot in hitBoxZones)
            {
                if (slot?.hitBox != null)
                    slot.hitBox.HitConfirmed -= HandleHitConfirmed;
            }
        }
    }
}
