using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Composition;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat,
        Relevance = "3D projectile launcher; supports physics-based projectiles and raycast hitscan.",
        Axioms = AuthoringWorldAxiom.Realtime | AuthoringWorldAxiom.Dimensions3D,
        NativeSetup = new[] { "Add to a 3D scene.", "Configure Hit Mask for world geometry." },
        AssignmentFields = new[] { nameof(hitMask) },
        FirstProof = "Fire a hitscan attack and verify it registers on a 3D HealthComponent.",
        ExpertAdvice = "Set Hit Mask to exclude the shooter's layer. Ensure projectile prefabs have a Rigidbody or IProjectileRuntimeBody for movement."
    )]
    public sealed class ProjectileLauncher3D : ProjectileLauncherBase, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (hitMask == 0) yield return "Hit Mask is empty. No collisions will be detected.";
        }
        [Header("Hitscan")]
        [SerializeField] private LayerMask hitMask = Physics.DefaultRaycastLayers;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;

        protected override ProjectileSpawnResult ExecuteImmediate(ProjectileSpawnCommand command)
        {
            ProjectileSpawnResult result = command.DeliveryMode == ProjectileDeliveryMode.Hitscan
                ? ExecuteHitscan(command)
                : ExecuteProjectilePrefab(command);
            return ApplyImpactEffects(command, result);
        }

        private ProjectileSpawnResult ExecuteProjectilePrefab(ProjectileSpawnCommand command)
        {
            Vector3 direction = NormalizedDirection(command.Direction);
            GameObject instance = SpawnPrefabInstance(command, Quaternion.LookRotation(direction, Vector3.up));
            if (instance == null)
                return ProjectileSpawnResult.Failed("Projectile prefab command had no prefab.");

            if (!instance.TryGetComponent(out IProjectileRuntimeBody _)
                && instance.TryGetComponent(out Rigidbody body))
            {
                body.linearVelocity = direction * command.Speed;
            }

            return ProjectileSpawnResult.Spawned(instance);
        }

        private ProjectileSpawnResult ExecuteHitscan(ProjectileSpawnCommand command)
        {
            Vector3 direction = NormalizedDirection(command.Direction);
            float distance = command.MaxDistance > 0f ? command.MaxDistance : 1000f;

            if (!Physics.Raycast(command.Origin, direction, out RaycastHit hit, distance, hitMask, triggerInteraction))
                return ProjectileSpawnResult.Missed();

            HealthComponent health = hit.collider.GetComponentInParent<HealthComponent>();
            bool damaged = TryApplyDamage(health, command, hit.point);
            if (health != null && !damaged)
                return ProjectileSpawnResult.Ignored("Projectile hit was rejected by target health.");

            if (damaged && command.Knockback > 0f && health != null)
            {
                KnockbackReceiver knockbackReceiver = health.GetComponent<KnockbackReceiver>();
                if (knockbackReceiver != null)
                {
                    Vector3 knockbackDirection = health.transform.position - command.Origin;
                    knockbackDirection.y = 0f;
                    if (knockbackDirection.sqrMagnitude < 0.001f)
                        knockbackDirection = direction;

                    knockbackDirection = knockbackDirection.normalized;
                    knockbackDirection.y = 0.25f;
                    knockbackReceiver.ApplyKnockback(knockbackDirection * command.Knockback);
                }
            }

            GameObject hitObject = health != null ? health.gameObject : hit.collider.gameObject;
            return ProjectileSpawnResult.Hit(hitObject, hit.point);
        }
    }
}
