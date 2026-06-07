using NeonBlack.Gameplay.Data.Definitions;

namespace NeonBlack.Gameplay.Core.Runtime
{
    /// <summary>
    /// Current platform runtime composition context for the active gameplay session.
    /// </summary>
    public sealed class GameplayPlatformContext
    {
        public static GameplayPlatformContext Current { get; private set; }

        public PlatformServiceRegistry Services { get; } = new PlatformServiceRegistry();
        public SessionDefinition SessionDefinition { get; private set; }

        public GameplayPlatformContext(SessionDefinition sessionDefinition = null)
        {
            SessionDefinition = sessionDefinition;
        }

        public void SetSessionDefinition(SessionDefinition sessionDefinition)
        {
            SessionDefinition = sessionDefinition;
        }

        public static GameplayPlatformContext CreateOrReplace(SessionDefinition sessionDefinition = null)
        {
            Current = new GameplayPlatformContext(sessionDefinition);
            return Current;
        }

        public static void ClearCurrent()
        {
            Current?.Services.Clear();
            Current = null;
        }
    }
}
