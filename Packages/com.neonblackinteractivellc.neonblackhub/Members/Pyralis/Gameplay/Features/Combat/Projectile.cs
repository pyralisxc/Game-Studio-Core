using System.Collections;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
/// <summary>
/// Self-propelled 3D projectile body for ProjectileDefinition prefab delivery.
/// Attach to a prefab alongside a Rigidbody and a trigger Collider.
///
/// Setup:
/// 1. Create a prefab with: Rigidbody + any trigger Collider (e.g. SphereCollider IsTrigger=true) + this component.
/// 2. On the Rigidbody, set Collision Detection to Continuous and uncheck Use Gravity (this script controls gravity).
/// 3. Assign the prefab to a ProjectileDefinition, then assign that definition to a Ranged or Thrown WeaponData asset.
/// 4. Put impact VFX/SFX on ProjectileImpactDefinition, not on this prefab body.
/// 5. On the pawn combat component, assign a Projectile Spawn Point child Transform (e.g. a hand/muzzle empty GameObject).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour, IProjectileRuntimeBody
{
    [Header("Motion")]
    [Tooltip("When true, gravity pulls the projectile downward, creating an arc. " +
             "When false, the projectile travels in a perfectly straight line.")]
    [SerializeField] private bool useArc = false;

    [Tooltip("Multiplier applied to physics gravity when Use Arc is enabled. " +
             "Higher values drop the projectile faster.")]
    [SerializeField] private float gravityScale = 1f;

    [Header("Impact")]
    [SerializeField] private MonoBehaviour hitPauseSink;
    [SerializeField] private MonoBehaviour cameraShakeSink;

    // Runtime state.
    private Rigidbody   _rb;
    private ProjectileSpawnCommand _command;
    private bool        _hasHit;   // guards against multiple OnTriggerEnter calls in the same frame
    private IHitPauseSink _hitPauseSink;
    private ICameraShakeSink _cameraShakeSink;
    private Coroutine _lifetimeRoutine;
    private ProjectilePoolHandle _poolHandle;
    private Vector3 _origin;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;   // we handle gravity manually to support gravityScale
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        _hitPauseSink = ResolveHitPauseSink();
        _cameraShakeSink = ResolveCameraShakeSink();
    }

    private void OnDisable()
    {
        if (_lifetimeRoutine != null)
        {
            StopCoroutine(_lifetimeRoutine);
            _lifetimeRoutine = null;
        }
    }

    public void Launch(ProjectileSpawnCommand command, IHitPauseSink hitPause = null, ICameraShakeSink cameraShake = null)
    {
        if (_lifetimeRoutine != null)
            StopCoroutine(_lifetimeRoutine);

        _hasHit = false;
        _command = command;
        _origin = transform.position;
        _hitPauseSink = hitPause ?? ResolveHitPauseSink();
        _cameraShakeSink = cameraShake ?? ResolveCameraShakeSink();
        _poolHandle = GetComponent<ProjectilePoolHandle>();

        Vector3 direction = command.Direction.sqrMagnitude > 0f ? command.Direction.normalized : transform.forward;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        _rb.linearVelocity = direction * command.Speed;

        if (command.Lifetime > 0f)
            _lifetimeRoutine = StartCoroutine(LifetimeRoutine());
    }

    public void SetImpactFeedbackSinks(IHitPauseSink hitPause, ICameraShakeSink cameraShake)
    {
        _hitPauseSink = hitPause;
        _cameraShakeSink = cameraShake;
        hitPauseSink = hitPause as MonoBehaviour;
        cameraShakeSink = cameraShake as MonoBehaviour;
    }

    private void FixedUpdate()
    {
        if (!useArc) return;
        // Apply scaled gravity manually so we can tune the arc without Unity's fixed gravity.
        _rb.linearVelocity += Vector3.down * (Physics.gravity.magnitude * gravityScale * Time.fixedDeltaTime);
    }

    private void Update()
    {
        if (_hasHit || _command.MaxDistance <= 0f)
            return;

        if ((transform.position - _origin).sqrMagnitude >= _command.MaxDistance * _command.MaxDistance)
        {
            ApplyImpactResult(ProjectileSpawnResult.Missed());
            Retire();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasHit) return;

        // Never hit the owner.
        if (_command.Owner != null && other.gameObject == _command.Owner) return;
        if (_command.Owner != null && other.transform.IsChildOf(_command.Owner.transform)) return;

        // Damage any HealthComponent on the hit object.
        if (other.GetComponentInParent<HealthComponent>() is HealthComponent health)
        {
            // Faction check: same-faction targets ignore the projectile (mirrors HitBox behaviour).
            if (!_command.AllowFriendlyFire
                && health.faction != Faction.Neutral
                && health.faction == _command.SourceFaction)
            {
                return;
            }

            _hasHit = true;
            bool damaged = _command.Damage > 0f && health.TryTakeDamage(_command.Damage, transform.position, _command.Owner);

            // Knockback.
            if (damaged && _command.Knockback > 0f)
            {
                var kb = health.GetComponent<KnockbackReceiver>();
                if (kb != null)
                {
                    Vector3 dir = health.transform.position - transform.position;
                    dir.y = 0f;
                    if (dir.sqrMagnitude < 0.001f) dir = transform.forward;
                    dir        = dir.normalized;
                    dir.y      = 0.25f;
                    kb.ApplyKnockback(dir * _command.Knockback);
                }
            }

            ApplyImpactResult(damaged
                ? ProjectileSpawnResult.Hit(health.gameObject, transform.position)
                : ProjectileSpawnResult.Ignored("Projectile hit was rejected by target health."));
        }
        else
        {
            // Hit something solid but non-damageable (wall, terrain).
            _hasHit = true;
            ApplyImpactResult(ProjectileSpawnResult.Hit(other.gameObject, transform.position));
        }

        Retire();
    }

    private void ApplyImpactResult(ProjectileSpawnResult result)
    {
        ProjectileImpactEffectPlayer.Apply(_command.ImpactDefinition, result, _command, ResolveHitPauseSink(), ResolveCameraShakeSink());
    }

    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(_command.Lifetime);
        _lifetimeRoutine = null;
        if (!_hasHit)
        {
            ApplyImpactResult(ProjectileSpawnResult.Missed());
            Retire();
        }
    }

    private void Retire()
    {
        if (_lifetimeRoutine != null)
        {
            StopCoroutine(_lifetimeRoutine);
            _lifetimeRoutine = null;
        }

        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        if (_poolHandle != null && _poolHandle.ReleaseToPool())
            return;

        Destroy(gameObject);
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
}
}
