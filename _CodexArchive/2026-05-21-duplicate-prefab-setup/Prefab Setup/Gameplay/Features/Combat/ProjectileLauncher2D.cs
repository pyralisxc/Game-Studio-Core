using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    public sealed class ProjectileLauncher2D : ProjectileLauncherBase
    {
        [Header("Hitscan")]
        [SerializeField] private LayerMask hitMask = Physics2D.DefaultRaycastLayers;

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
            GameObject instance = SpawnPrefabInstance(command, RotationFromDirection(direction));
            if (instance == null)
                return ProjectileSpawnResult.Failed("Projectile prefab command had no prefab.");

            if (instance.TryGetComponent(out Rigidbody2D body))
                body.linearVelocity = (Vector2)direction * command.Speed;
            else if (instance.TryGetComponent(out Rigidbody body3D))
                body3D.linearVelocity = direction * command.Speed;

            return ProjectileSpawnResult.Spawned(instance);
        }

        private ProjectileSpawnResult ExecuteHitscan(ProjectileSpawnCommand command)
        {
            Vector3 direction = NormalizedDirection(command.Direction);
            float distance = command.MaxDistance > 0f ? command.MaxDistance : 1000f;
            RaycastHit2D hit = Physics2D.Raycast(command.Origin, direction, distance, hitMask);

            if (hit.collider == null)
                return ProjectileSpawnResult.Missed();

            HealthComponent health = hit.collider.GetComponentInParent<HealthComponent>();
            bool damaged = TryApplyDamage(health, command, hit.point);

            if (damaged && command.Knockback > 0f && hit.rigidbody != null)
            {
                Vector2 knockbackDirection = ((Vector2)hit.point - (Vector2)command.Origin).normalized;
                if (knockbackDirection == Vector2.zero)
                    knockbackDirection = direction;

                hit.rigidbody.AddForce(knockbackDirection * command.Knockback, ForceMode2D.Impulse);
            }

            GameObject hitObject = health != null ? health.gameObject : hit.collider.gameObject;
            return ProjectileSpawnResult.Hit(hitObject, hit.point);
        }

        private static Quaternion RotationFromDirection(Vector3 direction)
        {
            Vector3 normalized = NormalizedDirection(direction);
            float angle = Mathf.Atan2(normalized.y, normalized.x) * Mathf.Rad2Deg;
            return Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
}
