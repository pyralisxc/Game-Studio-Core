using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Enums;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace NeonBlack.Gameplay.Editor
{
    public sealed class PyralisAuthoringSceneSurfaceRow
    {
        public PyralisAuthoringSceneSurfaceRow(
            string surface,
            bool present,
            bool recommended,
            string current,
            string nextFix,
            PyralisAuthoringEvidenceState evidenceState = PyralisAuthoringEvidenceState.NotRelevant)
        {
            Surface = surface;
            Present = present;
            Recommended = recommended;
            Current = current;
            NextFix = nextFix;
            EvidenceState = evidenceState;
        }

        public string Surface { get; }
        public bool Present { get; }
        public bool Recommended { get; }
        public string Current { get; }
        public string NextFix { get; }
        public PyralisAuthoringEvidenceState EvidenceState { get; }
        public bool SupportsFirstProofAttempt => !Recommended || Present;
    }

    public sealed class PyralisAuthoringSceneSurfaceSnapshot
    {
        private readonly List<PyralisAuthoringSceneSurfaceRow> _rows;

        private PyralisAuthoringSceneSurfaceSnapshot(List<PyralisAuthoringSceneSurfaceRow> rows)
        {
            _rows = rows ?? new List<PyralisAuthoringSceneSurfaceRow>();
        }

        public IReadOnlyList<PyralisAuthoringSceneSurfaceRow> Rows => _rows;

        public static PyralisAuthoringSceneSurfaceSnapshot Build(Object activeSetup)
        {
            GameplaySessionBootstrap bootstrap = PyralisAuthoringWindow.GetSelectedBootstrap(activeSetup);
            PyralisAuthoringRouteDescriptor route = PyralisAuthoringRouteDescriptor.Build(activeSetup);
            SceneSurfaceCounts counts = SceneSurfaceCounts.Build(bootstrap);
            List<PyralisAuthoringSceneSurfaceRow> rows = new List<PyralisAuthoringSceneSurfaceRow>();

            bool wantsWorld = PyralisAuthoringSceneSurfaceGuidance.IsRecommended(route, PyralisAuthoringSceneSurfaceGuidance.EnvironmentPlayfield);
            bool wantsCamera = PyralisAuthoringSceneSurfaceGuidance.IsRecommended(route, PyralisAuthoringSceneSurfaceGuidance.CameraBounds);
            bool wantsUi = PyralisAuthoringSceneSurfaceGuidance.IsRecommended(route, PyralisAuthoringSceneSurfaceGuidance.UiHudMenus);
            bool wantsScoring = PyralisAuthoringSceneSurfaceGuidance.IsRecommended(route, PyralisAuthoringSceneSurfaceGuidance.ScoringObjectives);
            bool wantsActionOrTabletop = PyralisAuthoringSceneSurfaceGuidance.IsRecommended(route, PyralisAuthoringSceneSurfaceGuidance.BoardActionSelection);
            bool wantsHazardsOrPickups = PyralisAuthoringSceneSurfaceGuidance.IsRecommended(route, PyralisAuthoringSceneSurfaceGuidance.PickupsHazardsEnemies);

            bool needsWalkableEnvironmentSurface = IsSideView2DMovementProof(bootstrap);
            bool environmentPresent = wantsWorld && needsWalkableEnvironmentSurface
                ? counts.HasPlayableEnvironmentSurface
                : counts.HasEnvironmentSurface;

            rows.Add(new PyralisAuthoringSceneSurfaceRow(
                PyralisAuthoringSceneSurfaceGuidance.EnvironmentPlayfield,
                environmentPresent,
                wantsWorld,
                counts.GetEnvironmentSummary(),
                PyralisAuthoringSceneSurfaceGuidance.GetNextFix(PyralisAuthoringSceneSurfaceGuidance.EnvironmentPlayfield, wantsWorld),
                GetEvidenceState(environmentPresent, wantsWorld, counts.LinkedSpawnPointCount > 0)));

            rows.Add(new PyralisAuthoringSceneSurfaceRow(
                PyralisAuthoringSceneSurfaceGuidance.CameraBounds,
                counts.HasCameraSurface,
                wantsCamera,
                counts.GetCameraSummary(),
                PyralisAuthoringSceneSurfaceGuidance.GetNextFix(PyralisAuthoringSceneSurfaceGuidance.CameraBounds, wantsCamera),
                GetEvidenceState(counts.HasCameraSurface, wantsCamera, counts.LinkedCameraRigCount > 0)));

            rows.Add(new PyralisAuthoringSceneSurfaceRow(
                PyralisAuthoringSceneSurfaceGuidance.UiHudMenus,
                counts.HasUiSurface,
                wantsUi,
                counts.GetUiSummary(),
                PyralisAuthoringSceneSurfaceGuidance.GetNextFix(PyralisAuthoringSceneSurfaceGuidance.UiHudMenus, wantsUi),
                GetEvidenceState(counts.HasUiSurface, wantsUi)));

            rows.Add(new PyralisAuthoringSceneSurfaceRow(
                PyralisAuthoringSceneSurfaceGuidance.ScoringObjectives,
                counts.ScoreServiceCount > 0,
                wantsScoring,
                counts.ScoreServiceCount > 0 ? $"{counts.ScoreServiceCount} score service object(s)" : "No score service detected",
                PyralisAuthoringSceneSurfaceGuidance.GetNextFix(PyralisAuthoringSceneSurfaceGuidance.ScoringObjectives, wantsScoring),
                GetEvidenceState(counts.ScoreServiceCount > 0, wantsScoring)));

            rows.Add(new PyralisAuthoringSceneSurfaceRow(
                PyralisAuthoringSceneSurfaceGuidance.BoardActionSelection,
                counts.HasSelectionSurface,
                wantsActionOrTabletop,
                counts.GetSelectionSummary(),
                PyralisAuthoringSceneSurfaceGuidance.GetNextFix(PyralisAuthoringSceneSurfaceGuidance.BoardActionSelection, wantsActionOrTabletop),
                GetEvidenceState(counts.HasSelectionSurface, wantsActionOrTabletop)));

            rows.Add(new PyralisAuthoringSceneSurfaceRow(
                PyralisAuthoringSceneSurfaceGuidance.PickupsHazardsEnemies,
                counts.HasEncounterSurface,
                wantsHazardsOrPickups,
                counts.GetEncounterSummary(),
                PyralisAuthoringSceneSurfaceGuidance.GetNextFix(PyralisAuthoringSceneSurfaceGuidance.PickupsHazardsEnemies, wantsHazardsOrPickups),
                GetEvidenceState(counts.HasEncounterSurface, wantsHazardsOrPickups)));

            return new PyralisAuthoringSceneSurfaceSnapshot(rows);
        }

        private static PyralisAuthoringEvidenceState GetEvidenceState(bool present, bool recommended, bool linkedToActiveSetup = false)
        {
            if (!recommended && !present)
                return PyralisAuthoringEvidenceState.NotRelevant;

            if (!present)
                return PyralisAuthoringEvidenceState.Missing;

            return linkedToActiveSetup
                ? PyralisAuthoringEvidenceState.LinkedToActiveSetup
                : PyralisAuthoringEvidenceState.CandidateDetected;
        }

        private static bool IsSideView2DMovementProof(GameplaySessionBootstrap bootstrap)
        {
            PawnMovementProfile profile = GetFirstPawnMovementProfile(bootstrap);
            return profile != null
                && profile.movementMode == MovementMode.TwoD
                && profile.use2DPhysics
                && profile.allow2DJump;
        }

        private static PawnMovementProfile GetFirstPawnMovementProfile(GameplaySessionBootstrap bootstrap)
        {
            if (bootstrap == null)
                return null;

            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            SessionDefinition session = serializedBootstrap.FindProperty("sessionDefinition")?.objectReferenceValue as SessionDefinition;
            if (session == null || session.defaultParticipants == null)
                return null;

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                PawnDefinition pawn = participant != null ? participant.defaultPawn : null;
                if (pawn != null && pawn.movementProfile != null)
                    return pawn.movementProfile;
            }

            return null;
        }

        private sealed class SceneSurfaceCounts
        {
            public int CanvasCount;
            public int EventSystemCount;
            public int CameraCount;
            public int CameraBoundsProviderCount;
            public int ColliderCount;
            public int Collider2DCount;
            public int TilemapCount;
            public int SpawnPointCount;
            public int ScoreServiceCount;
            public int HudPresenterCount;
            public int MenuPresenterCount;
            public int SelectionPresenterCount;
            public int PickupSurfaceCount;
            public int HazardSurfaceCount;
            public int EnemySurfaceCount;
            public int ZoneSurfaceCount;
            public int LinkedSpawnPointCount;
            public int LinkedCameraRigCount;

            public bool HasPlayableEnvironmentSurface => ColliderCount > 0 || Collider2DCount > 0 || TilemapCount > 0 || ZoneSurfaceCount > 0;
            public bool HasEnvironmentSurface => HasPlayableEnvironmentSurface || SpawnPointCount > 0;
            public bool HasCameraSurface => CameraCount > 0 || CameraBoundsProviderCount > 0;
            public bool HasUiSurface => CanvasCount > 0 && (EventSystemCount > 0 || HudPresenterCount > 0 || MenuPresenterCount > 0 || SelectionPresenterCount > 0);
            public bool HasSelectionSurface => SelectionPresenterCount > 0 || MenuPresenterCount > 0 || (CanvasCount > 0 && EventSystemCount > 0);
            public bool HasEncounterSurface => PickupSurfaceCount > 0 || HazardSurfaceCount > 0 || EnemySurfaceCount > 0 || ZoneSurfaceCount > 0;

            public static SceneSurfaceCounts Build(GameplaySessionBootstrap bootstrap)
            {
                SceneSurfaceCounts counts = new SceneSurfaceCounts
                {
                    CanvasCount = CountSceneComponents<Canvas>(bootstrap),
                    EventSystemCount = CountSceneComponents<EventSystem>(bootstrap),
                    CameraCount = CountSceneComponents<Camera>(bootstrap),
                    ColliderCount = CountSceneComponents<Collider>(bootstrap),
                    Collider2DCount = CountSceneComponents<Collider2D>(bootstrap),
                    TilemapCount = CountSceneComponents<Tilemap>(bootstrap)
                };

                counts.SpawnPointCount = GetSpawnPointCount(bootstrap);
                counts.LinkedSpawnPointCount = counts.SpawnPointCount;
                counts.LinkedCameraRigCount = GetLinkedCameraRigCount(bootstrap);

                MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);
                for (int i = 0; i < behaviours.Length; i++)
                {
                    MonoBehaviour behaviour = behaviours[i];
                    if (behaviour == null || !IsInBootstrapScene(bootstrap, behaviour))
                        continue;

                    if (behaviour is ICameraBoundsProvider)
                        counts.CameraBoundsProviderCount++;

                    string typeName = behaviour.GetType().Name;
                    if (typeName == "ParticipantScoreService" || HasInterfaceName(behaviour, "ISessionScoreService"))
                        counts.ScoreServiceCount++;
                    if (typeName == "ParticipantFeedbackHudPresenter" || typeName == "ParticipantHealthHudBinder")
                        counts.HudPresenterCount++;
                    if (typeName == "UIManager" || typeName == "SettingsManager" || typeName == "SceneFader" || typeName == "MainMenuManager")
                        counts.MenuPresenterCount++;
                    if (typeName == "TabletopBoardGridPresenter" || typeName == "TabletopBoardSelectionBridge" || typeName.Contains("Action") && typeName.Contains("Presenter"))
                        counts.SelectionPresenterCount++;
                    if (typeName.Contains("Collectible") || typeName.Contains("Pickup"))
                        counts.PickupSurfaceCount++;
                    if (typeName.Contains("Hazard") || typeName.Contains("DamageZone") || typeName.Contains("DifficultyManager"))
                        counts.HazardSurfaceCount++;
                    if (typeName.Contains("Enemy"))
                        counts.EnemySurfaceCount++;
                    if (typeName.Contains("Zone") || typeName == "ArenaZone")
                        counts.ZoneSurfaceCount++;
                }

                return counts;
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

            private static bool IsInBootstrapScene(GameplaySessionBootstrap bootstrap, Component component)
            {
                return component != null
                    && (bootstrap == null || component.gameObject.scene == bootstrap.gameObject.scene);
            }

            public string GetEnvironmentSummary()
            {
                List<string> parts = new List<string>();
                AddPart(parts, ColliderCount, "3D collider");
                AddPart(parts, Collider2DCount, "2D collider");
                AddPart(parts, TilemapCount, "tilemap");
                AddPart(parts, SpawnPointCount, "spawn point");
                AddPart(parts, ZoneSurfaceCount, "zone");
                return parts.Count > 0 ? string.Join(", ", parts) : "No colliders, tilemaps, zones, or spawn points detected";
            }

            public string GetCameraSummary()
            {
                List<string> parts = new List<string>();
                AddPart(parts, CameraCount, "camera");
                AddPart(parts, CameraBoundsProviderCount, "camera bounds provider");
                return parts.Count > 0 ? string.Join(", ", parts) : "No camera or bounds provider detected";
            }

            public string GetUiSummary()
            {
                List<string> parts = new List<string>();
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
                List<string> parts = new List<string>();
                AddPart(parts, SelectionPresenterCount, "selection presenter");
                AddPart(parts, MenuPresenterCount, "menu surface");
                if (CanvasCount > 0 && EventSystemCount > 0)
                    parts.Add("Canvas + EventSystem");
                return parts.Count > 0 ? string.Join(", ", parts) : "No board/action/menu selection surface detected";
            }

            public string GetEncounterSummary()
            {
                List<string> parts = new List<string>();
                AddPart(parts, PickupSurfaceCount, "pickup surface");
                AddPart(parts, HazardSurfaceCount, "hazard surface");
                AddPart(parts, EnemySurfaceCount, "enemy surface");
                AddPart(parts, ZoneSurfaceCount, "zone");
                return parts.Count > 0 ? string.Join(", ", parts) : "No pickup, hazard, enemy, or encounter-zone surface detected";
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

            private static bool HasInterfaceName(MonoBehaviour behaviour, string interfaceName)
            {
                System.Type[] interfaces = behaviour.GetType().GetInterfaces();
                for (int i = 0; i < interfaces.Length; i++)
                {
                    if (interfaces[i].Name == interfaceName)
                        return true;
                }

                return false;
            }

            private static void AddPart(List<string> parts, int count, string singular)
            {
                if (count <= 0)
                    return;

                parts.Add(count == 1 ? $"1 {singular}" : $"{count} {singular}s");
            }
        }
    }
}
