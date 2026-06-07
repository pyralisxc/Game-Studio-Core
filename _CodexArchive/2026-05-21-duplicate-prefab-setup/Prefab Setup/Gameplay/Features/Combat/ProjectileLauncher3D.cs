using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    public sealed class ProjectileLauncher3D : ProjectileLauncherBase
    {
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

            if (instance.TryGetComponent(out Projectile projectile))
            {
                projectile.Launch(command.Owner, command.SourceFaction, command.Damage, command.Knockback, command.Speed);
            }
            else if (instance.TryGetComponent(out Rigidbody body))
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
