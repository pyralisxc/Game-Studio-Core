using NeonBlack.Gameplay.Data.Profiles;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(EnemyFeatureProfile))]
    public class EnemyFeatureProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            EnemyFeatureProfile profile = (EnemyFeatureProfile)target;
            List<string> issues = profile.GetValidationIssues();
            for (int i = 0; i < issues.Count; i++)
                EditorGUILayout.HelpBox(issues[i], MessageType.Warning);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
