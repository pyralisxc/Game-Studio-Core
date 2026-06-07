using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(RuntimePatternDefinition))]
    public class RuntimePatternDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            RuntimePatternDefinition pattern = (RuntimePatternDefinition)target;

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Setup Readiness", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Capability Family", pattern.capabilityFamily.ToString());
            EditorGUILayout.LabelField("Participant Embodiment", pattern.participantEmbodiment.ToString());
            EditorGUILayout.LabelField("Supports Pawn", pattern.SupportsControlSurface(RuntimeControlSurface.Pawn) ? "Yes" : "No");
            EditorGUILayout.LabelField("Supports Non-Pawn Surface", SupportsNonPawnSurface(pattern) ? "Yes" : "No");

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Supported Control Surfaces", EditorStyles.boldLabel);
            DrawControlSurfaces(pattern);

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Companion Patterns", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Recommended", CountAssigned(pattern.recommendedCompanionPatterns).ToString());
            EditorGUILayout.LabelField("Cautionary", CountAssigned(pattern.cautionaryCompanionPatterns).ToString());

            List<string> issues = pattern.GetValidationIssues();
            DrawIssues(issues);

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawControlSurfaces(RuntimePatternDefinition pattern)
        {
            if (pattern.supportedControlSurfaces == null || pattern.supportedControlSurfaces.Length == 0)
            {
                EditorGUILayout.HelpBox("No supported control surfaces assigned.", MessageType.Warning);
                return;
            }

            for (int i = 0; i < pattern.supportedControlSurfaces.Length; i++)
                EditorGUILayout.LabelField("-", pattern.supportedControlSurfaces[i].ToString());
        }

        private static void DrawIssues(List<string> issues)
        {
            if (issues.Count == 0)
            {
                EditorGUILayout.HelpBox("Pattern metadata is ready for setup-profile use.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space(4f);
            for (int i = 0; i < issues.Count; i++)
                EditorGUILayout.HelpBox(issues[i], MessageType.Warning);
        }

        private static bool SupportsNonPawnSurface(RuntimePatternDefinition pattern)
        {
            if (pattern.supportedControlSurfaces == null)
                return false;

            for (int i = 0; i < pattern.supportedControlSurfaces.Length; i++)
            {
                if (pattern.supportedControlSurfaces[i] != RuntimeControlSurface.Pawn)
                    return true;
            }

            return false;
        }

        private static int CountAssigned(RuntimePatternDefinition[] patterns)
        {
            if (patterns == null)
                return 0;

            int count = 0;
            for (int i = 0; i < patterns.Length; i++)
            {
                if (patterns[i] != null)
                    count++;
            }

            return count;
        }
    }
}
