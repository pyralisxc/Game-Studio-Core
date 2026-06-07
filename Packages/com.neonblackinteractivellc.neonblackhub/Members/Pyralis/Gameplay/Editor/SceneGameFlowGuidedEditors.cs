using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Runtime;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Features.GameFlow;
using NeonBlack.Gameplay.Features.Hazards;
using NeonBlack.Gameplay.Features.Respawn;
using UnityEditor;
using UnityEngine;
using static NeonBlack.Gameplay.Editor.Inspectors.SceneGameFlowEditorUtility;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(GameManager))]
    public sealed class GameManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Game Manager",
                new PyralisGuideSection(
                    "What This Is",
                    "GameManager orchestrates a 2D score-loop scene. Use this Inspector for concrete service references, timings, and navigation fields; use Authoring for route setup.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Systems/Architecture_Overview.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Assign Score Manager, Hazard Spawner, Pickup Spawner, and Difficulty Manager.",
                        "Assign Camera Bounds Source only for custom providers; prefer the scene CinemachineCameraRigController so 2D movement, hazards, and pickups share authored bounds.",
                        "Assign Scene Navigator Source for restart and main-menu navigation.",
                        "Assign Settings Source when restart or menu navigation should save player settings first.",
                        "Assign the primary player or player controllers when participant roster services are not driving the scene.",
                        "Keep Main Menu Scene Name aligned with the scene name in Build Settings."
                    }),
                new PyralisGuideSection(
                    "Customize Here",
                    null,
                    new[]
                    {
                        "Use one GameManager owner per score-loop scene.",
                        "Random restart mode needs a Level Registry asset."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetGameManagerMessages(serializedObject, (GameManager)target), "GameManager needs Score Manager, Hazard Spawner, Pickup Spawner, and Difficulty Manager to run the score loop.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetGameManagerMessages(SerializedObject serializedObject, GameManager manager)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            RequireObject(serializedObject, messages, "scoreManager", "Score Manager");
            RequireObject(serializedObject, messages, "hazardSpawner", "Hazard Spawner");
            RequireObject(serializedObject, messages, "pickupSpawner", "Pickup Spawner");
            RequireObject(serializedObject, messages, "difficultyManager", "Difficulty Manager");
            RequireObject(serializedObject, messages, "cameraBoundsSource", "Camera Bounds Source");
            RequireInterface<ISceneNavigator>(serializedObject, messages, "sceneNavigatorSource", "Scene Navigator Source", "ISceneNavigator");
            RequireOptionalInterface<IGameplaySettingsApplier>(serializedObject, messages, "settingsSource", "Settings Source", "IGameplaySettingsApplier");
            RequireString(serializedObject, messages, "mainMenuSceneName", "Main Menu Scene Name");
            RequireNonNegative(serializedObject, messages, "deathAnimDuration", "Death Anim Duration");

            SerializedProperty player = serializedObject.FindProperty("player");
            SerializedProperty primary = serializedObject.FindProperty("primaryPlayerController");
            SerializedProperty controllers = serializedObject.FindProperty("playerControllers");
            bool hasPlayer = player != null && player.objectReferenceValue != null;
            bool hasPrimary = primary != null && primary.objectReferenceValue != null;
            bool hasControllers = controllers != null && controllers.isArray && controllers.arraySize > 0;
            if (!hasPlayer && !hasPrimary && !hasControllers)
                messages.Add(PyralisGuideIssue.Optional("No player references are assigned. This is fine only when ParticipantRosterService supplies active pawns."));

            if (manager != null && UnityEngine.Object.FindObjectsByType<GameManager>().Length > 1)
                messages.Add(PyralisGuideIssue.Optional("Multiple GameManager instances are loaded. Use one owner per 2D score loop scene."));

            return messages;
        }
    }

    [CustomEditor(typeof(PlayerSpawner))]
    public sealed class PlayerSpawnerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Player Spawner",
                new PyralisGuideSection(
                    "What This Is",
                    "PlayerSpawner tracks a player or participant pawn, listens for death, and restores or respawns the pawn at authored spawn points. Use this Inspector for respawn fields and references.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Pawn_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Assign Current Player to reuse a scene player, or assign Player Prefab to instantiate one.",
                        "Use Participant Spawn Service and Roster Service when respawning participant-owned pawns.",
                        "Add spawn points for explicit respawn positions; otherwise this GameObject position is used.",
                        "Make sure the player prefab or current player has a HealthComponent in its hierarchy."
                    }),
                new PyralisGuideSection(
                    "Customize Here",
                    null,
                    new[]
                    {
                        "Respawn delay, shield, lives, and target seat values should stay non-negative.",
                        "Countdown Format should contain {0} when countdown text is enabled."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetPlayerSpawnerMessages(serializedObject), "PlayerSpawner needs Current Player, Player Prefab, or participant infrastructure before it can respawn players.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetPlayerSpawnerMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            bool hasPlayer = HasObject(serializedObject, "currentPlayer");
            bool hasPrefab = HasObject(serializedObject, "playerPrefab");
            bool hasParticipantRoute = HasObject(serializedObject, "participantSpawnService") || HasObject(serializedObject, "rosterService");
            if (!hasPlayer && !hasPrefab && !hasParticipantRoute)
                messages.Add(PyralisGuideIssue.Required("PlayerSpawner needs Current Player, Player Prefab, or participant infrastructure."));

            RequireNonNegative(serializedObject, messages, "respawnDelay", "Respawn Delay");
            RequireNonNegative(serializedObject, messages, "respawnShield", "Respawn Shield");
            RequireNonNegative(serializedObject, messages, "startingLives", "Starting Lives");
            RequireNonNegative(serializedObject, messages, "targetSeatIndex", "Target Seat Index");
            RequirePositive(serializedObject, messages, "countdownFontSize", "Countdown Font Size");

            SerializedProperty showCountdown = serializedObject.FindProperty("showCountdown");
            SerializedProperty countdownFormat = serializedObject.FindProperty("countdownFormat");
            if (showCountdown != null
                && showCountdown.boolValue
                && countdownFormat != null
                && !countdownFormat.stringValue.Contains("{0"))
            {
                messages.Add(PyralisGuideIssue.Required("Countdown Format should contain {0} so the remaining seconds can be shown."));
            }

            SerializedProperty spawnPoints = serializedObject.FindProperty("spawnPoints");
            if (spawnPoints != null && spawnPoints.isArray)
            {
                for (int i = 0; i < spawnPoints.arraySize; i++)
                {
                    if (spawnPoints.GetArrayElementAtIndex(i).objectReferenceValue == null)
                    {
                        messages.Add(PyralisGuideIssue.Optional("Spawn Points contains an empty entry. Empty entries are skipped at runtime."));
                        break;
                    }
                }
            }

            return messages;
        }
    }

    [CustomEditor(typeof(PlayerRegistry))]
    public sealed class PlayerRegistryEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Player Registry",
                new PyralisGuideSection(
                    "What This Is",
                    "PlayerRegistry exposes the active player transform as a lightweight IPlayerProvider path when participant infrastructure is not present.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Pawn_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Pair it with Motor2D when this provider needs a 2D motor.",
                        "Prefer assigning IPlayerProvider or ParticipantRosterService in new systems."
                    }),
                new PyralisGuideSection(
                    "Customize Here",
                    null,
                    new[]
                    {
                        "Use this as a lightweight bridge, not as a full roster.",
                        "ParticipantRosterService is the preferred path for multiple seats."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetPlayerRegistryMessages((PlayerRegistry)target), "PlayerRegistry should live on the active player root.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetPlayerRegistryMessages(PlayerRegistry registry)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            GameObject root = registry != null ? registry.gameObject : null;
            if (root != null && root.GetComponent<Motor2D>() == null)
                messages.Add(PyralisGuideIssue.Optional("No Motor2D found on this GameObject. This is fine for transform-only provider use; add Motor2D only when a 2D movement path needs it."));

            if (UnityEngine.Object.FindObjectsByType<PlayerRegistry>().Length > 1)
                messages.Add(PyralisGuideIssue.Optional("Multiple PlayerRegistry instances are loaded. Prefer ParticipantRosterService when more than one player can be active."));

            return messages;
        }
    }

    [CustomEditor(typeof(DifficultyManager))]
    public sealed class DifficultyManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Difficulty Manager",
                new PyralisGuideSection(
                    "What This Is",
                    "DifficultyManager feeds HazardSpawner with spawn intervals, hazard timing, counts, margins, and wave/step progression.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Hazards_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Use Linear or Exponential for continuous ramps, Steps for discrete pressure increases, and Wave for authored encounter cycles.",
                        "Keep min/max spawn counts and hazard counts coherent so HazardSpawner never receives impossible ranges.",
                        "Tune warning and shadow durations so hazards remain readable at high difficulty."
                    }),
                new PyralisGuideSection(
                    "Customize Here",
                    null,
                    new[]
                    {
                        "Wave mode needs at least one Wave Entry.",
                        "Spawn intervals and warning durations should stay positive so hazards remain readable."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetDifficultyMessages(serializedObject), "DifficultyManager is ready to drive hazard pacing.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetDifficultyMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty mode = serializedObject.FindProperty("_mode");
            SerializedProperty waves = serializedObject.FindProperty("_waves");
            if (mode != null
                && mode.intValue == (int)DifficultyManager.DifficultyMode.Wave
                && (waves == null || !waves.isArray || waves.arraySize == 0))
            {
                messages.Add(PyralisGuideIssue.Required("Wave mode needs at least one Wave Entry."));
            }

            RequirePositive(serializedObject, messages, "_initialSpawnInterval", "Initial Spawn Interval");
            RequirePositive(serializedObject, messages, "_minSpawnInterval", "Min Spawn Interval");
            RequirePositive(serializedObject, messages, "_initialShadowDuration", "Initial Shadow Duration");
            RequirePositive(serializedObject, messages, "_warningFlashDuration", "Warning Flash Duration");
            RequireNonNegative(serializedObject, messages, "_initialSpawnDelay", "Initial Spawn Delay");
            RequireNonNegative(serializedObject, messages, "_spawnMargin", "Spawn Margin");
            RequireNonNegative(serializedObject, messages, "_minDistanceFromPlayer", "Min Distance From Player");

            SerializedProperty minInterval = serializedObject.FindProperty("_minSpawnInterval");
            SerializedProperty initialInterval = serializedObject.FindProperty("_initialSpawnInterval");
            if (minInterval != null && initialInterval != null && minInterval.floatValue > initialInterval.floatValue)
                messages.Add(PyralisGuideIssue.Optional("Min Spawn Interval is greater than Initial Spawn Interval, so the curve starts clamped to the minimum."));

            SerializedProperty minSpawn = serializedObject.FindProperty("_initialMinSpawnCount");
            SerializedProperty maxSpawn = serializedObject.FindProperty("_initialMaxSpawnCount");
            if (minSpawn != null && maxSpawn != null && maxSpawn.intValue < minSpawn.intValue)
                messages.Add(PyralisGuideIssue.Required("Initial Max Spawn Count should be at least Initial Min Spawn Count."));

            SerializedProperty minHazards = serializedObject.FindProperty("_initialMinHazards");
            SerializedProperty maxHazards = serializedObject.FindProperty("_initialMaxHazards");
            if (minHazards != null && maxHazards != null && maxHazards.intValue > 0 && maxHazards.intValue < minHazards.intValue)
                messages.Add(PyralisGuideIssue.Required("Initial Max Hazards should be zero for unlimited or at least Initial Min Hazards."));

            return messages;
        }
    }

    [CustomEditor(typeof(TimeManager))]
    public sealed class TimeManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Time Manager",
                new PyralisGuideSection(
                    "What This Is",
                    "TimeManager owns realtime freeze-frame hit pauses for combat, projectiles, hazards, and scripted feedback.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Systems/Architecture_Overview.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Call Freeze(duration) from combat feedback paths, not from Update loops."
                    }),
                new PyralisGuideSection(
                    "Customize Here",
                    null,
                    new[]
                    {
                        "Keep one TimeManager loaded for global hit pause.",
                        "Use unscaled waits for effects that must complete during freeze-frame."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetTimeManagerMessages(), "Only one TimeManager should be loaded for global hit pause.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetTimeManagerMessages()
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            if (UnityEngine.Object.FindObjectsByType<TimeManager>().Length > 1)
                messages.Add(PyralisGuideIssue.Optional("Only one TimeManager should be loaded. Awake destroys duplicates, but scenes should not author competing singletons."));

            if (Application.isPlaying && Time.timeScale == 0f)
                messages.Add(PyralisGuideIssue.Optional("Time.timeScale is currently zero. If gameplay appears paused, check active freeze-frame callers."));

            return messages;
        }
    }

    internal static class SceneGameFlowEditorUtility
    {
        public static void RequireObject(SerializedObject serializedObject, List<PyralisGuideIssue> messages, string propertyName, string displayName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null && property.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Required(displayName + " should be assigned."));
        }

        public static bool HasObject(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            return property != null && property.objectReferenceValue != null;
        }

        public static void RequireString(SerializedObject serializedObject, List<PyralisGuideIssue> messages, string propertyName, string displayName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null && string.IsNullOrWhiteSpace(property.stringValue))
                messages.Add(PyralisGuideIssue.Required(displayName + " should not be blank."));
        }

        public static void RequirePositive(SerializedObject serializedObject, List<PyralisGuideIssue> messages, string propertyName, string displayName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null && property.floatValue <= 0f)
                messages.Add(PyralisGuideIssue.Required(displayName + " must be greater than zero."));
        }

        public static void RequireNonNegative(SerializedObject serializedObject, List<PyralisGuideIssue> messages, string propertyName, string displayName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
                return;

            if (property.propertyType == SerializedPropertyType.Integer && property.intValue < 0)
                messages.Add(PyralisGuideIssue.Required(displayName + " cannot be negative."));
            else if (property.propertyType == SerializedPropertyType.Float && property.floatValue < 0f)
                messages.Add(PyralisGuideIssue.Required(displayName + " cannot be negative."));
        }

        public static void RequireInterface<T>(SerializedObject serializedObject, List<PyralisGuideIssue> messages, string propertyName, string displayName, string interfaceName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
                return;

            if (property.objectReferenceValue == null)
            {
                messages.Add(PyralisGuideIssue.Required(displayName + " should be assigned."));
                return;
            }

            if (!(property.objectReferenceValue is T))
                messages.Add(PyralisGuideIssue.Required(displayName + " must reference a component that implements " + interfaceName + "."));
        }

        public static void RequireOptionalInterface<T>(SerializedObject serializedObject, List<PyralisGuideIssue> messages, string propertyName, string displayName, string interfaceName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == null)
                return;

            if (!(property.objectReferenceValue is T))
                messages.Add(PyralisGuideIssue.Required(displayName + " must reference a component that implements " + interfaceName + "."));
        }
    }
}
