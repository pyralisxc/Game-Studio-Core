using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Character
{
    internal sealed class PawnRootRuntimeReferences
    {
        private readonly GameObject _owner;

        private PawnRootRuntimeReferences(
            GameObject owner,
            IActorHealthState health,
            ActorAnimationDriver animation,
            IActorKnockbackController knockback)
        {
            _owner = owner;
            Health = health;
            Animation = animation;
            Knockback = knockback;
        }

        public IActorHealthState Health { get; }
        public ActorAnimationDriver Animation { get; }
        public IActorKnockbackController Knockback { get; }

        public static PawnRootRuntimeReferences Capture(GameObject owner)
        {
            return new PawnRootRuntimeReferences(
                owner,
                owner != null ? owner.GetComponent<IActorHealthState>() : null,
                owner != null ? owner.GetComponent<ActorAnimationDriver>() : null,
                owner != null ? owner.GetComponent<IActorKnockbackController>() : null);
        }

        public MonoBehaviour[] GetProfileReceivers()
        {
            return _owner != null
                ? _owner.GetComponentsInChildren<MonoBehaviour>(true)
                : System.Array.Empty<MonoBehaviour>();
        }
    }
}
