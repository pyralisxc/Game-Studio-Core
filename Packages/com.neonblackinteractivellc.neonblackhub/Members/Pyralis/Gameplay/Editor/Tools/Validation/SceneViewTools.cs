using UnityEditor;
using UnityEngine;

/// <summary>
/// Optional Scene view overlays for NeonBlack Gameplay authoring.
/// Toggle each feature through Tools > NeonBlack > Gameplay > Scene View Tools.
/// Preferences persist between editor sessions.
/// </summary>
[InitializeOnLoad]
public static class SceneViewTools
{
    private const string PREF_ROTATION = "NeonBlackGameplayTools_ShowRotation";
    private const string PREF_LAYER = "NeonBlackGameplayTools_ShowLayer";

    private static bool _showRotation;
    private static bool _showLayer;
    private static GUIStyle _rotStyle;
    private static GUIStyle _layerStyle;

    static SceneViewTools()
    {
        _showRotation = EditorPrefs.GetBool(PREF_ROTATION, false);
        _showLayer = EditorPrefs.GetBool(PREF_LAYER, false);

        SceneView.duringSceneGui += OnSceneGUI;
    }

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

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (!_showRotation && !_showLayer)
            return;

        Transform selectedTransform = Selection.activeTransform;
        if (selectedTransform == null)
            return;

        InitStyles();

        Vector3 worldPos = selectedTransform.position;
        float boundsHeight = GetBoundsHeight(selectedTransform);
        Vector3 labelPos = worldPos + Vector3.up * (boundsHeight + 0.15f);
        Vector2 screenPos = HandleUtility.WorldToGUIPoint(labelPos);
        bool rotationVisible = _showRotation && Tools.current == Tool.Rotate;

        Handles.BeginGUI();

        if (rotationVisible)
            DrawRotationLabel(selectedTransform, worldPos, screenPos, boundsHeight);

        if (_showLayer)
            DrawLayerLabel(selectedTransform, screenPos, rotationVisible);

        Handles.EndGUI();

        if (rotationVisible && Event.current != null && Event.current.type == EventType.MouseDrag)
            sceneView.Repaint();
    }

    private static void DrawRotationLabel(Transform selectedTransform, Vector3 worldPos, Vector2 screenPos, float boundsHeight)
    {
        Vector3 euler = selectedTransform.eulerAngles;
        float rx = NormalizeAngle(euler.x);
        float ry = NormalizeAngle(euler.y);
        float rz = NormalizeAngle(euler.z);
        string rotText = $"X {rx:F1} deg  Y {ry:F1} deg  Z {rz:F1} deg";

        Vector2 rotSize = _rotStyle.CalcSize(new GUIContent(rotText));
        Rect rotRect = new Rect(
            screenPos.x - rotSize.x * 0.5f,
            screenPos.y - rotSize.y - 2f,
            rotSize.x,
            rotSize.y);
        GUI.Label(rotRect, rotText, _rotStyle);

        float axisLength = Mathf.Max(boundsHeight * 0.6f, 0.3f);
        Handles.color = new Color(1f, 0.2f, 0.2f, 0.8f);
        Handles.DrawLine(worldPos, worldPos + selectedTransform.right * axisLength, 2f);
        Handles.color = new Color(0.2f, 1f, 0.2f, 0.8f);
        Handles.DrawLine(worldPos, worldPos + selectedTransform.up * axisLength, 2f);
        Handles.color = new Color(0.3f, 0.5f, 1f, 0.8f);
        Handles.DrawLine(worldPos, worldPos + selectedTransform.forward * axisLength, 2f);
        Handles.color = Color.white;
    }

    private static void DrawLayerLabel(Transform selectedTransform, Vector2 screenPos, bool rotationVisible)
    {
        SpriteRenderer spriteRenderer = selectedTransform.GetComponentInChildren<SpriteRenderer>();
        string layerText;
        if (spriteRenderer != null)
        {
            string sortName = string.IsNullOrEmpty(spriteRenderer.sortingLayerName)
                ? "Default"
                : spriteRenderer.sortingLayerName;
            layerText = $"{sortName} [{spriteRenderer.sortingOrder}]";
        }
        else
        {
            layerText = LayerMask.LayerToName(selectedTransform.gameObject.layer);
            if (string.IsNullOrEmpty(layerText))
                layerText = $"Layer {selectedTransform.gameObject.layer}";
        }

        Vector2 layerSize = _layerStyle.CalcSize(new GUIContent(layerText));
        float yShift = rotationVisible ? _rotStyle.CalcSize(new GUIContent("X")).y + 2f : 0f;
        Rect layerRect = new Rect(
            screenPos.x - layerSize.x * 0.5f,
            screenPos.y - layerSize.y - 2f + yShift,
            layerSize.x,
            layerSize.y);
        GUI.Label(layerRect, layerText, _layerStyle);
    }

    private static float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f)
            angle -= 360f;
        if (angle < -180f)
            angle += 360f;
        return angle;
    }

    private static float GetBoundsHeight(Transform selectedTransform)
    {
        Renderer renderer = selectedTransform.GetComponentInChildren<Renderer>();
        if (renderer != null)
            return renderer.bounds.size.y;

        Collider collider = selectedTransform.GetComponentInChildren<Collider>();
        if (collider != null)
            return collider.bounds.size.y;

        return 0.5f;
    }

    private static void InitStyles()
    {
        if (_rotStyle != null)
            return;

        Texture2D backgroundTexture = MakeTex(4, 4, new Color(0f, 0f, 0f, 0.6f));
        _rotStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 11,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white, background = backgroundTexture },
            padding = new RectOffset(5, 5, 2, 2)
        };

        _layerStyle = new GUIStyle(_rotStyle)
        {
            fontSize = 10,
            normal = { textColor = new Color(0.6f, 1f, 0.6f), background = backgroundTexture }
        };
    }

    private static Texture2D MakeTex(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;

        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
}
