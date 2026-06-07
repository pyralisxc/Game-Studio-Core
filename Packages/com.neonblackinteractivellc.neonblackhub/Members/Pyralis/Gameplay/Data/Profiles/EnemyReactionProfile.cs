using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Shared reaction and presentation feedback tuning for enemies.
    /// </summary>
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Enemy Reaction Profile", fileName = "EnemyReactionProfile")]
    public class EnemyReactionProfile : ScriptableObject
    {
        public bool enableReactions = true;
        public float hurtLockDuration = 0.08f;
        public float staggerDamageThreshold = 20f;
        public float staggerLockDuration = 0.18f;
        public float hitPauseDuration = 0.03f;
        public float cameraShakeIntensity = 0.08f;
        public float cameraShakeDuration = 0.08f;
        public bool clearKnockbackOnDeath = true;

        public void Sanitize()
        {
            hurtLockDuration = Mathf.Max(0f, hurtLockDuration);
            staggerDamageThreshold = Mathf.Max(0f, staggerDamageThreshold);
            staggerLockDuration = Mathf.Max(0f, staggerLockDuration);
            hitPauseDuration = Mathf.Max(0f, hitPauseDuration);
            cameraShakeIntensity = Mathf.Max(0f, cameraShakeIntensity);
            cameraShakeDuration = Mathf.Max(0f, cameraShakeDuration);
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
