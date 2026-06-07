using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Core.Runtime;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;
using VContainer;

namespace NeonBlack.Gameplay.Features.Composition
{
    /// <summary>
    /// Rich runtime initialization context supplied to feature module runtimes.
    /// </summary>
    public sealed class FeatureRuntimeInitializationContext
    {
        public ActorFeatureContext ActorContext { get; }
        public FeatureModuleDefinition Definition { get; }
        public PlatformServiceRegistry Services { get; }
        public IObjectResolver Resolver { get; }

        public GameObject ActorObject => ActorContext != null ? ActorContext.ActorObject : null;
        public Transform ActorTransform => ActorContext != null ? ActorContext.ActorTransform : null;
        public ParticipantHandle Participant => ActorContext != null ? ActorContext.Participant : null;
        public PawnDefinition PawnDefinition => ActorContext != null ? ActorContext.PawnDefinition : null;
        public ActorPresentationMode PresentationMode => ActorContext != null ? ActorContext.PresentationMode : ActorPresentationMode.Sprite2D;

        public FeatureRuntimeInitializationContext(ActorFeatureContext actorContext, FeatureModuleDefinition definition, PlatformServiceRegistry services)
        {
            ActorContext = actorContext;
            Definition = definition;
            Services = services;
            IObjectResolver resolver = null;
            if (services != null)
                services.TryResolve(out resolver);
            Resolver = resolver;
        }

        public T GetProfile<T>(ScriptableObject preferred = null) where T : ScriptableObject
        {
            return ActorContext != null ? ActorContext.GetProfile<T>(preferred) : null;
        }
    }
}
