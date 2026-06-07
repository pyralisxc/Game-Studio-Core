using NeonBlack.Gameplay.Features.Input;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom editor for InputZoneSet.
/// When the InputZoneSet asset is selected in the Project window:
///   • All dead zone polygons render in the Scene view.
///   • Vertex handles are draggable — move any point directly in the scene.
///   • Use the Inspector buttons to add or remove vertices per zone.
///   • Toggle between Portrait and Landscape to edit each orientation independently.
/// No GameObject selection required.
/// </summary>
[CustomEditor(typeof(InputZoneSet))]
public class InputZoneSetEditor : Editor
{
    // ─────────────────────────────────────────────────────────────────────
    // State
    // ─────────────────────────────────────────────────────────────────────

    private bool _editPortrait = true;

    // ─────────────────────────────────────────────────────────────────────
    // Enable / Disable
    // ─────────────────────────────────────────────────────────────────────

    private void OnEnable()  => SceneView.duringSceneGui += OnSceneGUI;
    private void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

    // ─────────────────────────────────────────────────────────────────────
    // Inspector
    // ─────────────────────────────────────────────────────────────────────

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        InputZoneSet asset = (InputZoneSet)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Scene-View Zone Editor", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Select this asset in the Project window to see and drag dead zones in the Scene view.\n" +
            "Switch orientation below to edit Portrait or Landscape polygon points independently.",
            MessageType.Info);

        // Orientation toggle
        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = _editPortrait ? new Color(0.5f, 0.8f, 1f) : Color.white;
        if (GUILayout.Button("✏  Edit Portrait", GUILayout.Height(28)))
            _editPortrait = true;
        GUI.backgroundColor = !_editPortrait ? new Color(1f, 0.85f, 0.4f) : Color.white;
        if (GUILayout.Button("✏  Edit Landscape", GUILayout.Height(28)))
            _editPortrait = false;
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField($"Currently editing: {(_editPortrait ? "Portrait" : "Landscape")}",
            EditorStyles.centeredGreyMiniLabel);

        InputZoneSet.ScreenPolygon[] zones = _editPortrait ? asset.portrait : asset.landscape;

        EditorGUILayout.Space(6);

        if (zones != null)
        {
            for (int i = 0; i < zones.Length; i++)
                DrawZoneControls($"Dead Zone [{i}]", zones[i], new Color(1f, 0.2f, 0.2f), asset);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(asset);
            SceneView.RepaintAll();
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Inspector zone controls
    // ─────────────────────────────────────────────────────────────────────

    private void DrawZoneControls(string label, InputZoneSet.ScreenPolygon zone,
                                  Color color, InputZoneSet asset)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel) { richText = true };
        EditorGUILayout.LabelField(
            $"<color=#{ColorToHex(color)}>■</color>  {label}  ({zone.points?.Length ?? 0} vertices)",
            titleStyle);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("+ Add Vertex", GUILayout.Height(22)))
        {
            Undo.RecordObject(asset, $"Add Vertex — {label}");
            AddVertex(zone);
            EditorUtility.SetDirty(asset);
            SceneView.RepaintAll();
        }

        GUI.enabled = zone.points != null && zone.points.Length > 0;
        if (GUILayout.Button("− Remove Last", GUILayout.Height(22)))
        {
            Undo.RecordObject(asset, $"Remove Vertex — {label}");
            RemoveLastVertex(zone);
            EditorUtility.SetDirty(asset);
            SceneView.RepaintAll();
        }
        GUI.enabled = true;

        if (GUILayout.Button("✕ Clear", GUILayout.Height(22), GUILayout.Width(60)))
        {
            if (EditorUtility.DisplayDialog($"Clear {label}?",
                    $"Remove all {zone.points?.Length ?? 0} vertices from {label}?", "Clear", "Cancel"))
            {
                Undo.RecordObject(asset, $"Clear — {label}");
                zone.points = new Vector2[0];
                EditorUtility.SetDirty(asset);
                SceneView.RepaintAll();
            }
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(2);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Scene view drawing + handles
    // ─────────────────────────────────────────────────────────────────────

    private void OnSceneGUI(SceneView sceneView)
    {
        InputZoneSet asset = (InputZoneSet)target;
        if (asset == null) return;

        InputZoneSet.ScreenPolygon[] zones = _editPortrait ? asset.portrait : asset.landscape;
        if (zones == null) return;

        bool changed = false;
        for (int i = 0; i < zones.Length; i++)
            changed |= DrawEditablePolygon(zones[i],
                new Color(1f, 0.1f, 0.1f, 0.18f), new Color(1f, 0.2f, 0.2f, 0.85f),
                $"Dead Zone [{i}]", asset);

        if (changed)
        {
            EditorUtility.SetDirty(asset);
            Repaint();
        }
    }

    /// <summary>Draws one polygon with fill, outline, label, and draggable vertex handles.</summary>
    private bool DrawEditablePolygon(InputZoneSet.ScreenPolygon zone,
                                     Color fill, Color outline, string label,
                                     InputZoneSet asset)
    {
        if (zone == null || zone.points == null || zone.points.Length == 0) return false;

        // Points are world-space XY — lift to z=0
        Vector3[] world = new Vector3[zone.points.Length];
        for (int i = 0; i < zone.points.Length; i++)
            world[i] = new Vector3(zone.points[i].x, zone.points[i].y, 0f);

        // Fill
        if (world.Length >= 3)
        {
            Handles.color = fill;
            Handles.DrawAAConvexPolygon(world);
        }

        // Closed outline
        Handles.color = outline;
        Vector3[] loop = new Vector3[world.Length + 1];
        world.CopyTo(loop, 0);
        loop[world.Length] = world[0];
        Handles.DrawPolyLine(loop);

        // Centroid label
        Vector3 centroid = Vector3.zero;
        foreach (Vector3 p in world) centroid += p;
        centroid /= world.Length;
        Handles.Label(centroid, label);

        // Vertex drag handles
        bool changed = false;
        for (int i = 0; i < zone.points.Length; i++)
        {
            float size = HandleUtility.GetHandleSize(world[i]) * 0.07f;
            Handles.color = Color.white;

            EditorGUI.BeginChangeCheck();
            Vector3 moved = Handles.FreeMoveHandle(world[i], size, Vector3.zero, Handles.SphereHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(asset, $"Move {label} Vertex");
                zone.points[i] = new Vector2(moved.x, moved.y);
                changed = true;
            }

            Handles.Label(world[i] + Vector3.up * (HandleUtility.GetHandleSize(world[i]) * 0.12f), $"{i}");
        }

        return changed;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Vertex helpers
    // ─────────────────────────────────────────────────────────────────────

    private static void AddVertex(InputZoneSet.ScreenPolygon zone)
    {
        if (zone.points == null || zone.points.Length == 0)
        {
            Camera cam = Camera.main;
            float halfH = cam != null ? cam.orthographicSize             : 3f;
            float halfW = cam != null ? cam.orthographicSize * cam.aspect : 5f;
            float cx    = cam != null ? cam.transform.position.x         : 0f;
            float cy    = cam != null ? cam.transform.position.y         : 0f;
            float left   = cx - halfW;
            float bottom = cy - halfH;
            float right  = cx;
            float top    = cy;
            zone.points = new Vector2[]
            {
                new Vector2(left,  bottom),
                new Vector2(right, bottom),
                new Vector2(right, top),
                new Vector2(left,  top)
            };
            return;
        }

        int n = zone.points.Length;
        Vector2 midpoint = (zone.points[n - 1] + zone.points[0]) * 0.5f + new Vector2(0.1f, 0.1f);
        System.Array.Resize(ref zone.points, n + 1);
        zone.points[n] = midpoint;
    }

    private static void RemoveLastVertex(InputZoneSet.ScreenPolygon zone)
    {
        if (zone.points == null || zone.points.Length == 0) return;
        System.Array.Resize(ref zone.points, zone.points.Length - 1);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Utility
    // ─────────────────────────────────────────────────────────────────────

    private static string ColorToHex(Color c)
    {
        return $"{ToByte(c.r):X2}{ToByte(c.g):X2}{ToByte(c.b):X2}";
    }

    private static int ToByte(float f) => Mathf.Clamp(Mathf.RoundToInt(f * 255), 0, 255);
}



