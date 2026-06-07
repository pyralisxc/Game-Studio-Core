using System.Collections;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    [AddComponentMenu("NeonBlack/Gameplay/Combat/Projectile 2D")]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class Projectile2D : MonoBehaviour, IProjectileRuntimeBody
    {
        [Header("Impact Feedback")]
        [SerializeField] private MonoBehaviour hitPauseSink;
        [SerializeField] private MonoBehaviour cameraShakeSink;

        private Rigidbody2D _body;
        private Collider2D _collider;
        private ProjectileSpawnCommand _command;
        private IHitPauseSink _hitPauseSink;
        private ICameraShakeSink _cameraShakeSink;
        private ProjectilePoolHandle _poolHandle;
        private Coroutine _lifetimeRoutine;
        private Vector3 _origin;
        private bool _hasHit;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _collider = GetComponent<Collider2D>();
            _collider.isTrigger = true;
            _hitPauseSink = hitPauseSink as IHitPauseSink;
            _cameraShakeSink = cameraShakeSink as ICameraShakeSink;
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

            _command = command;
            _origin = transform.position;
            _hasHit = false;
            _poolHandle = GetComponent<ProjectilePoolHandle>();
            _hitPauseSink = hitPause ?? _hitPauseSink;
            _cameraShakeSink = cameraShake ?? _cameraShakeSink;

            Vector3 direction = command.Direction.sqrMagnitude > 0f ? command.Direction.normalized : Vector3.right;
            _body.linearVelocity = (Vector2)direction * command.Speed;

            if (command.Lifetime > 0f)
                _lifetimeRoutine = StartCoroutine(LifetimeRoutine());
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

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_hasHit)
                return;

            if (_command.Owner != null && (other.gameObject == _command.Owner || other.transform.IsChildOf(_command.Owner.transform)))
                return;

            HealthComponent health = other.GetComponentInParent<HealthComponent>();
            if (health != null)
            {
                if (!_command.AllowFriendlyFire
                    && health.faction != Faction.Neutral
                    && health.faction == _command.SourceFaction)
                {
                    return;
                }

                _hasHit = true;
                bool damaged = _command.Damage > 0f && health.TryTakeDamage(_command.Damage, transform.position, _command.Owner);
                if (damaged && _command.Knockback > 0f && other.attachedRigidbody != null)
                {
                    Vector2 direction = ((Vector2)health.transform.position - (Vector2)transform.position).normalized;
                    if (direction == Vector2.zero)
                        direction = _body.linearVelocity.sqrMagnitude > 0f ? _body.linearVelocity.normalized : Vector2.right;

                    other.attachedRigidbody.AddForce(direction * _command.Knockback, ForceMode2D.Impulse);
                }

                ApplyImpactResult(damaged
                    ? ProjectileSpawnResult.Hit(health.gameObject, transform.position)
                    : ProjectileSpawnResult.Ignored("Projectile hit was rejected by target health."));
            }
            else
            {
                _hasHit = true;
                ApplyImpactResult(ProjectileSpawnResult.Hit(other.gameObject, transform.position));
            }

            Retire();
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

        private void ApplyImpactResult(ProjectileSpawnResult result)
        {
            ProjectileImpactEffectPlayer.Apply(_command.ImpactDefinition, result, _command, _hitPauseSink, _cameraShakeSink);
        }

        private void Retire()
        {
            if (_lifetimeRoutine != null)
            {
                StopCoroutine(_lifetimeRoutine);
                _lifetimeRoutine = null;
            }

            if (_body != null)
            {
                _body.linearVelocity = Vector2.zero;
                _body.angularVelocity = 0f;
            }

            if (_poolHandle != null && _poolHandle.ReleaseToPool())
                return;

            Destroy(gameObject);
        }
    }
}
