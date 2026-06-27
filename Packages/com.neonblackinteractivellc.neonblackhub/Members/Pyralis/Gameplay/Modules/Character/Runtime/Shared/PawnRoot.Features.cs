using NeonBlack.Gameplay.Modules.Actor.Composition;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Core.Types.Animation;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Character
{
    public partial class PawnRoot
    {
        private void InstallFeatureModules()
        {
            _runtime ??= PawnRootRuntimeReferences.Capture(gameObject);
            FeatureModuleDefinition[] definitions = pawnDefinition != null ? pawnDefinition.featureModules : null;
            if (definitions == null || definitions.Length == 0)
                return;

            ActorFeatureHost featureHost = _runtime.FeatureHost;
            if (featureHost == null)
            {
                Debug.LogWarning(
                    $"PawnRoot `{name}` has feature modules assigned through PawnDefinition `{pawnDefinition.name}`, but the pawn prefab is missing ActorFeatureHost. Add ActorFeatureHost to the pawn root so optional features are explicit in the prefab.",
                    this);
                return;
            }

            featureHost.InitializeFeatures(
                new FeatureHostInitializationContext(BuildFeatureContext(), _resolver),
                definitions);
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
