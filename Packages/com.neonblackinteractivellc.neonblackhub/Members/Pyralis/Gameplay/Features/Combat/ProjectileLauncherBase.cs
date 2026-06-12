using System.Collections;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    [AuthoringContract(
        Capability = AuthoringCapability.RangedFlow,
        Relevance = "Base component for firing projectiles and hitscan attacks with built-in pooling and feedback routing.",
        ExpertAdvice = "Extend this class to create custom 2D or 3D launchers. It handles the low-level spawning and impact feedback routing via IHitPauseSink and ICameraShakeSink.",
        NativeSetup = new[] 
        { 
            "Add ProjectileLauncher2D or 3D to a scene coordinator.", 
            "Assign a Projectile Parent transform and configure pooling settings." 
        },
        FirstProof = "proof.npc-enemy-behavior",
        AssignmentFields = new[] { nameof(usePrefabPooling), nameof(maxPoolSizePerPrefab) },
        DocumentationURL = "https://docs.neonblack.com/pyralis/combat/projectiles"
    )]
    public abstract class ProjectileLauncherBase : MonoBehaviour
    {
        [Header("Prefab Delivery")]
        [SerializeField] private bool usePrefabPooling;
        [SerializeField] private Transform projectileParent;
        [SerializeField] private int maxPoolSizePerPrefab = 64;

        [Header("Impact Feedback")]
        [SerializeField] private MonoBehaviour hitPauseSink;
        [SerializeField] private MonoBehaviour cameraShakeSink;

        private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new Dictionary<GameObject, Queue<GameObject>>();
        private readonly List<Coroutine> _delayedCommandRoutines = new List<Coroutine>();
        private IHitPauseSink _hitPauseSink;
        private ICameraShakeSink _cameraShakeSink;

        protected virtual void Awake()
        {
            _hitPauseSink = ResolveHitPauseSink();
            _cameraShakeSink = ResolveCameraShakeSink();
        }

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
                Coroutine routine = null;
                routine = StartCoroutine(RunDelayedCommand(command, () =>
                {
                    if (routine != null)
                        _delayedCommandRoutines.Remove(routine);
                }));
                _delayedCommandRoutines.Add(routine);
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
            return ProjectileImpactEffectPlayer.Apply(command.ImpactDefinition, result, command, ResolveHitPauseSink(), ResolveCameraShakeSink());
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

            bool hasRuntimeBody = instance.TryGetComponent(out IProjectileRuntimeBody runtimeBody);
            if (usePrefabPooling)
            {
                ProjectilePoolHandle handle = instance.GetComponent<ProjectilePoolHandle>();
                if (handle == null)
                    handle = instance.AddComponent<ProjectilePoolHandle>();

                handle.Configure(this, prefab);
                if (!hasRuntimeBody)
                    handle.ScheduleReturn(command.Lifetime);
            }
            else if (!hasRuntimeBody && command.Lifetime > 0f)
            {
                Destroy(instance, command.Lifetime);
            }

            instance.transform.SetParent(projectileParent, true);
            instance.transform.SetPositionAndRotation(command.Origin, rotation);
            instance.SetActive(true);
            runtimeBody?.Launch(command, ResolveHitPauseSink(), ResolveCameraShakeSink());
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

            return health.TryTakeDamage(command.Damage, hitPoint, command.Owner);
        }

        protected static Vector3 NormalizedDirection(Vector3 direction)
        {
            return direction.sqrMagnitude > 0f ? direction.normalized : Vector3.forward;
        }

        public void SetImpactFeedbackSinks(IHitPauseSink hitPause, ICameraShakeSink cameraShake)
        {
            _hitPauseSink = hitPause;
            _cameraShakeSink = cameraShake;
            hitPauseSink = hitPause as MonoBehaviour;
            cameraShakeSink = cameraShake as MonoBehaviour;
        }

        private IHitPauseSink ResolveHitPauseSink()
        {
            if (_hitPauseSink != null)
                return _hitPauseSink;

            _hitPauseSink = hitPauseSink as IHitPauseSink;
            return _hitPauseSink;
        }

        private ICameraShakeSink ResolveCameraShakeSink()
        {
            if (_cameraShakeSink != null)
                return _cameraShakeSink;

            _cameraShakeSink = cameraShakeSink as ICameraShakeSink;
            return _cameraShakeSink;
        }

        protected virtual void OnDisable()
        {
            for (int i = _delayedCommandRoutines.Count - 1; i >= 0; i--)
            {
                if (_delayedCommandRoutines[i] != null)
                    StopCoroutine(_delayedCommandRoutines[i]);
            }

            _delayedCommandRoutines.Clear();
        }

        private IEnumerator RunDelayedCommand(ProjectileSpawnCommand command, System.Action onComplete)
        {
            try
            {
                yield return ExecuteAfterDelay(command);
            }
            finally
            {
                onComplete?.Invoke();
            }
        }

        private IEnumerator ExecuteAfterDelay(ProjectileSpawnCommand command)
        {
            yield return new WaitForSeconds(command.Delay);
            if (!isActiveAndEnabled)
                yield break;

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
