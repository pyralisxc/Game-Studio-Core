using System.Collections.Generic;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Feedback;
using NeonBlack.Gameplay.Features.Spawning;
using UnityEditor;
using UnityEngine;
using static NeonBlack.Gameplay.Editor.Inspectors.FinalP2EditorUtility;
using static NeonBlack.Gameplay.Editor.Inspectors.SceneGameFlowEditorUtility;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(ProjectilePoolHandle))]
    public sealed class ProjectilePoolHandleEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Projectile Pool Handle",
                new PyralisGuideSection(
                    "What This Is",
                    "ProjectilePoolHandle is runtime-managed by projectile pooling. ProjectileLauncherBase adds and configures it on pooled projectile instances so lifetime returns go back through the owning launcher.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Systems/Architecture_Overview.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Do not add this by hand to scene objects or authored prefabs.",
                        "Enable prefab pooling on ProjectileLauncher2D or ProjectileLauncher3D when projectile reuse is desired.",
                        "Let the launcher configure the pool owner and source prefab at runtime.",
                        "Use ReleaseToPool only from runtime projectile cleanup paths."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not expect this component to fire or move projectiles.",
                        "Do not duplicate it on the same projectile instance.",
                        "Do not author this as an Add Component menu surface; it is hidden from the menu intentionally."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetProjectilePoolHandleMessages((ProjectilePoolHandle)target), "ProjectilePoolHandle is runtime-managed by projectile pooling.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetProjectilePoolHandleMessages(ProjectilePoolHandle handle)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            if (handle != null && handle.GetComponents<ProjectilePoolHandle>().Length > 1)
                messages.Add(PyralisGuideIssue.Optional("Multiple ProjectilePoolHandle components are on this object. A pooled projectile should have only one."));

            if (!Application.isPlaying)
                messages.Add(PyralisGuideIssue.Optional("This component is normally added and configured at runtime by ProjectileLauncherBase; remove it from authored prefabs unless a custom pooling path owns it."));

            return messages;
        }
    }

    [CustomEditor(typeof(ParticipantFeedbackService))]
    public sealed class ParticipantFeedbackServiceEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Participant Feedback Service",
                new PyralisGuideSection(
                    "What This Is",
                    "ParticipantFeedbackService broadcasts score, combo, damage, heal, status, and combat-alert feedback events for participant-aware HUDs and floating feedback.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Systems/Architecture_Overview.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place one service in the gameplay/service scene when participant feedback should be shared globally.",
                        "Wire UnityEvent listeners to HUD presenters, popup spawners, combat alert displays, or audio feedback.",
                        "Register it with the scene's service composition path when dependency injection expects IParticipantFeedbackPublisher or IParticipantFeedbackStream.",
                        "Leave listener lists empty only when another script subscribes to FeedbackPublished directly."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not place multiple feedback services in the same loaded gameplay context.",
                        "Do not wire participant feedback directly to one hard-coded player when the session has multiple seats.",
                        "Do not use this for non-participant world ambience; it expects ParticipantHandle messages."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetParticipantFeedbackServiceMessages(serializedObject), "ParticipantFeedbackService should be registered once.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetParticipantFeedbackServiceMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            if (Object.FindObjectsByType<ParticipantFeedbackService>().Length > 1)
                messages.Add(PyralisGuideIssue.Optional("Multiple ParticipantFeedbackService instances are loaded. Keep one per gameplay/service context unless scenes are intentionally isolated."));

            if (!HasAnyPersistentEventListener(serializedObject))
                messages.Add(PyralisGuideIssue.Optional("No persistent UnityEvent listeners are assigned. This is fine only when runtime code subscribes to FeedbackPublished or resolves the service directly."));

            return messages;
        }

        private static bool HasAnyPersistentEventListener(SerializedObject serializedObject)
        {
            return HasPersistentEventListener(serializedObject.FindProperty("OnParticipantScorePopup"))
                || HasPersistentEventListener(serializedObject.FindProperty("OnParticipantComboPopup"))
                || HasPersistentEventListener(serializedObject.FindProperty("OnParticipantDamageFeedback"))
                || HasPersistentEventListener(serializedObject.FindProperty("OnParticipantHealFeedback"))
                || HasPersistentEventListener(serializedObject.FindProperty("OnParticipantStatusFeedback"))
                || HasPersistentEventListener(serializedObject.FindProperty("OnParticipantCombatAlert"));
        }
    }

    [CustomEditor(typeof(EnemySpawner))]
    public sealed class EnemySpawnerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Enemy Spawner",
                new PyralisGuideSection(
                    "What This Is",
                    "EnemySpawner is a 3D encounter spawner that either keeps a continuous enemy count alive or runs authored waves, then tracks spawned enemies through HealthComponent death events.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Systems/Architecture_Overview.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Assign at least one enemy prefab with HealthComponent on its root or children.",
                        "Add spawn points when enemies should appear at authored anchors; otherwise the spawner uses its own position.",
                        "Use Continuous mode for pressure maintenance and Waves mode for arena-style encounter beats.",
                        "Keep spawn radius and timing values non-negative."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not leave null prefab slots; null entries are skipped and can hide missing content.",
                        "Do not use prefabs without HealthComponent; the spawner cannot track death or wave completion reliably.",
                        "Do not leave every spawn point slot empty after creating a Spawn Points array."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetEnemySpawnerMessages(serializedObject), "EnemySpawner needs at least one enemy prefab with HealthComponent.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetEnemySpawnerMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty prefabs = serializedObject.FindProperty("enemyPrefabs");
            bool hasValidHealthPrefab = false;
            if (prefabs == null || !prefabs.isArray || prefabs.arraySize == 0)
            {
                messages.Add(PyralisGuideIssue.Required("Enemy Prefabs needs at least one prefab."));
            }
            else
            {
                for (int i = 0; i < prefabs.arraySize; i++)
                {
                    SerializedProperty element = prefabs.GetArrayElementAtIndex(i);
                    GameObject prefab = element.objectReferenceValue as GameObject;
                    if (prefab == null)
                    {
                        messages.Add(PyralisGuideIssue.Optional("Enemy Prefabs contains an empty entry. Empty entries are skipped at runtime."));
                        continue;
                    }

                    if (prefab.GetComponentInChildren<HealthComponent>(true) != null)
                        hasValidHealthPrefab = true;
                    else
                        messages.Add(PyralisGuideIssue.Required("Enemy prefab '" + prefab.name + "' needs a HealthComponent so death tracking works."));
                }

                if (!hasValidHealthPrefab)
                    messages.Add(PyralisGuideIssue.Required("EnemySpawner needs at least one enemy prefab with HealthComponent."));
            }

            AddArrayNullWarning(messages, serializedObject.FindProperty("spawnPoints"), "Spawn Points");
            RequireNonNegative(serializedObject, messages, "spawnRadius", "Spawn Radius");
            RequireNonNegative(serializedObject, messages, "respawnDelay", "Respawn Delay");
            RequireNonNegative(serializedObject, messages, "waveCooldown", "Wave Cooldown");
            RequireNonNegative(serializedObject, messages, "totalWaves", "Total Waves");
            RequireNonNegative(serializedObject, messages, "initialDelay", "Initial Delay");
            RequireNonNegative(serializedObject, messages, "spawnStagger", "Spawn Stagger");

            SerializedProperty mode = serializedObject.FindProperty("mode");
            if (mode != null && mode.intValue == (int)EnemySpawner.SpawnerMode.Continuous)
                RequirePositive(serializedObject, messages, "maxAlive", "Max Alive");
            else if (mode != null && mode.intValue == (int)EnemySpawner.SpawnerMode.Waves)
                RequirePositive(serializedObject, messages, "enemiesPerWave", "Enemies Per Wave");

            return messages;
        }
    }

    internal static class FinalP2EditorUtility
    {
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

        public static bool HasPersistentEventListener(SerializedProperty unityEventProperty)
        {
            SerializedProperty calls = unityEventProperty?.FindPropertyRelative("m_PersistentCalls.m_Calls");
            return calls != null && calls.isArray && calls.arraySize > 0;
        }
    }
}
