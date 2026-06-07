using NeonBlack.Gameplay.Features.UI;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Adds "Capture → Portrait" and "Capture → Landscape" buttons to the
/// UIOrientationHandler Inspector so you don't have to type values manually.
/// </summary>
[CustomEditor(typeof(UIOrientationHandler))]
public class UIOrientationHandlerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UIOrientationHandler handler = (UIOrientationHandler)target;

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Capture current RectTransform", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = new Color(0.5f, 0.8f, 1f);
        if (GUILayout.Button("Capture → Portrait", GUILayout.Height(28)))
        {
            Undo.RecordObject(handler, "Capture Portrait Layout");
            handler.CapturePortrait();
        }

        GUI.backgroundColor = new Color(1f, 0.85f, 0.4f);
        if (GUILayout.Button("Capture → Landscape", GUILayout.Height(28)))
        {
            Undo.RecordObject(handler, "Capture Landscape Layout");
            handler.CaptureLandscape();
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        bool pCaptured = handler.portrait.captured;
        bool lCaptured = handler.landscape.captured;

        string status = $"Portrait: {(pCaptured ? "✓ Captured" : "✗ NOT captured — layout will be ignored at runtime")}   |   " +
                        $"Landscape: {(lCaptured ? "✓ Captured" : "✗ NOT captured — layout will be ignored at runtime")}";
        MessageType msgType = (pCaptured && lCaptured) ? MessageType.Info : MessageType.Warning;
        EditorGUILayout.HelpBox(status, msgType);

        EditorGUILayout.HelpBox(
            "1. Arrange your UI for Portrait → click Capture → Portrait.\n" +
            "2. Rearrange for Landscape → click Capture → Landscape.\n" +
            "Resize the Game view (tab → change aspect) to preview each orientation.\n" +
            "Layout is only applied after BOTH orientations have been captured.",
            MessageType.Info);
    }
}
