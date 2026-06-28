using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Modules.Combat;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom inspector for WorldHealthBar. Replaces the sorting layer string with a
/// project-backed dropdown and explains when world-space health UI is appropriate.
/// </summary>
[CustomEditor(typeof(WorldHealthBar))]
public class WorldHealthBarEditor : Editor
{
    private SerializedProperty _sortingLayerName;
    private SerializedProperty _sortingOrderInLayer;

    private void OnEnable()
    {
        _sortingLayerName = serializedObject.FindProperty("sortingLayerName");
        _sortingOrderInLayer = serializedObject.FindProperty("sortingOrderInLayer");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

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

            using (new EditorGUI.DisabledScope(prop.name == "m_Script"))
                EditorGUILayout.PropertyField(prop, true);
        }

        PyralisInspectorValidation.DrawValidationMessages(GetMessages((WorldHealthBar)target), "WorldHealthBar is ready for explicit world-space health presentation.");
        serializedObject.ApplyModifiedProperties();
    }

    private List<PyralisInspectorValidationIssue> GetMessages(WorldHealthBar healthBar)
    {
        List<PyralisInspectorValidationIssue> messages = new List<PyralisInspectorValidationIssue>();

        if (healthBar != null && healthBar.GetComponent<HealthComponent>() == null)
            messages.Add(PyralisInspectorValidationIssue.Required("HealthComponent is required on the same GameObject."));

        SerializedProperty targetCamera = serializedObject.FindProperty("targetCamera");
        if (targetCamera != null && targetCamera.objectReferenceValue == null)
            messages.Add(PyralisInspectorValidationIssue.Recommended("Target Camera is empty. Assign a gameplay camera or set it at runtime so the bar billboards correctly."));

        bool showDamageNumbers = serializedObject.FindProperty("showDamageNumbers")?.boolValue == true;
        bool showHealNumbers = serializedObject.FindProperty("showHealNumbers")?.boolValue == true;
        SerializedProperty damageNumberSink = serializedObject.FindProperty("damageNumberSink");
        if ((showDamageNumbers || showHealNumbers) && damageNumberSink != null && damageNumberSink.objectReferenceValue == null)
            messages.Add(PyralisInspectorValidationIssue.Recommended("Damage Number Sink is empty. Assign DamageNumberSpawner or another IDamageNumberSink to show damage/heal numbers."));

        if (damageNumberSink != null
            && damageNumberSink.objectReferenceValue is Component sinkComponent
            && sinkComponent.GetComponent<IDamageNumberSink>() == null)
        {
            messages.Add(PyralisInspectorValidationIssue.Required("Damage Number Sink must reference a component that implements IDamageNumberSink."));
        }

        return messages;
    }

    private void DrawSortingLayerDropdown()
    {
        SortingLayer[] layers = SortingLayer.layers;
        string[] names = new string[layers.Length];

        for (int i = 0; i < layers.Length; i++)
            names[i] = layers[i].name;

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

        EditorGUI.BeginChangeCheck();
        int selected = EditorGUILayout.Popup(
            new GUIContent(
                "Sorting Layer",
                "Sorting layer the health bar canvas is rendered on. Must match the layer used by your character sprites."),
            currentIndex,
            names);

        if (EditorGUI.EndChangeCheck())
            _sortingLayerName.stringValue = names[selected];

        bool valid = false;
        foreach (SortingLayer layer in layers)
        {
            if (layer.name == _sortingLayerName.stringValue)
            {
                valid = true;
                break;
            }
        }

        if (!valid)
        {
            EditorGUILayout.HelpBox(
                $"Sorting layer \"{_sortingLayerName.stringValue}\" does not exist in this project. Go to Project Settings > Tags and Layers to create it, or pick an existing layer above.",
                MessageType.Warning);
        }
    }
}
