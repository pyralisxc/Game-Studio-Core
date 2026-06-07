using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Hazards;
using NeonBlack.Gameplay.Features.Spawning;
using NeonBlack.Gameplay.Features.Zones;
using UnityEditor;
using UnityEngine;
using static NeonBlack.Gameplay.Editor.Inspectors.SceneGameFlowEditorUtility;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(DamageZone))]
    public sealed class DamageZoneEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Damage Zone",
                new PyralisGuideSection(
                    "What This Is",
                    "DamageZone is a 3D trigger volume that repeatedly damages HealthComponent targets while they remain inside the BoxCollider.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Hazards_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place this on a 3D volume such as lava, spikes, traps, or kill zones.",
                        "Use a BoxCollider sized to the damage volume; Awake forces it to Is Trigger.",
                        "Assign Hazard Impact Profile when damage, targeting, knockback, and feedback should come from shared data.",
                        "Use fallback damage fields only for simple zones without a profile."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not use this for 2D trigger hazards; use DamageZone2D for Collider2D paths.",
                        "Do not set Tick Interval too low unless rapid repeated damage is intended.",
                        "Do not leave fallback damage at zero when no Hazard Impact Profile is assigned."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetDamageZoneMessages(serializedObject, (DamageZone)target), "DamageZone needs a BoxCollider trigger and positive tick timing.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetDamageZoneMessages(SerializedObject serializedObject, DamageZone zone)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            BoxCollider box = zone != null ? zone.GetComponent<BoxCollider>() : null;
            if (box == null)
                messages.Add(PyralisGuideIssue.Required("BoxCollider is required for 3D trigger damage."));
            else if (!box.isTrigger)
                messages.Add(PyralisGuideIssue.Optional("BoxCollider is not set to Is Trigger yet. Awake will force it on, but authoring it as a trigger makes scene intent clearer."));

            SerializedProperty impactProfile = serializedObject.FindProperty("impactProfile");
            SerializedProperty damage = serializedObject.FindProperty("damagePerTick");
            SerializedProperty tickInterval = serializedObject.FindProperty("tickInterval");
            SerializedProperty knockback = serializedObject.FindProperty("knockbackForce");

            if (impactProfile != null && impactProfile.objectReferenceValue == null && damage != null && damage.floatValue <= 0f)
                messages.Add(PyralisGuideIssue.Required("Fallback Damage Per Tick must be greater than zero when Impact Profile is empty."));

            if (tickInterval != null && tickInterval.floatValue <= 0f)
                messages.Add(PyralisGuideIssue.Required("Tick Interval must be greater than zero."));

            if (knockback != null && knockback.floatValue < 0f)
                messages.Add(PyralisGuideIssue.Required("Knockback Force cannot be negative."));

            return messages;
        }
    }

    [CustomEditor(typeof(Hazard))]
    public sealed class HazardEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Hazard",
                new PyralisGuideSection(
                    "What This Is",
                    "Hazard controls a pooled 2D hazard prefab: shadow/warning visuals, hit colliders, movement patterns, targeting, explosion behavior, and pool return.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Hazards_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Assign Hazard Data before this prefab is used by HazardSpawner.",
                        "Assign Shadow Renderer and every Collider2D that should become active during the hit phase.",
                        "Assign Lane Renderer for crossing hazards and Explosion Effect for explosive hazard data.",
                        "Assign Camera Shake Sink when Hazard Data enables screen shake.",
                        "Assign Settings Source when hazard audio should follow the player SFX volume.",
                        "Keep hit colliders disabled or harmless in prefab idle state; Hazard enables them only during active windows."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not put Shadow Renderer and Outline Renderer on the same child; outline toggles would hide the shadow too.",
                        "Do not rely on player tags; hazards affect actor targets through HealthComponent, Motor2D, HazardImpactProfile, and the configured hazard outcome sink.",
                        "Do not enable explosion data without a Rigidbody2D on the root and an explosion effect child."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetHazardMessages(serializedObject, (Hazard)target), "Hazard needs Hazard Data, Shadow Renderer, and hit colliders.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetHazardMessages(SerializedObject serializedObject, Hazard hazard)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            RequireObject(serializedObject, messages, "_data", "Hazard Data");
            RequireObject(serializedObject, messages, "_shadowRenderer", "Shadow Renderer");

            SerializedProperty hitColliders = serializedObject.FindProperty("_hitColliders");
            if (hitColliders == null || !hitColliders.isArray || hitColliders.arraySize == 0)
            {
                messages.Add(PyralisGuideIssue.Required("Hit Colliders should contain at least one Collider2D used during the hit phase."));
            }
            else
            {
                for (int i = 0; i < hitColliders.arraySize; i++)
                {
                    if (hitColliders.GetArrayElementAtIndex(i).objectReferenceValue == null)
                    {
                        messages.Add(PyralisGuideIssue.Required("Hit Colliders contains an empty entry."));
                        break;
                    }
                }
            }

            SerializedProperty shadow = serializedObject.FindProperty("_shadowRenderer");
            SerializedProperty outline = serializedObject.FindProperty("_outlineRenderer");
            if (shadow != null
                && outline != null
                && shadow.objectReferenceValue != null
                && outline.objectReferenceValue != null
                && shadow.objectReferenceValue == outline.objectReferenceValue)
            {
                messages.Add(PyralisGuideIssue.Required("Shadow Renderer and Outline Renderer should be separate SpriteRenderer components."));
            }

            if (hazard != null && hazard.GetComponent<Rigidbody2D>() == null)
                messages.Add(PyralisGuideIssue.Optional("No Rigidbody2D found on the root. Explosive hazard data requires a kinematic Rigidbody2D so child trigger events route correctly."));

            if (!HasObject(serializedObject, "_laneRenderer"))
                messages.Add(PyralisGuideIssue.Optional("Lane Renderer is empty. Crossing hazards will not show a lane warning band."));

            if (!HasObject(serializedObject, "_explosionEffect"))
                messages.Add(PyralisGuideIssue.Optional("Explosion Effect is empty. Explosive Hazard Data will log a setup warning at runtime."));

            SerializedProperty cameraShakeSink = serializedObject.FindProperty("_cameraShakeSink");
            if (cameraShakeSink != null && cameraShakeSink.objectReferenceValue != null && !(cameraShakeSink.objectReferenceValue is ICameraShakeSink))
                messages.Add(PyralisGuideIssue.Required("Camera Shake Sink must reference a component that implements ICameraShakeSink."));

            if (hazard != null
                && hazard.Data != null
                && hazard.Data.enableScreenShake
                && (cameraShakeSink == null || cameraShakeSink.objectReferenceValue == null))
            {
                messages.Add(PyralisGuideIssue.Recommended("Camera Shake Sink is empty. Assign CameraShake or another ICameraShakeSink because Hazard Data enables screen shake."));
            }

            SerializedProperty settingsSource = serializedObject.FindProperty("_settingsSource");
            if (settingsSource != null && settingsSource.objectReferenceValue != null && !(settingsSource.objectReferenceValue is IGameplaySettingsApplier))
                messages.Add(PyralisGuideIssue.Required("Settings Source must reference a component that implements IGameplaySettingsApplier."));

            if (hazard != null
                && hazard.Data != null
                && settingsSource != null
                && settingsSource.objectReferenceValue == null
                && (hazard.Data.slamImpactClip != null || hazard.Data.bounceClip != null || hazard.Data.explosionClip != null || hazard.Data.crossingEntryClip != null || hazard.Data.crossingTravelClip != null || hazard.Data.crossingExitClip != null))
            {
                messages.Add(PyralisGuideIssue.Optional("Settings Source is empty. Hazard SFX will play at authored Audio Volume without player SFX scaling."));
            }

            return messages;
        }
    }

    [CustomEditor(typeof(HazardSpawner))]
    public sealed class HazardSpawnerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Hazard Spawner",
                new PyralisGuideSection(
                    "What This Is",
                    "HazardSpawner owns 2D hazard pools, weighted hazard selection, spawn bursts, crossing paths, and camera-bounded spawn placement driven by DifficultyManager.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Hazards_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Assign at least one Hazard Entry with a prefab whose root has Hazard and Hazard Data.",
                        "Assign Difficulty Manager so spawn timing, margins, min/max hazards, and burst counts come from authored progression.",
                        "Configure Gameplay State, Camera Bounds, Hazard Outcome, and Pickup Burst services directly or let GameManager provide them at runtime.",
                        "Tune Pool Size for expected concurrency; the spawner can auto-expand but logs a warning.",
                        "Set Spawn Size Radius large enough that hazards do not spawn partly off-screen."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not leave Hazard Entries empty; no valid hazards can spawn.",
                        "Do not put Hazard on a child of the prefab root; pool creation expects it on the instantiated root.",
                        "Do not start spawning until gameplay state, hazard outcome, and camera bounds services are configured."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetHazardSpawnerMessages(serializedObject), "HazardSpawner needs at least one valid Hazard Entry.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetHazardSpawnerMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty entries = serializedObject.FindProperty("_hazardEntries");
            if (entries == null || !entries.isArray || entries.arraySize == 0)
            {
                messages.Add(PyralisGuideIssue.Required("Hazard Entries needs at least one entry."));
            }
            else
            {
                bool hasValidEntry = false;
                for (int i = 0; i < entries.arraySize; i++)
                {
                    SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                    SerializedProperty prefab = entry.FindPropertyRelative("prefab");
                    SerializedProperty weight = entry.FindPropertyRelative("weight");
                    SerializedProperty poolSize = entry.FindPropertyRelative("poolSize");
                    SerializedProperty radius = entry.FindPropertyRelative("spawnSizeRadius");
                    if (prefab != null && prefab.objectReferenceValue != null)
                        hasValidEntry = true;
                    else
                        messages.Add(PyralisGuideIssue.Required("Hazard Entry " + i + " needs a prefab."));

                    if (weight != null && weight.intValue <= 0)
                        messages.Add(PyralisGuideIssue.Required("Hazard Entry " + i + " weight must be greater than zero."));

                    if (poolSize != null && poolSize.intValue <= 0)
                        messages.Add(PyralisGuideIssue.Required("Hazard Entry " + i + " pool size must be greater than zero."));

                    if (radius != null && radius.floatValue < 0f)
                        messages.Add(PyralisGuideIssue.Required("Hazard Entry " + i + " spawn size radius cannot be negative."));
                }

                if (!hasValidEntry)
                    messages.Add(PyralisGuideIssue.Required("HazardSpawner needs at least one valid Hazard Entry."));
            }

            if (!HasObject(serializedObject, "_difficultyManager"))
                messages.Add(PyralisGuideIssue.Optional("Difficulty Manager is empty. The spawner will use fallback timing and camera margins."));

            if (!HasObject(serializedObject, "_gameplayStateSource"))
                messages.Add(PyralisGuideIssue.Optional("Gameplay State Source is empty. GameManager can provide it at runtime; otherwise assign an IGameplayStateReader component."));

            if (!HasObject(serializedObject, "_cameraBoundsSource") && !HasObject(serializedObject, "_targetCamera"))
                messages.Add(PyralisGuideIssue.Optional("Camera Bounds Source and Target Camera are empty. GameManager can provide camera bounds; otherwise assign an ICameraBoundsProvider or explicit Camera."));

            if (!HasObject(serializedObject, "_hazardOutcomeSource"))
                messages.Add(PyralisGuideIssue.Optional("Hazard Outcome Source is empty. GameManager can provide it at runtime; otherwise assign an IHazardOutcomeSink component."));

            return messages;
        }
    }

    [CustomEditor(typeof(Spawner))]
    public sealed class SpawnerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Spawner",
                new PyralisGuideSection(
                    "What This Is",
                    "Spawner is a general-purpose 3D scene spawner for prefab objects or sprite slices, with optional automatic timing, alive/total limits, hierarchy parenting, and patrol movement.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Systems/Architecture_Overview.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Assign at least one Prefab or Sprite option.",
                        "Use Auto Spawn with a positive Spawn Interval for timed spawning, or call SpawnOne from another script or UI event.",
                        "Enable Add Physics To Sprites only when generated sprite objects should fall and collide.",
                        "Set Max Alive or Max Total to zero when unlimited spawning is intended."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not leave every prefab and sprite slot empty; SpawnOne will only warn and return.",
                        "Do not enable Patrol with zero patrol distance; the spawner cannot calculate movement.",
                        "Do not enable sprite physics if the generated objects should be static visuals."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetSpawnerMessages(serializedObject), "Spawner needs at least one prefab or sprite option.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetSpawnerMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty prefabs = serializedObject.FindProperty("prefabs");
            SerializedProperty sprites = serializedObject.FindProperty("sprites");
            bool hasPrefab = HasAnyAssignedArrayObject(prefabs);
            bool hasSprite = HasAnyAssignedArrayObject(sprites);

            if (!hasPrefab && !hasSprite)
                messages.Add(PyralisGuideIssue.Required("Spawner needs at least one prefab or sprite option."));

            AddArrayNullWarning(messages, prefabs, "Prefabs");
            AddArrayNullWarning(messages, sprites, "Sprites");

            SerializedProperty spriteScale = serializedObject.FindProperty("spriteScale");
            if (spriteScale != null && spriteScale.floatValue <= 0f)
                messages.Add(PyralisGuideIssue.Required("Sprite Scale must be greater than zero."));

            SerializedProperty spawnRadius = serializedObject.FindProperty("spawnRadius");
            if (spawnRadius != null && spawnRadius.floatValue < 0f)
                messages.Add(PyralisGuideIssue.Required("Spawn Radius cannot be negative."));

            SerializedProperty spawnInterval = serializedObject.FindProperty("spawnInterval");
            SerializedProperty autoSpawn = serializedObject.FindProperty("autoSpawn");
            if (autoSpawn != null && autoSpawn.boolValue && spawnInterval != null && spawnInterval.floatValue <= 0f)
                messages.Add(PyralisGuideIssue.Required("Auto Spawn needs a Spawn Interval greater than zero."));

            SerializedProperty maxAlive = serializedObject.FindProperty("maxAlive");
            if (maxAlive != null && maxAlive.intValue < 0)
                messages.Add(PyralisGuideIssue.Required("Max Alive cannot be negative. Use zero for unlimited."));

            SerializedProperty maxTotal = serializedObject.FindProperty("maxTotal");
            if (maxTotal != null && maxTotal.intValue < 0)
                messages.Add(PyralisGuideIssue.Required("Max Total cannot be negative. Use zero for unlimited."));

            SerializedProperty patrol = serializedObject.FindProperty("patrol");
            SerializedProperty patrolDistance = serializedObject.FindProperty("patrolDistance");
            if (patrol != null && patrol.boolValue && patrolDistance != null && patrolDistance.floatValue <= 0f)
                messages.Add(PyralisGuideIssue.Required("Patrol Distance must be greater than zero when Patrol is enabled."));

            SerializedProperty patrolSpeed = serializedObject.FindProperty("patrolSpeed");
            if (patrol != null && patrol.boolValue && patrolSpeed != null && patrolSpeed.floatValue < 0f)
                messages.Add(PyralisGuideIssue.Required("Patrol Speed cannot be negative."));

            return messages;
        }

        private static bool HasAnyAssignedArrayObject(SerializedProperty property)
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

        private static void AddArrayNullWarning(List<PyralisGuideIssue> messages, SerializedProperty property, string displayName)
        {
            if (property == null || !property.isArray)
                return;

            for (int i = 0; i < property.arraySize; i++)
            {
                if (property.GetArrayElementAtIndex(i).objectReferenceValue == null)
                {
                    messages.Add(PyralisGuideIssue.Optional(displayName + " contains an empty entry. Empty entries can be selected by SpawnOne and cause an invalid spawn."));
                    return;
                }
            }
        }
    }

    [CustomEditor(typeof(SpawnTracker))]
    public sealed class SpawnTrackerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Spawn Tracker",
                new PyralisGuideSection(
                    "What This Is",
                    "SpawnTracker is runtime-added by Spawner so it can decrement alive counts when spawned objects are destroyed.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Systems/Architecture_Overview.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Usually do not add this by hand; Spawner adds it to every spawned object.",
                        "If you author it manually, make sure another script assigns OnDestroyed before the object is destroyed.",
                        "Use it only for spawned-object lifetime accounting."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not expect this to spawn objects; it only reports destruction.",
                        "Do not depend on it for gameplay scoring or cleanup unless a caller assigns OnDestroyed.",
                        "Do not add multiple SpawnTrackers to the same object."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetSpawnTrackerMessages((SpawnTracker)target), "SpawnTracker is runtime-added by Spawner.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetSpawnTrackerMessages(SpawnTracker tracker)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            if (tracker != null && tracker.GetComponents<SpawnTracker>().Length > 1)
                messages.Add(PyralisGuideIssue.Optional("Multiple SpawnTracker components are on this object. A spawned object normally needs only one."));

            messages.Add(PyralisGuideIssue.Optional("SpawnTracker is normally added by Spawner at runtime; hand-authored trackers need a script to assign OnDestroyed."));
            return messages;
        }
    }
}
