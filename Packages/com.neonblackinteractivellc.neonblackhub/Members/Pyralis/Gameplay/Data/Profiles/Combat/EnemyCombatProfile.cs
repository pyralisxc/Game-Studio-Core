using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions.Combat;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Shared combat authoring profile for enemy attack selection and timing.
    /// </summary>
    [AuthoringContract(
        Category = "Combat",
        CapabilityPath = "Combat/Actions/Enemy Combat Profile",
        Surface = AuthoringSurface.Profile,
        Summary = "Defines how an AI enemy chooses and sequences its attacks.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/enemies",
        RequiredFields = new[] { nameof(attackSequence) },
        SetupSteps = new[] { "Add EnemyAttacks to the attackSequence array.", "Set Attack Mode." },
        SuccessChecks = new[] { "Verify the enemy cycles through the defined attacks during combat." },
        Tags = new[] { "capability:Combat", "runtime:Combat" },
        Selectable = false
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Enemy Combat Profile", fileName = "EnemyCombatProfile")]
    public class EnemyCombatProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (attackSequence == null || attackSequence.Length == 0)
                yield return RuntimeValidationIssue.Required("Attack Sequence is empty. Enemy will not be able to attack.");
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
