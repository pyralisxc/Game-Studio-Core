using Pys.Authoring.Editor.Exports;
using Pys.Authoring.Editor.Projections;
using UnityEditor;
using UnityEngine;

namespace Pys.Authoring.Editor.Window
{
    public sealed partial class PysAuthoringWindow
    {
        private void DrawGuide()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Guide", EditorStyles.boldLabel);
            if (lastGuide == null)
            {
                EditorGUILayout.HelpBox("Open Settings and scan a scripts folder.", MessageType.Info);
                return;
            }

            DrawCompactRow("Selected Intent", lastGuide.SelectedDisplayName);
            DrawWrappedRow("Readiness Target", lastGuide.ProofTarget);
            DrawCompactRow("Ready", lastGuide.ProofReady ? "Yes" : "No");
            guideShowBlockingOnly = EditorGUILayout.Toggle("Show Blocking Rows Only", guideShowBlockingOnly);
            GuideProjection renderedGuide = RenderedGuideProjection();
            string currentGroup = string.Empty;
            for (int i = 0; i < renderedGuide.Rows.Count; i++)
            {
                GuideRow row = renderedGuide.Rows[i];
                string nextGroup = GuideGroupLabel(row);
                if (nextGroup != currentGroup)
                {
                    currentGroup = nextGroup;
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(currentGroup, EditorStyles.boldLabel);
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField(row.Order + ". " + row.Title, EditorStyles.boldLabel);
                DrawInlineCounts(
                    "Role: " + row.Role,
                    "State: " + (row.BlocksProof ? "Blocking" : "Evidence"),
                    "Action: " + row.ActionLabel);

                DrawWrappedRow("Detail", row.Detail);
                DrawWrappedRow("Native Action", row.NativeAction);
                DrawWrappedRow("Success", row.SuccessCheck);
                DrawCompactRow("StableId", row.StableId);
                DrawCompactRow("Owner", row.OwnerId);
            }

            if (GUILayout.Button("Export Guide JSON"))
                ProjectionJsonExporter.ExportGuide(renderedGuide, scriptsRoot);
        }

        private static string GuideGroupLabel(GuideRow row)
        {
            if (row == null)
                return "Ungrouped";

            string stage = string.IsNullOrWhiteSpace(row.RouteStage) ? "Route" : row.RouteStage;
            string domain = string.IsNullOrWhiteSpace(row.SetupDomain) ? "General" : row.SetupDomain;
            return stage + " / " + domain;
        }

        private GuideProjection RenderedGuideProjection()
        {
            if (lastGuide == null || !guideShowBlockingOnly)
                return lastGuide;

            GuideProjection projection = new GuideProjection
            {
                SelectedContractId = lastGuide.SelectedContractId,
                SelectedDisplayName = lastGuide.SelectedDisplayName,
                ProofTarget = lastGuide.ProofTarget,
                ProofReady = lastGuide.ProofReady
            };

            for (int i = 0; i < lastGuide.Rows.Count; i++)
            {
                if (lastGuide.Rows[i].BlocksProof)
                    projection.Rows.Add(lastGuide.Rows[i]);
            }

            return projection;
        }
    }
}
