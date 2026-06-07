using NeonBlack.Gameplay.Data.Profiles;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(CameraRigProfile))]
    public class CameraRigProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            CameraRigProfile profile = (CameraRigProfile)target;
            if (profile.minZoom > profile.maxZoom)
                EditorGUILayout.HelpBox("Min Zoom should not exceed Max Zoom.", MessageType.Warning);
            if (!profile.useCinemachine)
                EditorGUILayout.HelpBox("The shared-core rewrite expects Cinemachine-backed rigs by default. Disable this only for specialized cases.", MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
