using System;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    public static class PyralisInspectorHandoff
    {
        private const string InspectorHandoffText = "Inspector owns local field edits. PYS Authoring owns graph projections, next steps, and proof guidance.";

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
                    ? "PYS Authoring"
                    : context;
                EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(
                    string.IsNullOrWhiteSpace(summary) ? InspectorHandoffSummary : summary,
                    EditorStyles.wordWrappedMiniLabel);

                GUIContent button = new GUIContent(
                    "Open PYS Authoring",
                    "Open the graph-backed PYS Authoring Window for route setup, next steps, and proof readiness.");
                if (GUILayout.Button(button))
                    EditorApplication.ExecuteMenuItem("Tools/PYS/Authoring");
            }
        }

        public static string AuthoringDocPath(string relativePath)
        {
            return string.Empty;
        }

        public static string InspectorHandoffSummary => InspectorHandoffText;
    }
}
