using NeonBlack.Gameplay.Features.Combat;

namespace NeonBlack.Gameplay.Features.Enemies
{
    public partial class EnemyCombatModule
    {
        public void ExecuteAttack(float distanceToPlayer, EnemyAnimationModule animationModule)
        {
            EnemyAttack atk = _combatProcessor.PickNextAttack(
                attackSequence,
                attackMode,
                usePrioritySelection,
                attackPriorityProfile,
                preferAttacksCurrentlyInRange,
                distanceToPlayer,
                ref _sequenceIndex,
                rangeWeight,
                damageWeight,
                knockbackWeight,
                assetPriorityWeight,
                GetAttackEffectiveRange);

            if (atk == null) return;

            _attackTimer = atk.attackCooldown > 0f ? atk.attackCooldown : attackCooldown;

            animationModule.TriggerAttack(atk, _attackTriggerHashes);

            HitBox box = GetZoneHitBox(atk.hitBoxZone);
            if (box == null && hitBoxZones != null && hitBoxZones.Length > 0)
                box = hitBoxZones[0].hitBox;

            if (box != null)
            {
                if (!_hitBoxOriginalScales.ContainsKey(box))
                    _hitBoxOriginalScales[box] = box.transform.localScale;

                StartCoroutine(_combatProcessor.EnemyHitBoxRoutine(
                    box,
                    atk.damage * _outgoingDamageMultiplier,
                    atk.knockbackForce * _outgoingKnockbackMultiplier,
                    atk.hitDelay,
                    atk.hitDuration,
                    atk.attackRadius));
            }
        }
    }
}
