using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Features.Combat;
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

        PyralisInspectorGuide.DrawFieldGuide(
            "Inspector Field Guide: World Health Bar",
            new PyralisGuideSection(
                "What this component does",
                "WorldHealthBar presents health near an actor in the world instead of in a fixed HUD.",
                new[]
                {
                    "Use it for enemies, bosses, destructibles, brawler opponents, or side-scroller actors with visible health.",
                    "Skip it for board/card/tabletop games unless pieces need world-space status markers.",
                    "Keep the health data on HealthComponent; this script is presentation only."
                },
                PyralisInspectorGuide.AuthoringDocPath("Prefabs/Health_Combat_Setup.md")),
            new PyralisGuideSection(
                "Beginner wiring",
                "Most issues are references or render ordering.",
                new[]
                {
                    "Assign the HealthComponent or target health source this bar should watch.",
                    "Place the bar under the actor or set the follow/offset fields so it tracks the intended target.",
                    "Pick a Sorting Layer that exists in Project Settings so the bar renders above sprites.",
                    "Use Sorting Order In Layer to resolve overlap between characters, hit effects, and UI."
                },
                PyralisInspectorGuide.AuthoringDocPath("Prefabs/UI_HUD_Setup.md")),
            new PyralisGuideSection(
                "Path choices",
                "Health can be shown in different places depending on genre.",
                new[]
                {
                    "Arcade/platformer path: use WorldHealthBar for enemies and a HUD bar for the player.",
                    "Fighter/brawler path: use world bars for groups or large enemies, HUD bars for main combatants.",
                    "Turn-based/menu path: show health in the selected target panel or action menu instead.",
                    "Card/tabletop path: show health as counters, tokens, or card UI instead of world-space bars."
                }));

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

        PyralisInspectorGuide.DrawValidationMessages(GetMessages((WorldHealthBar)target), "WorldHealthBar is ready for explicit world-space health presentation.");
        serializedObject.ApplyModifiedProperties();
    }

    private List<PyralisGuideIssue> GetMessages(WorldHealthBar healthBar)
    {
        List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();

        if (healthBar != null && healthBar.GetComponent<HealthComponent>() == null)
            messages.Add(PyralisGuideIssue.Required("HealthComponent is required on the same GameObject."));

        SerializedProperty targetCamera = serializedObject.FindProperty("targetCamera");
        if (targetCamera != null && targetCamera.objectReferenceValue == null)
            messages.Add(PyralisGuideIssue.Recommended("Target Camera is empty. Assign a gameplay camera or set it at runtime so the bar billboards correctly."));

        bool showDamageNumbers = serializedObject.FindProperty("showDamageNumbers")?.boolValue == true;
        bool showHealNumbers = serializedObject.FindProperty("showHealNumbers")?.boolValue == true;
        SerializedProperty damageNumberSink = serializedObject.FindProperty("damageNumberSink");
        if ((showDamageNumbers || showHealNumbers) && damageNumberSink != null && damageNumberSink.objectReferenceValue == null)
            messages.Add(PyralisGuideIssue.Recommended("Damage Number Sink is empty. Assign DamageNumberSpawner or another IDamageNumberSink to show damage/heal numbers."));

        if (damageNumberSink != null
            && damageNumberSink.objectReferenceValue is Component sinkComponent
            && sinkComponent.GetComponent<IDamageNumberSink>() == null)
        {
            messages.Add(PyralisGuideIssue.Required("Damage Number Sink must reference a component that implements IDamageNumberSink."));
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
