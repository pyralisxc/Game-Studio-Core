using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisAuthoringMapRenderer
    {
        private static readonly Dictionary<string, bool> Foldouts = new Dictionary<string, bool>();

        public static void Draw(Object activeSetup, Object selection, PyralisAuthoringSetupGraph graph)
        {
            EditorGUILayout.LabelField("Setup Map", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Use Map for scene and setup reality: selected context, authored links, scene surfaces, missing fields, and object wiring. Intent does not change this view; Hygiene owns graph integrity and developer audits.", MessageType.Info);
            PyralisAuthoringGraphJsonExportControl.Draw("Map", graph);
            DrawActiveAndSelectedContext(activeSetup, selection);
            DrawYouAreHereChain(graph);
            DrawSceneSurfaceSnapshot(graph);
            DrawSceneSetupIssues(graph);
            DrawGraphConnections(graph);
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
            EditorGUILayout.LabelField("Developer Route Connections", EditorStyles.boldLabel);
            PyralisAuthoringWindowText.DrawSemanticHelpBox("Collapsed reference view for how current scene/setup nodes connect. Open Hygiene when graph blockers need deeper evidence.", MessageType.Info);

            IReadOnlyList<PyralisAuthoringGraphConnectionRow> rows = PyralisAuthoringSetupGraphProjection.BuildMapConnectionRows(graph);
            const string key = "Pyralis.AuthoringWindow.Map.RouteConnections";
            bool expanded = Foldouts.TryGetValue(key, out bool value) && value;
            expanded = EditorGUILayout.Foldout(expanded, $"Connections ({rows.Count})", true);
            Foldouts[key] = expanded;
            if (!expanded)
            {
                EditorGUILayout.LabelField("Collapsed by default so Map stays focused on the current scene and setup objects.", EditorStyles.wordWrappedMiniLabel);
                return;
            }

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

        private static void DrawSceneSetupIssues(PyralisAuthoringSetupGraph graph)
        {
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Scene Setup Issues", EditorStyles.boldLabel);
            PyralisAuthoringWindowText.DrawSemanticHelpBox("These are concrete Unity setup items: missing scene surfaces, empty fields, component requirements, or selected-route wiring that should be fixed in Project, Hierarchy, or Inspector.", MessageType.Info);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                IReadOnlyList<PyralisAuthoringGraphAuditRow> rows = PyralisAuthoringSetupGraphProjection.BuildMapSceneSetupIssueRows(graph);
                int visibleCount = 0;
                for (int i = 0; i < rows.Count; i++)
                {
                    PyralisAuthoringGraphAuditRow row = rows[i];
                    DrawSceneSetupIssueRow(row);
                    visibleCount++;
                }

                if (visibleCount == 0)
                    EditorGUILayout.LabelField("No current scene/setup issues are exposed by the graph.", EditorStyles.wordWrappedMiniLabel);
            }
        }

        private static void DrawSceneSetupIssueRow(PyralisAuthoringGraphAuditRow row)
        {
            PyralisAuthoringGraphNode node = row.Node;
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(row.Label, GetEvidenceLabel(row.EvidenceState), EditorStyles.boldLabel);
                if (!string.IsNullOrWhiteSpace(row.Message))
                    PyralisAuthoringWindowText.DrawSemanticMiniLabel(row.Message);
                if (node.NativeAction.HasValue)
                    PyralisAuthoringSurfaceBeacon.DrawNativeAction(node.NativeAction.Value, node.NativeAction.Value.ToGuidanceSentence());
                if (node.AssignmentFields.Length > 0)
                    PyralisAuthoringWindowPrimitives.DrawMiniList("Field or component", node.AssignmentFields);
                if (node.NativeSetup.Length > 0)
                    PyralisAuthoringWindowPrimitives.DrawMiniList("Unity setup", node.NativeSetup);
                if (!string.IsNullOrWhiteSpace(node.BlockingReason))
                    PyralisAuthoringWindowPrimitives.DrawMiniField("Why", node.BlockingReason);

                using (new EditorGUI.DisabledScope(!row.CanInspectTarget))
                {
                    if (GUILayout.Button("Inspect Target"))
                        PyralisAuthoringWindowPrimitives.SelectAndPing(row.Target);
                }
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
