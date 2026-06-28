using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Combat
{
    public partial class EnemyCombatModule
    {
        public void SetOutgoingDamageMultiplier(float multiplier) => _outgoingDamageMultiplier = Mathf.Max(multiplier, 0f);

        public void SetOutgoingKnockbackMultiplier(float multiplier) => _outgoingKnockbackMultiplier = Mathf.Max(multiplier, 0f);

        public void DisableAllHitBoxes()
        {
            StopAllCoroutines();

            if (hitBoxZones == null)
                return;

            for (int i = 0; i < hitBoxZones.Length; i++)
            {
                HitBox hitBox = hitBoxZones[i].hitBox;
                if (hitBox == null)
                    continue;

                hitBox.ClearHitSet();
                if (_hitBoxOriginalScales.TryGetValue(hitBox, out Vector3 originalScale))
                    hitBox.transform.localScale = originalScale;
            }
        }
    }
}
