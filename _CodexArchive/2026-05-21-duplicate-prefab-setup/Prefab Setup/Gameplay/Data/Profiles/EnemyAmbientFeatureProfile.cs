using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [CreateAssetMenu(menuName = "NeonBlack/Gameplay/Profiles/Enemy Ambient Feature Profile", fileName = "EnemyAmbientFeatureProfile")]
    public class EnemyAmbientFeatureProfile : ScriptableObject
    {
        public bool enableAmbientLookAround = true;
        public float lookAroundInterval = 3f;
        public bool requirePatrolState = true;
        public bool suppressDuringReactionLock = true;

        public void Sanitize()
        {
            lookAroundInterval = Mathf.Max(0.1f, lookAroundInterval);
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
