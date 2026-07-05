using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using TMPro;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Feedback
{
    /// <summary>
    /// Scene damage-number pool that implements IDamageNumberSink.
    /// No prefab is required; pooled numbers are runtime output owned by this spawner.
    /// </summary>
    public class DamageNumberSpawner : MonoBehaviour, IDamageNumberSink
    {
        [Header("Pool")]
        [SerializeField] private int initialPoolSize = 20;

        [Header("Presentation")]
        [Tooltip("Camera used to billboard floating numbers. Assign explicitly for split-screen, replay, or custom camera rigs.")]
        [SerializeField] private Camera popupCamera;

        private readonly Queue<DamageNumber> _pool = new Queue<DamageNumber>();

        private void Awake()
        {
            int poolSize = Mathf.Max(1, initialPoolSize);
            for (int i = 0; i < poolSize; i++)
                _pool.Enqueue(CreateNew());
        }

        public void Spawn(float amount, Vector3 worldPos, bool isCritical = false)
        {
            DamageNumber number = GetFromPool();
            number.ConfigureRuntime(popupCamera, this);
            number.Play(amount, worldPos, isCritical: isCritical, isHeal: false);
        }

        public void SpawnHeal(float amount, Vector3 worldPos)
        {
            DamageNumber number = GetFromPool();
            number.ConfigureRuntime(popupCamera, this);
            number.Play(amount, worldPos, isCritical: false, isHeal: true);
        }

        public void SetPopupCamera(Camera camera)
        {
            popupCamera = camera;
        }

        public void Return(DamageNumber number)
        {
            if (number == null)
                return;

            number.gameObject.SetActive(false);
            _pool.Enqueue(number);
        }

        private DamageNumber GetFromPool()
        {
            if (_pool.Count == 0)
                return CreateNew();

            DamageNumber number = _pool.Dequeue();
            return number != null ? number : CreateNew();
        }

        private DamageNumber CreateNew()
        {
            GameObject go = new GameObject("DamageNumber");
            go.transform.SetParent(transform);
            DamageNumber number = go.AddComponent<DamageNumber>();
            number.ConfigureRuntime(popupCamera, this);
            go.SetActive(false);
            return number;
        }
    }

    public class DamageNumber : MonoBehaviour
    {
        public enum RiseStyle
        {
            Straight,
            Drift,
            Arc
        }

        [Header("Motion")]
        [SerializeField] private float riseSpeed = 2.5f;
        [SerializeField] private RiseStyle riseStyle = RiseStyle.Straight;
        [SerializeField] private float horizontalScatter = 0.25f;
        [SerializeField] private float lifetime = 0.9f;
        [Range(0f, 1f)]
        [SerializeField] private float fadeStart = 0.45f;
        [SerializeField] private bool scalePopOnSpawn = false;

        [Header("Text")]
        [SerializeField] private float fontSize = 2.2f;
        [SerializeField] private FontStyles fontStyle = FontStyles.Bold;
        [SerializeField] private float criticalSizeMultiplier = 1.4f;
        [SerializeField] private bool showPlusOnHeal = true;
        [SerializeField] private TMP_FontAsset fontAsset = null;

        [Header("Colours")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color criticalColor = new Color(1f, 0.35f, 0f, 1f);
        [SerializeField] private Color healColor = new Color(0.2f, 1f, 0.35f, 1f);

        [Header("Outline")]
        [SerializeField] private bool useOutline = false;
        [SerializeField] private Color outlineColor = Color.black;
        [Range(0f, 1f)]
        [SerializeField] private float outlineWidth = 0.25f;

        private const float PopDuration = 0.12f;

        private TextMeshPro _label;
        private float _timer;
        private Camera _camera;
        private DamageNumberSpawner _owner;
        private bool _active;
        private float _popTimer;
        private Vector3 _driftDirection;

        private void Awake()
        {
            _label = gameObject.AddComponent<TextMeshPro>();
            _label.alignment = TextAlignmentOptions.Center;
            _label.fontSize = fontSize;
            _label.fontStyle = fontStyle;
            _label.textWrappingMode = TextWrappingModes.NoWrap;
            _label.overflowMode = TextOverflowModes.Overflow;
            if (fontAsset != null)
                _label.font = fontAsset;

            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!_active)
                return;

            _timer -= Time.deltaTime;
            float progress = 1f - Mathf.Clamp01(_timer / Mathf.Max(0.001f, lifetime));

            UpdatePosition(progress);
            FaceCamera();
            UpdatePopScale();
            UpdateFade(progress);

            if (_timer <= 0f)
            {
                _active = false;
                _owner?.Return(this);
            }
        }

        public void ConfigureRuntime(Camera camera, DamageNumberSpawner owner)
        {
            _camera = camera;
            _owner = owner;
        }

        public void Play(float amount, Vector3 worldPosition, bool isCritical = false, bool isHeal = false)
        {
            float scatter = Random.Range(-horizontalScatter, horizontalScatter);
            transform.position = new Vector3(worldPosition.x + scatter, worldPosition.y, worldPosition.z);
            transform.localScale = scalePopOnSpawn ? Vector3.zero : Vector3.one;

            _timer = Mathf.Max(0.001f, lifetime);
            _popTimer = 0f;
            _active = true;
            _driftDirection = new Vector3(Random.value > 0.5f ? 1f : -1f, 0f, 0f);
            gameObject.SetActive(true);

            _label.text = isHeal
                ? showPlusOnHeal ? $"+{Mathf.CeilToInt(amount)}" : Mathf.CeilToInt(amount).ToString()
                : Mathf.CeilToInt(amount).ToString();

            Color baseColor = isHeal
                ? healColor
                : isCritical
                    ? criticalColor
                    : normalColor;
            _label.color = baseColor;
            _label.fontSize = isCritical ? fontSize * criticalSizeMultiplier : fontSize;
            _label.fontStyle = fontStyle;
            if (fontAsset != null)
                _label.font = fontAsset;

            _label.outlineWidth = useOutline ? outlineWidth : 0f;
            if (useOutline)
                _label.outlineColor = outlineColor;
        }

        private void UpdatePosition(float progress)
        {
            switch (riseStyle)
            {
                case RiseStyle.Drift:
                    transform.position += (_driftDirection * riseSpeed * 0.35f + Vector3.up * riseSpeed) * Time.deltaTime;
                    break;
                case RiseStyle.Arc:
                    float arcMultiplier = Mathf.Clamp01(1f - progress) * 2f;
                    transform.position += Vector3.up * riseSpeed * arcMultiplier * Time.deltaTime;
                    break;
                default:
                    transform.position += Vector3.up * riseSpeed * Time.deltaTime;
                    break;
            }
        }

        private void FaceCamera()
        {
            if (_camera != null)
                transform.rotation = _camera.transform.rotation;
        }

        private void UpdatePopScale()
        {
            if (!scalePopOnSpawn || _popTimer >= PopDuration)
                return;

            _popTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_popTimer / PopDuration);
            float scale = t < 0.5f
                ? Mathf.Lerp(0f, 1.2f, t * 2f)
                : Mathf.Lerp(1.2f, 1f, (t - 0.5f) * 2f);
            transform.localScale = Vector3.one * scale;
        }

        private void UpdateFade(float progress)
        {
            float fadeProgress = Mathf.Clamp01((progress - fadeStart) / Mathf.Max(0.001f, 1f - fadeStart));
            Color color = _label.color;
            color.a = 1f - fadeProgress;
            _label.color = color;
        }
    }
}
