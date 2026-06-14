using System;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Rules.Board;
using NeonBlack.Gameplay.Core.Runtime;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Feedback.UI;
using NeonBlack.Gameplay.Features.GameFlow;
using NeonBlack.Gameplay.Features.Settings;
using NeonBlack.Gameplay.Features.Tabletop;
using NeonBlack.Gameplay.Presentation.Camera;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    public sealed class PyralisAuthoringSceneEvidence
    {
        private PyralisAuthoringSceneEvidence(GameplaySessionBootstrap bootstrap)
        {
            Bootstrap = bootstrap;
        }

        public GameplaySessionBootstrap Bootstrap { get; private set; }
        public int CanvasCount { get; private set; }
        public int EventSystemCount { get; private set; }
        public int CameraCount { get; private set; }
        public int CameraBoundsProviderCount { get; private set; }
        public int ColliderCount { get; private set; }
        public int Collider2DCount { get; private set; }
        public int TilemapCount { get; private set; }
        public int ScoreServiceCount { get; private set; }
        public int HudPresenterCount { get; private set; }
        public int MenuPresenterCount { get; private set; }
        public int SelectionPresenterCount { get; private set; }
        public int PickupSurfaceCount { get; private set; }
        public int HazardSurfaceCount { get; private set; }
        public int EnemySurfaceCount { get; private set; }
        public int ZoneSurfaceCount { get; private set; }
        public int SpawnPointCount { get; private set; }
        public int LinkedSpawnPointCount => SpawnPointCount;
        public int LinkedCameraRigCount { get; private set; }

        public IGameplayStateReader GameplayStateService { get; private set; }
        public ICameraBoundsProvider CameraBoundsService { get; private set; }
        public ISessionScoreService ScoreService { get; private set; }
        public SettingsManager SettingsManager { get; private set; }
        public ProjectileLauncherBase ProjectileLauncher { get; private set; }
        public TabletopBoardGridPresenter TabletopGridPresenter { get; private set; }
        public TabletopBoardSelectionBridge TabletopSelectionBridge { get; private set; }
        public Canvas Canvas { get; private set; }
        public UIManager UiManager { get; private set; }
        public ParticipantFeedbackHudPresenter FeedbackHud { get; private set; }
        public ParticipantHealthHudBinder HealthHud { get; private set; }

        public bool HasPlayableEnvironmentSurface => ColliderCount > 0 || Collider2DCount > 0 || TilemapCount > 0 || ZoneSurfaceCount > 0;
        public bool HasEnvironmentSurface => HasPlayableEnvironmentSurface || SpawnPointCount > 0;
        public bool HasCameraSurface => CameraCount > 0 || CameraBoundsProviderCount > 0;
        public bool HasUiSurface => CanvasCount > 0 && (EventSystemCount > 0 || HudPresenterCount > 0 || MenuPresenterCount > 0 || SelectionPresenterCount > 0);
        public bool HasSelectionSurface => SelectionPresenterCount > 0 || MenuPresenterCount > 0 || (CanvasCount > 0 && EventSystemCount > 0);
        public bool HasEncounterSurface => PickupSurfaceCount > 0 || HazardSurfaceCount > 0 || EnemySurfaceCount > 0 || ZoneSurfaceCount > 0;
        public bool HasGameplayStateService => GameplayStateService != null;
        public bool HasCameraBoundsService => CameraBoundsService != null;
        public bool HasScoreService => ScoreService != null;
        public bool HasSettingsManager => SettingsManager != null;
        public bool HasProjectileLauncher => ProjectileLauncher != null;
        public bool HasTabletopGridPresenter => TabletopGridPresenter != null;
        public bool HasTabletopSelectionBridge => TabletopSelectionBridge != null;
        public bool HasCanvas => Canvas != null || CanvasCount > 0;
        public bool HasUiManager => UiManager != null;
        public bool HasFeedbackHud => FeedbackHud != null;
        public bool HasHealthHud => HealthHud != null;
        public bool HasHudSurface => HasUiManager || HasFeedbackHud || HasHealthHud;

        public static PyralisAuthoringSceneEvidence Build(GameplaySessionBootstrap bootstrap)
        {
            PyralisAuthoringSceneEvidence evidence = new PyralisAuthoringSceneEvidence(bootstrap)
            {
                CanvasCount = CountSceneComponents<Canvas>(bootstrap),
                EventSystemCount = CountSceneComponents<EventSystem>(bootstrap),
                CameraCount = CountSceneComponents<Camera>(bootstrap),
                ColliderCount = CountSceneComponents<Collider>(bootstrap),
                Collider2DCount = CountSceneComponents<Collider2D>(bootstrap),
                TilemapCount = CountSceneComponents<Tilemap>(bootstrap),
                SpawnPointCount = GetSpawnPointCount(bootstrap),
                LinkedCameraRigCount = GetLinkedCameraRigCount(bootstrap)
            };

            if (TryFindSceneComponent(bootstrap, out Canvas canvas))
                evidence.Canvas = canvas;
            if (TryFindSceneComponent(bootstrap, out SettingsManager settingsManager))
                evidence.SettingsManager = settingsManager;
            if (TryFindSceneComponent(bootstrap, out ProjectileLauncherBase projectileLauncher))
                evidence.ProjectileLauncher = projectileLauncher;
            if (TryFindSceneComponent(bootstrap, out TabletopBoardGridPresenter tabletopGridPresenter))
                evidence.TabletopGridPresenter = tabletopGridPresenter;
            if (TryFindSceneComponent(bootstrap, out TabletopBoardSelectionBridge tabletopSelectionBridge))
                evidence.TabletopSelectionBridge = tabletopSelectionBridge;
            if (TryFindSceneComponent(bootstrap, out UIManager uiManager))
                evidence.UiManager = uiManager;
            if (TryFindSceneComponent(bootstrap, out ParticipantFeedbackHudPresenter feedbackHud))
                evidence.FeedbackHud = feedbackHud;
            if (TryFindSceneComponent(bootstrap, out ParticipantHealthHudBinder healthHud))
                evidence.HealthHud = healthHud;

            MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null || !IsInBootstrapScene(bootstrap, behaviour))
                    continue;

                evidence.AddBehaviourEvidence(behaviour);
            }

            return evidence;
        }

        public bool TryGetSceneService<T>(out T service) where T : class
        {
            if (typeof(T) == typeof(IGameplayStateReader))
            {
                service = GameplayStateService as T;
                return service != null;
            }

            if (typeof(T) == typeof(ICameraBoundsProvider))
            {
                service = CameraBoundsService as T;
                return service != null;
            }

            if (typeof(T) == typeof(ISessionScoreService))
            {
                service = ScoreService as T;
                return service != null;
            }

            MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null && IsInBootstrapScene(Bootstrap, behaviour) && behaviour is T typedService)
                {
                    service = typedService;
                    return true;
                }
            }

            service = null;
            return false;
        }

        public bool TryGetSceneComponent<T>(out T component) where T : Component
        {
            return TryFindSceneComponent(Bootstrap, out component);
        }

        public string GetEnvironmentSummary()
        {
            System.Collections.Generic.List<string> parts = new System.Collections.Generic.List<string>();
            AddPart(parts, ColliderCount, "3D collider");
            AddPart(parts, Collider2DCount, "2D collider");
            AddPart(parts, TilemapCount, "tilemap");
            AddPart(parts, SpawnPointCount, "spawn point");
            AddPart(parts, ZoneSurfaceCount, "zone");
            return parts.Count > 0 ? string.Join(", ", parts) : "No colliders, tilemaps, zones, or spawn points detected";
        }

        public string GetCameraSummary()
        {
            System.Collections.Generic.List<string> parts = new System.Collections.Generic.List<string>();
            AddPart(parts, CameraCount, "camera");
            AddPart(parts, CameraBoundsProviderCount, "camera bounds provider");
            return parts.Count > 0 ? string.Join(", ", parts) : "No camera or bounds provider detected";
        }

        public string GetUiSummary()
        {
            System.Collections.Generic.List<string> parts = new System.Collections.Generic.List<string>();
            if (CanvasCount <= 0)
                parts.Add("No Canvas");
            AddPart(parts, CanvasCount, "Canvas");
            AddPart(parts, EventSystemCount, "EventSystem");
            AddPart(parts, HudPresenterCount, "HUD presenter");
            AddPart(parts, MenuPresenterCount, "menu/settings/scene-flow presenter");
            return parts.Count > 0 ? string.Join(", ", parts) : "No Canvas, EventSystem, HUD, menu, or settings presenter detected";
        }

        public string GetSelectionSummary()
        {
            System.Collections.Generic.List<string> parts = new System.Collections.Generic.List<string>();
            AddPart(parts, SelectionPresenterCount, "selection presenter");
            AddPart(parts, MenuPresenterCount, "menu surface");
            if (CanvasCount > 0 && EventSystemCount > 0)
                parts.Add("Canvas + EventSystem");
            return parts.Count > 0 ? string.Join(", ", parts) : "No board/action/menu selection surface detected";
        }

        public string GetEncounterSummary()
        {
            System.Collections.Generic.List<string> parts = new System.Collections.Generic.List<string>();
            AddPart(parts, PickupSurfaceCount, "pickup surface");
            AddPart(parts, HazardSurfaceCount, "hazard surface");
            AddPart(parts, EnemySurfaceCount, "enemy surface");
            AddPart(parts, ZoneSurfaceCount, "zone");
            return parts.Count > 0 ? string.Join(", ", parts) : "No pickup, hazard, enemy, or encounter-zone surface detected";
        }

        private void AddBehaviourEvidence(MonoBehaviour behaviour)
        {
            if (behaviour is IGameplayStateReader gameplayStateReader && GameplayStateService == null)
                GameplayStateService = gameplayStateReader;
            if (behaviour is ICameraBoundsProvider cameraBoundsProvider)
            {
                CameraBoundsProviderCount++;
                CameraBoundsService ??= cameraBoundsProvider;
            }
            if (behaviour is ISessionScoreService scoreService)
            {
                ScoreServiceCount++;
                ScoreService ??= scoreService;
            }

            if (behaviour is ParticipantFeedbackHudPresenter || behaviour is ParticipantHealthHudBinder)
                HudPresenterCount++;
            if (behaviour is UIManager || behaviour is SettingsManager || IsTypeNamed(behaviour, "SceneFader") || IsTypeNamed(behaviour, "MainMenuManager"))
                MenuPresenterCount++;
            if (behaviour is TabletopBoardGridPresenter || behaviour is TabletopBoardSelectionBridge || IsActionPresenter(behaviour))
                SelectionPresenterCount++;

            string typeName = behaviour.GetType().Name;
            if (typeName.Contains("Collectible") || typeName.Contains("Pickup"))
                PickupSurfaceCount++;
            if (typeName.Contains("Hazard") || typeName.Contains("DamageZone") || typeName.Contains("DifficultyManager"))
                HazardSurfaceCount++;
            if (typeName.Contains("Enemy"))
                EnemySurfaceCount++;
            if (typeName.Contains("Zone") || typeName == "ArenaZone")
                ZoneSurfaceCount++;
        }

        private static bool TryFindSceneComponent<T>(GameplaySessionBootstrap bootstrap, out T component) where T : Component
        {
            T[] components = Object.FindObjectsByType<T>(FindObjectsInactive.Include);
            for (int i = 0; i < components.Length; i++)
            {
                T candidate = components[i];
                if (IsInBootstrapScene(bootstrap, candidate))
                {
                    component = candidate;
                    return true;
                }
            }

            component = null;
            return false;
        }

        private static int CountSceneComponents<T>(GameplaySessionBootstrap bootstrap) where T : Component
        {
            T[] components = Object.FindObjectsByType<T>(FindObjectsInactive.Include);
            int count = 0;
            for (int i = 0; i < components.Length; i++)
            {
                if (IsInBootstrapScene(bootstrap, components[i]))
                    count++;
            }

            return count;
        }

        private static int GetSpawnPointCount(GameplaySessionBootstrap bootstrap)
        {
            if (bootstrap == null)
                return 0;

            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            SerializedProperty spawnPoints = serializedBootstrap.FindProperty("spawnPoints");
            if (spawnPoints == null || !spawnPoints.isArray)
                return 0;

            int count = 0;
            for (int i = 0; i < spawnPoints.arraySize; i++)
            {
                if (spawnPoints.GetArrayElementAtIndex(i).objectReferenceValue != null)
                    count++;
            }

            return count;
        }

        private static int GetLinkedCameraRigCount(GameplaySessionBootstrap bootstrap)
        {
            if (bootstrap == null)
                return 0;

            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            return serializedBootstrap.FindProperty("cameraRigController")?.objectReferenceValue != null ? 1 : 0;
        }

        private static bool IsInBootstrapScene(GameplaySessionBootstrap bootstrap, Component component)
        {
            return component != null
                && (bootstrap == null || component.gameObject.scene == bootstrap.gameObject.scene);
        }

        private static bool IsTypeNamed(MonoBehaviour behaviour, string typeName)
        {
            return behaviour != null
                && string.Equals(behaviour.GetType().Name, typeName, StringComparison.Ordinal);
        }

        private static bool IsActionPresenter(MonoBehaviour behaviour)
        {
            if (behaviour == null)
                return false;

            string typeName = behaviour.GetType().Name;
            return typeName.Contains("Action") && typeName.Contains("Presenter");
        }

        private static void AddPart(System.Collections.Generic.List<string> parts, int count, string singular)
        {
            if (count <= 0)
                return;

            parts.Add(count == 1 ? $"1 {singular}" : $"{count} {singular}s");
        }
    }
}
