using System.Collections.Generic;
using NeonBlack.Gameplay.Features.Encounters;
using NeonBlack.Gameplay.Features.Environment;
using NeonBlack.Gameplay.Features.Traversal;
using NeonBlack.Gameplay.Presentation.Visuals;
using UnityEditor;
using UnityEngine;
using static NeonBlack.Gameplay.Editor.Inspectors.SceneGameFlowEditorUtility;
using static NeonBlack.Gameplay.Editor.Inspectors.VisualWorldHelperEditorUtility;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(SpriteFlasher))]
    public sealed class SpriteFlasherEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Sprite Flasher",
                new PyralisGuideSection(
                    "What This Is",
                    "SpriteFlasher plays FlashPresetSO color effects on one or more SpriteRenderer targets for hazards, actors, UI sprites, and background feedback.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Hazards_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Assign SpriteRenderers manually or keep Auto Find Renderers enabled.",
                        "Assign Default Preset when Play() or Play On Start should work without an explicit preset.",
                        "Use PlayOneShot for temporary feedback that should ignore the preset loop count.",
                        "Keep finite presets on UI or gameplay feedback unless an intentional looping effect is needed."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not disable Auto Find Renderers while leaving Renderers empty.",
                        "Do not enable Play On Start without a Default Preset.",
                        "Do not leave empty renderer slots; null entries are skipped and can hide setup mistakes."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetSpriteFlasherMessages(serializedObject), "SpriteFlasher needs renderers or Auto Find Renderers.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetSpriteFlasherMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty renderers = serializedObject.FindProperty("_renderers");
            SerializedProperty autoFind = serializedObject.FindProperty("_autoFindRenderers");
            SerializedProperty defaultPreset = serializedObject.FindProperty("_defaultPreset");
            SerializedProperty playOnStart = serializedObject.FindProperty("_playOnStart");

            bool hasRenderer = HasAnyAssignedArrayObject(renderers);
            bool canAutoFind = autoFind != null && autoFind.boolValue;
            if (!hasRenderer && !canAutoFind)
                messages.Add(PyralisGuideIssue.Required("SpriteFlasher needs renderers or Auto Find Renderers."));

            AddArrayNullWarning(messages, renderers, "Renderers");

            if (playOnStart != null && playOnStart.boolValue && (defaultPreset == null || defaultPreset.objectReferenceValue == null))
                messages.Add(PyralisGuideIssue.Required("Play On Start needs a Default Preset."));

            if (defaultPreset == null || defaultPreset.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Optional("Default Preset is empty. Play() will warn unless another script passes a preset."));

            return messages;
        }
    }

    [CustomEditor(typeof(TextFlasher))]
    public sealed class TextFlasherEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Text Flasher",
                new PyralisGuideSection(
                    "What This Is",
                    "TextFlasher plays FlashPresetSO color effects on TMP_Text targets using TMP_Text.color, so UI and world text can share the same flash presets as sprites.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Systems/Architecture_Overview.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Assign TMP_Text targets manually or keep Auto Find Texts enabled.",
                        "Assign Default Preset when Play() or Play On Start should work without an explicit preset.",
                        "Use this for labels, HUD text, popup text, and any TextMeshProUGUI or TextMeshPro feedback.",
                        "Keep Auto Find Texts on when the component sits on a page root with child labels."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not disable Auto Find Texts while leaving Texts empty.",
                        "Do not enable Play On Start without a Default Preset.",
                        "Do not drive TMP material color from a separate script at the same time unless the ownership is clear."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetTextFlasherMessages(serializedObject), "TextFlasher needs text targets or Auto Find Texts.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetTextFlasherMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty texts = serializedObject.FindProperty("_texts");
            SerializedProperty autoFind = serializedObject.FindProperty("_autoFindTexts");
            SerializedProperty defaultPreset = serializedObject.FindProperty("_defaultPreset");
            SerializedProperty playOnStart = serializedObject.FindProperty("_playOnStart");

            bool hasText = HasAnyAssignedArrayObject(texts);
            bool canAutoFind = autoFind != null && autoFind.boolValue;
            if (!hasText && !canAutoFind)
                messages.Add(PyralisGuideIssue.Required("TextFlasher needs text targets or Auto Find Texts."));

            AddArrayNullWarning(messages, texts, "Texts");

            if (playOnStart != null && playOnStart.boolValue && (defaultPreset == null || defaultPreset.objectReferenceValue == null))
                messages.Add(PyralisGuideIssue.Required("Play On Start needs a Default Preset."));

            if (defaultPreset == null || defaultPreset.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Optional("Default Preset is empty. Play() will warn unless another script passes a preset."));

            return messages;
        }
    }

    [CustomEditor(typeof(DepthSorting))]
    public sealed class DepthSortingEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Depth Sorting",
                new PyralisGuideSection(
                    "What This Is",
                    "DepthSorting updates a SpriteRenderer sorting order from world Z so 2.5D characters and props layer correctly as they move through depth.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Systems/Architecture_Overview.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place this on the SpriteRenderer child, not the gameplay root.",
                        "Keep all depth-sorted characters on the same Sorting Layer.",
                        "Tune Sorting Scale so nearby actors separate without huge order jumps.",
                        "Use Base Order only for intentional always-above offsets."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not place this on a root object with no SpriteRenderer.",
                        "Do not set Sorting Scale to zero unless depth layering should be disabled.",
                        "Do not mix many unrelated Sorting Layers when characters should overlap each other."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetDepthSortingMessages(serializedObject, (DepthSorting)target), "DepthSorting should live on a SpriteRenderer child.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetDepthSortingMessages(SerializedObject serializedObject, DepthSorting depthSorting)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            if (depthSorting != null && depthSorting.GetComponent<SpriteRenderer>() == null)
                messages.Add(PyralisGuideIssue.Required("SpriteRenderer is required on the same GameObject."));

            SerializedProperty sortingScale = serializedObject.FindProperty("sortingScale");
            if (sortingScale != null && sortingScale.floatValue == 0f)
                messages.Add(PyralisGuideIssue.Optional("Sorting Scale is zero, so world Z will not affect sorting order."));

            if (depthSorting != null && depthSorting.transform.parent == null)
                messages.Add(PyralisGuideIssue.Optional("No parent transform found. This can work, but character sprites usually sort from the parent/root Z position."));

            return messages;
        }
    }

    [CustomEditor(typeof(ArenaZone))]
    public sealed class ArenaZoneEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Arena Zone",
                new PyralisGuideSection(
                    "What This Is",
                    "ArenaZone defines a combat section that triggers enemy spawners, raises exit blockers, tracks spawned enemies, and clears the zone once all tracked enemies are dead.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Systems/Architecture_Overview.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Size the BoxCollider around the player entry volume; Awake forces it to Is Trigger.",
                        "Assign Enemy Spawners that should activate when the player enters.",
                        "Assign Exit Blockers that should close during combat and reopen when clear.",
                        "Assign camera profiles only when this arena should change camera framing."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not leave Player Tag blank; the zone will never trigger.",
                        "Do not forget EnemySpawner tracked enemies if enemies already exist before activation.",
                        "Do not use negative camera transition duration."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetArenaZoneMessages(serializedObject, (ArenaZone)target), "ArenaZone needs a BoxCollider trigger and player tag.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetArenaZoneMessages(SerializedObject serializedObject, ArenaZone zone)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            BoxCollider box = zone != null ? zone.GetComponent<BoxCollider>() : null;
            if (box == null)
                messages.Add(PyralisGuideIssue.Required("BoxCollider is required for the arena trigger volume."));
            else if (!box.isTrigger)
                messages.Add(PyralisGuideIssue.Optional("BoxCollider is not set to Is Trigger yet. Awake will force it on, but authoring it as a trigger makes scene intent clearer."));

            RequireString(serializedObject, messages, "playerTag", "Player Tag");
            RequireNonNegative(serializedObject, messages, "cameraTransitionDuration", "Camera Transition Duration");
            AddArrayNullWarning(messages, serializedObject.FindProperty("enemySpawners"), "Enemy Spawners");
            AddArrayNullWarning(messages, serializedObject.FindProperty("exitBlockers"), "Exit Blockers");

            return messages;
        }
    }

    [CustomEditor(typeof(TilemapGround))]
    public sealed class TilemapGroundEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Tilemap Ground",
                new PyralisGuideSection(
                    "What This Is",
                    "TilemapGround bakes a 2D Tilemap footprint into a simple 3D XZ ground mesh, optional MeshCollider, and ground layer assignment.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Systems/Architecture_Overview.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Assign Source Tilemap before baking.",
                        "Assign Ground Material when the generated mesh should use a specific material.",
                        "Keep Use Collider enabled when actors should walk or collide on the baked ground.",
                        "Make sure Ground Layer Name matches a layer that exists in Project Settings."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not leave Source Tilemap empty; BakeTilemapToGround cannot build a mesh.",
                        "Do not use a missing layer name; LayerMask.NameToLayer returns -1.",
                        "Do not expect this to render individual tile art; it creates one ground plane using the material."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetTilemapGroundMessages(serializedObject), "TilemapGround needs a Source Tilemap.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetTilemapGroundMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            RequireObject(serializedObject, messages, "groundTilemap", "Source Tilemap");
            RequireString(serializedObject, messages, "_groundLayerName", "Ground Layer Name");

            SerializedProperty layer = serializedObject.FindProperty("_groundLayerName");
            if (layer != null && !string.IsNullOrWhiteSpace(layer.stringValue) && LayerMask.NameToLayer(layer.stringValue) < 0)
                messages.Add(PyralisGuideIssue.Required("Ground Layer Name does not match a layer in Project Settings."));

            if (!HasObject(serializedObject, "groundMaterial"))
                messages.Add(PyralisGuideIssue.Optional("Ground Material is empty. BakeTilemapToGround will create a default URP Lit material at runtime/editor time."));

            return messages;
        }
    }

    [CustomEditor(typeof(GrabDetector))]
    public sealed class GrabDetectorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Grab Detector",
                new PyralisGuideSection(
                    "What This Is",
                    "GrabDetector is a child trigger for CharacterController-style actors that detects ClimbZone triggers and routes grab/exit events to an IClimbTraversalActor parent.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Pawn_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place this on a child trigger object near the actor's hands or grab reach.",
                        "Use a Collider on this GameObject and set it to Is Trigger.",
                        "Keep an IClimbTraversalActor implementation somewhere in the parent hierarchy.",
                        "Make sure climbable objects use ClimbZone."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not put this on the same root as the climb zone it should detect.",
                        "Do not use a non-trigger collider; OnTriggerEnter/Exit drive grab detection.",
                        "Do not author this on objects without a climb traversal actor parent."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetGrabDetectorMessages((GrabDetector)target), "GrabDetector needs a trigger Collider and climb traversal actor parent.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetGrabDetectorMessages(GrabDetector detector)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            Collider collider = detector != null ? detector.GetComponent<Collider>() : null;
            if (collider == null)
                messages.Add(PyralisGuideIssue.Required("Collider is required on the same GameObject."));
            else if (!collider.isTrigger)
                messages.Add(PyralisGuideIssue.Required("Collider should be set to Is Trigger."));

            if (detector != null && !HasClimbActorParent(detector))
                messages.Add(PyralisGuideIssue.Required("No IClimbTraversalActor implementation found in the parent hierarchy."));

            return messages;
        }

        private static bool HasClimbActorParent(GrabDetector detector)
        {
            MonoBehaviour[] behaviours = detector.GetComponentsInParent<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is NeonBlack.Gameplay.Core.Contracts.IClimbTraversalActor)
                    return true;
            }

            return false;
        }
    }

    internal static class VisualWorldHelperEditorUtility
    {
        public static bool HasAnyAssignedArrayObject(SerializedProperty property)
        {
            if (property == null || !property.isArray)
                return false;

            for (int i = 0; i < property.arraySize; i++)
            {
                if (property.GetArrayElementAtIndex(i).objectReferenceValue != null)
                    return true;
            }

            return false;
        }

        public static void AddArrayNullWarning(List<PyralisGuideIssue> messages, SerializedProperty property, string displayName)
        {
            if (property == null || !property.isArray)
                return;

            for (int i = 0; i < property.arraySize; i++)
            {
                if (property.GetArrayElementAtIndex(i).objectReferenceValue == null)
                {
                    messages.Add(PyralisGuideIssue.Optional(displayName + " contains an empty entry. Empty entries are skipped at runtime but can hide setup mistakes."));
                    return;
                }
            }
        }
    }
}
