using System.Collections;
using UnityEngine;

namespace NeonBlack.Gameplay.Presentation.Camera
{
/// <summary>
/// Attach to the Main Camera. Provides a lightweight positional shake callable from anywhere.
///
/// Setup: Add this component to your Main Camera GameObject.
/// Usage: CameraShaker.Instance?.Shake(duration, magnitude)
///
/// Shake interrupts itself Ã¢â‚¬â€ calling Shake() while already shaking restarts from the original rest
/// position so the camera never drifts.
/// </summary>
[DefaultExecutionOrder(-10)]
public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance { get; private set; }

    private Coroutine _shakeCoroutine;
    private Vector3   _restPosition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        _restPosition = transform.localPosition;
    }

    /// <summary>
    /// Shake the camera. Interrupts any shake already in progress.
    /// </summary>
    /// <param name="duration">Seconds the shake lasts.</param>
    /// <param name="magnitude">Peak displacement in world units at the start of the shake.</param>
    public void Shake(float duration, float magnitude)
    {
        if (duration <= 0f || magnitude <= 0f) return;
        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
            transform.localPosition = _restPosition;
        }
        _shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float dampened = magnitude * (1f - elapsed / duration);
            transform.localPosition = _restPosition + (Vector3)Random.insideUnitCircle * dampened;
            yield return null;
        }
        transform.localPosition = _restPosition;
        _shakeCoroutine = null;
    }
}
}
