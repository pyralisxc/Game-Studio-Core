using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Presentation.Animation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeonBlack.Gameplay.Presentation.Visuals
{
/// <summary>
/// Coroutine-driven color flash effects on one or more SpriteRenderers.
/// Supports Pulse, Strobe, Blink, and ColorCycle modes via FlashPresetSO assets.
/// A single component works on hazards, players, UI sprites, backgrounds, and other 2D visuals.
/// </summary>
[AuthoringContract(
    Capability = AuthoringCapability.VFX,
    Relevance = "Coroutine-driven color flash effects on SpriteRenderers.",
    SupportedLanes = new[] { ActorPresentationMode.Sprite2D, ActorPresentationMode.Billboard2_5D },
    AssignmentFields = new[] { "_renderers", "_defaultPreset", "_playOnStart" },
    FirstProof = "Assign a FlashPresetSO and call Play() from a script or UnityEvent.",
    NativeSetup = new[]
    {
        "Add SpriteFlasher to an actor or object prefab.",
        "Enable Auto Find Renderers or assign targets manually.",
        "Assign a FlashPresetSO for common effects (Hit, Flash)."
    },
    ExpertAdvice = "Use SpriteFlasher for hit reactions and status effects. For best performance, group multiple renderers into one flasher if they should flash in sync.",
    DocumentationURL = "https://docs.neonblack.com/pyralis/visuals"
)]
public class SpriteFlasher : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField, Tooltip("SpriteRenderers to flash. Leave empty and enable Auto Find to collect them automatically.")]
    private List<SpriteRenderer> _renderers = new List<SpriteRenderer>();
    [SerializeField, Tooltip("If true and _renderers is empty, finds all SpriteRenderers on this GameObject and its children on Awake.")]
    private bool _autoFindRenderers = true;

    [Header("Default Preset")]
    [SerializeField, Tooltip("Preset used when Play() is called with no argument, and when Play On Start is enabled.")]
    private FlashPresetSO _defaultPreset;
    [SerializeField, Tooltip("If true, plays the default preset automatically from Start.")]
    private bool _playOnStart;

    [Header("Events")]
    [SerializeField, Tooltip("Fired when a finite effect finishes and colors are fully restored.")]
    private UnityEvent _onFlashComplete;

    private Coroutine _routine;
    private Color[] _originalColors;
    private bool _initialized;

    private void Awake() => Initialize();

    private void Start()
    {
        if (_playOnStart && _defaultPreset != null)
            Play(_defaultPreset);
    }

    private void OnDisable()
    {
        Stop();
        _initialized = false;
    }

    public void Play() => Play(_defaultPreset);

    public void Play(FlashPresetSO preset)
    {
        if (preset == null)
        {
            Debug.LogWarning("[SpriteFlasher] Play called with no preset.", this);
            return;
        }

        Stop();
        if (!_initialized)
            Initialize();

        int loops = preset.loopCount < 0 ? -1 : Mathf.Max(1, preset.loopCount);
        _routine = StartCoroutine(FlashRoutine(preset, loops));
    }

    public void PlayOneShot(FlashPresetSO preset)
    {
        if (preset == null)
            return;

        Stop();
        if (!_initialized)
            Initialize();

        _routine = StartCoroutine(FlashRoutine(preset, 1));
    }

    public void Stop()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }

        RestoreOriginalColors();
    }

    private void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;

        if (_autoFindRenderers && _renderers.Count == 0)
            _renderers.AddRange(GetComponentsInChildren<SpriteRenderer>(true));

        CacheOriginalColors();
    }

    private void CacheOriginalColors()
    {
        _originalColors = new Color[_renderers.Count];
        for (int i = 0; i < _renderers.Count; i++)
            _originalColors[i] = _renderers[i] != null ? _renderers[i].color : Color.white;
    }

    private IEnumerator FlashRoutine(FlashPresetSO preset, int loops)
    {
        Color[] baseColors = CaptureBaseColors(preset);
        int played = 0;

        while (loops < 0 || played < loops)
        {
            switch (preset.mode)
            {
                case FlashPresetSO.FlashMode.Pulse:
                    yield return PulseRoutine(preset, baseColors);
                    break;
                case FlashPresetSO.FlashMode.Strobe:
                    yield return StrobeRoutine(preset, baseColors);
                    break;
                case FlashPresetSO.FlashMode.Blink:
                    yield return BlinkRoutine(preset, baseColors);
                    break;
                case FlashPresetSO.FlashMode.ColorCycle:
                    yield return ColorCycleRoutine(preset, baseColors);
                    break;
            }

            if (loops > 0)
                played++;

            bool moreLoops = loops < 0 || played < loops;
            if (moreLoops && preset.cycleDelay > 0f)
            {
                if (preset.mode != FlashPresetSO.FlashMode.ColorCycle)
                    RestoreToCapture(baseColors);
                yield return new WaitForSeconds(preset.cycleDelay);
            }
        }

        RestoreToCapture(baseColors);
        _onFlashComplete?.Invoke();
    }

    private IEnumerator PulseRoutine(FlashPresetSO preset, Color[] baseColors)
    {
        Color flash = WithAlpha(preset.flashColor, preset.overrideAlpha, preset.flashAlpha, baseColors);
        yield return LerpAllRoutine(baseColors, flash, preset.flashDuration, preset.easeIn);
        yield return LerpAllRoutine(flash, baseColors, preset.flashDuration, preset.easeOut);
    }

    private IEnumerator StrobeRoutine(FlashPresetSO preset, Color[] baseColors)
    {
        Color flash = WithAlpha(preset.flashColor, preset.overrideAlpha, preset.flashAlpha, baseColors);
        SetAll(flash);
        yield return new WaitForSeconds(preset.flashDuration);
        RestoreToCapture(baseColors);
        if (preset.interval > 0f)
            yield return new WaitForSeconds(preset.interval);
    }

    private IEnumerator BlinkRoutine(FlashPresetSO preset, Color[] baseColors)
    {
        float fadeTime = preset.flashDuration * 0.25f;
        float holdTime = preset.flashDuration * 0.50f;
        Color flash = WithAlpha(preset.flashColor, preset.overrideAlpha, preset.flashAlpha, baseColors);

        yield return LerpAllRoutine(baseColors, flash, fadeTime, preset.easeIn);
        yield return new WaitForSeconds(holdTime);
        yield return LerpAllRoutine(flash, baseColors, fadeTime, preset.easeOut);

        if (preset.interval > 0f)
            yield return new WaitForSeconds(preset.interval);
    }

    private IEnumerator ColorCycleRoutine(FlashPresetSO preset, Color[] baseColors)
    {
        if (preset.cycleColors == null || preset.cycleColors.Length == 0)
        {
            Debug.LogWarning("[SpriteFlasher] ColorCycle mode selected but cycleColors is empty.", this);
            yield break;
        }

        foreach (Color color in preset.cycleColors)
        {
            Color stepped = preset.overrideAlpha ? new Color(color.r, color.g, color.b, preset.flashAlpha) : color;
            SetAll(stepped);
            yield return new WaitForSeconds(preset.flashDuration);
        }
    }

    private IEnumerator LerpAllRoutine(Color[] from, Color to, float duration, FlashPresetSO.FlashEase ease)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float e = ApplyEase(ease, Mathf.Clamp01(elapsed / duration));
            for (int i = 0; i < _renderers.Count; i++)
                if (_renderers[i] != null)
                    _renderers[i].color = Color.LerpUnclamped(from[i], to, e);
            yield return null;
        }

        SetAll(to);
    }

    private IEnumerator LerpAllRoutine(Color from, Color[] to, float duration, FlashPresetSO.FlashEase ease)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float e = ApplyEase(ease, Mathf.Clamp01(elapsed / duration));
            for (int i = 0; i < _renderers.Count; i++)
                if (_renderers[i] != null)
                    _renderers[i].color = Color.LerpUnclamped(from, to[i], e);
            yield return null;
        }

        RestoreToCapture(to);
    }

    private Color[] CaptureBaseColors(FlashPresetSO preset)
    {
        Color[] colors = new Color[_renderers.Count];
        for (int i = 0; i < _renderers.Count; i++)
        {
            if (_renderers[i] == null)
            {
                colors[i] = Color.white;
                continue;
            }

            colors[i] = preset.useRendererColorAsBase ? _renderers[i].color : preset.baseColor;
            if (preset.overrideAlpha)
                colors[i].a = preset.baseAlpha;
        }

        return colors;
    }

    private void SetAll(Color color)
    {
        foreach (SpriteRenderer renderer in _renderers)
            if (renderer != null)
                renderer.color = color;
    }

    private void RestoreToCapture(Color[] baseColors)
    {
        for (int i = 0; i < _renderers.Count && i < baseColors.Length; i++)
            if (_renderers[i] != null)
                _renderers[i].color = baseColors[i];
    }

    private void RestoreOriginalColors()
    {
        if (_originalColors == null)
            return;

        for (int i = 0; i < _renderers.Count && i < _originalColors.Length; i++)
            if (_renderers[i] != null)
                _renderers[i].color = _originalColors[i];
    }

    private static Color WithAlpha(Color color, bool applyOverride, float overrideAlpha, Color[] baseColors)
    {
        color.a = applyOverride ? overrideAlpha : (baseColors.Length > 0 ? baseColors[0].a : 1f);
        return color;
    }

    private static float ApplyEase(FlashPresetSO.FlashEase ease, float t)
    {
        switch (ease)
        {
            case FlashPresetSO.FlashEase.Linear:
                return t;
            case FlashPresetSO.FlashEase.InSine:
                return 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
            case FlashPresetSO.FlashEase.OutSine:
                return Mathf.Sin(t * Mathf.PI * 0.5f);
            case FlashPresetSO.FlashEase.InOutSine:
                return -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;
            case FlashPresetSO.FlashEase.InQuad:
                return t * t;
            case FlashPresetSO.FlashEase.OutQuad:
                return 1f - (1f - t) * (1f - t);
            case FlashPresetSO.FlashEase.InOutQuad:
                return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) * 0.5f;
            case FlashPresetSO.FlashEase.InCubic:
                return t * t * t;
            case FlashPresetSO.FlashEase.OutCubic:
                return 1f - Mathf.Pow(1f - t, 3f);
            default:
                return Mathf.SmoothStep(0f, 1f, t);
        }
    }
}
}
