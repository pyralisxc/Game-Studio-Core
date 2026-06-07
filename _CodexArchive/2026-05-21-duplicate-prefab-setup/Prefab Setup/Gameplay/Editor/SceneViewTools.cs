using UnityEditor;
using UnityEngine;

/// <summary>
/// Scene-view utility overlays for NeonBlack Gameplay authoring.
///
/// Features:
///   1. Rotation Label  — shows live Euler angles (X / Y / Z) as a world-space
///      label above the selected object while you drag the rotation handle.
///   2. Layer Badge     — shows the object's layer name in a small badge so you
///      never have to check the Inspector just to see what layer something is on.
///
/// No component needed — works on any selected GameObject automatically.
/// Toggle each feature via Tools > NeonBlack > Gameplay > Scene View Tools.
/// Preferences persist between sessions via EditorPrefs.
/// </summary>
[InitializeOnLoad]
public static class SceneViewTools
{
    // ── EditorPrefs keys ──────────────────────────────────────────────────── //
    private const string PREF_ROTATION = "NeonBlackGameplayTools_ShowRotation";
    private const string PREF_LAYER    = "NeonBlackGameplayTools_ShowLayer";

    // ── State ─────────────────────────────────────────────────────────────── //
    private static bool _showRotation;
    private static bool _showLayer;

    // Cached styles — rebuilt once per domain reload.
    private static GUIStyle _rotStyle;
    private static GUIStyle _layerStyle;

    // ── Constructor — called once when the editor loads ───────────────────── //
    static SceneViewTools()
    {
        _showRotation = EditorPrefs.GetBool(PREF_ROTATION, true);
        _showLayer    = EditorPrefs.GetBool(PREF_LAYER,    true);

        SceneView.duringSceneGui += OnSceneGUI;
    }

    // ── Menu items ────────────────────────────────────────────────────────── //
    [MenuItem("Tools/NeonBlack/Gameplay/Scene View Tools/Toggle Rotation Label")]
    private static void ToggleRotation()
    {
        _showRotation = !_showRotation;
        EditorPrefs.SetBool(PREF_ROTATION, _showRotation);
        SceneView.RepaintAll();
    }

    [MenuItem("Tools/NeonBlack/Gameplay/Scene View Tools/Toggle Rotation Label", validate = true)]
    private static bool ToggleRotationValidate()
    {
        Menu.SetChecked("Tools/NeonBlack/Gameplay/Scene View Tools/Toggle Rotation Label", _showRotation);
        return true;
    }

    [MenuItem("Tools/NeonBlack/Gameplay/Scene View Tools/Toggle Layer Badge")]
    private static void ToggleLayer()
    {
        _showLayer = !_showLayer;
        EditorPrefs.SetBool(PREF_LAYER, _showLayer);
        SceneView.RepaintAll();
    }

    [MenuItem("Tools/NeonBlack/Gameplay/Scene View Tools/Toggle Layer Badge", validate = true)]
    private static bool ToggleLayerValidate()
    {
        Menu.SetChecked("Tools/NeonBlack/Gameplay/Scene View Tools/Toggle Layer Badge", _showLayer);
        return true;
    }

    // ── Scene GUI ─────────────────────────────────────────────────────────── //
    private static void OnSceneGUI(SceneView sceneView)
    {
        Transform t = Selection.activeTransform;
        if (t == null) return;

        InitStyles();

        Handles.BeginGUI();

        Vector3 worldPos  = t.position;
        // Offset the label above the object's bounds so it doesn't overlap the gizmo.
        float   boundsH   = GetBoundsHeight(t);
        float   yOffset   = boundsH + 0.15f;
        Vector3 labelPos  = worldPos + Vector3.up * yOffset;

        Vector2 screenPos = HandleUtility.WorldToGUIPoint(labelPos);

        if (_showRotation && Tools.current == Tool.Rotate)
        {
            Vector3 euler = t.eulerAngles;
            // Remap angles to -180..180 range so 359° shows as -1° — more readable.
            float rx = NormalizeAngle(euler.x);
            float ry = NormalizeAngle(euler.y);
            float rz = NormalizeAngle(euler.z);

            string rotText = $"X {rx:F1}°  Y {ry:F1}°  Z {rz:F1}°";

            Vector2 rotSize = _rotStyle.CalcSize(new GUIContent(rotText));
            Rect    rotRect = new Rect(screenPos.x - rotSize.x * 0.5f,
                                       screenPos.y - rotSize.y - 2f,
                                       rotSize.x, rotSize.y);
            GUI.Label(rotRect, rotText, _rotStyle);

            // Coloured axis lines so you can see orientation at a glance.
            float axisLen = Mathf.Max(boundsH * 0.6f, 0.3f);
            Handles.color = new Color(1f, 0.2f, 0.2f, 0.8f);
            Handles.DrawLine(worldPos, worldPos + t.right   * axisLen, 2f);
            Handles.color = new Color(0.2f, 1f, 0.2f, 0.8f);
            Handles.DrawLine(worldPos, worldPos + t.up      * axisLen, 2f);
            Handles.color = new Color(0.3f, 0.5f, 1f, 0.8f);
            Handles.DrawLine(worldPos, worldPos + t.forward * axisLen, 2f);
            Handles.color = Color.white;
        }

        if (_showLayer)
        {
            // Show Sorting Layer + Order in Layer from the nearest SpriteRenderer.
            SpriteRenderer sr = t.GetComponentInChildren<SpriteRenderer>();
            string layerText;
            if (sr != null)
            {
                string sortName = sr.sortingLayerName;
                if (string.IsNullOrEmpty(sortName)) sortName = "Default";
                layerText = $"{sortName}  [{sr.sortingOrder}]";
            }
            else
            {
                // Fallback to physics layer name for non-sprite objects.
                layerText = LayerMask.LayerToName(t.gameObject.layer);
                if (string.IsNullOrEmpty(layerText))
                    layerText = $"Layer {t.gameObject.layer}";
            }

            Vector2 layerSize = _layerStyle.CalcSize(new GUIContent(layerText));
            // Stack below rotation label when both are visible.
            bool rotVisible = _showRotation && Tools.current == Tool.Rotate;
            float yShift = rotVisible ? _rotStyle.CalcSize(new GUIContent("X")).y + 2f : 0f;
            Rect layerRect = new Rect(screenPos.x - layerSize.x * 0.5f,
                                      screenPos.y - layerSize.y - 2f + yShift,
                                      layerSize.x, layerSize.y);
            GUI.Label(layerRect, layerText, _layerStyle);
        }

        Handles.EndGUI();

        // Force the scene view to repaint every frame so the label tracks the
        // rotation handle in real time as you drag.
        if (_showRotation || _showLayer)
            sceneView.Repaint();
    }

    // ── Helpers ───────────────────────────────────────────────────────────── //

    /// Remap 0–360 to -180–180 for display.
    private static float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f)  angle -= 360f;
        if (angle < -180f) angle += 360f;
        return angle;
    }

    /// Rough bounds height so the label floats above the mesh instead of inside it.
    private static float GetBoundsHeight(Transform t)
    {
        Renderer r = t.GetComponentInChildren<Renderer>();
        if (r != null) return r.bounds.size.y;

        Collider c = t.GetComponentInChildren<Collider>();
        if (c != null) return c.bounds.size.y;

        return 0.5f;
    }

    private static void InitStyles()
    {
        if (_rotStyle != null) return;

        _rotStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize        = 11,
            fontStyle       = FontStyle.Bold,
            alignment       = TextAnchor.MiddleCenter,
            normal          = { textColor = Color.white },
        };
        // Dark background so it's readable against any scene backdrop.
        Texture2D bgTex = MakeTex(4, 4, new Color(0f, 0f, 0f, 0.6f));
        _rotStyle.normal.background  = bgTex;
        _rotStyle.padding            = new RectOffset(5, 5, 2, 2);

        _layerStyle = new GUIStyle(_rotStyle)
        {
            fontSize = 10,
            normal   = { textColor = new Color(0.6f, 1f, 0.6f), background = bgTex }
        };
    }

    private static Texture2D MakeTex(int w, int h, Color col)
    {
        Color[] pix = new Color[w * h];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
        Texture2D tex = new Texture2D(w, h);
        tex.SetPixels(pix);
        tex.Apply();
        return tex;
    }
}
