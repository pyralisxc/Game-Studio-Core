using NeonBlack.Gameplay.Data.Definitions;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(GameModeDefinition))]
    public class GameModeDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            GameModeDefinition definition = (GameModeDefinition)target;
            List<string> issues = definition.GetValidationIssues();
            for (int i = 0; i < issues.Count; i++)
                EditorGUILayout.HelpBox(issues[i], MessageType.Warning);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
