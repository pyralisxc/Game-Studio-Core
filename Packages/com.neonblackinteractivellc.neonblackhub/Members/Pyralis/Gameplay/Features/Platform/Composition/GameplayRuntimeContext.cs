using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Core.Config
{
    /// <summary>
    /// Runtime session context used by gameplay systems, menus, and compatibility bridges.
    /// </summary>
    public static class GameplayRuntimeContext
    {
        public static SessionDefinition ActiveSessionDefinition { get; private set; }
        public static GameModeDefinition ActiveGameMode => ActiveSessionDefinition != null
            ? ActiveSessionDefinition.defaultGameMode
            : null;
        public static InputProfile DefaultInputProfile { get; private set; }
        public static InputActionAsset DefaultInputActions => DefaultInputProfile != null
            ? DefaultInputProfile.actions
            : null;

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Clear();
        }

        public static void SetSession(SessionDefinition definition)
        {
            ActiveSessionDefinition = definition;
            DefaultInputProfile = definition != null
                ? definition.defaultInputProfile
                : null;
        }

        public static void SetDefaultInputProfile(InputProfile profile)
        {
            DefaultInputProfile = profile;
        }

        public static void Clear()
        {
            ActiveSessionDefinition = null;
            DefaultInputProfile = null;
        }
    }
}
