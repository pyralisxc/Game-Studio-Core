using NeonBlack.Gameplay.Data.Definitions.Combat;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Animation;

namespace NeonBlack.Gameplay.Modules.Combat
{
    public partial class EnemyCombatModule
    {
        public void ExecuteAttack(float distanceToPlayer)
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

            _attackTriggerHashes.TryGetValue(atk, out int triggerHash);
            PublishCombatResult(new ActorCombatResult(
                ActorCombatResultKind.AttackStarted,
                gameObject,
                animationSignal: atk.useCustomAnimationKey ? ActorAnimationSignal.Custom : atk.animationSignal,
                step: atk.animationStep,
                customAnimationKey: atk.useCustomAnimationKey ? atk.customAnimationKey : null,
                animatorTriggerHash: triggerHash));

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
