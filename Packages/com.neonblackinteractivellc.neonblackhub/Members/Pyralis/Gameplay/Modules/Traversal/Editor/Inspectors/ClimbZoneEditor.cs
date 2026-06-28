using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Traversal.Editor
{
    /// <summary>
    /// Custom editor for ClimbZone. Adds beginner setup guidance and scene handles
    /// for control points and the stand-up point.
    /// </summary>
    [CustomEditor(typeof(ClimbZone))]
    public class ClimbZoneEditor : UnityEditor.Editor
    {
        private static GUIStyle _headerStyle;
        private static GUIStyle _paramNameStyle;
        private static GUIStyle _paramTypeStyle;

        private static void EnsureStyles()
        {
            if (_headerStyle == null)
                _headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 11 };

            if (_paramNameStyle == null)
                _paramNameStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    normal = { textColor = new Color(0.4f, 0.85f, 1f) },
                    fontSize = 11
                };

            if (_paramTypeStyle == null)
                _paramTypeStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    normal = { textColor = new Color(0.65f, 0.65f, 0.65f) },
                    fontSize = 10
                };
        }

        public override void OnInspectorGUI()
        {
            EnsureStyles();
            serializedObject.Update();

            DrawDefaultInspector();
            DrawGrabDetectorSetup();
            DrawAnimatorParameters();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGrabDetectorSetup()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Grab Detector Setup", _headerStyle);
            EditorGUILayout.HelpBox(
                "Detection is driven by the GrabDetector component on a child GameObject of the player at hand/chest height, usually around local Y 1.4.\n\n" +
                "Setup:\n" +
                "1. Add a child GameObject to the player named GrabDetector.\n" +
                "2. Set its local position near the hands or chest.\n" +
                "3. Add a BoxCollider, enable Is Trigger, and size it around the grab area.\n" +
                "4. Add the GrabDetector component to it.\n\n" +
                "No tag is required. GrabDetector calls TryGrab() directly on this ClimbZone.",
                MessageType.Info);
        }

        private void DrawAnimatorParameters()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Required Animator Parameters", _headerStyle);
            EditorGUILayout.HelpBox(
                "Add these parameters to the player Animator Controller when you want climb animations. Missing parameters are skipped, so gameplay can still run while you wire animation later.",
                MessageType.None);

            EditorGUILayout.Space(2f);

            SerializedProperty climbTypeProp = serializedObject.FindProperty("climbType");
            bool isSide = climbTypeProp.enumValueIndex == (int)ClimbZone.ClimbType.Side;

            DrawParamRow(
                isSide ? "SideClimb" : "FwdClimb",
                "Trigger",
                isSide ? "Fires at the start of a side ledge climb." : "Fires at the start of a forward climb or ladder-top climb.");
            DrawParamRow("ClimbUp", "Trigger", "Plays the climb-up or pull-up animation when the player reaches the top.");

            SerializedProperty hangProp = serializedObject.FindProperty("hangOnGrab");
            if (hangProp.boolValue)
            {
                EditorGUILayout.Space(2f);
                EditorGUILayout.LabelField("Hang State (hangOnGrab = true)", EditorStyles.miniLabel);
                DrawParamRow("IsHanging", "Bool", "True while the player hangs on the ledge; false otherwise.");
                DrawParamRow("ShimmySpeed", "Float", "Lateral shimmy velocity fed each frame, usually -1 to 1.");
                DrawParamRow("LedgeDrop", "Trigger", "Fires when the player drops down from the ledge.");
            }
        }

        private static void DrawParamRow(string paramName, string paramType, string description)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label(paramName, _paramNameStyle, GUILayout.Width(115f));
            GUILayout.Label(paramType, _paramTypeStyle, GUILayout.Width(52f));
            GUILayout.Label(description, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndHorizontal();
        }

        private void OnSceneGUI()
        {
            ClimbZone zone = (ClimbZone)target;

            SerializedProperty cp1Prop = serializedObject.FindProperty("controlPoint1");
            SerializedProperty cp2Prop = serializedObject.FindProperty("controlPoint2");
            SerializedProperty supProp = serializedObject.FindProperty("standUpOffset");

            Vector3 cp1World = zone.transform.TransformPoint(cp1Prop.vector3Value);
            Vector3 cp2World = zone.transform.TransformPoint(cp2Prop.vector3Value);
            Vector3 standWorld = zone.transform.TransformPoint(supProp.vector3Value);
            Vector3 origin = zone.transform.position;

            EditorGUI.BeginChangeCheck();
            Vector3 newCp1 = Handles.PositionHandle(cp1World, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(zone, "Move Climb CP1");
                cp1Prop.vector3Value = zone.transform.InverseTransformPoint(newCp1);
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.BeginChangeCheck();
            Vector3 newCp2 = Handles.PositionHandle(cp2World, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(zone, "Move Climb CP2");
                cp2Prop.vector3Value = zone.transform.InverseTransformPoint(newCp2);
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.BeginChangeCheck();
            Vector3 newStand = Handles.PositionHandle(standWorld, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(zone, "Move Climb Stand-Up Point");
                supProp.vector3Value = zone.transform.InverseTransformPoint(newStand);
                serializedObject.ApplyModifiedProperties();
            }

            Handles.DrawBezier(origin, standWorld, cp1World, cp2World, Color.cyan, null, 2.5f);

            Handles.color = new Color(0f, 1f, 1f, 0.35f);
            Handles.DrawLine(origin, cp1World);
            Handles.DrawLine(standWorld, cp2World);

            GUIStyle cyanLabel = new GUIStyle { normal = { textColor = Color.cyan }, fontSize = 11 };
            GUIStyle yellowLabel = new GUIStyle { normal = { textColor = Color.yellow }, fontSize = 11 };
            Handles.Label(cp1World + Vector3.up * 0.18f, "CP1 (pull-up)", cyanLabel);
            Handles.Label(cp2World + Vector3.up * 0.18f, "CP2 (land)", cyanLabel);
            Handles.Label(standWorld + Vector3.up * 0.18f, "Stand-up", yellowLabel);
        }
    }
}
