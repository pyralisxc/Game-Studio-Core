using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Composition;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Enemies
{
    [AddComponentMenu("NeonBlack/Gameplay/Enemies/Enemy Reaction Feature Runtime")]
    [AuthoringContract(
        Capability = AuthoringCapability.Combat,
        ModuleId = "enemy.reaction",
        Lane = "Enemy",
        ProfileType = typeof(EnemyReactionProfile),
        RequiredInterfaces = new[] { typeof(IFeatureModuleRuntime), typeof(IEnemyReactionState) },
        RequiredComponentNames = new[] { "NeonBlack.Gameplay.Features.Enemies.EnemyAI", "NeonBlack.Gameplay.Features.Combat.HealthComponent" },
        AssignmentFields = new[] { nameof(reactionProfile), nameof(hitPauseSink), nameof(cameraShakeSink) },
        FirstProof = "Verify that hit pause and camera shake are triggered when the enemy takes damage.",
        NativeSetup = new[]
        {
            "create EnemyReactionProfile",
            "create FeatureModuleDefinition",
            "assign runtime prefab with EnemyReactionFeatureRuntime",
            "assign profile asset",
            "add module to EnemyFeatureProfile.featureModules"
        },
        CustomizationMoments = new[]
        {
            "EnemyReactionProfile.enableReactions",
            "EnemyReactionProfile.staggerDamageThreshold",
            "EnemyReactionProfile.hitPauseDuration"
        }
    )]
    public partial class EnemyReactionFeatureRuntime : MonoBehaviour, IFeatureModuleRuntime, IEnemyReactionState
{
        [SerializeField] private EnemyReactionProfile reactionProfile;
        [SerializeField] private MonoBehaviour hitPauseSink;
        [SerializeField] private MonoBehaviour cameraShakeSink;
        private ActorFeatureContext _context;
        private IActorHealthState _health;
        private KnockbackReceiver _knockback;
        private IActorFeedbackPublisher _feedbackPublisher;
        private IHitPauseSink _hitPauseSink;
        private ICameraShakeSink _cameraShakeSink;
        private float _reactionLockTimer;

        public string ModuleId => "enemy.reaction";
        public bool IsReactionLocked => _reactionLockTimer > 0f;

        private void Update()
        {
            if (_reactionLockTimer > 0f)
                _reactionLockTimer -= Time.deltaTime;
        }

        private void HandleDamaged(float damage)
        {
            if (reactionProfile == null || !reactionProfile.enableReactions || _context == null)
                return;

            bool shouldStagger = damage >= reactionProfile.staggerDamageThreshold;
            _reactionLockTimer = Mathf.Max(_reactionLockTimer, shouldStagger ? reactionProfile.staggerLockDuration : reactionProfile.hurtLockDuration);

            if (reactionProfile.hitPauseDuration > 0f)
                ResolveHitPauseSink()?.Freeze(reactionProfile.hitPauseDuration);

            if (reactionProfile.cameraShakeIntensity > 0f && reactionProfile.cameraShakeDuration > 0f)
                ResolveCameraShakeSink()?.Shake(reactionProfile.cameraShakeIntensity, reactionProfile.cameraShakeDuration);

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
