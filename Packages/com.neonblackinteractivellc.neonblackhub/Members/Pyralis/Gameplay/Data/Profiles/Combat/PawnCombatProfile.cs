using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions.Combat;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Shared combat authoring profile for pawn composition.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Combat,
        Priority = AuthoringPriority.AuxiliaryDefault,
        Lane = "Combat",
        Relevance = "Defines the core combat parameters for a pawn archetype.",
        NativeSetup = new[] { "Set base damage and cooldowns.", "Configure block reduction." },
        AssignmentFields = new[] { nameof(baseDamage), nameof(attackCooldown), nameof(attackWeapon), nameof(primarySequence) },
        Proof = "Verify the pawn can attack and take damage in-game.",
        ExpertAdvice = "Use comboResetTime to control the window for continuing a combo. Assign a WeaponData asset to define the hitboxes and visual effects of the attack.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/combat",
        CapabilityPath = "Combat/Actions/Pawn Combat Profile",
        RuntimeFamilies = new[] { RuntimeCapabilityFamily.Combat }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Pawn Combat Profile", fileName = "PawnCombatProfile", order = -20)]
    public class PawnCombatProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (baseDamage < 0f)
                yield return PyralisRuntimeValidationIssue.Required("Base Damage cannot be negative.", nameof(baseDamage), nameof(PawnCombatProfile), issueCode: "PawnCombatProfile.BaseDamage.Invalid");
            if (attackCooldown <= 0f)
                yield return PyralisRuntimeValidationIssue.Required("Attack Cooldown must be greater than zero.", nameof(attackCooldown), nameof(PawnCombatProfile), issueCode: "PawnCombatProfile.AttackCooldown.Invalid");

            if (!enableCombat)
                yield break;

            foreach (PyralisRuntimeValidationIssue issue in GetWeaponIssues(attackWeapon, nameof(attackWeapon), "Attack Weapon"))
                yield return issue;
            foreach (PyralisRuntimeValidationIssue issue in GetWeaponIssues(kickWeapon, nameof(kickWeapon), "Kick Weapon"))
                yield return issue;
            foreach (PyralisRuntimeValidationIssue issue in GetWeaponIssues(aerialWeapon, nameof(aerialWeapon), "Aerial Weapon"))
                yield return issue;

            foreach (PyralisRuntimeValidationIssue issue in GetSequenceIssues(primarySequence, nameof(primarySequence), "Primary Sequence"))
                yield return issue;
            foreach (PyralisRuntimeValidationIssue issue in GetSequenceIssues(secondarySequence, nameof(secondarySequence), "Secondary Sequence"))
                yield return issue;
            foreach (PyralisRuntimeValidationIssue issue in GetSequenceIssues(aerialSequence, nameof(aerialSequence), "Aerial Sequence"))
                yield return issue;
        }

        public bool enableCombat = true;
        public float baseDamage = 10f;
        public float baseKnockback = 5f;
        public float attackCooldown = 0.5f;
        public float kickCooldown = 0.8f;
        public float blockDamageReduction = 0.2f;
        public int maxAerialAttacks = 2;
        public float comboResetTime = 1.5f;
        public float combatWindow = 3f;
        public WeaponData attackWeapon;
        public WeaponData kickWeapon;
        public WeaponData aerialWeapon;
        public CombatSequenceDefinition primarySequence;
        public CombatSequenceDefinition secondarySequence;
        public CombatSequenceDefinition aerialSequence;

        public void Sanitize()
        {
            baseDamage = Mathf.Max(0f, baseDamage);
            baseKnockback = Mathf.Max(0f, baseKnockback);
            attackCooldown = Mathf.Max(0f, attackCooldown);
            kickCooldown = Mathf.Max(0f, kickCooldown);
            comboResetTime = Mathf.Max(0f, comboResetTime);
            combatWindow = Mathf.Max(0f, combatWindow);
            blockDamageReduction = Mathf.Clamp01(blockDamageReduction);
            maxAerialAttacks = Mathf.Max(0, maxAerialAttacks);
        }

        private void OnValidate()
        {
            Sanitize();
        }

        private static IEnumerable<PyralisRuntimeValidationIssue> GetWeaponIssues(
            WeaponData weapon,
            string fieldPath,
            string label)
        {
            if (weapon == null)
                yield break;

            foreach (PyralisRuntimeValidationIssue issue in weapon.GetRuntimeValidationIssues())
            {
                if (issue != null && !string.IsNullOrWhiteSpace(issue.Message))
                {
                    yield return new PyralisRuntimeValidationIssue(
                        $"{label}: {issue.Message}",
                        fieldPath,
                        nameof(PawnCombatProfile),
                        "Open the assigned WeaponData and resolve the named issue.",
                        "Assigned WeaponData reports no validation issues.",
                        issue.Severity,
                        "PawnCombatProfile.Weapon." + issue.IssueCode);
                }
            }
        }

        private static IEnumerable<PyralisRuntimeValidationIssue> GetSequenceIssues(
            CombatSequenceDefinition sequence,
            string fieldPath,
            string label)
        {
            if (sequence == null)
                yield break;

            foreach (PyralisRuntimeValidationIssue issue in sequence.GetRuntimeValidationIssues())
            {
                if (issue != null && !string.IsNullOrWhiteSpace(issue.Message))
                {
                    yield return new PyralisRuntimeValidationIssue(
                        $"{label}: {issue.Message}",
                        fieldPath,
                        nameof(PawnCombatProfile),
                        "Open the assigned CombatSequenceDefinition and resolve the named issue.",
                        "Assigned CombatSequenceDefinition reports no validation issues.",
                        issue.Severity,
                        "PawnCombatProfile.Sequence." + issue.IssueCode);
                }
            }
        }
    }
}
