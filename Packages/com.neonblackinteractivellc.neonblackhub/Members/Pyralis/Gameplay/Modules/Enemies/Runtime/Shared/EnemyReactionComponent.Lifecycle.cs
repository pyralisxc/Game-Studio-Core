using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Enemies
{
    public partial class EnemyReactionComponent
    {
        private void Awake()
        {
            _health = GetComponent<IActorHealthState>();
            _knockback = GetComponent<IActorKnockbackController>();
            _feedbackPublisher = GetComponent<IActorFeedbackPublisher>();
            _animation = GetComponent<IActorAnimationController>();
            _hitPauseSink = ResolveHitPauseSink();
            _cameraShakeSink = ResolveCameraShakeSink();

            if (reactionProfile != null)
                reactionProfile.Sanitize();

            if (_health != null)
            {
                _health.Damaged += HandleDamaged;
                _health.Died += HandleDeath;
            }
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.Damaged -= HandleDamaged;
                _health.Died -= HandleDeath;
            }

            _health = null;
            _knockback = null;
            _feedbackPublisher = null;
            _animation = null;
            _hitPauseSink = null;
            _cameraShakeSink = null;
            _reactionLockTimer = 0f;
        }

        public void SetImpactFeedbackSinks(IHitPauseSink hitPause, ICameraShakeSink cameraShake)
        {
            _hitPauseSink = hitPause;
            _cameraShakeSink = cameraShake;
            hitPauseSink = hitPause as MonoBehaviour;
            cameraShakeSink = cameraShake as MonoBehaviour;
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
