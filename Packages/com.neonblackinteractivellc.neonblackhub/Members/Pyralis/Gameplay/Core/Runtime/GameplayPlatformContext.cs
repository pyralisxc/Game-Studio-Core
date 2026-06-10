using NeonBlack.Gameplay.Core.Contracts;
namespace NeonBlack.Gameplay.Core.Runtime
{
    /// <summary>
    /// Current platform runtime composition context for the active gameplay session.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Setup,
        Relevance = "Current platform runtime composition context for the active gameplay session.",
        ExpertAdvice = "Use TryGetCurrent to safely resolve the platform context without hard singleton dependencies."
    )]
    public sealed class GameplayPlatformContext
    {
        private static GameplayPlatformContext _current;

        [System.Obsolete("Use TryGetCurrent, TryGetServices, or TryResolve so missing platform setup stays explicit.")]
        public static GameplayPlatformContext Current => _current;

        public PlatformServiceRegistry Services { get; } = new PlatformServiceRegistry();

        public GameplayPlatformContext(object sessionDefinition = null)
        {
        }

        [System.Obsolete("GameplayRuntimeContext owns active session data; GameplayPlatformContext owns only runtime services.")]
        public void SetSessionDefinition(object sessionDefinition)
        {
        }

        public static bool TryGetCurrent(out GameplayPlatformContext context)
        {
            context = _current;
            return context != null;
        }

        public static bool TryGetServices(out PlatformServiceRegistry services)
        {
            services = _current?.Services;
            return services != null;
        }

        public static bool TryResolve<T>(out T service) where T : class
        {
            service = null;
            return TryGetServices(out PlatformServiceRegistry services)
                && services.TryResolve(out service)
                && service != null;
        }

        [System.Obsolete("Use TryGetServices and handle the missing platform setup path explicitly.")]
        public static PlatformServiceRegistry GetServicesOrEmpty()
        {
            return TryGetServices(out PlatformServiceRegistry services)
                ? services
                : new PlatformServiceRegistry();
        }

        public static GameplayPlatformContext CreateOrReplace(object sessionDefinition = null)
        {
            _current = new GameplayPlatformContext(sessionDefinition);
            return _current;
        }

        public static void ClearCurrent()
        {
            _current?.Services.Clear();
            _current = null;
        }

        public static bool ClearCurrentIf(GameplayPlatformContext context)
        {
            if (_current != context)
                return false;

            ClearCurrent();
            return true;
        }
    }
}
