using NeonBlack.Gameplay.Data.Profiles;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(HazardFeedbackProfile))]
    public class HazardFeedbackProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            HazardFeedbackProfile profile = (HazardFeedbackProfile)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Hazard Feedback Profile",
                "A hazard feedback profile controls flashes, impact effects, popups, and readable warning feedback for hazards.",
                whenToUse: new[]
                {
                    "Use this when hazards should communicate activation, explosion, bounce, collection, or exit states.",
                    "Share profiles across hazard families to keep danger feedback consistent."
                },
                createBefore: new[]
                {
                    "FlashPresetSO assets for any enabled flash events.",
                    "Hazard runtime prefab or HazardData that references feedback behavior."
                },
                assignFirst: new[]
                {
                    "Enable the flash and popup events the hazard should communicate.",
                    "Assign flash presets for enabled flash events.",
                    "Set popup text, color, lifetime, and offset."
                },
                safeToCustomize: new[]
                {
                    "Popup text can be short for arcade readability.",
                    "Flash and popup systems can be enabled independently.",
                    "Use different colors for warning, explosion, collectible, and clear states."
                },
                validation: new[]
                {
                    "Enabled flash events have matching presets.",
                    "Enabled popups have non-empty text.",
                    "Popup lifetime and font size are visible but not noisy."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Hazard_Difficulty_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(profile), "Hazard feedback profile is ready for hazard setup.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetValidationIssues(HazardFeedbackProfile profile)
        {
            List<string> issues = new List<string>();

            if (profile == null)
                return issues;

            if (profile.flashOnActivation && profile.activationFlashPreset == null)
                issues.Add("Activation flash is enabled but Activation Flash Preset is empty.");
            if (profile.flashOnExplosion && profile.explosionFlashPreset == null)
                issues.Add("Explosion flash is enabled but Explosion Flash Preset is empty.");
            if (profile.flashOnBounce && profile.bounceFlashPreset == null)
                issues.Add("Bounce flash is enabled but Bounce Flash Preset is empty.");

            return issues;
        }
    }
}
