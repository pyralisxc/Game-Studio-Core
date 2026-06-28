using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using NeonBlack.Gameplay.Core.Contracts;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Shared defaults for save-backed user settings and runtime presentation choices.
    /// </summary>
    [AuthoringContract(
        Category = "U I",
        CapabilityPath = "Settings/Profiles/Settings Profile",
        Surface = AuthoringSurface.Profile,
        Summary = "Project-window creation path for settings and menu defaults.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/core",
        RequiredFields = new[] { nameof(mixer), nameof(defaultMusicVolume), nameof(defaultSfxVolume) },
        SuccessChecks = new[] { "Check that volumes are applied correctly in the main menu." },
        Tags = new[] { "capability:UI", "runtime:PlatformCore" },
        Selectable = false
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Settings Profile", fileName = "SettingsProfile", order = -10)]
    public class SettingsProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (mixer == null) yield return PyralisRuntimeValidationIssue.Required("Audio Mixer is missing.");
        }

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
