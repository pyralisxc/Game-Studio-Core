using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Navigation;
using NeonBlack.Gameplay.Core.Runtime;
using NeonBlack.Gameplay.Features.Settings;
using UnityEditor;
using UnityEngine;
using static NeonBlack.Gameplay.Editor.Inspectors.SceneGameFlowEditorUtility;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(SettingsMenu))]
    public sealed class SettingsMenuEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Settings Menu",
                new PyralisGuideSection(
                    "What This Is",
                    "SettingsMenu drives the 3D main-menu settings panel: volume sliders, fullscreen state, and resolution selection.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Systems/Architecture_Overview.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place this on the settings panel that is shown from the main menu.",
                        "Assign Settings Source to SettingsManager or another IGameplaySettingsApplier.",
                        "Assign any sliders, toggle, and dropdown the panel actually exposes.",
                        "Wire the resolution dropdown On Value Changed event only if another script needs to observe it; this script adds its own listener on enable."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not leave every control empty; the panel will open but cannot change settings.",
                        "Do not place this on a gameplay pause screen unless fullscreen and resolution changes are intended there.",
                        "Do not add duplicate listener setup in another component unless it removes its listeners cleanly."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetSettingsMenuMessages(serializedObject), "SettingsMenu needs at least one settings control assigned.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetSettingsMenuMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            bool hasAnyControl =
                HasObject(serializedObject, "masterSlider")
                || HasObject(serializedObject, "musicSlider")
                || HasObject(serializedObject, "sfxSlider")
                || HasObject(serializedObject, "fullscreenToggle")
                || HasObject(serializedObject, "resolutionDropdown");

            if (!hasAnyControl)
                messages.Add(PyralisGuideIssue.Required("SettingsMenu needs at least one settings control assigned."));

            AddSettingsSourceMessages(serializedObject, messages, "settingsSource");

            if (!HasObject(serializedObject, "masterSlider"))
                messages.Add(PyralisGuideIssue.Optional("Master Slider is empty. Master volume will keep its current settings service value."));

            if (!HasObject(serializedObject, "musicSlider"))
                messages.Add(PyralisGuideIssue.Optional("Music Slider is empty. Music volume will keep its current settings service value."));

            if (!HasObject(serializedObject, "sfxSlider"))
                messages.Add(PyralisGuideIssue.Optional("SFX Slider is empty. SFX volume will keep its current settings service value."));

            if (!HasObject(serializedObject, "resolutionDropdown"))
                messages.Add(PyralisGuideIssue.Optional("Resolution Dropdown is empty. Resolution selection will be unavailable."));

            return messages;
        }

        private static void AddSettingsSourceMessages(SerializedObject serializedObject, List<PyralisGuideIssue> messages, string fieldName)
        {
            SerializedProperty settings = serializedObject.FindProperty(fieldName);
            if (settings == null)
                return;

            if (settings.objectReferenceValue == null)
            {
                messages.Add(PyralisGuideIssue.Required("Settings Source is required for settings UI to read, apply, and save values."));
                return;
            }

            if (settings.objectReferenceValue is Component settingsComponent
                && settingsComponent.GetComponent<IGameplaySettingsApplier>() == null)
                messages.Add(PyralisGuideIssue.Required("Settings Source must reference a component that implements IGameplaySettingsApplier."));
        }
    }

    [CustomEditor(typeof(SettingsScreen))]
    public sealed class SettingsScreenEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Settings Screen",
                new PyralisGuideSection(
                    "What This Is",
                    "SettingsScreen swaps between a main menu page and a settings page, forwards slider/toggle values to an explicit settings service, and can pause active gameplay while settings are open.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Systems/Architecture_Overview.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Assign Main Menu Page and Settings Page roots from the same Canvas.",
                        "Assign Settings Source to SettingsManager or another IGameplaySettingsApplier.",
                        "Assign Gameplay State Source only when this screen should pause active gameplay.",
                        "Assign the Back Button so Close can save values and return to the main page.",
                        "Assign sliders and toggles for each setting exposed by the screen.",
                        "Start the Settings Page inactive when the menu should open on the main page."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not assign child controls as page roots; page swapping should hide whole panels.",
                        "Do not use RemoveAllListeners on these controls from another script; this component removes only its own delegates.",
                        "Do not expect sliders to save unless Settings Source is assigned."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetSettingsScreenMessages(serializedObject), "SettingsScreen needs Main Menu Page and Settings Page assigned.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetSettingsScreenMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            RequireObject(serializedObject, messages, "_mainMenuPage", "Main Menu Page");
            RequireObject(serializedObject, messages, "_settingsPage", "Settings Page");
            RequireObject(serializedObject, messages, "_backButton", "Back Button");
            AddSettingsSourceMessages(serializedObject, messages, "_settingsSource");

            SerializedProperty gameplayState = serializedObject.FindProperty("_gameplayStateSource");
            if (gameplayState != null
                && gameplayState.objectReferenceValue is Component gameplayComponent
                && gameplayComponent.GetComponent<IGameplayStateReader>() == null)
                messages.Add(PyralisGuideIssue.Required("Gameplay State Source must reference a component that implements IGameplayStateReader."));

            bool hasAnySettingControl =
                HasObject(serializedObject, "_masterVolumeSlider")
                || HasObject(serializedObject, "_musicVolumeSlider")
                || HasObject(serializedObject, "_sfxVolumeSlider")
                || HasObject(serializedObject, "_joystickDeadzoneSlider")
                || HasObject(serializedObject, "_swapControlsToggle");

            if (!hasAnySettingControl)
                messages.Add(PyralisGuideIssue.Optional("No setting controls are assigned. The screen can open and close, but it will not edit settings."));

            return messages;
        }

        private static void AddSettingsSourceMessages(SerializedObject serializedObject, List<PyralisGuideIssue> messages, string fieldName)
        {
            SerializedProperty settings = serializedObject.FindProperty(fieldName);
            if (settings == null)
                return;

            if (settings.objectReferenceValue == null)
            {
                messages.Add(PyralisGuideIssue.Required("Settings Source is required for settings UI to read, apply, and save values."));
                return;
            }

            if (settings.objectReferenceValue is Component settingsComponent
                && settingsComponent.GetComponent<IGameplaySettingsApplier>() == null)
                messages.Add(PyralisGuideIssue.Required("Settings Source must reference a component that implements IGameplaySettingsApplier."));
        }
    }

    [CustomEditor(typeof(SceneFader))]
    public sealed class SceneFaderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Scene Fader",
                new PyralisGuideSection(
                    "What This Is",
                    "SceneFader is a persistent ISceneNavigator that fades to black, optionally routes through the loading screen, and restores time scale before loading.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Systems/Architecture_Overview.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place one SceneFader in the bootstrap or first navigation scene.",
                        "Use FadeToSceneViaLoader when the LoadingScreen scene should show progress.",
                        "Keep fade durations short enough that menu buttons still feel responsive."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not load multiple SceneFaders; Awake keeps one active transition service and destroys duplicates.",
                        "Do not use FadeToSceneViaLoader unless SceneNames.LoadingScreen is in Build Settings.",
                        "Do not pair this with another transition singleton unless ownership is clear."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetSceneFaderMessages(), "SceneFader is ready as an explicit ISceneNavigator.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetSceneFaderMessages()
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            if (UnityEngine.Object.FindObjectsByType<SceneFader>().Length > 1)
                messages.Add(PyralisGuideIssue.Optional("Multiple SceneFader instances are loaded. Keep one active transition service per menu flow."));

            return messages;
        }
    }

    [CustomEditor(typeof(SceneLoader))]
    public sealed class SceneLoaderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Scene Loader",
                new PyralisGuideSection(
                    "What This Is",
                    "SceneLoader is a persistent ISceneNavigator that creates its own fade canvas at runtime.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Systems/Architecture_Overview.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Assign SceneLoader only to components that need this transition style.",
                        "Keep Fade Duration non-negative; zero gives an instant cut with the generated fade canvas.",
                        "Prefer one navigation owner per menu flow: SceneLoader or SceneFader."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not leave scene-navigation components without an explicit Scene Navigator Source.",
                        "Do not place one SceneLoader in every scene; Awake destroys duplicates but setup becomes harder to reason about.",
                        "Do not use this and SceneFader for the same button unless the transition path is intentional."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetSceneLoaderMessages(serializedObject), "SceneLoader is ready as an explicit ISceneNavigator.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetSceneLoaderMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            RequireNonNegative(serializedObject, messages, "fadeDuration", "Fade Duration");

            if (UnityEngine.Object.FindObjectsByType<SceneLoader>().Length > 1)
                messages.Add(PyralisGuideIssue.Optional("Multiple SceneLoader instances are loaded. Awake keeps one singleton and destroys duplicates."));

            if (UnityEngine.Object.FindObjectsByType<SceneFader>().Length > 0)
                messages.Add(PyralisGuideIssue.Optional("A SceneFader is also loaded. Confirm which transition service owns menu navigation."));

            return messages;
        }
    }

    [CustomEditor(typeof(SceneGuard))]
    public sealed class SceneGuardEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Scene Guard",
                new PyralisGuideSection(
                    "What This Is",
                    "SceneGuard is a lightweight scene-transition cleanup helper that destroys duplicate active EventSystems and AudioListeners at Awake.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Systems/Architecture_Overview.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place this in scenes that may be loaded after a persistent UI or camera bootstrap.",
                        "Keep one active EventSystem and one active AudioListener as the expected final state.",
                        "Use it as cleanup support, not as a substitute for clean scene ownership."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not rely on SceneGuard to select a specific EventSystem beyond preferring the active scene.",
                        "Do not put critical setup only on duplicate EventSystem GameObjects; they may be destroyed.",
                        "Do not treat duplicate AudioListeners as harmless; Unity will warn and audio routing can be unclear."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetSceneGuardMessages(), "SceneGuard will enforce one active EventSystem and one active AudioListener.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetSceneGuardMessages()
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            if (UnityEngine.Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>().Length > 1)
                messages.Add(PyralisGuideIssue.Optional("More than one active EventSystem is loaded. SceneGuard will destroy duplicates on Awake."));

            if (UnityEngine.Object.FindObjectsByType<AudioListener>().Length > 1)
                messages.Add(PyralisGuideIssue.Optional("More than one active AudioListener is loaded. SceneGuard will destroy duplicate listeners on Awake."));

            return messages;
        }
    }

    [CustomEditor(typeof(SplashScreenController))]
    public sealed class SplashScreenControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Splash Screen Controller",
                new PyralisGuideSection(
                    "What This Is",
                    "SplashScreenController drives the optional intro scene, plays a video or static fallback, preloads the next scene, fades out, and then activates the next scene.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Systems/Architecture_Overview.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Assign Next Scene Name to the menu or first playable scene.",
                        "Assign Black Overlay if the splash should fade to black before activation.",
                        "For video splash, assign Video Player, Video Display, and Video Clip together.",
                        "Keep skip lock and fallback display times non-negative."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not leave Next Scene Name blank; async loading will fail.",
                        "Do not assign a video display without a video clip unless a static image is intentionally shown.",
                        "Do not set Fade Out Seconds negative; the fade routine will be skipped only for zero or less."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetSplashMessages(serializedObject), "SplashScreenController needs a Next Scene Name before it can continue.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetSplashMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            RequireString(serializedObject, messages, "_nextSceneName", "Next Scene Name");
            RequireNonNegative(serializedObject, messages, "_fallbackDisplaySeconds", "Fallback Display Seconds");
            RequireNonNegative(serializedObject, messages, "_fadeOutSeconds", "Fade Out Seconds");
            RequireNonNegative(serializedObject, messages, "_skipLockSeconds", "Skip Lock Seconds");

            bool hasVideoPlayer = HasObject(serializedObject, "_videoPlayer");
            bool hasVideoDisplay = HasObject(serializedObject, "_videoDisplay");
            bool hasVideoClip = HasObject(serializedObject, "_videoClip");
            if ((hasVideoPlayer || hasVideoDisplay || hasVideoClip) && !(hasVideoPlayer && hasVideoDisplay && hasVideoClip))
                messages.Add(PyralisGuideIssue.Optional("Video splash setup is partial. Assign Video Player, Video Display, and Video Clip together, or leave all empty for a static splash."));

            if (!HasObject(serializedObject, "_blackOverlay"))
                messages.Add(PyralisGuideIssue.Optional("Black Overlay is empty. The splash can still load, but no fade-out image will be animated."));

            return messages;
        }
    }

    [CustomEditor(typeof(LoadingScreenController))]
    public sealed class LoadingScreenControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Loading Screen Controller",
                new PyralisGuideSection(
                    "What This Is",
                    "LoadingScreenController reads SceneFader.PendingScene, shows optional progress UI, and activates the target scene once async loading reaches ready state.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Systems/Architecture_Overview.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Use this only in the loading scene referenced by SceneNames.LoadingScreen.",
                        "Route into it through SceneFader.FadeToSceneViaLoader so PendingScene is set.",
                        "Assign Progress Bar and Label when the loading scene should display progress."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not open the loading scene directly unless falling back to MainMenu is acceptable.",
                        "Do not put gameplay-only startup logic here; this scene should remain transitional.",
                        "Do not assume Progress Bar or Label are required; they are optional presentation."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetLoadingMessages(serializedObject), "LoadingScreenController should be used only in the loading scene.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetLoadingMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            if (!HasObject(serializedObject, "_progressBar"))
                messages.Add(PyralisGuideIssue.Optional("Progress Bar is empty. The loading scene will still activate the target scene without a bar."));

            if (!HasObject(serializedObject, "_label"))
                messages.Add(PyralisGuideIssue.Optional("Label is empty. The loading scene will not show progress text."));

            return messages;
        }
    }

    [CustomEditor(typeof(MainMenuManager))]
    public sealed class MainMenuManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Main Menu Manager",
                new PyralisGuideSection(
                    "What This Is",
                    "MainMenuManager controls the panel-driven main menu, opens settings, credits, and co-op panels, and sends play/load/exit buttons through an explicit scene navigation service.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Systems/Architecture_Overview.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Assign Main Panel and every main menu button that should be clickable.",
                        "Assign Settings, Credits, and Co-op panels only for pages exposed by this menu.",
                        "Assign Game Scene Name to the scene loaded by New Game, Load Game, and Host Co-op.",
                        "Assign Scene Navigator Source to SceneFader, SceneLoader, or another ISceneNavigator.",
                        "Assign each panel Back button so settings, credits, and co-op pages can return to Main Panel."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not leave New Game, Load Game, Settings, Credits, Co-op, or Exit buttons empty unless that action is intentionally disabled.",
                        "Do not wire Credits as a separate scene unless that is a deliberate presentation choice; the shell route expects a simple panel.",
                        "Do not use blank Game Scene Name; navigation services cannot load an unnamed scene.",
                        "Do not forget the Scene Navigator Source in the bootstrap/menu scene."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetMainMenuMessages(serializedObject), "MainMenuManager needs a game scene name and button references.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetMainMenuMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            RequireObject(serializedObject, messages, "mainPanel", "Main Panel");
            RequireObject(serializedObject, messages, "newGameButton", "New Game Button");
            RequireObject(serializedObject, messages, "loadGameButton", "Load Game Button");
            RequireObject(serializedObject, messages, "settingsButton", "Settings Button");
            RequireObject(serializedObject, messages, "creditsButton", "Credits Button");
            RequireObject(serializedObject, messages, "coopButton", "Co-op Button");
            RequireObject(serializedObject, messages, "exitButton", "Exit Button");
            RequireString(serializedObject, messages, "gameSceneName", "Game Scene Name");
            AddSceneNavigatorMessages(serializedObject, messages, "sceneNavigatorSource");

            if (!HasObject(serializedObject, "settingsPanel"))
                messages.Add(PyralisGuideIssue.Optional("Settings Panel is empty. The Settings button listener will still run but ShowPanel has no target panel."));

            if (!HasObject(serializedObject, "creditsPanel"))
                messages.Add(PyralisGuideIssue.Optional("Credits Panel is empty. The Credits button listener will still run but ShowPanel has no target panel."));

            if (HasObject(serializedObject, "creditsPanel") && !HasObject(serializedObject, "creditsBackButton"))
                messages.Add(PyralisGuideIssue.Optional("Credits Back Button is empty. Credits can open, but beginners need a button path back to Main Panel."));

            if (!HasObject(serializedObject, "coopPanel"))
                messages.Add(PyralisGuideIssue.Optional("Co-op Panel is empty. The Co-op button listener will still run but ShowPanel has no target panel."));

            return messages;
        }

        private static void AddSceneNavigatorMessages(SerializedObject serializedObject, List<PyralisGuideIssue> messages, string fieldName)
        {
            SerializedProperty property = serializedObject.FindProperty(fieldName);
            if (property == null)
                return;

            if (property.objectReferenceValue == null)
            {
                messages.Add(PyralisGuideIssue.Required("Scene Navigator Source is required for play/load/exit buttons."));
                return;
            }

            if (property.objectReferenceValue is Component component
                && component.GetComponent<ISceneNavigator>() == null)
                messages.Add(PyralisGuideIssue.Required("Scene Navigator Source must reference a component that implements ISceneNavigator."));
        }
    }
}
