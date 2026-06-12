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
        private void DrawGuideMode(Object selection, PyralisAuthoringRouteReport report, Object activeSetup, PyralisAuthoringRouteReport activeSetupReport)
        {
            if (ShouldShowSelectionFirstGuide(selection, activeSetup))
            {
                EditorGUILayout.LabelField("Selected Object Next Step", EditorStyles.boldLabel);
                DrawCurrentStepPanel(selection, report);

                EditorGUILayout.Space(10f);
                EditorGUILayout.LabelField("What This Selection Does", EditorStyles.boldLabel);
                PyralisSelectedContextRenderer.Draw(selection, report, FillMissingRuntimePatternText);
                DrawSelectionGuide(selection, report);

                EditorGUILayout.Space(10f);
                DrawCurrentIntentGuide(GetCachedIntentModel());
                DrawReflectiveContracts(activeSetup);
            }
            else
            {
                DrawCurrentIntentGuide(GetCachedIntentModel());
                DrawReflectiveContracts(activeSetup);

                EditorGUILayout.Space(10f);
                EditorGUILayout.LabelField("What This Selection Does", EditorStyles.boldLabel);
                PyralisSelectedContextRenderer.Draw(selection, report, FillMissingRuntimePatternText);
                DrawSelectionGuide(selection, report);
            }

            if (activeSetup == null || activeSetup == selection)
                return;

            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Steady Setup Context", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Active Setup", $"{activeSetup.name} ({activeSetup.GetType().Name})", EditorStyles.wordWrappedLabel);
                if (activeSetupReport != null)
                {
                    EditorGUILayout.LabelField("Route", activeSetupReport.RouteName, EditorStyles.wordWrappedLabel);
                    EditorGUILayout.LabelField("Next Required Step", activeSetupReport.NextStep, EditorStyles.wordWrappedLabel);
                }
            }
        }

        private static bool ShouldShowSelectionFirstGuide(Object selection, Object activeSetup)
        {
            return activeSetup == null
                && selection is GameObject selectedGameObject
                && selectedGameObject.GetComponent<GameplaySessionBootstrap>() == null;
        }

        private static void DrawCurrentStepPanel(Object selection, PyralisAuthoringRouteReport report)
        {
            if (report == null)
                return;

            EditorGUILayout.LabelField("Current Step", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(report.RouteName, EditorStyles.miniBoldLabel);
                PyralisAuthoringWindowText.DrawSemanticHelpBox(report.NextStep, report.ValidationIssues.Count > 0 ? MessageType.Warning : MessageType.Info);

                EditorGUILayout.LabelField("Primary Action", EditorStyles.miniBoldLabel);
                DrawPrimaryAction(selection, report);

                const string key = "Pyralis.AuthoringWindow.CurrentStep.Why";
                bool isOpen = ServiceStepFoldouts.TryGetValue(key, out bool value) && value;
                isOpen = EditorGUILayout.Foldout(isOpen, "Why This Matters", true);
                ServiceStepFoldouts[key] = isOpen;

                if (isOpen)
                    PyralisAuthoringWindowText.DrawSemanticMiniLabel(report.RouteGuidance);
            }
        }

        private static void DrawCurrentIntentGuide(PyralisAuthoringIntentModel model)
        {
            EditorGUILayout.LabelField("Current Intent Guide", EditorStyles.boldLabel);
            PyralisAuthoringWindowText.DrawSemanticHelpBox(
                "Ranked cookbook cards for the selected Intent. Use these to decide what to create, inspect, customize, or defer. Facts remains the full dictionary outside the current intent.",
                MessageType.Info);

            if (model == null)
            {
                EditorGUILayout.LabelField("No intent model is available yet.", EditorStyles.wordWrappedMiniLabel);
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                PyralisAuthoringWindowPrimitives.DrawMiniField("Intent Summary", model.Summary);
                PyralisAuthoringWindowPrimitives.DrawMiniField("Matched Intent Families", model.MatchingIntents != null && model.MatchingIntents.Count > 0
                    ? JoinFactDisplayNames(model.MatchingIntents)
                    : "No named family matched yet. Toggle intent controls to shape the guide.");
            }

            DrawIntentRows(
                "Recommended Cards",
                "Highest-ranked facts and capabilities for the current intent. Start at the top unless Overview reports a blocking setup issue.",
                model.Recommendations,
                "Cards are sorted by lane, goals, related route intent, and caution fit.");

            DrawIntentRows(
                "Caution Cards",
                "Useful facts that are not a clean fit for the selected lane. Keep them visible as tradeoffs, not primary steps.",
                model.Cautions,
                "Cautions help prevent pawn, combat, UI, board, or networking assumptions from leaking into the wrong route.");
        }

        private void DrawReflectiveContracts(Object activeSetup)
        {
            if (activeSetup == null)
                return;

            GameplaySessionBootstrap bootstrap = PyralisAuthoringSetupContextResolver.GetSelectedBootstrap(activeSetup);
            if (bootstrap == null)
                return;

            PyralisSetupFlowReport flowReport = PyralisSetupFlowValidator.BuildReport(bootstrap);
            if (flowReport == null || flowReport.Steps.Count == 0)
                return;

            List<PyralisSetupFlowStep> reflectiveSteps = new List<PyralisSetupFlowStep>();
            foreach (PyralisSetupFlowStep step in flowReport.Steps)
            {
                if (step.StepId == PyralisSetupFlowStepId.Unknown)
                    reflectiveSteps.Add(step);
            }

            if (reflectiveSteps.Count == 0)
                return;

            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Reflective Design Contracts", EditorStyles.boldLabel);
            PyralisAuthoringWindowText.DrawSemanticHelpBox("These contracts are discovered reflectively from feature code and attributes. They ensure the scene state matches the design intent.", MessageType.Info);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                foreach (PyralisSetupFlowStep step in reflectiveSteps)
                    DrawReflectiveContractRow(step);
            }
        }

        private static void DrawReflectiveContractRow(PyralisSetupFlowStep step)
        {
            MessageType msgType = step.Status switch
            {
                PyralisSetupFlowStepStatus.Ready => MessageType.Info,
                PyralisSetupFlowStepStatus.Missing => MessageType.Warning,
                PyralisSetupFlowStepStatus.Blocked => MessageType.Error,
                _ => MessageType.None
            };

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    string statusPrefix = step.Status == PyralisSetupFlowStepStatus.Ready ? "[Ready]" : "[Needs Work]";
                    EditorGUILayout.LabelField($"{statusPrefix} {step.Label}", EditorStyles.boldLabel);

                    if (step.ReferencedObject != null)
                    {
                        if (GUILayout.Button("Ping", GUILayout.Width(44f)))
                            EditorGUIUtility.PingObject(step.ReferencedObject);

                        if (GUILayout.Button("Select", GUILayout.Width(56f)))
                            Selection.activeObject = step.ReferencedObject;
                    }
                }

                if (!string.IsNullOrWhiteSpace(step.Message))
                    EditorGUILayout.HelpBox(step.Message, msgType);
            }
        }

        private static void DrawPrimaryAction(Object selection, PyralisAuthoringRouteReport report)
        {
            PyralisPrimaryActionGuidance guidance = PyralisCurrentStepPrimaryActionGuidance.Build(selection, report);
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

        private static string JoinFactDisplayNames(IReadOnlyList<PyralisAuthoringFact> facts)
        {
            if (facts == null || facts.Count == 0)
                return string.Empty;

            List<string> names = new List<string>();
            for (int i = 0; i < facts.Count; i++)
            {
                if (facts[i] != null && !string.IsNullOrWhiteSpace(facts[i].DisplayName))
                    names.Add(facts[i].DisplayName);
            }

            return names.Count > 0 ? string.Join(", ", names) : string.Empty;
        }

        private static void DrawSelectionGuide(Object selection, PyralisAuthoringRouteReport report)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Important Values", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(GetImportantValuesText(selection), EditorStyles.wordWrappedMiniLabel);

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("What To Check First", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(report.NextStep, EditorStyles.wordWrappedMiniLabel);

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Runtime Meaning", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(report.RouteGuidance, EditorStyles.wordWrappedMiniLabel);
            }
        }

        private static string GetImportantValuesText(Object selection)
        {
            return selection switch
            {
                GameplaySessionBootstrap => "Session reference, spawn points, and scene-level setup helpers.",
                SessionDefinition => "Game Rules, players/seats, local/network mode, and participant limits.",
                GameModeDefinition => "Setup Profile plus rule-level defaults for the playable loop.",
                GameSetupProfile => "Capability ingredients that describe what this game route needs before scene wiring starts.",
                RuntimePatternDefinition => "Optional advanced route contract with capability family, participant embodiment, presentation lanes, first-proof requirements, supported control surfaces, required systems, and setup notes.",
                ParticipantDefinition => "Display name, seat index, input ownership, and optional Pawn Actor.",
                PawnDefinition => "Pawn prefab, profiles, feature modules, and presentation setup.",
                Component component => component is PawnRoot
                    ? "PawnRoot marks the prefab root that Pyralis treats as a pawn actor."
                    : "This component participates in the selected GameObject's runtime behavior. Use the Inspector for field values and this window for setup meaning.",
                GameObject => "Pyralis components on this object and the likely authoring root for setup.",
                null => "Select a Pyralis setup asset, scene root, pawn prefab, or component to see its authoring meaning.",
                _ => "Use this asset's Inspector for fields, and use this page to understand how it fits into setup."
            };
        }
    }
}
