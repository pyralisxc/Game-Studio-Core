using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisAuthoringValidateRenderer
    {
        public static void Draw(
            Object activeSetup,
            PyralisAuthoringRouteReport report,
            Func<PyralisAuthoringValidationIssue, bool> runGuidanceAction)
        {
            EditorGUILayout.LabelField("Validate Active Setup", EditorStyles.boldLabel);

            if (activeSetup == null)
            {
                EditorGUILayout.HelpBox("Select a Bootstrap, Session, Game Mode, Setup Profile, Participant, Pawn, Runtime Pattern, or Feature Module asset to validate it here.", MessageType.Info);
                return;
            }

            PyralisAuthoringValidationModel model = PyralisAuthoringValidationModel.Build(activeSetup, report);
            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(activeSetup);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Active Setup", activeSetup.name);
                EditorGUILayout.LabelField("Route", model.RouteName);
                EditorGUILayout.LabelField("Next Step", model.NextStep, EditorStyles.wordWrappedLabel);
                DrawValidateReadinessBuckets(graph);
            }

            if (!model.HasIssues)
            {
                EditorGUILayout.HelpBox("No validation issues found for the selected item.", MessageType.Info);
                return;
            }

            DrawValidationIssueGroup(PyralisAuthoringValidationCategory.SessionSetup, model.Issues, runGuidanceAction);
            DrawValidationIssueGroup(PyralisAuthoringValidationCategory.GameRules, model.Issues, runGuidanceAction);
            DrawValidationIssueGroup(PyralisAuthoringValidationCategory.SetupProfile, model.Issues, runGuidanceAction);
            DrawValidationIssueGroup(PyralisAuthoringValidationCategory.PlayersSeats, model.Issues, runGuidanceAction);
            DrawValidationIssueGroup(PyralisAuthoringValidationCategory.PawnsActors, model.Issues, runGuidanceAction);
            DrawValidationIssueGroup(PyralisAuthoringValidationCategory.SceneObjects, model.Issues, runGuidanceAction);
            DrawValidationIssueGroup(PyralisAuthoringValidationCategory.CodeContract, model.Issues, runGuidanceAction);
            DrawValidationIssueGroup(PyralisAuthoringValidationCategory.Other, model.Issues, runGuidanceAction);
        }

        private static void DrawValidateReadinessBuckets(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return;

            IReadOnlyList<PyralisAuthoringValidationGraphRow> required = PyralisAuthoringSetupGraphProjection.BuildValidationRows(graph, PyralisAuthoringGraphEvidenceState.Blocked);
            IReadOnlyList<PyralisAuthoringValidationGraphRow> recommended = PyralisAuthoringSetupGraphProjection.BuildValidationRows(graph, PyralisAuthoringGraphEvidenceState.Missing);
            IReadOnlyList<PyralisAuthoringValidationGraphRow> enhancers = PyralisAuthoringSetupGraphProjection.BuildValidationRows(graph, PyralisAuthoringGraphEvidenceState.CandidateDetected);
            if (required.Count == 0 && recommended.Count == 0 && enhancers.Count == 0)
                return;

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Graph Readiness Evidence", EditorStyles.miniBoldLabel);
            DrawReadinessBucket("Required Before Play", required, MessageType.Error);
            DrawReadinessBucket("Recommended Before Play", recommended, MessageType.Warning);
            DrawReadinessBucket("Proof Enhancers", enhancers, MessageType.Info);
        }

        private static void DrawReadinessBucket(
            string label,
            IReadOnlyList<PyralisAuthoringValidationGraphRow> issues,
            MessageType messageType)
        {
            if (issues == null || issues.Count == 0)
                return;

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
            int visible = Mathf.Min(issues.Count, 4);
            for (int i = 0; i < visible; i++)
            {
                PyralisAuthoringValidationGraphRow issue = issues[i];
                if (issue == null)
                    continue;

                string text = string.IsNullOrWhiteSpace(issue.NativeAction)
                    ? $"{issue.Message}\nGraph node: {issue.NodeId}"
                    : $"{issue.Message}\nGraph node: {issue.NodeId}\nNext native action: {issue.NativeAction}";
                EditorGUILayout.HelpBox(text, messageType);
            }

            if (issues.Count > visible)
                EditorGUILayout.LabelField("+" + (issues.Count - visible) + " more readiness item(s)", EditorStyles.miniLabel);
        }

        private static void DrawValidationIssueGroup(
            PyralisAuthoringValidationCategory category,
            IReadOnlyList<PyralisAuthoringValidationIssue> issues,
            Func<PyralisAuthoringValidationIssue, bool> runGuidanceAction)
        {
            bool drewAny = false;

            for (int i = 0; i < issues.Count; i++)
            {
                PyralisAuthoringValidationIssue issue = issues[i];
                if (issue == null || issue.Category != category)
                    continue;

                if (!drewAny)
                {
                    EditorGUILayout.Space(4f);
                    EditorGUILayout.LabelField(PyralisAuthoringValidationModel.GetCategoryTitle(category), EditorStyles.miniBoldLabel);
                    drewAny = true;
                }

                DrawValidationIssueCard(issue, runGuidanceAction);
            }
        }

        private static void DrawValidationIssueCard(
            PyralisAuthoringValidationIssue issue,
            Func<PyralisAuthoringValidationIssue, bool> runGuidanceAction)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Issue Code", issue.IssueCode, EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.LabelField("Problem", issue.Problem, EditorStyles.wordWrappedLabel);
                EditorGUILayout.LabelField("Affected Field", issue.AffectedMember, EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.LabelField("Why It Matters", issue.WhyItMatters, EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.LabelField("Inspect Next", issue.InspectionHint, EditorStyles.wordWrappedMiniLabel);
                DrawValidationIssueTypedMetadata(issue);
                DrawValidationIssueEvidence(issue);

                if (issue.HasGuidanceAction && GUILayout.Button(issue.GuidanceActionLabel))
                    runGuidanceAction?.Invoke(issue);

                if (issue.CanInspectTarget && GUILayout.Button(issue.PrimaryActionLabel))
                    PyralisAuthoringWindowPrimitives.SelectAndPing(issue.Target);
            }
        }

        private static void DrawValidationIssueTypedMetadata(PyralisAuthoringValidationIssue issue)
        {
            PyralisAuthoringIssue typedIssue = issue?.TypedIssue;
            if (typedIssue == null)
                return;

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("Typed Issue", EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Severity", typedIssue.Severity.ToString(), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("Work Intent", typedIssue.WorkIntent, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("Evidence", PyralisAuthoringLabelUtility.GetEvidenceLabel(typedIssue.EvidenceState), EditorStyles.wordWrappedMiniLabel);
            if (!string.IsNullOrWhiteSpace(typedIssue.FieldOrComponent))
                EditorGUILayout.LabelField("Field / Component", typedIssue.FieldOrComponent, EditorStyles.wordWrappedMiniLabel);
            if (typedIssue.NativeAction.HasValue)
            {
                EditorGUILayout.LabelField("Native Unity Action", EditorStyles.miniBoldLabel);
                PyralisAuthoringSurfaceBeacon.DrawNativeAction(typedIssue.NativeAction.Value, typedIssue.NativeAction.Value.ToGuidanceSentence());
            }
            EditorGUI.indentLevel--;
        }

        private static void DrawValidationIssueEvidence(PyralisAuthoringValidationIssue issue)
        {
            if (issue == null || !issue.HasAuditEvidence)
                return;

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("Audit Evidence", EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;
            if (!string.IsNullOrWhiteSpace(issue.Expected))
                EditorGUILayout.LabelField("Expected", issue.Expected, EditorStyles.wordWrappedMiniLabel);
            if (!string.IsNullOrWhiteSpace(issue.Found))
                EditorGUILayout.LabelField("Found", issue.Found, EditorStyles.wordWrappedMiniLabel);
            if (!string.IsNullOrWhiteSpace(issue.SuccessLooksLike))
                EditorGUILayout.LabelField("Success Looks Like", issue.SuccessLooksLike, EditorStyles.wordWrappedMiniLabel);
            EditorGUI.indentLevel--;
        }
    }
}
