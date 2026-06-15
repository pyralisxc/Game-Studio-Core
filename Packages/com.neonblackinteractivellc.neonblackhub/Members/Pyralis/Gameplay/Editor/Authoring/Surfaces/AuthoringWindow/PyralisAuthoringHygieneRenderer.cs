using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisAuthoringHygieneRenderer
    {
        private static IReadOnlyList<PyralisSourceDependencyHygieneRecord> _dependencyRecords;

        public static void Draw(Object activeSetup, PyralisAuthoringSetupGraph graph)
        {
            EditorGUILayout.LabelField("Hygiene", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Use Hygiene as the developer audit surface: graph integrity, proof blockers, dependency pressure, source origins, and stable node ids. Map owns concrete scene and Inspector setup issues.", MessageType.Info);

            if (activeSetup == null)
            {
                EditorGUILayout.HelpBox("Select a Bootstrap, Session, Game Mode, Participant, Pawn, or Feature Module asset so Hygiene can inspect its resolved setup graph.", MessageType.Info);
                DrawSourceDependencyHygiene();
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Active Setup", activeSetup.name);
                EditorGUILayout.LabelField("Route", graph != null ? graph.RouteName : "No graph");
                EditorGUILayout.LabelField("Graph Size", graph != null ? $"{graph.Nodes.Count} nodes, {graph.Edges.Count} edges" : "No graph", EditorStyles.wordWrappedLabel);
                PyralisAuthoringWindowText.DrawSemanticMiniLabel("Scene-specific repair actions are shown in Map. Hygiene keeps the diagnostic view graph-first.");
            }

            bool hasGraphAuditFindings = DrawGraphAuditBuckets(graph);
            if (!hasGraphAuditFindings)
                EditorGUILayout.HelpBox("No graph hygiene findings found for the selected item.", MessageType.Info);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Graph Audit Details", EditorStyles.boldLabel);
            DrawGraphAuditDetails(graph);

            DrawSourceDependencyHygiene();
        }

        private static bool DrawGraphAuditBuckets(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return false;

            IReadOnlyList<PyralisAuthoringGraphAuditSection> sections = PyralisAuthoringSetupGraphProjection.BuildHygieneSections(graph);
            if (!HasRows(sections))
                return false;

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Graph Audit Buckets", EditorStyles.boldLabel);
            for (int i = 0; i < sections.Count; i++)
            {
                PyralisAuthoringGraphAuditSection section = sections[i];
                if (section == null || !section.HasRows)
                    continue;

                DrawReadinessBucket(section.Label, section.Rows, GetMessageType(section.EvidenceState));
            }

            return true;
        }

        private static void DrawSourceDependencyHygiene()
        {
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Source Dependency Hygiene", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Read-only code pressure scan. It highlights scripts that touch many Pyralis domains, rely on concrete cross-domain references, or use lookup/static patterns that can hide ownership.", MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Refresh Dependency Audit", GUILayout.MaxWidth(220f)))
                    _dependencyRecords = PyralisSourceDependencyHygieneScanner.ScanPackage();
            }

            _dependencyRecords ??= PyralisSourceDependencyHygieneScanner.ScanPackage();
            if (_dependencyRecords == null || _dependencyRecords.Count == 0)
            {
                EditorGUILayout.HelpBox("No package source files found for dependency hygiene.", MessageType.Info);
                return;
            }

            int watchCount = CountRisk(PyralisSourceDependencyRisk.Watch);
            int heavyCount = CountRisk(PyralisSourceDependencyRisk.Heavy);
            int boundaryRiskCount = CountRisk(PyralisSourceDependencyRisk.BoundaryRisk);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Scanned Scripts", _dependencyRecords.Count.ToString());
                EditorGUILayout.LabelField("Watch / Heavy / Boundary Risk", $"{watchCount} / {heavyCount} / {boundaryRiskCount}");
                PyralisAuthoringWindowText.DrawSemanticMiniLabel("Use this as a heat map. A high score is not a failure; it is a prompt to check whether ownership is still obvious.");
            }

            int visible = 0;
            for (int i = 0; i < _dependencyRecords.Count && visible < 8; i++)
            {
                PyralisSourceDependencyHygieneRecord record = _dependencyRecords[i];
                if (record == null || record.Risk == PyralisSourceDependencyRisk.Low)
                    continue;

                DrawDependencyPressureCard(record);
                visible++;
            }

            if (visible == 0)
                EditorGUILayout.HelpBox("No notable dependency pressure found in the package scan.", MessageType.Info);

            int remaining = _dependencyRecords.Count(record => record != null && record.Risk != PyralisSourceDependencyRisk.Low) - visible;
            if (remaining > 0)
                EditorGUILayout.LabelField("+" + remaining + " more script(s) with dependency pressure", EditorStyles.miniLabel);
        }

        private static int CountRisk(PyralisSourceDependencyRisk risk)
        {
            if (_dependencyRecords == null)
                return 0;

            int count = 0;
            for (int i = 0; i < _dependencyRecords.Count; i++)
            {
                if (_dependencyRecords[i] != null && _dependencyRecords[i].Risk == risk)
                    count++;
            }

            return count;
        }

        private static void DrawDependencyPressureCard(PyralisSourceDependencyHygieneRecord record)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(record.FileName, $"{record.Risk} ({record.RiskScore})", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Owner Domain", record.OwnerDomain, EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.LabelField("Dependencies", $"{record.DependencyCount} total, {record.Domains.Count} domain(s), {record.ConcreteCrossDomainCount} concrete cross-domain", EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.LabelField("Unity Lookup / Static / Reflection", $"{record.UnityLookupCount} / {record.StaticAccessCount} / {record.ReflectionOrStringLookupCount}", EditorStyles.wordWrappedMiniLabel);
                if (record.Domains.Count > 0)
                    PyralisAuthoringWindowPrimitives.DrawMiniList("Domains", record.Domains, "Pyralis domains inferred from path and source namespaces.", 6);
                PyralisAuthoringWindowPrimitives.DrawMiniList("Pressure Reasons", record.Reasons, "Why this script is showing up in the hygiene scan.", 4);
                if (!string.IsNullOrWhiteSpace(record.AssetPath))
                    EditorGUILayout.LabelField("Source", record.AssetPath, EditorStyles.wordWrappedMiniLabel);
            }
        }

        private static void DrawGraphAuditDetails(PyralisAuthoringSetupGraph graph)
        {
            IReadOnlyList<PyralisAuthoringGraphAuditRow> rows = PyralisAuthoringSetupGraphProjection.BuildHygieneDetailRows(graph);
            if (rows == null || rows.Count == 0)
            {
                EditorGUILayout.HelpBox("Hygiene did not find unvalidated graph nodes, explicit runtime/scene findings, or proof blocker links. Use Map for scene setup repair.", MessageType.Info);
                return;
            }

            string currentGroup = string.Empty;
            for (int i = 0; i < rows.Count; i++)
            {
                PyralisAuthoringGraphAuditRow row = rows[i];
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

        private static bool HasRows(IReadOnlyList<PyralisAuthoringGraphAuditSection> sections)
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

        private static MessageType GetMessageType(PyralisAuthoringGraphEvidenceState evidenceState) =>
            evidenceState == PyralisAuthoringGraphEvidenceState.Unknown
                ? MessageType.Info
                : MessageType.Warning;

        private static void DrawReadinessBucket(
            string label,
            IReadOnlyList<PyralisAuthoringGraphAuditRow> issues,
            MessageType messageType)
        {
            if (issues == null || issues.Count == 0)
                return;

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
            int visible = Mathf.Min(issues.Count, 4);
            for (int i = 0; i < visible; i++)
            {
                PyralisAuthoringGraphAuditRow issue = issues[i];
                if (issue == null)
                    continue;

                string text = string.IsNullOrWhiteSpace(issue.NativeAction)
                    ? $"{issue.Label}: {issue.Message}\nEvidence source: {issue.SourceLabel}\nOrigin: {issue.OriginLabel}\nReference id: {issue.NodeId}"
                    : $"{issue.Label}: {issue.Message}\nGraph source detail: {issue.NativeAction}\nEvidence source: {issue.SourceLabel}\nOrigin: {issue.OriginLabel}\nReference id: {issue.NodeId}";
                EditorGUILayout.HelpBox(text, messageType);
            }

            if (issues.Count > visible)
                EditorGUILayout.LabelField("+" + (issues.Count - visible) + " more audit finding(s)", EditorStyles.miniLabel);
        }

        private static void DrawGraphEvidenceCard(PyralisAuthoringGraphAuditRow issue)
        {
            if (issue == null)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(issue.Label, issue.NodeId, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Evidence", issue.EvidenceState.ToString(), EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.LabelField("Origin", issue.OriginLabel, EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.LabelField("Source", issue.SourceLabel, EditorStyles.wordWrappedMiniLabel);
                if (!string.IsNullOrWhiteSpace(issue.Message))
                    EditorGUILayout.LabelField("Graph Finding", issue.Message, EditorStyles.wordWrappedLabel);
                if (!string.IsNullOrWhiteSpace(issue.NativeAction))
                    EditorGUILayout.LabelField("Source Detail", issue.NativeAction, EditorStyles.wordWrappedMiniLabel);
                if (issue.Node != null && issue.Node.AssignmentFields.Length > 0)
                    PyralisAuthoringWindowPrimitives.DrawMiniList("Graph Fields", issue.Node.AssignmentFields);
                if (issue.Node != null && issue.Node.CustomizationMoments.Length > 0)
                    PyralisAuthoringWindowPrimitives.DrawMiniList("Customization", issue.Node.CustomizationMoments);
                if (!string.IsNullOrWhiteSpace(issue.Node?.BlockingReason))
                    EditorGUILayout.LabelField("Blocking Reason", issue.Node.BlockingReason, EditorStyles.wordWrappedMiniLabel);
            }
        }
    }
}
