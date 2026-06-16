using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Characters
{
    public partial class PawnRoot
    {
        private void InstallFeatureModules()
        {
            _runtime ??= PawnRootRuntimeReferences.Capture(gameObject);
            ActorFeatureHost featureHost = _runtime.EnsureFeatureHost();

            featureHost.InitializeFeatures(
                new FeatureHostInitializationContext(BuildFeatureContext(), _resolver),
                pawnDefinition != null ? pawnDefinition.featureModules : null);
        }

        private ActorFeatureContext BuildFeatureContext()
        {
            _runtime ??= PawnRootRuntimeReferences.Capture(gameObject);

            return new ActorFeatureContext(
                gameObject,
                participant: Participant,
                pawnDefinition: pawnDefinition,
                gameMode: ActiveGameMode,
                health: _runtime.Health,
                animation: _runtime.Animation,
                knockback: _runtime.Knockback,
                presentationMode: pawnDefinition != null && pawnDefinition.presentationProfile != null
                    ? pawnDefinition.presentationProfile.presentationMode
                    : ActorPresentationMode.Sprite2D,
                authoredProfiles: new ScriptableObject[]
                {
                    pawnDefinition != null ? pawnDefinition.movementProfile : null,
                    pawnDefinition != null ? pawnDefinition.combatProfile : null,
                    pawnDefinition != null ? pawnDefinition.traversalProfile : null,
                    pawnDefinition != null ? pawnDefinition.presentationProfile : null,
                    pawnDefinition != null ? pawnDefinition.animationProfile : null
                });
        }

        private void OnDestroy()
        {
            _runtime?.FeatureHost?.ShutdownFeatures();
        }
    }
}
