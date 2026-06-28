using System.Collections.Generic;
using Pys.Authoring.Contracts;
using Pys.Authoring.Editor.Scanning;
using Pys.Authoring.Editor.Vocabulary;

namespace Pys.Authoring.Editor.Hygiene
{
    public static class DependencyGraphProjection
    {
        public static AuthoringGraph Build(UnityCodebaseScanResult scanResult)
        {
            AuthoringGraph graph = new AuthoringGraph();
            AuthoringVocabularyDictionary vocabulary = AuthoringVocabulary.BuildDefault();
            if (scanResult == null)
                return graph;

            AddTypeObservations(graph, scanResult.Types, vocabulary);
            AddAssemblyDefinitions(graph, scanResult.AssemblyDefinitions, vocabulary);
            AddSourceDependencies(graph, scanResult.SourceDependencies, vocabulary);
            AddUnityObjects(graph, scanResult.SceneObjects, AuthoringGraphNodeKind.SceneObject, AuthoringGraphEdgeKind.SceneContains, vocabulary);
            AddUnityObjects(graph, scanResult.Prefabs, AuthoringGraphNodeKind.Prefab, AuthoringGraphEdgeKind.PrefabContains, vocabulary);
            AddAssets(graph, scanResult.Assets, vocabulary);
            return graph;
        }

        public static AuthoringGraph BuildTypeObservationGraph(IReadOnlyList<UnityTypeObservation> observations)
        {
            AuthoringGraph graph = new AuthoringGraph();
            AddTypeObservations(graph, observations, AuthoringVocabulary.BuildDefault());
            return graph;
        }

        private static void AddTypeObservations(
            AuthoringGraph graph,
            IReadOnlyList<UnityTypeObservation> observations,
            AuthoringVocabularyDictionary vocabulary)
        {
            if (observations == null)
                return;

            Dictionary<string, int> stableIdCounts = CountContractStableIds(observations);
            Dictionary<string, int> stableIdIndexes = new Dictionary<string, int>();

            for (int i = 0; i < observations.Count; i++)
            {
                UnityTypeObservation observation = observations[i];
                if (observation == null)
                    continue;

                string typeId = "type:" + observation.FullName;
                AuthoringGraphNode typeNode = GetOrAddNode(graph, typeId, observation.DisplayName, AuthoringGraphNodeKind.Type, vocabulary);
                typeNode.Metadata["assembly"] = observation.AssemblyName;
                typeNode.Metadata["assetPath"] = observation.AssetPath;
                typeNode.Metadata["inScope"] = "true";
                if (observation.ImplementsAuthoringValidationProvider)
                    typeNode.Metadata["validationProvider"] = "true";

                AddEdges(graph, typeId, observation.ImplementedInterfaces, "interface:", AuthoringGraphNodeKind.Type, AuthoringGraphEdgeKind.Implements, vocabulary);
                AddEdges(graph, typeId, observation.SerializedFields, "field:" + observation.FullName + ".", AuthoringGraphNodeKind.Field, AuthoringGraphEdgeKind.SerializedField, vocabulary);
                AddEdges(graph, typeId, observation.RequiredComponents, "component:", AuthoringGraphNodeKind.Component, AuthoringGraphEdgeKind.RequiredComponent, vocabulary);
                AddContracts(graph, typeId, observation.AssetPath, observation.Contracts, stableIdCounts, stableIdIndexes, vocabulary);

                if (observation.ImplementsAuthoringValidationProvider)
                {
                    string validatorId = "validator:" + observation.FullName;
                    GetOrAddNode(graph, validatorId, observation.DisplayName, AuthoringGraphNodeKind.Validator, vocabulary);
                    graph.Edges.Add(new AuthoringGraphEdge(typeId, validatorId, AuthoringGraphEdgeKind.Implements));
                }
            }
        }

        private static void AddAssemblyDefinitions(
            AuthoringGraph graph,
            IReadOnlyList<AssemblyDefinitionObservation> observations,
            AuthoringVocabularyDictionary vocabulary)
        {
            if (observations == null)
                return;

            for (int i = 0; i < observations.Count; i++)
            {
                AssemblyDefinitionObservation observation = observations[i];
                if (observation == null)
                    continue;

                string assemblyId = "assembly:" + observation.Name;
                AuthoringGraphNode assemblyNode = GetOrAddNode(graph, assemblyId, observation.Name, AuthoringGraphNodeKind.Assembly, vocabulary);
                assemblyNode.Metadata["assetPath"] = observation.AssetPath;

                AddEdges(graph, assemblyId, observation.References, "assembly:", AuthoringGraphNodeKind.Assembly, AuthoringGraphEdgeKind.AssemblyReference, vocabulary);
            }
        }

        private static void AddSourceDependencies(
            AuthoringGraph graph,
            IReadOnlyList<SourceDependencyObservation> observations,
            AuthoringVocabularyDictionary vocabulary)
        {
            if (observations == null)
                return;

            for (int i = 0; i < observations.Count; i++)
            {
                SourceDependencyObservation observation = observations[i];
                if (observation == null)
                    continue;

                string scriptId = "script:" + observation.AssetPath;
                GetOrAddNode(graph, scriptId, observation.AssetPath, AuthoringGraphNodeKind.Script, vocabulary);

                AddEdges(graph, scriptId, observation.Namespaces, "namespace:", AuthoringGraphNodeKind.Namespace, AuthoringGraphEdgeKind.NamespaceUsing, vocabulary);
            }
        }

        private static void AddUnityObjects(
            AuthoringGraph graph,
            IReadOnlyList<UnityObjectObservation> observations,
            AuthoringGraphNodeKind objectKind,
            AuthoringGraphEdgeKind componentEdgeKind,
            AuthoringVocabularyDictionary vocabulary)
        {
            if (observations == null)
                return;

            for (int i = 0; i < observations.Count; i++)
            {
                UnityObjectObservation observation = observations[i];
                if (observation == null)
                    continue;

                AuthoringGraphNode objectNode = GetOrAddNode(graph, observation.ObjectId, observation.Label, objectKind, vocabulary);
                objectNode.Metadata["sourcePath"] = observation.SourcePath;
                objectNode.Metadata["type"] = observation.TypeName;

                AddEdges(graph, observation.ObjectId, observation.Components, "component:", AuthoringGraphNodeKind.Component, componentEdgeKind, vocabulary);
                AddIssues(graph, observation.ObjectId, observation.Issues, vocabulary);
            }
        }

        private static void AddAssets(
            AuthoringGraph graph,
            IReadOnlyList<UnityAssetObservation> observations,
            AuthoringVocabularyDictionary vocabulary)
        {
            if (observations == null)
                return;

            for (int i = 0; i < observations.Count; i++)
            {
                UnityAssetObservation observation = observations[i];
                if (observation == null)
                    continue;

                AuthoringGraphNode assetNode = GetOrAddNode(graph, observation.ObjectId, observation.Label, AuthoringGraphNodeKind.Asset, vocabulary);
                assetNode.Metadata["sourcePath"] = observation.SourcePath;
                assetNode.Metadata["type"] = observation.TypeName;
            }
        }

        private static void AddIssues(
            AuthoringGraph graph,
            string ownerId,
            IReadOnlyList<AuthoringIssue> issues,
            AuthoringVocabularyDictionary vocabulary)
        {
            if (issues == null)
                return;

            for (int i = 0; i < issues.Count; i++)
            {
                AuthoringIssue issue = issues[i];
                if (issue == null)
                    continue;

                string issueCode = string.IsNullOrWhiteSpace(issue.IssueCode) ? "Validation.Issue" : issue.IssueCode;
                string issueId = "issue:" + ownerId + ":" + issueCode + ":" + i;
                AuthoringGraphNode issueNode = GetOrAddNode(graph, issueId, issue.Message, AuthoringGraphNodeKind.Issue, vocabulary);
                issueNode.Metadata["issueCode"] = issueCode;
                issueNode.Metadata["fieldPath"] = issue.FieldPath;
                issueNode.Metadata["targetLabel"] = issue.TargetLabel;
                issueNode.Metadata["nativeAction"] = issue.NativeAction;
                issueNode.Metadata["successCheck"] = issue.SuccessCheck;
                issueNode.Metadata["severity"] = issue.Severity.ToString();
                issueNode.Metadata["actionKind"] = issue.ActionKind.ToString();
                graph.Edges.Add(new AuthoringGraphEdge(ownerId, issueId, AuthoringGraphEdgeKind.ValidatorReports));
            }
        }

        private static void AddContracts(
            AuthoringGraph graph,
            string typeId,
            string sourcePath,
            IReadOnlyList<ResolvedAuthoringContract> contracts,
            Dictionary<string, int> stableIdCounts,
            Dictionary<string, int> stableIdIndexes,
            AuthoringVocabularyDictionary vocabulary)
        {
            if (contracts == null)
                return;

            for (int i = 0; i < contracts.Count; i++)
            {
                ResolvedAuthoringContract contract = contracts[i];
                if (contract == null)
                    continue;

                string contractId = ContractNodeId(contract, stableIdCounts, stableIdIndexes);
                AuthoringGraphNode contractNode = GetOrAddNode(graph, contractId, contract.DisplayName, AuthoringGraphNodeKind.Contract, vocabulary);
                contractNode.Metadata["stableId"] = contract.StableId;
                contractNode.Metadata["sourceType"] = contract.SourceTypeName;
                contractNode.Metadata["sourcePath"] = sourcePath ?? string.Empty;
                contractNode.Metadata["category"] = contract.Category;
                contractNode.Metadata["capabilityPath"] = contract.CapabilityPath;
                contractNode.Metadata["surface"] = contract.Surface.ToString();
                contractNode.Metadata["selectable"] = contract.Selectable ? "true" : "false";
                contractNode.Metadata["summary"] = contract.Summary;
                contractNode.Metadata["metadataGaps"] = string.Join(",", contract.MetadataGaps);
                contractNode.Metadata["setupSteps"] = string.Join("\n", contract.SetupSteps);
                contractNode.Metadata["successChecks"] = string.Join("\n", contract.SuccessChecks);
                contractNode.Metadata["roleTags"] = string.Join(",", contract.RoleTags);
                contractNode.Metadata["tags"] = string.Join(",", contract.Tags);
                if (IsDuplicateStableId(contract.StableId, stableIdCounts))
                    contractNode.Metadata["duplicateStableId"] = "true";

                graph.Edges.Add(new AuthoringGraphEdge(typeId, contractId, AuthoringGraphEdgeKind.ContractDeclares));

                AddEdges(graph, contractId, contract.RequiredFields, "field:" + contract.SourceTypeName + ".", AuthoringGraphNodeKind.Field, AuthoringGraphEdgeKind.SerializedField, vocabulary);
                AddEdges(graph, contractId, contract.RequiredComponents, "component:", AuthoringGraphNodeKind.Component, AuthoringGraphEdgeKind.RequiredComponent, vocabulary);
                AddEdges(graph, contractId, contract.RequiredInterfaces, "interface:", AuthoringGraphNodeKind.Type, AuthoringGraphEdgeKind.Implements, vocabulary);
                AddMetadataGapIssues(graph, contractId, contract.MetadataGaps, vocabulary);
            }
        }

        private static Dictionary<string, int> CountContractStableIds(IReadOnlyList<UnityTypeObservation> observations)
        {
            Dictionary<string, int> counts = new Dictionary<string, int>();
            if (observations == null)
                return counts;

            for (int i = 0; i < observations.Count; i++)
            {
                UnityTypeObservation observation = observations[i];
                if (observation == null || observation.Contracts == null)
                    continue;

                for (int contractIndex = 0; contractIndex < observation.Contracts.Count; contractIndex++)
                {
                    ResolvedAuthoringContract contract = observation.Contracts[contractIndex];
                    if (contract == null || string.IsNullOrWhiteSpace(contract.StableId))
                        continue;

                    counts.TryGetValue(contract.StableId, out int count);
                    counts[contract.StableId] = count + 1;
                }
            }

            return counts;
        }

        private static string ContractNodeId(
            ResolvedAuthoringContract contract,
            Dictionary<string, int> stableIdCounts,
            Dictionary<string, int> stableIdIndexes)
        {
            string stableId = contract != null ? contract.StableId : string.Empty;
            if (string.IsNullOrWhiteSpace(stableId))
                stableId = "unknown";

            if (!IsDuplicateStableId(stableId, stableIdCounts))
                return "contract:" + stableId;

            stableIdIndexes.TryGetValue(stableId, out int index);
            index++;
            stableIdIndexes[stableId] = index;

            string sourceType = contract != null ? contract.SourceTypeName : string.Empty;
            return "contract:" + stableId + "@" + SanitizeNodeIdPart(sourceType) + "#" + index;
        }

        private static bool IsDuplicateStableId(string stableId, Dictionary<string, int> stableIdCounts)
        {
            return !string.IsNullOrWhiteSpace(stableId)
                && stableIdCounts != null
                && stableIdCounts.TryGetValue(stableId, out int count)
                && count > 1;
        }

        private static string SanitizeNodeIdPart(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "unknown";

            return value.Trim()
                .Replace(' ', '_')
                .Replace('/', '.')
                .Replace('\\', '.')
                .Replace(':', '.');
        }

        private static void AddMetadataGapIssues(
            AuthoringGraph graph,
            string contractId,
            IReadOnlyList<string> gaps,
            AuthoringVocabularyDictionary vocabulary)
        {
            if (gaps == null)
                return;

            for (int i = 0; i < gaps.Count; i++)
            {
                string gap = gaps[i];
                if (string.IsNullOrWhiteSpace(gap))
                    continue;

                string issueId = "issue:" + contractId + ":missing-" + gap;
                AuthoringGraphNode issueNode = GetOrAddNode(graph, issueId, "Missing " + gap, AuthoringGraphNodeKind.Issue, vocabulary);
                issueNode.Metadata["issueCode"] = "Contract.Metadata.Missing";
                issueNode.Metadata["field"] = gap;
                issueNode.Metadata["severity"] = "Warning";
                issueNode.Metadata["actionKind"] = AuthoringActionKind.ReviewCode.ToString();
                graph.Edges.Add(new AuthoringGraphEdge(contractId, issueId, AuthoringGraphEdgeKind.ValidatorReports));
            }
        }

        private static void AddEdges(
            AuthoringGraph graph,
            string fromNodeId,
            IEnumerable<string> values,
            string nodePrefix,
            AuthoringGraphNodeKind nodeKind,
            AuthoringGraphEdgeKind edgeKind,
            AuthoringVocabularyDictionary vocabulary)
        {
            if (values == null)
                return;

            foreach (string value in values)
            {
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                string nodeId = nodePrefix + value;
                GetOrAddNode(graph, nodeId, value, nodeKind, vocabulary);
                graph.Edges.Add(new AuthoringGraphEdge(fromNodeId, nodeId, edgeKind));
            }
        }

        private static AuthoringGraphNode GetOrAddNode(
            AuthoringGraph graph,
            string nodeId,
            string fallbackLabel,
            AuthoringGraphNodeKind kind,
            AuthoringVocabularyDictionary vocabulary)
        {
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                if (graph.Nodes[i].Id == nodeId)
                    return graph.Nodes[i];
            }

            string label = vocabulary != null
                ? vocabulary.Label(AuthoringVocabularyKey.Node(kind), fallbackLabel)
                : fallbackLabel;

            if (!string.IsNullOrWhiteSpace(fallbackLabel))
                label = fallbackLabel;

            AuthoringGraphNode node = new AuthoringGraphNode(nodeId, label, kind);
            node.Metadata["kindLabel"] = vocabulary != null ? vocabulary.Label(AuthoringVocabularyKey.Node(kind), kind.ToString()) : kind.ToString();
            graph.Nodes.Add(node);
            return node;
        }
    }
}
