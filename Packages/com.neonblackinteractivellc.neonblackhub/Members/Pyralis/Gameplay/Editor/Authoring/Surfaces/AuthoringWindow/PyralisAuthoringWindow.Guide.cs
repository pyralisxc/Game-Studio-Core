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
        private void DrawGuideMode(Object selection, Object activeSetup)
        {
            PyralisAuthoringSetupGraph contextGraph = PyralisAuthoringSetupGraphBuilder.Build(activeSetup != null ? activeSetup : selection);
            PyralisAuthoringCurrentStepGraphRow currentStep = PyralisAuthoringSetupGraphProjection.BuildCurrentStepRow(contextGraph);
            if (ShouldShowSelectionFirstGuide(selection, activeSetup))
            {
                EditorGUILayout.LabelField("Selected Object Next Step", EditorStyles.boldLabel);
                DrawCurrentStepPanel(selection, currentStep);

                EditorGUILayout.Space(10f);
                EditorGUILayout.LabelField("What This Selection Does", EditorStyles.boldLabel);
                PyralisSelectedContextRenderer.Draw(selection, contextGraph, currentStep, FillMissingRuntimePatternText);
                DrawSelectionGuide(selection, contextGraph);

                EditorGUILayout.Space(10f);
                DrawCurrentIntentGuide(GetCachedIntentModel());
                DrawReflectiveContracts(contextGraph);
            }
            else
            {
                DrawCurrentIntentGuide(GetCachedIntentModel());
                DrawReflectiveContracts(contextGraph);

                EditorGUILayout.Space(10f);
                EditorGUILayout.LabelField("What This Selection Does", EditorStyles.boldLabel);
                PyralisSelectedContextRenderer.Draw(selection, contextGraph, currentStep, FillMissingRuntimePatternText);
                DrawSelectionGuide(selection, contextGraph);
            }

            if (activeSetup == null || activeSetup == selection)
                return;

            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Steady Setup Context", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Active Setup", $"{activeSetup.name} ({activeSetup.GetType().Name})", EditorStyles.wordWrappedLabel);
                PyralisAuthoringSetupGraph activeGraph = PyralisAuthoringSetupGraphBuilder.Build(activeSetup);
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

        private static void DrawSelectionGuide(Object selection, PyralisAuthoringSetupGraph graph)
        {
            PyralisAuthoringSelectedContextGraphRow selectedContext = PyralisAuthoringSetupGraphProjection.BuildSelectedContextRow(graph, selection);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Important Values", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(selectedContext.Role, EditorStyles.wordWrappedMiniLabel);

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("What To Check First", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(!string.IsNullOrWhiteSpace(selectedContext.NextCheck) ? selectedContext.NextCheck : "Use Map and Validate to find the next unresolved graph node.", EditorStyles.wordWrappedMiniLabel);

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Runtime Meaning", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(!string.IsNullOrWhiteSpace(selectedContext.RuntimeMeaning) ? selectedContext.RuntimeMeaning : "No graph context has been resolved for this selection yet.", EditorStyles.wordWrappedMiniLabel);
            }
        }
    }
}
