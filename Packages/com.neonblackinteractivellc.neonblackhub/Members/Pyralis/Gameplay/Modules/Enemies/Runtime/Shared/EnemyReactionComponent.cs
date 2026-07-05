using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Types.Animation;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Enemies
{
    [AddComponentMenu("NeonBlack/Gameplay/Enemies/Enemy Reaction Component")]
    [AuthoringContract(
        StableId = "enemy.reaction.component",
        Category = "Combat",
        CapabilityPath = "Combat/Actions/Enemy Reaction Component",
        Surface = AuthoringSurface.Profile,
        RequiredFields = new[] { nameof(reactionProfile) },
        RequiredComponentNames = new[] { "NeonBlack.Gameplay.Modules.Enemies.EnemyAI", "NeonBlack.Gameplay.Modules.Combat.HealthComponent" },
        RequiredInterfaces = new[] { typeof(IEnemyReactionState) },
        SetupSteps = new[]
        {
            "add EnemyReactionComponent to the enemy root",
            "assign EnemyReactionProfile",
            "assign hit pause and camera shake sinks only when enemy reactions should drive those optional feedback outputs"
        },
        SuccessChecks = new[] { "Verify that hit pause and camera shake are triggered when the enemy takes damage." },
        Tags = new[] { "capability:Combat", "lane:Enemy" },
        Selectable = false
    )]
    public partial class EnemyReactionComponent : GameplayTickBehaviour, IEnemyReactionState
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
        protected override GameplayTickDomain TickDomain => GameplayTickDomain.Enemies;
        protected override bool UsesGameplayTick => true;

        protected override void OnGameplayTick(in GameplayTickContext context)
        {
            if (_reactionLockTimer > 0f)
                _reactionLockTimer -= context.DeltaTime;
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
