using NeonBlack.Gameplay.Core.Config;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(GameConfig))]
    public class GameConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            GameConfig config = (GameConfig)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Game Config",
                "Game Config is a project-level wiring point for scenes and service prefabs. Current gameplay setup should prefer SessionDefinition, GameModeDefinition, and profiles.",
                whenToUse: new[]
                {
                    "Use this when scene-loader or project bootstrap code expects a central GameConfig asset.",
                    "Use it as a bridge to SessionDefinition rather than as the main gameplay design surface."
                },
                createBefore: new[]
                {
                    "SessionDefinition for the active gameplay setup.",
                    "SceneLoader, TimeManager, or CameraShake prefabs only when your bootstrap expects custom service prefabs."
                },
                assignFirst: new[]
                {
                    "Assign Session Definition.",
                    "Set Main Menu Scene and Gameplay Scene if this scene flow uses them.",
                    "Assign service prefabs only when overriding default service creation."
                },
                safeToCustomize: new[]
                {
                    "Service prefab fields can stay empty when defaults are acceptable.",
                    "Gameplay Scene can stay empty if GameModeDefinition controls scene selection."
                },
                validation: new[]
                {
                    "Session Definition points to the canonical session asset.",
                    "Scene names match Build Settings when this scene flow uses them."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Scene_Flow_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(config), "Game config is ready as a project config.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetValidationIssues(GameConfig config)
        {
            List<string> issues = new List<string>();

            if (config != null && config.sessionDefinition == null)
                issues.Add("Session Definition is empty. New Pyralis setup should assign the canonical SessionDefinition here when GameConfig is used.");

            return issues;
        }
    }
}
