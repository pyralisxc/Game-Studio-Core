using NeonBlack.Gameplay.Data.Profiles;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(PlayfieldProfile))]
    public class PlayfieldProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            PlayfieldProfile profile = (PlayfieldProfile)target;
            if (profile.clampToBounds && profile.minBounds.x > profile.maxBounds.x)
                EditorGUILayout.HelpBox("Min Bounds X should not exceed Max Bounds X.", MessageType.Warning);
            if (profile.clampToBounds && profile.minBounds.y > profile.maxBounds.y)
                EditorGUILayout.HelpBox("Min Bounds Y should not exceed Max Bounds Y.", MessageType.Warning);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
