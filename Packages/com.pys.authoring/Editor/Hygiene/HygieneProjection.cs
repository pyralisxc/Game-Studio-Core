using System.Collections.Generic;
using Pys.Authoring.Contracts;

namespace Pys.Authoring.Editor.Hygiene
{
    public enum HygieneLensKind
    {
        Overview,
        Ownership,
        Dependencies,
        Contracts,
        RuntimeFlow,
        ProjectionIntegrity,
        DocsAndClaims
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
        {
            Lens = lens;
            IssueCode = issueCode ?? string.Empty;
            Title = title ?? string.Empty;
            Severity = severity;
            OwnerId = ownerId ?? string.Empty;
            Detail = detail ?? string.Empty;
        }

        public HygieneLensKind Lens { get; }

        public string IssueCode { get; }

        public string Title { get; }

        public HygieneSeverity Severity { get; }

        public string OwnerId { get; }

        public string Detail { get; }
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
            AddNamespaceFanoutRows(graph, projection);
            AddAssemblyReferenceRows(graph, projection);
            AddValidationMetadataRows(graph, projection);
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

                if (!node.Metadata.TryGetValue("metadataGaps", out string gaps) || string.IsNullOrWhiteSpace(gaps))
                    continue;

                projection.Rows.Add(new HygieneRow(
                    HygieneLensKind.Contracts,
                    "Contract.Metadata.Missing",
                    "Contract metadata needs attention",
                    HygieneSeverity.Warning,
                    node.Id,
                    gaps));
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

                projection.Rows.Add(new HygieneRow(
                    HygieneLensKind.Contracts,
                    "Contract.StableId.Duplicate",
                    "Duplicate contract StableId",
                    HygieneSeverity.Warning,
                    "contract:" + pair.Key,
                    DuplicateStableIdDetail(pair.Key, pair.Value)));
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

                projection.Rows.Add(new HygieneRow(
                    HygieneLensKind.Dependencies,
                    "Source.NamespaceFanout",
                    "Source file has broad namespace fanout",
                    HygieneSeverity.Review,
                    pair.Key,
                    pair.Value + " using directives"));
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

                if (!references.Contains(edge.ToNodeId))
                    references.Add(edge.ToNodeId);
            }

            foreach (KeyValuePair<string, List<string>> pair in referencesByAssembly)
            {
                projection.Rows.Add(new HygieneRow(
                    HygieneLensKind.Dependencies,
                    "Assembly.Reference.Group",
                    "Assembly references",
                    HygieneSeverity.Info,
                    pair.Key,
                    pair.Value.Count + " reference(s): " + string.Join(", ", pair.Value)));
            }
        }

        private static void AddValidationMetadataRows(AuthoringGraph graph, HygieneProjection projection)
        {
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                AuthoringGraphNode node = graph.Nodes[i];
                if (node == null || node.Kind != AuthoringGraphNodeKind.Issue)
                    continue;

                node.Metadata.TryGetValue("issueCode", out string issueCode);
                node.Metadata.TryGetValue("nativeAction", out string nativeAction);
                node.Metadata.TryGetValue("fieldPath", out string fieldPath);
                if (!string.IsNullOrWhiteSpace(issueCode)
                    && !string.IsNullOrWhiteSpace(nativeAction)
                    && !string.IsNullOrWhiteSpace(fieldPath))
                {
                    continue;
                }

                projection.Rows.Add(new HygieneRow(
                    HygieneLensKind.ProjectionIntegrity,
                    "Validation.Metadata.Incomplete",
                    "Validation issue is missing structured metadata",
                    HygieneSeverity.Review,
                    node.Id,
                    "Issue code, field path, and native action help projections stay display/export aligned."));
            }
        }

        private static void BuildLenses(HygieneProjection projection)
        {
            projection.Lenses.Clear();
            AddLens(projection, HygieneLensKind.Overview, "Overview", "What needs attention first?");
            AddLens(projection, HygieneLensKind.Ownership, "Ownership", "Do owners and responsibilities stay clear?");
            AddLens(projection, HygieneLensKind.Dependencies, "Dependencies", "How are files, assemblies, and systems connected?");
            AddLens(projection, HygieneLensKind.Contracts, "Contracts", "Are machine-readable contracts complete enough to steer projections?");
            AddLens(projection, HygieneLensKind.RuntimeFlow, "Runtime Flow", "Are requests, facts, state, and handlers explicit?");
            AddLens(projection, HygieneLensKind.ProjectionIntegrity, "Projection Integrity", "Do projections have enough typed evidence to display and export the same view?");
            AddLens(projection, HygieneLensKind.DocsAndClaims, "Docs & Claims", "Do prose claims stay separate from typed evidence?");
        }

        private static void AddLens(HygieneProjection projection, HygieneLensKind kind, string title, string question)
        {
            HygieneLensProjection lens = new HygieneLensProjection(kind, title, question);

            for (int i = 0; i < projection.Rows.Count; i++)
            {
                HygieneRow row = projection.Rows[i];
                if (row == null)
                    continue;

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

        private static string StableIdFor(AuthoringGraphNode node)
        {
            if (node == null)
                return string.Empty;

            if (node.Metadata.TryGetValue("stableId", out string stableId) && !string.IsNullOrWhiteSpace(stableId))
                return stableId;

            const string Prefix = "contract:";
            return node.Id.StartsWith(Prefix) ? node.Id.Substring(Prefix.Length) : string.Empty;
        }

        private static string DuplicateStableIdDetail(string stableId, List<AuthoringGraphNode> contracts)
        {
            List<string> sources = new List<string>();
            for (int i = 0; i < contracts.Count; i++)
            {
                AuthoringGraphNode contract = contracts[i];
                contract.Metadata.TryGetValue("sourceType", out string sourceType);
                contract.Metadata.TryGetValue("sourcePath", out string sourcePath);
                string source = string.IsNullOrWhiteSpace(sourceType) ? contract.Label : sourceType;
                if (!string.IsNullOrWhiteSpace(sourcePath))
                    source += " (" + sourcePath + ")";

                sources.Add(source);
            }

            return stableId + " is declared by " + string.Join("; ", sources);
        }
    }
}
