using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisAuthoringMapRenderer
    {
        public static void Draw(Object activeSetup, Object selection, PyralisAuthoringSetupGraph graph)
        {
            EditorGUILayout.LabelField("Setup Map", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Use this page to see the route Pyralis inferred from your Intent, assets, scene objects, and contracts. Edit actual fields in the Inspector when a row names a missing link.", MessageType.Info);
            DrawActiveAndSelectedContext(activeSetup, selection);
            DrawIntentFocus(graph);
            DrawYouAreHereChain(graph);
            DrawFirstProofBlockers(graph);
            DrawGraphConnections(graph);
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

        private static void DrawIntentFocus(PyralisAuthoringSetupGraph graph)
        {
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Intent Focus", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                PyralisAuthoringWindowPrimitives.DrawMiniField("Route Focus", PyralisAuthoringSetupGraphProjection.BuildIntentFocusSummary(graph));
                PyralisAuthoringWindowPrimitives.DrawMiniField("First Proof", PyralisAuthoringSetupGraphProjection.BuildFirstProofPrioritySummary(graph));
            }
        }

        private static void DrawYouAreHereChain(PyralisAuthoringSetupGraph graph)
        {
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Setup Chain", EditorStyles.boldLabel);

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

        private static void DrawFirstProofBlockers(PyralisAuthoringSetupGraph graph)
        {
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> rows = PyralisAuthoringSetupGraphProjection.BuildProofBlockerRows(graph);
            if (rows.Count == 0)
                return;

            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Fix Before First Proof", EditorStyles.boldLabel);
            PyralisAuthoringWindowText.DrawSemanticHelpBox("These rows explain why an assigned definition asset is not enough yet. Fill the named field, prefab, component, or scene surface before treating the proof as playable.", MessageType.Warning);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                int visibleCount = Mathf.Min(rows.Count, 8);
                for (int i = 0; i < visibleCount; i++)
                    DrawGraphConnectionRow(rows[i]);

                if (visibleCount < rows.Count)
                    EditorGUILayout.LabelField($"{rows.Count - visibleCount} more proof blocker(s) are visible in Validate.", EditorStyles.wordWrappedMiniLabel);
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

        private static void DrawGraphConnections(PyralisAuthoringSetupGraph graph)
        {
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Route Connections", EditorStyles.boldLabel);
            PyralisAuthoringWindowText.DrawSemanticHelpBox("These rows show how setup, capabilities, contracts, proof targets, and scene evidence connect. Use them when the route feels unclear.", MessageType.Info);

            IReadOnlyList<PyralisAuthoringGraphConnectionRow> rows = PyralisAuthoringSetupGraphProjection.BuildMapConnectionRows(graph);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (rows.Count == 0)
                {
                    EditorGUILayout.LabelField("No route connections were resolved yet.", EditorStyles.wordWrappedMiniLabel);
                    return;
                }

                int visibleCount = Mathf.Min(rows.Count, 32);
                for (int i = 0; i < visibleCount; i++)
                    DrawGraphConnectionRow(rows[i]);

                if (visibleCount < rows.Count)
                    EditorGUILayout.LabelField($"{rows.Count - visibleCount} more reflected connections are hidden here. Use Facts when you need the full cookbook audit.", EditorStyles.wordWrappedMiniLabel);
            }
        }

        private static void DrawGraphConnectionRow(PyralisAuthoringGraphConnectionRow row)
        {
            if (row == null)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField($"{row.FromLabel} -> {row.ToLabel}", EditorStyles.miniBoldLabel);
                EditorGUI.indentLevel++;
                PyralisAuthoringWindowPrimitives.DrawMiniField("Relationship", row.Relationship);
                if (!string.IsNullOrWhiteSpace(row.Detail))
                    PyralisAuthoringWindowPrimitives.DrawMiniField("Meaning", row.Detail);
                EditorGUI.indentLevel--;
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
