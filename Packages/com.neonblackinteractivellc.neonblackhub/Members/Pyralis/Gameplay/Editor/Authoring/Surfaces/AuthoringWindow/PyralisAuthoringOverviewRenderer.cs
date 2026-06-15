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
        public static void DrawGuidanceCard(PyralisAuthoringOverviewModel model, PyralisAuthoringSetupGraph graph)
        {
            if (model == null)
                return;

            EditorGUILayout.LabelField("Next Unity Action", EditorStyles.miniBoldLabel);
            PyralisAuthoringCurrentStepGraphRow currentStep = PyralisAuthoringSetupGraphProjection.BuildCurrentStepRow(graph);
            string guidance = currentStep != null && !string.IsNullOrWhiteSpace(currentStep.Message)
                ? currentStep.Message
                : model.FirstProofGuidance;
            PyralisAuthoringWindowText.DrawSemanticHelpBox(guidance, MessageType.Info);
            if (currentStep != null && !string.IsNullOrWhiteSpace(currentStep.RouteName))
                PyralisAuthoringWindowPrimitives.DrawMiniField("Route", currentStep.RouteName);
            PyralisAuthoringWindowPrimitives.DrawMiniField("Next", currentStep != null && !string.IsNullOrWhiteSpace(currentStep.Label) ? currentStep.Label : model.BestNextAction);
            if (currentStep != null && currentStep.NativeAction.HasValue)
                PyralisAuthoringSurfaceBeacon.DrawNativeAction(currentStep.NativeAction.Value, currentStep.NativeAction.Value.ToGuidanceSentence());
            PyralisAuthoringWindowPrimitives.DrawMiniField("Proof Status", GetFlowTestStatus(model));
        }

        public static void DrawActionButtons(PyralisAuthoringOverviewModel model, Action openIntent, Action openGuide, Action openMap)
        {
            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open Intent"))
                {
                    openIntent?.Invoke();
                }

                if (GUILayout.Button("Open Guide"))
                {
                    openGuide?.Invoke();
                }

                if (GUILayout.Button("Open Map"))
                {
                    openMap?.Invoke();
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
                PyralisAuthoringWindowPrimitives.DrawMiniField("Route Chain", model.FirstProofChainSummary);
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
                int visibleCount = Mathf.Min(issueCount, 3);
                for (int i = 0; i < visibleCount; i++)
                    DrawOverviewIssueCard(issues[i]);
                EditorGUI.indentLevel--;

                if (issueCount > visibleCount)
                    EditorGUILayout.LabelField($"{issueCount - visibleCount} more item(s) are in Guide.", EditorStyles.wordWrappedMiniLabel);
            }
        }

        private static void DrawOverviewIssueCard(PyralisAuthoringOverviewIssue issue)
        {
            if (issue == null)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(issue.Label, GetEvidenceLabel(issue.EvidenceState), EditorStyles.boldLabel);
                PyralisAuthoringWindowPrimitives.DrawMiniField("Why It Matters", issue.WorkIntentLabel);
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

        private static string GetEvidenceLabel(PyralisAuthoringGraphEvidenceState evidenceState)
        {
            return evidenceState switch
            {
                PyralisAuthoringGraphEvidenceState.Ready => "Ready",
                PyralisAuthoringGraphEvidenceState.Optional => "Optional",
                PyralisAuthoringGraphEvidenceState.Missing => "Missing",
                PyralisAuthoringGraphEvidenceState.CandidateDetected => "Recommended",
                PyralisAuthoringGraphEvidenceState.Blocked => "Blocked",
                _ => "Unknown"
            };
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
                case "Optional Features":
                    return "No optional feature work is competing with this proof.";
                default:
                    return "Nothing in this lane.";
            }
        }

    }
}
