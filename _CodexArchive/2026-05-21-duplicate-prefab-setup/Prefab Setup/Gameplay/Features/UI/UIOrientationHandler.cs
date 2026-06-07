using UnityEngine;
using UnityEngine.UI;

namespace NeonBlack.Gameplay.Features.UI
{
/// <summary>
/// Repositions and resizes UI elements when the device rotates between
/// portrait and landscape. Add one of these to ANY Canvas child that needs
/// to move or resize on rotation.
///
/// SETUP PER ELEMENT:
///   1. Select the UI element (e.g. JoystickContainer, DashButton).
///   2. Add Component â†’ UIOrientationHandler.
///   3. Fill in Portrait and Landscape blocks:
///        AnchorMin / AnchorMax  â€” normalised 0-1 anchor corners
///        AnchoredPosition       â€” position relative to anchors (pixels)
///        SizeDelta              â€” width / height in pixels
///        LocalScale             â€” override scale per orientation (1,1,1 = unchanged)
///        LocalEulerAngles       â€” override rotation per orientation (0,0,0 = unchanged)
///   4. Click the Capture buttons to snapshot the current RectTransform into each block.
///
/// WORKFLOW:
///   a. Set up your Canvas for portrait â†’ resize Game view to portrait aspect.
///   b. On each element click "Capture â†’ Portrait".
///   c. Drag elements to landscape positions â†’ resize Game view to landscape.
///   d. Click "Capture â†’ Landscape".
///   e. Hit Play â€” rotation is handled automatically at runtime.
///
/// The script checks orientation every frame and only re-applies layout on change.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UIOrientationHandler : MonoBehaviour
{
    [System.Serializable]
    public class LayoutData
    {
        /// <summary>True once Capture has been called for this slot. Layout is never applied until captured.</summary>
        [HideInInspector]
        public bool captured;
        [Tooltip("Normalised anchor min (same as RectTransform.anchorMin).")]
        public Vector2 anchorMin = new Vector2(0f, 0f);
        [Tooltip("Normalised anchor max (same as RectTransform.anchorMax).")]
        public Vector2 anchorMax = new Vector2(0f, 0f);
        [Tooltip("Position offset from the anchor point in pixels (anchoredPosition).")]
        public Vector2 anchoredPosition = Vector2.zero;
        [Tooltip("Width and height in pixels (sizeDelta).")]
        public Vector2 sizeDelta = new Vector2(100f, 100f);
        [Tooltip("Pivot point (0-1). Affects how anchoredPosition is calculated.")]
        public Vector2 pivot = new Vector2(0.5f, 0.5f);
        [Tooltip("Local scale override. (1,1,1) = no change.")]
        public Vector3 localScale = Vector3.one;
        [Tooltip("Local rotation override in Euler angles. (0,0,0) = no rotation.")]
        public Vector3 localEulerAngles = Vector3.zero;
    }

    [Header("Portrait Layout")]
    public LayoutData portrait;

    [Header("Landscape Layout")]
    public LayoutData landscape;

    [Header("Canvas Scaler (optional)")]
    [SerializeField, Tooltip("Assign the root Canvas's CanvasScaler to automatically flip matchWidthOrHeight on rotation.\n0 = match width (good for landscape), 1 = match height (good for portrait).")]
    private CanvasScaler _canvasScaler;
    [SerializeField, Range(0f, 1f), Tooltip("matchWidthOrHeight value applied in Portrait mode. Recommended: 1 (match height).")]
    private float _portraitMatch = 1f;
    [SerializeField, Range(0f, 1f), Tooltip("matchWidthOrHeight value applied in Landscape mode. Recommended: 0 (match width).")]
    private float _landscapeMatch = 0f;

    private RectTransform _rt;
    private bool _wasPortrait;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
    }

    private void Start()
    {
        // Force apply on first frame
        _wasPortrait = !IsPortrait();
        ApplyCurrentLayout();
    }

    private void Update()
    {
        bool nowPortrait = IsPortrait();
        if (nowPortrait != _wasPortrait)
        {
            _wasPortrait = nowPortrait;
            ApplyCurrentLayout();
        }
    }

    private void ApplyCurrentLayout()
    {
        if (_rt == null) return;
        bool portrait_ = IsPortrait();
        LayoutData data = portrait_ ? portrait : landscape;
        if (!data.captured) return;   // never collapsed by uncaptured defaults
        Apply(data);

        // Adjust Canvas Scaler match so UI scales correctly per orientation.
        if (_canvasScaler != null)
            _canvasScaler.matchWidthOrHeight = portrait_ ? _portraitMatch : _landscapeMatch;
    }

    private void Apply(LayoutData data)
    {
        _rt.anchorMin              = data.anchorMin;
        _rt.anchorMax              = data.anchorMax;
        _rt.pivot                  = data.pivot;
        _rt.anchoredPosition       = data.anchoredPosition;
        _rt.sizeDelta              = data.sizeDelta;
        _rt.localScale             = data.localScale;
        _rt.localEulerAngles       = data.localEulerAngles;
    }

    private static bool IsPortrait() => Screen.height >= Screen.width;

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // Editor helpers â€” called by the custom inspector buttons
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>Copy the current RectTransform values into the Portrait block.</summary>
    public void CapturePortrait()
    {
        RectTransform rt = GetComponent<RectTransform>();
        portrait.anchorMin        = rt.anchorMin;
        portrait.anchorMax        = rt.anchorMax;
        portrait.pivot            = rt.pivot;
        portrait.anchoredPosition = rt.anchoredPosition;
        portrait.sizeDelta        = rt.sizeDelta;
        portrait.localScale       = rt.transform.localScale;
        portrait.localEulerAngles = rt.transform.localEulerAngles;
        portrait.captured         = true;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    /// <summary>Copy the current RectTransform values into the Landscape block.</summary>
    public void CaptureLandscape()
    {
        RectTransform rt = GetComponent<RectTransform>();
        landscape.anchorMin        = rt.anchorMin;
        landscape.anchorMax        = rt.anchorMax;
        landscape.pivot            = rt.pivot;
        landscape.anchoredPosition = rt.anchoredPosition;
        landscape.sizeDelta        = rt.sizeDelta;
        landscape.localScale       = rt.transform.localScale;
        landscape.localEulerAngles = rt.transform.localEulerAngles;
        landscape.captured         = true;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}
}
