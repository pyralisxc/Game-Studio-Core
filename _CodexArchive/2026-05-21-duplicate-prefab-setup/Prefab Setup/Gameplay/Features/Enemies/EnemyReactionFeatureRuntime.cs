using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Presentation.Visuals;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Runtime;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Composition;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Enemies
{
    [AddComponentMenu("NeonBlack/Gameplay/Enemies/Enemy Reaction Feature Runtime")]
    public class EnemyReactionFeatureRuntime : MonoBehaviour, IFeatureModuleRuntime, IEnemyReactionState
    {
        [SerializeField] private EnemyReactionProfile reactionProfile;
        private ActorFeatureContext _context;
        private IActorHealthState _health;
        private KnockbackReceiver _knockback;
        private IActorFeedbackPublisher _feedbackPublisher;
        private float _reactionLockTimer;

        public string ModuleId => "enemy.reaction";
        public bool IsReactionLocked => _reactionLockTimer > 0f;

        private void Update()
        {
            if (_reactionLockTimer > 0f)
                _reactionLockTimer -= Time.deltaTime;
        }

        public void InitializeFeature(FeatureRuntimeInitializationContext initializationContext)
        {
            ActorFeatureContext context = initializationContext != null ? initializationContext.ActorContext : null;
            FeatureModuleDefinition definition = initializationContext != null ? initializationContext.Definition : null;
            _context = context;
            reactionProfile = initializationContext.GetProfile<EnemyReactionProfile>(definition != null ? definition.profileAsset : null);
            _health = context.Health;
            _knockback = context.Knockback as KnockbackReceiver;
            _feedbackPublisher = context != null && context.ActorObject != null
                ? context.ActorObject.GetComponent<IActorFeedbackPublisher>()
                : null;

            if (reactionProfile != null)
                reactionProfile.Sanitize();

            if (_health != null)
            {
                _health.Damaged += HandleDamaged;
                _health.Died += HandleDeath;
            }
        }

        public void ShutdownFeature()
        {
            if (_health != null)
            {
                _health.Damaged -= HandleDamaged;
                _health.Died -= HandleDeath;
            }

            _context = null;
            _health = null;
            _knockback = null;
            _feedbackPublisher = null;
            _reactionLockTimer = 0f;
        }

        private void HandleDamaged(float damage)
        {
            if (reactionProfile == null || !reactionProfile.enableReactions || _context == null)
                return;

            bool shouldStagger = damage >= reactionProfile.staggerDamageThreshold;
            _reactionLockTimer = Mathf.Max(_reactionLockTimer, shouldStagger ? reactionProfile.staggerLockDuration : reactionProfile.hurtLockDuration);

            if (reactionProfile.hitPauseDuration > 0f && TimeManager.Instance != null)
                TimeManager.Instance.Freeze(reactionProfile.hitPauseDuration);

            if (reactionProfile.cameraShakeIntensity > 0f && CameraShake.Instance != null)
                CameraShake.Instance.Shake(reactionProfile.cameraShakeIntensity, reactionProfile.cameraShakeDuration);

            if (_context.Animation != null)
            {
                if (shouldStagger)
                {
                    _context.Animation.TriggerSignal(ActorAnimationSignal.Stagger);
                    _feedbackPublisher?.PublishStagger(damage);
                }
                else
                    _context.Animation.TriggerSignal(ActorAnimationSignal.Hurt);
            }
        }

        private void HandleDeath()
        {
            if (reactionProfile != null && reactionProfile.clearKnockbackOnDeath)
                _knockback?.ClearKnockback();
        }
    }
}
