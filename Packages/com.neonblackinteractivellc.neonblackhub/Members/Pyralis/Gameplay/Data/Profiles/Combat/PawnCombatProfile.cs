using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Combat;
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
        NativeSetup = new[] { "Create Asset.", "Set base damage and cooldowns.", "Configure block reduction." },
        AssignmentFields = new[] { nameof(baseDamage), nameof(attackCooldown), nameof(attackWeapon), nameof(primarySequence) },
        FirstProof = "Verify the pawn can attack and take damage in-game.",
        ExpertAdvice = "Use comboResetTime to control the window for continuing a combo. Assign a WeaponData asset to define the hitboxes and visual effects of the attack.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/combat"
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Pawn Combat Profile", fileName = "PawnCombatProfile", order = -20)]
    public class PawnCombatProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (baseDamage < 0f) yield return "Base Damage cannot be negative.";
            if (attackCooldown <= 0f) yield return "Attack Cooldown must be greater than zero.";
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
    }
}
