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
            DrawProfileShortcuts();

            CameraRigProfile profile = (CameraRigProfile)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Camera Rig Profile",
                "A camera rig profile defines shared or split participant framing. GameModeDefinition and CinemachineCameraRigController use it to decide how the camera presents the playfield.",
                whenToUse: new[]
                {
                    "Use this when a mode needs shared camera, split screen, camera/cursor control, orthographic framing, zoom, damping, playfield locking, or camera shake defaults.",
                    "Camera-only, board/card/tabletop, brawler, fighter, and side-scroller setups usually need one."
                },
                createBefore: new[]
                {
                    "GameModeDefinition that will reference this profile.",
                    "Camera Root with CinemachineCameraRigController when using Pyralis camera control."
                },
                assignFirst: new[]
                {
                    "Choose Presentation Mode: Shared or SplitScreen.",
                    "Set orthographic vs perspective intent.",
                    "Tune profile transform, follow offset, pitch/yaw/roll, Min Zoom, Max Zoom, damping, default distance, and orthographic size."
                },
                safeToCustomize: new[]
                {
                    "Apply the 2D orthographic starting point for top-down, board, and 2D pawn movement proofs, then tune the fields for the actual game view. It sets Follow Damping to 0 for snap/no-lag follow.",
                    "Use Profile Transform applies Follow Offset and View Euler Angles to the shared Cinemachine Camera at runtime. Disable it when you want to hand-place and hand-rotate the Cinemachine Camera in the scene.",
                    "View Euler Angles are pitch, yaw, and roll in Unity degrees. For a normal 2D orthographic view, keep them at 0, 0, 0. For angled 3D or 2.5D follow, raise the Follow Offset and add pitch/yaw here.",
                    "Orthographic Size controls how close a 2D view feels. Follow Damping controls how quickly the runtime focus catches up; 0 means immediate. Min/Max Zoom clamp automatic shared-camera framing.",
                    "Use Cinemachine should stay enabled for normal Pyralis camera rigs.",
                    "Lock To Playfield keeps camera behavior tied to PlayfieldProfile bounds.",
                    "Shake amplitude/frequency are defaults; combat/projectile/hazard impacts can still call CameraShake."
                },
                validation: new[]
                {
                    "GameModeDefinition references this profile.",
                    "Camera Root has CinemachineCameraRigController when shared camera control is expected.",
                    "Min Zoom is not larger than Max Zoom."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Camera_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(profile), "Camera rig profile is ready for game-mode assignment.");

            if (!profile.useCinemachine)
                EditorGUILayout.HelpBox("The shared-core rewrite expects Cinemachine-backed rigs by default. Disable this only for specialized cases.", MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawProfileShortcuts()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Camera Starting Points", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField("These apply editable camera starter values to this profile only; tune every field below for the actual game view.", EditorStyles.wordWrappedMiniLabel);
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

        private static System.Collections.Generic.List<string> GetValidationIssues(CameraRigProfile profile)
        {
            System.Collections.Generic.List<string> issues = new System.Collections.Generic.List<string>();

            if (profile != null && profile.minZoom > profile.maxZoom)
                issues.Add("Min Zoom should not exceed Max Zoom.");

            return issues;
        }
    }
}
