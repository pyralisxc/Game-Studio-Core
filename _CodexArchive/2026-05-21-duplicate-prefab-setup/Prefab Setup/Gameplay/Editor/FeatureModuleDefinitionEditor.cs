using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    [CustomEditor(typeof(FeatureModuleDefinition))]
    public class FeatureModuleDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            FeatureModuleDefinition definition = (FeatureModuleDefinition)target;

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Platform Metadata", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Network Role", definition.networkRole.ToString());
            EditorGUILayout.LabelField("Authoring Category", string.IsNullOrWhiteSpace(definition.authoringCategory) ? "(Missing)" : definition.authoringCategory);
            EditorGUILayout.LabelField("Gizmo Mode", definition.gizmoMode.ToString());

            if (GUILayout.Button("Sanitize Metadata"))
            {
                Undo.RecordObject(definition, "Sanitize Feature Module Metadata");
                definition.Sanitize();
                EditorUtility.SetDirty(definition);
            }

            List<string> issues = definition.GetValidationIssues();
            if (issues.Count == 0)
                return;

            EditorGUILayout.Space(6f);
            foreach (string issue in issues)
                EditorGUILayout.HelpBox(issue, MessageType.Warning);
        }
    }
}
