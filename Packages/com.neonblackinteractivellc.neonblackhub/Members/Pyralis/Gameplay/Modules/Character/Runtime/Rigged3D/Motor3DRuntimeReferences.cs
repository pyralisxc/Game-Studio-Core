using NeonBlack.Gameplay.Modules.Actor.Composition;
using NeonBlack.Gameplay.Modules.Interaction;
using NeonBlack.Gameplay.Core.Contracts;
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
            IActorCombatCommandReceiver combat,
            IActorHealthState health,
            IActorDamageImmunityController damageImmunity,
            IActorGuardController guardFeature,
            ActorFeatureHost featureHost)
        {
            _owner = owner;
            Input = input;
            Movement = movement;
            Traversal = traversal;
            Presentation = presentation;
            Combat = combat;
            Health = health;
            DamageImmunity = damageImmunity;
            GuardFeature = guardFeature;
            FeatureHost = featureHost;
        }

        public Pawn3DInputModule Input { get; }
        public Pawn3DMovementComponent Movement { get; }
        public IPawnTraversalModule Traversal { get; }
        public Pawn3DPresentationComponent Presentation { get; }
        public IActorCombatCommandReceiver Combat { get; }
        public IActorHealthState Health { get; }
        public IActorDamageImmunityController DamageImmunity { get; }
        public ActorFeatureHost FeatureHost { get; private set; }
        public IActorTraversalFeature TraversalFeature { get; private set; }
        public IActorInteractionFeature InteractionFeature { get; private set; }
        public IActorGuardController GuardFeature { get; private set; }

        public static Motor3DRuntimeReferences Capture(GameObject owner)
        {
            return new Motor3DRuntimeReferences(
                owner,
                owner != null ? owner.GetComponent<Pawn3DInputModule>() : null,
                owner != null ? owner.GetComponent<Pawn3DMovementComponent>() : null,
                owner != null ? owner.GetComponent<IPawnTraversalModule>() : null,
                owner != null ? owner.GetComponent<Pawn3DPresentationComponent>() : null,
                owner != null ? owner.GetComponent<IActorCombatCommandReceiver>() : null,
                owner != null ? owner.GetComponent<IActorHealthState>() : null,
                owner != null ? owner.GetComponent<IActorDamageImmunityController>() : null,
                owner != null ? owner.GetComponent<IActorGuardController>() : null,
                owner != null ? owner.GetComponent<ActorFeatureHost>() : null);
        }

        public void ResolveFeatureModules()
        {
            if (FeatureHost == null && _owner != null)
                FeatureHost = _owner.GetComponent<ActorFeatureHost>();

            if (FeatureHost == null)
                return;

            TraversalFeature ??= FeatureHost.TryGetInstalledFeature(out IActorTraversalFeature traversalFeature)
                ? traversalFeature
                : null;
            InteractionFeature ??= FeatureHost.TryGetInstalledFeature(out IActorInteractionFeature interactionFeature)
                ? interactionFeature
                : null;
            GuardFeature ??= FeatureHost.TryGetInstalledFeature(out IActorGuardController guardFeature)
                ? guardFeature
                : null;
        }
    }
}
