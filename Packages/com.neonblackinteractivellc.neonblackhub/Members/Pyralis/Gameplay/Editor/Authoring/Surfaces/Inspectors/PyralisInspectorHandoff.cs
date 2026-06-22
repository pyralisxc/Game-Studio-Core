using System;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    public static class PyralisInspectorHandoff
    {
        private const string AuthoringDocsRoot = "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Authoring/";
        private const string InspectorHandoffText = "Inspector owns local field edits. Pyralis Authoring owns route setup, next steps, and first proof guidance.";

        public static void DrawAuthoringButton()
        {
            DrawAuthoringButton(null, null);
        }

        public static void DrawAuthoringButton(string context, string summary)
        {
            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                string label = string.IsNullOrWhiteSpace(context)
                    ? "Pyralis Authoring"
                    : context;
                EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(
                    string.IsNullOrWhiteSpace(summary) ? InspectorHandoffSummary : summary,
                    EditorStyles.wordWrappedMiniLabel);

                GUIContent button = new GUIContent(
                    "Open Pyralis Authoring",
                    "Open the graph-backed Pyralis Authoring Window for route setup, next steps, and first proof readiness.");
                if (GUILayout.Button(button))
                    NeonBlack.Gameplay.Editor.PyralisAuthoringWindow.Open();
            }
        }

        public static string AuthoringDocPath(string relativePath)
        {
            return string.IsNullOrWhiteSpace(relativePath)
                ? AuthoringDocsRoot + "START_HERE.md"
                : AuthoringDocsRoot + relativePath;
        }

        public static string InspectorHandoffSummary => InspectorHandoffText;
    }
}
