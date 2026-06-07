using System.Collections.Generic;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    [CreateAssetMenu(menuName = "NeonBlack/Gameplay/Combat/Projectile Impact Definition", fileName = "ProjectileImpactDefinition")]
    public class ProjectileImpactDefinition : ScriptableObject
    {
        public string impactId = "impact.projectile";
        public string displayName = "Projectile Impact";
        public GameObject hitEffectPrefab;
        public GameObject missEffectPrefab;
        public AudioClip hitSound;
        public AudioClip missSound;
        public float effectLifetime = 2f;
        public bool applyHitPause = false;
        public float hitPauseDuration = 0.05f;
        public bool applyCameraShake = false;
        public float cameraShakeIntensity = 0.1f;
        public float cameraShakeDuration = 0.1f;
        public bool spawnMissEffectAtMaxDistance = true;

        [TextArea(2, 5)]
        public string notes = string.Empty;

        public void Sanitize()
        {
            impactId = !string.IsNullOrWhiteSpace(impactId) ? impactId.Trim() : name;
            displayName = !string.IsNullOrWhiteSpace(displayName) ? displayName.Trim() : impactId;
            effectLifetime = Mathf.Max(0f, effectLifetime);
            hitPauseDuration = Mathf.Max(0f, hitPauseDuration);
            cameraShakeIntensity = Mathf.Max(0f, cameraShakeIntensity);
            cameraShakeDuration = Mathf.Max(0f, cameraShakeDuration);
        }

        public List<string> GetValidationIssues()
        {
            var issues = new List<string>();

            if (string.IsNullOrWhiteSpace(impactId))
                issues.Add("Impact id is required.");

            if (string.IsNullOrWhiteSpace(displayName))
                issues.Add("Display name is required.");

            if (applyHitPause && hitPauseDuration <= 0f)
                issues.Add("Hit pause requires a duration greater than zero.");

            if (applyCameraShake && (cameraShakeIntensity <= 0f || cameraShakeDuration <= 0f))
                issues.Add("Camera shake requires intensity and duration greater than zero.");

            return issues;
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
