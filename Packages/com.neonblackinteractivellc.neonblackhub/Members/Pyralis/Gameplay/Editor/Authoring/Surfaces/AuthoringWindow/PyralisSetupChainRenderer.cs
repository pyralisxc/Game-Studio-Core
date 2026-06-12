using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisSetupChainRenderer
    {
        private static readonly Dictionary<string, bool> Foldouts = new Dictionary<string, bool>();

        public static void Draw(Object selection, PyralisAuthoringRouteReport report, bool showOnlyNextAction)
        {
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Setup Chain", EditorStyles.boldLabel);

            GameplaySessionBootstrap bootstrap = PyralisAuthoringSetupContextResolver.GetSelectedBootstrap(selection);
            SessionDefinition session = PyralisAuthoringSetupContextResolver.GetSelectedSession(selection, bootstrap);
            GameModeDefinition mode = PyralisAuthoringSetupContextResolver.GetSelectedMode(selection, session);
            GameSetupProfile setupProfile = PyralisAuthoringSetupContextResolver.GetSelectedSetupProfile(selection, mode);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.HelpBox("Use this map to understand the route chain before editing Inspector fields. It diagnoses what is connected, what is missing, and which native Unity step belongs next.", MessageType.Info);

                if (!showOnlyNextAction)
                {
                    DrawServiceStep("Scene Root", bootstrap != null, bootstrap, "Startup object found.", "Select a GameplaySessionBootstrap or Gameplay Root object.", "This is the scene object that starts the Pyralis session when Play begins.");
                    DrawServiceStep("Session", session != null, session, "Session asset is connected.", "Create or assign the first asset the scene root reads.", "The session names the game mode and the players, seats, cursors, or other participants that can join.");
                    DrawServiceStep("Game Rules", mode != null, mode, "Game rules asset is connected.", "Create or assign the rules asset for this session.", "The game mode points at the setup profile and owns rule-level choices for this playable loop.");
                    DrawServiceStep("Setup Profile", setupProfile != null, setupProfile, "Setup profile is connected.", "Create or assign the profile that combines game capability ingredients.", "The setup profile combines capability ingredients before prefab or scene wiring starts.");
                    DrawRuntimePatternServiceSteps(setupProfile);
                    DrawParticipantServiceSteps(session, PyralisAuthoringRouteDescriptor.Build(setupProfile, session, mode));
                }

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Current Recommendation", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(report.NextStep, EditorStyles.wordWrappedLabel);
            }

            DrawMissingCoreActions(selection, bootstrap, session, mode, setupProfile);
        }

        private static void DrawServiceStep(string label, bool isReady, Object target, string readyText, string missingText, string detailText = null, bool isOptional = false)
        {
            DrawExpandableServiceStep(label, isReady, target, readyText, missingText, detailText, isOptional);
        }

        private static void DrawExpandableServiceStep(string label, bool isReady, Object target, string readyText, string missingText, string detailText, bool isOptional = false)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                string status = PyralisAuthoringWindowPrimitives.GetReadinessBadge(isReady, target, isOptional);
                string detail = isReady ? readyText : missingText;
                string targetName = target != null ? $" ({target.name})" : string.Empty;
                EditorGUILayout.LabelField(label, $"{status}{targetName}: {detail}", EditorStyles.wordWrappedLabel);
                using (new EditorGUI.DisabledScope(target == null))
                {
                    if (GUILayout.Button("Inspect Asset", GUILayout.Width(96f)))
                    {
                        Selection.activeObject = target;
                        EditorGUIUtility.PingObject(target);
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(detailText))
                return;

            string key = "Pyralis.AuthoringWindow.ServiceStep." + label;
            bool isOpen = Foldouts.TryGetValue(key, out bool value) && value;
            isOpen = EditorGUILayout.Foldout(isOpen, "Details", true);
            Foldouts[key] = isOpen;

            if (isOpen)
            {
                EditorGUI.indentLevel++;
                PyralisAuthoringWindowText.DrawSemanticMiniLabel(detailText);
                EditorGUI.indentLevel--;
            }
        }

        private static void DrawRuntimePatternServiceSteps(GameSetupProfile setupProfile)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Capability Ingredients", EditorStyles.miniBoldLabel);

            if (setupProfile == null || setupProfile.runtimePatterns == null || setupProfile.runtimePatterns.Length == 0)
            {
                DrawServiceStep("Capability Ingredients", PyralisRuntimeCapabilityCatalogRenderer.HasAnyRuntimeCapability(setupProfile), setupProfile, string.Empty, "Choose capability ingredients before scene wiring.", "Capabilities describe the kind of game being built: pawn action, tabletop, camera/cursor, scoring, combat, traversal, and similar capability families. Optional contracts can be added later.");
                return;
            }

            for (int i = 0; i < setupProfile.runtimePatterns.Length; i++)
            {
                RuntimePatternDefinition pattern = setupProfile.runtimePatterns[i];
                string readyText = pattern != null
                    ? $"{GetRuntimePatternLabel(pattern)} adds optional route-contract metadata."
                    : string.Empty;
                DrawServiceStep($"Contract {i}", pattern != null, pattern, readyText, "Empty optional contract slot; remove it or assign a RuntimePatternDefinition if this route needs reusable metadata.", "Use capability ingredients to declare game intent before creating scene objects or prefabs.");
            }
        }

        private static void DrawParticipantServiceSteps(SessionDefinition session, PyralisAuthoringRouteDescriptor route)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Participant Prep", EditorStyles.miniBoldLabel);

            if (session == null || session.defaultParticipants == null || session.defaultParticipants.Length == 0)
            {
                DrawServiceStep("Player / Seat", false, session, string.Empty, "Create or assign participants after the route exists.", "A participant can be a player, AI, board seat, hand, faction, cursor, camera owner, or turn owner depending on the setup profile.");
                return;
            }

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                DrawServiceStep($"Player / Seat {i}", participant != null, participant, "Participant asset is available for Inspector setup.", "Empty participant slot.", "This is who or what participates in the game session.");
                if (participant != null)
                {
                    bool pawnOptional = route == null || !route.RequiresPawn;
                    string missingText = pawnOptional
                        ? "Leave empty for no-pawn routes; create one only if this participant owns an actor body."
                        : "Create or assign a PawnDefinition for this pawn-backed route.";
                    DrawServiceStep("Pawn Actor", participant.defaultPawn != null, participant.defaultPawn, "Pawn definition is available for Inspector setup.", missingText, "A pawn actor is the spawned or placed body controlled by a participant.", pawnOptional);
                }
            }
        }

        private static void DrawMissingCoreActions(
            Object selection,
            GameplaySessionBootstrap bootstrap,
            SessionDefinition session,
            GameModeDefinition mode,
            GameSetupProfile setupProfile)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Native Next Step", EditorStyles.miniBoldLabel);

            bool drewStep = false;

            if (bootstrap != null && session == null)
            {
                drewStep = true;
                DrawNativeWorkflowStep(
                    "Create a Session Definition",
                    "Project window: choose or create a setup folder for this proof, keep imported art folders separate, then right-click inside that content pane -> Create -> NeonBlack -> Definitions -> Session Definition. Then drag it into GameplaySessionBootstrap > Session Definition, or use the field's object picker circle and double-click the asset.");
            }

            if (session != null && mode == null)
            {
                drewStep = true;
                DrawNativeWorkflowStep(
                    "Create or choose a Game Mode Definition",
                    "Project window: open the setup folder so its contents are visible, then right-click inside that content pane -> Create -> NeonBlack -> Definitions -> Game Mode Definition. Then select/open the SessionDefinition asset and assign Default Game Mode by drag/drop or the field's object picker circle.");
            }

            if (mode != null && setupProfile == null)
            {
                drewStep = true;
                DrawNativeWorkflowStep(
                    "Create or choose a Game Setup Profile",
                    "Project window: open the setup folder so its contents are visible, then right-click inside that content pane -> Create -> NeonBlack -> Profiles -> Game Setup Profile. Then select/open the GameModeDefinition asset and assign Setup Profile by drag/drop or the field's object picker circle.");
            }

            if (session != null && (session.defaultParticipants == null || session.defaultParticipants.Length == 0))
            {
                drewStep = true;
                DrawNativeWorkflowStep(
                    "Create a Participant Definition",
                    "Project window: open the setup folder so its contents are visible, then right-click inside that content pane -> Create -> NeonBlack -> Definitions -> Participant Definition. Configure player/input/seat fields, then add it to SessionDefinition > Default Participants.");
            }

            PyralisAuthoringRouteDescriptor route = PyralisAuthoringRouteDescriptor.Build(setupProfile, session, mode);
            if (selection is ParticipantDefinition participant && participant.defaultPawn == null)
            {
                if (route.RequiresPawn)
                {
                    drewStep = true;
                    DrawNativeWorkflowStep(
                        "Create a Pawn Definition",
                        "Project window: open the setup folder so its contents are visible, then right-click inside that content pane -> Create -> NeonBlack -> Definitions -> Pawn Definition. Assign its pawn prefab, then assign the Pawn Definition into ParticipantDefinition > Default Pawn by drag/drop or the field's object picker circle.");
                }
            }

            if (!drewStep)
                EditorGUILayout.HelpBox("No obvious missing setup link for this selection. Use Inspect Asset on a service step when you need field-level editing.", MessageType.Info);
        }

        private static void DrawNativeWorkflowStep(string title, string instruction)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(title, EditorStyles.miniBoldLabel);
                PyralisAuthoringActionSurface surface = GetWorkflowStepSurface(instruction);
                PyralisAuthoringSurfaceBeacon.DrawNativeAction(
                    new PyralisAuthoringNativeAction("Focus", surface, title, instruction, "the visible Unity surface matches the step"),
                    instruction);
            }
        }

        private static PyralisAuthoringActionSurface GetWorkflowStepSurface(string instruction)
        {
            if (string.IsNullOrWhiteSpace(instruction))
                return PyralisAuthoringActionSurface.AuthoringWindow;

            if (instruction.IndexOf("Project window", System.StringComparison.OrdinalIgnoreCase) >= 0
                || instruction.IndexOf("Create ->", System.StringComparison.OrdinalIgnoreCase) >= 0
                || instruction.IndexOf("Create -> NeonBlack", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return PyralisAuthoringActionSurface.ProjectWindow;

            if (instruction.IndexOf("Hierarchy", System.StringComparison.OrdinalIgnoreCase) >= 0
                || instruction.IndexOf("right-click -> Create Empty", System.StringComparison.OrdinalIgnoreCase) >= 0
                || instruction.IndexOf("GameObject ->", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return PyralisAuthoringActionSurface.Hierarchy;

            if (instruction.IndexOf("Play Mode", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return PyralisAuthoringActionSurface.PlayMode;

            if (instruction.IndexOf("Inspector", System.StringComparison.OrdinalIgnoreCase) >= 0
                || instruction.IndexOf("Add Component", System.StringComparison.OrdinalIgnoreCase) >= 0
                || instruction.IndexOf("object picker", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return PyralisAuthoringActionSurface.Inspector;

            return PyralisAuthoringActionSurface.AuthoringWindow;
        }

        public static string GetRuntimePatternLabel(RuntimePatternDefinition pattern)
        {
            if (pattern == null)
                return "(Missing)";

            if (!string.IsNullOrWhiteSpace(pattern.displayName))
                return pattern.displayName;

            return !string.IsNullOrWhiteSpace(pattern.patternId) ? pattern.patternId : pattern.name;
        }

    }
}