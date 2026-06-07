using NeonBlack.Gameplay.Data.Profiles;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(SettingsProfile))]
    public class SettingsProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            SettingsProfile profile = (SettingsProfile)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Settings Profile",
                "A settings profile stores default values used by SettingsManager before saved player preferences override them.",
                whenToUse: new[]
                {
                    "Use this when a session needs volume, fullscreen, control swap, or deadzone defaults.",
                    "Assign it from SessionDefinition or SettingsManager depending on the setup."
                },
                createBefore: new[]
                {
                    "AudioMixer if music/SFX mixer parameters should be driven.",
                    "SessionDefinition or SettingsManager that will reference this profile."
                },
                assignFirst: new[]
                {
                    "Assign Mixer when audio sliders should affect an AudioMixer.",
                    "Set default music/SFX volume.",
                    "Set joystick and gamepad deadzone defaults."
                },
                safeToCustomize: new[]
                {
                    "Volume values are normalized 0 to 1.",
                    "Deadzones are clamped from 0 to 0.5.",
                    "Fullscreen and swap controls are just startup defaults."
                },
                validation: new[]
                {
                    "SettingsManager references this profile when settings UI is used.",
                    "Mixer is assigned if audio mixer controls are expected.",
                    "Values are sane after OnValidate clamps them."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Settings_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(profile), "Settings profile is ready for settings assignment.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetValidationIssues(SettingsProfile profile)
        {
            return new List<string>();
        }
    }
}
