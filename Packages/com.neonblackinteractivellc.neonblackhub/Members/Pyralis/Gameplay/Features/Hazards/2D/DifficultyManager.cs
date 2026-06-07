using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeonBlack.Gameplay.Features.Hazards
{
/// <summary>
/// Controls all difficulty parameters fed to HazardSpawner each spawn cycle.
/// Modes: Linear, Exponential, Steps, Wave.
/// Wave mode supports a configurable list of named waves, each with its own timings,
/// hazard counts, and duration range. Pattern can be Random, Sequential, or Weighted.
/// Setup: Attach to the same GameObject as GameManager.
/// Wire into GameManager's _difficultyManager slot.
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    // Shared structs / enums

    /// <summary>Full set of timing values passed to each spawned hazard.</summary>
    public struct HazardTiming
    {
        /// <summary>Seconds the translucent shadow is visible.</summary>
        public float shadowDuration;
        /// <summary>Seconds the warning outline flashes before slam.</summary>
        public float warningFlashDuration;
        /// <summary>Seconds the slam sprite stays on screen. 0 = use HazardData value.</summary>
        public float slamDuration;
        /// <summary>Seconds the hazard fades out after retracting. 0 = use HazardData value.</summary>
        public float retractDuration;
    }

    public enum DifficultyMode  { Linear, Exponential, Steps, Wave }
    public enum WavePatternMode { Random, Sequential, Weighted }

    // Wave entry

    [System.Serializable]
    public class WaveEntry
    {
        [Tooltip("Label shown in the Inspector for readability (e.g. 'Chill', 'Storm').")]
        public string waveName = "Wave";

        [Header("Spawn Timing")]
        [Tooltip("Seconds between hazard spawns during this wave.")]
        public float spawnInterval = 2f;
        [Tooltip("Seconds the shadow is visible before the warning flash.")]
        public float shadowDuration = 2f;
        [Tooltip("Seconds the warning outline flashes before the slam.")]
        public float warningFlashDuration = 0.5f;
        [Tooltip("Seconds the slam sprite stays fully visible. 0 = use the hazard's HazardData value.")]
        public float slamDuration = 0f;
        [Tooltip("Seconds the hazard takes to fade out. 0 = use the hazard's HazardData value.")]
        public float retractDuration = 0f;

        [Header("Hazard Count")]
        [Tooltip("Minimum hazards kept on screen during this wave. 0 = no minimum.")]
        public int minHazards = 0;
        [Tooltip("Maximum hazards allowed on screen during this wave. 0 = unlimited.")]
        public int maxHazards = 0;
        [Tooltip("Minimum hazards spawned per interval burst during this wave.")]
        [Range(1, 10)]
        public int minSpawnCount = 1;
        [Tooltip("Maximum hazards spawned per interval burst during this wave.")]
        [Range(1, 10)]
        public int maxSpawnCount = 1;

        [Header("Wave Duration")]
        [Tooltip("Minimum seconds this wave lasts before switching.")]
        public float minDuration = 5f;
        [Tooltip("Maximum seconds this wave lasts before switching.")]
        public float maxDuration = 12f;

        [Header("Selection")]
        [Tooltip("Relative weight for Weighted pattern mode. Higher = chosen more often.")]
        [Range(1, 20)]
        public int weight = 1;
    }

    // Inspector fields

    [Header("Mode")]
    [SerializeField, Tooltip("Which difficulty curve to use.")]
    private DifficultyMode _mode = DifficultyMode.Linear;

    [Header("Base Timings (Linear / Exponential / Steps)")]
    [SerializeField, Tooltip("Starting seconds between hazard spawns.")]
    private float _initialSpawnInterval = 3.0f;
    [SerializeField, Tooltip("Minimum allowed seconds between hazard spawns (hard floor).")]
    private float _minSpawnInterval = 0.8f;
    [SerializeField, Tooltip("Starting seconds the shadow is visible before the warning flash.")]
    private float _initialShadowDuration = 2.0f;
    [SerializeField, Tooltip("Seconds the warning outline flashes before the slam. Usually kept short (0.3-0.6s).")]
    private float _warningFlashDuration = 0.5f;
    [SerializeField, Tooltip("Seconds the slam sprite stays on screen. 0 = each hazard uses its own HazardData value.")]
    private float _slamDuration = 0f;
    [SerializeField, Tooltip("Seconds the hazard fades out. 0 = each hazard uses its own HazardData value.")]
    private float _retractDuration = 0f;
    [SerializeField, Tooltip("Minimum allowed shadow duration (hard floor). Keeps hazards readable.")]
    private float _minShadowDuration = 0.3f;
    [SerializeField, Tooltip("Minimum hazards spawned in a single burst (Linear/Exponential/Steps).")]
    [Range(1, 10)]
    private int _initialMinSpawnCount = 1;
    [SerializeField, Tooltip("Maximum hazards spawned in a single burst (Linear/Exponential/Steps).")]
    [Range(1, 10)]
    private int _initialMaxSpawnCount = 1;

    [Header("Difficulty Curve")]
    [SerializeField, Tooltip("Rate at which spawn interval decreases per second (Linear) or is used as the exponential curve input (Exponential).")]
    [Range(0.01f, 0.2f)]
    private float _difficultyRate = 0.04f;
    [SerializeField, Tooltip("Independent rate at which shadow duration decreases. Smaller = shadow stays long for longer, keeping hazards readable at higher difficulty. Tune separately from spawn interval rate.")]
    [Range(0.001f, 0.1f)]
    private float _shadowDifficultyRate = 0.02f;
    [SerializeField, Tooltip("Exponential mode only - controls curve steepness. Higher = gentler early ramp.")]
    [Range(0.5f, 5f)]
    private float _exponentialBase = 1.5f;
    [SerializeField, Tooltip("Linear/Exponential: seconds between each +1 to the minimum hazard count. 0 = static.")]
    private float _minHazardsGrowthInterval = 0f;
    [SerializeField, Tooltip("Linear/Exponential: seconds between each +1 to the maximum hazard count. 0 = static.")]
    private float _maxHazardsGrowthInterval = 0f;
    [SerializeField, Tooltip("Linear/Exponential: seconds between each +1 to the minimum spawn count per burst. 0 = static.")]
    private float _minSpawnCountGrowthInterval = 0f;
    [SerializeField, Tooltip("Linear/Exponential: seconds between each +1 to the maximum spawn count per burst. 0 = static.")]
    private float _maxSpawnCountGrowthInterval = 0f;

    [Header("Step Mode")]
    [SerializeField, Tooltip("Step mode only - seconds between each difficulty step-up.")]
    private float _stepInterval = 30f;
    [SerializeField, Tooltip("Step mode only - how much spawn interval shrinks per step.")]
    private float _spawnIntervalStepAmount = 0.3f;
    [SerializeField, Tooltip("Step mode only - how much shadow duration shrinks per step.")]
    private float _shadowDurationStepAmount = 0.2f;
    [SerializeField, Tooltip("Step mode only - how much the minimum hazard count increases per step.")]
    private int _minHazardsStepAmount = 0;
    [SerializeField, Tooltip("Step mode only - how much the maximum hazard count increases per step.")]
    private int _maxHazardsStepAmount = 1;
    [SerializeField, Tooltip("Step mode only - how much the minimum spawn count per burst increases per step. 0 = never grows.")]
    private int _minSpawnCountStepAmount = 0;
    [SerializeField, Tooltip("Step mode only - how much the maximum spawn count per burst increases per step. 0 = never grows. Set to 1 to add one extra hazard per burst each step.")]
    private int _maxSpawnCountStepAmount = 1;

    [Header("Wave Mode - Waves")]
    [SerializeField, Tooltip("All possible waves. Each wave has its own timings, hazard counts, duration, and weight.")]
    private WaveEntry[] _waves;

    [Header("Wave Mode - Pattern")]
    [SerializeField, Tooltip("How the next wave is chosen: Random (ignores weight), Sequential (loops in order), Weighted (uses weight field).")]
    private WavePatternMode _wavePattern = WavePatternMode.Random;
    [SerializeField, Tooltip("Index of the wave to start on (0-based). -1 = choose by pattern (random/weighted) or index 0 (sequential).")]
    [Range(-1, 19)]
    private int _startingWaveIndex = -1;

    [Header("Spawn Gating")]
    [SerializeField, Tooltip("Seconds to wait after the game starts before the first hazard appears.")]
    private float _initialSpawnDelay = 2f;
    [SerializeField, Tooltip("Minimum hazards kept on screen at all times (Linear/Exponential/Steps). 0 = no minimum.")]
    private int _initialMinHazards = 0;
    [SerializeField, Tooltip("Maximum hazards allowed on screen at once (Linear/Exponential/Steps). 0 = unlimited.")]
    private int _initialMaxHazards = 0;

    [Header("Spawn Area")]
    [SerializeField, Tooltip("Margin in world units from the camera edges where hazards will not spawn.")]
    private float _spawnMargin = 0.5f;
    [SerializeField, Tooltip("Minimum world-unit distance from the player when picking a spawn position.")]
    private float _minDistanceFromPlayer = 1.5f;
    [SerializeField, Tooltip("0 = fully random. 1 = always near screen edges. Blends between the two.")]
    [Range(0f, 1f)]
    private float _edgeBias = 0f;

    [Header("Debug")]
#if UNITY_EDITOR
    [SerializeField, Tooltip("Show live values in the Inspector during Play mode.")]
    private bool _showDebugValues = false;
    // NonSerialized - these are live display values only. Marking them [SerializeField]
    // inside #if UNITY_EDITOR causes scene-file serialization warnings on stripped builds.
    [System.NonSerialized] public float  _debugSpawnInterval;
    [System.NonSerialized] public float  _debugShadowDuration;
    [System.NonSerialized] public float  _debugWarningFlash;
    [System.NonSerialized] public string _debugCurrentWave;
#endif

    // Public events

    /// <summary>Fired each time the difficulty advances a step. Passes the new step index.</summary>
    public UnityEvent<int> OnStepAdvanced;
    /// <summary>Fired each time the active wave changes. Passes the new wave index.</summary>
    public UnityEvent<int> OnWaveChanged;

    // Runtime state

    private float _elapsedTime;
    private bool  _isActive;

    private int   _currentStep;
    private float _stepSpawnInterval;
    private float _stepShadowDuration;
    private int   _stepMinHazards;
    private int   _stepMaxHazards;
    private int   _stepMinSpawnCount;
    private int   _stepMaxSpawnCount;

    private int   _currentWaveIndex;
    private float _waveTimer;
    private List<int> _waveWeightedIndices = new List<int>();

    // Public properties

    public float ElapsedTime           => _elapsedTime;
    public float InitialSpawnDelay     => _initialSpawnDelay;
    public float SpawnMargin           => _spawnMargin;
    public float MinDistanceFromPlayer => _minDistanceFromPlayer;
    public float EdgeBias              => _edgeBias;

    private WaveEntry ActiveWave => (_mode == DifficultyMode.Wave && _waves != null
        && _waves.Length > 0 && _currentWaveIndex < _waves.Length)
        ? _waves[_currentWaveIndex] : null;

    public float CurrentSpawnInterval
    {
        get
        {
            switch (_mode)
            {
                case DifficultyMode.Exponential:
                    float expT = 1f - Mathf.Exp(-_difficultyRate * _elapsedTime / _exponentialBase);
                    return Mathf.Max(_minSpawnInterval, Mathf.Lerp(_initialSpawnInterval, _minSpawnInterval, expT));
                case DifficultyMode.Steps:
                    return _stepSpawnInterval;
                case DifficultyMode.Wave:
                    return ActiveWave != null ? ActiveWave.spawnInterval : _initialSpawnInterval;
                default:
                    return Mathf.Max(_minSpawnInterval, _initialSpawnInterval - _elapsedTime * _difficultyRate);
            }
        }
    }

    /// <summary>Full timing bundle for the hazard about to spawn.</summary>
    public HazardTiming CurrentTiming
    {
        get
        {
            if (ActiveWave != null)
            {
                return new HazardTiming
                {
                    shadowDuration       = Mathf.Max(0.05f, ActiveWave.shadowDuration),
                    warningFlashDuration = Mathf.Max(0.05f, ActiveWave.warningFlashDuration),
                    slamDuration         = ActiveWave.slamDuration,
                    retractDuration      = ActiveWave.retractDuration
                };
            }

            float shadow;
            switch (_mode)
            {
                case DifficultyMode.Steps:
                    shadow = _stepShadowDuration;
                    break;
                case DifficultyMode.Exponential:
                    float expT = 1f - Mathf.Exp(-_shadowDifficultyRate * _elapsedTime / _exponentialBase);
                    shadow = Mathf.Max(_minShadowDuration, Mathf.Lerp(_initialShadowDuration, _minShadowDuration, expT));
                    break;
                default: // Linear
                    shadow = Mathf.Max(_minShadowDuration, _initialShadowDuration - _elapsedTime * _shadowDifficultyRate);
                    break;
            }

            return new HazardTiming
            {
                shadowDuration       = shadow,
                warningFlashDuration = _warningFlashDuration,
                slamDuration         = _slamDuration,
                retractDuration      = _retractDuration
            };
        }
    }

    public int CurrentMinHazards
    {
        get
        {
            if (_mode == DifficultyMode.Steps) return _stepMinHazards;
            if (ActiveWave != null) return ActiveWave.minHazards;
            int growth = _minHazardsGrowthInterval > 0f ? Mathf.FloorToInt(_elapsedTime / _minHazardsGrowthInterval) : 0;
            return Mathf.Max(0, _initialMinHazards + growth);
        }
    }

    public int CurrentMaxHazards
    {
        get
        {
            if (_mode == DifficultyMode.Steps) return _stepMaxHazards;
            WaveEntry wave = ActiveWave;
            if (wave != null) return wave.maxHazards;
            if (_initialMaxHazards == 0) return 0;
            int minGrowth = _minHazardsGrowthInterval > 0f ? Mathf.FloorToInt(_elapsedTime / _minHazardsGrowthInterval) : 0;
            int minVal    = Mathf.Max(0, _initialMinHazards + minGrowth);
            int maxGrowth = _maxHazardsGrowthInterval > 0f ? Mathf.FloorToInt(_elapsedTime / _maxHazardsGrowthInterval) : 0;
            return Mathf.Max(_initialMaxHazards + maxGrowth, minVal);
        }
    }

    public int CurrentMinSpawnCount
    {
        get
        {
            if (_mode == DifficultyMode.Steps) return _stepMinSpawnCount;
            if (ActiveWave != null) return Mathf.Max(1, ActiveWave.minSpawnCount);
            int growth = _minSpawnCountGrowthInterval > 0f ? Mathf.FloorToInt(_elapsedTime / _minSpawnCountGrowthInterval) : 0;
            return Mathf.Max(1, _initialMinSpawnCount + growth);
        }
    }

    public int CurrentMaxSpawnCount
    {
        get
        {
            if (_mode == DifficultyMode.Steps) return _stepMaxSpawnCount;
            WaveEntry wave = ActiveWave;
            int minGrowth = _minSpawnCountGrowthInterval > 0f ? Mathf.FloorToInt(_elapsedTime / _minSpawnCountGrowthInterval) : 0;
            int minVal    = Mathf.Max(1, _initialMinSpawnCount + minGrowth);
            if (wave != null) return Mathf.Max(minVal, wave.maxSpawnCount);
            int maxGrowth = _maxSpawnCountGrowthInterval > 0f ? Mathf.FloorToInt(_elapsedTime / _maxSpawnCountGrowthInterval) : 0;
            return Mathf.Max(minVal, _initialMaxSpawnCount + maxGrowth);
        }
    }

    // Lifecycle

    private void Update()
    {
        if (!_isActive) return;
        _elapsedTime += Time.deltaTime;

        if (_mode == DifficultyMode.Steps) UpdateSteps();
        if (_mode == DifficultyMode.Wave)  UpdateWave();

#if UNITY_EDITOR
        if (_showDebugValues)
        {
            _debugSpawnInterval  = CurrentSpawnInterval;
            HazardTiming t       = CurrentTiming;
            _debugShadowDuration = t.shadowDuration;
            _debugWarningFlash   = t.warningFlashDuration;
            _debugCurrentWave    = ActiveWave != null
                ? $"[{_currentWaveIndex}] {ActiveWave.waveName} ({_waveTimer:F1}s left)"
                : _mode.ToString();
        }
#endif
    }

    private void UpdateSteps()
    {
        int newStep = Mathf.FloorToInt(_elapsedTime / _stepInterval);
        if (newStep <= _currentStep) return;
        int steps = newStep - _currentStep;
        _currentStep = newStep;
        _stepSpawnInterval  = Mathf.Max(_minSpawnInterval,  _stepSpawnInterval  - _spawnIntervalStepAmount  * steps);
        _stepShadowDuration = Mathf.Max(_minShadowDuration, _stepShadowDuration - _shadowDurationStepAmount * steps);
        _stepMinHazards    += _minHazardsStepAmount * steps;
        if (_stepMaxHazards > 0 || _maxHazardsStepAmount > 0)
            _stepMaxHazards = Mathf.Max(_stepMinHazards, _stepMaxHazards + _maxHazardsStepAmount * steps);
        _stepMinSpawnCount = Mathf.Max(1, _stepMinSpawnCount + _minSpawnCountStepAmount * steps);
        _stepMaxSpawnCount = Mathf.Max(_stepMinSpawnCount, _stepMaxSpawnCount + _maxSpawnCountStepAmount * steps);
        OnStepAdvanced?.Invoke(_currentStep);
    }

    private void UpdateWave()
    {
        if (_waves == null || _waves.Length == 0) return;
        _waveTimer -= Time.deltaTime;
        if (_waveTimer > 0f) return;
        _currentWaveIndex = PickNextWaveIndex();
        _waveTimer = ActiveWave != null
            ? Random.Range(ActiveWave.minDuration, ActiveWave.maxDuration)
            : 5f;
        OnWaveChanged?.Invoke(_currentWaveIndex);
    }

    private int PickNextWaveIndex()
    {
        if (_waves == null || _waves.Length == 0) return 0;

        // Sequential is inherently non-repeating - just advance.
        if (_wavePattern == WavePatternMode.Sequential)
            return (_currentWaveIndex + 1) % _waves.Length;

        // For Random and Weighted, try up to 5 times to avoid picking the same wave twice.
        // With only 1 wave configured there's no alternative, so the guard is skipped.
        int next = _currentWaveIndex;
        int attempts = 0;
        int maxAttempts = _waves.Length > 1 ? 5 : 1;
        while (next == _currentWaveIndex && attempts < maxAttempts)
        {
            next = _wavePattern == WavePatternMode.Weighted && _waveWeightedIndices.Count > 0
                ? _waveWeightedIndices[Random.Range(0, _waveWeightedIndices.Count)]
                : Random.Range(0, _waves.Length);
            attempts++;
        }
        return next;
    }

    public void ResetDifficulty()
    {
        _elapsedTime = 0f;
        _currentStep = 0;
        _stepSpawnInterval  = _initialSpawnInterval;
        _stepShadowDuration = _initialShadowDuration;
        _stepMinHazards     = _initialMinHazards;
        _stepMaxHazards     = _initialMaxHazards;
        _stepMinSpawnCount  = _initialMinSpawnCount;
        _stepMaxSpawnCount  = _initialMaxSpawnCount;

        // Build weighted index list
        _waveWeightedIndices.Clear();
        if (_waves != null)
        {
            for (int i = 0; i < _waves.Length; i++)
            {
                int w = _waves[i] != null ? Mathf.Max(1, _waves[i].weight) : 1;
                for (int j = 0; j < w; j++)
                    _waveWeightedIndices.Add(i);
            }
        }

        // Pick starting wave
        if (_mode == DifficultyMode.Wave && (_waves == null || _waves.Length == 0))
            Debug.LogError("[DifficultyManager] Mode is set to Wave but no waves are configured. Add at least one WaveEntry in the Inspector.");

        if (_mode == DifficultyMode.Wave && _waves != null && _waves.Length > 0)
        {
            if (_startingWaveIndex >= 0 && _startingWaveIndex < _waves.Length)
                _currentWaveIndex = _startingWaveIndex;
            else
                _currentWaveIndex = _wavePattern == WavePatternMode.Sequential ? 0 : PickNextWaveIndex();

            _waveTimer = ActiveWave != null
                ? Random.Range(ActiveWave.minDuration, ActiveWave.maxDuration)
                : 5f;
        }

        _isActive = true;
    }

    public void StopDifficulty()
    {
        _isActive = false;
    }
}
}
