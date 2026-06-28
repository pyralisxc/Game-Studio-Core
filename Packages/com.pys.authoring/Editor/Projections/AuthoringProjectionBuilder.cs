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

            List<AuthoringGraphNode> intentCandidates = FindIntentCandidateContracts(graph);
            for (int i = 0; i < intentCandidates.Count; i++)
            {
                AuthoringGraphNode node = intentCandidates[i];
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
                    SourcePath = sourcePath ?? string.Empty,
                    OrganizationPattern = IntentOrganizationPattern(graph, node),
                    DependencyCount = ContractDependencyCount(node)
                });

                if (node.Id == projection.SelectedContractId)
                {
                    projection.SelectedDisplayName = node.Label;
                    projection.SelectedDisabledReason = disabledReason;
                }
            }

            return projection;
        }

        private static List<AuthoringGraphNode> FindIntentCandidateContracts(AuthoringGraph graph)
        {
            List<AuthoringGraphNode> contracts = ContractNodes(graph);
            List<AuthoringGraphNode> explicitGoals = new List<AuthoringGraphNode>();
            for (int i = 0; i < contracts.Count; i++)
            {
                if (IsExplicitIntentGoal(contracts[i]))
                    explicitGoals.Add(contracts[i]);
            }

            if (explicitGoals.Count > 0)
                return explicitGoals;

            List<AuthoringGraphNode> routeTerminals = new List<AuthoringGraphNode>();
            for (int i = 0; i < contracts.Count; i++)
            {
                AuthoringGraphNode contract = contracts[i];
                if (ContractDependencyCount(contract) == 0)
                    continue;

                if (IsContractPrerequisiteForAnother(graph, contract))
                    continue;

                routeTerminals.Add(contract);
            }

            if (routeTerminals.Count > 0)
                return routeTerminals;

            return contracts;
        }

        private static List<AuthoringGraphNode> ContractNodes(AuthoringGraph graph)
        {
            List<AuthoringGraphNode> contracts = new List<AuthoringGraphNode>();
            if (graph == null)
                return contracts;

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                AuthoringGraphNode node = graph.Nodes[i];
                if (node.Kind == AuthoringGraphNodeKind.Contract)
                    contracts.Add(node);
            }

            return contracts;
        }

        private static bool IsExplicitIntentGoal(AuthoringGraphNode contract)
        {
            if (contract == null)
                return false;

            string surface = Metadata(contract, "surface");
            if (surface == AuthoringSurface.Goal.ToString())
                return true;

            if (!string.IsNullOrWhiteSpace(Metadata(contract, "proofTarget")))
                return true;

            return !string.IsNullOrWhiteSpace(Metadata(contract, "successChecks"));
        }

        private static bool IsContractPrerequisiteForAnother(AuthoringGraph graph, AuthoringGraphNode contract)
        {
            if (graph == null || contract == null)
                return false;

            string stableId = StableIdFor(contract);
            if (string.IsNullOrWhiteSpace(stableId))
                return false;

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                AuthoringGraphNode node = graph.Nodes[i];
                if (node.Kind != AuthoringGraphNodeKind.Contract || node.Id == contract.Id)
                    continue;

                string[] prerequisites = MetadataList(node, "prerequisiteStableIds");
                for (int prerequisiteIndex = 0; prerequisiteIndex < prerequisites.Length; prerequisiteIndex++)
                {
                    if (prerequisites[prerequisiteIndex] == stableId)
                        return true;
                }
            }

            return false;
        }

        private static string IntentOrganizationPattern(AuthoringGraph graph, AuthoringGraphNode contract)
        {
            if (contract == null)
                return string.Empty;

            if (Metadata(contract, "surface") == AuthoringSurface.Goal.ToString())
                return "Goal surface";

            if (!string.IsNullOrWhiteSpace(Metadata(contract, "proofTarget")))
                return "Proof target";

            if (!string.IsNullOrWhiteSpace(Metadata(contract, "successChecks")))
                return "Success checks";

            if (ContractDependencyCount(contract) > 0 && !IsContractPrerequisiteForAnother(graph, contract))
                return "Route terminal";

            return "Selectable contract";
        }

        private static int ContractDependencyCount(AuthoringGraphNode contract)
        {
            return MetadataList(contract, "prerequisiteStableIds").Length;
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
            if (guide != null && guide.SelectedDisplayName == "No intent selected")
            {
                projection.Summary = "No intent selected.";
                projection.NextAction = "Select an Intent contract to build a Guide path.";
                projection.Reason = "No intent selected";
                projection.Readiness = "No intent selected";
                return projection;
            }

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
            if (selectedContract == null)
            {
                projection.SelectedDisplayName = "No intent selected";
                projection.ProofTarget = string.Empty;
                projection.ProofReady = false;
                return projection;
            }

            List<AuthoringGraphNode> routeContracts = BuildDependencyClosure(graph, selectedContract);
            AddRouteContractRows(graph, routeContracts, selectedContract, projection);
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

        private static List<AuthoringGraphNode> BuildDependencyClosure(AuthoringGraph graph, AuthoringGraphNode selectedContract)
        {
            List<AuthoringGraphNode> closure = new List<AuthoringGraphNode>();
            HashSet<string> visiting = new HashSet<string>();
            HashSet<string> visited = new HashSet<string>();
            AddDependencyClosureNode(graph, selectedContract, closure, visiting, visited);
            closure.Sort(CompareRouteContracts);
            return closure;
        }

        private static void AddDependencyClosureNode(
            AuthoringGraph graph,
            AuthoringGraphNode contract,
            List<AuthoringGraphNode> closure,
            HashSet<string> visiting,
            HashSet<string> visited)
        {
            if (graph == null || contract == null || visited.Contains(contract.Id) || visiting.Contains(contract.Id))
                return;

            visiting.Add(contract.Id);
            string[] prerequisiteStableIds = MetadataList(contract, "prerequisiteStableIds");
            for (int i = 0; i < prerequisiteStableIds.Length; i++)
            {
                List<AuthoringGraphNode> prerequisites = FindContractsByStableId(graph, prerequisiteStableIds[i]);
                for (int prerequisiteIndex = 0; prerequisiteIndex < prerequisites.Count; prerequisiteIndex++)
                    AddDependencyClosureNode(graph, prerequisites[prerequisiteIndex], closure, visiting, visited);
            }

            visiting.Remove(contract.Id);
            visited.Add(contract.Id);
            closure.Add(contract);
        }

        private static int CompareRouteContracts(AuthoringGraphNode left, AuthoringGraphNode right)
        {
            int leftOrder = MetadataInt(left, "routeOrder");
            int rightOrder = MetadataInt(right, "routeOrder");
            if (leftOrder != rightOrder)
                return leftOrder.CompareTo(rightOrder);

            string leftStage = Metadata(left, "routeStage");
            string rightStage = Metadata(right, "routeStage");
            int stageComparison = string.CompareOrdinal(leftStage, rightStage);
            if (stageComparison != 0)
                return stageComparison;

            return string.CompareOrdinal(left != null ? left.Label : string.Empty, right != null ? right.Label : string.Empty);
        }

        private static void AddRouteContractRows(
            AuthoringGraph graph,
            List<AuthoringGraphNode> routeContracts,
            AuthoringGraphNode selectedContract,
            GuideProjection projection)
        {
            if (projection == null || routeContracts == null || selectedContract == null)
                return;

            projection.SelectedDisplayName = selectedContract.Label;
            projection.ProofTarget = FirstNonEmpty(Metadata(selectedContract, "proofTarget"), selectedContract.Label);
            HashSet<string> addedIssueIds = new HashSet<string>();

            for (int i = 0; i < routeContracts.Count; i++)
            {
                AuthoringGraphNode contract = routeContracts[i];
                AddContractIdentityAndMetadataRows(contract, projection);
                AddIssueRowsForContract(graph, routeContracts, contract, projection, addedIssueIds);
                AddContractSetupStepRows(contract, projection);
            }

            AddContractProofCheckRows(selectedContract, projection);
        }

        private static void AddContractIdentityAndMetadataRows(AuthoringGraphNode contract, GuideProjection projection)
        {
            string duplicateStableId = Metadata(contract, "duplicateStableId");
            string stableId = StableIdFor(contract);
            string sourceType = Metadata(contract, "sourceType");
            string sourcePath = Metadata(contract, "sourcePath");

            if (duplicateStableId == "true")
            {
                AddGuideRow(projection, contract, "ContractIdentity", "Resolve duplicate StableId", stableId, AuthoringActionKind.ReviewCode.ToString(), "Give this contract a StableId that is unique inside the selected scripts folder.", "Only one contract reports this StableId after scanning. Source: " + sourceType + " " + sourcePath, true);
            }

            string gaps = Metadata(contract, "metadataGaps");
            if (!string.IsNullOrWhiteSpace(gaps))
            {
                AddGuideRow(projection, contract, "ContractMetadata", "Complete contract metadata", gaps, AuthoringActionKind.ReviewCode.ToString(), "Edit the contract metadata inside the selected scripts folder.", "The contract metadata gap no longer appears after scanning.", true);
            }
        }

        private static void AddIssueRowsForContract(
            AuthoringGraph graph,
            List<AuthoringGraphNode> routeContracts,
            AuthoringGraphNode contract,
            GuideProjection projection,
            HashSet<string> addedIssueIds)
        {
            if (graph == null || contract == null)
                return;

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                AuthoringGraphNode issue = graph.Nodes[i];
                if (issue.Kind != AuthoringGraphNodeKind.Issue || addedIssueIds.Contains(issue.Id))
                    continue;

                if (!IssueBelongsToContract(graph, routeContracts, contract, issue))
                    continue;

                addedIssueIds.Add(issue.Id);
                string actionKind = Metadata(issue, "actionKind");
                AddGuideRow(
                    projection,
                    issue,
                    "Issue",
                    issue.Label,
                    Metadata(issue, "issueCode"),
                    actionKind,
                    Metadata(issue, "nativeAction"),
                    Metadata(issue, "successCheck"),
                    true,
                    contract);
            }
        }

        private static bool IssueBelongsToContract(
            AuthoringGraph graph,
            List<AuthoringGraphNode> routeContracts,
            AuthoringGraphNode contract,
            AuthoringGraphNode issue)
        {
            if (HasEdge(graph, contract.Id, issue.Id, AuthoringGraphEdgeKind.ValidatorReports))
                return true;

            string contractStableId = StableIdFor(contract);
            string ownerStableId = Metadata(issue, "ownerStableId");
            if (!string.IsNullOrWhiteSpace(ownerStableId) && ownerStableId == contractStableId)
                return true;

            string[] relatedStableIds = MetadataList(issue, "relatedStableIds");
            for (int i = 0; i < relatedStableIds.Length; i++)
            {
                if (relatedStableIds[i] == contractStableId)
                    return true;
            }

            return false;
        }

        private static void AddContractSetupStepRows(AuthoringGraphNode contract, GuideProjection projection)
        {
            string actionKind = Metadata(contract, "actionKind");
            if (string.IsNullOrWhiteSpace(actionKind) || actionKind == AuthoringActionKind.None.ToString())
                actionKind = AuthoringActionKind.InspectObject.ToString();

            string setupSteps = Metadata(contract, "setupSteps");
            if (string.IsNullOrWhiteSpace(setupSteps))
                return;

            string[] split = setupSteps.Split('\n');
            for (int i = 0; i < split.Length; i++)
            {
                string line = split[i].Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                AddGuideRow(projection, contract, "SetupStep", "Complete setup step", line, actionKind, line, "The setup step is represented by validation evidence after scanning.", true);
            }
        }

        private static void AddContractProofCheckRows(AuthoringGraphNode contract, GuideProjection projection)
        {
            string successChecks = Metadata(contract, "successChecks");
            if (string.IsNullOrWhiteSpace(successChecks))
                return;

            string[] split = successChecks.Split('\n');
            for (int i = 0; i < split.Length; i++)
            {
                string line = split[i].Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                AddGuideRow(projection, contract, "ProofCheck", "Verify proof target", line, AuthoringActionKind.RunPlayModeCheck.ToString(), line, line, false);
            }
        }

        private static void AddGuideRow(
            GuideProjection projection,
            AuthoringGraphNode owner,
            string role,
            string title,
            string detail,
            string actionKind,
            string nativeAction,
            string successCheck,
            bool blocksProof,
            AuthoringGraphNode routeContract = null)
        {
            AuthoringGraphNode routeSource = routeContract ?? owner;
            string normalizedActionKind = string.IsNullOrWhiteSpace(actionKind) ? string.Empty : actionKind;
            projection.Rows.Add(new GuideRow
            {
                Order = projection.Rows.Count + 1,
                Role = role ?? string.Empty,
                OwnerId = owner != null ? owner.Id : string.Empty,
                Title = title ?? string.Empty,
                Detail = detail ?? string.Empty,
                ActionKind = normalizedActionKind,
                ActionLabel = ActionLabel(normalizedActionKind),
                NativeAction = nativeAction ?? string.Empty,
                SuccessCheck = successCheck ?? string.Empty,
                BlocksProof = blocksProof,
                StableId = StableIdFor(routeSource),
                RouteStage = Metadata(routeSource, "routeStage"),
                RouteOrder = MetadataInt(routeSource, "routeOrder"),
                SetupDomain = Metadata(routeSource, "setupDomain")
            });
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

        private static List<AuthoringGraphNode> FindContractsByStableId(AuthoringGraph graph, string stableId)
        {
            List<AuthoringGraphNode> contracts = new List<AuthoringGraphNode>();
            if (graph == null || string.IsNullOrWhiteSpace(stableId))
                return contracts;

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                AuthoringGraphNode node = graph.Nodes[i];
                if (node.Kind == AuthoringGraphNodeKind.Contract && StableIdFor(node) == stableId)
                    contracts.Add(node);
            }

            return contracts;
        }

        private static bool HasEdge(AuthoringGraph graph, string fromNodeId, string toNodeId, AuthoringGraphEdgeKind kind)
        {
            if (graph == null)
                return false;

            for (int i = 0; i < graph.Edges.Count; i++)
            {
                AuthoringGraphEdge edge = graph.Edges[i];
                if (edge.FromNodeId == fromNodeId && edge.ToNodeId == toNodeId && edge.Kind == kind)
                    return true;
            }

            return false;
        }

        private static string Metadata(AuthoringGraphNode node, string key)
        {
            if (node == null || string.IsNullOrWhiteSpace(key))
                return string.Empty;

            return node.Metadata.TryGetValue(key, out string value) ? value ?? string.Empty : string.Empty;
        }

        private static int MetadataInt(AuthoringGraphNode node, string key)
        {
            string value = Metadata(node, key);
            return int.TryParse(value, out int parsed) ? parsed : 0;
        }

        private static string[] MetadataList(AuthoringGraphNode node, string key)
        {
            string value = Metadata(node, key);
            if (string.IsNullOrWhiteSpace(value))
                return new string[0];

            string[] raw = value.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < raw.Length; i++)
                raw[i] = raw[i].Trim();

            return raw;
        }

        private static string FirstNonEmpty(string preferred, string fallback)
        {
            return !string.IsNullOrWhiteSpace(preferred) ? preferred : fallback ?? string.Empty;
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
