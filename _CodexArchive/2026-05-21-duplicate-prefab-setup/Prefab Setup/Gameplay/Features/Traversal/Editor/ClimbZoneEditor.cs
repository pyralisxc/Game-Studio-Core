using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Traversal.Editor
{
/// <summary>
/// Custom editor for `ClimbZone` that draws inspector guidance and
/// provides in-scene handles for control points (CP1, CP2) and the
/// stand-up point. Enhances designer workflow by documenting required
/// animator parameters and showing draggable gizmos.
/// </summary>
[CustomEditor(typeof(ClimbZone))]
public class ClimbZoneEditor : UnityEditor.Editor
{
    // ── Styles ────────────────────────────────────────────────────────────── //
    private static GUIStyle _headerStyle;
    private static GUIStyle _paramNameStyle;
    private static GUIStyle _paramTypeStyle;

    private static void EnsureStyles()
    {
        if (_headerStyle == null)
            _headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 11 };

        if (_paramNameStyle == null)
            _paramNameStyle = new GUIStyle(EditorStyles.boldLabel)
                { normal = { textColor = new Color(0.4f, 0.85f, 1f) }, fontSize = 11 };

        if (_paramTypeStyle == null)
            _paramTypeStyle = new GUIStyle(EditorStyles.miniLabel)
                { normal = { textColor = new Color(0.65f, 0.65f, 0.65f) }, fontSize = 10 };
    }

    // ── Inspector ─────────────────────────────────────────────────────────── //
    public override void OnInspectorGUI()
    {
        EnsureStyles();
        serializedObject.Update();
        DrawDefaultInspector();

        // ── 1. Grab Detector setup notice ────────────────────────────────── //
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Grab Detector Setup", _headerStyle);
        EditorGUILayout.HelpBox(
            "Detection is driven by the GrabDetector component on a child GameObject of the Player at hand/chest height (~local Y 1.4).\n\n" +
            "Setup:\n" +
            "  1. Add a child GameObject to the Player named 'GrabDetector'.\n" +
            "  2. Set its local position to (0, 1.4, 0).\n" +
            "  3. Add a BoxCollider (Is Trigger = true, Size ~0.5 x 0.4 x 0.5).\n" +
            "  4. Add the GrabDetector component to it.\n\n" +
            "No tag required — GrabDetector calls TryGrab() directly on this ClimbZone.",
            MessageType.Info);

        // ── 2. Required Animator Parameters ─────────────────────────────── //
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Required Animator Parameters", _headerStyle);
        EditorGUILayout.HelpBox(
            "Add these parameters to the Player Animator Controller. Missing parameters are silently skipped — no errors, but animations won't play.",
            MessageType.None);

        EditorGUILayout.Space(2);

        // Climb trigger depends on ClimbType
        SerializedProperty climbTypeProp = serializedObject.FindProperty("climbType");
        bool isSide = climbTypeProp.enumValueIndex == (int)ClimbZone.ClimbType.Side;

        DrawParamRow(isSide ? "SideClimb" : "FwdClimb", "Trigger",
            isSide ? "Fires at the start of a side ledge climb." : "Fires at the start of a forward climb (e.g. ladder top).");
        DrawParamRow("ClimbUp", "Trigger",
            "Plays the climb-up / pull-up animation when the player reaches the top.");

        // Hang-state params (only if hangOnGrab is enabled)
        SerializedProperty hangProp = serializedObject.FindProperty("hangOnGrab");
        if (hangProp.boolValue)
        {
            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("  — Hang State (hangOnGrab = true)", EditorStyles.miniLabel);
            DrawParamRow("IsHanging",   "Bool",    "True while the player hangs on the ledge; false otherwise.");
            DrawParamRow("ShimmySpeed", "Float",   "Lateral shimmy velocity fed each frame (typically −1 to 1).");
            DrawParamRow("LedgeDrop",   "Trigger", "Fires when the player drops down from the ledge.");
        }

        serializedObject.ApplyModifiedProperties();
    }

    private static void DrawParamRow(string paramName, string paramType, string description)
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        GUILayout.Label(paramName, _paramNameStyle, GUILayout.Width(115));
        GUILayout.Label(paramType, _paramTypeStyle, GUILayout.Width(52));
        GUILayout.Label(description, EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.EndHorizontal();
    }

    // ── Scene View ────────────────────────────────────────────────────────── //
    private void OnSceneGUI()
    {
        ClimbZone zone = (ClimbZone)target;

        SerializedProperty cp1Prop = serializedObject.FindProperty("controlPoint1");
        SerializedProperty cp2Prop = serializedObject.FindProperty("controlPoint2");
        SerializedProperty supProp = serializedObject.FindProperty("standUpOffset");

        Vector3 cp1World  = zone.transform.TransformPoint(cp1Prop.vector3Value);
        Vector3 cp2World  = zone.transform.TransformPoint(cp2Prop.vector3Value);
        Vector3 standWorld = zone.transform.TransformPoint(supProp.vector3Value);
        Vector3 origin    = zone.transform.position;

        // --- Draggable handle for CP1 ---
        EditorGUI.BeginChangeCheck();
        Vector3 newCp1 = Handles.PositionHandle(cp1World, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(zone, "Move Climb CP1");
            cp1Prop.vector3Value = zone.transform.InverseTransformPoint(newCp1);
            serializedObject.ApplyModifiedProperties();
        }

        // --- Draggable handle for CP2 ---
        EditorGUI.BeginChangeCheck();
        Vector3 newCp2 = Handles.PositionHandle(cp2World, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(zone, "Move Climb CP2");
            cp2Prop.vector3Value = zone.transform.InverseTransformPoint(newCp2);
            serializedObject.ApplyModifiedProperties();
        }

        // --- Draggable handle for Stand-up point ---
        EditorGUI.BeginChangeCheck();
        Vector3 newStand = Handles.PositionHandle(standWorld, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(zone, "Move Climb Stand-Up Point");
            supProp.vector3Value = zone.transform.InverseTransformPoint(newStand);
            serializedObject.ApplyModifiedProperties();
        }

        // --- Bezier curve preview via Handles ---
        Handles.DrawBezier(origin, standWorld, cp1World, cp2World, Color.cyan, null, 2.5f);

        // Tangent lines from endpoints to their control points
        Handles.color = new Color(0f, 1f, 1f, 0.35f);
        Handles.DrawLine(origin,     cp1World);
        Handles.DrawLine(standWorld, cp2World);

        // Labels
        GUIStyle label = new GUIStyle { normal = { textColor = Color.cyan }, fontSize = 11 };
        Handles.Label(cp1World  + Vector3.up * 0.18f, "CP1 (pull-up)",  label);
        Handles.Label(cp2World  + Vector3.up * 0.18f, "CP2 (land)",     label);
        Handles.Label(standWorld + Vector3.up * 0.18f, "Stand-up",      new GUIStyle { normal = { textColor = Color.yellow }, fontSize = 11 });
    }
}
}
