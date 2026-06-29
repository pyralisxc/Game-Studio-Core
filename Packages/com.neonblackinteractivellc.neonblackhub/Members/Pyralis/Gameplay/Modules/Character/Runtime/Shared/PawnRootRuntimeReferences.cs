using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Character
{
    internal sealed class PawnRootRuntimeReferences
    {
        private readonly GameObject _owner;

        private PawnRootRuntimeReferences(
            GameObject owner,
            IActorHealthState health,
            IActorKnockbackController knockback)
        {
            _owner = owner;
            Health = health;
            Knockback = knockback;
        }

        public IActorHealthState Health { get; }
        public IActorKnockbackController Knockback { get; }

        public static PawnRootRuntimeReferences Capture(GameObject owner)
        {
            return new PawnRootRuntimeReferences(
                owner,
                owner != null ? owner.GetComponent<IActorHealthState>() : null,
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
