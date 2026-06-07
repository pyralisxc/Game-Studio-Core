using UnityEngine;

namespace NeonBlack.Gameplay.Presentation.Camera
{
/// <summary>
/// Keeps the camera showing a consistent gameplay area across all screen sizes,
/// aspect ratios, and device rotations.
///
/// STRATEGY Ã¢â‚¬â€ "Show All" (rotation-safe):
///   You define a MINIMUM gameplay rectangle in world units (_minWorldWidth Ãƒâ€” _minWorldHeight).
///   That full rectangle is ALWAYS visible, on every device, in any orientation.
///   Wider or taller screens simply reveal more world beyond the minimum area Ã¢â‚¬â€ no
///   warping, no cropping, no black bars.
///
///   Portrait phone  Ã¢â€ â€™ full width visible, more height revealed above/below
///   Landscape phone Ã¢â€ â€™ full height visible, more width revealed left/right
///   Tablet          Ã¢â€ â€™ both axes show more Ã¢â‚¬â€ gameplay area scales gracefully
///   Device rotated  Ã¢â€ â€™ orthoSize instantly recalculates, player is never cut off
///
/// LETTERBOX MODE (optional):
///   When _letterbox is true, black bars are added so every device sees exactly
///   the minimum rectangle Ã¢â‚¬â€ pixel-identical layout at the cost of screen space.
///
/// SETUP:
///   1. Attach to the Main Camera.
///   2. Set _minWorldWidth and _minWorldHeight to match your design gameplay area.
///      Default 10.8 Ãƒâ€” 19.2 = 1080Ãƒâ€”1920 at 100 pixels-per-unit.
///   3. Leave _letterbox false for mobile.
///   4. In Unity Player Settings Ã¢â€ â€™ Resolution and Presentation, enable the
///      orientations you want (Portrait, Landscape, or both for auto-rotation).
/// </summary>
[RequireComponent(typeof(UnityEngine.Camera))]
[DefaultExecutionOrder(-50)] // Must initialise before anything that reads HalfWidth/HalfHeight.
public class CameraAspectController : MonoBehaviour
{
    [Header("Landscape Minimum Gameplay Area (world units)")]
    [SerializeField, Tooltip("Minimum world units visible horizontally in LANDSCAPE. Default 19.2 = 1920px at 100px/unit.")]
    private float _minWorldWidth = 19.2f;
    [SerializeField, Tooltip("Minimum world units visible vertically in LANDSCAPE. Default 10.8 = 1080px at 100px/unit.")]
    private float _minWorldHeight = 10.8f;

    [Header("Portrait Minimum Gameplay Area (world units)")]
    [SerializeField, Tooltip("Minimum world units visible horizontally in PORTRAIT. Default 10.8 = 1080px at 100px/unit.\nSet to 0 to reuse the landscape values (single-orientation games).")]
    private float _portraitMinWidth = 10.8f;
    [SerializeField, Tooltip("Minimum world units visible vertically in PORTRAIT. Default 19.2 = 1920px at 100px/unit.\nSet to 0 to reuse the landscape values (single-orientation games).")]
    private float _portraitMinHeight = 19.2f;

    [Header("Letterbox (optional)")]
    [SerializeField, Tooltip("Add black bars to enforce exactly the minimum rectangle on every screen.\n" +
                             "Leave false (recommended) so extra screen area shows more world instead.")]
    private bool _letterbox = false;

    private int    _cachedScreenW;
    private int    _cachedScreenH;
    private UnityEngine.Camera _cam;

    // Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬
    // Public world extents Ã¢â‚¬â€ read by spawners, Motor2D, etc.
    // Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬

    /// <summary>Half the visible world height at the current orthographic size.</summary>
    public float HalfHeight => _cam != null ? _cam.orthographicSize : 0f;

    /// <summary>Half the visible world width at the current orthographic size and aspect.</summary>
    public float HalfWidth  => _cam != null ? _cam.orthographicSize * _cam.aspect : 0f;

    /// <summary>Singleton-style accessor so spawners can read extents without a direct reference.</summary>
    public static CameraAspectController Instance { get; private set; }

    /// <summary>
    /// The game's main camera. Use this instead of Camera.main (which does a scene search every call).
    /// All systems that need the camera Ã¢â‚¬â€ spawners, controllers, background managers Ã¢â‚¬â€ should read from here.
    /// </summary>
    public static UnityEngine.Camera Main { get; private set; }

    /// <summary>
    /// Fired whenever the screen resolution or orientation changes and the ortho size is recalculated.
    /// Subscribe to this instead of polling Screen.width/height in your own Update().
    /// Example: CameraAspectController.OnResolutionChanged += FitToCamera;
    /// </summary>
    public static event System.Action OnResolutionChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        _cam = GetComponent<UnityEngine.Camera>();
        Main = _cam;
        Apply();
    }

    private void Update()
    {
        // Catches device rotation, editor window resize, split-screen entry, etc.
        if (Screen.width != _cachedScreenW || Screen.height != _cachedScreenH)
            Apply();
    }

    private void Apply()
    {
        if (_cam == null) return;
        _cachedScreenW = Screen.width;
        _cachedScreenH = Screen.height;

        _cam.rect = new Rect(0f, 0f, 1f, 1f); // always reset viewport first

        if (_letterbox)
            ApplyLetterbox();
        else
            ApplyShowAll();

        OnResolutionChanged?.Invoke();
    }

    /// Returns the correct minimum world dimensions for the current screen orientation.
    /// Portrait values are used when screen height >= width; landscape values otherwise.
    /// If portrait fields are zero, falls back to landscape values (single-orientation setup).
    /// </summary>
    private void GetMinDimensions(out float minW, out float minH)
    {
        bool isPortrait = Screen.height >= Screen.width;
        if (isPortrait && _portraitMinWidth > 0f && _portraitMinHeight > 0f)
        {
            minW = _portraitMinWidth;
            minH = _portraitMinHeight;
        }
        else
        {
            minW = _minWorldWidth;
            minH = _minWorldHeight;
        }
    }

    /// <summary>
    /// Sets orthographic size so the full minimum rectangle is always visible.
    /// Wider/taller screens reveal extra world content on the long axis.
    ///
    /// Math:
    ///   halfHeight = orthoSize
    ///   halfWidth  = orthoSize * screenAspect
    ///
    ///   We need: halfHeight >= minH/2  AND  halfWidth >= minW/2
    ///   So:  orthoSize = max( minH/2,  (minW/2) / screenAspect )
    /// </summary>
    private void ApplyShowAll()
    {
        GetMinDimensions(out float minW, out float minH);
        float screenAspect = (float)Screen.width / Screen.height;
        _cam.orthographicSize = Mathf.Max(
            minH * 0.5f,
            (minW  * 0.5f) / screenAspect
        );
    }

    /// <summary>
    /// Fixes orthoSize to show exactly the minimum rectangle and adds
    /// black bars (letterbox or pillarbox) to fill the remaining screen.
    /// </summary>
    private void ApplyLetterbox()
    {
        GetMinDimensions(out float minW, out float minH);
        float minAspect    = minW / minH;
        float screenAspect = (float)Screen.width / Screen.height;

        // Use the orientation-correct ortho size
        _cam.orthographicSize = minH * 0.5f;

        if (Mathf.Approximately(screenAspect, minAspect)) return;

        if (screenAspect > minAspect)
        {
            // Screen wider than gameplay area Ã¢â‚¬â€ pillarbox
            float w = minAspect / screenAspect;
            _cam.rect = new Rect((1f - w) * 0.5f, 0f, w, 1f);
        }
        else
        {
            // Screen taller than gameplay area Ã¢â‚¬â€ letterbox
            float h = screenAspect / minAspect;
            _cam.rect = new Rect(0f, (1f - h) * 0.5f, 1f, h);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_minWorldWidth   <= 0f) _minWorldWidth   = 19.2f;
        if (_minWorldHeight  <= 0f) _minWorldHeight  = 10.8f;
        // Portrait fields of 0 = "use landscape values" Ã¢â‚¬â€ that's intentional, don't reset them.
    }
#endif

    private void OnDestroy()
    {
        if (Instance == this) { Instance = null; Main = null; }
        // Clear the static event so subscribers from the destroyed scene don't leak.
        OnResolutionChanged = null;
    }
}
}
