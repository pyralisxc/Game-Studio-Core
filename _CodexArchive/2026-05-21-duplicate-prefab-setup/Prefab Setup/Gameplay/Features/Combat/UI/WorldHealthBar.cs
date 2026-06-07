using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NeonBlack.Gameplay.Features.Combat
{
/// <summary>
/// Polished world-space health bar with a full suite of visual features.
///
/// Features:
///   â€¢ Ghost / drain bar â€” optional amber bar lingers then slowly drains on damage
///   â€¢ Smooth animated HP fill with configurable speed
///   â€¢ Always-green fill â€” width shrinks with HP, revealing the lost-HP colour behind it
///   â€¢ Border layer (configurable thickness)
///   â€¢ Segment dividers splitting the bar into equal sections
///   â€¢ Scale-punch animation when taking damage
///   â€¢ Low-HP pulse flash at configurable threshold
///   â€¢ Optional floating character name label above the bar
///   â€¢ Optional live HP number display inside the bar
///   â€¢ Auto-spawns damage/heal numbers via DamageNumberSpawner (if present)
///   â€¢ Auto-hide with smooth fade; or always-visible mode
///
/// Setup: Add this component to any root GameObject that also has a HealthComponent.
///        Everything is built at runtime â€” no prefab or Canvas needed in the scene.
/// </summary>
[RequireComponent(typeof(HealthComponent))]
public class WorldHealthBar : MonoBehaviour
{
    // Internal canvas coordinate width. All positions and sizes are expressed
    // in this pixel space, then the root transform is scaled to barSize world units.
    private const float CW           = 400f;
    // Texture height used when generating the rounded-corner sprite (pixels).
    private const int   BarSpriteTexH = 16;

    // â”€â”€ Position â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    [Header("Position")]
    [Tooltip("World-space offset from the character root. e.g. (0, 2.2, 0) above the head.")]
    [SerializeField] private Vector3 barOffset = new Vector3(0f, 2.2f, 0f);

    // â”€â”€ Bar Size â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    [Header("Bar Size")]
    [Tooltip("Width and height of the health bar in world units.")]
    [SerializeField] private Vector2 barSize  = new Vector2(1.2f, 0.12f);
    [Tooltip("Border thickness around the bar in canvas pixels. 0 = no border.")]
    [SerializeField] private float   borderPx     = 4f;
    [Tooltip("Corner rounding as a fraction of bar height. 0 = sharp square corners, 0.5 = full pill / capsule shape.")]
    [Range(0f, 0.5f)]
    [SerializeField] private float   cornerRadius = 0f;

    // â”€â”€ Colours â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    [Header("Colours")]
    [SerializeField] private Color bgColor      = new Color(0.08f, 0.08f, 0.08f, 0.92f);
    [SerializeField] private Color borderColor  = new Color(0.00f, 0.00f, 0.00f, 1.00f);
    [Tooltip("Fill colour at full / high HP.")]
    [SerializeField] private Color fillColor     = new Color(0.20f, 0.80f, 0.25f, 1.00f);  // green
    [Tooltip("Fill colour at the mid-point â€” used only when Fill Gradient is enabled.")]
    [SerializeField] private Color midHpColor    = new Color(0.95f, 0.80f, 0.10f, 1.00f);  // yellow
    [Tooltip("Colour of the region that represents missing / lost HP.")]
    [SerializeField] private Color emptyHpColor  = new Color(0.65f, 0.10f, 0.10f, 1.00f);  // red
    [Tooltip("Colour of the fill when the low-HP flash is active.")]
    [SerializeField] private Color lowHpColor    = new Color(0.90f, 0.20f, 0.10f, 1.00f);  // bright red
    [Tooltip("Blend the fill colour from Fill Colour â†’ Mid HP Colour â†’ Low HP Colour as HP falls. Disable to keep a constant fill colour.")]
    [SerializeField] private bool  fillGradient  = false;
    [Tooltip("Colour of the ghost / drain bar (sits behind the fill).")]
    [SerializeField] private Color ghostColor    = new Color(1.00f, 0.85f, 0.15f, 0.75f);  // amber
    [SerializeField] private Color segmentColor  = new Color(0.00f, 0.00f, 0.00f, 0.50f);

    // â”€â”€ Ghost Bar â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    [Header("Ghost Bar  (damage drain)")]
    [Tooltip("Seconds after a hit before the ghost bar starts draining down.")]
    [SerializeField] private float ghostDelay      = 0.60f;
    [Tooltip("How fast the ghost bar drains to current HP (0\u20131 fill units per second).")]
    [SerializeField] private float ghostDrainSpeed = 0.50f;
    [Tooltip("Show the amber ghost/trail bar behind the fill. Disable for a fill-only bar that immediately reflects current HP without the lingering effect.")]
    [SerializeField] private bool  showGhostBar    = false;

    // â”€â”€ Fill Animation â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    [Header("Fill Animation")]
    [Tooltip("Speed the green fill animates to the new HP (fill units/s). 0 = instant snap.")]
    [SerializeField] private float fillAnimSpeed = 8f;
    [Tooltip("Brief scale-up punch when damage is taken.")]
    [SerializeField] private bool  punchOnDamage = true;
    [Tooltip("Peak scale multiplier during the punch.")]
    [SerializeField] private float punchScale    = 1.12f;
    [Tooltip("Punch animation duration in seconds.")]
    [SerializeField] private float punchDuration = 0.12f;

    // â”€â”€ Low HP Flash â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    [Header("Low HP Flash")]
    [SerializeField] private bool  flashAtLowHp   = true;
    [Tooltip("How fast the bar pulses when at low HP.")]
    [SerializeField] private float flashSpeed     = 3f;
    [Tooltip("HP fraction at or below which the pulse flash begins.")]
    [SerializeField] private float flashThreshold = 0.25f;

    // â”€â”€ Segments â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    [Header("Segments")]
    [Tooltip("Divide the bar into N equal sections. 0 or 1 = none. 4 = dividers at 25\u202650\u202675%.")]
    [SerializeField] private int segmentCount = 4;

    // â”€â”€ Labels â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    [Header("Labels")]
    [SerializeField] private bool   showName      = true;
    [Tooltip("Display name above the bar. Leave blank to use this GameObject\u2019s name.")]
    [SerializeField] private string displayName   = "";
    [SerializeField] private bool   showHpNumbers = true;
    [SerializeField] private Color  nameColor     = Color.white;
    [SerializeField] private Color  hpNumberColor = new Color(1f, 1f, 1f, 0.80f);

    // â”€â”€ Label Style â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    [Header("Label Style")]
    [Tooltip("Font size for the name label above the bar.")]
    [SerializeField] private float         nameSize      = 14f;
    [Tooltip("Font style for the name label (Bold, Italic, BoldItalic, Normal, etc.).")]
    [SerializeField] private FontStyles    nameFontStyle = FontStyles.Bold;
    [Tooltip("Custom font asset for the name label. Leave None to use the TMP default.")]
    [SerializeField] private TMP_FontAsset nameFont      = null;
    [Tooltip("Font size for the HP number display inside the bar.")]
    [SerializeField] private float         hpSize        = 10f;
    [Tooltip("Font style for the HP numbers (Bold, Italic, BoldItalic, Normal, etc.).")]
    [SerializeField] private FontStyles    hpFontStyle   = FontStyles.Bold;
    [Tooltip("Custom font asset for the HP numbers. Leave None to use the TMP default.")]
    [SerializeField] private TMP_FontAsset hpFont        = null;

    // â”€â”€ HP Label Format â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    public enum HpLabelFormat { Numeric, Percentage, Both }

    [Header("HP Label Format")]
    [Tooltip("Numeric = \"80/100\".  Percentage = \"80%\".  Both = \"80/100 (80%)\"")]
    [SerializeField] private HpLabelFormat hpFormat = HpLabelFormat.Numeric;

    // â”€â”€ Damage Numbers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    [Header("Damage Numbers")]
    [Tooltip("World-space offset from the character root where floating numbers appear.")]
    [SerializeField] private Vector3 numberSpawnOffset  = new Vector3(0f, 1.5f, 0f);
    [Tooltip("Spawn a floating number when this character takes damage.")]
    [SerializeField] private bool    showDamageNumbers  = true;
    [Tooltip("Spawn a floating number when this character is healed.")]
    [SerializeField] private bool    showHealNumbers    = true;

    // â”€â”€ Sorting â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    [Header("Sorting")]
    [Tooltip("Sorting layer for the health bar canvas. Must exactly match a name in\nProject Settings > Tags & Layers (e.g. Default, Characters). Same layer as your sprite.")]
    [SerializeField] private string sortingLayerName  = "Default";

    [Tooltip("Order within the sorting layer. Higher numbers render on top.")]
    [SerializeField] private int sortingOrderInLayer = 1;

    // â”€â”€ Visibility â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    [Header("Visibility")]
    [Tooltip("Seconds of inactivity before the bar fades out.")]
    [SerializeField] private float hideDelay     = 3f;
    [SerializeField] private float fadeSpeed     = 5f;
    [Tooltip("Keep the bar permanently visible \u2014 never auto-hide.")]
    [SerializeField] private bool  alwaysVisible = false;

    // â”€â”€ Runtime â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    private HealthComponent _health;
    private Camera          _cam;
    private Transform       _canvasRoot;
    private Image           _fill;    // green \u2014 current HP (animated toward _targetFill)
    private Image           _ghost;   // amber \u2014 holds position then drains after delay
    private CanvasGroup     _group;
    private TMP_Text        _nameLabel;
    private TMP_Text        _hpLabel;

    private bool    _visible;
    private float   _hideTimer;
    private float   _targetFill;    // true current HP as 0\u20131
    private float   _ghostTimer;    // countdown before ghost starts draining
    private bool    _isPunching;
    private float   _punchTimer;
    private Vector3 _baseScale;
    private float   _flashTimer;    private Sprite  _roundedSprite;  // generated once; null = square corners
    // â”€â”€ Lifecycle â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    private void Awake()
    {
        _health = GetComponent<HealthComponent>();
        _cam    = Camera.main;

        BuildCanvas();

        _health.OnDamaged.AddListener(OnDamaged);
        _health.OnHealed.AddListener(OnHealed);
        _health.OnDeath.AddListener(OnDeath);

        _group.alpha = alwaysVisible ? 1f : 0f;
        _visible     = alwaysVisible;
        _baseScale   = _canvasRoot.localScale;

        if (_nameLabel != null)
            _nameLabel.text = string.IsNullOrEmpty(displayName) ? gameObject.name : displayName;
    }

    private void Start()
    {
        // Prime fill in Start (not Awake) so HealthComponent.Awake has already
        // run and CurrentHealth is properly initialised to MaxHealth.
        _targetFill = Mathf.Clamp01(_health.HealthPercent);
        _fill.rectTransform.sizeDelta = new Vector2(CW * _targetFill, 0f);
        if (_ghost != null) _ghost.rectTransform.sizeDelta = new Vector2(CW * _targetFill, 0f);
        RefreshFillColor(_targetFill);
        UpdateHpLabel();
    }

    private void LateUpdate()
    {
        if (_canvasRoot == null) return;

        // Continuously poll health so the bar can never drift out of sync.
        _targetFill = Mathf.Clamp01(_health.HealthPercent);

        // Follow character and billboard to camera.
        _canvasRoot.position = transform.position + barOffset;
        if (_cam != null) _canvasRoot.rotation = _cam.transform.rotation;

        // â”€â”€ Animate green fill toward true HP â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        // Resize the fill rect directly â€” more reliable than Image.Type.Filled.
        float curFill = _fill.rectTransform.sizeDelta.x / CW;
        curFill = fillAnimSpeed > 0f
            ? Mathf.MoveTowards(curFill, _targetFill, fillAnimSpeed * Time.deltaTime)
            : _targetFill;
        _fill.rectTransform.sizeDelta = new Vector2(CW * curFill, 0f);

        // Colour gradient (may be overridden below by the flash block).
        RefreshFillColor(curFill);

        // â”€â”€ Ghost bar drains toward true HP after delay â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        if (_ghost != null)
        {
            _ghostTimer -= Time.deltaTime;
            if (_ghostTimer <= 0f)
            {
                float ghostFill = _ghost.rectTransform.sizeDelta.x / CW;
                if (ghostFill > _targetFill)
                {
                    ghostFill = Mathf.MoveTowards(ghostFill, _targetFill, ghostDrainSpeed * Time.deltaTime);
                    _ghost.rectTransform.sizeDelta = new Vector2(CW * ghostFill, 0f);
                }
            }
        }

        // â”€â”€ Scale punch â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        if (_isPunching)
        {
            _punchTimer -= Time.deltaTime;
            float t = Mathf.Clamp01(1f - _punchTimer / punchDuration);
            _canvasRoot.localScale = _baseScale * Mathf.Lerp(1f, punchScale, Mathf.Sin(t * Mathf.PI));
            if (_punchTimer <= 0f)
            {
                _isPunching            = false;
                _canvasRoot.localScale = _baseScale;
            }
        }

        // â”€â”€ Low HP pulse flash â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        // Overrides the gradient colour set above by RefreshFillColor.
        if (flashAtLowHp && _targetFill <= flashThreshold)
        {
            _flashTimer += Time.deltaTime * flashSpeed;
            float pulse = Mathf.Sin(_flashTimer * Mathf.PI * 2f) * 0.5f + 0.5f;
            _fill.color = Color.Lerp(lowHpColor, Color.white, pulse * 0.30f);
        }

        // â”€â”€ Auto-hide countdown â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        if (!alwaysVisible && _visible)
        {
            _hideTimer -= Time.deltaTime;
            if (_hideTimer <= 0f) _visible = false;
        }

        _group.alpha = Mathf.MoveTowards(
            _group.alpha, (_visible || alwaysVisible) ? 1f : 0f, fadeSpeed * Time.deltaTime);
    }

    // â”€â”€ Health callbacks â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    private void OnDamaged(float amount)
    {
        _targetFill = Mathf.Clamp01(_health.HealthPercent);
        _ghostTimer = ghostDelay;   // ghost holds position; drains after delay
        Show();
        UpdateHpLabel();
        if (punchOnDamage) TriggerPunch();
        if (showDamageNumbers)
            DamageNumberSpawner.Instance?.Spawn(amount, transform.position + numberSpawnOffset);
    }

    private void OnHealed(float amount)
    {
        _targetFill = Mathf.Clamp01(_health.HealthPercent);
        if (_ghost != null) _ghost.rectTransform.sizeDelta = new Vector2(CW * _targetFill, 0f);  // no ghost delay on heals â€” snap forward
        if (!alwaysVisible) Show();
        UpdateHpLabel();
        if (showHealNumbers)
            DamageNumberSpawner.Instance?.SpawnHeal(amount, transform.position + numberSpawnOffset);
    }

    private void OnDeath() { _visible = false; }

    // â”€â”€ Helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    private void Show()         { _visible = true; _hideTimer = hideDelay; }
    private void TriggerPunch() { _isPunching = true; _punchTimer = punchDuration; }

    /// <summary>
    /// Sets fill colour using optional gradient (fillColor â†’ midHpColor â†’ lowHpColor).
    /// Skipped when the low-HP flash owns the colour.
    /// </summary>
    private void RefreshFillColor(float pct)
    {
        if (_fill == null) return;
        if (flashAtLowHp && _targetFill <= flashThreshold) return;

        if (fillGradient)
        {
            // Above 50% HP: lerp from midHpColor to fillColor
            // Below 50% HP: lerp from lowHpColor to midHpColor
            _fill.color = pct >= 0.5f
                ? Color.Lerp(midHpColor, fillColor,   (pct - 0.5f) * 2f)
                : Color.Lerp(lowHpColor, midHpColor,   pct         * 2f);
        }
        else
        {
            _fill.color = fillColor;
        }
    }

    private void UpdateHpLabel()
    {
        if (_hpLabel == null || !showHpNumbers) return;
        int cur  = Mathf.CeilToInt(_health.CurrentHealth);
        int max  = Mathf.CeilToInt(_health.MaxHealth);
        int pct  = Mathf.RoundToInt(_health.HealthPercent * 100f);
        _hpLabel.text = hpFormat switch
        {
            HpLabelFormat.Percentage => $"{pct}%",
            HpLabelFormat.Both       => $"{cur}/{max}  ({pct}%)",
            _                        => $"{cur}/{max}",
        };
    }

    // â”€â”€ Canvas builder â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    private void BuildCanvas()
    {
        // Canvas height derived from barSize aspect ratio.
        float ch    = CW * (barSize.y / Mathf.Max(0.001f, barSize.x));
        // World units per canvas pixel \u2014 scales root to barSize.
        float scale = barSize.x / CW;

        GameObject root = new GameObject("WorldHealthBar");
        root.transform.SetParent(transform);
        root.transform.localPosition = barOffset;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale    = new Vector3(scale, scale, scale);

        Canvas canvas              = root.AddComponent<Canvas>();
        canvas.renderMode             = RenderMode.WorldSpace;
        canvas.sortingLayerName       = sortingLayerName;
        canvas.sortingOrder           = sortingOrderInLayer;
        canvas.overrideSorting        = true;
        root.AddComponent<CanvasScaler>();

        _group                = root.AddComponent<CanvasGroup>();
        _group.interactable   = false;
        _group.blocksRaycasts = false;

        _canvasRoot = root.transform;
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(CW, ch);

        // Border (slightly larger rect behind everything)
        Image borderImg = null;
        if (borderPx > 0f)
            borderImg = MakeRect(root, "Border", borderColor, CW + borderPx * 2f, ch + borderPx * 2f);

        // Dark background
        Image bgImg    = MakeRect(root, "BG", bgColor, CW, ch);
        // Lost HP region â€” full-width coloured layer visible wherever the fill doesn't cover
        Image emptyImg = MakeRect(root, "EmptyHP", emptyHpColor, CW, ch);
        // Ghost bar (amber â€” stays put on damage, drains after delay)
        if (showGhostBar)
            _ghost = MakeFill(root, "Ghost", ghostColor, CW, ch);

        // HP fill (green â€” snaps/animates to current HP)
        _fill = MakeFill(root, "Fill", fillColor, CW, ch);

        // Segment dividers
        MakeSegments(root, CW, ch);

        // Name label above the bar
        if (showName)
            _nameLabel = MakeLabel(root, "Name", nameColor, nameSize, nameFontStyle, nameFont,
                new Vector2(CW, 28f), new Vector2(0f, ch * 0.5f + 14f));

        // HP numbers centred inside the bar
        if (showHpNumbers)
            _hpLabel = MakeLabel(root, "HP", hpNumberColor, hpSize, hpFontStyle, hpFont,
                new Vector2(CW * 0.9f, ch), Vector2.zero);

        // Rounded corners â€” generate once and apply to every bar layer
        if (cornerRadius > 0f)
        {
            Sprite s   = GetBarSprite();
            float  ppu = BarSpriteTexH / ch;
            if (borderImg != null) SetRounded(borderImg, s, BarSpriteTexH / (ch + borderPx * 2f));
            SetRounded(bgImg,    s, ppu);
            SetRounded(emptyImg, s, ppu);
            if (_ghost != null) SetRounded(_ghost, s, ppu);
            SetRounded(_fill,    s, ppu);
        }
    }

    // Creates a plain coloured rectangle in canvas pixel space, centred at anchoredPos.
    private Image MakeRect(GameObject parent, string goName, Color color, float w, float h,
                           Vector2 anchoredPos = default)
    {
        GameObject go = new GameObject(goName);
        go.transform.SetParent(parent.transform, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        RectTransform rt = img.rectTransform;
        rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = new Vector2(w, h);
        rt.anchoredPosition = anchoredPos;
        return img;
    }

    // Creates a left-anchored coloured rectangle whose width is driven by HP percentage.
    // The right edge shrinks as HP falls, revealing the EmptyHP layer behind it.
    private Image MakeFill(GameObject parent, string goName, Color color, float w, float h)
    {
        GameObject go = new GameObject(goName);
        go.transform.SetParent(parent.transform, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        RectTransform rt = img.rectTransform;
        rt.anchorMin        = new Vector2(0f, 0f);   // anchor to left-bottom of parent
        rt.anchorMax        = new Vector2(0f, 1f);   // anchor to left-top  (full height stretch)
        rt.pivot            = new Vector2(0f, 0.5f); // pivot at left-centre
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta        = new Vector2(w, 0f);    // explicit pixel width; height = parent height
        return img;
    }

    // Builds thin vertical divider lines, splitting the bar into segmentCount sections.
    private void MakeSegments(GameObject parent, float w, float h)
    {
        if (segmentCount <= 1) return;
        float lineW = Mathf.Max(1f, w * 0.005f);
        for (int i = 1; i < segmentCount; i++)
        {
            float xPos = ((float)i / segmentCount - 0.5f) * w;
            Image ln   = MakeRect(parent, $"Seg{i}", segmentColor, lineW, h);
            ln.rectTransform.anchoredPosition = new Vector2(xPos, 0f);
        }
    }

    // Creates a world-space TMP text label inside this canvas.
    private TMP_Text MakeLabel(GameObject parent, string goName, Color color,
                               float fontSize, FontStyles fontStyle, TMP_FontAsset font,
                               Vector2 sizeDelta, Vector2 anchoredPos)
    {
        GameObject go = new GameObject(goName);
        go.transform.SetParent(parent.transform, false);
        TMP_Text t      = go.AddComponent<TextMeshProUGUI>();
        t.color         = color;
        t.fontSize      = fontSize;
        t.fontStyle     = fontStyle;
        if (font != null) t.font = font;
        t.alignment     = TextAlignmentOptions.Center;
        t.overflowMode  = TextOverflowModes.Overflow;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = sizeDelta;
        rt.anchoredPosition = anchoredPos;
        return t;
    }

    // â”€â”€ Rounded-corner helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //

    /// <summary>
    /// Generates (on first call) and returns a 9-sliced white sprite with rounded corners.
    /// The radius is derived from the current cornerRadius field.
    /// </summary>
    private Sprite GetBarSprite()
    {
        if (_roundedSprite != null) return _roundedSprite;

        const int W = 64;
        const int H = BarSpriteTexH;
        int r = Mathf.RoundToInt(cornerRadius * H);  // pixel radius

        Texture2D tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        tex.wrapMode  = TextureWrapMode.Clamp;
        Color[] pixels = new Color[W * H];
        for (int y = 0; y < H; y++)
        for (int x = 0; x < W; x++)
            pixels[y * W + x] = InsideRounded(x, y, W, H, r) ? Color.white : Color.clear;
        tex.SetPixels(pixels);
        tex.Apply();

        // 9-slice border = corner radius in all four directions so the corners never stretch.
        _roundedSprite = Sprite.Create(
            tex,
            new Rect(0, 0, W, H),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(r, r, r, r));
        return _roundedSprite;
    }

    /// <summary>
    /// Returns true if pixel (px, py) is inside the rounded-rectangle shape.
    /// </summary>
    private static bool InsideRounded(int px, int py, int w, int h, int r)
    {
        if (r <= 0) return true;
        // For each corner quadrant check if the pixel is inside the rounded arc
        int cx = px < r          ? r         : (px >= w - r ? w - r - 1 : px);
        int cy = py < r          ? r         : (py >= h - r ? h - r - 1 : py);
        int dx = px - cx;
        int dy = py - cy;
        return (dx * dx + dy * dy) <= r * r;
    }

    /// <summary>
    /// Applies the rounded sprite to an Image as a 9-sliced type with the given pixels-per-unit.
    /// </summary>
    private static void SetRounded(Image img, Sprite s, float pixelsPerUnit)
    {
        if (img == null || s == null) return;
        img.sprite                    = s;
        img.type                      = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier   = pixelsPerUnit;
    }
}
}
