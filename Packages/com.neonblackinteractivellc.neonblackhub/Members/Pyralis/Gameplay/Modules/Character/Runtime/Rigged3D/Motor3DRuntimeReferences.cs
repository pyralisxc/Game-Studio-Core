using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Participants;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Character
{
    internal sealed class Motor3DRuntimeReferences
    {
        private readonly GameObject _owner;

        private Motor3DRuntimeReferences(
            GameObject owner,
            Pawn3DInputModule input,
            Pawn3DMovementComponent movement,
            IPawnTraversalModule traversal,
            Pawn3DPresentationComponent presentation,
            IActorCombatRuntimeTickReceiver combatTicker,
            IActorCombatRequestReceiver combatRequests,
            IActorHealthState health,
            IActorDamageImmunityController damageImmunity,
            IActorGuardController guardFeature,
            IActorTraversalFeature traversalFeature,
            IActorInteractionRequestReceiver interactionRequests)
        {
            _owner = owner;
            Input = input;
            Movement = movement;
            Traversal = traversal;
            Presentation = presentation;
            CombatTicker = combatTicker;
            CombatRequests = combatRequests;
            Health = health;
            DamageImmunity = damageImmunity;
            GuardFeature = guardFeature;
            TraversalFeature = traversalFeature;
            InteractionRequests = interactionRequests;
        }

        public Pawn3DInputModule Input { get; }
        public Pawn3DMovementComponent Movement { get; }
        public IPawnTraversalModule Traversal { get; }
        public Pawn3DPresentationComponent Presentation { get; }
        public IActorCombatRuntimeTickReceiver CombatTicker { get; }
        public IActorCombatRequestReceiver CombatRequests { get; }
        public IActorHealthState Health { get; }
        public IActorDamageImmunityController DamageImmunity { get; }
        public IActorTraversalFeature TraversalFeature { get; private set; }
        public IActorInteractionRequestReceiver InteractionRequests { get; private set; }
        public IActorGuardController GuardFeature { get; private set; }

        public static Motor3DRuntimeReferences Capture(GameObject owner)
        {
            return new Motor3DRuntimeReferences(
                owner,
                owner != null ? owner.GetComponent<Pawn3DInputModule>() : null,
                owner != null ? owner.GetComponent<Pawn3DMovementComponent>() : null,
                owner != null ? owner.GetComponent<IPawnTraversalModule>() : null,
                owner != null ? owner.GetComponent<Pawn3DPresentationComponent>() : null,
                owner != null ? owner.GetComponent<IActorCombatRuntimeTickReceiver>() : null,
                owner != null ? owner.GetComponent<IActorCombatRequestReceiver>() : null,
                owner != null ? owner.GetComponent<IActorHealthState>() : null,
                owner != null ? owner.GetComponent<IActorDamageImmunityController>() : null,
                owner != null ? owner.GetComponent<IActorGuardController>() : null,
                owner != null ? owner.GetComponent<IActorTraversalFeature>() : null,
                owner != null ? owner.GetComponent<IActorInteractionRequestReceiver>() : null);
        }

        public void ResolveDirectCapabilities()
        {
            if (_owner == null)
                return;

            TraversalFeature ??= _owner.GetComponent<IActorTraversalFeature>();
            InteractionRequests ??= _owner.GetComponent<IActorInteractionRequestReceiver>();
            GuardFeature ??= _owner.GetComponent<IActorGuardController>();
        }
    }
}
