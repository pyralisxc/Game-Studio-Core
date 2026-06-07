using NeonBlack.Gameplay.Core.Runtime;

namespace NeonBlack.Gameplay.Features.Composition
{
    /// <summary>
    /// Context used by feature hosts when installing authored feature modules.
    /// </summary>
    public sealed class FeatureHostInitializationContext
    {
        public ActorFeatureContext ActorContext { get; }
        public PlatformServiceRegistry Services { get; }

        public FeatureHostInitializationContext(ActorFeatureContext actorContext, PlatformServiceRegistry services)
        {
            ActorContext = actorContext;
            Services = services;
        }
    }
}
