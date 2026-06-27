using System;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Core.Types.Animation;
using UnityEngine;
using VContainer;

namespace NeonBlack.Gameplay.Modules.Actor.Composition
{
    /// <summary>
    /// Rich runtime initialization context supplied to feature module runtimes.
    /// </summary>
    public sealed class FeatureRuntimeInitializationContext
    {
        public ActorFeatureContext ActorContext { get; }
        public FeatureModuleDefinition Definition { get; }
        public IObjectResolver Resolver { get; }

        public GameObject ActorObject => ActorContext != null ? ActorContext.ActorObject : null;
        public Transform ActorTransform => ActorContext != null ? ActorContext.ActorTransform : null;
        public ParticipantHandle Participant => ActorContext != null ? ActorContext.Participant : null;
        public PawnDefinition PawnDefinition => ActorContext != null ? ActorContext.PawnDefinition : null;
        public ActorPresentationMode PresentationMode => ActorContext != null ? ActorContext.PresentationMode : ActorPresentationMode.Sprite2D;

        public FeatureRuntimeInitializationContext(ActorFeatureContext actorContext, FeatureModuleDefinition definition, IObjectResolver resolver)
        {
            ActorContext = actorContext;
            Definition = definition;
            Resolver = resolver;
        }

        public T GetProfile<T>(ScriptableObject preferred = null) where T : ScriptableObject
        {
            return ActorContext != null ? ActorContext.GetProfile<T>(preferred) : null;
        }

        public PawnProfileApplicationContext BuildPawnProfileApplicationContext()
        {
            return new PawnProfileApplicationContext(ActorObject, PawnDefinition, Participant);
        }
    }
}
