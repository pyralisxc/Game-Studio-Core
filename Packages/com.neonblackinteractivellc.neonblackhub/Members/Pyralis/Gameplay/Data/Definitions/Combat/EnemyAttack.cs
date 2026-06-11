using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat,
        Relevance = "Defines the selection criteria and execution of a specific AI attack.",
        NativeSetup = new[] { "Create Asset.", "Assign animation signal.", "Set Range and Priority." },
        AssignmentFields = new[] { nameof(animationSignal), nameof(hitBoxZone), nameof(attackRange), nameof(aiPriority) },
        FirstProof = "Verify the enemy triggers this attack when within the specified range.",
        ExpertAdvice = "Set Priority higher for 'punish' or 'finisher' moves. Use weight for random selection within the same priority."
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Combat/Enemy Attack", fileName = "NewEnemyAttack")]
    public class EnemyAttack : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (attackRange < 0f) yield return "Attack Range cannot be negative.";
            if (damage < 0f) yield return "Damage cannot be negative.";
        }

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
