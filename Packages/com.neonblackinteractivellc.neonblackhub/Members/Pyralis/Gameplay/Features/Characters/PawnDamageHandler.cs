using UnityEngine;
using NeonBlack.Gameplay.Core.Enums;
using NeonBlack.Gameplay.Features.Combat;

namespace NeonBlack.Gameplay.Features.Characters
{
    public class PawnDamageHandler
    {
        private float _outgoingDamageMultiplier = 1f;
        private float _outgoingKnockbackMultiplier = 1f;

        public float OutgoingDamageMultiplier => _outgoingDamageMultiplier;
        public float OutgoingKnockbackMultiplier => _outgoingKnockbackMultiplier;

        public void SetOutgoingDamageMultiplier(float multiplier)
        {
            _outgoingDamageMultiplier = Mathf.Max(multiplier, 0f);
        }

        public void SetOutgoingKnockbackMultiplier(float multiplier)
        {
            _outgoingKnockbackMultiplier = Mathf.Max(multiplier, 0f);
        }

        public bool TryModifyIncomingDamage(
            GameObject owner,
            GameObject source, 
            ref float incomingDamage, 
            bool isBlocking, 
            float blockDamageReduction, 
            float blockFrontalAngle, 
            bool facingRight)
        {
            if (owner.GetComponentInChildren<IActorGuardFeature>(true) != null)
                return false;

            if (!isBlocking || source == null)
                return false;

            Vector3 toAttacker = source.transform.position - owner.transform.position;
            toAttacker.y = 0f;
            if (toAttacker.sqrMagnitude <= 0.001f)
                return false;

            Vector3 facingDir = facingRight ? Vector3.right : Vector3.left;
            float dot = Vector3.Dot(facingDir, toAttacker.normalized);
            float threshold = Mathf.Cos(blockFrontalAngle * Mathf.Deg2Rad);
            if (dot < threshold)
                return false;

            incomingDamage *= blockDamageReduction;
            return true;
        }

        public float GetModifiedDamage(float baseDamage) => baseDamage * _outgoingDamageMultiplier;
        public float GetModifiedKnockback(float baseKnockback) => baseKnockback * _outgoingKnockbackMultiplier;
    }
}
