using NeonBlack.Gameplay.Modules.Combat;
using NeonBlack.Gameplay.Data.Definitions.Combat;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Enemies
{
    public partial class EnemyCombatModule
    {
        private float GetAttackEffectiveRange(EnemyAttack attack)
        {
            if (attack == null) return _computedAttackRange;

            HitBox zone = GetZoneHitBox(attack.hitBoxZone);
            if (zone != null && zone.TryGetEnemyAttackRangeOverride(out float hitBoxRangeOverride))
                return hitBoxRangeOverride;
            if (attack.attackRange > 0f) return attack.attackRange;
            return _computedAttackRange + Mathf.Max(0f, attack.attackRadius);
        }

        private float GetMinAttackRange()
        {
            if (attackSequence == null || attackSequence.Length == 0)
                return Mathf.Max(0.5f, _computedAttackRange);

            float minRange = float.MaxValue;
            bool found = false;
            foreach (EnemyAttack attack in attackSequence)
            {
                if (attack == null) continue;

                found = true;
                minRange = Mathf.Min(minRange, Mathf.Max(0.1f, GetAttackEffectiveRange(attack)));
            }

            return found ? minRange : Mathf.Max(0.5f, _computedAttackRange);
        }

        private HitBox GetZoneHitBox(string zoneName)
        {
            if (hitBoxZones == null || string.IsNullOrEmpty(zoneName)) return null;

            foreach (HitBoxSlot slot in hitBoxZones)
                if (slot.zoneName == zoneName)
                    return slot.hitBox;

            return null;
        }

        private float MeasureHitBoxRange(HitBox box, float absOffsetX)
        {
            if (box == null) return 1.0f;

            Collider collider = box.GetComponent<Collider>();
            if (collider == null) return 1.0f;

            float halfExtent = collider is BoxCollider boxCollider
                ? boxCollider.size.x * 0.5f * Mathf.Abs(box.transform.lossyScale.x)
                : collider.bounds.extents.x;
            return absOffsetX + halfExtent;
        }
    }
}
