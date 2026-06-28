using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Types.Animation;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Enemies
{
    [AddComponentMenu("NeonBlack/Gameplay/Enemies/Enemy Reaction Component")]
    [AuthoringContract(
        Capability = AuthoringCapability.Combat,
        ModuleId = "enemy.reaction",
        Lane = "Enemy",
        ProfileType = typeof(EnemyReactionProfile),
        RequiredInterfaces = new[] { typeof(IEnemyReactionState) },
        RequiredComponentNames = new[] { "NeonBlack.Gameplay.Modules.Enemies.EnemyAI", "NeonBlack.Gameplay.Modules.Combat.HealthComponent" },
        AssignmentFields = new[] { nameof(reactionProfile), nameof(hitPauseSink), nameof(cameraShakeSink) },
        Proof = "Verify that hit pause and camera shake are triggered when the enemy takes damage.",
        NativeSetup = new[]
        {
            "add EnemyReactionComponent to the enemy root",
            "assign EnemyReactionProfile"
        },
        CustomizationMoments = new[]
        {
            "EnemyReactionProfile.enableReactions",
            "EnemyReactionProfile.staggerDamageThreshold",
            "EnemyReactionProfile.hitPauseDuration"
        },
        CapabilityPath = "Combat/Actions/Enemy Reaction Component"
    )]
    public partial class EnemyReactionComponent : MonoBehaviour, IEnemyReactionState
{
        [SerializeField] private EnemyReactionProfile reactionProfile;
        [SerializeField] private MonoBehaviour hitPauseSink;
        [SerializeField] private MonoBehaviour cameraShakeSink;
        private IActorHealthState _health;
        private IActorKnockbackController _knockback;
        private IActorFeedbackPublisher _feedbackPublisher;
        private IActorAnimationController _animation;
        private IHitPauseSink _hitPauseSink;
        private ICameraShakeSink _cameraShakeSink;
        private float _reactionLockTimer;

        public bool IsReactionLocked => _reactionLockTimer > 0f;

        private void Update()
        {
            if (_reactionLockTimer > 0f)
                _reactionLockTimer -= Time.deltaTime;
        }

        private void HandleDamaged(float damage)
        {
            if (reactionProfile == null || !reactionProfile.enableReactions)
                return;

            bool shouldStagger = damage >= reactionProfile.staggerDamageThreshold;
            _reactionLockTimer = Mathf.Max(_reactionLockTimer, shouldStagger ? reactionProfile.staggerLockDuration : reactionProfile.hurtLockDuration);

            if (reactionProfile.hitPauseDuration > 0f)
                ResolveHitPauseSink()?.Freeze(reactionProfile.hitPauseDuration);

            if (reactionProfile.cameraShakeIntensity > 0f && reactionProfile.cameraShakeDuration > 0f)
                ResolveCameraShakeSink()?.Shake(reactionProfile.cameraShakeIntensity, reactionProfile.cameraShakeDuration);

            if (_animation != null)
            {
                if (shouldStagger)
                {
                    _animation.TriggerSignal(ActorAnimationSignal.Stagger);
                    _feedbackPublisher?.PublishStagger(damage);
                }
                else
                    _animation.TriggerSignal(ActorAnimationSignal.Hurt);
            }
        }

        private void HandleDeath()
        {
            if (reactionProfile != null && reactionProfile.clearKnockbackOnDeath)
                _knockback?.ClearKnockback();
        }
    }
}
