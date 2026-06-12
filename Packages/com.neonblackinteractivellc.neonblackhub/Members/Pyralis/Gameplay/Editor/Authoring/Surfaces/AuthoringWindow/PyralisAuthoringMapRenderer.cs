using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Editor.Inspectors;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisAuthoringMapRenderer
    {
        public static void Draw(Object activeSetup, Object selection, PyralisAuthoringRouteReport report)
        {
            EditorGUILayout.LabelField("Setup Map", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Use this page to understand how the active setup is connected. Edit actual fields in the Inspector when a row names a missing link.", MessageType.Info);
            DrawActiveAndSelectedContext(activeSetup, selection);
            DrawYouAreHereChain(activeSetup);
            PyralisSetupChainRenderer.Draw(activeSetup, report, false);
            DrawSceneSurfaceSnapshot(activeSetup);
            DrawReadinessSummary(activeSetup);
        }

        private static void DrawActiveAndSelectedContext(Object activeSetup, Object selection)
        {
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Selected Authoring Context", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Active Setup", activeSetup != null ? $"{activeSetup.name} ({activeSetup.GetType().Name})" : "No setup context", EditorStyles.wordWrappedLabel);
                EditorGUILayout.LabelField("Current Selection", selection != null ? $"{selection.name} ({selection.GetType().Name})" : "Nothing selected", EditorStyles.wordWrappedLabel);
            }
        }

        private static void DrawYouAreHereChain(Object activeSetup)
        {
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("You Are Here", EditorStyles.boldLabel);

            GameplaySessionBootstrap bootstrap = PyralisAuthoringSetupContextResolver.GetSelectedBootstrap(activeSetup);
            PyralisAuthoringRouteDescriptor route = PyralisAuthoringRouteDescriptor.Build(activeSetup);
            PyralisAuthoringSceneSurfaceSnapshot surfaces = PyralisAuthoringSceneSurfaceSnapshot.Build(activeSetup);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawSetupChainRow("Scene Root", bootstrap, bootstrap != null, false, "Scene object that starts the session.");
                DrawSetupChainRow("Session", route.Session, route.Session != null, false, "Asset that names game rules and participants.");
                DrawSetupChainRow("Game Rules", route.Mode, route.Mode != null, false, "Ruleset that chooses the setup profile.");
                DrawSetupChainRow("Setup Profile", route.SetupProfile, route.SetupProfile != null, false, "Editable capability contract for this route.");
                DrawSetupChainRow("Capabilities", route.SetupProfile, route.HasSelectedCapabilities, false, GetCapabilityChainMessage(route));
                DrawSetupChainRow("Participants", route.Session, route.HasParticipants, false, route.HasParticipants ? "Players, seats, hands, factions, or command owners are assigned." : "Assign at least one default participant.");
                DrawSetupChainRow("Pawn / No Pawn", GetFirstParticipant(route.Session), GetPawnChainReady(route), !route.RequiresPawn, GetPawnChainMessage(route));
                DrawSetupChainRow("Scene Surfaces", bootstrap, GetRecommendedSceneSurfacesReady(surfaces), false, GetSceneSurfaceChainMessage(surfaces));
            }
        }

        private static void DrawSetupChainRow(string label, Object target, bool isReady, bool isOptional, string message)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                string status = PyralisAuthoringWindowPrimitives.GetReadinessBadge(isReady, target, isOptional);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
                    using (new EditorGUI.DisabledScope(target == null))
                    {
                        if (GUILayout.Button("Inspect", GUILayout.Width(72f)))
                        {
                            Selection.activeObject = target;
                            EditorGUIUtility.PingObject(target);
                        }
                    }
                }

                EditorGUI.indentLevel++;
                PyralisAuthoringWindowText.DrawSemanticMiniLabel($"{status}: {message}");
                EditorGUI.indentLevel--;
            }
        }

        private static string GetCapabilityChainMessage(PyralisAuthoringRouteDescriptor route)
        {
            if (route.SetupProfile == null)
                return "Create or assign the setup profile before choosing capabilities.";

            if (!route.HasAssignedPatterns)
                return "Choose capability ingredients before scene wiring.";

            if (!route.HasValidPatterns)
                return "Fix setup capability validation before trusting route guidance.";

            return route.RouteName;
        }

        private static bool GetPawnChainReady(PyralisAuthoringRouteDescriptor route)
        {
            if (!route.RequiresPawn)
                return true;

            return route.HasParticipants && string.IsNullOrWhiteSpace(route.ParticipantPawnIssue);
        }

        private static string GetPawnChainMessage(PyralisAuthoringRouteDescriptor route)
        {
            if (!route.RequiresPawn)
                return "No-pawn route: empty PawnDefinition fields are correct unless you intentionally add actor bodies.";

            if (string.IsNullOrWhiteSpace(route.ParticipantPawnIssue))
                return "Pawn-backed route has participant pawn setup.";

            return route.ParticipantPawnIssue;
        }

        private static Object GetFirstParticipant(SessionDefinition session)
        {
            if (session == null || session.defaultParticipants == null || session.defaultParticipants.Length == 0)
                return session;

            return session.defaultParticipants[0] != null ? session.defaultParticipants[0] : session;
        }

        private static bool GetRecommendedSceneSurfacesReady(PyralisAuthoringSceneSurfaceSnapshot snapshot)
        {
            if (snapshot == null || snapshot.Rows.Count == 0)
                return true;

            for (int i = 0; i < snapshot.Rows.Count; i++)
            {
                PyralisAuthoringSceneSurfaceRow row = snapshot.Rows[i];
                if (row != null && !row.SupportsFirstProofAttempt)
                    return false;
            }

            return true;
        }

        private static string GetSceneSurfaceChainMessage(PyralisAuthoringSceneSurfaceSnapshot snapshot)
        {
            if (snapshot == null)
                return "Scene surface scan is unavailable.";

            int missing = 0;
            List<string> missingSurfaces = new List<string>();
            for (int i = 0; i < snapshot.Rows.Count; i++)
            {
                PyralisAuthoringSceneSurfaceRow row = snapshot.Rows[i];
                if (row != null && !row.SupportsFirstProofAttempt)
                {
                    missing++;
                    if (!string.IsNullOrWhiteSpace(row.Surface))
                        missingSurfaces.Add(row.Surface);
                }
            }

            if (missing == 0)
                return "Route-recommended scene surface evidence is present or not needed yet. Play Mode still proves behavior.";

            string missingText = string.Join(", ", missingSurfaces);
            return $"{missing} proof enhancer scene surface(s) are not detected yet: {missingText}. Scroll to Scene Surface Scan or Overview feature cards, then create the needed object through the native Hierarchy, Project, or Inspector path.";
        }

        private static void DrawSceneSurfaceSnapshot(Object activeSetup)
        {
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Scene Surface Scan", EditorStyles.boldLabel);
            PyralisAuthoringWindowText.DrawSemanticHelpBox("This reads ordinary Unity scene objects too. A found surface is evidence, not proof: Play Mode still owns the final route proof.", MessageType.Info);

            PyralisAuthoringSceneSurfaceSnapshot snapshot = PyralisAuthoringSceneSurfaceSnapshot.Build(activeSetup);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                for (int i = 0; i < snapshot.Rows.Count; i++)
                    DrawSceneSurfaceRow(snapshot.Rows[i]);
            }
        }

        private static void DrawSceneSurfaceRow(PyralisAuthoringSceneSurfaceRow row)
        {
            if (row == null)
                return;

            string status = $"[{PyralisAuthoringLabelUtility.GetEvidenceLabel(row.EvidenceState)}]";
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(row.Surface, status, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                PyralisAuthoringWindowPrimitives.DrawMiniField("Evidence", row.Current);
                PyralisAuthoringWindowPrimitives.DrawMiniField("Next fix", row.NextFix);
                EditorGUI.indentLevel--;
            }
        }

        private static void DrawReadinessSummary(Object selection)
        {
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Readiness Summary", EditorStyles.boldLabel);

            GameplaySessionBootstrap bootstrap = PyralisAuthoringSetupContextResolver.GetSelectedBootstrap(selection);
            SessionDefinition session = PyralisAuthoringSetupContextResolver.GetSelectedSession(selection, bootstrap);
            GameModeDefinition mode = PyralisAuthoringSetupContextResolver.GetSelectedMode(selection, session);
            GameSetupProfile setupProfile = PyralisAuthoringSetupContextResolver.GetSelectedSetupProfile(selection, mode);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                PyralisSetupFlowReport flowReport = bootstrap != null ? PyralisSetupFlowValidator.BuildReport(bootstrap) : null;
                DrawCompactReadinessRow("Scene Root", bootstrap != null, false, bootstrap);
                DrawCompactReadinessRow("Session", session != null, false, session);
                DrawCompactReadinessRow("Game Rules", mode != null, false, mode);
                DrawCompactReadinessRow("Setup Profile", setupProfile != null, false, setupProfile);
                DrawCompactReadinessRow("Capabilities", GetStepReady(flowReport, PyralisSetupFlowStepId.AddRuntimePatterns, PyralisRuntimeCapabilityCatalogRenderer.HasAnyRuntimeCapability(setupProfile)), false, setupProfile, GetStepMessage(flowReport, PyralisSetupFlowStepId.AddRuntimePatterns));
                DrawCompactReadinessRow("Players / Seats", GetStepReady(flowReport, PyralisSetupFlowStepId.AssignDefaultParticipants, session != null && session.defaultParticipants != null && session.defaultParticipants.Length > 0), false, session, GetStepMessage(flowReport, PyralisSetupFlowStepId.AssignDefaultParticipants));
                DrawCompactReadinessRow("Pawn / No Pawn", GetStepReady(flowReport, PyralisSetupFlowStepId.AssignParticipantPawn, HasAnyPawn(session)), true, session, GetStepMessage(flowReport, PyralisSetupFlowStepId.AssignParticipantPawn));
                DrawCompactReadinessRow("Scene Roots", GetStepReady(flowReport, PyralisSetupFlowStepId.SceneAndPrefabReadiness, bootstrap != null), true, bootstrap, GetStepMessage(flowReport, PyralisSetupFlowStepId.SceneAndPrefabReadiness));
            }
        }

        private static void DrawCompactReadinessRow(string label, bool isReady, bool isOptional, Object target = null, string message = null)
        {
            string targetName = target != null ? $" ({target.name})" : string.Empty;
            EditorGUILayout.LabelField(label, PyralisAuthoringWindowPrimitives.GetReadinessBadge(isReady, target, isOptional) + targetName);
            if (!string.IsNullOrWhiteSpace(message))
            {
                EditorGUI.indentLevel++;
                PyralisAuthoringWindowText.DrawSemanticMiniLabel(message);
                EditorGUI.indentLevel--;
            }
        }

        private static bool GetStepReady(PyralisSetupFlowReport report, PyralisSetupFlowStepId stepId, bool fallback)
        {
            PyralisSetupFlowStep step = report != null ? report.GetStep(stepId) : null;
            return step != null ? step.Status == PyralisSetupFlowStepStatus.Ready || step.Status == PyralisSetupFlowStepStatus.Optional : fallback;
        }

        private static string GetStepMessage(PyralisSetupFlowReport report, PyralisSetupFlowStepId stepId)
        {
            PyralisSetupFlowStep step = report != null ? report.GetStep(stepId) : null;
            return step != null ? step.Message : null;
        }

        private static bool HasAnyPawn(SessionDefinition session)
        {
            if (session == null || session.defaultParticipants == null)
                return false;

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                if (participant != null && participant.defaultPawn != null)
                    return true;
            }

            return false;
        }
    }
}
