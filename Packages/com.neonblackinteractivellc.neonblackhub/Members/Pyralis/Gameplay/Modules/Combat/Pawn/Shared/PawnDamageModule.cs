using UnityEngine;
using NeonBlack.Gameplay.Core.Contracts;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Combat
{
    [AuthoringContract(
        Category = "Combat, Combat State",
        CapabilityPath = "Combat/Actions/Pawn Damage Module",
        Surface = AuthoringSurface.Goal,
        Summary = "Pawn module for managing outgoing damage and knockback multipliers.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/combat",
        Tags = new[] { "capability:Combat", "capability:CombatState" }
    )]
    public class PawnDamageModule : MonoBehaviour, IDamageModifier, IActorCombatModifierReceiver
{
        private PawnDamageHandler _damageHandler;

        public PawnDamageHandler DamageHandler => _damageHandler;

        private void Awake()
        {
            _damageHandler = new PawnDamageHandler();
        }

        public bool TryModifyIncomingDamage(GameObject source, ref float incomingDamage)
        {
            // Note: block logic should probably be moved here or to a PawnBlockModule
            // For now, keeping consistency with existing PawnDamageHandler
            return false; 
        }

        // Overload for block-aware damage modification
        public bool TryModifyIncomingDamage(
            GameObject source, 
            ref float incomingDamage, 
            bool isBlocking, 
            float blockReduction, 
            float blockAngle, 
            bool facingRight)
        {
            return _damageHandler.TryModifyIncomingDamage(
                gameObject, 
                source, 
                ref incomingDamage, 
                isBlocking, 
                blockReduction, 
                blockAngle, 
                facingRight);
        }

        public float GetModifiedDamage(float baseDamage) => _damageHandler.GetModifiedDamage(baseDamage);
        public float GetModifiedKnockback(float baseKnockback) => _damageHandler.GetModifiedKnockback(baseKnockback);

        public void SetOutgoingDamageMultiplier(float multiplier) => _damageHandler.SetOutgoingDamageMultiplier(multiplier);
        public void SetOutgoingKnockbackMultiplier(float multiplier) => _damageHandler.SetOutgoingKnockbackMultiplier(multiplier);
    }
}
