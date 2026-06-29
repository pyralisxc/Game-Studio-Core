using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions.Combat;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEngine;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Modules.Combat
{
    public partial class PawnCombatBehaviour
    {
        [Header("Combo Settings")]
        [SerializeField] private float comboResetTime = 1.5f;
        [SerializeField] private float combatWindow = 3f;
        [SerializeField] private int maxAerialAttacks = 2;
        [SerializeField] private float attackCooldown = 0.5f;
        [SerializeField] private float kickCooldown = 0.8f;

        [Header("Movement Modifiers")]
        [Range(0f, 1f)]
        [SerializeField] private float attackMoveMultiplier = 0.2f;
        [Range(0f, 1f)]
        [SerializeField] private float aerialAttackMoveMultiplier = 0.5f;

        [Header("Combat Definitions")]
        [SerializeField] private CombatSequenceDefinition primarySequence;
        [SerializeField] private CombatSequenceDefinition secondarySequence;
        [SerializeField] private CombatSequenceDefinition aerialSequence;
        [SerializeField] private string aerialHitBoxZone = "Aerial";

        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (!HasActions(primarySequence))
                yield return RuntimeValidationIssue.Required("Primary Sequence needs at least one CombatActionDefinition. PawnCombatBehaviour does not invent local primary attacks.");
            if (!HasActions(secondarySequence))
                yield return RuntimeValidationIssue.Required("Secondary Sequence needs at least one CombatActionDefinition. PawnCombatBehaviour does not invent local secondary attacks.");
            if (maxAerialAttacks > 0 && !HasActions(aerialSequence))
                yield return RuntimeValidationIssue.Required("Aerial Sequence needs at least one CombatActionDefinition when Max Aerial Attacks is greater than zero.");
            if (attackCooldown < 0f)
                yield return RuntimeValidationIssue.Required("Attack Cooldown cannot be negative.");
            if (maxAerialAttacks < 0)
                yield return RuntimeValidationIssue.Required("Max Aerial Attacks cannot be negative.");
        }

        private static bool HasActions(CombatSequenceDefinition sequence)
        {
            return sequence != null && sequence.actions != null && sequence.actions.Length > 0;
        }

        public void ApplyCombatProfile(PawnProfileApplicationContext context, PawnCombatProfile profile)
        {
            if (profile == null)
                return;

            attackCooldown = profile.attackCooldown;
            kickCooldown = profile.kickCooldown;
            comboResetTime = profile.comboResetTime;
            combatWindow = profile.combatWindow;
            primarySequence = profile.primarySequence;
            secondarySequence = profile.secondarySequence;
            aerialSequence = profile.aerialSequence;
            maxAerialAttacks = profile.maxAerialAttacks;

            WeaponModule?.SetWeapons(profile.attackWeapon, profile.kickWeapon, profile.aerialWeapon);
            DamageModule?.SetOutgoingDamageMultiplier(1.0f);
        }
    }
}
