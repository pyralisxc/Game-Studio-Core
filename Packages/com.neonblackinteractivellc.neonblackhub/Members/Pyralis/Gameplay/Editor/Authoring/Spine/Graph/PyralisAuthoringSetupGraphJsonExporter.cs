using System;
using System.Collections.Generic;
using System.Linq;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    public static class PyralisAuthoringSetupGraphJsonExporter
    {
        public static string ToMapJson(PyralisAuthoringSetupGraph graph)
        {
            MapSnapshot snapshot = BuildMapSnapshot(graph);
            return JsonUtility.ToJson(snapshot, true);
        }

        public static string ToHygieneJson(
            PyralisAuthoringSetupGraph graph,
            IReadOnlyList<PyralisSourceDependencyHygieneRecord> dependencyRecords)
        {
            HygieneSnapshot snapshot = BuildHygieneSnapshot(graph, dependencyRecords);
            return JsonUtility.ToJson(snapshot, true);
        }

        public static string ToJson(PyralisAuthoringSetupGraph graph, string view)
        {
            return string.Equals(view, "Hygiene", StringComparison.OrdinalIgnoreCase)
                ? ToHygieneJson(graph, Array.Empty<PyralisSourceDependencyHygieneRecord>())
                : ToMapJson(graph);
        }

        private static MapSnapshot BuildMapSnapshot(PyralisAuthoringSetupGraph graph)
        {
            return new MapSnapshot
            {
                schema = "pyralis.authoring.mapSnapshot.v1",
                purpose = "Read-only Map tab snapshot. Describes current setup topology, scene surfaces, map rows, connections, and concrete scene/setup issues.",
                view = "Map",
                routeName = graph != null ? graph.RouteName : "No setup route selected",
                exportedAtUtc = DateTime.UtcNow.ToString("o"),
                source = BuildSourceInfo(graph?.Source),
                currentRoute = BuildCurrentRoute(graph?.RouteAnalysis),
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
                sceneSurfaces = PyralisAuthoringSetupGraphProjection.FindSceneSurfaceNodes(graph)
                    .Select(BuildNode)
                    .ToArray(),
                sceneSetupIssues = PyralisAuthoringSetupGraphProjection.BuildMapSceneSetupIssueRows(graph)
                    .Select(BuildMapIssue)
                    .ToArray()
            };
        }

        private static CurrentRouteSnapshot BuildCurrentRoute(PyralisSetupRouteAnalysis route)
        {
            if (route == null)
            {
                return new CurrentRouteSnapshot
                {
                    routeName = "No setup route selected",
                    hasSelectedCapabilities = false,
                    requiresPawn = false,
                    hasParticipants = false,
                    hasAnyDefaultPawn = false,
                    participantPawnIssue = string.Empty,
                    participantPawnIssueKind = PyralisParticipantPawnIssueKind.None.ToString(),
                    capabilityFamilies = Array.Empty<string>(),
                    routeFacts = Array.Empty<RouteFactSnapshot>(),
                    session = null,
                    mode = null,
                    participant = null,
                    pawn = null
                };
            }

            return new CurrentRouteSnapshot
            {
                routeName = route.RouteName,
                hasSelectedCapabilities = route.HasSelectedCapabilities,
                requiresPawn = route.RequiresPawn,
                hasParticipants = route.HasParticipants,
                hasAnyDefaultPawn = route.HasAnyDefaultPawn,
                participantPawnIssue = route.ParticipantPawnIssue ?? string.Empty,
                participantPawnIssueKind = route.ParticipantPawnIssueKind.ToString(),
                capabilityFamilies = (route.CapabilityFamilies ?? Array.Empty<RuntimeCapabilityFamily>())
                    .Select(family => family.ToString())
                    .ToArray(),
                routeFacts = (route.RouteFacts ?? Array.Empty<PyralisAuthoringRouteFact>())
                    .Select(BuildRouteFact)
                    .ToArray(),
                session = BuildSourceInfo(route.Session),
                mode = BuildSourceInfo(route.Mode),
                participant = BuildSourceInfo(route.Participant),
                pawn = BuildSourceInfo(route.Pawn)
            };
        }

        private static RouteFactSnapshot BuildRouteFact(PyralisAuthoringRouteFact fact)
        {
            return new RouteFactSnapshot
            {
                capability = fact.Capability.ToString(),
                label = fact.Label,
                family = fact.Family.ToString(),
                primaryProofCandidate = fact.PrimaryProofCandidate
            };
        }

        private static HygieneSnapshot BuildHygieneSnapshot(
            PyralisAuthoringSetupGraph graph,
            IReadOnlyList<PyralisSourceDependencyHygieneRecord> dependencyRecords)
        {
            IReadOnlyList<PyralisSourceDependencyHygieneRecord> safeDependencyRecords =
                dependencyRecords ?? Array.Empty<PyralisSourceDependencyHygieneRecord>();
            PyralisAuthoringGraphConnectionRow[] proofBlockers =
                PyralisAuthoringSetupGraphProjection.BuildProofBlockerRows(graph).ToArray();

            return new HygieneSnapshot
            {
                schema = "pyralis.authoring.hygieneSnapshot.v1",
                purpose = "Read-only Hygiene tab snapshot. Describes graph integrity, blocker evidence, source origins, dependency pressure, and contract source pressure.",
                view = "Hygiene",
                routeName = graph != null ? graph.RouteName : "No setup route selected",
                exportedAtUtc = DateTime.UtcNow.ToString("o"),
                source = BuildSourceInfo(graph?.Source),
                graphSummary = BuildGraphSummary(graph, safeDependencyRecords, proofBlockers),
                hygieneSections = PyralisAuthoringSetupGraphProjection.BuildHygieneSections(graph)
                    .Select(BuildHygieneSection)
                    .ToArray(),
                hygieneRows = PyralisAuthoringSetupGraphProjection.BuildHygieneDetailRows(graph)
                    .Select(BuildHygieneRow)
                    .ToArray(),
                proofBlockers = proofBlockers.Select(BuildConnection).ToArray(),
                sourceOriginCounts = CountBy(graph?.Nodes, node => node.SourceOrigin.ToString()),
                sourceKindCounts = CountBy(graph?.Nodes, node => node.SourceKind.ToString()),
                evidenceStateCounts = CountBy(graph?.Nodes, node => node.EvidenceState.ToString()),
                dependencyPressureSummary = BuildDependencyPressureSummary(safeDependencyRecords),
                cleanupFocus = BuildCleanupFocus(safeDependencyRecords)
                    .Select(BuildDependencyPressure)
                    .ToArray(),
                dependencyPressure = safeDependencyRecords
                    .Where(record => record != null && record.Risk != PyralisSourceDependencyRisk.Low)
                    .OrderBy(record => PyralisSourceDependencyHygieneScanner.GetCleanupPriority(record.PressureKind))
                    .ThenByDescending(record => record.RiskScore)
                    .ThenBy(record => record.FileName, StringComparer.Ordinal)
                    .Take(32)
                    .Select(BuildDependencyPressure)
                    .ToArray(),
                contractSourcePressure = BuildContractSourcePressure(graph)
            };
        }

        private static PyralisSourceDependencyHygieneRecord[] BuildCleanupFocus(
            IReadOnlyList<PyralisSourceDependencyHygieneRecord> dependencyRecords)
        {
            if (dependencyRecords == null)
                return Array.Empty<PyralisSourceDependencyHygieneRecord>();

            return dependencyRecords
                .Where(record => record != null && record.Risk != PyralisSourceDependencyRisk.Low)
                .OrderBy(record => PyralisSourceDependencyHygieneScanner.GetCleanupPriority(record.PressureKind))
                .ThenByDescending(record => record.RiskScore)
                .ThenBy(record => record.FileName, StringComparer.Ordinal)
                .Take(16)
                .ToArray();
        }

        private static DependencyPressureSummarySnapshot BuildDependencyPressureSummary(
            IReadOnlyList<PyralisSourceDependencyHygieneRecord> dependencyRecords)
        {
            PyralisSourceDependencyHygieneRecord[] pressureRecords = dependencyRecords == null
                ? Array.Empty<PyralisSourceDependencyHygieneRecord>()
                : dependencyRecords
                    .Where(record => record != null && record.Risk != PyralisSourceDependencyRisk.Low)
                    .OrderByDescending(record => record.RiskScore)
                    .ToArray();

            return new DependencyPressureSummarySnapshot
            {
                totalPressureRecordCount = pressureRecords.Length,
                exportedTopRecordCount = Math.Min(32, pressureRecords.Length),
                exportedCleanupFocusCount = Math.Min(16, CountCleanupFocusRecords(pressureRecords)),
                actionablePressureRecordCount = CountActionablePressureRecords(pressureRecords),
                expectedPressureRecordCount = Math.Max(0, pressureRecords.Length - CountActionablePressureRecords(pressureRecords)),
                omittedRecordCount = Math.Max(0, pressureRecords.Length - 32),
                highestRiskScore = pressureRecords.Length > 0 ? pressureRecords[0].RiskScore : 0,
                riskCounts = CountBy(pressureRecords, record => record.Risk.ToString()),
                pressureKindCounts = CountBy(pressureRecords, record => record.PressureKind.ToString()),
                ownerDomainCounts = CountBy(pressureRecords, record => record.OwnerDomain),
                touchedDomainCounts = CountDomains(pressureRecords)
            };
        }

        private static GraphSummarySnapshot BuildGraphSummary(
            PyralisAuthoringSetupGraph graph,
            IReadOnlyList<PyralisSourceDependencyHygieneRecord> dependencyRecords,
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> proofBlockers)
        {
            return new GraphSummarySnapshot
            {
                nodeCount = graph?.Nodes.Count ?? 0,
                edgeCount = graph?.Edges.Count ?? 0,
                unknownNodeCount = graph?.Nodes.Count(node => node.EvidenceState == PyralisAuthoringGraphEvidenceState.Unknown) ?? 0,
                missingNodeCount = graph?.Nodes.Count(node => node.EvidenceState == PyralisAuthoringGraphEvidenceState.Missing) ?? 0,
                blockedNodeCount = graph?.Nodes.Count(node => node.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked) ?? 0,
                proofBlockerCount = proofBlockers?.Count ?? 0,
                dependencyPressureCount = dependencyRecords?.Count(record => record != null && record.Risk != PyralisSourceDependencyRisk.Low) ?? 0,
                contractNodeCount = graph?.Nodes.Count(node => node.Kind == PyralisAuthoringGraphNodeKind.Contract || node.SourceContract != null) ?? 0
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
                sourceObject = BuildSourceInfo(node.SourceObject)
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
                fromNodeId = row.From != null ? row.From.StableId : string.Empty,
                toNodeId = row.To != null ? row.To.StableId : string.Empty,
                from = row.FromLabel,
                to = row.ToLabel,
                relationship = row.Relationship,
                detail = row.Detail
            };
        }

        private static MapIssueSnapshot BuildMapIssue(PyralisAuthoringGraphAuditRow row)
        {
            PyralisAuthoringGraphNode node = row.Node;
            return new MapIssueSnapshot
            {
                nodeId = row.NodeId,
                label = row.Label,
                evidenceState = row.EvidenceState.ToString(),
                message = row.Message,
                nativeAction = row.NativeAction,
                assignmentFields = node != null ? node.AssignmentFields : Array.Empty<string>(),
                nativeSetup = node != null ? node.NativeSetup : Array.Empty<string>(),
                blockingReason = node != null ? node.BlockingReason : string.Empty,
                target = BuildSourceInfo(row.Target)
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

        private static DependencyPressureSnapshot BuildDependencyPressure(PyralisSourceDependencyHygieneRecord record)
        {
            return new DependencyPressureSnapshot
            {
                assetPath = record.AssetPath,
                fileName = record.FileName,
                ownerDomain = record.OwnerDomain,
                domains = record.Domains.ToArray(),
                dependencyCount = record.DependencyCount,
                concreteCrossDomainCount = record.ConcreteCrossDomainCount,
                serializedFieldCount = record.SerializedFieldCount,
                unityLookupCount = record.UnityLookupCount,
                localComponentLookupCount = record.LocalComponentLookupCount,
                broadUnityDiscoveryCount = record.BroadUnityDiscoveryCount,
                staticAccessCount = record.StaticAccessCount,
                reflectionOrStringLookupCount = record.ReflectionOrStringLookupCount,
                riskScore = record.RiskScore,
                risk = record.Risk.ToString(),
                pressureKind = record.PressureKind.ToString(),
                cleanupPriority = PyralisSourceDependencyHygieneScanner.GetCleanupPriority(record.PressureKind),
                cleanupFocus = IsCleanupFocus(record.PressureKind),
                reviewHint = record.ReviewHint,
                reasons = record.Reasons.ToArray()
            };
        }

        private static int CountCleanupFocusRecords(IReadOnlyList<PyralisSourceDependencyHygieneRecord> records)
        {
            if (records == null)
                return 0;

            int count = 0;
            for (int i = 0; i < records.Count; i++)
            {
                PyralisSourceDependencyHygieneRecord record = records[i];
                if (record != null && record.Risk != PyralisSourceDependencyRisk.Low && IsCleanupFocus(record.PressureKind))
                    count++;
            }

            return count;
        }

        private static int CountActionablePressureRecords(IReadOnlyList<PyralisSourceDependencyHygieneRecord> records)
        {
            if (records == null)
                return 0;

            int count = 0;
            for (int i = 0; i < records.Count; i++)
            {
                PyralisSourceDependencyHygieneRecord record = records[i];
                if (record != null && record.Risk != PyralisSourceDependencyRisk.Low && IsActionablePressure(record.PressureKind))
                    count++;
            }

            return count;
        }

        private static bool IsCleanupFocus(PyralisSourceDependencyPressureKind pressureKind)
        {
            return pressureKind == PyralisSourceDependencyPressureKind.RuntimeOwnership
                || pressureKind == PyralisSourceDependencyPressureKind.CompatibilitySurface
                || pressureKind == PyralisSourceDependencyPressureKind.AcceptedComposition;
        }

        private static bool IsActionablePressure(PyralisSourceDependencyPressureKind pressureKind)
        {
            return pressureKind == PyralisSourceDependencyPressureKind.RuntimeOwnership
                || pressureKind == PyralisSourceDependencyPressureKind.CompatibilitySurface;
        }

        private static ContractPressureSnapshot[] BuildContractSourcePressure(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return Array.Empty<ContractPressureSnapshot>();

            return graph.Nodes
                .Where(node => node != null && (node.Kind == PyralisAuthoringGraphNodeKind.Contract || node.SourceContract != null))
                .Select(BuildContractPressure)
                .ToArray();
        }

        private static ContractPressureSnapshot BuildContractPressure(PyralisAuthoringGraphNode node)
        {
            ResolvedAuthoringContract contract = node.SourceContract;
            return new ContractPressureSnapshot
            {
                nodeId = node.StableId,
                label = node.Label,
                sourceOrigin = node.SourceOrigin.ToString(),
                evidenceState = node.EvidenceState.ToString(),
                stableId = contract != null ? contract.StableId : string.Empty,
                displayName = contract != null ? contract.DisplayName : string.Empty,
                category = contract != null ? contract.AuthoringCategory : string.Empty,
                moduleId = contract != null ? contract.ModuleId : string.Empty,
                setupNodeId = contract != null ? contract.SetupNodeId : string.Empty,
                capability = contract != null ? contract.Capability.ToString() : node.AuthoringCapability.ToString(),
                confidence = contract != null ? contract.Confidence.ToString() : string.Empty,
                sourceType = contract?.SourceType != null ? contract.SourceType.FullName : string.Empty,
                assignmentFieldCount = node.AssignmentFields.Length,
                customizationMomentCount = node.CustomizationMoments.Length,
                nativeSetupCount = node.NativeSetup.Length
            };
        }

        private static CountSnapshot[] CountBy(
            IEnumerable<PyralisAuthoringGraphNode> nodes,
            Func<PyralisAuthoringGraphNode, string> selector)
        {
            if (nodes == null)
                return Array.Empty<CountSnapshot>();

            return nodes
                .Where(node => node != null)
                .GroupBy(selector)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new CountSnapshot { label = group.Key, count = group.Count() })
                .ToArray();
        }

        private static CountSnapshot[] CountBy(
            IEnumerable<PyralisSourceDependencyHygieneRecord> records,
            Func<PyralisSourceDependencyHygieneRecord, string> selector)
        {
            if (records == null)
                return Array.Empty<CountSnapshot>();

            return records
                .Where(record => record != null)
                .GroupBy(record => NormalizeCountLabel(selector(record)))
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new CountSnapshot { label = group.Key, count = group.Count() })
                .ToArray();
        }

        private static CountSnapshot[] CountDomains(IEnumerable<PyralisSourceDependencyHygieneRecord> records)
        {
            if (records == null)
                return Array.Empty<CountSnapshot>();

            return records
                .Where(record => record?.Domains != null)
                .SelectMany(record => record.Domains)
                .GroupBy(NormalizeCountLabel)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new CountSnapshot { label = group.Key, count = group.Count() })
                .ToArray();
        }

        private static string NormalizeCountLabel(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "Unknown" : value;
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
        private sealed class MapSnapshot
        {
            public string schema;
            public string purpose;
            public string view;
            public string routeName;
            public string exportedAtUtc;
            public SourceSnapshot source;
            public CurrentRouteSnapshot currentRoute;
            public int nodeCount;
            public int edgeCount;
            public NodeSnapshot[] nodes;
            public EdgeSnapshot[] edges;
            public MapRowSnapshot[] mapRows;
            public ConnectionSnapshot[] mapConnections;
            public NodeSnapshot[] sceneSurfaces;
            public MapIssueSnapshot[] sceneSetupIssues;
        }

        [Serializable]
        private sealed class CurrentRouteSnapshot
        {
            public string routeName;
            public bool hasSelectedCapabilities;
            public bool requiresPawn;
            public bool hasParticipants;
            public bool hasAnyDefaultPawn;
            public string participantPawnIssue;
            public string participantPawnIssueKind;
            public string[] capabilityFamilies;
            public RouteFactSnapshot[] routeFacts;
            public SourceSnapshot session;
            public SourceSnapshot mode;
            public SourceSnapshot participant;
            public SourceSnapshot pawn;
        }

        [Serializable]
        private sealed class RouteFactSnapshot
        {
            public string capability;
            public string label;
            public string family;
            public bool primaryProofCandidate;
        }

        [Serializable]
        private sealed class HygieneSnapshot
        {
            public string schema;
            public string purpose;
            public string view;
            public string routeName;
            public string exportedAtUtc;
            public SourceSnapshot source;
            public GraphSummarySnapshot graphSummary;
            public HygieneSectionSnapshot[] hygieneSections;
            public HygieneRowSnapshot[] hygieneRows;
            public ConnectionSnapshot[] proofBlockers;
            public CountSnapshot[] sourceOriginCounts;
            public CountSnapshot[] sourceKindCounts;
            public CountSnapshot[] evidenceStateCounts;
            public DependencyPressureSummarySnapshot dependencyPressureSummary;
            public DependencyPressureSnapshot[] cleanupFocus;
            public DependencyPressureSnapshot[] dependencyPressure;
            public ContractPressureSnapshot[] contractSourcePressure;
        }

        [Serializable]
        private sealed class GraphSummarySnapshot
        {
            public int nodeCount;
            public int edgeCount;
            public int unknownNodeCount;
            public int missingNodeCount;
            public int blockedNodeCount;
            public int proofBlockerCount;
            public int dependencyPressureCount;
            public int contractNodeCount;
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
        private sealed class MapIssueSnapshot
        {
            public string nodeId;
            public string label;
            public string evidenceState;
            public string message;
            public string nativeAction;
            public string[] assignmentFields;
            public string[] nativeSetup;
            public string blockingReason;
            public SourceSnapshot target;
        }

        [Serializable]
        private sealed class ConnectionSnapshot
        {
            public string fromNodeId;
            public string toNodeId;
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
        private sealed class DependencyPressureSnapshot
        {
            public string assetPath;
            public string fileName;
            public string ownerDomain;
            public string[] domains;
            public int dependencyCount;
            public int concreteCrossDomainCount;
            public int serializedFieldCount;
            public int unityLookupCount;
            public int localComponentLookupCount;
            public int broadUnityDiscoveryCount;
            public int staticAccessCount;
            public int reflectionOrStringLookupCount;
            public int riskScore;
            public string risk;
            public string pressureKind;
            public int cleanupPriority;
            public bool cleanupFocus;
            public string reviewHint;
            public string[] reasons;
        }

        [Serializable]
        private sealed class DependencyPressureSummarySnapshot
        {
            public int totalPressureRecordCount;
            public int exportedTopRecordCount;
            public int exportedCleanupFocusCount;
            public int actionablePressureRecordCount;
            public int expectedPressureRecordCount;
            public int omittedRecordCount;
            public int highestRiskScore;
            public CountSnapshot[] riskCounts;
            public CountSnapshot[] pressureKindCounts;
            public CountSnapshot[] ownerDomainCounts;
            public CountSnapshot[] touchedDomainCounts;
        }

        [Serializable]
        private sealed class ContractPressureSnapshot
        {
            public string nodeId;
            public string label;
            public string sourceOrigin;
            public string evidenceState;
            public string stableId;
            public string displayName;
            public string category;
            public string moduleId;
            public string setupNodeId;
            public string capability;
            public string confidence;
            public string sourceType;
            public int assignmentFieldCount;
            public int customizationMomentCount;
            public int nativeSetupCount;
        }

        [Serializable]
        private sealed class CountSnapshot
        {
            public string label;
            public int count;
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
    }
}
