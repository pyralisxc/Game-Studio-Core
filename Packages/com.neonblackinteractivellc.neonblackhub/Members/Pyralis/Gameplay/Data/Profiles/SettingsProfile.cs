using UnityEngine;
using UnityEngine.Audio;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Shared defaults for save-backed user settings and runtime presentation choices.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.UI, 
        Relevance = "Project-window creation path for settings and menu defaults.",
        AssignmentFields = new[] { nameof(mixer), nameof(defaultMusicVolume), nameof(defaultSfxVolume) },
        FirstProof = "Check that volumes are applied correctly in the main menu.",
        NativeSetup = new[] { "Create Asset" }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Settings Profile", fileName = "SettingsProfile", order = -10)]
    public class SettingsProfile : ScriptableObject
    {
        public AudioMixer mixer;
        public float defaultMusicVolume = 1f;
        public float defaultSfxVolume = 1f;
        public float defaultJoystickDeadzone = 0.1f;
        public float defaultGamepadDeadzone = 0.2f;
        public bool defaultSwapControls = false;
        public bool defaultFullscreen = true;

        public void Sanitize()
        {
            defaultMusicVolume = Mathf.Clamp01(defaultMusicVolume);
            defaultSfxVolume = Mathf.Clamp01(defaultSfxVolume);
            defaultJoystickDeadzone = Mathf.Clamp(defaultJoystickDeadzone, 0f, 0.5f);
            defaultGamepadDeadzone = Mathf.Clamp(defaultGamepadDeadzone, 0f, 0.5f);
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
