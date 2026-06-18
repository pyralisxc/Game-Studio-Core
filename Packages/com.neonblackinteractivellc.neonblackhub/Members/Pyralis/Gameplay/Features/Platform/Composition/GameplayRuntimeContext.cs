using NeonBlack.Gameplay.Data.Definitions;

namespace NeonBlack.Gameplay.Core.Config
{
    /// <summary>
    /// Runtime session context used by gameplay systems, menus, and direct-scene query helpers.
    /// </summary>
    public static class GameplayRuntimeContext
    {
        public static SessionDefinition ActiveSessionDefinition { get; private set; }
        public static GameModeDefinition ActiveGameMode => ActiveSessionDefinition != null
            ? ActiveSessionDefinition.defaultGameMode
            : null;

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Clear();
        }

        public static void SetSession(SessionDefinition definition)
        {
            ActiveSessionDefinition = definition;
        }

        public static void Clear()
        {
            ActiveSessionDefinition = null;
        }
    }
}
