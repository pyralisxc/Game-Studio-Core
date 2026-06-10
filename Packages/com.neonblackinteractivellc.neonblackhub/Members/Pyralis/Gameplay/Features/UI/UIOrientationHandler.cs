using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace NeonBlack.Gameplay.Features.UI
{
    /// <summary>
    /// Repositions and resizes a UI element when the device rotates between portrait and landscape.
    /// Add this to any Canvas child that needs orientation-specific layout.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.UI,
        Relevance = "Maintains UI layout integrity across portrait and landscape device orientations.",
        Axioms = AuthoringWorldAxiom.None,
        RequiredComponents = new[] { typeof(RectTransform) },
        AssignmentFields = new[] { nameof(portrait), nameof(landscape) },
        FirstProof = "Rotate the device/screen and verify the UI element repositions to the captured layout.",
        NativeSetup = new[] 
        { 
            "Add UIOrientationHandler to a UI element.",
            "Capture Portrait and Landscape layouts in the Inspector."
        }
    )]
    [RequireComponent(typeof(RectTransform))]
    public class UIOrientationHandler : MonoBehaviour
{
        [System.Serializable]
        public class LayoutData
        {
            [HideInInspector]
            public bool captured;

            [Tooltip("Normalized anchor min, matching RectTransform.anchorMin.")]
            public Vector2 anchorMin = new Vector2(0f, 0f);

            [Tooltip("Normalized anchor max, matching RectTransform.anchorMax.")]
            public Vector2 anchorMax = new Vector2(0f, 0f);

            [Tooltip("Position offset from the anchor point in pixels.")]
            public Vector2 anchoredPosition = Vector2.zero;

            [Tooltip("Width and height in pixels.")]
            public Vector2 sizeDelta = new Vector2(100f, 100f);

            [Tooltip("Pivot point from 0 to 1.")]
            public Vector2 pivot = new Vector2(0.5f, 0.5f);

            [Tooltip("Local scale override. (1,1,1) leaves scale unchanged.")]
            public Vector3 localScale = Vector3.one;

            [Tooltip("Local rotation override in Euler angles. (0,0,0) leaves rotation unchanged.")]
            public Vector3 localEulerAngles = Vector3.zero;
        }

        [Header("Portrait Layout")]
        public LayoutData portrait;

        [Header("Landscape Layout")]
        public LayoutData landscape;

        [Header("Canvas Scaler (optional)")]
        [SerializeField, Tooltip("Assign the root CanvasScaler to flip matchWidthOrHeight on rotation. 0 = match width, 1 = match height.")]
        private CanvasScaler _canvasScaler;

        [SerializeField, Range(0f, 1f), Tooltip("matchWidthOrHeight value applied in Portrait mode. Recommended: 1.")]
        private float _portraitMatch = 1f;

        [SerializeField, Range(0f, 1f), Tooltip("matchWidthOrHeight value applied in Landscape mode. Recommended: 0.")]
        private float _landscapeMatch = 0f;

        private RectTransform _rt;
        private bool _wasPortrait;

        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
        }

        private void Start()
        {
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

        public void CapturePortrait()
        {
            CaptureInto(portrait);
        }

        public void CaptureLandscape()
        {
            CaptureInto(landscape);
        }

        private void ApplyCurrentLayout()
        {
            if (_rt == null)
                return;

            bool isPortrait = IsPortrait();
            LayoutData data = isPortrait ? portrait : landscape;
            if (!data.captured)
                return;

            Apply(data);

            if (_canvasScaler != null)
                _canvasScaler.matchWidthOrHeight = isPortrait ? _portraitMatch : _landscapeMatch;
        }

        private void Apply(LayoutData data)
        {
            _rt.anchorMin = data.anchorMin;
            _rt.anchorMax = data.anchorMax;
            _rt.pivot = data.pivot;
            _rt.anchoredPosition = data.anchoredPosition;
            _rt.sizeDelta = data.sizeDelta;
            _rt.localScale = data.localScale;
            _rt.localEulerAngles = data.localEulerAngles;
        }

        private void CaptureInto(LayoutData data)
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            data.anchorMin = rectTransform.anchorMin;
            data.anchorMax = rectTransform.anchorMax;
            data.pivot = rectTransform.pivot;
            data.anchoredPosition = rectTransform.anchoredPosition;
            data.sizeDelta = rectTransform.sizeDelta;
            data.localScale = rectTransform.localScale;
            data.localEulerAngles = rectTransform.localEulerAngles;
            data.captured = true;
        }

        private static bool IsPortrait()
        {
            return Screen.height >= Screen.width;
        }
    }
}
