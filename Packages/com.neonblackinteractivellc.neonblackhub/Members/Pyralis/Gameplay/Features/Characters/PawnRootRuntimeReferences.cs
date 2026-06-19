using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Characters
{
    internal sealed class PawnRootRuntimeReferences
    {
        private readonly GameObject _owner;

        private PawnRootRuntimeReferences(
            GameObject owner,
            ActorFeatureHost featureHost,
            HealthComponent health,
            ActorAnimationDriver animation,
            KnockbackReceiver knockback)
        {
            _owner = owner;
            FeatureHost = featureHost;
            Health = health;
            Animation = animation;
            Knockback = knockback;
        }

        public ActorFeatureHost FeatureHost { get; private set; }
        public HealthComponent Health { get; }
        public ActorAnimationDriver Animation { get; }
        public KnockbackReceiver Knockback { get; }

        public static PawnRootRuntimeReferences Capture(GameObject owner)
        {
            return new PawnRootRuntimeReferences(
                owner,
                owner != null ? owner.GetComponent<ActorFeatureHost>() : null,
                owner != null ? owner.GetComponent<HealthComponent>() : null,
                owner != null ? owner.GetComponent<ActorAnimationDriver>() : null,
                owner != null ? owner.GetComponent<KnockbackReceiver>() : null);
        }

        public MonoBehaviour[] GetProfileReceivers()
        {
            return _owner != null
                ? _owner.GetComponentsInChildren<MonoBehaviour>(true)
                : System.Array.Empty<MonoBehaviour>();
        }
    }
}
