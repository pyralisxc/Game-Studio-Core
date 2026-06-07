using System.Collections;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Features.GameFlow;
using NeonBlack.Gameplay.Presentation.Camera;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Pickups
{

/// <summary>
/// Manages the collectible object pool for the 2D score loop. Handles initial spawning,
/// periodic and burst spawning, and position-targeted spawning (hazard interactions).
/// Supports minimum on-screen count enforcement and spawn clustering.
/// Setup: Attach to the "Spawners" GameObject.
/// Assign _crumbPrefab (a prefab with Collectible2D component) in the Inspector.
/// Wire into GameManager's _pickupSpawner slot.
/// </summary>
[DefaultExecutionOrder(-10)]
[AddComponentMenu("NeonBlack/Gameplay/Pickups/Collectible Spawner 2D")]
public class CollectibleSpawner2D : MonoBehaviour, IPickupSpawnSurface
{
    public static CollectibleSpawner2D Instance { get; private set; }

    [Header("Collectible Prefab")]
    [SerializeField, Tooltip("The collectible prefab (must have Collectible2D component).")]
    private GameObject _crumbPrefab;

    [Header("Pool")]
    [SerializeField, Tooltip("Number of collectible instances to pre-pool. Set this to your target maximum active collectibles. Pool grows at runtime if exhausted but pre-pooling avoids allocation spikes. 200 = reasonable floor, 1000 = trail-density max.")]
    private int _poolSize = 200;

    [Header("Initial Spawn")]
    [SerializeField, Tooltip("Collectibles placed on the screen when a round begins.")]
    private int _initialCrumbCount = 80;
    [SerializeField, Tooltip("If > 0, collectibles are grouped into clusters of this size at start.")]
    private int _initialClusterSize = 0;
    [SerializeField, Tooltip("Radius of each cluster in world units (used when clusterSize > 0).")]
    private float _initialClusterRadius = 0.4f;

    [Header("Periodic Spawn")]
    [SerializeField, Tooltip("Seconds between automatic single collectible spawns during play.")]
    private float _spawnInterval = 5f;
    [SerializeField, Tooltip("If > 0, also guarantees at least this many collectibles are always on screen. Checked every spawn interval and immediately after any collectible is returned to the pool.")]
    private int _minimumOnScreen = 50;

    [Header("Burst Spawn")]
    [SerializeField, Tooltip("Number of collectibles to drop in a burst when a burst is triggered (e.g., from a cereal box hazard).")]
    private int _burstCount = 5;
    [SerializeField, Tooltip("Radius of the burst scatter in world units.")]
    private float _burstRadius = 0.6f;

    [Header("Spawn Area")]
    [SerializeField, Tooltip("World-unit margin from camera edges where collectibles will not spawn.")]
    private float _spawnMargin = 0.3f;
    [SerializeField, Tooltip("Half the visual size of a collectible in world units. Added to _spawnMargin so collectibles never spawn partially off-screen.")]
    [Min(0f)]
    private float _crumbSizeRadius = 0.1f;
    [SerializeField, Tooltip("Minimum world-unit distance from player when spawning a collectible. 0 = no restriction.")]
    private float _minDistanceFromPlayer = 0.5f;
    [SerializeField, Tooltip("Optional surface override for spawn positions and runtime spawn availability. Defaults to this component when unset.")]
    private MonoBehaviour _spawnSurfaceSource;

    private Queue<Collectible2D> _pool = new Queue<Collectible2D>();
    // HashSet instead of List: ReturnCollectible calls Remove() which is O(n) on a List.
    // At 200-1000 collectibles with mass hazard sweeps, O(1) HashSet removal matters.
    private HashSet<Collectible2D> _activeCrumbs = new HashSet<Collectible2D>();
    private Coroutine _periodicCoroutine;
    private Camera _mainCamera;
    private Vector3 _crumbPrefabScale;
    private IPickupSpawnSurface _spawnSurface;

    public bool CanAcceptRuntimeSpawns => IsGameplayActive();

    private void Update()
    {
        // Centralized bob tick \u2014 one MonoBehaviour Update instead of N individual Collectible2D Updates.
        // The native\u2192managed bridge call per MonoBehaviour.Update is the bottleneck at high collectible counts.
        float dt = Time.deltaTime;
        foreach (Collectible2D c in _activeCrumbs)
            c.Tick(dt);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        _spawnSurface = ResolveSpawnSurface();
        _mainCamera = CameraAspectController.Main != null ? CameraAspectController.Main : Camera.main;
        if (_mainCamera == null)
            Debug.LogError("[CollectibleSpawner2D] Camera.main is null. Make sure your Main Camera is tagged 'MainCamera'.");
        if (_crumbPrefab == null)
        {
            Debug.LogError("[CollectibleSpawner2D] _crumbPrefab is not assigned in the Inspector. Drag your Collectible2D prefab into the CollectibleSpawner2D's _crumbPrefab slot.");
            return;
        }
        _crumbPrefabScale = _crumbPrefab.transform.localScale;
        InitializePool();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void InitializePool()
    {
        for (int i = 0; i < _poolSize; i++)
        {
            Collectible2D c = Instantiate(_crumbPrefab, transform).GetComponent<Collectible2D>();
            if (c == null)
            {
                Debug.LogError("[CollectibleSpawner2D] The _crumbPrefab does not have a Collectible2D component attached.");
                return;
            }
            c.gameObject.SetActive(false);
            _pool.Enqueue(c);
        }
    }

    /// <summary>Clears the board and spawns the initial set of collectibles. Starts periodic spawning.</summary>
    public void SpawnInitialCollectibles()
    {
        if (!HasGameManager())
        {
            Debug.LogError("[CollectibleSpawner2D] SpawnInitialCollectibles() called with no GameManager in scene. Ensure CollectibleSpawner2D only exists in the Game scene.");
            return;
        }
        ClearAllCollectibles();
        SpawnInitialWave();

        // Stop any leftover coroutine from a previous SpawnInitialCollectibles() call.
        // Without this, calling StartGame() without a full scene reload would stack multiple PeriodicSpawnRoutines.
        if (_periodicCoroutine != null) StopCoroutine(_periodicCoroutine);
        _periodicCoroutine = StartCoroutine(PeriodicSpawnRoutine());
    }

    /// <summary>Spawns collectibles in a scatter around a world position using _burstCount and _burstRadius.</summary>
    public void SpawnCollectibleBurstAt(Vector2 center)
    {
        SpawnCollectiblesAt(center, _burstCount, _burstRadius);
    }

    /// <summary>Spawns count collectibles scattered around a world position with a given radius.</summary>
    public void SpawnCollectiblesAt(Vector2 center, int count, float radius = 0.5f)
    {
        for (int i = 0; i < count; i++)
            SpawnCollectibleAt(center + Random.insideUnitCircle * radius);
    }

    private void SpawnCollectibleAt(Vector2 position)
    {
        Collectible2D collectible = GetFromPool();
        if (collectible == null) return;

        // Reset scale \u2014 effects or animations may have modified it during the previous activation.
        collectible.transform.localScale = _crumbPrefabScale;
        collectible.transform.position = new Vector3(position.x, position.y, 0f);
        collectible.gameObject.SetActive(true);
        _activeCrumbs.Add(collectible);
    }

    public bool TryGetSpawnPosition(out Vector2 position)
    {
        if (_mainCamera == null)
        {
            position = Vector2.zero;
            return false;
        }

        float totalMargin = _spawnMargin + _crumbSizeRadius;
        float baseH = CameraAspectController.Instance != null
            ? CameraAspectController.Instance.HalfHeight
            : _mainCamera.orthographicSize;
        float baseW = CameraAspectController.Instance != null
            ? CameraAspectController.Instance.HalfWidth
            : _mainCamera.orthographicSize * _mainCamera.aspect;
        float halfH = Mathf.Max(baseH - totalMargin, 0.1f);
        float halfW = Mathf.Max(baseW - totalMargin, 0.1f);
        Vector3 camPos = _mainCamera.transform.position;

        int attempts = 0;
        do
        {
            position = new Vector2(
                Random.Range(camPos.x - halfW, camPos.x + halfW),
                Random.Range(camPos.y - halfH, camPos.y + halfH));
            attempts++;
        }
        while (!IsValidSpawnDistance(position) && attempts < 10);

        if (!IsValidSpawnDistance(position))
        {
            Debug.LogWarning("[CollectibleSpawner2D] Could not find a spawn position far enough from the active participants after 10 attempts. Consider reducing _minDistanceFromPlayer or the spawn margin.");
        }

        return true;
    }

    private IEnumerator PeriodicSpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_spawnInterval);
            if (_spawnSurface == null || !_spawnSurface.CanAcceptRuntimeSpawns)
                continue;

            RefillToMinimumOrSpawnSingle();
        }
    }

    public void ReturnCollectible(Collectible2D collectible)
    {
        _activeCrumbs.Remove(collectible);
        collectible.gameObject.SetActive(false);
        _pool.Enqueue(collectible);
        CheckMinimum();
    }

    /// <summary>
    /// Immediately tops up active collectibles to _minimumOnScreen when the count drops below it.
    /// Called from ReturnCollectible so a mass sweep (explosion, slam) doesn't leave the board empty
    /// between periodic spawn intervals.
    /// </summary>
    private void CheckMinimum()
    {
        if (_minimumOnScreen <= 0) return;
        if (_spawnSurface == null || !_spawnSurface.CanAcceptRuntimeSpawns) return;
        int deficit = _minimumOnScreen - _activeCrumbs.Count;
        for (int i = 0; i < deficit; i++)
            TrySpawnRandomCollectible();
    }

    private void SpawnInitialWave()
    {
        if (_initialClusterSize > 1)
        {
            int clusters = Mathf.CeilToInt((float)_initialCrumbCount / _initialClusterSize);
            for (int c = 0; c < clusters; c++)
            {
                if (!_spawnSurface.TryGetSpawnPosition(out Vector2 center))
                    return;
                int inThisCluster = Mathf.Min(_initialClusterSize, _initialCrumbCount - c * _initialClusterSize);
                for (int i = 0; i < inThisCluster; i++)
                    SpawnCollectibleAt(center + Random.insideUnitCircle * _initialClusterRadius);
            }
            return;
        }

        for (int i = 0; i < _initialCrumbCount; i++)
            TrySpawnRandomCollectible();
    }

    private void RefillToMinimumOrSpawnSingle()
    {
        // Enforce minimum on-screen count first.
        if (_minimumOnScreen > 0)
        {
            int deficit = _minimumOnScreen - _activeCrumbs.Count;
            for (int i = 0; i < deficit; i++)
                TrySpawnRandomCollectible();
            return;
        }

        TrySpawnRandomCollectible();
    }

    private static bool HasGameManager()
    {
        return GameManager.Instance != null;
    }

    private static bool IsGameplayActive()
    {
        return GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing;
    }

    private void TrySpawnRandomCollectible()
    {
        if (_spawnSurface != null && _spawnSurface.TryGetSpawnPosition(out Vector2 position))
            SpawnCollectibleAt(position);
    }

    private bool IsValidSpawnDistance(Vector2 position)
    {
        if (_minDistanceFromPlayer <= 0f)
            return true;

        return !ParticipantQueryUtility.TryGetClosestParticipantTransform(position, out _, out float nearestDistance)
            || nearestDistance >= _minDistanceFromPlayer;
    }

    /// <summary>Stops periodic spawning and deactivates all active collectibles.</summary>
    public void ClearAllCollectibles()
    {
        if (_periodicCoroutine != null)
        {
            StopCoroutine(_periodicCoroutine);
            _periodicCoroutine = null;
        }

        // HashSet doesn't support index-based reverse iteration \u2014 foreach + Clear is safe
        // because we do not modify the set during the loop (Clear happens after).
        foreach (Collectible2D c in _activeCrumbs)
        {
            if (c != null)
            {
                c.gameObject.SetActive(false);
                _pool.Enqueue(c);
            }
        }
        _activeCrumbs.Clear();
    }

    private Collectible2D GetFromPool()
    {
        if (_pool.Count > 0) return _pool.Dequeue();

        if (_crumbPrefab != null)
        {
            Collectible2D c = Instantiate(_crumbPrefab, transform).GetComponent<Collectible2D>();
            return c;
        }
        return null;
    }

    private IPickupSpawnSurface ResolveSpawnSurface()
    {
        if (_spawnSurfaceSource is IPickupSpawnSurface configuredSurface)
            return configuredSurface;

        return this;
    }
}

} // namespace NeonBlack.Gameplay.Features.Pickups
