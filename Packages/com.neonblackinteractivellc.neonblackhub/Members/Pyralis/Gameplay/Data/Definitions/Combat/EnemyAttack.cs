using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    [CreateAssetMenu(menuName = "NeonBlack/Combat/Enemy Attack", fileName = "NewEnemyAttack")]
    public class EnemyAttack : ScriptableObject
    {
        [Header("Animation")]
        public string animatorTrigger = "Attack";
        public ActorAnimationSignal animationSignal = ActorAnimationSignal.AttackPrimary;
        public bool useCustomAnimationKey = false;
        public string customAnimationKey = "EnemyAttack";
        public int animationStep = 1;

        [Header("Hit Zone")]
        public string hitBoxZone = "Punch";

        [Header("Damage")]
        public float damage = 10f;
        public float knockbackForce = 5f;

        [Header("Timing")]
        public float hitDelay = 0f;
        public float hitDuration = 0.15f;
        public float attackCooldown = 0f;

        [Header("Range Override")]
        public float attackRange = 0f;
        public float attackRadius = 0f;

        [Header("Weight (Random mode only)")]
        public float weight = 1f;

        [Header("AI Priority")]
        public float aiPriority = 1f;
    }

    public enum AttackMode
    {
        Sequential,
        Random
    }
}
