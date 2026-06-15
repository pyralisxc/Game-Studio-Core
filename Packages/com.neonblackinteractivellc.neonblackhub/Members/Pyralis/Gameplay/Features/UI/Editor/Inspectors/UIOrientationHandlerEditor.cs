using NeonBlack.Gameplay.Features.UI;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Adds Capture Portrait and Capture Landscape buttons to UIOrientationHandler.
/// </summary>
[CustomEditor(typeof(UIOrientationHandler))]
public class UIOrientationHandlerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UIOrientationHandler handler = (UIOrientationHandler)target;
        if (handler == null)
            return;

        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Capture Current RectTransform", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            GUI.backgroundColor = new Color(0.5f, 0.8f, 1f);
            if (GUILayout.Button("Capture Portrait", GUILayout.Height(28)))
                Capture(handler, true);

            GUI.backgroundColor = new Color(1f, 0.85f, 0.4f);
            if (GUILayout.Button("Capture Landscape", GUILayout.Height(28)))
                Capture(handler, false);

            GUI.backgroundColor = Color.white;
        }

        bool portraitCaptured = handler.portrait.captured;
        bool landscapeCaptured = handler.landscape.captured;

        string status =
            $"Portrait: {(portraitCaptured ? "Captured" : "Not captured - layout will be ignored at runtime")}   |   " +
            $"Landscape: {(landscapeCaptured ? "Captured" : "Not captured - layout will be ignored at runtime")}";
        MessageType messageType = portraitCaptured && landscapeCaptured ? MessageType.Info : MessageType.Warning;
        EditorGUILayout.HelpBox(status, messageType);

        EditorGUILayout.HelpBox(
            "1. Arrange your UI for Portrait, then click Capture Portrait.\n" +
            "2. Rearrange for Landscape, then click Capture Landscape.\n" +
            "3. Resize the Game view to preview each orientation.\n" +
            "Layout is only applied after both orientations have been captured.",
            MessageType.Info);
    }

    private static void Capture(UIOrientationHandler handler, bool portrait)
    {
        Undo.RecordObject(handler, portrait ? "Capture Portrait Layout" : "Capture Landscape Layout");

        if (portrait)
            handler.CapturePortrait();
        else
            handler.CaptureLandscape();

        EditorUtility.SetDirty(handler);
        PrefabUtility.RecordPrefabInstancePropertyModifications(handler);
    }
}
