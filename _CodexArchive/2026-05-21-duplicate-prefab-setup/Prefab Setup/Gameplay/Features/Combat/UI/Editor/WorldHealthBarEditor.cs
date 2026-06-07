using NeonBlack.Gameplay.Features.Combat;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom inspector for WorldHealthBar.
/// Replaces the plain-text "sortingLayerName" field with a dropdown populated
/// from the project's actual sorting layers so there is no risk of a silent typo.
/// All other fields are drawn normally via the default inspector.
/// </summary>
[CustomEditor(typeof(WorldHealthBar))]
public class WorldHealthBarEditor : Editor
{
    // The two sorting fields we intercept; everything else is drawn normally.
    private SerializedProperty _sortingLayerName;
    private SerializedProperty _sortingOrderInLayer;

    private void OnEnable()
    {
        _sortingLayerName    = serializedObject.FindProperty("sortingLayerName");
        _sortingOrderInLayer = serializedObject.FindProperty("sortingOrderInLayer");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw every property except the two we handle ourselves.
        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;
        while (prop.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (prop.name == "sortingLayerName")
            {
                DrawSortingLayerDropdown();
                continue;
            }

            if (prop.name == "sortingOrderInLayer")
            {
                EditorGUILayout.PropertyField(_sortingOrderInLayer);
                continue;
            }

            // Default draw for everything else (m_Script, all headers, colours, etc.)
            using (new EditorGUI.DisabledScope(prop.name == "m_Script"))
                EditorGUILayout.PropertyField(prop, true);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSortingLayerDropdown()
    {
        // Build parallel arrays of names and IDs from the project's sorting layers.
        SortingLayer[] layers   = SortingLayer.layers;
        string[]       names    = new string[layers.Length];
        int[]          ids      = new int[layers.Length];

        for (int i = 0; i < layers.Length; i++)
        {
            names[i] = layers[i].name;
            ids[i]   = layers[i].id;
        }

        // Find the currently selected index by matching the stored name string.
        string currentName = _sortingLayerName.stringValue;
        int currentIndex = 0;
        for (int i = 0; i < names.Length; i++)
        {
            if (names[i] == currentName)
            {
                currentIndex = i;
                break;
            }
        }

        // Draw the dropdown.
        EditorGUI.BeginChangeCheck();
        int selected = EditorGUILayout.Popup(
            new GUIContent("Sorting Layer", "Sorting layer the health bar canvas is rendered on. Must match the layer used by your character sprites."),
            currentIndex,
            names);

        if (EditorGUI.EndChangeCheck())
            _sortingLayerName.stringValue = names[selected];

        // Warn if the stored name is not in the project's sorting layer list.
        bool valid = false;
        foreach (var l in layers)
            if (l.name == _sortingLayerName.stringValue) { valid = true; break; }

        if (!valid)
            EditorGUILayout.HelpBox(
                $"Sorting layer \"{_sortingLayerName.stringValue}\" does not exist in this project. " +
                "Go to Project Settings → Tags & Layers to create it, or pick an existing layer above.",
                MessageType.Warning);
    }
}
