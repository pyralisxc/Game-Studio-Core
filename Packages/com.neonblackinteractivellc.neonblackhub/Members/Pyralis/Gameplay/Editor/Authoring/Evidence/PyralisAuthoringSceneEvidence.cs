using System;
using NeonBlack.Gameplay.Presentation.HUD.GameFlow;
using NeonBlack.Gameplay.Glue.Bootstrap;
using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Glue.Session;
using NeonBlack.Gameplay.Glue.Spawning;
using NeonBlack.Gameplay.Modules.Character;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Tabletop;
using NeonBlack.Gameplay.Modules.Combat;
using NeonBlack.Gameplay.Modules.Encounters;
using NeonBlack.Gameplay.Modules.Enemies;
using NeonBlack.Gameplay.Modules.Feedback.UI;
using NeonBlack.Gameplay.Glue.SceneFlow.Arcade2D;
using NeonBlack.Gameplay.Modules.Hazards;
using NeonBlack.Gameplay.Modules.Interaction;
using NeonBlack.Gameplay.Modules.Settings;
using NeonBlack.Gameplay.Modules.Spawning;
using NeonBlack.Gameplay.Modules.Tabletop;
using NeonBlack.Gameplay.Modules.Tabletop.Runtime;
using NeonBlack.Gameplay.Modules.Hazards.Zones;
using NeonBlack.Gameplay.Presentation.Camera.Zones;
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
        public int LinkedSpawnPointCount => CountLinkedSurfaces(PyralisAuthoringSceneSurfaceKind.EnvironmentPlayfield);
        public int LinkedCameraRigCount => CountLinkedSurfaces(PyralisAuthoringSceneSurfaceKind.CameraBounds);
        private readonly List<PyralisAuthoringSceneSurfaceDetectorResult> _detectorResults = new List<PyralisAuthoringSceneSurfaceDetectorResult>();
        private readonly List<PyralisAuthoringSceneSurfaceDetectorResult> _fallbackTypeNameResults = new List<PyralisAuthoringSceneSurfaceDetectorResult>();

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
        public IReadOnlyList<PyralisAuthoringSceneSurfaceDetectorResult> DetectorResults => _detectorResults;
        public IReadOnlyList<PyralisAuthoringSceneSurfaceDetectorResult> FallbackTypeNameResults => _fallbackTypeNameResults;

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
                SpawnPointCount = GetSpawnPointCount(bootstrap)
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

            evidence.AddComponentDetectorResults<Camera>(
                "scene.camera",
                PyralisAuthoringSceneSurfaceKind.CameraBounds,
                linkedToActiveSetup: IsLinkedCameraRig(bootstrap));
            evidence.AddComponentDetectorResults<Collider>(
                "scene.collider-3d",
                PyralisAuthoringSceneSurfaceKind.EnvironmentPlayfield);
            evidence.AddComponentDetectorResults<Collider2D>(
                "scene.collider-2d",
                PyralisAuthoringSceneSurfaceKind.EnvironmentPlayfield);
            evidence.AddComponentDetectorResults<Tilemap>(
                "scene.tilemap",
                PyralisAuthoringSceneSurfaceKind.EnvironmentPlayfield);
            evidence.AddSpawnPointDetectorResults(bootstrap);
            evidence.AddComponentDetectorResults<Canvas>(
                "scene.canvas",
                PyralisAuthoringSceneSurfaceKind.UiHudMenus);
            evidence.AddComponentDetectorResults<EventSystem>(
                "scene.event-system",
                PyralisAuthoringSceneSurfaceKind.UiHudMenus);

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

        public bool HasLinkedSurface(PyralisAuthoringSceneSurfaceKind kind)
        {
            return CountLinkedSurfaces(kind) > 0;
        }

        public string GetPrimaryDetectorId(PyralisAuthoringSceneSurfaceKind kind)
        {
            PyralisAuthoringSceneSurfaceDetectorResult result = GetPrimaryResult(kind);
            return result != null ? result.DetectorId : string.Empty;
        }

        public Object GetPrimaryCandidate(PyralisAuthoringSceneSurfaceKind kind)
        {
            PyralisAuthoringSceneSurfaceDetectorResult result = GetPrimaryResult(kind);
            return result != null ? result.CandidateObject : null;
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
                AddDetectorResult(
                    "scene.camera-bounds-provider",
                    PyralisAuthoringSceneSurfaceKind.CameraBounds,
                    behaviour,
                    IsLinkedCameraRig(Bootstrap, behaviour),
                    behaviour.GetType().Name);
            }
            if (behaviour is ISessionScoreService scoreService)
            {
                ScoreServiceCount++;
                ScoreService ??= scoreService;
                AddDetectorResult(
                    "scene.score-service",
                    PyralisAuthoringSceneSurfaceKind.ScoringObjectives,
                    behaviour,
                    false,
                    behaviour.GetType().Name);
            }

            if (behaviour is ParticipantFeedbackHudPresenter || behaviour is ParticipantHealthHudBinder)
            {
                HudPresenterCount++;
                AddDetectorResult(
                    "scene.hud-presenter",
                    PyralisAuthoringSceneSurfaceKind.UiHudMenus,
                    behaviour,
                    false,
                    behaviour.GetType().Name);
            }
            if (behaviour is UIManager || behaviour is SettingsManager || IsTypeNamed(behaviour, "SceneFader") || IsTypeNamed(behaviour, "MainMenuManager"))
            {
                MenuPresenterCount++;
                AddDetectorResult(
                    "scene.menu-presenter",
                    PyralisAuthoringSceneSurfaceKind.UiHudMenus,
                    behaviour,
                    false,
                    behaviour.GetType().Name);
            }
            if (behaviour is TabletopBoardGridPresenter || behaviour is TabletopBoardSelectionBridge || IsActionPresenter(behaviour))
            {
                SelectionPresenterCount++;
                AddDetectorResult(
                    "scene.selection-presenter",
                    PyralisAuthoringSceneSurfaceKind.BoardActionSelection,
                    behaviour,
                    false,
                    behaviour.GetType().Name);
            }

            if (behaviour is IPickupCollectible || behaviour is IPickupSpawnSurface || behaviour is IPickupBurstSpawnSurface)
            {
                PickupSurfaceCount++;
                AddDetectorResult(
                    "scene.pickup-surface",
                    PyralisAuthoringSceneSurfaceKind.PickupsHazardsEnemies,
                    behaviour,
                    false,
                    behaviour.GetType().Name);
            }
            if (behaviour is Hazard || behaviour is HazardSpawner || behaviour is DifficultyManager || behaviour is DamageZone || behaviour is DamageZone2D)
            {
                HazardSurfaceCount++;
                AddDetectorResult(
                    "scene.hazard-surface",
                    PyralisAuthoringSceneSurfaceKind.PickupsHazardsEnemies,
                    behaviour,
                    false,
                    behaviour.GetType().Name);
            }
            if (behaviour is EnemyAI || behaviour is EnemySpawner)
            {
                EnemySurfaceCount++;
                AddDetectorResult(
                    "scene.enemy-surface",
                    PyralisAuthoringSceneSurfaceKind.PickupsHazardsEnemies,
                    behaviour,
                    false,
                    behaviour.GetType().Name);
            }
            if (behaviour is ArenaZone || behaviour is CameraZone)
            {
                ZoneSurfaceCount++;
                AddDetectorResult(
                    "scene.zone-surface",
                    PyralisAuthoringSceneSurfaceKind.EnvironmentPlayfield,
                    behaviour,
                    false,
                    behaviour.GetType().Name);
                AddDetectorResult(
                    "scene.encounter-zone-surface",
                    PyralisAuthoringSceneSurfaceKind.PickupsHazardsEnemies,
                    behaviour,
                    false,
                    behaviour.GetType().Name);
            }

            string typeName = behaviour.GetType().Name;
            if (IsFallbackTypeNameSurface(behaviour, typeName))
                AddFallbackTypeNameResult(behaviour, typeName);
        }

        private void AddComponentDetectorResults<T>(
            string detectorId,
            PyralisAuthoringSceneSurfaceKind kind,
            bool linkedToActiveSetup = false) where T : Component
        {
            T[] components = Object.FindObjectsByType<T>(FindObjectsInactive.Include);
            for (int i = 0; i < components.Length; i++)
            {
                T component = components[i];
                if (!IsInBootstrapScene(Bootstrap, component))
                    continue;

                AddDetectorResult(detectorId, kind, component, linkedToActiveSetup, typeof(T).Name);
            }
        }

        private void AddSpawnPointDetectorResults(GameplaySessionBootstrap bootstrap)
        {
            if (bootstrap == null)
                return;

            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            ParticipantSpawnService spawnService = GetObjectReference<ParticipantSpawnService>(serializedBootstrap, "participantSpawnService");
            if (spawnService == null)
                spawnService = bootstrap.GetComponentInChildren<ParticipantSpawnService>(true);
            if (spawnService == null)
                return;

            SerializedObject serializedSpawnService = new SerializedObject(spawnService);
            SerializedProperty spawnPoints = serializedSpawnService.FindProperty("spawnPoints");
            if (spawnPoints == null || !spawnPoints.isArray)
                return;

            for (int i = 0; i < spawnPoints.arraySize; i++)
            {
                Transform spawnPoint = spawnPoints.GetArrayElementAtIndex(i).objectReferenceValue as Transform;
                if (spawnPoint == null)
                    continue;

                AddDetectorResult(
                    "scene.spawn-point",
                    PyralisAuthoringSceneSurfaceKind.EnvironmentPlayfield,
                    spawnPoint,
                    true,
                    "ParticipantSpawnService spawn point");
            }
        }

        private void AddDetectorResult(
            string detectorId,
            PyralisAuthoringSceneSurfaceKind kind,
            Object candidateObject,
            bool linkedToActiveSetup,
            string summary)
        {
            if (candidateObject == null)
                return;

            for (int i = 0; i < _detectorResults.Count; i++)
            {
                PyralisAuthoringSceneSurfaceDetectorResult result = _detectorResults[i];
                if (result.CandidateObject == candidateObject
                    && result.SurfaceKind == kind
                    && string.Equals(result.DetectorId, detectorId, StringComparison.Ordinal))
                {
                    return;
                }
            }

            _detectorResults.Add(new PyralisAuthoringSceneSurfaceDetectorResult(
                detectorId,
                kind,
                candidateObject,
                linkedToActiveSetup,
                summary));
        }

        private void AddFallbackTypeNameResult(MonoBehaviour behaviour, string typeName)
        {
            _fallbackTypeNameResults.Add(new PyralisAuthoringSceneSurfaceDetectorResult(
                "scene.fallback-type-name",
                PyralisAuthoringSceneSurfaceKind.FallbackTypeName,
                behaviour,
                false,
                $"{typeName} matched a scene-surface name heuristic.",
                "SceneSurface.FallbackTypeName",
                PyralisAuthoringNativeActionFactory.AddComponentAction(
                    typeName,
                    string.Empty,
                    "the scene surface is detected by a typed detector or contract-owned component role")));
        }

        private PyralisAuthoringSceneSurfaceDetectorResult GetPrimaryResult(PyralisAuthoringSceneSurfaceKind kind)
        {
            PyralisAuthoringSceneSurfaceDetectorResult fallback = null;
            for (int i = 0; i < _detectorResults.Count; i++)
            {
                PyralisAuthoringSceneSurfaceDetectorResult result = _detectorResults[i];
                if (result.SurfaceKind != kind)
                    continue;

                if (result.LinkedToActiveSetup)
                    return result;

                fallback ??= result;
            }

            return fallback;
        }

        private int CountLinkedSurfaces(PyralisAuthoringSceneSurfaceKind kind)
        {
            int count = 0;
            for (int i = 0; i < _detectorResults.Count; i++)
            {
                if (_detectorResults[i].SurfaceKind == kind && _detectorResults[i].LinkedToActiveSetup)
                    count++;
            }

            return count;
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
            ParticipantSpawnService spawnService = GetObjectReference<ParticipantSpawnService>(serializedBootstrap, "participantSpawnService");
            if (spawnService == null)
                spawnService = bootstrap.GetComponentInChildren<ParticipantSpawnService>(true);
            if (spawnService == null)
                return 0;

            SerializedObject serializedSpawnService = new SerializedObject(spawnService);
            SerializedProperty spawnPoints = serializedSpawnService.FindProperty("spawnPoints");
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

        private static T GetObjectReference<T>(SerializedObject serializedObject, string propertyName) where T : Object
        {
            return serializedObject.FindProperty(propertyName)?.objectReferenceValue as T;
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

        private static bool IsFallbackTypeNameSurface(MonoBehaviour behaviour, string typeName)
        {
            if (behaviour == null || string.IsNullOrWhiteSpace(typeName))
                return false;

            if (behaviour is IPickupCollectible
                || behaviour is IPickupSpawnSurface
                || behaviour is IPickupBurstSpawnSurface
                || behaviour is Hazard
                || behaviour is HazardSpawner
                || behaviour is DifficultyManager
                || behaviour is DamageZone
                || behaviour is DamageZone2D
                || behaviour is EnemyAI
                || behaviour is EnemySpawner
                || behaviour is ArenaZone
                || behaviour is CameraZone)
            {
                return false;
            }

            return typeName.Contains("Collectible", StringComparison.Ordinal)
                || typeName.Contains("Pickup", StringComparison.Ordinal)
                || typeName.Contains("Hazard", StringComparison.Ordinal)
                || typeName.Contains("DamageZone", StringComparison.Ordinal)
                || typeName.Contains("DifficultyManager", StringComparison.Ordinal)
                || typeName.Contains("Enemy", StringComparison.Ordinal)
                || typeName.Contains("Zone", StringComparison.Ordinal);
        }

        private static bool IsLinkedCameraRig(GameplaySessionBootstrap bootstrap)
        {
            if (bootstrap == null)
                return false;

            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            return serializedBootstrap.FindProperty("cameraRigController")?.objectReferenceValue != null;
        }

        private static bool IsLinkedCameraRig(GameplaySessionBootstrap bootstrap, Object candidate)
        {
            if (bootstrap == null || candidate == null)
                return false;

            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            Object linked = serializedBootstrap.FindProperty("cameraRigController")?.objectReferenceValue;
            return linked == candidate;
        }

        private static void AddPart(System.Collections.Generic.List<string> parts, int count, string singular)
        {
            if (count <= 0)
                return;

            parts.Add(count == 1 ? $"1 {singular}" : $"{count} {singular}s");
        }
    }
}
