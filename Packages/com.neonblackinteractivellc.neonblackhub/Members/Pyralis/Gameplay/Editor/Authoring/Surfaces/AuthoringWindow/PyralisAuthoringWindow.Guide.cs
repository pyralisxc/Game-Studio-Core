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
    public partial class PyralisAuthoringWindow
    {
        private void DrawGuideMode(Object selection, Object activeSetup, PyralisAuthoringSetupGraph contextGraph)
        {
            PyralisAuthoringCurrentStepGraphRow currentStep = PyralisAuthoringSetupGraphProjection.BuildCurrentStepRow(contextGraph);
            if (ShouldShowSelectionFirstGuide(selection, activeSetup))
            {
                EditorGUILayout.LabelField("Selected Object Next Step", EditorStyles.boldLabel);
                DrawCurrentStepPanel(selection, currentStep);

                EditorGUILayout.Space(10f);
                EditorGUILayout.LabelField("What This Selection Does", EditorStyles.boldLabel);
                PyralisSelectedContextRenderer.Draw(selection, contextGraph, currentStep);
                DrawSelectionGuide(selection, contextGraph);

                EditorGUILayout.Space(10f);
                DrawCurrentIntentGuide(contextGraph);
                DrawReflectiveContracts(contextGraph);
            }
            else
            {
                DrawCurrentIntentGuide(contextGraph);
                DrawReflectiveContracts(contextGraph);

                EditorGUILayout.Space(10f);
                EditorGUILayout.LabelField("What This Selection Does", EditorStyles.boldLabel);
                PyralisSelectedContextRenderer.Draw(selection, contextGraph, currentStep);
                DrawSelectionGuide(selection, contextGraph);
            }

            if (activeSetup == null || activeSetup == selection)
                return;

            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Steady Setup Context", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Active Setup", $"{activeSetup.name} ({activeSetup.GetType().Name})", EditorStyles.wordWrappedLabel);
                PyralisAuthoringSetupGraph activeGraph = GetCachedCurrentSetupGraph(activeSetup);
                PyralisAuthoringCurrentStepGraphRow activeCurrentStep = PyralisAuthoringSetupGraphProjection.BuildCurrentStepRow(activeGraph);
                EditorGUILayout.LabelField("Route", activeCurrentStep.RouteName, EditorStyles.wordWrappedLabel);
                EditorGUILayout.LabelField("Next Required Step", activeCurrentStep.Message, EditorStyles.wordWrappedLabel);
            }
        }

        private static bool ShouldShowSelectionFirstGuide(Object selection, Object activeSetup)
        {
            return activeSetup == null
                && selection is GameObject selectedGameObject
                && selectedGameObject.GetComponent<GameplaySessionBootstrap>() == null;
        }

        private static void DrawCurrentStepPanel(Object selection, PyralisAuthoringCurrentStepGraphRow currentStep)
        {
            if (currentStep == null)
                return;

            EditorGUILayout.LabelField("Current Step", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(currentStep.RouteName, currentStep.Label, EditorStyles.miniBoldLabel);
                PyralisAuthoringWindowText.DrawSemanticHelpBox(currentStep.Message, GetCurrentStepMessageType(currentStep));

                EditorGUILayout.LabelField("Primary Action", EditorStyles.miniBoldLabel);
                DrawPrimaryAction(selection, currentStep);

                const string key = "Pyralis.AuthoringWindow.CurrentStep.Why";
                bool isOpen = ServiceStepFoldouts.TryGetValue(key, out bool value) && value;
                isOpen = EditorGUILayout.Foldout(isOpen, "Why This Matters", true);
                ServiceStepFoldouts[key] = isOpen;

                if (isOpen)
                    PyralisAuthoringWindowText.DrawSemanticMiniLabel(currentStep.Detail);
            }
        }

        private void DrawCurrentIntentGuide(PyralisAuthoringSetupGraph graph)
        {
            EditorGUILayout.LabelField("Route Guide", EditorStyles.boldLabel);
            PyralisAuthoringGraphJsonExportControl.DrawRouteProofTrace(graph);

            PyralisAuthoringRouteWorkingProjection route = PyralisAuthoringSetupGraphProjection.BuildRouteWorkingProjection(graph);
            if (route.OrderedSteps.Count > 0)
            {
                PyralisAuthoringWindowText.DrawSemanticHelpBox(
                    "Follow this graph-derived setup-card path from a fresh scene toward the first playable proof. Intent helps choose what to wire, then the current setup graph, setup-flow evidence, and validation decide the ordered Unity actions.",
                    MessageType.Info);
                DrawRouteStepRows(route.OrderedSteps);
                return;
            }

            PyralisAuthoringWindowText.DrawSemanticHelpBox(
                "Guide renders the resolved setup graph. Open Intent to filter route capabilities, then create or wire gameplay assets so Guide can show the graph-backed route path.",
                MessageType.Info);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                PyralisAuthoringWindowPrimitives.DrawMiniField("Next Surface", "Intent");
                PyralisAuthoringWindowPrimitives.DrawMiniField("Route Contract", "Use Intent as the graph filter, then express durable setup through SessionDefinition, GameModeDefinition, participants, pawns, feature modules, scene objects, and contracts.");
                PyralisAuthoringWindowPrimitives.DrawMiniField("After That", "Return to Guide for graph-ranked setup nodes, proof support, and reflective contracts.");
            }
        }

        private static void DrawRouteStepRows(IReadOnlyList<PyralisAuthoringRouteStepRow> rows)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                for (int i = 0; i < rows.Count; i++)
                    DrawRouteStepRow(rows[i]);
            }
        }

        private static void DrawRouteStepRow(PyralisAuthoringRouteStepRow row)
        {
            if (row == null || row.Node == null)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                string foldoutKey = "Pyralis.AuthoringWindow.Guide.RouteStep." + row.StableId;
                bool expanded = GetFoldout(IntentRowFoldouts, foldoutKey, row.IsCurrentAction);
                using (new EditorGUILayout.HorizontalScope())
                {
                    expanded = EditorGUILayout.Foldout(expanded, new GUIContent($"{row.Sequence}. {row.Label}", row.Message), true);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(row.RoleLabel, GUILayout.Width(104f));
                    EditorGUILayout.LabelField(GetRouteStepStatus(row), GUILayout.Width(92f));
                }

                SetFoldout(IntentRowFoldouts, foldoutKey, expanded);
                PyralisAuthoringWindowPrimitives.DrawMiniField("Path", $"{row.PhaseLabel} / {row.RoleLabel}", "Route phase and role are derived from graph nodes and edges.");
                PyralisAuthoringWindowPrimitives.DrawMiniField("Why", row.Reason, "Why this graph step appears in the route path.");
                if (!string.IsNullOrWhiteSpace(row.UnityActionLabel))
                    PyralisAuthoringWindowPrimitives.DrawMiniField("Unity Action", row.UnityActionLabel, "Where to do this in Unity.");

                if (!expanded)
                {
                    PyralisAuthoringWindowPrimitives.DrawMiniList("Customization", row.CustomizationMoments, "Creator-owned choices to make after the route skeleton is understood.", 2);
                    return;
                }

                PyralisAuthoringWindowPrimitives.DrawMiniField("What It Means", row.Message, "Guidance from the resolved setup graph node.");
                PyralisAuthoringWindowPrimitives.DrawMiniList("Assignment Fields", row.AssignmentFields, "Unity fields or objects the creator may need to inspect or assign.");
                PyralisAuthoringWindowPrimitives.DrawMiniList("Customization", row.CustomizationMoments, "Creator-owned choices. Authoring guides these choices; it does not pick them.");
                PyralisAuthoringWindowPrimitives.DrawMiniField("Source", row.SourceOrigin.ToString(), "Where this setup meaning came from.");
            }
        }

        private static string GetRouteStepStatus(PyralisAuthoringRouteStepRow row)
        {
            if (row == null)
                return "Unknown";

            return row.EvidenceState switch
            {
                PyralisAuthoringGraphEvidenceState.Ready => "Ready",
                PyralisAuthoringGraphEvidenceState.Optional => "Can wait",
                PyralisAuthoringGraphEvidenceState.Missing => "Missing",
                PyralisAuthoringGraphEvidenceState.CandidateDetected => "Suggested",
                PyralisAuthoringGraphEvidenceState.Blocked => "Blocked",
                _ => "Unknown"
            };
        }

        private static void DrawReflectiveContracts(PyralisAuthoringSetupGraph graph)
        {
            IReadOnlyList<PyralisAuthoringReflectiveContractGraphRow> rows = PyralisAuthoringSetupGraphProjection.BuildReflectiveContractRows(graph);
            if (rows == null || rows.Count == 0)
                return;

            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Reflective Design Contracts", EditorStyles.boldLabel);
            PyralisAuthoringWindowText.DrawSemanticHelpBox("These contracts are discovered reflectively from feature code and attributes. They ensure the scene state matches the design intent.", MessageType.Info);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                foreach (PyralisAuthoringReflectiveContractGraphRow row in rows)
                    DrawReflectiveContractRow(row);
            }
        }

        private static void DrawReflectiveContractRow(PyralisAuthoringReflectiveContractGraphRow row)
        {
            MessageType msgType = row.EvidenceState switch
            {
                PyralisAuthoringGraphEvidenceState.Ready => MessageType.Info,
                PyralisAuthoringGraphEvidenceState.Missing => MessageType.Warning,
                PyralisAuthoringGraphEvidenceState.Blocked => MessageType.Error,
                _ => MessageType.None
            };

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    string statusPrefix = row.EvidenceState == PyralisAuthoringGraphEvidenceState.Ready ? "[Ready]" : "[Needs Work]";
                    EditorGUILayout.LabelField($"{statusPrefix} {row.Label}", EditorStyles.boldLabel);

                    if (row.Target != null)
                    {
                        if (GUILayout.Button("Ping", GUILayout.Width(44f)))
                            EditorGUIUtility.PingObject(row.Target);

                        if (GUILayout.Button("Select", GUILayout.Width(56f)))
                            Selection.activeObject = row.Target;
                    }
                }

                if (!string.IsNullOrWhiteSpace(row.Message))
                    EditorGUILayout.HelpBox(row.Message, msgType);
            }
        }

        private static void DrawPrimaryAction(Object selection, PyralisAuthoringCurrentStepGraphRow currentStep)
        {
            if (currentStep != null && currentStep.NativeAction.HasValue)
            {
                PyralisAuthoringSurfaceBeacon.DrawNativeAction(currentStep.NativeAction.Value, currentStep.NativeAction.Value.ToGuidanceSentence());
            }

            PyralisPrimaryActionGuidance guidance = PyralisCurrentStepPrimaryActionGuidance.Build(selection, currentStep);
            if (!string.IsNullOrWhiteSpace(guidance.Message))
                PyralisAuthoringWindowText.DrawSemanticHelpBox(guidance.Message, guidance.MessageType);
            if (!string.IsNullOrWhiteSpace(guidance.Detail))
                PyralisAuthoringWindowText.DrawSemanticMiniLabel(guidance.Detail);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(selection == null))
                {
                    if (GUILayout.Button("Inspect Selection"))
                    {
                        Selection.activeObject = selection;
                        EditorGUIUtility.PingObject(selection);
                    }
                }
            }
        }

        private static MessageType GetCurrentStepMessageType(PyralisAuthoringCurrentStepGraphRow currentStep)
        {
            if (currentStep == null)
                return MessageType.Info;

            switch (currentStep.EvidenceState)
            {
                case PyralisAuthoringGraphEvidenceState.Blocked:
                    return MessageType.Error;
                case PyralisAuthoringGraphEvidenceState.Missing:
                    return MessageType.Warning;
                default:
                    return MessageType.Info;
            }
        }

        private static void DrawSelectionGuide(Object selection, PyralisAuthoringSetupGraph graph)
        {
            PyralisAuthoringSelectedContextGraphRow selectedContext = PyralisAuthoringSetupGraphProjection.BuildSelectedContextRow(graph, selection);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Important Values", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(selectedContext.Role, EditorStyles.wordWrappedMiniLabel);

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("What To Check First", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(!string.IsNullOrWhiteSpace(selectedContext.NextCheck) ? selectedContext.NextCheck : "Use Map and Hygiene to find the next unresolved graph node.", EditorStyles.wordWrappedMiniLabel);

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Runtime Meaning", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(!string.IsNullOrWhiteSpace(selectedContext.RuntimeMeaning) ? selectedContext.RuntimeMeaning : "No graph context has been resolved for this selection yet.", EditorStyles.wordWrappedMiniLabel);
            }
        }
    }
}
