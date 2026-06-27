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
            string field = string.IsNullOrWhiteSpace(FieldOrComponent)
                ? "the relevant field or component"
                : PyralisAuthoringLabelUtility.GetNativeActionInstructionLabel(FieldOrComponent);
            string success = string.IsNullOrWhiteSpace(SuccessCheck) ? "the setup row updates" : SuccessCheck;
            return $"{Verb} in {PyralisAuthoringLabelUtility.GetSurfaceLabel(Surface)} on {target}, use {field}, then confirm {success}.";
        }
    }

    public static class PyralisAuthoringNativeActionFactory
    {
        public static PyralisAuthoringNativeAction CreateAssetAction(
            string assetLabel,
            string createMenuPath,
            string successCheck,
            string extraInstructions = "")
        {
            string label = FirstNonEmpty(assetLabel, "asset");
            string fieldInstruction = string.IsNullOrWhiteSpace(createMenuPath)
                ? "create the asset from the Project Create menu"
                : "Create -> " + createMenuPath;
            if (!string.IsNullOrWhiteSpace(extraInstructions))
                fieldInstruction += "; " + extraInstructions.Trim();

            return new PyralisAuthoringNativeAction(
                "Create",
                PyralisAuthoringActionSurface.ProjectWindow,
                label,
                fieldInstruction,
                successCheck);
        }

        public static PyralisAuthoringNativeAction CreateSceneObjectAction(
            string objectLabel,
            string componentLabel,
            string successCheck,
            string extraInstructions = "")
        {
            string fieldInstruction = "create or select a scene object";
            if (!string.IsNullOrWhiteSpace(componentLabel))
                fieldInstruction += " and add " + componentLabel;
            if (!string.IsNullOrWhiteSpace(extraInstructions))
                fieldInstruction += "; " + extraInstructions.Trim();

            return new PyralisAuthoringNativeAction(
                "Create or select",
                PyralisAuthoringActionSurface.Hierarchy,
                FirstNonEmpty(objectLabel, "scene object"),
                fieldInstruction,
                successCheck);
        }

        public static PyralisAuthoringNativeAction AddComponentAction(
            string targetLabel,
            string componentLabel,
            string successCheck,
            string extraInstructions = "")
        {
            string fieldInstruction = string.IsNullOrWhiteSpace(componentLabel)
                ? "Add Component"
                : "Add Component -> " + componentLabel;
            if (!string.IsNullOrWhiteSpace(extraInstructions))
                fieldInstruction += "; " + extraInstructions.Trim();

            return new PyralisAuthoringNativeAction(
                "Add",
                PyralisAuthoringActionSurface.Inspector,
                FirstNonEmpty(targetLabel, "selected GameObject"),
                fieldInstruction,
                successCheck);
        }

        public static PyralisAuthoringNativeAction CreateAssignmentAction(
            string verb,
            string assignmentField,
            string assignedObjectLabel,
            string extraInstructions,
            string successCheck,
            PyralisAuthoringActionSurface surface = PyralisAuthoringActionSurface.Inspector)
        {
            string normalizedField = assignmentField ?? string.Empty;
            string ownerLabel = GetAssignmentOwner(normalizedField);
            string fieldLabel = GetAssignmentFieldName(normalizedField);
            string assignedLabel = string.IsNullOrWhiteSpace(assignedObjectLabel)
                ? "the required asset or object"
                : assignedObjectLabel;

            string fieldTarget = FirstNonEmpty(fieldLabel, normalizedField, "the reflected field");
            string fieldInstruction = $"assign or create {assignedLabel} in {fieldTarget}";

            if (!string.IsNullOrWhiteSpace(extraInstructions))
                fieldInstruction += "; " + extraInstructions.Trim();

            return new PyralisAuthoringNativeAction(
                verb,
                surface,
                FirstNonEmpty(ownerLabel, normalizedField, assignedLabel),
                fieldInstruction,
                successCheck);
        }

        public static PyralisAuthoringNativeAction ConfigureInspectorAction(
            string targetLabel,
            string fieldOrToolLabel,
            string extraInstructions,
            string successCheck)
        {
            string fieldInstruction = FirstNonEmpty(fieldOrToolLabel, "the reflected Inspector fields");
            if (!string.IsNullOrWhiteSpace(extraInstructions))
                fieldInstruction += "; " + extraInstructions.Trim();

            return new PyralisAuthoringNativeAction(
                "Configure",
                PyralisAuthoringActionSurface.Inspector,
                FirstNonEmpty(targetLabel, "selected setup object"),
                fieldInstruction,
                successCheck);
        }

        public static PyralisAuthoringNativeAction UseInspectorToolAction(
            string targetLabel,
            string toolLabel,
            string extraInstructions,
            string successCheck)
        {
            string fieldInstruction = FirstNonEmpty(toolLabel, "the Inspector tool");
            if (!string.IsNullOrWhiteSpace(extraInstructions))
                fieldInstruction += "; " + extraInstructions.Trim();

            return new PyralisAuthoringNativeAction(
                "Use",
                PyralisAuthoringActionSurface.Inspector,
                FirstNonEmpty(targetLabel, "selected setup object"),
                fieldInstruction,
                successCheck);
        }

        private static string GetAssignmentOwner(string assignmentField)
        {
            int separator = GetLastSeparator(assignmentField);
            return separator > 0 ? assignmentField.Substring(0, separator) : string.Empty;
        }

        private static string GetAssignmentFieldName(string assignmentField)
        {
            int separator = GetLastSeparator(assignmentField);
            return separator >= 0 && separator < assignmentField.Length - 1
                ? assignmentField.Substring(separator + 1)
                : assignmentField;
        }

        private static int GetLastSeparator(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return -1;

            int dot = value.LastIndexOf('.');
            int slash = value.LastIndexOf('/');
            return dot > slash ? dot : slash;
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null)
                return string.Empty;

            for (int i = 0; i < values.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(values[i]))
                    return values[i];
            }

            return string.Empty;
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
                _ => "A route-owned Unity scene surface that matches the selected intent."
            };
        }

        public static string GetSuccess(string surface)
        {
            return surface switch
            {
                EnvironmentPlayfield => "The proof has a place to happen, and the route's actors, board spaces, hazards, pickups, camera bounds, or generated chunks are not floating in undefined scene space.",
                CameraBounds => "Pressing Play frames the pawn, board, cursor, or playfield without requiring the developer to hunt for the action.",
                UiHudMenus => "Pressing Play shows the route's necessary HUD, prompts, buttons, card hand, board controls, score, or menu surface without custom debugging.",
                ScoringObjectives => "The route can prove at least one score/objective/result change and show or record it somewhere meaningful.",
                BoardActionSelection => "The developer can choose one legal action, board cell, card, menu command, target, or route-specific selection and see the system respond.",
                PickupsHazardsEnemies => "The route can demonstrate one authored encounter interaction without requiring the whole level to be final.",
                _ => "The route has enough scene support to make its active proof believable."
            };
        }
    }
}
