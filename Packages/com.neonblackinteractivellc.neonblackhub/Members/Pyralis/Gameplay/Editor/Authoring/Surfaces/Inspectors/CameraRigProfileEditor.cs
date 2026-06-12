using NeonBlack.Gameplay.Data.Profiles;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(CameraRigProfile))]
    public class CameraRigProfileEditor : PyralisBaseEditor
    {
        protected override void DrawCustomInspector()
        {
            base.DrawCustomInspector();
            DrawProfileShortcuts();

            CameraRigProfile profile = (CameraRigProfile)target;

            if (!profile.useCinemachine)
                EditorGUILayout.HelpBox("The shared-core rewrite expects Cinemachine-backed rigs by default. Disable this only for specialized cases.", MessageType.Info);
        }

        private void DrawProfileShortcuts()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Camera Starting Points", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField("These apply editable camera defaults to this profile only; tune every field below for the actual game view.", EditorStyles.wordWrappedMiniLabel);
                if (GUILayout.Button("Apply 2D Orthographic Start"))
                {
                    serializedObject.FindProperty("presentationMode").enumValueIndex = (int)CameraRigProfile.CameraPresentationMode.Shared;
                    serializedObject.FindProperty("useCinemachine").boolValue = true;
                    serializedObject.FindProperty("orthographic").boolValue = true;
                    serializedObject.FindProperty("lockToPlayfield").boolValue = true;
                    serializedObject.FindProperty("useProfileTransform").boolValue = true;
                    serializedObject.FindProperty("followOffset").vector3Value = new Vector3(0f, 0f, -10f);
                    serializedObject.FindProperty("viewEulerAngles").vector3Value = Vector3.zero;
                    serializedObject.FindProperty("orthographicSize").floatValue = 5f;
                    serializedObject.FindProperty("minZoom").floatValue = 3f;
                    serializedObject.FindProperty("maxZoom").floatValue = 18f;
                    serializedObject.FindProperty("followDamping").floatValue = 0f;
                    serializedObject.FindProperty("zoomDamping").floatValue = 0f;
                }

                if (GUILayout.Button("Apply Tabletop / Board Start"))
                {
                    serializedObject.FindProperty("presentationMode").enumValueIndex = (int)CameraRigProfile.CameraPresentationMode.Shared;
                    serializedObject.FindProperty("useCinemachine").boolValue = true;
                    serializedObject.FindProperty("orthographic").boolValue = true;
                    serializedObject.FindProperty("lockToPlayfield").boolValue = true;
                    serializedObject.FindProperty("useProfileTransform").boolValue = true;
                    serializedObject.FindProperty("followOffset").vector3Value = new Vector3(0f, 0f, -12f);
                    serializedObject.FindProperty("viewEulerAngles").vector3Value = Vector3.zero;
                    serializedObject.FindProperty("orthographicSize").floatValue = 8f;
                    serializedObject.FindProperty("minZoom").floatValue = 5f;
                    serializedObject.FindProperty("maxZoom").floatValue = 24f;
                    serializedObject.FindProperty("followDamping").floatValue = 5f;
                    serializedObject.FindProperty("zoomDamping").floatValue = 5f;
                }

                if (GUILayout.Button("Apply 3D / Angled Follow Start"))
                {
                    serializedObject.FindProperty("presentationMode").enumValueIndex = (int)CameraRigProfile.CameraPresentationMode.Shared;
                    serializedObject.FindProperty("useCinemachine").boolValue = true;
                    serializedObject.FindProperty("orthographic").boolValue = false;
                    serializedObject.FindProperty("lockToPlayfield").boolValue = true;
                    serializedObject.FindProperty("useProfileTransform").boolValue = true;
                    serializedObject.FindProperty("followOffset").vector3Value = new Vector3(0f, 6f, -10f);
                    serializedObject.FindProperty("viewEulerAngles").vector3Value = new Vector3(30f, 0f, 0f);
                    serializedObject.FindProperty("defaultDistance").floatValue = 10f;
                    serializedObject.FindProperty("minZoom").floatValue = 4f;
                    serializedObject.FindProperty("maxZoom").floatValue = 20f;
                    serializedObject.FindProperty("followDamping").floatValue = 7f;
                    serializedObject.FindProperty("zoomDamping").floatValue = 6f;
                }
            }
        }
    }
}
