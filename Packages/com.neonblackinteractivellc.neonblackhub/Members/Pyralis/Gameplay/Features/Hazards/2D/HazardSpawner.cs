using System.Collections;
using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Hazards
{
    /// <summary>
    /// Owns pooling and spawn orchestration for 2D hazards. Difficulty pacing
    /// still comes from DifficultyManager, but this class keeps setup, pooling,
    /// and spawn-shape responsibilities in smaller helpers.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Combat | AuthoringCapability.Session,
        Relevance = "Orchestrates pooling and spawning of 2D hazards based on difficulty pacing.",
        Axioms = AuthoringWorldAxiom.Dimensions2D,
        NativeSetup = new[]
        {
            "Add HazardSpawner to a scene GameObject.",
            "Wire references to DifficultyManager, Camera, and Outcome Sink.",
            "Populate Hazard Entries with prefabs and weights."
        },
        FirstProof = "Start the game and verify hazards begin spawning around the play area.",
        AssignmentFields = new[] { "_hazardEntries", "_difficultyManager", "_targetCamera" }
    )]
    [DefaultExecutionOrder(-10)]
    public class HazardSpawner : MonoBehaviour
{
        [System.Serializable]
        public class HazardEntry
        {
            [Tooltip("The hazard prefab. Must have a Hazard component with HazardData assigned.")]
            public GameObject prefab;
            [Tooltip("Relative spawn weight. Higher = spawns more often relative to other types.")]
            [Range(1, 20)]
            public int weight = 1;

            [Tooltip("How many instances to pre-pool for this hazard type. Auto-expands at runtime if needed.")]
            [Range(1, 20)]
            public int poolSize = 3;

            [Tooltip("Half the visual width/height of this hazard in world units. Added to the spawn margin so the object never spawns partially off-screen.")]
            [Min(0f)]
            public float spawnSizeRadius = 0.5f;
        }

        private readonly struct SpawnBounds
        {
            public SpawnBounds(Vector3 cameraPosition, float halfWidth, float halfHeight)
            {
                CameraPosition = cameraPosition;
                HalfWidth = halfWidth;
                HalfHeight = halfHeight;
            }

            public Vector3 CameraPosition { get; }
            public float HalfWidth { get; }
            public float HalfHeight { get; }
        }

        [Header("Hazard Types")]
        [SerializeField, Tooltip("All hazard types, their weights, and per-type pool sizes.")]
        private HazardEntry[] _hazardEntries;

        [Header("References")]
        [SerializeField, Tooltip("DifficultyManager that controls all spawn timing and area settings.")]
        private DifficultyManager _difficultyManager;
        [SerializeField, Tooltip("Optional gameplay state reader. When empty, the scene orchestrator should configure this component before play.")]
        private MonoBehaviour _gameplayStateSource;
        [SerializeField, Tooltip("Optional camera bounds provider, usually CinemachineCameraRigController.")]
        private MonoBehaviour _cameraBoundsSource;
        [SerializeField, Tooltip("Camera used for spawn bounds when no camera bounds provider is configured.")]
        private Camera _targetCamera;
        [SerializeField, Tooltip("Optional hazard outcome sink. When empty, the scene orchestrator should configure this component before play.")]
        private MonoBehaviour _hazardOutcomeSource;
        [SerializeField, Tooltip("Optional pickup burst surface for hazards that spawn collectibles.")]
        private MonoBehaviour _pickupBurstSurfaceSource;

        private readonly Dictionary<int, Queue<Hazard>> _pools = new Dictionary<int, Queue<Hazard>>();
        private readonly Dictionary<Hazard, int> _hazardTypeMap = new Dictionary<Hazard, int>();
        private readonly Dictionary<int, Vector3> _prefabScales = new Dictionary<int, Vector3>();
        private readonly Dictionary<int, float> _prefabRadii = new Dictionary<int, float>();
        private readonly HashSet<Hazard> _activeHazards = new HashSet<Hazard>();
        private readonly List<int> _weightedIndices = new List<int>();
        private readonly HashSet<int> _poolExpandedWarned = new HashSet<int>();

        private bool _fillWarningLogged;
        private bool _getFromPoolErrLogged;
        private bool _missingRuntimeServicesLogged;
        private int _totalPoolSize;
        private Coroutine _spawnCoroutine;
        private IGameplayStateReader _gameplayStateReader;
        private ICameraBoundsProvider _cameraBoundsProvider;
        private IHazardOutcomeSink _hazardOutcomeSink;
        private IPickupBurstSpawnSurface _pickupBurstSpawnSurface;

        private void Awake()
        {
            _gameplayStateReader = _gameplayStateSource as IGameplayStateReader;
            _cameraBoundsProvider = _cameraBoundsSource as ICameraBoundsProvider;
            _hazardOutcomeSink = _hazardOutcomeSource as IHazardOutcomeSink;
            _pickupBurstSpawnSurface = _pickupBurstSurfaceSource as IPickupBurstSpawnSurface;
            InitializePools();
        }

        public void ConfigureRuntime(
            IGameplayStateReader stateReader,
            ICameraBoundsProvider boundsProvider,
            IHazardOutcomeSink hazardOutcomeSink,
            IPickupBurstSpawnSurface pickupBurstSpawnSurface)
        {
            if (stateReader != null)
                _gameplayStateReader = stateReader;
            if (boundsProvider != null)
                _cameraBoundsProvider = boundsProvider;
            if (hazardOutcomeSink != null)
                _hazardOutcomeSink = hazardOutcomeSink;
            if (pickupBurstSpawnSurface != null)
                _pickupBurstSpawnSurface = pickupBurstSpawnSurface;
        }

        public void StartSpawning()
        {
            if (!HasRequiredRuntimeServices())
                return;

            StopSpawning();
            _spawnCoroutine = StartCoroutine(SpawnRoutine());
        }

        public void StopSpawning()
        {
            if (_spawnCoroutine == null)
                return;

            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }

        public void ReturnToPool(Hazard hazard)
        {
            _activeHazards.Remove(hazard);
            hazard.gameObject.SetActive(false);

            if (_hazardTypeMap.TryGetValue(hazard, out int typeIndex) && _pools.ContainsKey(typeIndex))
                _pools[typeIndex].Enqueue(hazard);
        }

        public void SpawnBounceChildren(Hazard parent, Vector2 position, Vector2 direction)
        {
            if (!_hazardTypeMap.TryGetValue(parent, out int typeIndex))
                return;

            float angle = parent.Data != null ? parent.Data.splitAngle : 45f;
            float scale = parent.Data != null ? parent.Data.splitChildScale : 0.6f;

            DifficultyManager.HazardTiming childTiming = _difficultyManager != null
                ? _difficultyManager.CurrentTiming
                : new DifficultyManager.HazardTiming();
            childTiming.shadowDuration = 0f;
            childTiming.warningFlashDuration = 0f;

            const float travelDistance = 25f;
            for (int i = 0; i < 2; i++)
            {
                Hazard child = GetFromPool(typeIndex);
                if (child == null)
                    continue;

                float deflection = i == 0 ? angle : -angle;
                Vector2 childDirection = (Quaternion.Euler(0f, 0f, deflection) * direction).normalized;

                child.CrossingStart = position;
                child.CrossingEnd = position + childDirection * travelDistance;
                child.transform.position = position;
                child.SetBouncyDirectionOverride(childDirection);
                child.ConfigureRuntime(_cameraBoundsProvider, _hazardOutcomeSink, _pickupBurstSpawnSurface);
                child.transform.localScale = parent.transform.localScale * scale;
                ApplySpawnRotation(child);

                child.gameObject.SetActive(true);
                _activeHazards.Add(child);
                child.Initialize(this, childTiming);
            }

            ReturnToPool(parent);
        }

        public void ClearAllHazards()
        {
            foreach (Hazard hazard in _activeHazards)
            {
                if (hazard == null)
                    continue;

                hazard.ForceStop();
                hazard.gameObject.SetActive(false);
                if (_hazardTypeMap.TryGetValue(hazard, out int typeIndex) && _pools.ContainsKey(typeIndex))
                    _pools[typeIndex].Enqueue(hazard);
            }

            _activeHazards.Clear();
        }

        private void InitializePools()
        {
            if (_hazardEntries == null || _hazardEntries.Length == 0)
            {
                Debug.LogError("[HazardSpawner] No hazard entries assigned. Add prefabs in the Inspector.");
                return;
            }

            for (int i = 0; i < _hazardEntries.Length; i++)
            {
                HazardEntry entry = _hazardEntries[i];
                if (!IsValidEntry(entry, i))
                    continue;

                _pools[i] = new Queue<Hazard>();
                _prefabScales[i] = entry.prefab.transform.localScale;
                _prefabRadii[i] = entry.spawnSizeRadius;

                int created = CreateInitialPool(entry, i);
                if (created <= 0)
                {
                    Debug.LogError($"[HazardSpawner] Pool for '{entry.prefab.name}' (entry #{i}) has 0 valid instances - it will never spawn.");
                    continue;
                }

                AddWeightedEntryIndex(i, entry.weight);
            }

            if (_weightedIndices.Count == 0)
                Debug.LogError("[HazardSpawner] No valid hazard types available. Check prefabs and Hazard components.");

            CacheTotalPoolSize();
        }

        private bool IsValidEntry(HazardEntry entry, int entryIndex)
        {
            if (entry == null || entry.prefab == null)
            {
                Debug.LogError($"[HazardSpawner] Entry #{entryIndex} has no prefab assigned.");
                return false;
            }

            if (entry.poolSize <= 0)
            {
                Debug.LogError($"[HazardSpawner] Entry #{entryIndex} '{entry.prefab.name}' has poolSize={entry.poolSize}. Must be at least 1.");
                return false;
            }

            return true;
        }

        private int CreateInitialPool(HazardEntry entry, int typeIndex)
        {
            int created = 0;
            for (int i = 0; i < entry.poolSize; i++)
            {
                Hazard hazard = CreatePooledHazard(entry.prefab, typeIndex);
                if (hazard == null)
                    break;

                _pools[typeIndex].Enqueue(hazard);
                created++;
            }

            return created;
        }

        private Hazard CreatePooledHazard(GameObject prefab, int typeIndex)
        {
            GameObject instance = Instantiate(prefab, transform);
            Hazard hazard = instance.GetComponent<Hazard>();
            if (hazard == null)
            {
                Component[] components = instance.GetComponents<Component>();
                string names = string.Join(", ", System.Array.ConvertAll(components, component => component != null ? component.GetType().Name : "null"));
                Debug.LogError($"[HazardSpawner] '{instance.name}' is missing a Hazard component on the root GameObject. Components found: [{names}]. Move the Hazard script to the root.");
                Destroy(instance);
                return null;
            }

            hazard.gameObject.SetActive(false);
            _hazardTypeMap[hazard] = typeIndex;
            return hazard;
        }

        private void AddWeightedEntryIndex(int typeIndex, int weight)
        {
            int effectiveWeight = Mathf.Max(1, weight);
            for (int i = 0; i < effectiveWeight; i++)
                _weightedIndices.Add(typeIndex);
        }

        private void CacheTotalPoolSize()
        {
            _totalPoolSize = 0;
            if (_hazardEntries == null)
                return;

            foreach (HazardEntry entry in _hazardEntries)
            {
                if (entry != null)
                    _totalPoolSize += entry.poolSize;
            }
        }

        private IEnumerator SpawnRoutine()
        {
            float delay = _difficultyManager != null ? _difficultyManager.InitialSpawnDelay : 2f;
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            while (true)
            {
                float interval = _difficultyManager != null ? _difficultyManager.CurrentSpawnInterval : 3f;
                yield return new WaitForSeconds(interval);

                if (_gameplayStateReader == null || !_gameplayStateReader.IsGameplayActive)
                    continue;

                yield return FillToMinimumHazards();
                yield return SpawnBurst();
            }
        }

        private IEnumerator FillToMinimumHazards()
        {
            int minHazards = _difficultyManager != null ? _difficultyManager.CurrentMinHazards : 0;
            int fillLimit = _totalPoolSize > 0 ? _totalPoolSize : 20;
            int filled = 0;

            while (_activeHazards.Count < minHazards && filled < fillLimit)
            {
                SpawnRandomHazard();
                filled++;
                yield return null;
            }

            if (filled > 0 && filled >= fillLimit && _activeHazards.Count < minHazards && !_fillWarningLogged)
            {
                _fillWarningLogged = true;
                Debug.LogWarning($"[HazardSpawner] Could not fill to minHazards ({minHazards}) after {fillLimit} attempts - pool may be exhausted.");
            }
        }

        private IEnumerator SpawnBurst()
        {
            int minBurst = _difficultyManager != null ? _difficultyManager.CurrentMinSpawnCount : 1;
            int maxBurst = _difficultyManager != null ? _difficultyManager.CurrentMaxSpawnCount : 1;
            int burstCount = Random.Range(minBurst, maxBurst + 1);

            for (int i = 0; i < burstCount; i++)
            {
                SpawnRandomHazard();
                if (burstCount > 1 && i < burstCount - 1)
                    yield return new WaitForSeconds(0.08f);
            }
        }

        private void SpawnRandomHazard()
        {
            if (_weightedIndices.Count == 0)
                return;

            int maxHazards = _difficultyManager != null ? _difficultyManager.CurrentMaxHazards : 0;
            if (maxHazards > 0 && _activeHazards.Count >= maxHazards)
                return;

            int typeIndex = _weightedIndices[Random.Range(0, _weightedIndices.Count)];
            Hazard hazard = GetFromPool(typeIndex);
            if (hazard == null)
            {
                if (!_getFromPoolErrLogged)
                {
                    _getFromPoolErrLogged = true;
                    Debug.LogWarning($"[HazardSpawner] Could not get a hazard from pool for type index {typeIndex}.");
                }

                return;
            }

            hazard.transform.localScale = _prefabScales[typeIndex];
            ConfigureSpawnTransform(hazard, typeIndex);
            hazard.ConfigureRuntime(_cameraBoundsProvider, _hazardOutcomeSink, _pickupBurstSpawnSurface);

            hazard.gameObject.SetActive(true);
            _activeHazards.Add(hazard);
            hazard.Initialize(this, GetCurrentTiming());
        }

        private DifficultyManager.HazardTiming GetCurrentTiming()
        {
            return _difficultyManager != null
                ? _difficultyManager.CurrentTiming
                : new DifficultyManager.HazardTiming { shadowDuration = 2f, warningFlashDuration = 0.5f };
        }

        private void ConfigureSpawnTransform(Hazard hazard, int typeIndex)
        {
            bool usesCrossingPath = hazard.Data != null && RequiresCrossingPositions(hazard.Data.hazardType);
            if (usesCrossingPath)
            {
                GetCrossingPositions(hazard.Data, out Vector2 start, out Vector2 end);
                hazard.CrossingStart = start;
                hazard.CrossingEnd = end;
                hazard.transform.position = start;
            }
            else
            {
                float radius = _prefabRadii.ContainsKey(typeIndex) ? _prefabRadii[typeIndex] : 0f;
                hazard.transform.position = GetSpawnPosition(radius);
            }

            ApplySpawnRotation(hazard);
        }

        private static void ApplySpawnRotation(Hazard hazard)
        {
            if (hazard.Data == null)
            {
                hazard.transform.rotation = Quaternion.identity;
                return;
            }

            float spawnAngle = hazard.Data.randomRotationOnSpawn
                ? Random.Range(0f, 360f)
                : hazard.Data.fixedSpawnRotation;
            hazard.transform.rotation = Quaternion.Euler(0f, 0f, spawnAngle);
        }

        private Vector2 GetSpawnPosition(float objectRadius = 0f)
        {
            if (!TryGetSpawnBounds(objectRadius, out SpawnBounds bounds))
                return Vector2.zero;

            float minDistance = _difficultyManager != null ? _difficultyManager.MinDistanceFromPlayer : 1.5f;
            float edgeBias = _difficultyManager != null ? _difficultyManager.EdgeBias : 0f;

            Vector2 position = Vector2.zero;
            int attempts = 0;
            Transform nearestParticipant;
            float nearestDistance;
            do
            {
                position = edgeBias > 0f && Random.value < edgeBias
                    ? PickEdgePoint(bounds)
                    : GetRandomInteriorPoint(bounds);
                attempts++;
            }
            while (ParticipantQueryUtility.TryGetClosestParticipantTransform(position, out nearestParticipant, out nearestDistance)
                && nearestDistance < minDistance
                && attempts < 15);

            return position;
        }

        private static Vector2 PickEdgePoint(SpawnBounds bounds)
        {
            int edge = Random.Range(0, 4);
            float t = Random.value;
            switch (edge)
            {
                case 0:
                    return new Vector2(
                        Mathf.Lerp(bounds.CameraPosition.x - bounds.HalfWidth, bounds.CameraPosition.x + bounds.HalfWidth, t),
                        bounds.CameraPosition.y + bounds.HalfHeight);
                case 1:
                    return new Vector2(
                        Mathf.Lerp(bounds.CameraPosition.x - bounds.HalfWidth, bounds.CameraPosition.x + bounds.HalfWidth, t),
                        bounds.CameraPosition.y - bounds.HalfHeight);
                case 2:
                    return new Vector2(
                        bounds.CameraPosition.x - bounds.HalfWidth,
                        Mathf.Lerp(bounds.CameraPosition.y - bounds.HalfHeight, bounds.CameraPosition.y + bounds.HalfHeight, t));
                default:
                    return new Vector2(
                        bounds.CameraPosition.x + bounds.HalfWidth,
                        Mathf.Lerp(bounds.CameraPosition.y - bounds.HalfHeight, bounds.CameraPosition.y + bounds.HalfHeight, t));
            }
        }

        private static Vector2 GetRandomInteriorPoint(SpawnBounds bounds)
        {
            return new Vector2(
                Random.Range(bounds.CameraPosition.x - bounds.HalfWidth, bounds.CameraPosition.x + bounds.HalfWidth),
                Random.Range(bounds.CameraPosition.y - bounds.HalfHeight, bounds.CameraPosition.y + bounds.HalfHeight));
        }

        private static bool RequiresCrossingPositions(HazardData.HazardType type)
        {
            return type == HazardData.HazardType.Crossing;
        }

        private void GetCrossingPositions(HazardData data, out Vector2 start, out Vector2 end)
        {
            float margin = _difficultyManager != null ? _difficultyManager.SpawnMargin : 0.5f;
            SpawnBounds bounds = GetCameraBounds(0f);
            const float offscreenPad = 1f;

            if (data.crossingAxis == HazardData.CrossingAxis.Horizontal)
            {
                float y = Random.Range(bounds.CameraPosition.y - bounds.HalfHeight + margin, bounds.CameraPosition.y + bounds.HalfHeight - margin);
                bool fromLeft = Random.value < 0.5f;
                start = new Vector2(bounds.CameraPosition.x + (fromLeft ? -bounds.HalfWidth - offscreenPad : bounds.HalfWidth + offscreenPad), y);
                end = new Vector2(bounds.CameraPosition.x + (fromLeft ? bounds.HalfWidth + offscreenPad : -bounds.HalfWidth - offscreenPad), y);
                return;
            }

            if (data.crossingAxis == HazardData.CrossingAxis.Vertical)
            {
                float x = Random.Range(bounds.CameraPosition.x - bounds.HalfWidth + margin, bounds.CameraPosition.x + bounds.HalfWidth - margin);
                bool fromBottom = Random.value < 0.5f;
                start = new Vector2(x, bounds.CameraPosition.y + (fromBottom ? -bounds.HalfHeight - offscreenPad : bounds.HalfHeight + offscreenPad));
                end = new Vector2(x, bounds.CameraPosition.y + (fromBottom ? bounds.HalfHeight + offscreenPad : -bounds.HalfHeight - offscreenPad));
                return;
            }

            int diagonalDirection = Random.Range(0, 4);
            float xLeft = bounds.CameraPosition.x - bounds.HalfWidth - offscreenPad;
            float xRight = bounds.CameraPosition.x + bounds.HalfWidth + offscreenPad;
            float yBottom = bounds.CameraPosition.y - bounds.HalfHeight - offscreenPad;
            float yTop = bounds.CameraPosition.y + bounds.HalfHeight + offscreenPad;

            switch (diagonalDirection)
            {
                case 0:
                    start = new Vector2(xLeft, yBottom);
                    end = new Vector2(xRight, yTop);
                    break;
                case 1:
                    start = new Vector2(xRight, yBottom);
                    end = new Vector2(xLeft, yTop);
                    break;
                case 2:
                    start = new Vector2(xLeft, yTop);
                    end = new Vector2(xRight, yBottom);
                    break;
                default:
                    start = new Vector2(xRight, yTop);
                    end = new Vector2(xLeft, yBottom);
                    break;
            }
        }

        private Hazard GetFromPool(int typeIndex)
        {
            if (_pools.ContainsKey(typeIndex) && _pools[typeIndex].Count > 0)
                return _pools[typeIndex].Dequeue();

            if (typeIndex < _hazardEntries.Length && _hazardEntries[typeIndex]?.prefab != null)
            {
                if (_poolExpandedWarned.Add(typeIndex))
                {
                    Debug.LogWarning($"[HazardSpawner] Pool for '{_hazardEntries[typeIndex].prefab.name}' exhausted - auto-expanding. Consider increasing poolSize in the Inspector.");
                }

                return CreatePooledHazard(_hazardEntries[typeIndex].prefab, typeIndex);
            }

            return null;
        }

        private bool TryGetSpawnBounds(float objectRadius, out SpawnBounds bounds)
        {
            float margin = (_difficultyManager != null ? _difficultyManager.SpawnMargin : 0.5f) + objectRadius;
            if (TryGetCameraBounds(margin, out CameraBounds2D cameraBounds))
            {
                bounds = new SpawnBounds(cameraBounds.Center, cameraBounds.HalfWidth, cameraBounds.HalfHeight);
                return true;
            }

            bounds = default;
            return false;
        }

        private SpawnBounds GetCameraBounds(float margin)
        {
            if (TryGetCameraBounds(margin, out CameraBounds2D bounds))
                return new SpawnBounds(bounds.Center, bounds.HalfWidth, bounds.HalfHeight);

            return new SpawnBounds(Vector3.zero, 0.1f, 0.1f);
        }

        private bool TryGetCameraBounds(float margin, out CameraBounds2D bounds)
        {
            ICameraBoundsProvider provider = _cameraBoundsProvider ?? _cameraBoundsSource as ICameraBoundsProvider;
            if (provider != null && provider.TryGetCameraBounds2D(margin, out bounds))
                return true;

            if (_targetCamera != null)
            {
                bounds = new CameraBounds2D(
                    _targetCamera,
                    _targetCamera.transform.position,
                    _targetCamera.orthographicSize * _targetCamera.aspect - margin,
                    _targetCamera.orthographicSize - margin);
                return true;
            }

            bounds = default;
            return false;
        }

        private bool HasRequiredRuntimeServices()
        {
            if (_gameplayStateReader != null && _hazardOutcomeSink != null && (_cameraBoundsProvider != null || _targetCamera != null))
                return true;

            if (!_missingRuntimeServicesLogged)
            {
                _missingRuntimeServicesLogged = true;
                Debug.LogError("[HazardSpawner] Missing runtime services. Configure gameplay state, hazard outcome, and camera bounds services before spawning.", this);
            }

            return false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            float margin = _difficultyManager != null ? _difficultyManager.SpawnMargin : 0.5f;
            if (!TryGetCameraBounds(margin, out CameraBounds2D bounds))
                return;

            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.25f);
            Gizmos.DrawWireCube(new Vector3(bounds.Center.x, bounds.Center.y, 0f), new Vector3(bounds.HalfWidth * 2f, bounds.HalfHeight * 2f, 0f));
        }
#endif
    }
}
