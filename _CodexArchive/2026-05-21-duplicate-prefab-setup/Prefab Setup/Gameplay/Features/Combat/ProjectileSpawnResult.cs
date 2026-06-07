using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    public readonly struct ProjectileSpawnResult
    {
        public ProjectileSpawnStatus Status { get; }
        public GameObject SpawnedObject { get; }
        public GameObject HitObject { get; }
        public GameObject ImpactEffectObject { get; }
        public Vector3 HitPoint { get; }
        public string Message { get; }
        public bool DidHit => Status == ProjectileSpawnStatus.Hit;

        public ProjectileSpawnResult(
            ProjectileSpawnStatus status,
            GameObject spawnedObject = null,
            GameObject hitObject = null,
            GameObject impactEffectObject = null,
            Vector3 hitPoint = default,
            string message = "")
        {
            Status = status;
            SpawnedObject = spawnedObject;
            HitObject = hitObject;
            ImpactEffectObject = impactEffectObject;
            HitPoint = hitPoint;
            Message = message ?? string.Empty;
        }

        public ProjectileSpawnResult WithImpactEffect(GameObject impactEffectObject)
        {
            return new ProjectileSpawnResult(Status, SpawnedObject, HitObject, impactEffectObject, HitPoint, Message);
        }

        public static ProjectileSpawnResult Ignored(string message = "")
        {
            return new ProjectileSpawnResult(ProjectileSpawnStatus.Ignored, message: message);
        }

        public static ProjectileSpawnResult Pending(string message = "")
        {
            return new ProjectileSpawnResult(ProjectileSpawnStatus.Pending, message: message);
        }

        public static ProjectileSpawnResult Spawned(GameObject spawnedObject)
        {
            return new ProjectileSpawnResult(ProjectileSpawnStatus.Spawned, spawnedObject: spawnedObject);
        }

        public static ProjectileSpawnResult Hit(GameObject hitObject, Vector3 hitPoint)
        {
            return new ProjectileSpawnResult(ProjectileSpawnStatus.Hit, hitObject: hitObject, hitPoint: hitPoint);
        }

        public static ProjectileSpawnResult Missed()
        {
            return new ProjectileSpawnResult(ProjectileSpawnStatus.Missed);
        }

        public static ProjectileSpawnResult Failed(string message)
        {
            return new ProjectileSpawnResult(ProjectileSpawnStatus.Failed, message: message);
        }
    }
}
