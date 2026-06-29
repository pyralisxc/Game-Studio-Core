using System;
using System.Collections.Generic;
using Pys.Authoring.Contracts;

namespace Pys.Authoring.Editor.Hygiene
{
    public enum HygieneLensKind
    {
        Overview,
        Contracts,
        Dependencies,
        ValidationEvidence,
        ProjectionIntegrity,
        Ownership,
        RuntimeFlow,
        DocsAndClaims,
        VisualDependencyGraph
    }

    public enum HygieneSeverity
    {
        Info,
        Review,
        Warning,
        Error
    }

    public sealed class HygieneRow
    {
        public HygieneRow(string issueCode, string title, HygieneSeverity severity, string ownerId, string detail)
            : this(HygieneLensKind.Overview, issueCode, title, severity, ownerId, detail)
        {
        }

        public HygieneRow(HygieneLensKind lens, string issueCode, string title, HygieneSeverity severity, string ownerId, string detail)
            : this(lens, issueCode, title, severity, ownerId, detail, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "ObservedEvidence", false)
        {
        }

        public HygieneRow(
            HygieneLensKind lens,
            string issueCode,
            string title,
            HygieneSeverity severity,
            string ownerId,
            string detail,
            string sourceKind,
            string sourcePath,
            string evidenceIds,
            string claim,
            string evidence,
            string recommendation,
            string confidence,
            bool canNavigate)
        {
            Lens = lens;
            IssueCode = issueCode ?? string.Empty;
            Title = title ?? string.Empty;
            Severity = severity;
            OwnerId = ownerId ?? string.Empty;
            Detail = detail ?? string.Empty;
            SourceKind = sourceKind ?? string.Empty;
            SourcePath = sourcePath ?? string.Empty;
            EvidenceIds = evidenceIds ?? string.Empty;
            Claim = claim ?? string.Empty;
            Evidence = evidence ?? string.Empty;
            Recommendation = recommendation ?? string.Empty;
            Confidence = confidence ?? string.Empty;
            CanNavigate = canNavigate;
        }

        public HygieneLensKind Lens { get; }
        public string IssueCode { get; }
        public string Title { get; }
        public HygieneSeverity Severity { get; }
        public string OwnerId { get; }
        public string Detail { get; }
        public string SourceKind { get; }
        public string SourcePath { get; }
        public string EvidenceIds { get; }
        public string Claim { get; }
        public string Evidence { get; }
        public string Recommendation { get; }
        public string Confidence { get; }
        public bool CanNavigate { get; }
    }

    public sealed class HygieneLensProjection
    {
        public HygieneLensProjection(HygieneLensKind kind, string title, string question)
        {
            Kind = kind;
            Title = title ?? string.Empty;
            Question = question ?? string.Empty;
            Rows = new List<HygieneRow>();
        }

        public HygieneLensKind Kind { get; }
        public string Title { get; }
        public string Question { get; }
        public List<HygieneRow> Rows { get; }
        public int ReviewCount { get; set; }
        public int WarningCount { get; set; }
        public int ErrorCount { get; set; }
    }

    public sealed class HygieneProjection
    {
        public HygieneProjection()
        {
            Rows = new List<HygieneRow>();
            Lenses = new List<HygieneLensProjection>();
        }

        public List<HygieneRow> Rows { get; }
        public List<HygieneLensProjection> Lenses { get; }
        public int ReviewCount { get; set; }
        public int WarningCount { get; set; }
        public int ErrorCount { get; set; }
    }

    public static class HygieneProjectionBuilder
    {
        public static HygieneProjection Build(AuthoringGraph graph)
        {
            HygieneProjection projection = new HygieneProjection();
            BuildLenses(projection);
            if (graph == null)
                return projection;

            AddDuplicateStableIdRows(graph, projection);
            AddContractMetadataRows(graph, projection);
            AddContractReadinessRows(graph, projection);
            AddNamespaceFanoutRows(graph, projection);
            AddAssemblyReferenceRows(graph, projection);
            AddValidationMetadataRows(graph, projection);
            AddValidationOwnershipRows(graph, projection);
            AddExpectedEvidenceHonestyRows(graph, projection);
            AddDependencyGraphRows(graph, projection);
            BuildLenses(projection);
            CountSeverities(projection);
            return projection;
        }

        private static void AddContractMetadataRows(AuthoringGraph graph, HygieneProjection projection)
        {
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                AuthoringGraphNode node = graph.Nodes[i];
                if (node == null || node.Kind != AuthoringGraphNodeKind.Contract)
                    continue;

                string gaps = Metadata(node, "metadataGaps");
                if (string.IsNullOrWhiteSpace(gaps))
                    continue;

                AddRow(projection, new HygieneRow(
                    HygieneLensKind.Contracts,
                    "Contract.Metadata.Missing",
                    "Contract metadata needs attention",
                    HygieneSeverity.Warning,
                    node.Id,
                    gaps,
                    "Contract",
                    Metadata(node, "sourcePath"),
                    node.Id,
                    "Contract should describe enough metadata for projections.",
                    "Missing metadata: " + gaps,
                    "Complete the contract metadata on the declaring type.",
                    "ContractMetadata",
                    true));
            }
        }

        private static void AddContractReadinessRows(AuthoringGraph graph, HygieneProjection projection)
        {
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                AuthoringGraphNode node = graph.Nodes[i];
                if (node == null || node.Kind != AuthoringGraphNodeKind.Contract || !IsGoalCandidate(node))
                    continue;

                if (string.IsNullOrWhiteSpace(Metadata(node, "successDescription"))
                    && string.IsNullOrWhiteSpace(Metadata(node, "expectedEvidence"))
                    && string.IsNullOrWhiteSpace(Metadata(node, "completionSignals"))
                    && string.IsNullOrWhiteSpace(Metadata(node, "proofTarget")))
                {
                    AddRow(projection, new HygieneRow(
                        HygieneLensKind.Contracts,
                        "Contract.ReadinessHints.Missing",
                        "Goal contract lacks readiness hints",
                        HygieneSeverity.Review,
                        node.Id,
                        "Goal contracts should provide success, expected evidence, or completion signal hints.",
                        "Contract",
                        Metadata(node, "sourcePath"),
                        node.Id,
                        "Selectable goal can steer authoring.",
                        "No success/readiness/evidence metadata was observed.",
                        "Add SuccessDescription, ExpectedEvidence, or CompletionSignals.",
                        "ContractMetadata",
                        true));
                }

                if (string.IsNullOrWhiteSpace(Metadata(node, "validationOwnerStableId")))
                {
                    AddRow(projection, new HygieneRow(
                        HygieneLensKind.ValidationEvidence,
                        "Contract.ValidationOwner.Missing",
                        "Goal contract lacks validation owner stable ID",
                        HygieneSeverity.Review,
                        node.Id,
                        "ValidationOwnerStableId helps Guide connect readiness evidence to this route.",
                        "Contract",
                        Metadata(node, "sourcePath"),
                        node.Id,
                        "Guide should connect target-owned validation to selected routes.",
                        "No validation owner stable ID was observed.",
                        "Add ValidationOwnerStableId when target-owned validation exists for this route.",
                        "ContractMetadata",
                        true));
                }
            }
        }

        private static void AddDuplicateStableIdRows(AuthoringGraph graph, HygieneProjection projection)
        {
            Dictionary<string, List<AuthoringGraphNode>> contractsByStableId = new Dictionary<string, List<AuthoringGraphNode>>();
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                AuthoringGraphNode node = graph.Nodes[i];
                if (node == null || node.Kind != AuthoringGraphNodeKind.Contract)
                    continue;

                string stableId = StableIdFor(node);
                if (string.IsNullOrWhiteSpace(stableId))
                    continue;

                if (!contractsByStableId.TryGetValue(stableId, out List<AuthoringGraphNode> contracts))
                {
                    contracts = new List<AuthoringGraphNode>();
                    contractsByStableId[stableId] = contracts;
                }

                contracts.Add(node);
            }

            foreach (KeyValuePair<string, List<AuthoringGraphNode>> pair in contractsByStableId)
            {
                if (pair.Value.Count <= 1)
                    continue;

                AddRow(projection, new HygieneRow(
                    HygieneLensKind.Contracts,
                    "Contract.StableId.Duplicate",
                    "Duplicate contract StableId",
                    HygieneSeverity.Warning,
                    "contract:" + pair.Key,
                    DuplicateStableIdDetail(pair.Key, pair.Value),
                    "Contract",
                    string.Empty,
                    string.Join(",", NodeIds(pair.Value)),
                    "StableId should uniquely identify one contract.",
                    "Multiple contracts reported the same StableId.",
                    "Give each contract a unique StableId or split shared meaning into explicit supporting IDs.",
                    "ContractMetadata",
                    true));
            }
        }

        private static void AddNamespaceFanoutRows(AuthoringGraph graph, HygieneProjection projection)
        {
            Dictionary<string, int> countsByScript = new Dictionary<string, int>();
            for (int i = 0; i < graph.Edges.Count; i++)
            {
                AuthoringGraphEdge edge = graph.Edges[i];
                if (edge == null || edge.Kind != AuthoringGraphEdgeKind.NamespaceUsing)
                    continue;

                countsByScript.TryGetValue(edge.FromNodeId, out int count);
                countsByScript[edge.FromNodeId] = count + 1;
            }

            foreach (KeyValuePair<string, int> pair in countsByScript)
            {
                if (pair.Value <= 8)
                    continue;

                AddRow(projection, new HygieneRow(
                    HygieneLensKind.Dependencies,
                    "Source.NamespaceFanout",
                    "Source file has broad namespace fanout",
                    HygieneSeverity.Review,
                    pair.Key,
                    pair.Value + " using directives",
                    "Script",
                    pair.Key.StartsWith("script:", StringComparison.Ordinal) ? pair.Key.Substring("script:".Length) : string.Empty,
                    pair.Key,
                    "Source file should keep dependency surface understandable.",
                    pair.Value + " namespace using edges were observed.",
                    "Review whether this file owns too many dependency directions.",
                    "SourceDependencyScan",
                    true));
            }
        }

        private static void AddAssemblyReferenceRows(AuthoringGraph graph, HygieneProjection projection)
        {
            Dictionary<string, List<string>> referencesByAssembly = new Dictionary<string, List<string>>();
            for (int i = 0; i < graph.Edges.Count; i++)
            {
                AuthoringGraphEdge edge = graph.Edges[i];
                if (edge == null || edge.Kind != AuthoringGraphEdgeKind.AssemblyReference)
                    continue;

                if (!referencesByAssembly.TryGetValue(edge.FromNodeId, out List<string> references))
                {
                    references = new List<string>();
                    referencesByAssembly[edge.FromNodeId] = references;
                }

                references.Add(edge.ToNodeId);
            }

            foreach (KeyValuePair<string, List<string>> pair in referencesByAssembly)
            {
                AddRow(projection, new HygieneRow(
                    HygieneLensKind.Dependencies,
                    "Assembly.Reference.Group",
                    "Assembly references",
                    HygieneSeverity.Info,
                    pair.Key,
                    pair.Value.Count + " reference(s): " + string.Join(", ", pair.Value),
                    "Assembly",
                    string.Empty,
                    pair.Key + "," + string.Join(",", pair.Value),
                    "Assembly reference edges should remain inspectable.",
                    pair.Value.Count + " outgoing assembly reference edge(s).",
                    "Use dependency pressure only when this grouping becomes hard to reason about.",
                    "AsmdefScan",
                    true));
            }
        }

        private static void AddValidationMetadataRows(AuthoringGraph graph, HygieneProjection projection)
        {
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                AuthoringGraphNode node = graph.Nodes[i];
                if (node == null || node.Kind != AuthoringGraphNodeKind.Issue)
                    continue;

                string issueCode = Metadata(node, "issueCode");
                string nativeAction = Metadata(node, "nativeAction");
                string successCheck = Metadata(node, "successCheck");
                string ownerStableId = Metadata(node, "ownerStableId");
                string relatedStableIds = Metadata(node, "relatedStableIds");
                if (!string.IsNullOrWhiteSpace(issueCode)
                    && !string.IsNullOrWhiteSpace(nativeAction)
                    && !string.IsNullOrWhiteSpace(successCheck)
                    && (!string.IsNullOrWhiteSpace(ownerStableId) || !string.IsNullOrWhiteSpace(relatedStableIds)))
                {
                    continue;
                }

                AddRow(projection, new HygieneRow(
                    HygieneLensKind.ValidationEvidence,
                    "Validation.Metadata.Incomplete",
                    "Validation issue is missing structured metadata",
                    HygieneSeverity.Review,
                    node.Id,
                    "Issue code, action, success check, and owner/related stable IDs help projections stay aligned.",
                    "Issue",
                    string.Empty,
                    node.Id,
                    "Validation records should witness local readiness with enough structure.",
                    "Missing: " + MissingValidationMetadata(issueCode, nativeAction, successCheck, ownerStableId, relatedStableIds),
                    "Add structured properties to the target-owned validation issue object.",
                    "ValidationRecord",
                    false));
            }
        }

        private static void AddValidationOwnershipRows(AuthoringGraph graph, HygieneProjection projection)
        {
            HashSet<string> contractStableIds = ContractStableIds(graph);
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                AuthoringGraphNode issue = graph.Nodes[i];
                if (issue == null || issue.Kind != AuthoringGraphNodeKind.Issue)
                    continue;

                string ownerStableId = Metadata(issue, "ownerStableId");
                if (string.IsNullOrWhiteSpace(ownerStableId) || contractStableIds.Contains(ownerStableId))
                    continue;

                AddRow(projection, new HygieneRow(
                    HygieneLensKind.ValidationEvidence,
                    "Validation.Owner.Unmatched",
                    "Validation owner does not match an observed contract",
                    HygieneSeverity.Review,
                    issue.Id,
                    ownerStableId,
                    "Issue",
                    string.Empty,
                    issue.Id,
                    "Validation owner stable IDs should connect to observed route contracts.",
                    "No contract with this stable ID was observed.",
                    "Check the target validation owner stable ID or add the owning contract to the scanned root.",
                    "ValidationRecord",
                    false));
            }
        }

        private static void AddExpectedEvidenceHonestyRows(AuthoringGraph graph, HygieneProjection projection)
        {
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                AuthoringGraphNode contract = graph.Nodes[i];
                if (contract == null || contract.Kind != AuthoringGraphNodeKind.Contract)
                    continue;

                string[] expectedEvidence = SplitLines(Metadata(contract, "expectedEvidence"));
                for (int evidenceIndex = 0; evidenceIndex < expectedEvidence.Length; evidenceIndex++)
                {
                    string expected = expectedEvidence[evidenceIndex];
                    if (EvidenceObserved(graph, contract, expected))
                        continue;

                    AddRow(projection, new HygieneRow(
                        HygieneLensKind.Ownership,
                        "Honesty.ExpectedEvidence.Unobserved",
                        "Contract expects evidence that was not observed",
                        HygieneSeverity.Review,
                        contract.Id,
                        expected,
                        "Contract",
                        Metadata(contract, "sourcePath"),
                        contract.Id,
                        "Contract claims expected evidence: " + expected,
                        "No matching node, edge, label, ID, or metadata value was observed in the compiled graph.",
                        "Add matching reflected/scene/asset/validation evidence or revise the contract hint.",
                        "InferredFromGraph",
                        true));
                }
            }
        }

        private static void AddDependencyGraphRows(AuthoringGraph graph, HygieneProjection projection)
        {
            Dictionary<AuthoringGraphNodeKind, int> nodeCounts = new Dictionary<AuthoringGraphNodeKind, int>();
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                AuthoringGraphNode node = graph.Nodes[i];
                if (node == null)
                    continue;

                nodeCounts.TryGetValue(node.Kind, out int count);
                nodeCounts[node.Kind] = count + 1;
            }

            foreach (KeyValuePair<AuthoringGraphNodeKind, int> pair in nodeCounts)
            {
                AddRow(projection, new HygieneRow(
                    HygieneLensKind.VisualDependencyGraph,
                    "Graph.NodeKind.Group",
                    pair.Key + " nodes",
                    HygieneSeverity.Info,
                    "graph:node:" + pair.Key,
                    pair.Value + " node(s)",
                    "GraphNode",
                    string.Empty,
                    pair.Key.ToString(),
                    "Visual graph should expose node groups before drilling into individual graph records.",
                    pair.Value + " " + pair.Key + " node(s) were compiled.",
                    "Use this lens as the textual source for future visual graph rendering.",
                    "CompiledGraph",
                    false));
            }

            Dictionary<AuthoringGraphEdgeKind, int> counts = new Dictionary<AuthoringGraphEdgeKind, int>();
            for (int i = 0; i < graph.Edges.Count; i++)
            {
                AuthoringGraphEdge edge = graph.Edges[i];
                if (edge == null)
                    continue;

                counts.TryGetValue(edge.Kind, out int count);
                counts[edge.Kind] = count + 1;
            }

            foreach (KeyValuePair<AuthoringGraphEdgeKind, int> pair in counts)
            {
                AddRow(projection, new HygieneRow(
                    HygieneLensKind.VisualDependencyGraph,
                    "Graph.EdgeKind.Group",
                    pair.Key + " edges",
                    HygieneSeverity.Info,
                    "graph:" + pair.Key,
                    pair.Value + " edge(s)",
                    "GraphEdge",
                    string.Empty,
                    pair.Key.ToString(),
                    "Visual graph should expose dependency edge kinds before drilling into details.",
                    pair.Value + " " + pair.Key + " edge(s) were compiled.",
                    "Use this lens as the textual source for future visual graph rendering.",
                    "CompiledGraph",
                    false));
            }
        }

        private static void BuildLenses(HygieneProjection projection)
        {
            projection.Lenses.Clear();
            AddLens(projection, HygieneLensKind.Overview, "Overview", "What needs audit attention first?");
            AddLens(projection, HygieneLensKind.Contracts, "Contract Hygiene", "Are machine-readable contracts complete and honest enough to steer projections?");
            AddLens(projection, HygieneLensKind.Dependencies, "Dependency Pressure", "How are files, assemblies, and systems connected?");
            AddLens(projection, HygieneLensKind.ValidationEvidence, "Validation Evidence", "Are validation records structured enough to witness readiness?");
            AddLens(projection, HygieneLensKind.ProjectionIntegrity, "Projection Integrity", "Do projections have enough typed evidence to display and export the same view?");
            AddLens(projection, HygieneLensKind.Ownership, "Ownership & Honesty", "Do claims stay backed by observed evidence and clear ownership?");
            AddLens(projection, HygieneLensKind.RuntimeFlow, "Runtime Flow", "Are requests, facts, state, and handlers explicit?");
            AddLens(projection, HygieneLensKind.DocsAndClaims, "Docs & Claims", "Do prose claims stay separate from typed evidence?");
            AddLens(projection, HygieneLensKind.VisualDependencyGraph, "Dependency Graph", "What graph edge groups would a visual dependency view render?");
        }

        private static void AddLens(HygieneProjection projection, HygieneLensKind kind, string title, string question)
        {
            HygieneLensProjection lens = new HygieneLensProjection(kind, title, question);

            for (int i = 0; i < projection.Rows.Count; i++)
            {
                HygieneRow row = projection.Rows[i];
                if (kind == HygieneLensKind.Overview || row.Lens == kind)
                    lens.Rows.Add(row);
            }

            CountLensSeverities(lens);
            projection.Lenses.Add(lens);
        }

        private static void CountSeverities(HygieneProjection projection)
        {
            projection.ReviewCount = 0;
            projection.WarningCount = 0;
            projection.ErrorCount = 0;

            for (int i = 0; i < projection.Rows.Count; i++)
            {
                switch (projection.Rows[i].Severity)
                {
                    case HygieneSeverity.Review:
                        projection.ReviewCount++;
                        break;
                    case HygieneSeverity.Warning:
                        projection.WarningCount++;
                        break;
                    case HygieneSeverity.Error:
                        projection.ErrorCount++;
                        break;
                }
            }
        }

        private static void CountLensSeverities(HygieneLensProjection lens)
        {
            lens.ReviewCount = 0;
            lens.WarningCount = 0;
            lens.ErrorCount = 0;

            for (int i = 0; i < lens.Rows.Count; i++)
            {
                switch (lens.Rows[i].Severity)
                {
                    case HygieneSeverity.Review:
                        lens.ReviewCount++;
                        break;
                    case HygieneSeverity.Warning:
                        lens.WarningCount++;
                        break;
                    case HygieneSeverity.Error:
                        lens.ErrorCount++;
                        break;
                }
            }
        }

        private static void AddRow(HygieneProjection projection, HygieneRow row)
        {
            projection.Rows.Add(row);
        }

        private static bool IsGoalCandidate(AuthoringGraphNode node)
        {
            return Metadata(node, "surface") == AuthoringSurface.Goal.ToString()
                || !string.IsNullOrWhiteSpace(Metadata(node, "successDescription"))
                || !string.IsNullOrWhiteSpace(Metadata(node, "expectedEvidence"))
                || !string.IsNullOrWhiteSpace(Metadata(node, "completionSignals"))
                || !string.IsNullOrWhiteSpace(Metadata(node, "proofTarget"))
                || !string.IsNullOrWhiteSpace(Metadata(node, "successChecks"));
        }

        private static bool EvidenceObserved(AuthoringGraph graph, AuthoringGraphNode owner, string expected)
        {
            if (graph == null || string.IsNullOrWhiteSpace(expected))
                return false;

            string normalized = expected.Trim();
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                AuthoringGraphNode node = graph.Nodes[i];
                if (node == owner)
                    continue;

                if (ContainsText(node.Id, normalized) || ContainsText(node.Label, normalized))
                    return true;

                foreach (KeyValuePair<string, string> pair in node.Metadata)
                {
                    if (ContainsText(pair.Key, normalized) || ContainsText(pair.Value, normalized))
                        return true;
                }
            }

            for (int i = 0; i < graph.Edges.Count; i++)
            {
                AuthoringGraphEdge edge = graph.Edges[i];
                if (ContainsText(edge.FromNodeId, normalized) || ContainsText(edge.ToNodeId, normalized) || ContainsText(edge.Kind.ToString(), normalized))
                    return true;
            }

            return false;
        }

        private static bool ContainsText(string value, string expected)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value.IndexOf(expected, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string MissingValidationMetadata(string issueCode, string nativeAction, string successCheck, string ownerStableId, string relatedStableIds)
        {
            List<string> missing = new List<string>();
            if (string.IsNullOrWhiteSpace(issueCode))
                missing.Add("IssueCode");
            if (string.IsNullOrWhiteSpace(nativeAction))
                missing.Add("NativeAction");
            if (string.IsNullOrWhiteSpace(successCheck))
                missing.Add("SuccessCheck");
            if (string.IsNullOrWhiteSpace(ownerStableId) && string.IsNullOrWhiteSpace(relatedStableIds))
                missing.Add("OwnerStableId or RelatedStableIds");

            return string.Join(", ", missing);
        }

        private static HashSet<string> ContractStableIds(AuthoringGraph graph)
        {
            HashSet<string> stableIds = new HashSet<string>();
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                AuthoringGraphNode node = graph.Nodes[i];
                if (node != null && node.Kind == AuthoringGraphNodeKind.Contract)
                    stableIds.Add(StableIdFor(node));
            }

            return stableIds;
        }

        private static string[] NodeIds(List<AuthoringGraphNode> nodes)
        {
            string[] ids = new string[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
                ids[i] = nodes[i].Id;

            return ids;
        }

        private static string[] SplitLines(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new string[0];

            string[] raw = value.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> lines = new List<string>();
            for (int i = 0; i < raw.Length; i++)
            {
                string line = raw[i].Trim();
                if (!string.IsNullOrWhiteSpace(line))
                    lines.Add(line);
            }

            return lines.ToArray();
        }

        private static string StableIdFor(AuthoringGraphNode node)
        {
            if (node == null)
                return string.Empty;

            if (node.Metadata.TryGetValue("stableId", out string stableId) && !string.IsNullOrWhiteSpace(stableId))
                return stableId;

            const string Prefix = "contract:";
            return node.Id.StartsWith(Prefix, StringComparison.Ordinal) ? node.Id.Substring(Prefix.Length) : string.Empty;
        }

        private static string Metadata(AuthoringGraphNode node, string key)
        {
            if (node == null || string.IsNullOrWhiteSpace(key))
                return string.Empty;

            return node.Metadata.TryGetValue(key, out string value) ? value ?? string.Empty : string.Empty;
        }

        private static string DuplicateStableIdDetail(string stableId, List<AuthoringGraphNode> contracts)
        {
            List<string> parts = new List<string>();
            for (int i = 0; i < contracts.Count; i++)
            {
                AuthoringGraphNode contract = contracts[i];
                parts.Add(Metadata(contract, "sourceType") + " (" + Metadata(contract, "sourcePath") + ")");
            }

            return stableId + ": " + string.Join("; ", parts);
        }
    }
}
