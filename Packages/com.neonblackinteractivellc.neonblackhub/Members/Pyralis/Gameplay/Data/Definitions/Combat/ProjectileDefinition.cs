using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Actions;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    [CreateAssetMenu(menuName = "NeonBlack/Combat/Projectile Definition", fileName = "ProjectileDefinition")]
    public class ProjectileDefinition : ScriptableObject
    {
        public string projectileId = "projectile.new";
        public string displayName = "Projectile";
        public ProjectileDeliveryMode deliveryMode = ProjectileDeliveryMode.ProjectilePrefab;
        public GameObject projectilePrefab;
        public float damage = 10f;
        public float knockback = 0f;
        public float speed = 20f;
        public float maxDistance = 30f;
        public float lifetime = 5f;
        public bool allowFriendlyFire = false;
        public NeonBlack.Gameplay.Data.Definitions.ActionDefinition actionDefinition;
        public ProjectileImpactDefinition impactDefinition;

        [TextArea(2, 5)]
        public string notes = string.Empty;

        public void Sanitize()
        {
            projectileId = !string.IsNullOrWhiteSpace(projectileId) ? projectileId.Trim() : name;
            displayName = !string.IsNullOrWhiteSpace(displayName) ? displayName.Trim() : projectileId;
            damage = Mathf.Max(0f, damage);
            knockback = Mathf.Max(0f, knockback);
            speed = Mathf.Max(0f, speed);
            maxDistance = Mathf.Max(0f, maxDistance);
            lifetime = Mathf.Max(0.01f, lifetime);
        }

        public List<string> GetValidationIssues()
        {
            var issues = new List<string>();

            if (string.IsNullOrWhiteSpace(projectileId))
                issues.Add("Projectile id is required.");

            if (string.IsNullOrWhiteSpace(displayName))
                issues.Add("Display name is required.");

            if (deliveryMode == ProjectileDeliveryMode.ProjectilePrefab && projectilePrefab == null)
                issues.Add("Projectile prefab delivery requires a projectile prefab.");

            if (deliveryMode == ProjectileDeliveryMode.Hitscan && maxDistance <= 0f)
                issues.Add("Hitscan delivery requires a max distance greater than zero.");

            if (speed <= 0f && deliveryMode == ProjectileDeliveryMode.ProjectilePrefab)
                issues.Add("Projectile prefab delivery requires speed greater than zero.");

            return issues;
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
