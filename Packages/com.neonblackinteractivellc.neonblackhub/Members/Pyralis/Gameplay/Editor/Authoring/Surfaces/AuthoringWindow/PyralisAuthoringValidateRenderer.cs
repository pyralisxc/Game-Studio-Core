using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisAuthoringValidateRenderer
    {
        public static void Draw(Object activeSetup)
        {
            EditorGUILayout.LabelField("Validate Active Setup", EditorStyles.boldLabel);

            if (activeSetup == null)
            {
                EditorGUILayout.HelpBox("Select a Bootstrap, Session, Game Mode, Setup Profile, Participant, Pawn, Runtime Pattern, or Feature Module asset to validate it here.", MessageType.Info);
                return;
            }

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(activeSetup);
            PyralisAuthoringCurrentStepGraphRow currentStep = PyralisAuthoringSetupGraphProjection.BuildCurrentStepRow(graph);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Active Setup", activeSetup.name);
                EditorGUILayout.LabelField("Route", currentStep.RouteName);
                EditorGUILayout.LabelField("Next Step", currentStep.Message, EditorStyles.wordWrappedLabel);
            }

            bool hasGraphReadiness = DrawValidateReadinessBuckets(graph);
            if (!hasGraphReadiness)
            {
                EditorGUILayout.HelpBox("No validation issues found for the selected item.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Graph Evidence Details", EditorStyles.boldLabel);
            DrawGraphEvidenceDetails(graph);
        }

        private static bool DrawValidateReadinessBuckets(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return false;

            IReadOnlyList<PyralisAuthoringValidationGraphRow> required = PyralisAuthoringSetupGraphProjection.BuildValidationRows(graph, PyralisAuthoringGraphEvidenceState.Blocked);
            IReadOnlyList<PyralisAuthoringValidationGraphRow> recommended = PyralisAuthoringSetupGraphProjection.BuildValidationRows(graph, PyralisAuthoringGraphEvidenceState.Missing);
            IReadOnlyList<PyralisAuthoringValidationGraphRow> enhancers = PyralisAuthoringSetupGraphProjection.BuildValidationRows(graph, PyralisAuthoringGraphEvidenceState.CandidateDetected);
            if (required.Count == 0 && recommended.Count == 0 && enhancers.Count == 0)
                return false;

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Resolved Graph Readiness", EditorStyles.boldLabel);
            DrawReadinessBucket("Required Before Play", required, MessageType.Error);
            DrawReadinessBucket("Recommended Before Play", recommended, MessageType.Warning);
            DrawReadinessBucket("Proof Enhancers", enhancers, MessageType.Info);
            return true;
        }

        private static void DrawGraphEvidenceDetails(PyralisAuthoringSetupGraph graph)
        {
            List<PyralisAuthoringValidationGraphRow> rows = new List<PyralisAuthoringValidationGraphRow>();
            rows.AddRange(PyralisAuthoringSetupGraphProjection.BuildValidationRows(graph, PyralisAuthoringGraphEvidenceState.Blocked));
            rows.AddRange(PyralisAuthoringSetupGraphProjection.BuildValidationRows(graph, PyralisAuthoringGraphEvidenceState.Missing));
            rows.AddRange(PyralisAuthoringSetupGraphProjection.BuildValidationRows(graph, PyralisAuthoringGraphEvidenceState.CandidateDetected));

            string currentGroup = string.Empty;
            for (int i = 0; i < rows.Count; i++)
            {
                PyralisAuthoringValidationGraphRow row = rows[i];
                if (row == null)
                    continue;

                string group = string.IsNullOrWhiteSpace(row.SourceLabel) ? "Graph Evidence" : row.SourceLabel;
                if (!string.Equals(group, currentGroup, System.StringComparison.Ordinal))
                {
                    EditorGUILayout.Space(4f);
                    EditorGUILayout.LabelField(group, EditorStyles.miniBoldLabel);
                    currentGroup = group;
                }

                DrawGraphEvidenceCard(row);
            }
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
                    ? $"{issue.Label}: {issue.Message}\nGraph node: {issue.NodeId}\nSource: {issue.SourceLabel}\nOrigin: {issue.OriginLabel}"
                    : $"{issue.Label}: {issue.Message}\nGraph node: {issue.NodeId}\nSource: {issue.SourceLabel}\nOrigin: {issue.OriginLabel}\nNext native action: {issue.NativeAction}";
                EditorGUILayout.HelpBox(text, messageType);
                if (issue.CanInspectTarget && GUILayout.Button("Inspect " + issue.Label))
                    PyralisAuthoringWindowPrimitives.SelectAndPing(issue.Target);
            }

            if (issues.Count > visible)
                EditorGUILayout.LabelField("+" + (issues.Count - visible) + " more readiness item(s)", EditorStyles.miniLabel);
        }

        private static void DrawGraphEvidenceCard(PyralisAuthoringValidationGraphRow issue)
        {
            if (issue == null)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(issue.Label, issue.NodeId, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Evidence", issue.EvidenceState.ToString(), EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.LabelField("Origin", issue.OriginLabel, EditorStyles.wordWrappedMiniLabel);
                if (!string.IsNullOrWhiteSpace(issue.Message))
                    EditorGUILayout.LabelField("Problem", issue.Message, EditorStyles.wordWrappedLabel);
                if (!string.IsNullOrWhiteSpace(issue.NativeAction))
                    EditorGUILayout.LabelField("Native Unity Action", issue.NativeAction, EditorStyles.wordWrappedMiniLabel);
                if (issue.Node != null && issue.Node.AssignmentFields.Length > 0)
                    PyralisAuthoringWindowPrimitives.DrawMiniList("Fields", issue.Node.AssignmentFields);
                if (issue.Node != null && issue.Node.CustomizationMoments.Length > 0)
                    PyralisAuthoringWindowPrimitives.DrawMiniList("Customization", issue.Node.CustomizationMoments);
                if (!string.IsNullOrWhiteSpace(issue.Node?.BlockingReason))
                    EditorGUILayout.LabelField("Blocking Reason", issue.Node.BlockingReason, EditorStyles.wordWrappedMiniLabel);

                if (issue.CanInspectTarget && GUILayout.Button("Inspect Target"))
                    PyralisAuthoringWindowPrimitives.SelectAndPing(issue.Target);
            }
        }
    }
}
