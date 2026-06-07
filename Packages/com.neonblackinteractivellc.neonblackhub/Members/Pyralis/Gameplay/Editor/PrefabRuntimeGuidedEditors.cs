using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.GameFlow;
using NeonBlack.Gameplay.Features.Input;
using NeonBlack.Gameplay.Features.Pickups;
using NeonBlack.Gameplay.Features.Scoring;
using NeonBlack.Gameplay.Features.Settings;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(HealthComponent))]
    public class HealthComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Health Component",
                new PyralisGuideSection(
                    "What This Is",
                    "HealthComponent is the shared damage, healing, death, faction, and invincibility-frame component for pawns, enemies, destructible props, hazards, turrets, board-piece actors, or projectile targets.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Health_Combat_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Set Max Health to the actor or object health pool.",
                        "Set Faction so friendly fire rules work correctly.",
                        "Wire On Damaged, On Healed, and On Death only when designers need UnityEvent hooks.",
                        "Add WorldHealthBar or feedback components only after damage changes correctly."
                    }),
                new PyralisGuideSection(
                    "Route Fit",
                    null,
                    new[]
                    {
                        "Brawler/fighter path: place this on the pawn or enemy root so hitboxes and projectiles can find it.",
                        "Projectile path: hitscan and projectile launchers search hit colliders for a parent HealthComponent.",
                        "Board/card path: use this only for actor pieces or destructible objects that should take damage.",
                        "UI-only/menu path: skip this unless something can be damaged."
                    },
                    PyralisInspectorGuide.SetupManualPath("RUNTIME_PATTERN_COOKBOOK.md")));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetHealthMessages(), "HealthComponent is ready for damage routing.");
            serializedObject.ApplyModifiedProperties();
        }

        private List<PyralisGuideIssue> GetHealthMessages()
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty maxHealth = serializedObject.FindProperty("maxHealth");

            if (maxHealth != null && maxHealth.floatValue <= 0f)
                messages.Add(PyralisGuideIssue.Required("Max Health must be greater than zero."));

            return messages;
        }
    }

    public abstract class ProjectileLauncherEditorBase : UnityEditor.Editor
    {
        private const string CameraShakeSinkWarning = "Camera Shake Sink must reference a component that implements ICameraShakeSink.";

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Projectile Launcher",
                new PyralisGuideSection(
                    "What This Is",
                    "Projectile launchers execute projectile spawn commands. They can fire hitscan shots or prefab projectiles for pawns, enemies, turrets, traps, cards, board pieces, cameras, cursors, or scripted events.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Combat_Definitions_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Create ProjectileDefinition, ProjectileImpactDefinition, and FireModeDefinition assets first.",
                        "Place the launcher on the object that owns the firing surface or on a service object that can receive action commands.",
                        "Set Hit Mask to the layers shots should be allowed to hit.",
                        "Assign Hit Pause Sink and Camera Shake Sink when projectile impacts should trigger feedback services.",
                        "Use prefab pooling when firing many prefab projectiles."
                    }),
                new PyralisGuideSection(
                    "Route Fit",
                    null,
                    new[]
                    {
                        "Pawn path: launcher usually lives on the pawn or weapon child.",
                        "Enemy/turret/trap path: launcher can live on the actor or hazard object.",
                        "Board/card path: launcher can be driven by a selected card, board space, ability, or scripted action.",
                        "Camera/cursor path: launcher can fire from a cursor ray, camera ray, or selected target point."
                    },
                    PyralisInspectorGuide.SetupManualPath("RUNTIME_PATTERN_COOKBOOK.md")),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not expect the launcher to choose when to fire by itself. A combat action, input module, AI, card, or script must call it.",
                        "Do not use a 2D launcher for 3D physics raycasts, or a 3D launcher for 2D collider gameplay.",
                        "Do not forget HealthComponent on objects that should receive damage."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetLauncherMessages(), "Projectile launcher is ready to receive fire commands.");
            serializedObject.ApplyModifiedProperties();
        }

        private List<PyralisGuideIssue> GetLauncherMessages()
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty maxPool = serializedObject.FindProperty("maxPoolSizePerPrefab");

            if (maxPool != null && maxPool.intValue < 1)
                messages.Add(PyralisGuideIssue.Required("Max Pool Size Per Prefab must be at least 1 when prefab pooling is used."));

            RequireOptionalInterface<IHitPauseSink>(messages, "hitPauseSink", "Hit Pause Sink", "IHitPauseSink");
            RequireOptionalInterface<ICameraShakeSink>(messages, "cameraShakeSink", "Camera Shake Sink", "ICameraShakeSink");

            return messages;
        }

        private void RequireOptionalInterface<T>(List<PyralisGuideIssue> messages, string propertyName, string displayName, string interfaceName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == null)
                return;

            if (!(property.objectReferenceValue is T))
            {
                string message = displayName == "Camera Shake Sink" && interfaceName == "ICameraShakeSink"
                    ? CameraShakeSinkWarning
                    : displayName + " must reference a component that implements " + interfaceName + ".";
                messages.Add(PyralisGuideIssue.Required(message));
            }
        }
    }

    [CustomEditor(typeof(ProjectileLauncher2D))]
    public class ProjectileLauncher2DEditor : ProjectileLauncherEditorBase
    {
    }

    [CustomEditor(typeof(ProjectileLauncher3D))]
    public class ProjectileLauncher3DEditor : ProjectileLauncherEditorBase
    {
    }

    [CustomEditor(typeof(ParticipantScoreService))]
    public class ParticipantScoreServiceEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Participant Score Service",
                new PyralisGuideSection(
                    "What This Is",
                    "ParticipantScoreService tracks session points, survival time, high scores, and per-participant scoring. Use it for arcade loops, board victory points, resources, timers, or round results.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Scoring_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Add one score service when the selected setup profile includes scoring or objectives.",
                        "Wire UI only after score values change correctly in Play Mode.",
                        "Use events or code calls to add points from pickups, combat, cards, board moves, timers, or objectives."
                    }),
                new PyralisGuideSection(
                    "Route Fit",
                    null,
                    new[]
                    {
                        "Arcade path: pickups, hazards, and timers can call AddPoints or ResetScore.",
                        "Board/card path: victory points, resources, or round totals can route through this service.",
                        "No-score path: skip this component until the game has score, resources, objectives, or results."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(null, "ParticipantScoreService is ready for scoring routes.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(SettingsManager))]
    public class SettingsManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Settings Manager",
                new PyralisGuideSection(
                    "What This Is",
                    "SettingsManager loads, applies, and saves player-facing settings such as audio volume, joystick deadzone, gamepad deadzone, and swapped controls.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Settings_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Assign Settings Profile for clean defaults.",
                        "Assign Mixer Override only when this scene should use a different AudioMixer than the profile.",
                        "Expose MusicVolume and SFXVolume parameters on the AudioMixer if using mixer control."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not add multiple SettingsManager instances to active scenes.",
                        "Do not expect UI sliders to work until they call the manager or SettingsScreen wiring.",
                        "Do not skip SettingsProfile unless the scene is a throwaway prototype."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetSettingsMessages(), "SettingsManager is ready for settings flow.");
            serializedObject.ApplyModifiedProperties();
        }

        private List<PyralisGuideIssue> GetSettingsMessages()
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty profile = serializedObject.FindProperty("settingsProfile");

            if (profile != null && profile.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Recommended("Settings Profile is empty. Add one for stable defaults and AudioMixer routing."));

            return messages;
        }
    }

    [CustomEditor(typeof(PlayerInputHandler))]
    public class PlayerInputHandlerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Player Input Handler",
                new PyralisGuideSection(
                    "What This Is",
                    "PlayerInputHandler reads keyboard, gamepad, and virtual joystick input for the 2D pawn stack. It is for pawn-backed 2D character control, not every game type.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Input_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Use this on a 2D pawn with Motor2D.",
                        "Assign Gameplay State Source directly, or let GameManager configure it for tracked scene players.",
                        "Assign Settings Registrar Source when joystick deadzone, gamepad deadzone, or swapped controls should be pushed from settings.",
                        "Assign Input Actions when hardware input should use the Input System asset.",
                        "InputProfile > Move Action feeds Motor2D movement. In top-down/free 2D this moves X/Y; in side-view/platformer it moves X while Jump Action handles vertical motion.",
                        "InputProfile > Jump Action is a semantic request. Installed feature modules such as TopDownHopFeatureRuntime can handle it first; otherwise it falls back to side-view 2D jump only when PawnMovementProfile > Allow 2D Jump or Pawn2DMovementComponent > Jump Enabled is on.",
                        "InputProfile > Dash Action only triggers dash when the pawn movement route allows dash. Leave it empty when the pawn has no hardware dash.",
                        "Attack, kick, interact, block, guard, and custom actions only do work when the pawn has a matching combat component, input bridge, or feature module installed.",
                        "Assign Virtual Joystick, Left Zone, Right Zone, and Canvas when touch input is enabled."
                    }),
                new PyralisGuideSection(
                    "Route Fit",
                    null,
                    new[]
                    {
                        "Pawn-backed mobile path: use virtual joystick and touch zones.",
                        "Keyboard/gamepad path: assign Input Actions and keep hardware toggles enabled.",
                        "Board/card/menu path: skip this and route UI/cursor/card input through the appropriate control surface."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetInputMessages(), "PlayerInputHandler is ready for 2D pawn input.");
            serializedObject.ApplyModifiedProperties();
        }

        private List<PyralisGuideIssue> GetInputMessages()
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();

            if (((Component)target).GetComponent("Motor2D") == null)
                messages.Add(PyralisGuideIssue.Required("This GameObject should also have Motor2D because PlayerInputHandler feeds the 2D pawn motor."));
            else
                messages.Add(PyralisGuideIssue.Optional("Input route check: Move feeds the 2D motor. Jump, Dash, and optional actions are semantic requests that installed feature modules can handle before movement/combat fallbacks."));

            SerializedProperty gameplayState = serializedObject.FindProperty("_gameplayStateSource");
            if (gameplayState != null && gameplayState.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Optional("Gameplay State Source is empty. GameManager can configure tracked players at runtime; otherwise assign an IGameplayStateReader."));
            else if (gameplayState != null
                && gameplayState.objectReferenceValue is Component gameplayComponent
                && gameplayComponent.GetComponent<IGameplayStateReader>() == null)
                messages.Add(PyralisGuideIssue.Required("Gameplay State Source must reference a component that implements IGameplayStateReader."));

            SerializedProperty settingsRegistrar = serializedObject.FindProperty("_settingsRegistrarSource");
            if (settingsRegistrar != null && settingsRegistrar.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Optional("Settings Registrar Source is empty. Input keeps serialized/default deadzones until an IInputSettingsRegistrar configures it."));
            else if (settingsRegistrar != null
                && settingsRegistrar.objectReferenceValue is Component settingsComponent
                && settingsComponent.GetComponent<IInputSettingsRegistrar>() == null)
                messages.Add(PyralisGuideIssue.Required("Settings Registrar Source must reference a component that implements IInputSettingsRegistrar."));

            return messages;
        }
    }

    [CustomEditor(typeof(VirtualJoystick))]
    public class VirtualJoystickEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Virtual Joystick",
                new PyralisGuideSection(
                    "What This Is",
                    "VirtualJoystick is a floating touch joystick for mobile 2D pawn movement. It appears inside an activation zone and reports direction to PlayerInputHandler.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Input_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Create an invisible activation panel in the Canvas.",
                        "Create a joystick container with background and knob RectTransforms.",
                        "Assign Activation Zone, Background, Knob, and Canvas.",
                        "Reference this joystick from PlayerInputHandler."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not put the joystick container inside the activation panel if that breaks your layout.",
                        "Do not leave Canvas empty on non-overlay canvases.",
                        "Do not use this for board/card/menu input unless the game actually needs analog movement."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetJoystickMessages(), "VirtualJoystick is ready for touch input.");
            serializedObject.ApplyModifiedProperties();
        }

        private List<PyralisGuideIssue> GetJoystickMessages()
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            string[] required = { "_activationZone", "_background", "_knob", "_canvas" };

            for (int i = 0; i < required.Length; i++)
            {
                SerializedProperty property = serializedObject.FindProperty(required[i]);
                if (property != null && property.objectReferenceValue == null)
                    messages.Add(PyralisGuideIssue.Required(ObjectNames.NicifyVariableName(required[i]) + " is required."));
            }

            return messages;
        }
    }

    [CustomEditor(typeof(CollectibleSpawner2D))]
    public class CollectibleSpawner2DEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Collectible Spawner 2D",
                new PyralisGuideSection(
                    "What This Is",
                    "CollectibleSpawner2D pools and places collectible prefabs for 2D score loops. It supports initial population, periodic spawning, minimum on-screen counts, and burst drops.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Pickups_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Assign a collectible prefab with Collectible2D.",
                        "Configure Gameplay State and Camera Bounds directly, or let GameManager provide them at runtime.",
                        "Set Pool Size to the expected maximum active collectibles.",
                        "Tune Initial Spawn, Periodic Spawn, Burst Spawn, and Spawn Area after the camera/playfield is stable."
                    }),
                new PyralisGuideSection(
                    "Route Fit",
                    null,
                    new[]
                    {
                        "Arcade scoring path: use this with ParticipantScoreService and pickup feedback.",
                        "Procedural path: let generated chunks or spawn budgets call the spawner after the surface exists.",
                        "Board/card path: skip this unless the game has physical collectible objects."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetPickupMessages(), "CollectibleSpawner2D is ready for 2D pickup spawning.");
            serializedObject.ApplyModifiedProperties();
        }

        private List<PyralisGuideIssue> GetPickupMessages()
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty prefab = serializedObject.FindProperty("_crumbPrefab");
            SerializedProperty poolSize = serializedObject.FindProperty("_poolSize");
            SerializedProperty gameplayState = serializedObject.FindProperty("_gameplayStateSource");
            SerializedProperty cameraBounds = serializedObject.FindProperty("_cameraBoundsSource");
            SerializedProperty targetCamera = serializedObject.FindProperty("_targetCamera");

            if (prefab != null && prefab.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Required("Collectible prefab is required before the spawner can populate the playfield."));

            if (poolSize != null && poolSize.intValue < 1)
                messages.Add(PyralisGuideIssue.Required("Pool Size must be at least 1."));

            if (gameplayState != null && gameplayState.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Optional("Gameplay State Source is empty. GameManager can provide it at runtime; otherwise assign an IGameplayStateReader."));

            if ((cameraBounds == null || cameraBounds.objectReferenceValue == null)
                && (targetCamera == null || targetCamera.objectReferenceValue == null))
                messages.Add(PyralisGuideIssue.Optional("Camera Bounds Source and Target Camera are empty. GameManager can provide camera bounds; otherwise assign an ICameraBoundsProvider or explicit Camera."));

            return messages;
        }
    }

    [CustomEditor(typeof(UIManager))]
    public class UIManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: UI Manager",
                new PyralisGuideSection(
                    "What This Is",
                    "UIManager connects the 2D arcade HUD, score labels, time labels, game-over panel, settings button, restart, and menu buttons.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/UI_HUD_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place this on a UI object under the Canvas.",
                        "Assign Gameplay Session Source to a component implementing IGameplaySessionFlow.",
                        "Assign Score Service Source to ParticipantScoreService or another ISessionScoreService.",
                        "Assign HUD panel, game-over panel, labels, and buttons used by this scene.",
                        "Wire SettingsScreen only when this HUD opens settings.",
                        "Make sure the scene has EventSystem for UI clicks."
                    }),
                new PyralisGuideSection(
                    "Route Fit",
                    null,
                    new[]
                    {
                        "Arcade path: use score/time/game-over labels.",
                        "Board/card/menu path: use the UI guide as a reference, but build specific presenters for hands, board zones, turns, and actions.",
                        "No-HUD path: skip this until the game needs visible state or menu flow."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetUIManagerMessages(), "UIManager is ready for HUD/menu wiring.");
            serializedObject.ApplyModifiedProperties();
        }

        private List<PyralisGuideIssue> GetUIManagerMessages()
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty session = serializedObject.FindProperty("_gameplaySessionSource");
            SerializedProperty score = serializedObject.FindProperty("_scoreServiceSource");

            if (session != null && session.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Required("Gameplay Session Source is required for HUD state, restart, and main-menu commands."));
            else if (session != null
                && session.objectReferenceValue is Component sessionComponent
                && sessionComponent.GetComponent<IGameplaySessionFlow>() == null)
                messages.Add(PyralisGuideIssue.Required("Gameplay Session Source must reference a component that implements IGameplaySessionFlow."));

            if (score != null && score.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Required("Score Service Source is required for score/time labels and game-over totals."));
            else if (score != null
                && score.objectReferenceValue is Component scoreComponent
                && scoreComponent.GetComponent<ISessionScoreService>() == null)
                messages.Add(PyralisGuideIssue.Required("Score Service Source must reference a component that implements ISessionScoreService."));

            return messages;
        }
    }
}
