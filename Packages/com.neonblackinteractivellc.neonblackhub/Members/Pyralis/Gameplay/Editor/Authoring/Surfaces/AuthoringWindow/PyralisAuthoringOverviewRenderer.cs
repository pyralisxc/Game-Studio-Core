using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisAuthoringOverviewRenderer
    {
        public static void DrawGuidanceCard(PyralisAuthoringOverviewModel model, PyralisAuthoringRouteReport report, PyralisAuthoringSetupGraph graph)
        {
            if (model == null)
                return;

            EditorGUILayout.LabelField("Next Setup Guidance", EditorStyles.miniBoldLabel);
            PyralisAuthoringCurrentStepGraphRow currentStep = PyralisAuthoringSetupGraphProjection.BuildCurrentStepRow(graph, report);
            string guidance = currentStep != null && !string.IsNullOrWhiteSpace(currentStep.Message)
                ? currentStep.Message
                : report != null && !string.IsNullOrWhiteSpace(report.RouteGuidance)
                    ? report.RouteGuidance
                    : model.FirstProofGuidance;
            PyralisAuthoringWindowText.DrawSemanticHelpBox(guidance, MessageType.Info);
            PyralisAuthoringWindowPrimitives.DrawMiniField("Intent vs Setup", "Intent shapes the route. Project, Hierarchy, and Inspector create and wire the user's actual setup.");
            PyralisAuthoringWindowPrimitives.DrawMiniField("Next", currentStep != null && !string.IsNullOrWhiteSpace(currentStep.Label) ? currentStep.Label : model.BestNextAction);
            if (currentStep != null && currentStep.NativeAction.HasValue)
                PyralisAuthoringSurfaceBeacon.DrawNativeAction(currentStep.NativeAction.Value, currentStep.NativeAction.Value.ToGuidanceSentence());
            DrawGraphPriority(graph);
            PyralisAuthoringWindowPrimitives.DrawMiniField("Proof Status", GetFlowTestStatus(model));
        }

        public static void DrawActionButtons(PyralisAuthoringOverviewModel model, Action openIntent, Action openMap, Action openValidate)
        {
            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open Intent"))
                {
                    openIntent?.Invoke();
                }

                if (GUILayout.Button("Open Map"))
                {
                    openMap?.Invoke();
                }

                if (GUILayout.Button("Open Validate"))
                {
                    openValidate?.Invoke();
                }

                Object bestTarget = GetBestOverviewTarget(model);
                using (new EditorGUI.DisabledScope(bestTarget == null))
                {
                    if (GUILayout.Button("Inspect Best Target"))
                        PyralisAuthoringWindowPrimitives.SelectAndPing(bestTarget);
                }
            }
        }

        private static Object GetBestOverviewTarget(PyralisAuthoringOverviewModel model)
        {
            if (model == null)
                return null;

            Object target = GetFirstTarget(model.DoNow);
            if (target != null)
                return target;

            target = GetFirstTarget(model.DoSoon);
            if (target != null)
                return target;

            return GetFirstTarget(model.Later);
        }

        private static Object GetFirstTarget(IReadOnlyList<PyralisAuthoringOverviewIssue> issues)
        {
            if (issues == null)
                return null;

            for (int i = 0; i < issues.Count; i++)
            {
                if (issues[i] != null && issues[i].Target != null)
                    return issues[i].Target;
            }

            return null;
        }

        private static string GetFlowTestStatus(PyralisAuthoringOverviewModel model)
        {
            if (model == null)
                return "Select an active setup before testing the flow.";

            if (model.DoNow.Count > 0)
                return "Not ready to test yet. Clear Do Now in Edit Mode first, then use Play Mode only as the first proof test.";

            if (model.DoSoon.Count > 0)
                return "Ready for a narrow Play Mode proof. Proof Enhancers can make the first test clearer, but setup edits still belong in Edit Mode.";

            return "Ready for first proof. Run the smallest route pass named below, verify one interaction in Play Mode, stop Play Mode, then add one feature at a time.";
        }

        private static void DrawGraphPriority(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return;

            PyralisAuthoringGraphNode next = PyralisAuthoringSetupGraphProjection.FindFirstUnresolvedNode(graph);
            int blocked = PyralisAuthoringSetupGraphProjection.CountNodes(graph, PyralisAuthoringGraphEvidenceState.Blocked);
            int missing = PyralisAuthoringSetupGraphProjection.CountNodes(graph, PyralisAuthoringGraphEvidenceState.Missing);
            PyralisAuthoringWindowPrimitives.DrawMiniField("Resolved Graph", $"{blocked} blocked, {missing} missing");
            if (next != null)
                PyralisAuthoringWindowPrimitives.DrawMiniField("Graph Next", !string.IsNullOrWhiteSpace(next.Guidance) ? $"{next.Label}: {next.Guidance}" : next.Label);
        }

        public static void DrawFirstProofCard(PyralisAuthoringOverviewModel model, PyralisAuthoringSetupGraph graph)
        {
            if (model == null)
                return;

            PyralisAuthoringGraphNode proofNode = PyralisAuthoringSetupGraphProjection.FindCurrentProofNode(graph);
            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("First Playable Proof", proofNode != null ? proofNode.Label : model.FirstProofLabel, EditorStyles.miniBoldLabel);
                PyralisAuthoringWindowPrimitives.DrawMiniField("Setup Surface", GetFirstValue(proofNode?.NativeSetup, model.FirstProofSetupSurface));
                PyralisAuthoringWindowPrimitives.DrawMiniField("Success Looks Like", !string.IsNullOrWhiteSpace(proofNode?.BlockingReason) ? proofNode.BlockingReason : model.FirstProofSuccessCriteria);
                PyralisAuthoringWindowPrimitives.DrawMiniField("Proof Chain", model.FirstProofChainSummary);
                PyralisAuthoringWindowPrimitives.DrawMiniField("Defer Until After Proof", model.FirstProofDeferUntilAfter);
            }
        }

        private static string GetFirstValue(string[] values, string fallback)
        {
            if (values != null && values.Length > 0 && !string.IsNullOrWhiteSpace(values[0]))
                return values[0];

            return fallback;
        }

        public static void DrawPlayModeChecklist(PyralisAuthoringOverviewModel model)
        {
            if (model == null || model.PlayModeChecklist.Count == 0)
                return;

            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Play Mode Checklist", EditorStyles.miniBoldLabel);
                for (int i = 0; i < model.PlayModeChecklist.Count; i++)
                    DrawPlayModeChecklistItem(model.PlayModeChecklist[i]);
            }
        }

        public static void DrawPlayModeChecklistItem(PyralisAuthoringPlayModeChecklistItem item)
        {
            if (item == null)
                return;

            string status = item.Ready ? "Ready" : "Needs edit";
            EditorGUILayout.LabelField(item.Label, status, EditorStyles.miniBoldLabel);
            if (!string.IsNullOrWhiteSpace(item.Detail))
                PyralisAuthoringWindowText.DrawSemanticMiniLabel(item.Detail);
        }

        public static void DrawContractProofGuidance(Object activeSetup, PyralisAuthoringRouteReport report)
        {
            IReadOnlyList<ResolvedAuthoringContractProofGuidanceRow> rows = ResolvedAuthoringContractProofGuidance.Build(activeSetup, report);
            if (rows == null || rows.Count == 0)
                return;

            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Contract Proof Targets", EditorStyles.miniBoldLabel);
                PyralisAuthoringWindowText.DrawSemanticMiniLabel("Feature modules included in this setup can enhance the first proof, but Play Mode remains the proof pass.");
                EditorGUI.indentLevel++;
                for (int i = 0; i < rows.Count; i++)
                    DrawContractProofGuidanceRow(rows[i]);
                EditorGUI.indentLevel--;
            }
        }

        public static void DrawContractProofGuidanceRow(ResolvedAuthoringContractProofGuidanceRow row)
        {
            if (row == null || row.Contract == null)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                string proofLabel = row.ProofFact != null ? row.ProofFact.DisplayName : row.Contract.FirstProofTargetId;
                EditorGUILayout.LabelField(row.Contract.DisplayName, proofLabel, EditorStyles.boldLabel);
                PyralisAuthoringWindowPrimitives.DrawMiniField("Feature Module", row.Contract.StableId);
                PyralisAuthoringWindowPrimitives.DrawMiniField("Proof Target", string.IsNullOrWhiteSpace(row.Contract.FirstProofTargetId) ? "None recorded." : row.Contract.FirstProofTargetId);
                PyralisAuthoringWindowPrimitives.DrawMiniField("Proof Target Exists", row.ProofTargetExists ? "Yes - this contract maps to a route proof card." : "No - the contract points at a missing route proof card.");
                if (!string.IsNullOrWhiteSpace(row.Contract.FirstProofGuidance))
                {
                    PyralisAuthoringWindowPrimitives.DrawMiniField("Developer Proof Guidance", row.Contract.FirstProofGuidance);
                }

                PyralisAuthoringWindowPrimitives.DrawMiniField("Proof Status", GetContractProofStatusText(row));

                if (row.ProofFact != null)
                {
                    PyralisAuthoringWindowPrimitives.DrawMiniField("Play Mode Proof", row.ProofFact.FirstProof);
                    PyralisAuthoringWindowPrimitives.DrawMiniList("Proof Setup Fields", row.ProofFact.AssignmentFields);
                }

                if (row.HasUnsupportedLaneCaution)
                    PyralisAuthoringWindowPrimitives.DrawMiniField("Unsupported Lane Cautions", GetUnsupportedLaneCaution(row));
                else
                    PyralisAuthoringWindowPrimitives.DrawMiniList("Unsupported Lane Cautions", PyralisAuthoringFactExplorerRenderer.ToPresentationModeNames(row.Contract.UnsupportedPresentationModes));
            }
        }

        private static string GetContractProofStatusText(ResolvedAuthoringContractProofGuidanceRow row)
        {
            if (row == null)
                return "No proof guidance available.";

            switch (row.State)
            {
                case ResolvedAuthoringContractProofState.ProofTargetMissing:
                    return "Blocked: proof target is missing from PyralisAuthoringRouteProof.";
                case ResolvedAuthoringContractProofState.ProofBlockedBySetup:
                    return "Proof target exists, but route setup still has blockers. Clear Do Now before Play Mode.";
                default:
                    return "Proof not run in Play Mode. Enter Play only after the selected intent's Do Now setup is clear, then verify this proof target.";
            }
        }

        private static string GetUnsupportedLaneCaution(ResolvedAuthoringContractProofGuidanceRow row)
        {
            if (row == null || row.Contract == null || !row.ActiveLane.HasValue)
                return "No active lane caution.";

            if (!string.IsNullOrWhiteSpace(row.Contract.UnsupportedLaneMessage))
                return row.Contract.UnsupportedLaneMessage;

            return $"{row.Contract.DisplayName} does not support {row.ActiveLane.Value}. Choose a supported feature module or change the pawn presentation profile before Play Mode.";
        }

        public static void DrawLane(string title, string description, IReadOnlyList<PyralisAuthoringOverviewIssue> issues)
        {
            EditorGUILayout.Space(6f);
            int issueCount = issues != null ? issues.Count : 0;
            EditorGUILayout.LabelField(title, GetLaneCountLabel(issueCount), EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField(description, EditorStyles.wordWrappedMiniLabel);
            if (issueCount == 0)
            {
                EditorGUILayout.LabelField(GetEmptyLaneText(title), EditorStyles.wordWrappedMiniLabel);
            }
            else
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < issueCount; i++)
                    DrawOverviewIssueCard(issues[i]);
                EditorGUI.indentLevel--;
            }
        }

        private static void DrawOverviewIssueCard(PyralisAuthoringOverviewIssue issue)
        {
            if (issue == null)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(issue.Label, PyralisAuthoringWindowText.GetStatusLabel(issue.Status), EditorStyles.boldLabel);
                PyralisAuthoringWindowPrimitives.DrawMiniField("Setup Role", PyralisAuthoringWindowText.GetWorkIntentLabel(issue.WorkIntent));
                PyralisAuthoringWindowText.DrawSemanticMiniLabel(issue.Message);
                if (!string.IsNullOrWhiteSpace(issue.NativeActionGuidance))
                {
                    EditorGUILayout.Space(2f);
                    PyralisAuthoringWindowPrimitives.DrawMiniField("Native Unity Action", issue.NativeActionGuidance);
                }

                PyralisAuthoringWindowText.DrawSemanticMiniLabel(issue.Evidence);

                using (new EditorGUI.DisabledScope(issue.Target == null))
                {
                    if (GUILayout.Button("Inspect Target"))
                    {
                        Selection.activeObject = issue.Target;
                        EditorGUIUtility.PingObject(issue.Target);
                    }
                }
            }
        }

        private static string GetLaneCountLabel(int count)
        {
            return count == 1 ? "1 item" : count + " items";
        }

        private static string GetEmptyLaneText(string title)
        {
            switch (title)
            {
                case "Do Now":
                    return "No blockers in this lane.";
                case "Proof Enhancers":
                    return "No route-specific proof helpers are asking for attention right now.";
                case "Feature Cards":
                    return "No optional feature work is competing with this proof.";
                default:
                    return "Nothing in this lane.";
            }
        }

    }
}
