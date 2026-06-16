using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Composition;
using VContainer;

namespace NeonBlack.Gameplay.Features.Enemies
{
    public partial class EnemyAI
    {
        private IObjectResolver _resolver;

        [Inject]
        public void Construct(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        private void ApplyFeatureProfile(EnemyFeatureProfile profile)
        {
            if (profile == null) return;
            if (profile.combatProfile != null) CombatModule.ApplyCombatProfile(profile.combatProfile);
        }

        private void InitializeFeatureModules()
        {
            FeatureModuleDefinition[] definitions = enemyFeatureProfile != null ? enemyFeatureProfile.featureModules : null;
            if (definitions == null || definitions.Length == 0) return;
            ActorFeatureHost featureHost = _runtime.EnsureFeatureHost();
            featureHost.InitializeFeatures(new FeatureHostInitializationContext(_runtime.BuildFeatureContext(enemyFeatureProfile, this), _resolver), definitions);
            featureHost.TryGetInstalledFeature(out _reactionState);
        }
    }
}
