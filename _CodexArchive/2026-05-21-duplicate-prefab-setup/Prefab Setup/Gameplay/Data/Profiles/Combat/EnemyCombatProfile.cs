using NeonBlack.Gameplay.Features.Combat;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Shared combat authoring profile for enemy attack selection and timing.
    /// </summary>
    [CreateAssetMenu(menuName = "NeonBlack/Gameplay/Profiles/Enemy Combat Profile", fileName = "EnemyCombatProfile")]
    public class EnemyCombatProfile : ScriptableObject
    {
        public EnemyAttack[] attackSequence;
        public AttackMode attackMode = AttackMode.Sequential;
        public bool usePrioritySelection = true;
        public bool preferAttacksCurrentlyInRange = true;
        public float attackCooldown = 0.5f;
        public float attackRangeOverride = 0f;
        public float rangeWeight = 1f;
        public float damageWeight = 1f;
        public float knockbackWeight = 0.75f;
        public float assetPriorityWeight = 1f;

        public void Sanitize()
        {
            attackCooldown = Mathf.Max(0f, attackCooldown);
            attackRangeOverride = Mathf.Max(0f, attackRangeOverride);
            rangeWeight = Mathf.Max(0f, rangeWeight);
            damageWeight = Mathf.Max(0f, damageWeight);
            knockbackWeight = Mathf.Max(0f, knockbackWeight);
            assetPriorityWeight = Mathf.Max(0f, assetPriorityWeight);
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
