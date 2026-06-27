using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions.Combat;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Shared combat authoring profile for enemy attack selection and timing.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Combat,
        Relevance = "Defines how an AI enemy chooses and sequences its attacks.",
        NativeSetup = new[] { "Add EnemyAttacks to the attackSequence array.", "Set Attack Mode." },
        AssignmentFields = new[] { nameof(attackSequence), nameof(attackMode) },
        Proof = "Verify the enemy cycles through the defined attacks during combat.",
        ExpertAdvice = "Use Sequential mode for boss phases or predictable combos. Use Priority or Weighted for dynamic combat behavior.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/enemies",
        CapabilityPath = "Combat/Actions/Enemy Combat Profile",
        RuntimeFamilies = new[] { RuntimeCapabilityFamily.Combat }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Enemy Combat Profile", fileName = "EnemyCombatProfile")]
    public class EnemyCombatProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (attackSequence == null || attackSequence.Length == 0)
                yield return PyralisRuntimeValidationIssue.Required("Attack Sequence is empty. Enemy will not be able to attack.");
        }

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
