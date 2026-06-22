using System.Collections.Generic;
using System;
using NeonBlack.Gameplay.Core.Actions;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat, 
        RuntimeFamilies = new[] { RuntimeCapabilityFamily.GunsProjectiles, RuntimeCapabilityFamily.Combat },
        CapabilityPath = "Combat/Projectiles/Projectile Definition",
        Relevance = "Project-window creation path for projectile behavior.",
        RoleTags = new[] { AuthoringContractRoleTags.IntentRouteEssential, AuthoringContractRoleTags.CombatDefinitionRouteSupport },
        AssignmentFields = new[] { nameof(projectileId), nameof(projectilePrefab), nameof(speed) },
        FirstProof = "Spawn the projectile and verify it travels at the correct speed and deals damage."
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Combat/Projectile Definition", fileName = "ProjectileDefinition")]
    public class ProjectileDefinition : ScriptableObject, IRuntimeValidationProvider
    {
        private const string ProjectileRuntimeBodyInterfaceFullName = "NeonBlack.Gameplay.Features.Combat.IProjectileRuntimeBody";

        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (string.IsNullOrWhiteSpace(projectileId))
            {
                yield return PyralisRuntimeValidationIssue.Required(
                    "Projectile id is required.",
                    nameof(projectileId),
                    nameof(ProjectileDefinition),
                    "Open the ProjectileDefinition and set a stable Projectile Id.",
                    "ProjectileDefinition.projectileId is set.",
                    "ProjectileDefinition.ProjectileId.Missing");
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                yield return PyralisRuntimeValidationIssue.Required(
                    "Display name is required.",
                    nameof(displayName),
                    nameof(ProjectileDefinition),
                    "Open the ProjectileDefinition and set a readable Display Name.",
                    "ProjectileDefinition.displayName is set.",
                    "ProjectileDefinition.DisplayName.Missing");
            }

            if (deliveryMode == ProjectileDeliveryMode.ProjectilePrefab && projectilePrefab == null)
            {
                yield return PyralisRuntimeValidationIssue.Required(
                    "Projectile prefab delivery requires a projectile prefab.",
                    nameof(projectilePrefab),
                    nameof(ProjectileDefinition),
                    "Create a 2D or 3D projectile prefab, add Projectile or Projectile2D, add matching physics, then assign it to ProjectileDefinition.projectilePrefab.",
                    "ProjectileDefinition.projectilePrefab references a projectile prefab.",
                    "ProjectileDefinition.ProjectilePrefab.Missing");
            }

            if (deliveryMode == ProjectileDeliveryMode.Hitscan && maxDistance <= 0f)
            {
                yield return PyralisRuntimeValidationIssue.Required(
                    "Hitscan delivery requires a max distance greater than zero.",
                    nameof(maxDistance),
                    nameof(ProjectileDefinition),
                    "Set ProjectileDefinition.maxDistance above zero.",
                    "ProjectileDefinition.maxDistance is greater than zero.",
                    "ProjectileDefinition.Hitscan.MaxDistanceInvalid");
            }

            if (speed <= 0f && deliveryMode == ProjectileDeliveryMode.ProjectilePrefab)
            {
                yield return PyralisRuntimeValidationIssue.Required(
                    "Projectile prefab delivery requires speed greater than zero.",
                    nameof(speed),
                    nameof(ProjectileDefinition),
                    "Set ProjectileDefinition.speed above zero.",
                    "ProjectileDefinition.speed is greater than zero.",
                    "ProjectileDefinition.ProjectilePrefab.SpeedInvalid");
            }

            if (deliveryMode == ProjectileDeliveryMode.ProjectilePrefab && projectilePrefab != null)
            {
                foreach (PyralisRuntimeValidationIssue issue in GetProjectilePrefabValidationIssues(projectilePrefab))
                    yield return issue;
            }
        }

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

            foreach (PyralisRuntimeValidationIssue issue in GetRuntimeValidationIssues())
            {
                if (issue != null && !string.IsNullOrWhiteSpace(issue.Message))
                    issues.Add(issue.Message);
            }

            return issues;
        }

        private static IEnumerable<PyralisRuntimeValidationIssue> GetProjectilePrefabValidationIssues(GameObject prefab)
        {
            if (prefab == null)
                yield break;

            bool hasMissingScript = false;
            MonoBehaviour[] behaviours = prefab.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] == null)
                {
                    hasMissingScript = true;
                    break;
                }
            }

            if (hasMissingScript)
            {
                yield return PyralisRuntimeValidationIssue.Required(
                    $"Projectile prefab `{prefab.name}` has missing script references.",
                    nameof(projectilePrefab),
                    nameof(ProjectileDefinition),
                    "Open the projectile prefab and remove or replace missing MonoBehaviour scripts.",
                    "The projectile prefab has no missing script references.",
                    "ProjectileDefinition.ProjectilePrefab.MissingScript");
            }

            if (!HasComponentImplementing(prefab, ProjectileRuntimeBodyInterfaceFullName))
            {
                yield return PyralisRuntimeValidationIssue.Required(
                    $"Projectile prefab `{prefab.name}` needs Projectile or Projectile2D so ProjectileDefinition data reaches runtime shots.",
                    nameof(projectilePrefab),
                    nameof(ProjectileDefinition),
                    "Open the projectile prefab and add Projectile or Projectile2D.",
                    "The projectile prefab has a component implementing IProjectileRuntimeBody.",
                    "ProjectileDefinition.ProjectilePrefab.RuntimeBodyMissing");
            }

            bool has3DPhysics = prefab.GetComponentInChildren<Rigidbody>(true) != null
                || prefab.GetComponentInChildren<Collider>(true) != null;
            bool has2DPhysics = prefab.GetComponentInChildren<Rigidbody2D>(true) != null
                || prefab.GetComponentInChildren<Collider2D>(true) != null;

            if (!has3DPhysics && !has2DPhysics)
            {
                yield return PyralisRuntimeValidationIssue.Required(
                    $"Projectile prefab `{prefab.name}` needs 2D or 3D physics components for movement and hit detection.",
                    nameof(projectilePrefab),
                    nameof(ProjectileDefinition),
                    "Open the projectile prefab and add route-appropriate Rigidbody/Collider components.",
                    "The projectile prefab has one physics lane.",
                    "ProjectileDefinition.ProjectilePrefab.PhysicsMissing");
            }

            if (has2DPhysics && has3DPhysics)
            {
                yield return PyralisRuntimeValidationIssue.Recommended(
                    $"Projectile prefab `{prefab.name}` mixes 2D and 3D physics. Keep one physics lane per projectile prefab.",
                    nameof(projectilePrefab),
                    nameof(ProjectileDefinition),
                    "Inspect the projectile prefab and keep one 2D or 3D physics lane.",
                    "The projectile prefab uses one physics lane.",
                    "ProjectileDefinition.ProjectilePrefab.MixedPhysics");
            }
        }

        private static bool HasComponentImplementing(GameObject prefab, string interfaceFullTypeName)
        {
            if (string.IsNullOrWhiteSpace(interfaceFullTypeName))
                return false;

            MonoBehaviour[] behaviours = prefab.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] == null)
                    continue;

                Type[] interfaces = behaviours[i].GetType().GetInterfaces();
                for (int interfaceIndex = 0; interfaceIndex < interfaces.Length; interfaceIndex++)
                {
                    if (string.Equals(interfaces[interfaceIndex].FullName, interfaceFullTypeName, StringComparison.Ordinal))
                        return true;
                }

                if (string.Equals(behaviours[i].GetType().FullName, interfaceFullTypeName, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
