using UnityEngine;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Combat;

namespace NeonBlack.Gameplay.Features.Characters
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat | AuthoringCapability.CombatState,
        Relevance = "Pawn module for managing outgoing damage and knockback multipliers.",
        ExpertAdvice = "This module coordinates with the PawnDamageHandler to apply status-effect-based multipliers to all outgoing attacks.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/combat"
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