using System;
using System.Linq;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    public static class PyralisAuthoringSetupGraphJsonExporter
    {
        public static string ToJson(PyralisAuthoringSetupGraph graph, string view)
        {
            GraphSnapshot snapshot = BuildSnapshot(graph, view);
            return JsonUtility.ToJson(snapshot, true);
        }

        private static GraphSnapshot BuildSnapshot(PyralisAuthoringSetupGraph graph, string view)
        {
            return new GraphSnapshot
            {
                schema = "pyralis.authoring.setupGraph.snapshot.v1",
                purpose = "Read-only authoring graph diagnostic snapshot. Do not treat this JSON as setup truth or generated content.",
                view = string.IsNullOrWhiteSpace(view) ? "Graph" : view,
                routeName = graph != null ? graph.RouteName : "No setup route selected",
                exportedAtUtc = DateTime.UtcNow.ToString("o"),
                source = BuildSourceInfo(graph?.Source),
                nodeCount = graph?.Nodes.Count ?? 0,
                edgeCount = graph?.Edges.Count ?? 0,
                nodes = graph?.Nodes.Select(BuildNode).ToArray() ?? Array.Empty<NodeSnapshot>(),
                edges = graph?.Edges.Select(BuildEdge).ToArray() ?? Array.Empty<EdgeSnapshot>(),
                mapRows = PyralisAuthoringSetupGraphProjection.BuildSetupMapRows(graph)
                    .Select(BuildMapRow)
                    .ToArray(),
                mapConnections = PyralisAuthoringSetupGraphProjection.BuildMapConnectionRows(graph)
                    .Select(BuildConnection)
                    .ToArray(),
                hygieneSections = PyralisAuthoringSetupGraphProjection.BuildHygieneSections(graph)
                    .Select(BuildHygieneSection)
                    .ToArray(),
                hygieneRows = PyralisAuthoringSetupGraphProjection.BuildHygieneDetailRows(graph)
                    .Select(BuildHygieneRow)
                    .ToArray()
            };
        }

        private static NodeSnapshot BuildNode(PyralisAuthoringGraphNode node)
        {
            return new NodeSnapshot
            {
                id = node.StableId,
                label = node.Label,
                kind = node.Kind.ToString(),
                sourceKind = node.SourceKind.ToString(),
                sourceOrigin = node.SourceOrigin.ToString(),
                evidenceState = node.EvidenceState.ToString(),
                workIntent = node.WorkIntent.ToString(),
                issueSeverity = node.IssueSeverity.ToString(),
                capabilityFamily = node.CapabilityFamily.ToString(),
                authoringCapability = node.AuthoringCapability.ToString(),
                proofTargetId = node.ProofTargetId,
                guidance = node.Guidance,
                blockingReason = node.BlockingReason,
                nativeSetup = node.NativeSetup,
                assignmentFields = node.AssignmentFields,
                customizationMoments = node.CustomizationMoments,
                nativeAction = BuildNativeAction(node.NativeAction),
                sourceObject = BuildSourceInfo(node.SourceObject),
                sourceContract = BuildContract(node.SourceContract)
            };
        }

        private static EdgeSnapshot BuildEdge(PyralisAuthoringGraphEdge edge)
        {
            return new EdgeSnapshot
            {
                from = edge.FromNodeId,
                to = edge.ToNodeId,
                kind = edge.Kind.ToString(),
                label = edge.Label
            };
        }

        private static MapRowSnapshot BuildMapRow(PyralisAuthoringSetupGraphRow row)
        {
            return new MapRowSnapshot
            {
                label = row.Label,
                nodeId = row.Node != null ? row.Node.StableId : string.Empty,
                evidenceState = row.EffectiveEvidenceState.ToString(),
                isReady = row.IsReady,
                isMissing = row.IsMissing,
                isOptional = row.IsOptional,
                message = row.Message,
                target = BuildSourceInfo(row.Target)
            };
        }

        private static ConnectionSnapshot BuildConnection(PyralisAuthoringGraphConnectionRow row)
        {
            return new ConnectionSnapshot
            {
                from = row.FromLabel,
                to = row.ToLabel,
                relationship = row.Relationship,
                detail = row.Detail
            };
        }

        private static HygieneSectionSnapshot BuildHygieneSection(PyralisAuthoringGraphAuditSection section)
        {
            return new HygieneSectionSnapshot
            {
                label = section.Label,
                evidenceState = section.EvidenceState.ToString(),
                rows = section.Rows.Select(BuildHygieneRow).ToArray()
            };
        }

        private static HygieneRowSnapshot BuildHygieneRow(PyralisAuthoringGraphAuditRow row)
        {
            return new HygieneRowSnapshot
            {
                nodeId = row.NodeId,
                label = row.Label,
                evidenceState = row.EvidenceState.ToString(),
                source = row.SourceLabel,
                origin = row.OriginLabel,
                message = row.Message,
                nativeAction = row.NativeAction,
                canInspectTarget = row.CanInspectTarget,
                target = BuildSourceInfo(row.Target)
            };
        }

        private static NativeActionSnapshot BuildNativeAction(PyralisAuthoringNativeAction? nativeAction)
        {
            if (!nativeAction.HasValue)
                return null;

            PyralisAuthoringNativeAction action = nativeAction.Value;
            return new NativeActionSnapshot
            {
                verb = action.Verb,
                surface = action.Surface.ToString(),
                target = action.Target,
                fieldOrComponent = action.FieldOrComponent,
                successCheck = action.SuccessCheck,
                guidance = action.ToGuidanceSentence()
            };
        }

        private static ContractSnapshot BuildContract(ResolvedAuthoringContract contract)
        {
            if (contract == null)
                return null;

            return new ContractSnapshot
            {
                stableId = contract.StableId,
                displayName = contract.DisplayName,
                category = contract.AuthoringCategory,
                moduleId = contract.ModuleId,
                setupNodeId = contract.SetupNodeId,
                capability = contract.Capability.ToString(),
                confidence = contract.Confidence.ToString(),
                sourceType = contract.SourceType != null ? contract.SourceType.FullName : string.Empty
            };
        }

        private static SourceSnapshot BuildSourceInfo(Object source)
        {
            if (source == null)
                return null;

            return new SourceSnapshot
            {
                name = source.name,
                type = source.GetType().FullName,
                assetPath = AssetDatabase.GetAssetPath(source),
                globalObjectId = GetGlobalObjectId(source)
            };
        }

        private static string GetGlobalObjectId(Object source)
        {
            try
            {
                return GlobalObjectId.GetGlobalObjectIdSlow(source).ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        [Serializable]
        private sealed class GraphSnapshot
        {
            public string schema;
            public string purpose;
            public string view;
            public string routeName;
            public string exportedAtUtc;
            public SourceSnapshot source;
            public int nodeCount;
            public int edgeCount;
            public NodeSnapshot[] nodes;
            public EdgeSnapshot[] edges;
            public MapRowSnapshot[] mapRows;
            public ConnectionSnapshot[] mapConnections;
            public HygieneSectionSnapshot[] hygieneSections;
            public HygieneRowSnapshot[] hygieneRows;
        }

        [Serializable]
        private sealed class NodeSnapshot
        {
            public string id;
            public string label;
            public string kind;
            public string sourceKind;
            public string sourceOrigin;
            public string evidenceState;
            public string workIntent;
            public string issueSeverity;
            public string capabilityFamily;
            public string authoringCapability;
            public string proofTargetId;
            public string guidance;
            public string blockingReason;
            public string[] nativeSetup;
            public string[] assignmentFields;
            public string[] customizationMoments;
            public NativeActionSnapshot nativeAction;
            public SourceSnapshot sourceObject;
            public ContractSnapshot sourceContract;
        }

        [Serializable]
        private sealed class EdgeSnapshot
        {
            public string from;
            public string to;
            public string kind;
            public string label;
        }

        [Serializable]
        private sealed class MapRowSnapshot
        {
            public string label;
            public string nodeId;
            public string evidenceState;
            public bool isReady;
            public bool isMissing;
            public bool isOptional;
            public string message;
            public SourceSnapshot target;
        }

        [Serializable]
        private sealed class ConnectionSnapshot
        {
            public string from;
            public string to;
            public string relationship;
            public string detail;
        }

        [Serializable]
        private sealed class HygieneSectionSnapshot
        {
            public string label;
            public string evidenceState;
            public HygieneRowSnapshot[] rows;
        }

        [Serializable]
        private sealed class HygieneRowSnapshot
        {
            public string nodeId;
            public string label;
            public string evidenceState;
            public string source;
            public string origin;
            public string message;
            public string nativeAction;
            public bool canInspectTarget;
            public SourceSnapshot target;
        }

        [Serializable]
        private sealed class NativeActionSnapshot
        {
            public string verb;
            public string surface;
            public string target;
            public string fieldOrComponent;
            public string successCheck;
            public string guidance;
        }

        [Serializable]
        private sealed class SourceSnapshot
        {
            public string name;
            public string type;
            public string assetPath;
            public string globalObjectId;
        }

        [Serializable]
        private sealed class ContractSnapshot
        {
            public string stableId;
            public string displayName;
            public string category;
            public string moduleId;
            public string setupNodeId;
            public string capability;
            public string confidence;
            public string sourceType;
        }
    }
}
