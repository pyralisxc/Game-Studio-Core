using Pys.Authoring.Editor.Exports;
using Pys.Authoring.Editor.Hygiene;
using UnityEditor;
using UnityEngine;

namespace Pys.Authoring.Editor.Window
{
    public sealed partial class PysAuthoringWindow
    {
        private void DrawHygiene()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Hygiene", EditorStyles.boldLabel);

            DrawInlineCounts(
                "Types: " + observedTypeCount,
                "Asmdefs: " + observedAssemblyCount,
                "Files: " + observedSourceFileCount,
                "Nodes: " + observedNodeCount,
                "Edges: " + observedEdgeCount,
                "Review: " + hygieneReviewCount,
                "Warnings: " + hygieneWarningCount,
                "Errors: " + hygieneErrorCount);

            if (lastHygiene != null && lastHygiene.Lenses.Count > 0)
            {
                if (activeHygieneLensIndex < 0 || activeHygieneLensIndex >= lastHygiene.Lenses.Count)
                    activeHygieneLensIndex = 0;

                string[] lensLabels = new string[lastHygiene.Lenses.Count];
                for (int i = 0; i < lastHygiene.Lenses.Count; i++)
                    lensLabels[i] = lastHygiene.Lenses[i].Title;

                activeHygieneLensIndex = EditorGUIUtility.currentViewWidth < 760f
                    ? EditorGUILayout.Popup("Lens", activeHygieneLensIndex, lensLabels)
                    : GUILayout.Toolbar(activeHygieneLensIndex, lensLabels);
                HygieneLensProjection lens = lastHygiene.Lenses[activeHygieneLensIndex];
                EditorGUILayout.Space();
                hygieneSeverityFilterIndex = EditorGUILayout.Popup("Severity", hygieneSeverityFilterIndex, BuildHygieneSeverityFilterOptions());
                EditorGUILayout.LabelField(lens.Question, EditorStyles.wordWrappedLabel);
                DrawInlineCounts(
                    "Rows: " + lens.Rows.Count,
                    "Review: " + lens.ReviewCount,
                    "Warnings: " + lens.WarningCount,
                    "Errors: " + lens.ErrorCount);

                HygieneProjection renderedHygiene = RenderedHygieneProjection();
                DrawCompactRow("Rendered Rows", renderedHygiene.Rows.Count.ToString());
                HygieneSeverity? currentSeverity = null;
                for (int i = 0; i < renderedHygiene.Rows.Count; i++)
                {
                    HygieneRow row = renderedHygiene.Rows[i];
                    if (currentSeverity == null || currentSeverity.Value != row.Severity)
                    {
                        currentSeverity = row.Severity;
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField(row.Severity.ToString(), EditorStyles.boldLabel);
                    }

                    DrawHygieneRow(row);
                }
            }

            using (new EditorGUI.DisabledScope(lastGraph == null))
            {
                if (GUILayout.Button("Export Graph JSON"))
                    AuthoringGraphJsonExporter.Export(lastGraph, scriptsRoot);
            }

            using (new EditorGUI.DisabledScope(lastHygiene == null))
            {
                if (GUILayout.Button("Export Hygiene JSON"))
                    HygieneJsonExporter.Export(RenderedHygieneProjection(), scriptsRoot);
            }
        }

        private static void DrawHygieneRow(HygieneRow row)
        {
            if (row == null)
                return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(row.Title, EditorStyles.boldLabel);
            DrawInlineCounts(
                "Lens: " + row.Lens,
                "Issue: " + row.IssueCode,
                "Severity: " + row.Severity,
                "Owner: " + row.OwnerId,
                "Navigate: " + (row.CanNavigate ? "Yes" : "No"));
            DrawWrappedRow("Detail", row.Detail);
            DrawCompactRow("Source Kind", row.SourceKind);
            DrawWrappedRow("Source Path", row.SourcePath);
            DrawOptionalLabel("Evidence IDs", row.EvidenceIds);
            DrawWrappedRow("Claim", row.Claim);
            DrawWrappedRow("Evidence", row.Evidence);
            DrawWrappedRow("Recommendation", row.Recommendation);
            DrawOptionalLabel("Confidence", row.Confidence);
        }

        private static string[] BuildHygieneSeverityFilterOptions()
        {
            return new[] { "All", "Review+", "Warning+", "Errors Only", "Info Only" };
        }

        private bool IncludeHygieneRow(HygieneRow row)
        {
            if (row == null)
                return false;

            switch (hygieneSeverityFilterIndex)
            {
                case 1:
                    return row.Severity == HygieneSeverity.Review || row.Severity == HygieneSeverity.Warning || row.Severity == HygieneSeverity.Error;
                case 2:
                    return row.Severity == HygieneSeverity.Warning || row.Severity == HygieneSeverity.Error;
                case 3:
                    return row.Severity == HygieneSeverity.Error;
                case 4:
                    return row.Severity == HygieneSeverity.Info;
                default:
                    return true;
            }
        }

        private static void CountHygieneRow(HygieneProjection projection, HygieneLensProjection lens, HygieneRow row)
        {
            switch (row.Severity)
            {
                case HygieneSeverity.Review:
                    projection.ReviewCount++;
                    lens.ReviewCount++;
                    break;
                case HygieneSeverity.Warning:
                    projection.WarningCount++;
                    lens.WarningCount++;
                    break;
                case HygieneSeverity.Error:
                    projection.ErrorCount++;
                    lens.ErrorCount++;
                    break;
            }
        }

        private HygieneProjection RenderedHygieneProjection()
        {
            if (lastHygiene == null)
                return null;

            HygieneProjection projection = new HygieneProjection
            {
                ReviewCount = 0,
                WarningCount = 0,
                ErrorCount = 0
            };

            if (lastHygiene.Lenses.Count == 0)
                return projection;

            if (activeHygieneLensIndex < 0 || activeHygieneLensIndex >= lastHygiene.Lenses.Count)
                activeHygieneLensIndex = 0;

            HygieneLensProjection sourceLens = lastHygiene.Lenses[activeHygieneLensIndex];
            HygieneLensProjection lens = new HygieneLensProjection(sourceLens.Kind, sourceLens.Title, sourceLens.Question);
            projection.Lenses.Add(lens);

            for (int i = 0; i < sourceLens.Rows.Count; i++)
            {
                HygieneRow row = sourceLens.Rows[i];
                if (!IncludeHygieneRow(row))
                    continue;

                projection.Rows.Add(row);
                lens.Rows.Add(row);
                CountHygieneRow(projection, lens, row);
            }

            return projection;
        }
    }
}
