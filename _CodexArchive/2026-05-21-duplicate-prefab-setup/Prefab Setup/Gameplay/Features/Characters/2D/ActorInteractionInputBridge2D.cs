using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Features.Interaction;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    [AddComponentMenu("NeonBlack/Gameplay/Interaction/Actor Interaction Input Bridge 2D")]
    public class ActorInteractionInputBridge2D : MonoBehaviour, IActorInteractionInputReceiver2D
    {
        private ActorFeatureHost _featureHost;
        private IActorInteractionFeature _interactionFeature;

        private void Awake()
        {
            _featureHost = GetComponent<ActorFeatureHost>();
        }

        public void HandleInteractionInput()
        {
            _featureHost ??= GetComponent<ActorFeatureHost>();
            if (_featureHost == null)
                return;

            _interactionFeature ??= _featureHost.TryGetInstalledFeature(out IActorInteractionFeature feature)
                ? feature
                : null;
            _interactionFeature?.TryHandleInteraction();
        }
    }
}
