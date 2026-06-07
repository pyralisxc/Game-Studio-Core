using NeonBlack.Gameplay.Data.Definitions;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(SessionDefinition))]
    public class SessionDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            SessionDefinition definition = (SessionDefinition)target;
            if (definition.maxParticipants <= 0)
                EditorGUILayout.HelpBox("Max Participants should be at least 1.", MessageType.Warning);
            if (definition.defaultGameMode == null)
                EditorGUILayout.HelpBox("Assign a default game mode so the bootstrap can configure shared services.", MessageType.Info);
            if (!definition.localFirst)
                EditorGUILayout.HelpBox("This rewrite currently assumes local-first startup even when host authority is enabled.", MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
