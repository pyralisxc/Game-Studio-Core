using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisAuthoringValidateRenderer
    {
        public static void Draw(Object activeSetup, PyralisAuthoringSetupGraph graph)
        {
            EditorGUILayout.LabelField("Validate Active Setup", EditorStyles.boldLabel);

            if (activeSetup == null)
            {
                EditorGUILayout.HelpBox("Select a Bootstrap, Session, Game Mode, Setup Profile, Participant, Pawn, Runtime Pattern, or Feature Module asset to validate it here.", MessageType.Info);
                return;
            }

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

            IReadOnlyList<PyralisAuthoringValidationGraphSection> sections = PyralisAuthoringSetupGraphProjection.BuildValidationSections(graph);
            if (!HasRows(sections))
                return false;

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Resolved Graph Readiness", EditorStyles.boldLabel);
            for (int i = 0; i < sections.Count; i++)
            {
                PyralisAuthoringValidationGraphSection section = sections[i];
                if (section == null || !section.HasRows)
                    continue;

                DrawReadinessBucket(section.Label, section.Rows, GetMessageType(section.EvidenceState));
            }

            return true;
        }

        private static void DrawGraphEvidenceDetails(PyralisAuthoringSetupGraph graph)
        {
            IReadOnlyList<PyralisAuthoringValidationGraphRow> rows = PyralisAuthoringSetupGraphProjection.BuildValidationDetailRows(graph);

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

        private static bool HasRows(IReadOnlyList<PyralisAuthoringValidationGraphSection> sections)
        {
            if (sections == null)
                return false;

            for (int i = 0; i < sections.Count; i++)
            {
                if (sections[i] != null && sections[i].HasRows)
                    return true;
            }

            return false;
        }

        private static MessageType GetMessageType(PyralisAuthoringGraphEvidenceState evidenceState)
        {
            switch (evidenceState)
            {
                case PyralisAuthoringGraphEvidenceState.Blocked:
                    return MessageType.Error;
                case PyralisAuthoringGraphEvidenceState.Missing:
                    return MessageType.Warning;
                default:
                    return MessageType.Info;
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
