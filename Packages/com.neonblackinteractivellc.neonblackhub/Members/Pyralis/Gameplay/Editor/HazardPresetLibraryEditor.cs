using NeonBlack.Gameplay.Features.Hazards;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(HazardPresetLibrary))]
    public class HazardPresetLibraryEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            HazardPresetLibrary library = (HazardPresetLibrary)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Hazard Preset Library",
                "A hazard preset library is an editor/designer catalogue of named HazardData presets.",
                whenToUse: new[]
                {
                    "Use this when designers need a quick list of tuned hazards.",
                    "Use it for hazard playlists, encounter authoring, or debug spawning by preset name."
                },
                createBefore: new[]
                {
                    "HazardData assets for every preset entry."
                },
                assignFirst: new[]
                {
                    "Add entries with readable Preset Names.",
                    "Assign a HazardData asset to every entry."
                },
                safeToCustomize: new[]
                {
                    "Preset names can be friendly designer labels.",
                    "Keep names unique if runtime lookup by name is used."
                },
                validation: new[]
                {
                    "No empty entries.",
                    "No duplicate preset names.",
                    "Every entry points to HazardData."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Hazard_Difficulty_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(library), "Hazard preset library is ready for hazard authoring.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetValidationIssues(HazardPresetLibrary library)
        {
            List<string> issues = new List<string>();

            if (library?.presets == null || library.presets.Length == 0)
            {
                issues.Add("Presets is empty. Add HazardData entries before using this library.");
                return issues;
            }

            HashSet<string> names = new HashSet<string>();
            for (int i = 0; i < library.presets.Length; i++)
            {
                HazardPresetLibrary.Entry entry = library.presets[i];
                if (entry == null)
                {
                    issues.Add($"Presets[{i}] is empty.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.presetName))
                    issues.Add($"Presets[{i}] has no Preset Name.");
                else if (!names.Add(entry.presetName))
                    issues.Add($"Preset name `{entry.presetName}` is used more than once.");

                if (entry.data == null)
                    issues.Add($"Presets[{i}] has no HazardData asset.");
            }

            return issues;
        }
    }
}
