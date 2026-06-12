using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisAuthoringMapRenderer
    {
        public static void Draw(Object activeSetup, Object selection, PyralisAuthoringRouteReport report)
        {
            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(activeSetup);

            EditorGUILayout.LabelField("Setup Map", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Use this page to understand how the active setup is connected. Edit actual fields in the Inspector when a row names a missing link.", MessageType.Info);
            DrawActiveAndSelectedContext(activeSetup, selection);
            DrawYouAreHereChain(graph);
            DrawSceneSurfaceSnapshot(graph);
            DrawReadinessSummary(graph);
        }

        private static void DrawActiveAndSelectedContext(Object activeSetup, Object selection)
        {
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Selected Authoring Context", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Active Setup", activeSetup != null ? $"{activeSetup.name} ({activeSetup.GetType().Name})" : "No setup context", EditorStyles.wordWrappedLabel);
                EditorGUILayout.LabelField("Current Selection", selection != null ? $"{selection.name} ({selection.GetType().Name})" : "Nothing selected", EditorStyles.wordWrappedLabel);
            }
        }

        private static void DrawYouAreHereChain(PyralisAuthoringSetupGraph graph)
        {
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("You Are Here", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                IReadOnlyList<PyralisAuthoringSetupGraphRow> rows = PyralisAuthoringSetupGraphProjection.BuildSetupMapRows(graph);
                for (int i = 0; i < rows.Count; i++)
                    DrawSetupChainRow(rows[i]);
            }
        }

        private static void DrawSetupChainRow(PyralisAuthoringSetupGraphRow row)
        {
            if (row == null)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                Object target = row.Target;
                string status = PyralisAuthoringWindowPrimitives.GetReadinessBadge(row.IsReady, target, row.IsOptional);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(row.Label, EditorStyles.miniBoldLabel);
                    using (new EditorGUI.DisabledScope(target == null))
                    {
                        if (GUILayout.Button("Inspect", GUILayout.Width(72f)))
                        {
                            Selection.activeObject = target;
                            EditorGUIUtility.PingObject(target);
                        }
                    }
                }

                EditorGUI.indentLevel++;
                PyralisAuthoringWindowText.DrawSemanticMiniLabel($"{status}: {row.Message}");
                EditorGUI.indentLevel--;
            }
        }

        private static void DrawSceneSurfaceSnapshot(PyralisAuthoringSetupGraph graph)
        {
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Scene Surface Scan", EditorStyles.boldLabel);
            PyralisAuthoringWindowText.DrawSemanticHelpBox("This reads ordinary Unity scene objects too. A found surface is evidence, not proof: Play Mode still owns the final route proof.", MessageType.Info);

            IReadOnlyList<PyralisAuthoringGraphNode> surfaceNodes = PyralisAuthoringSetupGraphProjection.FindSceneSurfaceNodes(graph);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                for (int i = 0; i < surfaceNodes.Count; i++)
                    DrawSceneSurfaceRow(surfaceNodes[i]);
            }
        }

        private static void DrawSceneSurfaceRow(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return;

            string status = $"[{GetEvidenceLabel(node.EvidenceState)}]";
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(node.Label, status, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                PyralisAuthoringWindowPrimitives.DrawMiniField("Evidence", node.Guidance);
                PyralisAuthoringWindowPrimitives.DrawMiniList("Next fix", node.NativeSetup);
                EditorGUI.indentLevel--;
            }
        }

        private static void DrawReadinessSummary(PyralisAuthoringSetupGraph graph)
        {
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Readiness Summary", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                IReadOnlyList<PyralisAuthoringSetupGraphRow> rows = PyralisAuthoringSetupGraphProjection.BuildReadinessRows(graph);
                for (int i = 0; i < rows.Count; i++)
                    DrawCompactReadinessRow(rows[i]);
            }
        }

        private static void DrawCompactReadinessRow(PyralisAuthoringSetupGraphRow row)
        {
            if (row == null)
                return;

            Object target = row.Target;
            string targetName = target != null ? $" ({target.name})" : string.Empty;
            EditorGUILayout.LabelField(row.Label, PyralisAuthoringWindowPrimitives.GetReadinessBadge(row.IsReady, target, row.IsOptional) + targetName);
            if (!string.IsNullOrWhiteSpace(row.Message))
            {
                EditorGUI.indentLevel++;
                PyralisAuthoringWindowText.DrawSemanticMiniLabel(row.Message);
                EditorGUI.indentLevel--;
            }
        }

        private static string GetEvidenceLabel(PyralisAuthoringGraphEvidenceState state)
        {
            return state switch
            {
                PyralisAuthoringGraphEvidenceState.Ready => "Ready",
                PyralisAuthoringGraphEvidenceState.Optional => "Not relevant",
                PyralisAuthoringGraphEvidenceState.Missing => "Missing",
                PyralisAuthoringGraphEvidenceState.CandidateDetected => "Candidate detected",
                PyralisAuthoringGraphEvidenceState.Blocked => "Blocked",
                _ => "Unknown"
            };
        }

    }
}
