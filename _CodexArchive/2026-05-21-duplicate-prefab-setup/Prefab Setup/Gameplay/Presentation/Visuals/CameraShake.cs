using System.Collections;
using UnityEngine;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Presentation.Visuals
{
/// <summary>
/// Applies a decaying positional and rotational shake to the camera (or any Transform).
/// Attach to the CameraRig root GameObject or to the Main Camera.
///
/// Setup:
/// 1. Attach to the CameraRig or Main Camera GameObject.
/// 2. Call CameraShake.Instance.Shake(intensity, duration) from any script on heavy hits,
///    explosions, or landing impacts.
/// </summary>
public class CameraShake : MonoBehaviour, IGameService
{
    public static CameraShake Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Instance = null;

    [Header("Defaults")]
    [Tooltip("Position shake strength multiplier. 0 = no position shake, 1 = full.")]
    [Range(0f, 1f)]
    [SerializeField] private float positionInfluence = 0.5f;

    [Tooltip("Rotation shake strength multiplier. 0 = no rotation shake.")]
    [Range(0f, 1f)]
    [SerializeField] private float rotationInfluence = 0.1f;

    private Vector3    _originalPos;
    private Quaternion _originalRot;
    private bool       _isShaking;
    private float      _currentIntensity;
    private Coroutine  _shakeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Initialize();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Shutdown();
        }
    }

    /// <summary>
    /// Shake the camera with decaying intensity.
    /// </summary>
    /// <param name="intensity">Peak world-unit displacement. 0.15ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Å“0.4 is a good combat range.</param>
    /// <param name="duration">Seconds the shake lasts. 0.1ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Å“0.3 for hits, 0.5+ for explosions.</param>
    public void Shake(float intensity, float duration)
    {
        float scaled = intensity * positionInfluence;

        // Allow a stronger hit to interrupt an ongoing lighter shake.
        // Discard if the new shake is weaker than what's already playing.
        if (_isShaking && scaled <= _currentIntensity) return;

        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
            // Restore to rest so the new shake has a clean origin.
            transform.position = _originalPos;
            transform.rotation = _originalRot;
        }

        _originalPos      = transform.position;
        _originalRot      = transform.rotation;
        _currentIntensity = scaled;

        if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
        _shakeCoroutine = StartCoroutine(ShakeRoutine(scaled, duration));
    }

    private IEnumerator ShakeRoutine(float intensity, float duration)
    {
        _isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // Linearly decay shake from full intensity to zero over the duration.
            float strength = Mathf.Lerp(intensity, 0f, elapsed / duration);

            transform.position = _originalPos + Random.insideUnitSphere * strength;
            transform.rotation = Quaternion.Euler(
                _originalRot.eulerAngles + Random.insideUnitSphere * strength * rotationInfluence * 10f);

            yield return null;
        }

        transform.position = _originalPos;
        transform.rotation = _originalRot;
        _currentIntensity  = 0f;
        _isShaking = false;
    }

    public void Initialize() { }

    public void Shutdown()
    {
        if (_shakeCoroutine != null)
            StopCoroutine(_shakeCoroutine);
    }
}
}
