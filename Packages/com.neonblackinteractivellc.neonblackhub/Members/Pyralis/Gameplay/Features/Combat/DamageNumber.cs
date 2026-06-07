using UnityEngine;
using TMPro;

namespace NeonBlack.Gameplay.Features.Combat
{
/// <summary>
/// A single floating damage / heal number - built entirely at runtime.
/// Uses 3D world-space TextMeshPro so no Canvas or prefab setup is needed.
/// Pooled by DamageNumberSpawner - do not Instantiate this directly.
///
/// Inspector groups:
///   Motion   - rise speed, style, scatter, lifetime, fade timing, pop-on-spawn
///   Text     - font size, style, critical multiplier, font asset, heal prefix
///   Colours  - normal / critical / heal colours
///   Outline  - optional text outline for readability over bright backgrounds
/// </summary>
public class DamageNumber : MonoBehaviour
{
    public enum RiseStyle
    {
        Straight,   // pure vertical rise
        Drift,      // rises with a gentle random horizontal drift
        Arc,        // rises quickly then decelerates (like a thrown ball)
    }

    // Motion.
    [Header("Motion")]
    [Tooltip("How fast numbers rise in world units per second.")]
    [SerializeField] private float riseSpeed         = 2.5f;
    [Tooltip("Straight = pure vertical.  Drift = gentle horizontal wander.  Arc = fast rise with smooth deceleration.")]
    [SerializeField] private RiseStyle riseStyle     = RiseStyle.Straight;
    [Tooltip("Maximum random horizontal scatter on spawn so stacked hits don\u2019t overlap.")]
    [SerializeField] private float horizontalScatter = 0.25f;
    [Tooltip("How long (seconds) the number stays visible.")]
    [SerializeField] private float lifetime          = 0.9f;
    [Tooltip("Fraction of lifetime (0\u20131) after which the number begins fading out.")]
    [Range(0f, 1f)]
    [SerializeField] private float fadeStart         = 0.45f;
    [Tooltip("Scale the number up from zero on spawn for a punchy pop-in feel.")]
    [SerializeField] private bool  scalePopOnSpawn   = false;

    // Text.
    [Header("Text")]
    [Tooltip("World-unit font size for normal numbers.")]
    [SerializeField] private float         fontSize               = 2.2f;
    [SerializeField] private FontStyles    fontStyle              = FontStyles.Bold;
    [Tooltip("Font size multiplier applied to critical hit numbers.")]
    [SerializeField] private float         criticalSizeMultiplier = 1.4f;
    [Tooltip("Prefix heal numbers with \u2018+\u2019. Disable to show the raw number.")]
    [SerializeField] private bool          showPlusOnHeal         = true;
    [Tooltip("Optional custom TMP font asset. Leave None to use the TMP default.")]
    [SerializeField] private TMP_FontAsset fontAsset              = null;

    // Colours.
    [Header("Colours")]
    [SerializeField] private Color normalColor   = Color.white;
    [SerializeField] private Color criticalColor = new Color(1f, 0.35f, 0f, 1f);    // orange-red
    [SerializeField] private Color healColor     = new Color(0.2f, 1f, 0.35f, 1f);  // green

    // Outline.
    [Header("Outline  (improves readability over bright backgrounds)")]
    [Tooltip("Draw a coloured outline around the text.")]
    [SerializeField] private bool  useOutline   = false;
    [SerializeField] private Color outlineColor = Color.black;
    [Tooltip("Outline thickness (0 = none, 1 = maximum).")]
    [Range(0f, 1f)]
    [SerializeField] private float outlineWidth = 0.25f;

    // Runtime state.
    private TextMeshPro _label;
    private float       _timer;
    private Camera      _cam;
    private DamageNumberSpawner _owner;
    private bool        _active;
    private float       _popTimer;
    private Vector3     _driftDir;  // lateral direction for Drift mode

    private const float PopDuration = 0.12f;

    private void Awake()
    {
        _label                    = gameObject.AddComponent<TextMeshPro>();
        _label.alignment          = TextAlignmentOptions.Center;
        _label.fontSize           = fontSize;
        _label.fontStyle          = fontStyle;
        _label.textWrappingMode   = TextWrappingModes.NoWrap;
        _label.overflowMode       = TextOverflowModes.Overflow;
        if (fontAsset != null) _label.font = fontAsset;

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!_active) return;

        _timer -= Time.deltaTime;
        float progress = 1f - Mathf.Clamp01(_timer / lifetime);  // 0 at spawn, 1 at end

        // Rise.
        switch (riseStyle)
        {
            case RiseStyle.Drift:
                transform.position += (_driftDir * riseSpeed * 0.35f
                                      + Vector3.up * riseSpeed) * Time.deltaTime;
                break;
            case RiseStyle.Arc:
                // Decelerates as progress approaches 1 (fast start, coasts to a stop)
                float arcMult = Mathf.Clamp01(1f - progress) * 2f;
                transform.position += Vector3.up * riseSpeed * arcMult * Time.deltaTime;
                break;
            default:  // Straight
                transform.position += Vector3.up * riseSpeed * Time.deltaTime;
                break;
        }

        // Billboard.
        if (_cam != null)
            transform.rotation = _cam.transform.rotation;

        // Pop-in scale.
        if (scalePopOnSpawn && _popTimer < PopDuration)
        {
            _popTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_popTimer / PopDuration);
            float s = t < 0.5f
                ? Mathf.Lerp(0f,  1.2f, t * 2f)
                : Mathf.Lerp(1.2f, 1f, (t - 0.5f) * 2f);
            transform.localScale = Vector3.one * s;
        }

        // Fade out.
        float fadeProgress = Mathf.Clamp01((progress - fadeStart) / Mathf.Max(0.001f, 1f - fadeStart));
        Color c  = _label.color;
        c.a       = 1f - fadeProgress;
        _label.color = c;

        if (_timer <= 0f)
        {
            _active = false;
            _owner?.Return(this);
        }
    }

    public void ConfigureRuntime(Camera camera, DamageNumberSpawner owner)
    {
        _cam = camera;
        _owner = owner;
    }

    /// <summary>
    /// Initialise and play this number. Called by DamageNumberSpawner.
    /// </summary>
    public void Play(float amount, Vector3 worldPos, bool isCritical = false, bool isHeal = false)
    {
        float scatter        = Random.Range(-horizontalScatter, horizontalScatter);
        transform.position   = new Vector3(worldPos.x + scatter, worldPos.y, worldPos.z);
        transform.localScale = scalePopOnSpawn ? Vector3.zero : Vector3.one;

        _timer    = lifetime;
        _popTimer = 0f;
        _active   = true;
        _driftDir  = new Vector3(Random.value > 0.5f ? 1f : -1f, 0f, 0f);
        gameObject.SetActive(true);

        // Text content
        _label.text = isHeal
            ? (showPlusOnHeal ? $"+{Mathf.CeilToInt(amount)}" : Mathf.CeilToInt(amount).ToString())
            : Mathf.CeilToInt(amount).ToString();

        // Colour & size
        Color baseColor  = isHeal     ? healColor
                         : isCritical ? criticalColor
                                      : normalColor;
        _label.color     = baseColor;
        _label.fontSize  = isCritical ? fontSize * criticalSizeMultiplier : fontSize;
        _label.fontStyle = fontStyle;
        if (fontAsset != null) _label.font = fontAsset;

        // Outline
        _label.outlineWidth = useOutline ? outlineWidth : 0f;
        if (useOutline) _label.outlineColor = outlineColor;
    }
}
}
