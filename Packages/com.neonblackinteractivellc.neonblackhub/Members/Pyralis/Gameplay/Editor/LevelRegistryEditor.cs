using NeonBlack.Gameplay.Core.Navigation;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(LevelRegistry))]
    public class LevelRegistryEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            LevelRegistry registry = (LevelRegistry)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Level Registry",
                "A level registry is the ordered catalogue of LevelData assets used by menu and scene-flow systems.",
                whenToUse: new[]
                {
                    "Use this when a menu, scene loader, or random level picker needs a list of playable levels.",
                    "Keep one registry per game flow unless you intentionally need separate campaigns or playlists."
                },
                createBefore: new[]
                {
                    "LevelData assets for every scene/world that can be selected."
                },
                assignFirst: new[]
                {
                    "Add LevelData assets to Levels in display order.",
                    "Keep scene names unique across the list."
                },
                safeToCustomize: new[]
                {
                    "Order controls menu order and can be changed safely.",
                    "Null entries should be removed before using the registry at runtime."
                },
                validation: new[]
                {
                    "Levels has at least one entry.",
                    "No entries are missing.",
                    "Each LevelData has a scene name."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Scene_Flow_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(registry), "Level registry is ready for scene-flow setup.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetValidationIssues(LevelRegistry registry)
        {
            List<string> issues = new List<string>();

            if (registry == null)
                return issues;
            if (registry.levels == null || registry.levels.Length == 0)
            {
                issues.Add("Levels is empty. Add at least one LevelData asset.");
                return issues;
            }

            for (int i = 0; i < registry.levels.Length; i++)
            {
                if (registry.levels[i] == null)
                    issues.Add($"Levels[{i}] is empty.");
                else if (string.IsNullOrWhiteSpace(registry.levels[i].sceneName))
                    issues.Add($"Levels[{i}] has no Scene Name.");
            }

            return issues;
        }
    }
}
