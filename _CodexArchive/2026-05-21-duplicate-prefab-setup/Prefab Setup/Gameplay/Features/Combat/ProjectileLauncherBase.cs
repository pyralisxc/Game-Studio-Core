using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    public abstract class ProjectileLauncherBase : MonoBehaviour
    {
        [Header("Prefab Delivery")]
        [SerializeField] private bool usePrefabPooling;
        [SerializeField] private Transform projectileParent;
        [SerializeField] private int maxPoolSizePerPrefab = 64;

        private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new Dictionary<GameObject, Queue<GameObject>>();

        public ProjectileSpawnResult[] Fire(ProjectileFireRequest request)
        {
            ProjectileSpawnCommand[] commands = ProjectileFirePlanner.BuildCommands(request);
            var results = new ProjectileSpawnResult[commands.Length];

            for (int i = 0; i < commands.Length; i++)
                results[i] = Execute(commands[i]);

            return results;
        }

        public ProjectileSpawnResult Execute(ProjectileSpawnCommand command)
        {
            if (command.DeliveryMode == ProjectileDeliveryMode.ProjectilePrefab && command.ProjectilePrefab == null)
                return ProjectileSpawnResult.Failed("Projectile prefab delivery requires a prefab.");

            if (command.Delay > 0f)
            {
                StartCoroutine(ExecuteAfterDelay(command));
                return ProjectileSpawnResult.Pending("Projectile command scheduled.");
            }

            return ExecuteImmediate(command);
        }

        internal void ReturnToPool(GameObject prefab, GameObject instance)
        {
            if (!usePrefabPooling || prefab == null || instance == null)
            {
                DestroyInstance(instance);
                return;
            }

            Queue<GameObject> pool = GetPool(prefab);
            if (pool.Count >= Mathf.Max(1, maxPoolSizePerPrefab))
            {
                DestroyInstance(instance);
                return;
            }

            instance.SetActive(false);
            instance.transform.SetParent(projectileParent != null ? projectileParent : transform, false);
            pool.Enqueue(instance);
        }

        protected abstract ProjectileSpawnResult ExecuteImmediate(ProjectileSpawnCommand command);

        protected ProjectileSpawnResult ApplyImpactEffects(ProjectileSpawnCommand command, ProjectileSpawnResult result)
        {
            return ProjectileImpactEffectPlayer.Apply(command.ImpactDefinition, result, command);
        }

        protected GameObject SpawnPrefabInstance(ProjectileSpawnCommand command, Quaternion rotation)
        {
            GameObject prefab = command.ProjectilePrefab;
            if (prefab == null)
                return null;

            GameObject instance = null;
            if (usePrefabPooling)
            {
                Queue<GameObject> pool = GetPool(prefab);
                while (pool.Count > 0 && instance == null)
                    instance = pool.Dequeue();
            }

            if (instance == null)
                instance = Instantiate(prefab);

            instance.transform.SetParent(projectileParent, true);
            instance.transform.SetPositionAndRotation(command.Origin, rotation);
            instance.SetActive(true);

            if (usePrefabPooling)
            {
                ProjectilePoolHandle handle = instance.GetComponent<ProjectilePoolHandle>();
                if (handle == null)
                    handle = instance.AddComponent<ProjectilePoolHandle>();

                handle.Configure(this, prefab);
                handle.ScheduleReturn(command.Lifetime);
            }
            else if (command.Lifetime > 0f && instance.GetComponent<Projectile>() == null)
            {
                Destroy(instance, command.Lifetime);
            }

            return instance;
        }

        protected static bool TryApplyDamage(HealthComponent health, ProjectileSpawnCommand command, Vector3 hitPoint)
        {
            if (health == null || command.Damage <= 0f)
                return false;

            if (command.Owner != null)
            {
                if (health.gameObject == command.Owner || health.transform.IsChildOf(command.Owner.transform))
                    return false;
            }

            if (!command.AllowFriendlyFire
                && command.SourceFaction != Core.Contracts.Faction.Neutral
                && health.Faction == command.SourceFaction)
            {
                return false;
            }

            health.TakeDamage(command.Damage, hitPoint, command.Owner);
            return true;
        }

        protected static Vector3 NormalizedDirection(Vector3 direction)
        {
            return direction.sqrMagnitude > 0f ? direction.normalized : Vector3.forward;
        }

        private IEnumerator ExecuteAfterDelay(ProjectileSpawnCommand command)
        {
            yield return new WaitForSeconds(command.Delay);
            ExecuteImmediate(command);
        }

        private Queue<GameObject> GetPool(GameObject prefab)
        {
            if (!_pools.TryGetValue(prefab, out Queue<GameObject> pool))
            {
                pool = new Queue<GameObject>();
                _pools[prefab] = pool;
            }

            return pool;
        }

        private static void DestroyInstance(GameObject instance)
        {
            if (instance == null)
                return;

            if (Application.isPlaying)
                Destroy(instance);
            else
                DestroyImmediate(instance);
        }
    }
}
