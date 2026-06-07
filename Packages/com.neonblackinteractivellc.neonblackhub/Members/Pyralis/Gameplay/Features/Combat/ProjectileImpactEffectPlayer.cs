using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    public static class ProjectileImpactEffectPlayer
    {
        public static ProjectileSpawnResult Apply(
            ProjectileImpactDefinition definition,
            ProjectileSpawnResult result,
            ProjectileSpawnCommand command,
            IHitPauseSink hitPauseSink = null,
            ICameraShakeSink cameraShakeSink = null)
        {
            if (definition == null)
                return result;

            GameObject effectObject = null;
            if (result.Status == ProjectileSpawnStatus.Hit)
            {
                effectObject = SpawnEffect(definition.hitEffectPrefab, result.HitPoint, command.Direction, definition.effectLifetime);
                PlaySound(definition.hitSound, result.HitPoint);
                ApplyHitPause(definition, hitPauseSink);
                ApplyCameraShake(definition, cameraShakeSink);
            }
            else if (result.Status == ProjectileSpawnStatus.Missed)
            {
                Vector3 missPoint = GetMissPoint(command, definition);
                effectObject = SpawnEffect(definition.missEffectPrefab, missPoint, command.Direction, definition.effectLifetime);
                PlaySound(definition.missSound, missPoint);
            }

            return effectObject != null ? result.WithImpactEffect(effectObject) : result;
        }

        private static GameObject SpawnEffect(GameObject prefab, Vector3 position, Vector3 direction, float lifetime)
        {
            if (prefab == null)
                return null;

            Quaternion rotation = direction.sqrMagnitude > 0f
                ? Quaternion.LookRotation(direction.normalized, Vector3.up)
                : Quaternion.identity;

            GameObject instance = Object.Instantiate(prefab, position, rotation);
            if (lifetime > 0f)
                Object.Destroy(instance, lifetime);

            return instance;
        }

        private static void PlaySound(AudioClip clip, Vector3 position)
        {
            if (clip != null)
                AudioSource.PlayClipAtPoint(clip, position);
        }

        private static void ApplyHitPause(ProjectileImpactDefinition definition, IHitPauseSink hitPauseSink)
        {
            if (definition.applyHitPause && definition.hitPauseDuration > 0f)
                hitPauseSink?.Freeze(definition.hitPauseDuration);
        }

        private static void ApplyCameraShake(ProjectileImpactDefinition definition, ICameraShakeSink cameraShakeSink)
        {
            if (definition.applyCameraShake
                && definition.cameraShakeIntensity > 0f
                && definition.cameraShakeDuration > 0f)
            {
                cameraShakeSink?.Shake(definition.cameraShakeIntensity, definition.cameraShakeDuration);
            }
        }

        private static Vector3 GetMissPoint(ProjectileSpawnCommand command, ProjectileImpactDefinition definition)
        {
            if (!definition.spawnMissEffectAtMaxDistance)
                return command.Origin;

            Vector3 direction = command.Direction.sqrMagnitude > 0f ? command.Direction.normalized : Vector3.forward;
            float distance = command.MaxDistance > 0f ? command.MaxDistance : 1f;
            return command.Origin + direction * distance;
        }
    }
}
