using System.Collections;
using System.Collections.Generic;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Features.Combat;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Hazards
{
/// <summary>
/// Controls a single hazard's visual lifecycle and movement.
///
/// BEHAVIOUR (set via HazardData.hazardType):
///   Slam     -> Shadow -> Warning -> Slam -> Retract -> Pool
///   Crossing -> Lane warning -> Straight or sinusoidal travel -> Pool
///   Bouncy   -> Lane warning -> Wall-bouncing travel -> Pool
///
/// TARGETING MODIFIER (HazardData.enableTargeting - works with every type):
///   Slam             : shadow drifts toward the player during approach + warning.
///   Crossing / Bouncy: steers toward the player each frame during travel.
///
/// CROSSING VARIANT (HazardData.crossingVariant = Jump):
///   All crossing-style types hop in arcs rather than sliding smoothly.
///
/// -- PREFAB SETUP --------------------------------------------------------------
///  Root GameObject
///    +- Hazard.cs          (this script)
///    +- Collider2D(s)      any type + count -> wire ALL into _hitColliders list
///    +- ShadowSprite       child SpriteRenderer -> _shadowRenderer
///    +- OutlineSprite      child SpriteRenderer -> _outlineRenderer  (scale 1.08, order-1)
///    +- LaneSprite         child SpriteRenderer -> _laneRenderer     (white square, inactive)
///    +- ExplosionEffect    child GameObject    -> _explosionEffect   (inactive; Explosive only)
///         +- SpriteRenderer    (explosion art)
///         +- Collider2D(s)     (explosion hitbox, Is Trigger)
///
/// _hitColliders accepts any number of Collider2D components (Box, Circle, Capsule, Polygon)
/// from the root OR from child GameObjects. All are enabled / disabled together as a group.
/// </summary>
[AddComponentMenu("NeonBlack/Gameplay/Hazards/2D Hazard")]
[AuthoringContract(
    Capability = AuthoringCapability.Combat,
    Relevance = "Primary controller for 2D hazards, handling movement, targeting, and impact sequences.",
    Axioms = AuthoringWorldAxiom.Dimensions2D,
    NativeSetup = new[]
    {
        "Attach Hazard script to a GameObject.",
        "Wire child SpriteRenderers for Shadow, Outline, and Lane.",
        "Add Collider2Ds to the Hit Colliders list.",
        "Assign a HazardData ScriptableObject."
    },
    FirstProof = "Place a hazard in the scene and verify it executes its sequence (Slam, Crossing, etc.) on start.",
    AssignmentFields = new[] { "_data", "_hitColliders", "_shadowRenderer" }
)]
public partial class Hazard : MonoBehaviour
{
    [Header("Child Renderers")]
    [SerializeField] private SpriteRenderer _shadowRenderer;
    [SerializeField] private SpriteRenderer _outlineRenderer;
    [SerializeField, Tooltip("White-square sprite child used for the crossing lane warning band.")]
    private SpriteRenderer _laneRenderer;

    [Header("Hitboxes")]
    [SerializeField, Tooltip("Every Collider2D that should be active during the hazard's hit phase.\n" +
        "Supports any count and any Collider2D type: Box, Circle, Capsule, Polygon.\n" +
        "Drag components from the root or any child into this list.")]
    private List<Collider2D> _hitColliders = new List<Collider2D>();

    [Header("Explosion Effect")]
    [SerializeField, Tooltip("(Explosive only) Child GameObject with the explosion sprite and " +
        "its own Collider2D(s). Keep inactive in the prefab; it is activated at detonation.")]
    private GameObject _explosionEffect;

    [Header("Data")]
    [SerializeField] private HazardData _data;

    [Header("Feedback Services")]
    [SerializeField] private MonoBehaviour _cameraShakeSink;
    [SerializeField, Tooltip("Optional settings service used to scale hazard SFX volume. SettingsManager implements IGameplaySettingsApplier.")]
    private MonoBehaviour _settingsSource;

    [Header("Collectible Detection")]
    [SerializeField, Tooltip("Physics layer(s) containing collectible colliders.\n" +
        "MUST be assigned. Set this to the layer used by your collectible prefabs.\n" +
        "Leaving this unset (Nothing) means no collectibles will be removed by hazard impacts.")]
    private LayerMask _collectibleLayer = 0; // 0 = Nothing. Assign the collectible layer in the Inspector.

    [Header("Visual Settings")]
    [SerializeField, Tooltip("Shadow sprite alpha during the approach phase.")]
    private float _shadowAlpha  = 0.25f;
    [SerializeField, Tooltip("Shadow sprite alpha during the warning/flash phase.")]
    private float _warningAlpha = 0.55f;

    // -- Runtime ----------------------------------------------------------
    private HazardSpawner _spawner;
    private Coroutine     _sequenceCoroutine;
    private bool          _explosionTriggered;
    private bool          _pendingImpactExplosion;
    private bool          _isSplitChild;  // true on hazards spawned by SpawnBounceChildren; prevents recursive splits
    private Vector3       _prefabScale; // cached at Awake; used to reset scale cleanly on pool return
    private Vector2       _cachedHitSz; // cached at Awake (colliders are in default enabled state from prefab) so ShowLaneRenderer works correctly
    private AudioSource   _audioSource;
    private HazardFeedbackRuntime _feedbackRuntime;
    private ICameraBoundsProvider _cameraBoundsProvider;
    private IHazardOutcomeSink _hazardOutcomeSink;
    private IPickupBurstSpawnSurface _pickupBurstSpawnSurface;
    private ICameraShakeSink _resolvedCameraShakeSink;
    private IGameplaySettingsApplier _settings;
    // Bounce pattern per-activation state (re-initialised at the start of TravelBouncy each spawn)
    private bool          _zigzagFlipNext;   // Zigzag: true = next turn is left (+), false = right (-)
    private bool          _orbitClockwise;   // Orbit: direction the hazard circles the arena

    private readonly Collider2D[] _collectibleBuffer = new Collider2D[32];
    // Throttle for per-frame collectible sweep calls during travel.
    // Physics2D.OverlapCircle every frame * N hazards = major lag spike at high difficulty.
    private float _collectibleSweepTimer;
    private bool _collectibleLayerWarningLogged;
    // Cached WaitForSeconds reused each activation to avoid per-spawn heap allocations.
    // Value is set just before use since shadow duration changes each spawn.
    private WaitForSeconds _cachedWait;
    // Throttle counter for warning-flash outline alpha; only update ~20 times/sec
    // instead of every frame. Each SetOutlineAlpha call dirties the SpriteRenderer
    // and breaks sprite batching, which is expensive on mobile.
    private float _outlineAlphaTimer;
    private bool _loggedFeedbackValidationIssues;

    private void Awake()
    {
        _prefabScale = transform.localScale;
        // Cache collider size while colliders are in their prefab-default enabled state.
        // PolygonCollider2D.bounds.size returns zero when the collider is disabled,
        // so we must read this before any DisableAllColliders() call ever runs.
        _cachedHitSz = GetPrimaryHitColliderSize();

        // Get or create a 2D (non-spatial) AudioSource so sounds have equal volume
        // regardless of the hazard's world position (correct for an orthographic 2D game).
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.spatialBlend = 0f;   // 2D audio with no distance attenuation
        _audioSource.playOnAwake  = false;

        _feedbackRuntime = GetComponent<HazardFeedbackRuntime>() ?? GetComponentInChildren<HazardFeedbackRuntime>(true);
        _feedbackRuntime?.ApplyProfile(_data != null ? _data.feedbackProfile : null);
        _resolvedCameraShakeSink = ResolveCameraShakeSink();
    }

    /// <summary>Resolves the nearest active participant when available, otherwise falls back to the registered IPlayerProvider.</summary>
    private Transform Player => ParticipantQueryUtility.TryGetClosestParticipantTransform(transform.position, out Transform participant, out _)
        ? participant
        : (ParticipantQueryUtility.TryResolvePlayerProvider(out IPlayerProvider provider) ? provider.GetPlayerTransform() : null);

    public HazardData Data => _data;

    public void ConfigureRuntime(
        ICameraBoundsProvider cameraBoundsProvider,
        IHazardOutcomeSink hazardOutcomeSink,
        IPickupBurstSpawnSurface pickupBurstSpawnSurface)
    {
        if (cameraBoundsProvider != null)
            _cameraBoundsProvider = cameraBoundsProvider;
        if (hazardOutcomeSink != null)
            _hazardOutcomeSink = hazardOutcomeSink;
        if (pickupBurstSpawnSurface != null)
            _pickupBurstSpawnSurface = pickupBurstSpawnSurface;
    }

    public void SetCameraShakeSink(ICameraShakeSink sink)
    {
        _resolvedCameraShakeSink = sink;
        _cameraShakeSink = sink as MonoBehaviour;
    }

    public void SetSettings(IGameplaySettingsApplier settings)
    {
        _settings = settings;
        _settingsSource = settings as MonoBehaviour;
    }

    /// <summary>Set by HazardSpawner before Initialize for crossing-style hazards.</summary>
    [System.NonSerialized] public Vector2 CrossingStart;
    [System.NonSerialized] public Vector2 CrossingEnd;
    /// <summary>Set by SpawnBounceChildren to give split children a pre-determined initial direction.
    /// Consumed and cleared on the first frame of TravelBouncy.</summary>
    private Vector2? _bouncyDirOverride;

    // ---------------------------------------------------------------------
    // Public API
    // ---------------------------------------------------------------------

    public void Initialize(HazardSpawner spawner, DifficultyManager.HazardTiming timing)
    {
        if (_data == null)
        {
            Debug.LogError($"[Hazard] '{name}' has no HazardData assigned.", this);
            gameObject.SetActive(false);
            return;
        }

        _collectibleLayerWarningLogged = false;

        // -- Renderer health checks -----------------------------------------
        // These fire at spawn time so you see the exact prefab that is misconfigured.
        if (_shadowRenderer == null)
        {
            Debug.LogError($"[Hazard] '{name}': _shadowRenderer is not wired in the Inspector. "
                         + "Drag the shadow SpriteRenderer child into the _shadowRenderer slot.", this);
            gameObject.SetActive(false);
            return;
        }
        if (!_shadowRenderer.gameObject.activeSelf)
            Debug.LogWarning($"[Hazard] '{name}': _shadowRenderer's GameObject is inactive. "
                           + "Ensure the shadow child is active in the prefab.", this);
        if (_data.shadowSprite == null)
            Debug.LogWarning($"[Hazard] '{name}': HazardData '{_data.hazardName}' has no shadowSprite assigned. "
                           + "The approach phase will be invisible.", this);
        if (_data.fullyFormedSprite == null)
            Debug.LogWarning($"[Hazard] '{name}': HazardData '{_data.hazardName}' has no fullyFormedSprite assigned. "
                           + "The active phase will be invisible.", this);
        if (_outlineRenderer != null && _outlineRenderer.gameObject == _shadowRenderer.gameObject)
            Debug.LogError($"[Hazard] '{name}': _outlineRenderer and _shadowRenderer are on the SAME child "
                         + "GameObject. SetOutlineActive(false) will hide the shadow renderer too. "
                         + "Move the outline SpriteRenderer to its own separate child GameObject.", this);
        if (_hitColliders == null || _hitColliders.Count == 0)
            Debug.LogWarning($"[Hazard] '{name}': _hitColliders is empty; add Collider2D component(s) and wire them in.", this);
        if (_data.destroysNearbyCollectibles && _collectibleLayer.value == 0)
            LogMissingCollectibleLayer();

        // -- Explosion config health checks ---------------------------------
        if (_data.enableExplosion)
        {
            if (_explosionEffect == null)
                Debug.LogWarning($"[Hazard] '{name}': HazardData '{_data.hazardName}' has enableExplosion=true " +
                                 "but no _explosionEffect child is wired. Add a child GO with a SpriteRenderer " +
                                 "and Collider2D(s), keep it inactive in the prefab, then wire it into _explosionEffect.", this);

            // A Kinematic Rigidbody2D on the root is REQUIRED for the explosion child's
            // Collider2D(s) to route OnTriggerEnter2D events to this script.
            // Without it, the explosion hitbox fires into the void and never kills the player.
            if (GetComponent<Rigidbody2D>() == null)
                Debug.LogError($"[Hazard] '{name}': enableExplosion requires a Kinematic Rigidbody2D on the " +
                               "root GameObject so the explosion child's colliders route trigger events to " +
                               "this script. Add a Rigidbody2D (Body Type: Kinematic, Gravity Scale: 0, " +
                               "Simulated: true) to this prefab's root.", this);
        }
        // ------------------------------------------------------------------

        // -- Lane renderer health check (Crossing only) ---------------------
        if (_data.hazardType == HazardData.HazardType.Crossing)
        {
            if (_laneRenderer == null)
                Debug.LogWarning($"[Hazard] '{name}': HazardType is Crossing but _laneRenderer is not wired. " +
                                 "Drag the LaneSprite child SpriteRenderer into the _laneRenderer slot.", this);
            else if (_laneRenderer.sprite == null)
                Debug.LogWarning($"[Hazard] '{name}': _laneRenderer is wired but has no sprite assigned. " +
                                 "Assign a sprite to the LaneSprite SpriteRenderer in the prefab.", this);
        }
        // ------------------------------------------------------------------

        _spawner = spawner;
        _feedbackRuntime?.ApplyProfile(_data.feedbackProfile);
        LogFeedbackValidationIssues();

        // Reset all runtime state so re-pooled hazards start clean.
        _pendingImpactExplosion = false;
        _explosionTriggered     = false;
        if (_shadowRenderer != null) _shadowRenderer.color = new Color(1f, 1f, 1f, 0f);
        SetOutlineActive(false);
        HideLaneRenderer();
        DisableAllColliders();
        if (_explosionEffect != null) _explosionEffect.SetActive(false);
        if (_sequenceCoroutine != null) StopCoroutine(_sequenceCoroutine);

        switch (_data.hazardType)
        {
            case HazardData.HazardType.Crossing:
                _sequenceCoroutine = StartCoroutine(CrossingSequenceRoutine(timing));
                break;
            case HazardData.HazardType.Bouncy:
                _sequenceCoroutine = StartCoroutine(BouncySequenceRoutine(timing));
                break;
            default:
                _sequenceCoroutine = StartCoroutine(SlamSequenceRoutine(timing));
                break;
        }
    }

    public void ForceStop()
    {
        if (_sequenceCoroutine != null) { StopCoroutine(_sequenceCoroutine); _sequenceCoroutine = null; }
        ResetState();
    }

    /// <summary>Called by SpawnBounceChildren so the split child uses the pre-calculated direction
    /// instead of picking one from its pattern.</summary>
    public void SetBouncyDirectionOverride(Vector2 dir)
    {
        _bouncyDirOverride = dir.normalized;
        _isSplitChild      = true; // prevents this child from splitting again
    }

    // ---------------------------------------------------------------------
    // Collider helpers
    // ---------------------------------------------------------------------

    private void EnableHitColliders()
    {
        if (_hitColliders == null) return;
        foreach (var c in _hitColliders)
            if (c != null) c.enabled = true;
    }

    private void DisableAllColliders()
    {
        if (_hitColliders == null) return;
        foreach (var c in _hitColliders)
            if (c != null) c.enabled = false;
    }

    /// <summary>
    /// Returns the size of the first wired hit collider so the lane renderer can match it.
    /// Works in edit mode (no Bounds query needed).
    /// </summary>
    private Vector2 GetPrimaryHitColliderSize()
    {
        if (_hitColliders == null || _hitColliders.Count == 0 || _hitColliders[0] == null)
            return Vector2.one;
        Collider2D c = _hitColliders[0];
        if (c is BoxCollider2D     box)    return box.size;
        if (c is CircleCollider2D  circle) return Vector2.one * (circle.radius * 2f);
        if (c is CapsuleCollider2D cap)    return cap.size;
        return c.bounds.size; // Polygon or unknown
    }


    private void ResetState()
    {
        _pendingImpactExplosion = false;
        _explosionTriggered     = false;
        _isSplitChild           = false;
        _loggedFeedbackValidationIssues = false;
        _collectibleSweepTimer  = 0f;
        _outlineAlphaTimer      = 0f;
        StopTravelLoop();
        if (_shadowRenderer != null) _shadowRenderer.color = new Color(1f, 1f, 1f, 0f);
        SetOutlineActive(false);
        HideLaneRenderer();
        DisableAllColliders();
        if (_explosionEffect != null) _explosionEffect.SetActive(false);
        transform.rotation   = Quaternion.identity;
        transform.localScale = _prefabScale;
    }

    private void ReturnToPool()
    {
        ResetState();
        if (_spawner != null) _spawner.ReturnToPool(this);
        else gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsActorTarget(other))
        {
            // Dash i-frames: a dashing player is invulnerable.
            Motor2D cc = other.GetComponent<Motor2D>();
            if (cc == null)
                cc = other.GetComponentInParent<Motor2D>();
            if (cc != null && cc.IsDashing) return;

            bool appliedImpact = _data != null
                && _data.impactProfile != null
                && HazardImpactUtility.TryApplyImpact(other.gameObject, _data.impactProfile, gameObject, other.transform.position);

            if (!appliedImpact)
                _hazardOutcomeSink?.TryHandleHazardImpact(other.gameObject, gameObject, other.transform.position);
        }

        if (_data != null && _data.enableExplosion
            && _data.explosionTrigger == HazardData.ExplosionTrigger.OnImpact
            && (IsActorTarget(other) || other.TryGetComponent<Hazard>(out _)))
            _pendingImpactExplosion = true;
    }

    private static bool IsActorTarget(Collider2D other)
    {
        return other != null
            && (other.GetComponentInParent<Motor2D>() != null
                || other.GetComponentInParent<HealthComponent>() != null);
    }

    private void LogFeedbackValidationIssues()
    {
        if (_loggedFeedbackValidationIssues || _data == null || _data.feedbackProfile == null)
            return;

        _loggedFeedbackValidationIssues = true;

        if (_feedbackRuntime == null)
        {
            Debug.LogWarning(
                $"[Hazard] '{name}' uses HazardFeedbackProfile '{_data.feedbackProfile.name}' but no HazardFeedbackRuntime is present on the prefab.",
                this);
            return;
        }

        if (_feedbackRuntime is IRuntimeValidationProvider provider)
        {
            foreach (string issue in provider.GetRuntimeValidationIssues())
            {
                if (!string.IsNullOrWhiteSpace(issue))
                    Debug.LogWarning($"[Hazard] '{name}': {issue}", this);
            }
        }
    }

    // -- Visual helpers ---------------------------------------------------

    private void SetShadowSprite(Sprite s)  { if (_shadowRenderer != null) _shadowRenderer.sprite = s; }

    private void ApplyActiveTint()
    {
        if (_shadowRenderer == null) return;
        Color c = _data.tintColor;
        c.a = _shadowRenderer.color.a; // preserve current alpha
        _shadowRenderer.color = c;
    }

    private void SetShadowAlpha(float a)
    {
        if (_shadowRenderer == null) return;
        Color c = _shadowRenderer.color; c.a = a; _shadowRenderer.color = c;
    }

    private void SetOutlineActive(bool on) { if (_outlineRenderer != null) _outlineRenderer.gameObject.SetActive(on); }

    private void SetOutlineAlpha(float a)
    {
        if (_outlineRenderer == null) return;
        Color c = _outlineRenderer.color; c.a = a; _outlineRenderer.color = c;
    }

    private void SetOutlineSprite(Sprite s, Color col)
    {
        if (_outlineRenderer == null) return;
        _outlineRenderer.sprite = s;
        Color c = col; c.a = _outlineRenderer.color.a; _outlineRenderer.color = c;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // -- Crossing path (visible during play mode when Start/End are set) --
        if (CrossingStart != CrossingEnd)
        {
            UnityEditor.Handles.color = new Color(1f, 0.85f, 0f, 0.9f);
            UnityEditor.Handles.DrawLine(CrossingStart, CrossingEnd);
            UnityEditor.Handles.DrawSolidDisc(CrossingStart, Vector3.forward, 0.12f);
            UnityEditor.Handles.DrawSolidDisc(CrossingEnd,   Vector3.forward, 0.12f);
        }

        if (_data == null) return;

        // -- Explosion proximity radius ------------------------------------
        if (_data.enableExplosion && _data.explosionTrigger == HazardData.ExplosionTrigger.OnProximity)
        {
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.15f);
            Gizmos.DrawSphere(transform.position, _data.explosionProximityRadius);
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.85f);
            Gizmos.DrawWireSphere(transform.position, _data.explosionProximityRadius);
        }

        // -- Targeting lock-on radius -------------------------------------
        if (_data.enableTargeting && _data.lockOnRadius > 0f)
        {
            Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, _data.lockOnRadius);
        }

        // -- Crumb destroy radius -----------------------------------------
        if (_data.destroysNearbyCollectibles)
        {
            Vector2 sz   = GetPrimaryHitColliderSize();
            float radius = Mathf.Max(sz.x, sz.y) * 0.5f * _data.collectibleDestroyRadiusScale;
            Gizmos.color = new Color(0.6f, 0.4f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
#endif
}
}
