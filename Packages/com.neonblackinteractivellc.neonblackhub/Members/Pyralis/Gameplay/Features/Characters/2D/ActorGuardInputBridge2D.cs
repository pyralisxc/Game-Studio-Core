using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Features.Composition;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    [AddComponentMenu("NeonBlack/Gameplay/Characters/2D/Actor Guard Input Bridge 2D")]
    public class ActorGuardInputBridge2D : MonoBehaviour, IActorGuardInputReceiver2D
    {
        private ActorFeatureHost _featureHost;
        private IActorGuardFeature _guardFeature;

        private void Awake()
        {
            _featureHost = GetComponent<ActorFeatureHost>();
        }

        public void HandleGuardStartInput()
        {
            ResolveGuardFeature();
            _guardFeature?.BeginGuard();
        }

        public void HandleGuardEndInput()
        {
            ResolveGuardFeature();
            _guardFeature?.EndGuard();
        }

        private void ResolveGuardFeature()
        {
            _featureHost ??= GetComponent<ActorFeatureHost>();
            if (_featureHost == null)
                return;

            _guardFeature ??= _featureHost.TryGetInstalledFeature(out IActorGuardFeature feature)
                ? feature
                : null;
        }
    }
}
