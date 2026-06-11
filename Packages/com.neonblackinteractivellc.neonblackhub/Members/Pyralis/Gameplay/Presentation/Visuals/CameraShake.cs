using System.Collections;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Presentation.Visuals
{
    /// <summary>
    /// Canonical camera shake service for gameplay impact feedback.
    /// Add one to the bootstrap or camera rig; assign Target Transform for a custom rig.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.VFX,
        Relevance = "Canonical camera shake service for gameplay impact feedback.",
        Axioms = AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.Dimensions3D,
        RequiredInterfaces = new[] { typeof(ICameraShakeSink) },
        NativeSetup = new[]
        {
            "Add CameraShake to a Bootstrap child GameObject or assign to GameplaySessionBootstrap.",
            "Assign Target Transform (camera rig root or main camera).",
            "Set Default Shake Mode (Planar2D for most games)."
        },
        AssignmentFields = new[] { nameof(targetTransform), nameof(defaultShakeMode), nameof(positionInfluence), nameof(rotationInfluence) },
        FirstProof = "Calling Shake(intensity, duration) causes the target transform to vibrate.",
        ExpertAdvice = "2D path: use Planar2D and mostly position influence. 3D path: use Spatial3D or PositionAndRotation with lower intensity.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/visuals"
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Camera/Camera Shake")]
    public class CameraShake : MonoBehaviour, ICameraShakeSink
    {
        public enum ShakeMode
        {
            Planar2D,
            Spatial3D,
            RotationOnly,
            PositionAndRotation
        }

        [Header("Target")]
        [Tooltip("Transform to shake. Assign the camera rig root, virtual camera follow root, or Camera transform that should receive shake.")]
        [SerializeField] private Transform targetTransform;

        [Header("Defaults")]
        [Tooltip("Default movement style used by Shake(intensity, duration).")]
        [SerializeField] private ShakeMode defaultShakeMode = ShakeMode.Planar2D;

        [Tooltip("Position shake strength multiplier. 0 = no position shake, 1 = full.")]
        [Range(0f, 1f)]
        [SerializeField] private float positionInfluence = 1f;

        [Tooltip("Rotation shake strength multiplier. 0 = no rotation shake.")]
        [Range(0f, 1f)]
        [SerializeField] private float rotationInfluence = 0.1f;

        private Transform _activeTarget;
        private Vector3 _originalPos;
        private Quaternion _originalRot;
        private bool _isShaking;
        private float _currentIntensity;
        private Coroutine _shakeCoroutine;

        private void Awake()
        {
            ResolveTarget();
        }

        private void OnDisable()
        {
            StopShake();
        }

        public void Shake(float intensity, float duration) => Shake(intensity, duration, defaultShakeMode);

        public void Shake(float intensity, float duration, ShakeMode mode)
        {
            if (intensity <= 0f || duration <= 0f)
                return;

            Transform target = ResolveTarget();
            if (target == null)
                return;

            float scaled = intensity * positionInfluence;

            if (_isShaking && scaled <= _currentIntensity)
                return;

            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                RestoreTarget();
            }

            _activeTarget = target;
            _originalPos = target.position;
            _originalRot = target.rotation;
            _currentIntensity = scaled;
            _shakeCoroutine = StartCoroutine(ShakeRoutine(scaled, duration, mode));
        }

        public void StopShake()
        {
            if (_shakeCoroutine != null)
                StopCoroutine(_shakeCoroutine);

            if (_isShaking)
                RestoreTarget();

            _shakeCoroutine = null;
            _isShaking = false;
        }

        public void SetTarget(Transform target)
        {
            StopShake();
            targetTransform = target;
            _activeTarget = target;
        }

        private Transform ResolveTarget()
        {
            if (targetTransform != null)
                return _activeTarget = targetTransform;

            return _activeTarget = transform;
        }

        private IEnumerator ShakeRoutine(float intensity, float duration, ShakeMode mode)
        {
            _isShaking = true;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float strength = Mathf.Lerp(intensity, 0f, elapsed / duration);
                ApplyShakeFrame(strength, mode);
                yield return null;
            }

            RestoreTarget();
            _currentIntensity = 0f;
            _isShaking = false;
            _shakeCoroutine = null;
        }

        private void ApplyShakeFrame(float strength, ShakeMode mode)
        {
            Vector3 offset = Vector3.zero;
            Vector3 rotation = Vector3.zero;

            switch (mode)
            {
                case ShakeMode.Planar2D:
                    offset = (Vector3)Random.insideUnitCircle * strength;
                    break;
                case ShakeMode.Spatial3D:
                    offset = Random.insideUnitSphere * strength;
                    break;
                case ShakeMode.RotationOnly:
                    rotation = Random.insideUnitSphere * strength * rotationInfluence * 10f;
                    break;
                case ShakeMode.PositionAndRotation:
                    offset = Random.insideUnitSphere * strength;
                    rotation = Random.insideUnitSphere * strength * rotationInfluence * 10f;
                    break;
            }

            _activeTarget.position = _originalPos + offset;
            _activeTarget.rotation = Quaternion.Euler(_originalRot.eulerAngles + rotation);
        }

        private void RestoreTarget()
        {
            if (_activeTarget == null)
                return;

            _activeTarget.position = _originalPos;
            _activeTarget.rotation = _originalRot;
        }

        public void Initialize()
        {
        }

        public void Shutdown()
        {
            if (_shakeCoroutine != null)
                StopCoroutine(_shakeCoroutine);

            if (_isShaking)
                RestoreTarget();

            _shakeCoroutine = null;
            _isShaking = false;
        }
    }
}
