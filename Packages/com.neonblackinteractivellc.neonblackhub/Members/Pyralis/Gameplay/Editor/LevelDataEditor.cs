using NeonBlack.Gameplay.Core.Navigation;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(LevelData))]
    public class LevelDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            LevelData level = (LevelData)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Level Data",
                "Level Data describes one selectable scene/world for menu and scene-flow systems.",
                whenToUse: new[]
                {
                    "Use one LevelData asset per playable scene, world, board, arena, or level entry.",
                    "Add LevelData assets to LevelRegistry in display order."
                },
                createBefore: new[]
                {
                    "Unity scene included in Build Settings.",
                    "Preview sprite if the level selector shows thumbnails."
                },
                assignFirst: new[]
                {
                    "Set Scene Name exactly as it appears in Build Settings.",
                    "Set Display Name for menus.",
                    "Assign Preview Image if the menu uses one."
                },
                safeToCustomize: new[]
                {
                    "Display Name can change without breaking scene loading.",
                    "Scene Name must stay exact."
                },
                validation: new[]
                {
                    "Scene Name is not empty and matches Build Settings.",
                    "LevelRegistry includes this asset."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Scene_Flow_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(level), "Level data is ready for LevelRegistry assignment.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetValidationIssues(LevelData level)
        {
            List<string> issues = new List<string>();

            if (level == null)
                return issues;
            if (string.IsNullOrWhiteSpace(level.sceneName))
                issues.Add("Scene Name is required.");
            if (string.IsNullOrWhiteSpace(level.displayName))
                issues.Add("Display Name is recommended for menus.");

            return issues;
        }
    }
}
