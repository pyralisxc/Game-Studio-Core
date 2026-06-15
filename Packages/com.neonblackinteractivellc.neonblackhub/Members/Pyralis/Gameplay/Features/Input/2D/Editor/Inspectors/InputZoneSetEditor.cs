using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Features.Input;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom editor for InputZoneSet assets.
/// Select the asset in the Project window to draw and edit dead-zone polygons in the Scene view.
/// </summary>
[CustomEditor(typeof(InputZoneSet))]
public class InputZoneSetEditor : Editor
{
    private bool _editPortrait = true;

    private void OnEnable()
    {
        SceneView.duringSceneGui += DuringSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= DuringSceneGUI;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();

        InputZoneSet asset = (InputZoneSet)target;
        if (asset == null)
            return;

        PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
            "Guided Authoring: Input Zone Set",
            "An input zone set defines portrait and landscape screen polygons where gameplay input should be ignored or reserved for UI.",
            whenToUse: new[]
            {
                "Use this for mobile/touch games, virtual sticks, on-screen buttons, card hands, HUD panels, or board UI regions.",
                "Select the asset to edit polygons directly in the Scene view."
            },
            createBefore: new[]
            {
                "Camera and UI layout that define where touch/gameplay input should be blocked.",
                "Input system or pointer router that checks this zone set."
            },
            assignFirst: new[]
            {
                "Choose Portrait or Landscape editing mode.",
                "Add polygon vertices for each dead zone.",
                "Drag vertices in Scene view until the zones match your UI."
            },
            safeToCustomize: new[]
            {
                "Portrait and Landscape can have different zones.",
                "Zones can be empty when the game has no touch-blocking UI.",
                "Use several simple polygons instead of one hard-to-edit shape."
            },
            validation: new[]
            {
                "Scene-view zones cover the intended UI controls.",
                "Gameplay input is ignored inside zones and works outside zones.",
                "Both orientations are checked if the game supports rotation."
            },
            manualPath: PyralisInspectorGuide.AuthoringDocPath("AUTHORING_MODEL.md")));

        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Scene-View Zone Editor", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Select this asset in the Project window to see and drag dead zones in the Scene view.\n" +
            "Switch orientation below to edit Portrait or Landscape polygon points independently.",
            MessageType.Info);

        DrawOrientationButtons();
        DrawZoneButtons(asset);
    }

    private void DrawOrientationButtons()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            GUI.backgroundColor = _editPortrait ? new Color(0.5f, 0.8f, 1f) : Color.white;
            if (GUILayout.Button("Edit Portrait", GUILayout.Height(28)))
                _editPortrait = true;

            GUI.backgroundColor = !_editPortrait ? new Color(1f, 0.85f, 0.4f) : Color.white;
            if (GUILayout.Button("Edit Landscape", GUILayout.Height(28)))
                _editPortrait = false;

            GUI.backgroundColor = Color.white;
        }

        EditorGUILayout.LabelField(
            "Currently editing: " + (_editPortrait ? "Portrait" : "Landscape"),
            EditorStyles.centeredGreyMiniLabel);
    }

    private void DrawZoneButtons(InputZoneSet asset)
    {
        InputZoneSet.ScreenPolygon[] zones = _editPortrait ? asset.portrait : asset.landscape;
        if (zones == null)
            return;

        EditorGUILayout.Space(6f);
        for (int i = 0; i < zones.Length; i++)
            DrawZoneControls($"Dead Zone [{i}]", zones[i], new Color(1f, 0.2f, 0.2f), asset);
    }

    private void DrawZoneControls(string label, InputZoneSet.ScreenPolygon zone, Color color, InputZoneSet asset)
    {
        if (zone == null)
            return;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel) { richText = true };
        EditorGUILayout.LabelField(
            $"<color=#{ColorToHex(color)}>*</color> {label} ({zone.points?.Length ?? 0} vertices)",
            titleStyle);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("+ Add Vertex", GUILayout.Height(22)))
                MutateZone(asset, "Add Vertex - " + label, zone, AddVertex);

            using (new EditorGUI.DisabledScope(zone.points == null || zone.points.Length == 0))
            {
                if (GUILayout.Button("- Remove Last", GUILayout.Height(22)))
                    MutateZone(asset, "Remove Vertex - " + label, zone, RemoveLastVertex);
            }

            if (GUILayout.Button("Clear", GUILayout.Height(22), GUILayout.Width(60)) &&
                EditorUtility.DisplayDialog(
                    $"Clear {label}?",
                    $"Remove all {zone.points?.Length ?? 0} vertices from {label}?",
                    "Clear",
                    "Cancel"))
            {
                MutateZone(asset, "Clear - " + label, zone, selectedZone => selectedZone.points = new Vector2[0]);
            }
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(2f);
    }

    private void DuringSceneGUI(SceneView sceneView)
    {
        InputZoneSet asset = (InputZoneSet)target;
        if (asset == null)
            return;

        InputZoneSet.ScreenPolygon[] zones = _editPortrait ? asset.portrait : asset.landscape;
        if (zones == null)
            return;

        bool changed = false;
        for (int i = 0; i < zones.Length; i++)
        {
            changed |= DrawEditablePolygon(
                zones[i],
                new Color(1f, 0.1f, 0.1f, 0.18f),
                new Color(1f, 0.2f, 0.2f, 0.85f),
                $"Dead Zone [{i}]",
                asset);
        }

        if (changed)
        {
            MarkAssetChanged(asset);
            Repaint();
        }
    }

    private bool DrawEditablePolygon(
        InputZoneSet.ScreenPolygon zone,
        Color fill,
        Color outline,
        string label,
        InputZoneSet asset)
    {
        if (zone == null || zone.points == null || zone.points.Length == 0)
            return false;

        Vector3[] world = new Vector3[zone.points.Length];
        for (int i = 0; i < zone.points.Length; i++)
            world[i] = new Vector3(zone.points[i].x, zone.points[i].y, 0f);

        if (world.Length >= 3)
        {
            Handles.color = fill;
            Handles.DrawAAConvexPolygon(world);
        }

        Handles.color = outline;
        Vector3[] loop = new Vector3[world.Length + 1];
        world.CopyTo(loop, 0);
        loop[world.Length] = world[0];
        Handles.DrawPolyLine(loop);

        Vector3 centroid = Vector3.zero;
        for (int i = 0; i < world.Length; i++)
            centroid += world[i];
        centroid /= world.Length;
        Handles.Label(centroid, label);

        bool changed = false;
        for (int i = 0; i < zone.points.Length; i++)
        {
            float size = HandleUtility.GetHandleSize(world[i]) * 0.07f;
            Handles.color = Color.white;

            EditorGUI.BeginChangeCheck();
            Vector3 moved = Handles.FreeMoveHandle(world[i], size, Vector3.zero, Handles.SphereHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(asset, "Move " + label + " Vertex");
                zone.points[i] = new Vector2(moved.x, moved.y);
                zone.InvalidateBounds();
                changed = true;
            }

            Handles.Label(world[i] + Vector3.up * (HandleUtility.GetHandleSize(world[i]) * 0.12f), i.ToString());
        }

        return changed;
    }

    private static void MutateZone(
        InputZoneSet asset,
        string undoLabel,
        InputZoneSet.ScreenPolygon zone,
        System.Action<InputZoneSet.ScreenPolygon> mutation)
    {
        Undo.RecordObject(asset, undoLabel);
        mutation(zone);
        zone.InvalidateBounds();
        MarkAssetChanged(asset);
        SceneView.RepaintAll();
    }

    private static void MarkAssetChanged(InputZoneSet asset)
    {
        EditorUtility.SetDirty(asset);
    }

    private static void AddVertex(InputZoneSet.ScreenPolygon zone)
    {
        if (zone.points == null || zone.points.Length == 0)
        {
            Camera camera = SceneView.lastActiveSceneView != null
                ? SceneView.lastActiveSceneView.camera
                : null;
            float halfHeight = camera != null ? camera.orthographicSize : 3f;
            float halfWidth = camera != null ? camera.orthographicSize * camera.aspect : 5f;
            float centerX = camera != null ? camera.transform.position.x : 0f;
            float centerY = camera != null ? camera.transform.position.y : 0f;

            zone.points = new[]
            {
                new Vector2(centerX - halfWidth, centerY - halfHeight),
                new Vector2(centerX, centerY - halfHeight),
                new Vector2(centerX, centerY),
                new Vector2(centerX - halfWidth, centerY)
            };
            return;
        }

        int pointCount = zone.points.Length;
        Vector2 midpoint = (zone.points[pointCount - 1] + zone.points[0]) * 0.5f + new Vector2(0.1f, 0.1f);
        System.Array.Resize(ref zone.points, pointCount + 1);
        zone.points[pointCount] = midpoint;
    }

    private static void RemoveLastVertex(InputZoneSet.ScreenPolygon zone)
    {
        if (zone.points == null || zone.points.Length == 0)
            return;

        System.Array.Resize(ref zone.points, zone.points.Length - 1);
    }

    private static string ColorToHex(Color color)
    {
        return $"{ToByte(color.r):X2}{ToByte(color.g):X2}{ToByte(color.b):X2}";
    }

    private static int ToByte(float value)
    {
        return Mathf.Clamp(Mathf.RoundToInt(value * 255), 0, 255);
    }
}
