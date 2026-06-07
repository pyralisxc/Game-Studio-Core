using System;
using System.Collections;
using System.Collections.Generic;
using NeonBlack.Gameplay.Features.Combat;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Spawning
{
/// <summary>
/// Enemy spawner with two modes:
///
///   Continuous - keeps up to MaxAlive enemies alive at all times.
///                When one dies it is respawned after RespawnDelay seconds.
///
///   Waves      - spawns a fixed number of enemies per wave.
///                The next wave starts only after ALL enemies from the current
///                wave are dead, plus WaveCooldown seconds.
///                Set TotalWaves to 0 for endless waves.
///
/// Setup:
///   1. Add this component to an empty GameObject (the "spawner anchor").
///   2. Drag one or more enemy prefabs into Enemy Prefabs.
///   3. Optionally add Spawn Points - child Transforms or any scene objects.
///      If left empty the spawner's own position is used (+ Spawn Radius scatter).
///   4. Choose a Spawn Mode and tune the timing fields.
/// </summary>
[AddComponentMenu("NeonBlack/Gameplay/Spawning/Enemy Spawner")]
public class EnemySpawner : MonoBehaviour
{
    public event Action<HealthComponent> EnemySpawned;

    [Header("Enemy Prefabs")]
    [Tooltip("Drag enemy prefabs here. One is chosen at random each spawn.")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Spawn Points")]
    [Tooltip("Where enemies are spawned. Leave empty to use this object's position.")]
    [SerializeField] private Transform[] spawnPoints;

    [Tooltip("Random scatter radius around each spawn point (0 = exact).")]
    [SerializeField] private float spawnRadius = 0.5f;

    [Header("Spawn Mode")]
    [SerializeField] private SpawnerMode mode = SpawnerMode.Continuous;

    public enum SpawnerMode { Continuous, Waves }

    [Header("Continuous Settings")]
    [Tooltip("How many enemies to keep alive simultaneously.")]
    [SerializeField] private int maxAlive = 3;

    [Tooltip("Seconds after an enemy dies before a replacement is spawned.")]
    [SerializeField] private float respawnDelay = 3f;

    [Header("Wave Settings")]
    [Tooltip("Number of enemies spawned at the start of each wave.")]
    [SerializeField] private int enemiesPerWave = 5;

    [Tooltip("Seconds to wait after all wave enemies are dead before the next wave.")]
    [SerializeField] private float waveCooldown = 5f;

    [Tooltip("Total waves to run. 0 = run forever.")]
    [SerializeField] private int totalWaves;

    [Header("Timing")]
    [Tooltip("Seconds before the very first spawn after the scene loads.")]
    [SerializeField] private float initialDelay = 1f;

    [Tooltip("Stagger delay (seconds) between each individual enemy spawn in the same wave/batch.")]
    [SerializeField] private float spawnStagger = 0.2f;

    private int _aliveCount;
    private int _waveNumber;
    private bool _waveInProgress;
    private readonly List<HealthComponent> _trackedEnemies = new List<HealthComponent>();

    /// <summary>True once all waves have finished (Waves mode). Always false in Continuous mode.</summary>
    public bool IsFinished { get; private set; }

    /// <summary>Number of enemies currently alive that this spawner tracks.</summary>
    public int AliveCount => _aliveCount;

    /// <summary>Currently tracked live enemies owned by this spawner.</summary>
    public IReadOnlyList<HealthComponent> TrackedEnemies => _trackedEnemies;

    private void Start()
    {
        if (GetValidEnemyPrefabs().Count == 0)
        {
            Debug.LogWarning("[EnemySpawner] No valid enemy prefabs assigned!", this);
            return;
        }

        if (mode == SpawnerMode.Continuous)
            StartCoroutine(ContinuousRoutine());
        else
            StartCoroutine(WaveRoutine());
    }

    private IEnumerator ContinuousRoutine()
    {
        yield return new WaitForSeconds(initialDelay);

        for (int i = _aliveCount; i < maxAlive; i++)
        {
            SpawnOne();
            if (spawnStagger > 0f)
                yield return new WaitForSeconds(spawnStagger);
        }

        while (true)
        {
            yield return null;
            while (_aliveCount < maxAlive)
            {
                yield return new WaitForSeconds(respawnDelay);
                SpawnOne();
                if (spawnStagger > 0f)
                    yield return new WaitForSeconds(spawnStagger);
            }
        }
    }

    private IEnumerator WaveRoutine()
    {
        yield return new WaitForSeconds(initialDelay);

        while (totalWaves == 0 || _waveNumber < totalWaves)
        {
            _waveNumber++;
            _waveInProgress = true;
            Debug.Log($"[EnemySpawner] Wave {_waveNumber} starting - {enemiesPerWave} enemies.");

            for (int i = 0; i < enemiesPerWave; i++)
            {
                SpawnOne();
                if (spawnStagger > 0f)
                    yield return new WaitForSeconds(spawnStagger);
            }

            yield return new WaitUntil(() => _aliveCount <= 0);
            _waveInProgress = false;

            if (totalWaves > 0 && _waveNumber >= totalWaves)
                break;

            Debug.Log($"[EnemySpawner] Wave {_waveNumber} cleared! Next wave in {waveCooldown}s.");
            yield return new WaitForSeconds(waveCooldown);
        }

        Debug.Log("[EnemySpawner] All waves complete.");
        IsFinished = true;
    }

    private void SpawnOne()
    {
        GameObject prefab = PickEnemyPrefab();
        if (prefab == null)
            return;

        Vector3 position = PickSpawnPoint();

        GameObject enemy = Instantiate(prefab, position, Quaternion.identity);
        _aliveCount++;

        HealthComponent health = enemy.GetComponent<HealthComponent>();
        if (health == null)
        {
            Debug.LogWarning($"[EnemySpawner] Spawned '{enemy.name}' has no HealthComponent - alive count may drift.", this);
            return;
        }

        _trackedEnemies.Add(health);
        health.OnDeath.AddListener(() => HandleTrackedEnemyDeath(health));
        EnemySpawned?.Invoke(health);
    }

    private void HandleTrackedEnemyDeath(HealthComponent health)
    {
        _aliveCount = Mathf.Max(0, _aliveCount - 1);
        if (health != null)
            _trackedEnemies.Remove(health);
    }

    private Vector3 PickSpawnPoint()
    {
        Vector3 origin = TryPickSpawnOrigin(out Vector3 spawnOrigin) ? spawnOrigin : transform.position;

        if (spawnRadius > 0f)
        {
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * spawnRadius;
            origin += new Vector3(randomOffset.x, 0f, randomOffset.y);
        }

        return origin;
    }

    private GameObject PickEnemyPrefab()
    {
        List<GameObject> validPrefabs = GetValidEnemyPrefabs();
        if (validPrefabs.Count == 0)
        {
            Debug.LogWarning("[EnemySpawner] No valid enemy prefabs are available to spawn.", this);
            return null;
        }

        return validPrefabs[UnityEngine.Random.Range(0, validPrefabs.Count)];
    }

    private List<GameObject> GetValidEnemyPrefabs()
    {
        List<GameObject> validPrefabs = new List<GameObject>();
        if (enemyPrefabs == null)
            return validPrefabs;

        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            if (enemyPrefabs[i] != null)
                validPrefabs.Add(enemyPrefabs[i]);
        }

        return validPrefabs;
    }

    private bool TryPickSpawnOrigin(out Vector3 origin)
    {
        origin = transform.position;
        if (spawnPoints == null || spawnPoints.Length == 0)
            return false;

        int validCount = 0;
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
                validCount++;
        }

        if (validCount == 0)
            return false;

        int selectedIndex = UnityEngine.Random.Range(0, validCount);
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform point = spawnPoints[i];
            if (point == null)
                continue;

            if (selectedIndex == 0)
            {
                origin = point.position;
                return true;
            }

            selectedIndex--;
        }

        return false;
    }

    public void ForceSpawnOne() => SpawnOne();

    public void TriggerNextWave()
    {
        if (mode != SpawnerMode.Waves || _waveInProgress)
            return;

        StopAllCoroutines();
        StartCoroutine(WaveRoutine());
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Gizmos.color = new Color(1f, 0.4f, 0f, 0.9f);
            foreach (Transform point in spawnPoints)
            {
                if (point == null)
                    continue;

                Gizmos.DrawWireSphere(point.position, Mathf.Max(spawnRadius, 0.15f));
                Gizmos.DrawLine(transform.position, point.position);
            }
        }
        else
        {
            Gizmos.color = new Color(1f, 0.4f, 0f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, Mathf.Max(spawnRadius, 0.15f));
        }
    }
#endif
}
}
