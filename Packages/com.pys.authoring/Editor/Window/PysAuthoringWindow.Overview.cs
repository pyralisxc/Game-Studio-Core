using Pys.Authoring.Editor.Exports;
using Pys.Authoring.Editor.Projections;
using UnityEditor;
using UnityEngine;

namespace Pys.Authoring.Editor.Window
{
    public sealed partial class PysAuthoringWindow
    {
        private void DrawOverview()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Overview", EditorStyles.boldLabel);
            if (lastOverview == null)
            {
                EditorGUILayout.HelpBox("Open Settings and scan a scripts folder.", MessageType.Info);
                return;
            }

            DrawWrappedRow("Summary", lastOverview.Summary);
            DrawCompactRow("Selected Intent", lastOverview.SelectedIntent);
            DrawCompactRow("Readiness", lastOverview.Readiness);
            DrawWrappedRow("Readiness Target", lastOverview.ProofTarget);
            DrawWrappedRow("Next", lastOverview.NextAction);
            DrawWrappedRow("Reason", lastOverview.Reason);
            DrawCompactRow("Issues", lastOverview.IssueCount.ToString());

            if (lastOverview.NextActions.Count > 0)
            {
                DrawSectionHeader("Next Actions");
                for (int i = 0; i < lastOverview.NextActions.Count; i++)
                    DrawOverviewAction(lastOverview.NextActions[i]);
            }

            if (GUILayout.Button("Export Overview JSON"))
                ProjectionJsonExporter.ExportOverview(lastOverview, scriptsRoot);
        }

        private static void DrawOverviewAction(OverviewActionRow row)
        {
            if (row == null)
                return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(row.Order + ". " + row.Title, EditorStyles.boldLabel);
            DrawOptionalLabel("Action", row.ActionLabel);
            DrawWrappedRow("Native Action", row.NativeAction);
            DrawWrappedRow("Reason", row.Detail);
            DrawOptionalLabel("Source", row.SourceRole);
        }
    }
}
