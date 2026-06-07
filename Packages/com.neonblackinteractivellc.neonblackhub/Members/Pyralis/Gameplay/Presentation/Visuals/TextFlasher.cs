using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace NeonBlack.Gameplay.Presentation.Visuals
{
/// <summary>
/// Coroutine-driven color flash effects on one or more TMP_Text components.
/// Reuses FlashPresetSO assets so the same preset works on both SpriteFlasher and TextFlasher.
/// Drives TMP_Text.color directly, which reflects alpha, works with standard SDF materials,
/// and avoids material-instance mutation issues from driving _FaceColor directly.
/// </summary>
public class TextFlasher : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField, Tooltip("TMP_Text components to flash. Leave empty and enable Auto Find to collect them automatically.")]
    private List<TMP_Text> _texts = new List<TMP_Text>();
    [SerializeField, Tooltip("If true and _texts is empty, finds all TMP_Text components on this GameObject and its children on Awake.")]
    private bool _autoFindTexts = true;

    [Header("Default Preset")]
    [SerializeField, Tooltip("Preset used when Play() is called with no argument, and when Play On Start is enabled.")]
    private FlashPresetSO _defaultPreset;
    [SerializeField, Tooltip("If true, plays the default preset automatically from Start.")]
    private bool _playOnStart = false;

    [Header("Events")]
    [SerializeField, Tooltip("Fired when a finite effect (loopCount >= 1) finishes and colors are fully restored.")]
    private UnityEvent _onFlashComplete;

    private Coroutine _routine;
    private Color[] _originalColors;
    private bool _initialized;
    private bool _hasStarted;

    private static readonly int FaceColorId = Shader.PropertyToID("_FaceColor");

    private void Awake() => Initialize();

    private void Start()
    {
        _hasStarted = true;
        if (_playOnStart && _defaultPreset != null)
            Play(_defaultPreset);
    }

    private void OnEnable()
    {
        if (_hasStarted && _playOnStart && _defaultPreset != null)
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
            Debug.LogWarning("[TextFlasher] Play called with no preset.", this);
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

        if (_autoFindTexts && _texts.Count == 0)
            _texts.AddRange(GetComponentsInChildren<TMP_Text>(true));

        foreach (TMP_Text text in _texts)
        {
            if (text != null && text.fontSharedMaterial != null && text.fontSharedMaterial.HasProperty(FaceColorId))
                text.fontSharedMaterial.SetColor(FaceColorId, Color.white);
        }

        CacheOriginalColors();
    }

    private void CacheOriginalColors()
    {
        _originalColors = new Color[_texts.Count];
        for (int i = 0; i < _texts.Count; i++)
            _originalColors[i] = _texts[i] != null ? _texts[i].color : Color.white;
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
            Debug.LogWarning("[TextFlasher] ColorCycle mode selected but cycleColors is empty.", this);
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
            for (int i = 0; i < _texts.Count; i++)
                if (_texts[i] != null)
                    _texts[i].color = Color.LerpUnclamped(from[i], to, e);
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
            for (int i = 0; i < _texts.Count; i++)
                if (_texts[i] != null)
                    _texts[i].color = Color.LerpUnclamped(from, to[i], e);
            yield return null;
        }

        RestoreToCapture(to);
    }

    private Color[] CaptureBaseColors(FlashPresetSO preset)
    {
        Color[] colors = new Color[_texts.Count];
        for (int i = 0; i < _texts.Count; i++)
        {
            if (_texts[i] == null)
            {
                colors[i] = Color.white;
                continue;
            }

            colors[i] = preset.useRendererColorAsBase ? _texts[i].color : preset.baseColor;
            if (preset.overrideAlpha)
                colors[i].a = preset.baseAlpha;
        }

        return colors;
    }

    private void SetAll(Color color)
    {
        foreach (TMP_Text text in _texts)
            if (text != null)
                text.color = color;
    }

    private void RestoreToCapture(Color[] baseColors)
    {
        for (int i = 0; i < _texts.Count && i < baseColors.Length; i++)
            if (_texts[i] != null)
                _texts[i].color = baseColors[i];
    }

    private void RestoreOriginalColors()
    {
        if (_originalColors == null)
            return;

        for (int i = 0; i < _texts.Count && i < _originalColors.Length; i++)
            if (_texts[i] != null)
                _texts[i].color = _originalColors[i];
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
