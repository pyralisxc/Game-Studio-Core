using NeonBlack.Gameplay.Core.Config;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(InputConfig))]
    public class InputConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            bool hasActions = serializedObject.FindProperty("actions")?.objectReferenceValue != null;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Input Config",
                "Input Config is a lightweight project wrapper around a Unity Input Action Asset. Current setup usually prefers InputProfile.",
                whenToUse: new[]
                {
                    "Use this when a gameplay adapter expects InputConfig.",
                    "Prefer InputProfile for new session, participant, pawn, camera, cursor, and local-join setup."
                },
                createBefore: new[]
                {
                    "Unity Input Action Asset with the maps and actions your runtime expects."
                },
                assignFirst: new[]
                {
                    "Assign Actions.",
                    "Confirm action map names match the consuming runtime."
                },
                safeToCustomize: new[]
                {
                    "Use this as a narrow Unity Input asset reference; do not expand it into a second input-authoring model.",
                    "Move new setup guidance into InputProfile."
                },
                validation: new[]
                {
                    "Actions is assigned when this asset is used by a runtime adapter.",
                    "New player-owned input has a matching InputProfile."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("AUTHORING_MODEL.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(hasActions), "Input config is ready for input adapter use.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetValidationIssues(bool hasActions)
        {
            List<string> issues = new List<string>();

            if (!hasActions)
                issues.Add("Actions is empty. Assign an Input Action Asset when this config is used at runtime.");

            return issues;
        }
    }
}
