using System.Linq;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Character
{
    internal sealed class Pawn3DMovementRuntimeReferences
    {
        private Pawn3DMovementRuntimeReferences(
            CharacterController controller,
            IActorKnockbackController knockback,
            IPawnCombatMovementContext combat)
        {
            Controller = controller;
            Knockback = knockback;
            Combat = combat;
        }

        public CharacterController Controller { get; }
        public IActorKnockbackController Knockback { get; }
        public IPawnCombatMovementContext Combat { get; }

        public static Pawn3DMovementRuntimeReferences Capture(GameObject owner)
        {
            MonoBehaviour[] behaviours = owner != null
                ? owner.GetComponents<MonoBehaviour>()
                : System.Array.Empty<MonoBehaviour>();

            return new Pawn3DMovementRuntimeReferences(
                owner != null ? owner.GetComponent<CharacterController>() : null,
                behaviours.OfType<IActorKnockbackController>().FirstOrDefault(),
                behaviours.OfType<IPawnCombatMovementContext>().FirstOrDefault());
        }
    }
}
