using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Combat;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
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

        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (attackCooldown < 0f)
                yield return "Attack Cooldown cannot be negative.";
            if (maxAerialAttacks < 0)
                yield return "Max Aerial Attacks cannot be negative.";
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
