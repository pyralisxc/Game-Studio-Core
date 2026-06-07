using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Features.Composition;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Enemies
{
    [AddComponentMenu("NeonBlack/Gameplay/Enemies/Enemy Ambient Feature Runtime")]
    public class EnemyAmbientFeatureRuntime : MonoBehaviour, IFeatureModuleRuntime
    {
        [SerializeField] private EnemyAmbientFeatureProfile ambientProfile;
        private ActorFeatureContext _context;
        private EnemyAI _enemyAI;
        private IEnemyReactionState _reactionState;
        private float _lookAroundTimer;

        public string ModuleId => "enemy.ambient";

        private void Update()
        {
            if (_context == null || ambientProfile == null || !ambientProfile.enableAmbientLookAround || _enemyAI == null)
                return;

            if (ambientProfile.requirePatrolState && !_enemyAI.IsPatrolling)
                return;

            if (ambientProfile.suppressDuringReactionLock && _reactionState != null && _reactionState.IsReactionLocked)
                return;

            _lookAroundTimer -= Time.deltaTime;
            if (_lookAroundTimer > 0f)
                return;

            _lookAroundTimer = ambientProfile.lookAroundInterval;
            _context.Animation?.TriggerSignal(ActorAnimationSignal.LookAround);
        }

        public void InitializeFeature(FeatureRuntimeInitializationContext initializationContext)
        {
            ActorFeatureContext context = initializationContext != null ? initializationContext.ActorContext : null;
            FeatureModuleDefinition definition = initializationContext != null ? initializationContext.Definition : null;
            _context = context;
            _enemyAI = context != null ? context.EnemyActorState as EnemyAI : null;
            ambientProfile = initializationContext.GetProfile<EnemyAmbientFeatureProfile>(definition != null ? definition.profileAsset : null);
            ambientProfile?.Sanitize();
            _lookAroundTimer = ambientProfile != null ? ambientProfile.lookAroundInterval : 0f;

            ActorFeatureHost host = GetComponentInParent<ActorFeatureHost>();
            if (host != null)
                host.TryGetInstalledFeature(out _reactionState);
        }

        public void ShutdownFeature()
        {
            _context = null;
            _enemyAI = null;
            _reactionState = null;
        }
    }
}
