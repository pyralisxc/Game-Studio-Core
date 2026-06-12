namespace NeonBlack.Gameplay.Editor
{
    public enum PyralisAuthoringActionSurface
    {
        AuthoringWindow,
        ProjectWindow,
        Hierarchy,
        Inspector,
        PlayMode
    }

    public enum PyralisAuthoringSemanticTag
    {
        Authoring,
        Project,
        Hierarchy,
        Inspector,
        Component,
        Prefab,
        Definition,
        Profile,
        Input,
        UI,
        Animation,
        Audio,
        PlayMode
    }

    public enum PyralisAuthoringEvidenceState
    {
        NotRelevant,
        Missing,
        CandidateDetected,
        Conflict,
        LinkedToActiveSetup,
        Deprecated,
        Validated,
        PlayProven
    }

    public enum PyralisAuthoringProofState
    {
        NotReady,
        ReadyToAttempt,
        NotRun,
        Passed,
        Stale
    }

    public readonly struct PyralisAuthoringNativeAction
    {
        public PyralisAuthoringNativeAction(
            string verb,
            PyralisAuthoringActionSurface surface,
            string target,
            string fieldOrComponent,
            string successCheck)
        {
            Verb = verb ?? string.Empty;
            Surface = surface;
            Target = target ?? string.Empty;
            FieldOrComponent = fieldOrComponent ?? string.Empty;
            SuccessCheck = successCheck ?? string.Empty;
        }

        public string Verb { get; }
        public PyralisAuthoringActionSurface Surface { get; }
        public string Target { get; }
        public string FieldOrComponent { get; }
        public string SuccessCheck { get; }

        public string ToGuidanceSentence()
        {
            string target = string.IsNullOrWhiteSpace(Target) ? "the target object" : Target;
            string field = string.IsNullOrWhiteSpace(FieldOrComponent) ? "the relevant field or component" : FieldOrComponent;
            string success = string.IsNullOrWhiteSpace(SuccessCheck) ? "the setup row updates" : SuccessCheck;
            return $"{Verb} in {PyralisAuthoringLabelUtility.GetSurfaceLabel(Surface)} on {target}, use {field}, then confirm {success}.";
        }
    }

    public static class PyralisAuthoringLabelUtility
    {
        public static readonly PyralisAuthoringSemanticTag[] BeginnerLegendTags =
        {
            PyralisAuthoringSemanticTag.Project,
            PyralisAuthoringSemanticTag.Hierarchy,
            PyralisAuthoringSemanticTag.Inspector,
            PyralisAuthoringSemanticTag.Component,
            PyralisAuthoringSemanticTag.Prefab,
            PyralisAuthoringSemanticTag.Definition,
            PyralisAuthoringSemanticTag.Profile,
            PyralisAuthoringSemanticTag.Input,
            PyralisAuthoringSemanticTag.UI,
            PyralisAuthoringSemanticTag.Animation,
            PyralisAuthoringSemanticTag.Audio,
            PyralisAuthoringSemanticTag.PlayMode
        };

        public static string GetSurfaceLabel(PyralisAuthoringActionSurface surface)
        {
            switch (surface)
            {
                case PyralisAuthoringActionSurface.ProjectWindow:
                    return "Project";
                case PyralisAuthoringActionSurface.Hierarchy:
                    return "Hierarchy";
                case PyralisAuthoringActionSurface.Inspector:
                    return "Inspector";
                case PyralisAuthoringActionSurface.PlayMode:
                    return "Play Mode";
                default:
                    return "Authoring";
            }
        }

        public static PyralisAuthoringSemanticTag GetSemanticTag(PyralisAuthoringActionSurface surface)
        {
            switch (surface)
            {
                case PyralisAuthoringActionSurface.ProjectWindow:
                    return PyralisAuthoringSemanticTag.Project;
                case PyralisAuthoringActionSurface.Hierarchy:
                    return PyralisAuthoringSemanticTag.Hierarchy;
                case PyralisAuthoringActionSurface.Inspector:
                    return PyralisAuthoringSemanticTag.Inspector;
                case PyralisAuthoringActionSurface.PlayMode:
                    return PyralisAuthoringSemanticTag.PlayMode;
                default:
                    return PyralisAuthoringSemanticTag.Authoring;
            }
        }

        public static string GetSemanticTagLabel(PyralisAuthoringSemanticTag tag)
        {
            switch (tag)
            {
                case PyralisAuthoringSemanticTag.Project:
                    return "Project";
                case PyralisAuthoringSemanticTag.Hierarchy:
                    return "Hierarchy";
                case PyralisAuthoringSemanticTag.Inspector:
                    return "Inspector";
                case PyralisAuthoringSemanticTag.Component:
                    return "Component";
                case PyralisAuthoringSemanticTag.Prefab:
                    return "Prefab";
                case PyralisAuthoringSemanticTag.Definition:
                    return "Definition";
                case PyralisAuthoringSemanticTag.Profile:
                    return "Profile";
                case PyralisAuthoringSemanticTag.Input:
                    return "Input";
                case PyralisAuthoringSemanticTag.UI:
                    return "UI";
                case PyralisAuthoringSemanticTag.Animation:
                    return "Animation";
                case PyralisAuthoringSemanticTag.Audio:
                    return "Audio";
                case PyralisAuthoringSemanticTag.PlayMode:
                    return "Play Mode";
                default:
                    return "Authoring";
            }
        }

        public static UnityEngine.Color GetSemanticTagColor(PyralisAuthoringSemanticTag tag)
        {
            switch (tag)
            {
                case PyralisAuthoringSemanticTag.Project:
                    return new UnityEngine.Color(0.18f, 0.48f, 0.82f);
                case PyralisAuthoringSemanticTag.Hierarchy:
                    return new UnityEngine.Color(0.22f, 0.62f, 0.44f);
                case PyralisAuthoringSemanticTag.Inspector:
                    return new UnityEngine.Color(0.83f, 0.53f, 0.18f);
                case PyralisAuthoringSemanticTag.Component:
                    return new UnityEngine.Color(0.58f, 0.46f, 0.86f);
                case PyralisAuthoringSemanticTag.Prefab:
                    return new UnityEngine.Color(0.22f, 0.68f, 0.76f);
                case PyralisAuthoringSemanticTag.Definition:
                    return new UnityEngine.Color(0.72f, 0.38f, 0.72f);
                case PyralisAuthoringSemanticTag.Profile:
                    return new UnityEngine.Color(0.52f, 0.66f, 0.25f);
                case PyralisAuthoringSemanticTag.Input:
                    return new UnityEngine.Color(0.77f, 0.45f, 0.28f);
                case PyralisAuthoringSemanticTag.UI:
                    return new UnityEngine.Color(0.38f, 0.58f, 0.9f);
                case PyralisAuthoringSemanticTag.Animation:
                    return new UnityEngine.Color(0.84f, 0.46f, 0.58f);
                case PyralisAuthoringSemanticTag.Audio:
                    return new UnityEngine.Color(0.42f, 0.7f, 0.42f);
                case PyralisAuthoringSemanticTag.PlayMode:
                    return new UnityEngine.Color(0.88f, 0.22f, 0.42f);
                default:
                    return new UnityEngine.Color(0.52f, 0.52f, 0.52f);
            }
        }

        public static string GetEvidenceLabel(PyralisAuthoringEvidenceState state)
        {
            switch (state)
            {
                case PyralisAuthoringEvidenceState.Missing:
                    return "Needs setup";
                case PyralisAuthoringEvidenceState.CandidateDetected:
                    return "Found candidate surface";
                case PyralisAuthoringEvidenceState.LinkedToActiveSetup:
                    return "Linked to active setup";
                case PyralisAuthoringEvidenceState.Validated:
                    return "Validated";
                case PyralisAuthoringEvidenceState.PlayProven:
                    return "Play-proven";
                default:
                    return "Not needed for this proof";
            }
        }

        public static string GetProofLabel(PyralisAuthoringProofState state)
        {
            switch (state)
            {
                case PyralisAuthoringProofState.ReadyToAttempt:
                    return "Ready to attempt first proof";
                case PyralisAuthoringProofState.NotRun:
                    return "Play proof not run";
                case PyralisAuthoringProofState.Passed:
                    return "Play proof passed";
                case PyralisAuthoringProofState.Stale:
                    return "Play proof stale";
                default:
                    return "Not ready for first proof";
            }
        }
    }

    public static class PyralisAuthoringSurfaceBeacon
    {
        public static void DrawBeacon(PyralisAuthoringActionSurface surface, string tooltip = null)
        {
            string label = PyralisAuthoringLabelUtility.GetSurfaceLabel(surface);
            UnityEngine.Color color = PyralisAuthoringLabelUtility.GetSemanticTagColor(PyralisAuthoringLabelUtility.GetSemanticTag(surface));
            UnityEngine.Color previousContentColor = UnityEngine.GUI.contentColor;
            UnityEngine.GUI.contentColor = color;

            if (UnityEngine.GUILayout.Button(
                new UnityEngine.GUIContent(label, string.IsNullOrWhiteSpace(tooltip) ? GetBeaconTooltip(surface) : tooltip),
                UnityEditor.EditorStyles.miniButton,
                UnityEngine.GUILayout.Width(GetBeaconWidth(label))))
            {
                FocusSurface(surface);
            }

            UnityEngine.GUI.contentColor = previousContentColor;
        }

        public static void DrawBeaconRow(params PyralisAuthoringActionSurface[] surfaces)
        {
            if (surfaces == null || surfaces.Length == 0)
                return;

            using (new UnityEditor.EditorGUILayout.HorizontalScope())
            {
                UnityEditor.EditorGUILayout.LabelField("Surface Beacons", UnityEditor.EditorStyles.miniBoldLabel, UnityEngine.GUILayout.Width(104f));
                for (int i = 0; i < surfaces.Length; i++)
                    DrawBeacon(surfaces[i]);
            }
        }

        public static void DrawNativeAction(PyralisAuthoringNativeAction action, string guidance)
        {
            using (new UnityEditor.EditorGUILayout.HorizontalScope())
            {
                DrawBeacon(action.Surface);
                UnityEditor.EditorGUILayout.LabelField(guidance, UnityEditor.EditorStyles.wordWrappedMiniLabel);
            }
        }

        public static void FocusSurface(PyralisAuthoringActionSurface surface)
        {
            switch (surface)
            {
                case PyralisAuthoringActionSurface.ProjectWindow:
                    UnityEditor.EditorApplication.ExecuteMenuItem("Window/General/Project");
                    break;
                case PyralisAuthoringActionSurface.Hierarchy:
                    UnityEditor.EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
                    break;
                case PyralisAuthoringActionSurface.Inspector:
                    UnityEditor.EditorApplication.ExecuteMenuItem("Window/General/Inspector");
                    break;
                case PyralisAuthoringActionSurface.PlayMode:
                    UnityEditor.EditorApplication.ExecuteMenuItem("Window/General/Game");
                    break;
                default:
                    PyralisAuthoringWindow.Open();
                    break;
            }
        }

        private static float GetBeaconWidth(string label)
        {
            return string.Equals(label, "Play Mode", System.StringComparison.Ordinal) ? 74f : 68f;
        }

        private static string GetBeaconTooltip(PyralisAuthoringActionSurface surface)
        {
            return "Focus the Unity " + PyralisAuthoringLabelUtility.GetSurfaceLabel(surface) + " surface for this guidance step.";
        }
    }

    public static class PyralisAuthoringSceneSurfaceGuidance
    {
        public const string EnvironmentPlayfield = "Environment / Playfield";
        public const string CameraBounds = "Camera / Bounds";
        public const string UiHudMenus = "UI / HUD / Menus";
        public const string ScoringObjectives = "Scoring / Objectives";
        public const string BoardActionSelection = "Board / Action Selection";
        public const string PickupsHazardsEnemies = "Pickups / Hazards / Enemies";

        public static bool IsRecommended(PyralisAuthoringRouteDescriptor route, string surface)
        {
            if (route == null || string.IsNullOrWhiteSpace(surface))
                return false;

            return surface switch
            {
                EnvironmentPlayfield => route.UsesWorld,
                CameraBounds => route.UsesCamera,
                UiHudMenus => route.UsesUi,
                ScoringObjectives => route.UsesScoring,
                BoardActionSelection => route.UsesActionOrTabletop,
                PickupsHazardsEnemies => route.UsesHazardsOrPickups,
                _ => false
            };
        }

        public static string GetNextFix(string surface, bool recommended)
        {
            return surface switch
            {
                EnvironmentPlayfield => recommended
                    ? "Create an Environment or Playfield Root with the world art and gameplay surfaces this selected intent is proving now. Backgrounds can be flat sprites/PNGs, tilemaps, terrain, meshes, skyboxes, UI canvas art, or custom scene objects. Pyralis only reads intentional colliders, layers, bounds, zones, anchors, board spaces, or selectable surfaces when gameplay depends on them."
                    : "Optional until the selected intent uses walkable ground, board spaces, camera bounds, spawn areas, hazards, pickups, or generated content.",
                CameraBounds => recommended
                    ? "When the selected intent includes camera or bounds behavior, create a Camera Root with CinemachineCameraRigController, create or assign a CameraRigProfile in your project folderbase, create or choose a separate Cinemachine Camera for Shared Camera Behaviour, keep or create exactly one enabled physical Unity Camera for this shared proof, usually Main Camera, verify it is tagged MainCamera with Cinemachine Brain, and assign that physical camera as Target Camera. Disable or remove accidental extra physical Camera objects only when they were created by mistake; keep intentional overlay, split-screen, minimap, or render-texture cameras. Then drag the Camera Root object from Hierarchy into GameplaySessionBootstrap > Camera Rig Controller. For 2D, set the physical Target Camera Projection to Orthographic or use orthographic CameraRigProfile values, then tune Orthographic Size and 2D Bounds Framing. For angled 3D/2.5D, shape the shot with the physical Target Camera transform and the Cinemachine Camera Inspector."
                    : "Optional until the selected intent uses camera/cursor control, camera-aware spawning, board view, or bounded framing.",
                UiHudMenus => recommended
                    ? "Create UI Root with Canvas and EventSystem, then add HUD/menu presenters such as UIManager, ParticipantHealthHudBinder, ParticipantFeedbackHudPresenter, or board/action presenters."
                    : "Optional until the route needs HUD, action buttons, turn prompts, menus, settings, card hands, board UI, or visible scoring.",
                ScoringObjectives => recommended
                    ? "Add ParticipantScoreService or another ISessionScoreService, then connect HUD labels after score changes work."
                    : "Optional unless the route tracks score, timers, resources, objectives, or win/loss.",
                BoardActionSelection => recommended
                    ? "Add one selection surface first: TabletopBoardGridPresenter, TabletopBoardSelectionBridge, UI button, cursor bridge, collider/raycast target, card hand, or action presenter."
                    : "Optional unless the route uses tabletop, turns, cards, board spaces, menus, commands, or action targeting.",
                PickupsHazardsEnemies => recommended
                    ? "Treat these as feature cards after the first route works: CollectibleSpawner2D, hazard zones/spawners, EnemyAI, EnemySpawner, ArenaZone, or encounter anchors."
                    : "Optional later unless this loop uses pickups, hazards, enemies, combat arenas, or generated encounters.",
                _ => recommended
                    ? "Add the route-owned Unity scene surface selected by the current intent."
                    : "Optional unless the current intent selects a capability that reads this scene surface."
            };
        }

        public static string GetExpected(string surface)
        {
            return surface switch
            {
                EnvironmentPlayfield => "A deliberate world, board, arena, backdrop, bounds, collider, tilemap, mesh, terrain, spawn, zone, or selectable playfield surface that belongs to this route.",
                CameraBounds => "A Cinemachine-backed Pyralis camera route: Camera Root + CinemachineCameraRigController + CameraRigProfile + Shared Cinemachine Camera + physical Target Camera.",
                UiHudMenus => "A route-owned UI surface such as Canvas plus EventSystem, HUD presenter, menu presenter, board UI, action buttons, or equivalent project-owned UI.",
                ScoringObjectives => "A score, objective, timer, resource, result, or win/loss service when the route's capability ingredients claim scoring.",
                BoardActionSelection => "A selection surface the player can actually use: board grid presenter, card hand, action/menu presenter, UI buttons, cursor bridge, or collider/raycast target.",
                PickupsHazardsEnemies => "Encounter surfaces such as pickup spawners, hazard zones, enemy spawners, arena zones, or authored encounter anchors.",
                _ => "A route-owned Unity scene surface that matches the selected setup profile."
            };
        }

        public static string GetSuccess(string surface)
        {
            return surface switch
            {
                EnvironmentPlayfield => "The first proof has a place to happen, and the route's actors, board spaces, hazards, pickups, camera bounds, or generated chunks are not floating in undefined scene space.",
                CameraBounds => "Pressing Play frames the pawn, board, cursor, or playfield without requiring the developer to hunt for the action.",
                UiHudMenus => "Pressing Play shows the route's necessary HUD, prompts, buttons, card hand, board controls, score, or menu surface without custom debugging.",
                ScoringObjectives => "The route can prove at least one score/objective/result change and show or record it somewhere meaningful.",
                BoardActionSelection => "The developer can choose one legal action, board cell, card, menu command, target, or route-specific selection and see the system respond.",
                PickupsHazardsEnemies => "The route can demonstrate one authored encounter interaction without requiring the whole level to be final.",
                _ => "The route has enough scene support to make its first playable proof believable."
            };
        }
    }
}
