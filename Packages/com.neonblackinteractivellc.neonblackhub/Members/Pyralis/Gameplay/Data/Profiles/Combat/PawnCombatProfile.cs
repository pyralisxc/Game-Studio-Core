using NeonBlack.Gameplay.Features.Combat;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Shared combat authoring profile for pawn composition.
    /// </summary>
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Pawn Combat Profile", fileName = "PawnCombatProfile", order = -20)]
    public class PawnCombatProfile : ScriptableObject
    {
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
