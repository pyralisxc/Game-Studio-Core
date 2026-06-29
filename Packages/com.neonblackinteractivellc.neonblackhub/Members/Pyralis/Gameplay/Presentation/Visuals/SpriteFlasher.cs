using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Animation;
using NeonBlack.Gameplay.Data.Presentation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Presentation.Visuals
{
/// <summary>
/// Coroutine-driven color flash effects on one or more SpriteRenderers.
/// Supports Pulse, Strobe, Blink, and ColorCycle modes via FlashEffectProfile assets.
/// A single component works on hazards, players, UI sprites, backgrounds, and other 2D visuals.
/// </summary>
[AuthoringContract(
        Category = "V F X",
        CapabilityPath = "Presentation/Feedback/Sprite Flasher",
        Surface = AuthoringSurface.Goal,
        Summary = "Coroutine-driven color flash effects on SpriteRenderers.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/visuals",
        RequiredFields = new[] { "_renderers", "_defaultProfile", "_playOnStart" },
        SetupSteps = new[]
    {
        "Add SpriteFlasher to an actor or object prefab.",
        "Enable Auto Find Renderers or assign targets manually.",
        "Assign a FlashEffectProfile for common effects (Hit, Flash)."
    },
        SuccessChecks = new[] { "Assign a FlashEffectProfile and call Play() from a script or UnityEvent." },
        Tags = new[] { "capability:VFX" }
    )]
public class SpriteFlasher : MonoBehaviour, IVisualFlashPlayer
{
    [Header("Targets")]
    [SerializeField, Tooltip("SpriteRenderers to flash. Leave empty and enable Auto Find to collect them automatically.")]
    private List<SpriteRenderer> _renderers = new List<SpriteRenderer>();
    [SerializeField, Tooltip("If true and _renderers is empty, finds all SpriteRenderers on this GameObject and its children on Awake.")]
    private bool _autoFindRenderers = true;

    [Header("Default Profile")]
    [SerializeField, Tooltip("profile used when Play() is called with no argument, and when Play On Start is enabled.")]
    private FlashEffectProfile _defaultProfile;
    [SerializeField, Tooltip("If true, plays the default profile automatically from Start.")]
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
        if (_playOnStart && _defaultProfile != null)
            Play(_defaultProfile);
    }

    private void OnDisable()
    {
        Stop();
        _initialized = false;
    }

    public void Play() => Play(_defaultProfile);

    public void Play(FlashEffectProfile profile)
    {
        if (profile == null)
        {
            Debug.LogWarning("[SpriteFlasher] Play called with no profile.", this);
            return;
        }

        Stop();
        if (!_initialized)
            Initialize();

        int loops = profile.loopCount < 0 ? -1 : Mathf.Max(1, profile.loopCount);
        _routine = StartCoroutine(FlashRoutine(profile, loops));
    }

    public void PlayOneShot(FlashEffectProfile profile)
    {
        if (profile == null)
            return;

        Stop();
        if (!_initialized)
            Initialize();

        _routine = StartCoroutine(FlashRoutine(profile, 1));
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

    private IEnumerator FlashRoutine(FlashEffectProfile profile, int loops)
    {
        Color[] baseColors = CaptureBaseColors(profile);
        int played = 0;

        while (loops < 0 || played < loops)
        {
            switch (profile.mode)
            {
                case FlashEffectProfile.FlashMode.Pulse:
                    yield return PulseRoutine(profile, baseColors);
                    break;
                case FlashEffectProfile.FlashMode.Strobe:
                    yield return StrobeRoutine(profile, baseColors);
                    break;
                case FlashEffectProfile.FlashMode.Blink:
                    yield return BlinkRoutine(profile, baseColors);
                    break;
                case FlashEffectProfile.FlashMode.ColorCycle:
                    yield return ColorCycleRoutine(profile, baseColors);
                    break;
            }

            if (loops > 0)
                played++;

            bool moreLoops = loops < 0 || played < loops;
            if (moreLoops && profile.cycleDelay > 0f)
            {
                if (profile.mode != FlashEffectProfile.FlashMode.ColorCycle)
                    RestoreToCapture(baseColors);
                yield return new WaitForSeconds(profile.cycleDelay);
            }
        }

        RestoreToCapture(baseColors);
        _onFlashComplete?.Invoke();
    }

    private IEnumerator PulseRoutine(FlashEffectProfile profile, Color[] baseColors)
    {
        Color flash = WithAlpha(profile.flashColor, profile.overrideAlpha, profile.flashAlpha, baseColors);
        yield return LerpAllRoutine(baseColors, flash, profile.flashDuration, profile.easeIn);
        yield return LerpAllRoutine(flash, baseColors, profile.flashDuration, profile.easeOut);
    }

    private IEnumerator StrobeRoutine(FlashEffectProfile profile, Color[] baseColors)
    {
        Color flash = WithAlpha(profile.flashColor, profile.overrideAlpha, profile.flashAlpha, baseColors);
        SetAll(flash);
        yield return new WaitForSeconds(profile.flashDuration);
        RestoreToCapture(baseColors);
        if (profile.interval > 0f)
            yield return new WaitForSeconds(profile.interval);
    }

    private IEnumerator BlinkRoutine(FlashEffectProfile profile, Color[] baseColors)
    {
        float fadeTime = profile.flashDuration * 0.25f;
        float holdTime = profile.flashDuration * 0.50f;
        Color flash = WithAlpha(profile.flashColor, profile.overrideAlpha, profile.flashAlpha, baseColors);

        yield return LerpAllRoutine(baseColors, flash, fadeTime, profile.easeIn);
        yield return new WaitForSeconds(holdTime);
        yield return LerpAllRoutine(flash, baseColors, fadeTime, profile.easeOut);

        if (profile.interval > 0f)
            yield return new WaitForSeconds(profile.interval);
    }

    private IEnumerator ColorCycleRoutine(FlashEffectProfile profile, Color[] baseColors)
    {
        if (profile.cycleColors == null || profile.cycleColors.Length == 0)
        {
            Debug.LogWarning("[SpriteFlasher] ColorCycle mode selected but cycleColors is empty.", this);
            yield break;
        }

        foreach (Color color in profile.cycleColors)
        {
            Color stepped = profile.overrideAlpha ? new Color(color.r, color.g, color.b, profile.flashAlpha) : color;
            SetAll(stepped);
            yield return new WaitForSeconds(profile.flashDuration);
        }
    }

    private IEnumerator LerpAllRoutine(Color[] from, Color to, float duration, FlashEffectProfile.FlashEase ease)
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

    private IEnumerator LerpAllRoutine(Color from, Color[] to, float duration, FlashEffectProfile.FlashEase ease)
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

    private Color[] CaptureBaseColors(FlashEffectProfile profile)
    {
        Color[] colors = new Color[_renderers.Count];
        for (int i = 0; i < _renderers.Count; i++)
        {
            if (_renderers[i] == null)
            {
                colors[i] = Color.white;
                continue;
            }

            colors[i] = profile.useRendererColorAsBase ? _renderers[i].color : profile.baseColor;
            if (profile.overrideAlpha)
                colors[i].a = profile.baseAlpha;
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

    private static float ApplyEase(FlashEffectProfile.FlashEase ease, float t)
    {
        switch (ease)
        {
            case FlashEffectProfile.FlashEase.Linear:
                return t;
            case FlashEffectProfile.FlashEase.InSine:
                return 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
            case FlashEffectProfile.FlashEase.OutSine:
                return Mathf.Sin(t * Mathf.PI * 0.5f);
            case FlashEffectProfile.FlashEase.InOutSine:
                return -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;
            case FlashEffectProfile.FlashEase.InQuad:
                return t * t;
            case FlashEffectProfile.FlashEase.OutQuad:
                return 1f - (1f - t) * (1f - t);
            case FlashEffectProfile.FlashEase.InOutQuad:
                return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) * 0.5f;
            case FlashEffectProfile.FlashEase.InCubic:
                return t * t * t;
            case FlashEffectProfile.FlashEase.OutCubic:
                return 1f - Mathf.Pow(1f - t, 3f);
            default:
                return Mathf.SmoothStep(0f, 1f, t);
        }
    }
}
}
