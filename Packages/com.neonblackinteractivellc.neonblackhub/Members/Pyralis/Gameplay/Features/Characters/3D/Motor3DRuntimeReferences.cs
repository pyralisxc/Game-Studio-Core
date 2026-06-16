using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Features.Interaction;
using NeonBlack.Gameplay.Features.Traversal;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    internal sealed class Motor3DRuntimeReferences
    {
        private readonly GameObject _owner;

        private Motor3DRuntimeReferences(
            GameObject owner,
            Pawn3DInputModule input,
            Pawn3DMovementComponent movement,
            Pawn3DTraversalComponent traversal,
            Pawn3DPresentationComponent presentation,
            PawnCombatBehaviour combat,
            HealthComponent health,
            ActorFeatureHost featureHost)
        {
            _owner = owner;
            Input = input;
            Movement = movement;
            Traversal = traversal;
            Presentation = presentation;
            Combat = combat;
            Health = health;
            FeatureHost = featureHost;
        }

        public Pawn3DInputModule Input { get; }
        public Pawn3DMovementComponent Movement { get; }
        public Pawn3DTraversalComponent Traversal { get; }
        public Pawn3DPresentationComponent Presentation { get; }
        public PawnCombatBehaviour Combat { get; }
        public HealthComponent Health { get; }
        public ActorFeatureHost FeatureHost { get; private set; }
        public IActorTraversalFeature TraversalFeature { get; private set; }
        public IActorInteractionFeature InteractionFeature { get; private set; }
        public IActorGuardFeature GuardFeature { get; private set; }

        public static Motor3DRuntimeReferences Capture(GameObject owner)
        {
            return new Motor3DRuntimeReferences(
                owner,
                owner != null ? owner.GetComponent<Pawn3DInputModule>() : null,
                owner != null ? owner.GetComponent<Pawn3DMovementComponent>() : null,
                owner != null ? owner.GetComponent<Pawn3DTraversalComponent>() : null,
                owner != null ? owner.GetComponent<Pawn3DPresentationComponent>() : null,
                owner != null ? owner.GetComponent<PawnCombatBehaviour>() : null,
                owner != null ? owner.GetComponent<HealthComponent>() : null,
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
            GuardFeature ??= FeatureHost.TryGetInstalledFeature(out IActorGuardFeature guardFeature)
                ? guardFeature
                : null;
        }
    }
}
