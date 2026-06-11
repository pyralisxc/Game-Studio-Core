using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Presentation.Animation;

namespace NeonBlack.Gameplay.Features.Enemies
{
    public class EnemyCombatProcessor
    {
        public enum AttackPriorityProfile
        {
            LongestRange,
            HighestDamage,
            HighestKnockback,
            HighestAssetPriority,
            WeightedScore
        }

        private readonly List<EnemyAttack> _attackCandidates = new List<EnemyAttack>(8);

        public EnemyAttack PickNextAttack(
            EnemyAttack[] attackSequence,
            AttackMode attackMode,
            bool usePrioritySelection,
            AttackPriorityProfile attackPriorityProfile,
            bool preferAttacksCurrentlyInRange,
            float distToPlayer,
            ref int sequenceIndex,
            float rangeWeight,
            float damageWeight,
            float knockbackWeight,
            float assetPriorityWeight,
            System.Func<EnemyAttack, float> getAttackEffectiveRange)
        {
            if (attackSequence == null || attackSequence.Length == 0) return null;

            _attackCandidates.Clear();
            foreach (var a in attackSequence)
            {
                if (a == null) continue;
                float effectiveRange = getAttackEffectiveRange(a);
                if (!preferAttacksCurrentlyInRange || distToPlayer <= effectiveRange)
                    _attackCandidates.Add(a);
            }

            if (_attackCandidates.Count == 0)
            {
                foreach (var a in attackSequence)
                    if (a != null) _attackCandidates.Add(a);
            }
            if (_attackCandidates.Count == 0) return null;

            if (!usePrioritySelection)
                return PickByPatternOnly(attackSequence, _attackCandidates, attackMode, ref sequenceIndex);

            return PickByPriority(
                attackSequence, 
                _attackCandidates, 
                attackMode, 
                attackPriorityProfile, 
                distToPlayer, 
                ref sequenceIndex,
                rangeWeight,
                damageWeight,
                knockbackWeight,
                assetPriorityWeight,
                getAttackEffectiveRange);
        }

        private EnemyAttack PickByPatternOnly(EnemyAttack[] attackSequence, List<EnemyAttack> candidates, AttackMode attackMode, ref int sequenceIndex)
        {
            if (candidates == null || candidates.Count == 0) return null;

            if (attackMode == AttackMode.Sequential)
            {
                int len = attackSequence.Length;
                for (int i = 0; i < len; i++)
                {
                    int idx = (sequenceIndex + i) % len;
                    EnemyAttack atk = attackSequence[idx];
                    if (atk != null && candidates.Contains(atk))
                    {
                        sequenceIndex = idx + 1;
                        return atk;
                    }
                }
                EnemyAttack fallback = candidates[0];
                sequenceIndex++;
                return fallback;
            }

            float total = 0f;
            foreach (var a in candidates)
                total += Mathf.Max(a.weight, 0f);
            if (total <= 0f) return candidates[0];

            float roll = Random.Range(0f, total);
            float cumulative = 0f;
            foreach (var a in candidates)
            {
                cumulative += Mathf.Max(a.weight, 0f);
                if (roll <= cumulative) return a;
            }
            return candidates[candidates.Count - 1];
        }

        private EnemyAttack PickByPriority(
            EnemyAttack[] attackSequence, 
            List<EnemyAttack> candidates, 
            AttackMode attackMode,
            AttackPriorityProfile attackPriorityProfile,
            float distToPlayer, 
            ref int sequenceIndex,
            float rangeWeight,
            float damageWeight,
            float knockbackWeight,
            float assetPriorityWeight,
            System.Func<EnemyAttack, float> getAttackEffectiveRange)
        {
            if (candidates == null || candidates.Count == 0) return null;

            if (attackMode == AttackMode.Random)
            {
                float total = 0f;
                foreach (var a in candidates)
                    total += Mathf.Max(0.01f, EvaluateAttackScore(a, distToPlayer, attackPriorityProfile, rangeWeight, damageWeight, knockbackWeight, assetPriorityWeight, getAttackEffectiveRange)) * Mathf.Max(a.weight, 0f);

                if (total <= 0f) return candidates[0];

                float roll = Random.Range(0f, total);
                float cumulative = 0f;
                foreach (var a in candidates)
                {
                    cumulative += Mathf.Max(0.01f, EvaluateAttackScore(a, distToPlayer, attackPriorityProfile, rangeWeight, damageWeight, knockbackWeight, assetPriorityWeight, getAttackEffectiveRange)) * Mathf.Max(a.weight, 0f);
                    if (roll <= cumulative) return a;
                }
                return candidates[candidates.Count - 1];
            }

            EnemyAttack best = null;
            float bestScore = float.MinValue;
            int bestIndex = int.MaxValue;

            for (int i = 0; i < attackSequence.Length; i++)
            {
                EnemyAttack atk = attackSequence[i];
                if (atk == null || !candidates.Contains(atk)) continue;

                float score = EvaluateAttackScore(atk, distToPlayer, attackPriorityProfile, rangeWeight, damageWeight, knockbackWeight, assetPriorityWeight, getAttackEffectiveRange);
                if (score > bestScore)
                {
                    best = atk;
                    bestScore = score;
                    bestIndex = i;
                }
                else if (Mathf.Abs(score - bestScore) < 0.0001f && i < bestIndex)
                {
                    best = atk;
                    bestIndex = i;
                }
            }

            if (best != null)
            {
                sequenceIndex = bestIndex + 1;
                return best;
            }

            return candidates[0];
        }

        private float EvaluateAttackScore(
            EnemyAttack atk, 
            float distToPlayer, 
            AttackPriorityProfile attackPriorityProfile,
            float rangeWeight,
            float damageWeight,
            float knockbackWeight,
            float assetPriorityWeight,
            System.Func<EnemyAttack, float> getAttackEffectiveRange)
        {
            float range = getAttackEffectiveRange(atk);
            float damage = Mathf.Max(0f, atk.damage);
            float knockback = Mathf.Max(0f, atk.knockbackForce);
            float priority = Mathf.Max(0f, atk.aiPriority);

            switch (attackPriorityProfile)
            {
                case AttackPriorityProfile.LongestRange:
                    return range;
                case AttackPriorityProfile.HighestDamage:
                    return damage;
                case AttackPriorityProfile.HighestKnockback:
                    return knockback;
                case AttackPriorityProfile.HighestAssetPriority:
                    return priority;
                default:
                    float distanceFit = range > 0f ? Mathf.Clamp01(1f - Mathf.Abs(distToPlayer - range) / range) : 0f;
                    return (range * rangeWeight)
                    + (damage * damageWeight)
                    + (knockback * knockbackWeight)
                    + (priority * assetPriorityWeight)
                    + distanceFit;
            }
        }

        public void TriggerAttackAnimation(EnemyAttack attack, ActorAnimationDriver animationDriver, Animator animator, Dictionary<EnemyAttack, int> attackTriggerHashes)
        {
            if (attack == null)
                return;

            if (animationDriver != null)
            {
                int step = Mathf.Max(attack.animationStep, 1);
                if (attack.useCustomAnimationKey && !string.IsNullOrWhiteSpace(attack.customAnimationKey))
                    animationDriver.TriggerCustom(attack.customAnimationKey, intValue: step);
                else
                {
                    animationDriver.SetIntSignal(attack.animationSignal, step);
                    animationDriver.TriggerSignal(attack.animationSignal, intValue: step);
                }
            }

            if (!string.IsNullOrEmpty(attack.animatorTrigger)
            && attackTriggerHashes.TryGetValue(attack, out int hash))
                animator?.SetTrigger(hash);
        }

        public IEnumerator EnemyHitBoxRoutine(HitBox box, float damage, float knockback, float delay, float duration, float attackRadius = 0f)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);

            Collider col = box.GetComponent<Collider>();
            Vector3 originalScale = col != null ? col.transform.localScale : Vector3.one;

            if (attackRadius > 0f && col != null)
            {
                float radiusScale = 1f + (attackRadius * 0.5f);
                col.transform.localScale = originalScale * radiusScale;
            }

            box.Fire(damage, knockback);
            yield return new WaitForSeconds(Mathf.Max(duration, 0.01f));

            if (col != null)
                col.transform.localScale = originalScale;
        }
    }
}
