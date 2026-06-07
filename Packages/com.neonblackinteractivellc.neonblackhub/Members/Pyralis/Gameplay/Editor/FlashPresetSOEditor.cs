using NeonBlack.Gameplay.Presentation.Visuals;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(FlashPresetSO))]
    public class FlashPresetSOEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            FlashPresetSO preset = (FlashPresetSO)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Flash Preset",
                "A flash preset stores reusable sprite/text flash behavior for damage, hazards, pickups, status, and impact feedback.",
                whenToUse: new[]
                {
                    "Use this when multiple actors, hazards, or projectiles should share a visual flash style.",
                    "Assign it to SpriteFlasher/TextFlasher users or feedback profiles."
                },
                createBefore: new[]
                {
                    "SpriteFlasher or feedback profile that will use this preset."
                },
                assignFirst: new[]
                {
                    "Choose Mode: Pulse, Strobe, Blink, or Color Cycle.",
                    "Set colors and timing.",
                    "Set alpha override only when the flash should change transparency."
                },
                safeToCustomize: new[]
                {
                    "Use Renderer Color As Base is usually best for shared actors.",
                    "Loop Count of -1 can represent continuous looping where supported.",
                    "Color Cycle mode should have at least one cycle color."
                },
                validation: new[]
                {
                    "Flash Duration is positive.",
                    "Color Cycle has cycle colors.",
                    "Preset is assigned to the feedback profile or flasher that needs it."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Health_Combat_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(preset), "Flash preset is ready for feedback setup.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetValidationIssues(FlashPresetSO preset)
        {
            List<string> issues = new List<string>();

            if (preset == null)
                return issues;

            if (preset.flashDuration <= 0f)
                issues.Add("Flash Duration must be greater than zero.");
            if (preset.mode == FlashPresetSO.FlashMode.ColorCycle && (preset.cycleColors == null || preset.cycleColors.Length == 0))
                issues.Add("Color Cycle mode should define at least one Cycle Color.");

            return issues;
        }
    }
}
