using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Modules.Actor.Composition;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Enemies
{
    public partial class EnemyReactionFeatureRuntime
    {
        public void InitializeFeature(FeatureRuntimeInitializationContext initializationContext)
        {
            ActorFeatureContext context = initializationContext != null ? initializationContext.ActorContext : null;
            FeatureModuleDefinition definition = initializationContext != null ? initializationContext.Definition : null;
            _context = context;
            reactionProfile = initializationContext != null
                ? initializationContext.GetProfile<EnemyReactionProfile>(definition != null ? definition.profileAsset : null)
                : null;
            _health = context != null ? context.Health : null;
            _knockback = context != null ? context.Knockback : null;
            _feedbackPublisher = context != null && context.ActorObject != null
                ? context.ActorObject.GetComponent<IActorFeedbackPublisher>()
                : null;
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
