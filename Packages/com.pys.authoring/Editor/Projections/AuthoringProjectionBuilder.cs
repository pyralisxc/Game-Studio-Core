using System.Collections.Generic;
using Pys.Authoring.Contracts;
using Pys.Authoring.Editor.Vocabulary;

namespace Pys.Authoring.Editor.Projections
{
    public static class AuthoringProjectionBuilder
    {
        public static IntentProjection BuildIntent(AuthoringGraph graph)
        {
            return BuildIntent(graph, string.Empty);
        }

        public static IntentProjection BuildIntent(AuthoringGraph graph, string selectedContractId)
        {
            IntentProjection projection = new IntentProjection();
            projection.SelectedContractId = selectedContractId ?? string.Empty;
            if (graph == null)
                return projection;

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                AuthoringGraphNode node = graph.Nodes[i];
                if (node.Kind != AuthoringGraphNodeKind.Contract)
                    continue;

                node.Metadata.TryGetValue("selectable", out string selectableText);
                bool selectable = selectableText == "true";
                node.Metadata.TryGetValue("category", out string category);
                node.Metadata.TryGetValue("capabilityPath", out string capabilityPath);
                node.Metadata.TryGetValue("surface", out string surface);
                node.Metadata.TryGetValue("summary", out string summary);
                node.Metadata.TryGetValue("metadataGaps", out string gaps);
                node.Metadata.TryGetValue("stableId", out string stableId);
                node.Metadata.TryGetValue("sourceType", out string sourceType);
                node.Metadata.TryGetValue("sourcePath", out string sourcePath);
                node.Metadata.TryGetValue("duplicateStableId", out string duplicateStableId);
                bool duplicate = duplicateStableId == "true";
                string disabledReason = string.IsNullOrWhiteSpace(gaps) ? string.Empty : "Missing metadata: " + gaps;
                if (duplicate)
                {
                    string duplicateReason = "Duplicate StableId";
                    disabledReason = string.IsNullOrWhiteSpace(disabledReason)
                        ? duplicateReason
                        : duplicateReason + "; " + disabledReason;
                    selectable = false;
                }

                if (selectable)
                    projection.SelectableCount++;

                projection.Rows.Add(new IntentRow
                {
                    ContractId = node.Id,
                    DisplayName = node.Label,
                    Category = category ?? string.Empty,
                    CapabilityPath = capabilityPath ?? string.Empty,
                    Surface = surface ?? string.Empty,
                    Summary = summary ?? string.Empty,
                    Selectable = selectable,
                    DisabledReason = disabledReason,
                    StableId = stableId ?? string.Empty,
                    SourceType = sourceType ?? string.Empty,
                    SourcePath = sourcePath ?? string.Empty
                });

                if (node.Id == projection.SelectedContractId)
                {
                    projection.SelectedDisplayName = node.Label;
                    projection.SelectedDisabledReason = disabledReason;
                }
            }

            return projection;
        }

        public static FactsProjection BuildFacts(AuthoringGraph graph)
        {
            FactsProjection projection = new FactsProjection();
            if (graph == null)
                return projection;

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                switch (graph.Nodes[i].Kind)
                {
                    case AuthoringGraphNodeKind.Assembly:
                        projection.AssemblyCount++;
                        break;
                    case AuthoringGraphNodeKind.Namespace:
                        projection.NamespaceCount++;
                        break;
                    case AuthoringGraphNodeKind.Type:
                        projection.TypeCount++;
                        break;
                    case AuthoringGraphNodeKind.Script:
                        projection.ScriptCount++;
                        break;
                    case AuthoringGraphNodeKind.Field:
                        projection.FieldCount++;
                        break;
                    case AuthoringGraphNodeKind.Contract:
                        projection.ContractCount++;
                        break;
                    case AuthoringGraphNodeKind.Validator:
                        projection.ValidatorCount++;
                        break;
                    case AuthoringGraphNodeKind.SceneObject:
                        projection.SceneObjectCount++;
                        break;
                    case AuthoringGraphNodeKind.Prefab:
                        projection.PrefabCount++;
                        break;
                    case AuthoringGraphNodeKind.Asset:
                        projection.AssetCount++;
                        break;
                    case AuthoringGraphNodeKind.Issue:
                        projection.IssueCount++;
                        break;
                }

                AddFactRow(projection, graph, graph.Nodes[i]);
            }

            return projection;
        }

        public static MapProjection BuildMap(AuthoringGraph graph)
        {
            MapProjection projection = new MapProjection();
            if (graph == null)
                return projection;

            Dictionary<string, int> componentCounts = CountEdges(graph, AuthoringGraphEdgeKind.SceneContains, AuthoringGraphEdgeKind.PrefabContains);
            Dictionary<string, int> issueCounts = CountEdges(graph, AuthoringGraphEdgeKind.ValidatorReports);

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                AuthoringGraphNode node = graph.Nodes[i];
                if (node.Kind != AuthoringGraphNodeKind.SceneObject
                    && node.Kind != AuthoringGraphNodeKind.Prefab
                    && node.Kind != AuthoringGraphNodeKind.Asset)
                {
                    continue;
                }

                node.Metadata.TryGetValue("sourcePath", out string sourcePath);
                projection.Rows.Add(new MapRow
                {
                    Id = node.Id,
                    Label = node.Label,
                    Kind = node.Kind.ToString(),
                    SourcePath = sourcePath ?? string.Empty,
                    ComponentCount = componentCounts.TryGetValue(node.Id, out int components) ? components : 0,
                    IssueCount = issueCounts.TryGetValue(node.Id, out int issues) ? issues : 0
                });
            }

            return projection;
        }

        public static OverviewProjection BuildOverview(AuthoringGraph graph)
        {
            return BuildOverview(graph, BuildGuide(graph));
        }

        public static OverviewProjection BuildOverview(AuthoringGraph graph, GuideProjection guide)
        {
            OverviewProjection projection = new OverviewProjection();
            if (graph == null)
            {
                projection.Summary = "No graph has been scanned yet.";
                projection.NextAction = "Open Settings and scan a scripts folder.";
                return projection;
            }

            int issueCount = CountNodes(graph, AuthoringGraphNodeKind.Issue);
            projection.IssueCount = issueCount;
            projection.SelectedIntent = guide != null ? guide.SelectedDisplayName ?? string.Empty : string.Empty;
            projection.ProofTarget = guide != null ? guide.ProofTarget ?? string.Empty : string.Empty;
            projection.Readiness = guide != null && guide.ProofReady ? "Ready" : "Blocked";

            GuideRow activeRow = FindFirstBlockingGuideRow(guide);
            if (activeRow != null)
            {
                projection.Summary = string.IsNullOrWhiteSpace(projection.SelectedIntent)
                    ? issueCount + " graph issue(s) are asking for attention."
                    : "Selected intent is blocked before proof: " + projection.SelectedIntent;
                projection.NextAction = !string.IsNullOrWhiteSpace(activeRow.NativeAction) ? activeRow.NativeAction : activeRow.Title;
                projection.Reason = activeRow.Detail ?? string.Empty;
            }
            else
            {
                projection.Summary = string.IsNullOrWhiteSpace(projection.SelectedIntent)
                    ? "No graph issues are asking for attention."
                    : "Selected intent has no blocking guide rows.";
                projection.NextAction = string.IsNullOrWhiteSpace(projection.ProofTarget)
                    ? "Review Hygiene or export the graph when you need more detail."
                    : "Run the proof target: " + projection.ProofTarget;
                projection.Reason = "Proof ready";
                projection.Readiness = "Ready";
            }

            return projection;
        }

        public static GuideProjection BuildGuide(AuthoringGraph graph)
        {
            return BuildGuide(graph, string.Empty);
        }

        public static GuideProjection BuildGuide(AuthoringGraph graph, string selectedContractId)
        {
            GuideProjection projection = new GuideProjection();
            projection.SelectedContractId = selectedContractId ?? string.Empty;
            if (graph == null)
                return projection;

            AuthoringGraphNode selectedContract = FindSelectedContract(graph, selectedContractId);
            if (selectedContract != null)
                AddSelectedContractRows(selectedContract, projection);

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                AuthoringGraphNode node = graph.Nodes[i];
                if (node.Kind != AuthoringGraphNodeKind.Issue)
                    continue;

                node.Metadata.TryGetValue("nativeAction", out string nativeAction);
                node.Metadata.TryGetValue("successCheck", out string successCheck);
                node.Metadata.TryGetValue("issueCode", out string issueCode);
                node.Metadata.TryGetValue("actionKind", out string actionKind);
                string actionLabel = ActionLabel(actionKind);

                projection.Rows.Add(new GuideRow
                {
                    Order = projection.Rows.Count + 1,
                    Role = "Issue",
                    OwnerId = node.Id,
                    Title = node.Label,
                    Detail = issueCode ?? string.Empty,
                    ActionKind = actionKind ?? string.Empty,
                    ActionLabel = actionLabel,
                    NativeAction = nativeAction ?? string.Empty,
                    SuccessCheck = successCheck ?? string.Empty,
                    BlocksProof = true
                });
            }

            AddContractSetupRows(graph, projection);
            projection.ProofReady = FindFirstBlockingGuideRow(projection) == null;
            return projection;
        }

        private static void AddSelectedContractRows(AuthoringGraphNode contract, GuideProjection projection)
        {
            projection.SelectedDisplayName = contract.Label;
            projection.ProofTarget = contract.Label;
            contract.Metadata.TryGetValue("metadataGaps", out string gaps);
            contract.Metadata.TryGetValue("setupSteps", out string setupSteps);
            contract.Metadata.TryGetValue("successChecks", out string successChecks);
            contract.Metadata.TryGetValue("surface", out string surface);
            contract.Metadata.TryGetValue("duplicateStableId", out string duplicateStableId);
            contract.Metadata.TryGetValue("stableId", out string stableId);
            contract.Metadata.TryGetValue("sourceType", out string sourceType);
            contract.Metadata.TryGetValue("sourcePath", out string sourcePath);

            if (surface == AuthoringSurface.Goal.ToString())
                projection.ProofTarget = contract.Label;

            if (duplicateStableId == "true")
            {
                projection.Rows.Add(new GuideRow
                {
                    Order = projection.Rows.Count + 1,
                    Role = "ContractIdentity",
                    OwnerId = contract.Id,
                    Title = "Resolve duplicate StableId",
                    Detail = stableId ?? string.Empty,
                    ActionKind = AuthoringActionKind.ReviewCode.ToString(),
                    ActionLabel = ActionLabel(AuthoringActionKind.ReviewCode.ToString()),
                    NativeAction = "Give this contract a StableId that is unique inside the selected scripts folder.",
                    SuccessCheck = "Only one contract reports this StableId after scanning. Source: " + (sourceType ?? string.Empty) + " " + (sourcePath ?? string.Empty),
                    BlocksProof = true
                });
            }

            if (!string.IsNullOrWhiteSpace(gaps))
            {
                projection.Rows.Add(new GuideRow
                {
                    Order = projection.Rows.Count + 1,
                    Role = "ContractMetadata",
                    OwnerId = contract.Id,
                    Title = "Complete selected intent metadata",
                    Detail = gaps,
                    ActionKind = AuthoringActionKind.ReviewCode.ToString(),
                    ActionLabel = ActionLabel(AuthoringActionKind.ReviewCode.ToString()),
                    NativeAction = "Edit the selected contract metadata inside the selected scripts folder.",
                    SuccessCheck = "The selected contract metadata gap no longer appears after scanning.",
                    BlocksProof = true
                });
            }

            AddLinesAsGuideRows(projection, contract.Id, setupSteps, "SetupStep", AuthoringActionKind.InspectObject.ToString(), true);
            AddLinesAsGuideRows(projection, contract.Id, successChecks, "ProofCheck", AuthoringActionKind.RunPlayModeCheck.ToString(), false);
        }

        private static void AddLinesAsGuideRows(
            GuideProjection projection,
            string ownerId,
            string lines,
            string role,
            string actionKind,
            bool blocksProof)
        {
            if (string.IsNullOrWhiteSpace(lines))
                return;

            string[] split = lines.Split('\n');
            for (int i = 0; i < split.Length; i++)
            {
                string line = split[i].Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                projection.Rows.Add(new GuideRow
                {
                    Order = projection.Rows.Count + 1,
                    Role = role,
                    OwnerId = ownerId,
                    Title = role == "ProofCheck" ? "Verify proof target" : "Complete setup step",
                    Detail = line,
                    ActionKind = actionKind,
                    ActionLabel = ActionLabel(actionKind),
                    NativeAction = line,
                    SuccessCheck = role == "ProofCheck" ? line : "The setup step is represented by validation evidence after scanning.",
                    BlocksProof = blocksProof
                });
            }
        }

        private static void AddContractSetupRows(AuthoringGraph graph, GuideProjection projection)
        {
            AuthoringGraphNode selected = FindSelectedContract(graph, projection.SelectedContractId);
            string selectedStableId = StableIdFor(selected);

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                AuthoringGraphNode node = graph.Nodes[i];
                if (node.Kind != AuthoringGraphNodeKind.Contract)
                    continue;

                if (!string.IsNullOrWhiteSpace(projection.SelectedContractId) && node.Id == projection.SelectedContractId)
                    continue;

                if (!string.IsNullOrWhiteSpace(selectedStableId) && selectedStableId == StableIdFor(node))
                    continue;

                node.Metadata.TryGetValue("metadataGaps", out string gaps);
                if (string.IsNullOrWhiteSpace(gaps))
                    continue;

                projection.Rows.Add(new GuideRow
                {
                    Order = projection.Rows.Count + 1,
                    Role = "ContractMetadata",
                    OwnerId = node.Id,
                    Title = "Complete contract metadata",
                    Detail = gaps,
                    ActionKind = AuthoringActionKind.ReviewCode.ToString(),
                    ActionLabel = ActionLabel(AuthoringActionKind.ReviewCode.ToString()),
                    NativeAction = "Edit the script's AuthoringContract metadata inside the selected scripts folder.",
                    SuccessCheck = "The contract metadata gap no longer appears after scanning.",
                    BlocksProof = true
                });
            }
        }

        private static string ActionLabel(string actionKind)
        {
            if (string.IsNullOrWhiteSpace(actionKind) || actionKind == AuthoringActionKind.None.ToString())
                return string.Empty;

            AuthoringVocabularyDictionary vocabulary = AuthoringVocabulary.BuildDefault();
            return vocabulary.Label("action:" + actionKind, actionKind);
        }

        private static Dictionary<string, int> CountEdges(AuthoringGraph graph, params AuthoringGraphEdgeKind[] kinds)
        {
            Dictionary<string, int> counts = new Dictionary<string, int>();
            for (int i = 0; i < graph.Edges.Count; i++)
            {
                AuthoringGraphEdge edge = graph.Edges[i];
                if (!Contains(kinds, edge.Kind))
                    continue;

                counts.TryGetValue(edge.FromNodeId, out int count);
                counts[edge.FromNodeId] = count + 1;
            }

            return counts;
        }

        private static bool Contains(AuthoringGraphEdgeKind[] kinds, AuthoringGraphEdgeKind kind)
        {
            for (int i = 0; i < kinds.Length; i++)
            {
                if (kinds[i] == kind)
                    return true;
            }

            return false;
        }

        private static int CountNodes(AuthoringGraph graph, AuthoringGraphNodeKind kind)
        {
            int count = 0;
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                if (graph.Nodes[i].Kind == kind)
                    count++;
            }

            return count;
        }

        private static void AddFactRow(FactsProjection projection, AuthoringGraph graph, AuthoringGraphNode node)
        {
            if (projection == null || node == null)
                return;

            if (!ShouldProjectFact(node.Kind))
                return;

            node.Metadata.TryGetValue("sourcePath", out string sourcePath);
            if (string.IsNullOrWhiteSpace(sourcePath))
                node.Metadata.TryGetValue("assetPath", out sourcePath);

            projection.Rows.Add(new FactRow
            {
                Kind = node.Kind.ToString(),
                Label = node.Label,
                Detail = FactDetail(node),
                SourcePath = sourcePath ?? string.Empty,
                SourceCount = CountConnectedEdges(graph, node.Id),
                Confidence = FactConfidence(node.Kind)
            });
        }

        private static bool ShouldProjectFact(AuthoringGraphNodeKind kind)
        {
            return kind == AuthoringGraphNodeKind.Assembly
                || kind == AuthoringGraphNodeKind.Namespace
                || kind == AuthoringGraphNodeKind.Contract
                || kind == AuthoringGraphNodeKind.Validator
                || kind == AuthoringGraphNodeKind.Type
                || kind == AuthoringGraphNodeKind.Script
                || kind == AuthoringGraphNodeKind.Asset
                || kind == AuthoringGraphNodeKind.SceneObject
                || kind == AuthoringGraphNodeKind.Prefab
                || kind == AuthoringGraphNodeKind.Field
                || kind == AuthoringGraphNodeKind.Issue;
        }

        private static string FactDetail(AuthoringGraphNode node)
        {
            if (node == null)
                return string.Empty;

            if (node.Kind == AuthoringGraphNodeKind.Contract)
            {
                node.Metadata.TryGetValue("stableId", out string stableId);
                node.Metadata.TryGetValue("sourceType", out string sourceType);
                return "StableId: " + (stableId ?? string.Empty) + "; Source: " + (sourceType ?? string.Empty);
            }

            if (node.Kind == AuthoringGraphNodeKind.Validator)
                return "Validation provider";

            if (node.Kind == AuthoringGraphNodeKind.Assembly)
                return "Assembly definition";

            if (node.Kind == AuthoringGraphNodeKind.Namespace)
                return "Namespace dependency";

            node.Metadata.TryGetValue("kindLabel", out string kindLabel);
            return kindLabel ?? node.Kind.ToString();
        }

        private static string FactConfidence(AuthoringGraphNodeKind kind)
        {
            switch (kind)
            {
                case AuthoringGraphNodeKind.Contract:
                    return "ContractMetadata";
                case AuthoringGraphNodeKind.Validator:
                case AuthoringGraphNodeKind.Issue:
                    return "ValidationRecord";
                case AuthoringGraphNodeKind.SceneObject:
                case AuthoringGraphNodeKind.Prefab:
                case AuthoringGraphNodeKind.Asset:
                    return "UnityAssetDatabase";
                default:
                    return "ReflectionOrSource";
            }
        }

        private static int CountConnectedEdges(AuthoringGraph graph, string nodeId)
        {
            if (graph == null || string.IsNullOrWhiteSpace(nodeId))
                return 0;

            int count = 0;
            for (int i = 0; i < graph.Edges.Count; i++)
            {
                AuthoringGraphEdge edge = graph.Edges[i];
                if (edge.FromNodeId == nodeId || edge.ToNodeId == nodeId)
                    count++;
            }

            return count;
        }

        private static GuideRow FindFirstBlockingGuideRow(GuideProjection guide)
        {
            if (guide == null)
                return null;

            for (int i = 0; i < guide.Rows.Count; i++)
            {
                if (guide.Rows[i].BlocksProof)
                    return guide.Rows[i];
            }

            return null;
        }

        private static AuthoringGraphNode FindSelectedContract(AuthoringGraph graph, string selectedContractId)
        {
            if (graph == null || string.IsNullOrWhiteSpace(selectedContractId))
                return null;

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                AuthoringGraphNode node = graph.Nodes[i];
                if (node.Kind == AuthoringGraphNodeKind.Contract && node.Id == selectedContractId)
                    return node;
            }

            return null;
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
    }
}
