using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Runtime;
using NeonBlack.Gameplay.Presentation.Visuals;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    /// <summary>
    /// Overlap-query hitbox. The BoxCollider or SphereCollider on this GameObject
    /// is used as a sizing volume and gizmo only; it is disabled at runtime.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.CombatSensors,
        Priority = AuthoringPriority.Primary,
        Lane = "Combat",
        Relevance = "One-shot overlap query hitbox for melee and projectile impacts.",
        Axioms = AuthoringWorldAxiom.Realtime | AuthoringWorldAxiom.Dimensions3D,
        NativeSetup = new[] { "Add to a child GameObject of a combat actor.", "Assign a Box or Sphere collider (it will be disabled at runtime)." },
        AssignmentFields = new[] { nameof(owner), nameof(hitFXPrefab), nameof(hitPauseSink), nameof(cameraShakeSink) },
        FirstProof = "Trigger Fire() via an animation event or script and verify it detects HealthComponents in the overlap volume.",
        ExpertAdvice = "HitBoxes are disabled colliders used only for overlap queries. Ensure the owner is set for correct knockback calculation. Use hitPauseSink for juicy combat feel.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/hitbox"
    )]
[RequireComponent(typeof(Collider))]
    public class HitBox : MonoBehaviour
    {
        public event Action<GameObject> HitConfirmed;

        [Header("Owner")]
        [Tooltip("Root GameObject of the attacker. Used for faction checks and knockback direction.")]
        [SerializeField] private GameObject owner;

        [Header("Hit FX")]
        [Tooltip("Optional particle or VFX prefab spawned at the hit point.")]
        [SerializeField] private GameObject hitFXPrefab;

        [Tooltip("Optional audio clip played on hit.")]
        [SerializeField] private AudioClip hitSFX;

        [Header("Hit Pause")]
        [Tooltip("Seconds to freeze time on hit. 0 = disabled. 0.04-0.08 = light hit, 0.10-0.15 = heavy.")]
        [SerializeField] private float freezeFrameDuration = 0.06f;
        [Tooltip("Optional service that handles hit pause. Assign TimeManager or another component implementing IHitPauseSink.")]
        [SerializeField] private MonoBehaviour hitPauseSink;

        [Header("Camera Shake")]
        [Tooltip("Peak displacement in world units on hit. 0 = disabled. 0.10-0.20 = punch, 0.25-0.40 = heavy.")]
        [SerializeField] private float cameraShakeIntensity = 0.15f;

        [Tooltip("Seconds the camera shake lasts on hit.")]
        [SerializeField] private float cameraShakeDuration = 0.12f;
        [Tooltip("Optional service that handles camera shake. Assign CameraShake or another component implementing ICameraShakeSink.")]
        [SerializeField] private MonoBehaviour cameraShakeSink;

        [Header("Enemy AI Range Override")]
        [Tooltip("If enabled, EnemyAI can use this hitbox-specific range instead of global/per-attack range.")]
        [SerializeField] private bool enableEnemyAttackRangeOverride = false;

        [Tooltip("Hitbox-specific range used by EnemyAI when the override is enabled.")]
        [SerializeField] private float enemyAttackRangeOverride = 1.5f;

        private Collider _col;
        private AudioSource _audio;
        private IHitPauseSink _hitPauseSink;
        private ICameraShakeSink _cameraShakeSink;
        private readonly HashSet<GameObject> _hitThisSwing = new HashSet<GameObject>();
        private static readonly Collider[] OverlapBuffer = new Collider[16];

        private void Awake()
        {
            _col = GetComponent<Collider>();
            _audio = GetComponentInParent<AudioSource>();
            owner ??= GetComponentInParent<HealthComponent>()?.gameObject;
            _hitPauseSink = ResolveHitPauseSink();
            _cameraShakeSink = ResolveCameraShakeSink();

            // Keep the collider disabled because it is only a query-volume reference.
            _col.enabled = false;
        }

        public void SetHitPauseSink(IHitPauseSink sink)
        {
            _hitPauseSink = sink;
            hitPauseSink = sink as MonoBehaviour;
        }

        public void SetCameraShakeSink(ICameraShakeSink sink)
        {
            _cameraShakeSink = sink;
            cameraShakeSink = sink as MonoBehaviour;
        }

        /// <summary>
        /// Executes a one-shot overlap query at this hitbox's current world position and size.
        /// Call this at the exact hit frame of an attack.
        /// </summary>
        public void Fire(float damage, float knockback)
        {
            _hitThisSwing.Clear();
#if UNITY_EDITOR
            _gizmoArmedUntil = Time.realtimeSinceStartup + GizmoFlashDuration;
#endif

            int count = QueryOverlap();
            for (int i = 0; i < count; i++)
                ProcessHit(OverlapBuffer[i], damage, knockback);
        }

        /// <summary>
        /// Clears the per-swing hit set without firing a query. Use before a
        /// continuous effect that calls FireAdditive repeatedly.
        /// </summary>
        public void ClearHitSet()
        {
            _hitThisSwing.Clear();
        }

        /// <summary>
        /// Like Fire, but does not reset the per-swing hit set on entry.
        /// </summary>
        public void FireAdditive(float damage, float knockback)
        {
#if UNITY_EDITOR
            _gizmoArmedUntil = Time.realtimeSinceStartup + GizmoFlashDuration;
#endif

            int count = QueryOverlap();
            for (int i = 0; i < count; i++)
                ProcessHit(OverlapBuffer[i], damage, knockback);
        }

        public bool TryGetEnemyAttackRangeOverride(out float range)
        {
            if (enableEnemyAttackRangeOverride)
            {
                range = Mathf.Max(0.1f, enemyAttackRangeOverride);
                return true;
            }

            range = 0f;
            return false;
        }

        private int QueryOverlap()
        {
            if (_col == null)
                _col = GetComponent<Collider>();

            if (_col is BoxCollider box)
            {
                Vector3 worldCenter = transform.TransformPoint(box.center);
                Vector3 halfExtents = Vector3.Scale(box.size * 0.5f, transform.lossyScale);
                return Physics.OverlapBoxNonAlloc(
                    worldCenter,
                    halfExtents,
                    OverlapBuffer,
                    transform.rotation,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore);
            }

            if (_col is SphereCollider sphere)
            {
                Vector3 worldCenter = transform.TransformPoint(sphere.center);
                float worldRadius = sphere.radius * Mathf.Max(
                    transform.lossyScale.x,
                    transform.lossyScale.y,
                    transform.lossyScale.z);

                return Physics.OverlapSphereNonAlloc(
                    worldCenter,
                    worldRadius,
                    OverlapBuffer,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore);
            }

            return 0;
        }

        private void ProcessHit(Collider other, float damage, float knockback)
        {
            HealthComponent health = other.GetComponentInParent<HealthComponent>();
            if (health == null)
                return;

            if (IsOwnerHealth(health))
                return;

            if (_hitThisSwing.Contains(health.gameObject))
                return;

            Vector3 hitPoint = other.ClosestPoint(transform.position);
            if (!health.TryTakeDamage(damage, hitPoint, owner))
                return;

            _hitThisSwing.Add(health.gameObject);

            if (freezeFrameDuration > 0f)
                ResolveHitPauseSink()?.Freeze(freezeFrameDuration);

            if (cameraShakeIntensity > 0f && cameraShakeDuration > 0f)
                ResolveCameraShakeSink()?.Shake(cameraShakeIntensity, cameraShakeDuration);

            ApplyKnockback(health, knockback);
            PlayFeedback(hitPoint);
            HitConfirmed?.Invoke(health.gameObject);
        }

        private bool IsOwnerHealth(HealthComponent health)
        {
            if (health == null || owner == null)
                return false;

            return health.gameObject == owner
                || health.transform.IsChildOf(owner.transform)
                || owner.transform.IsChildOf(health.transform);
        }

        private void ApplyKnockback(HealthComponent health, float knockback)
        {
            if (knockback <= 0f)
                return;

            KnockbackReceiver receiver = health.GetComponent<KnockbackReceiver>();
            if (receiver == null)
                return;

            Vector3 attackerRoot = owner != null ? owner.transform.position : transform.position;
            Vector3 victimRoot = health.gameObject.transform.position;
            Vector3 direction = victimRoot - attackerRoot;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
                direction = transform.forward;

            direction = direction.normalized;
            direction.y = 0.25f;
            receiver.ApplyKnockback(direction * knockback);
        }

        private void PlayFeedback(Vector3 hitPoint)
        {
            if (hitFXPrefab != null)
                Instantiate(hitFXPrefab, hitPoint, Quaternion.identity);

            if (hitSFX != null && _audio != null)
                _audio.PlayOneShot(hitSFX);
        }

        private IHitPauseSink ResolveHitPauseSink()
        {
            if (_hitPauseSink != null)
                return _hitPauseSink;

            _hitPauseSink = hitPauseSink as IHitPauseSink;
            return _hitPauseSink;
        }

        private ICameraShakeSink ResolveCameraShakeSink()
        {
            if (_cameraShakeSink != null)
                return _cameraShakeSink;

            _cameraShakeSink = cameraShakeSink as ICameraShakeSink;
            return _cameraShakeSink;
        }

#if UNITY_EDITOR
        private float _gizmoArmedUntil;
        private const float GizmoFlashDuration = 0.15f;

        private void OnDrawGizmos()
        {
            if (_col == null)
                _col = GetComponent<Collider>();

            bool armed = Time.realtimeSinceStartup < _gizmoArmedUntil;
            Gizmos.color = armed
                ? new Color(1f, 0f, 0f, 0.55f)
                : new Color(1f, 0.5f, 0f, 0.15f);

            if (_col is BoxCollider box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (_col is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(transform.TransformPoint(sphere.center), sphere.radius);
            }
        }
#endif
    }
}
