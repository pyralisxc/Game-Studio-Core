using System;
using NeonBlack.Gameplay.Core.Runtime;
using VContainer;

namespace NeonBlack.Gameplay.Features.Composition
{
    /// <summary>
    /// Context used by feature hosts when installing authored feature modules.
    /// </summary>
    public sealed class FeatureHostInitializationContext
    {
        public ActorFeatureContext ActorContext { get; }
        public PlatformServiceRegistry Services { get; }
        public IObjectResolver Resolver { get; }

        public FeatureHostInitializationContext(ActorFeatureContext actorContext, IObjectResolver resolver)
        {
            ActorContext = actorContext;
            Resolver = resolver;
        }

        [Obsolete("Use IObjectResolver constructor instead.")]
        public FeatureHostInitializationContext(ActorFeatureContext actorContext, PlatformServiceRegistry services)
        {
            ActorContext = actorContext;
            Services = services;
            IObjectResolver resolver = null;
            if (services != null)
                services.TryResolve(out resolver);
            Resolver = resolver;
        }
    }
}
