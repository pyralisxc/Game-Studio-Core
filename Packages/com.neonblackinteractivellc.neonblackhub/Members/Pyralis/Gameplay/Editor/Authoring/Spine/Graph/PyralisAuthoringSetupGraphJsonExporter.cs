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

        public static string ToRouteProofTraceJson(PyralisAuthoringSetupGraph graph)
        {
            RouteProofTraceSnapshot snapshot = BuildRouteProofTraceSnapshot(graph);
            return JsonUtility.ToJson(snapshot, true);
        }

        public static string ToIntentJson(
            PyralisAuthoringIntentSelection selection,
            PyralisAuthoringIntentModel model,
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors)
        {
            IntentSnapshot snapshot = BuildIntentSnapshot(selection, model, descriptors);
            return JsonUtility.ToJson(snapshot, true);
        }

        public static string ToFactsJson(PyralisAuthoringSetupGraph graph)
        {
            FactsSnapshot snapshot = BuildFactsSnapshot(graph);
            return JsonUtility.ToJson(snapshot, true);
        }

        public static string ToJson(PyralisAuthoringSetupGraph graph, string view)
        {
            if (string.Equals(view, "Hygiene", StringComparison.OrdinalIgnoreCase))
                return ToHygieneJson(graph, Array.Empty<PyralisSourceDependencyHygieneRecord>());

            if (string.Equals(view, "Facts", StringComparison.OrdinalIgnoreCase))
                return ToFactsJson(graph);

            return ToMapJson(graph);
        }

        private static IntentSnapshot BuildIntentSnapshot(
            PyralisAuthoringIntentSelection selection,
            PyralisAuthoringIntentModel model,
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors)
        {
            selection ??= new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.None,
                AuthoringWorldAxiom.None);
            model ??= PyralisAuthoringSetupGraphProjection.BuildIntentModel(selection);
            descriptors ??= Array.Empty<PyralisAuthoringCapabilityDescriptor>();

            string[] selectedIds = selection.DescriptorIds ?? Array.Empty<string>();
            return new IntentSnapshot
            {
                schema = "pyralis.authoring.intentSnapshot.v1",
                purpose = "Read-only Intent tab snapshot. Describes current route steering only: DNA axioms, presentation lane, participant route, selected capability descriptors, available descriptor groups, and advisor rows. It does not describe scene/setup reality.",
                view = "Intent",
                exportedAtUtc = DateTime.UtcNow.ToString("o"),
                selection = new IntentSelectionSnapshot
                {
                    lane = selection.Lane.ToString(),
                    axioms = selection.Axioms.ToString(),
                    capabilities = selection.Capabilities.ToString(),
                    participantRoute = selection.ParticipantRoute.ToString(),
                    selectedDescriptorIds = selectedIds
                },
                summary = new IntentSummarySnapshot
                {
                    descriptorCount = descriptors.Count,
                    selectableDescriptorCount = descriptors.Count(descriptor => descriptor != null && descriptor.SelectableIntent),
                    selectedDescriptorCount = selectedIds.Length,
                    recommendationCount = model.Recommendations.Count,
                    cautionCount = model.Cautions.Count,
                    matchingIntentCount = model.MatchingIntents.Count
                },
                routeShapePreview = PyralisAuthoringSetupGraphProjection.BuildRouteShapeSummary(selection),
                advisorSummary = model.Summary,
                descriptorGroups = BuildIntentDescriptorGroups(descriptors, selectedIds),
                selectedDescriptors = descriptors
                    .Where(descriptor => descriptor != null && selectedIds.Contains(descriptor.StableId))
                    .Select(BuildIntentDescriptor)
                    .ToArray(),
                recommendations = model.Recommendations.Select(BuildIntentRow).ToArray(),
                cautions = model.Cautions.Select(BuildIntentRow).ToArray(),
                matchingIntents = model.MatchingIntents.Select(BuildFact).ToArray()
            };
        }

        private static FactsSnapshot BuildFactsSnapshot(PyralisAuthoringSetupGraph graph)
        {
            IReadOnlyList<PyralisAuthoringFact> facts = PyralisAuthoringSetupGraphProjection.BuildCookbookFacts(graph)
                ?? Array.Empty<PyralisAuthoringFact>();
            IReadOnlyList<PyralisAuthoringReflectiveContractGraphRow> contractRows =
                PyralisAuthoringSetupGraphProjection.BuildReflectiveContractRows(graph);
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> proofRows =
                PyralisAuthoringSetupGraphProjection.BuildProofSupportRows(graph);

            return new FactsSnapshot
            {
                schema = "pyralis.authoring.factsSnapshot.v1",
                purpose = "Read-only Facts tab snapshot. Describes the compiled dictionary/cookbook: vocabulary facts, reflected contracts, proof templates, source provenance, and graph coverage. Setup actions belong to Guide/Map.",
                view = "Facts",
                routeName = graph != null ? graph.RouteName : "No setup route selected",
                exportedAtUtc = DateTime.UtcNow.ToString("o"),
                source = BuildSourceInfo(graph?.Source),
                summary = new FactsSummarySnapshot
                {
                    factCount = facts.Count,
                    graphNodeCount = graph?.Nodes.Count ?? 0,
                    graphEdgeCount = graph?.Edges.Count ?? 0,
                    graphContractCount = contractRows.Count,
                    proofSupportCount = proofRows.Count
                },
                factKindCounts = CountBy(facts, fact => fact.Kind.ToString()),
                sourceKindCounts = CountBy(facts, fact => fact.SourceKind.ToString()),
                confidenceCounts = CountBy(facts, fact => fact.Confidence.ToString()),
                capabilityCounts = CountCapabilityFacts(facts),
                graphContractCoverage = contractRows.Select(BuildReflectiveContract).ToArray(),
                graphProofCoverage = proofRows.Select(BuildConnection).ToArray(),
                facts = facts.Select(BuildFact).ToArray()
            };
        }

        private static MapSnapshot BuildMapSnapshot(PyralisAuthoringSetupGraph graph)
        {
            MapRowSnapshot[] mapRows = PyralisAuthoringSetupGraphProjection.BuildSetupMapRows(graph)
                .Select(BuildMapRow)
                .ToArray();
            ConnectionSnapshot[] mapConnections = PyralisAuthoringSetupGraphProjection.BuildMapConnectionRows(graph)
                .Select(BuildConnection)
                .ToArray();
            MapIssueSnapshot[] sceneSetupIssues = PyralisAuthoringSetupGraphProjection.BuildMapSceneSetupIssueRows(graph)
                .Select(BuildMapIssue)
                .ToArray();

            return new MapSnapshot
            {
                schema = "pyralis.authoring.mapSnapshot.v1",
                purpose = "Read-only Map tab snapshot. Describes current setup topology, scene surfaces, map rows, connections, and concrete scene/setup issues.",
                view = "Map",
                routeName = graph != null ? graph.RouteName : "No setup route selected",
                exportedAtUtc = DateTime.UtcNow.ToString("o"),
                source = BuildSourceInfo(graph?.Source),
                summary = BuildMapSummary(graph, mapRows, sceneSetupIssues),
                currentRoute = BuildCurrentRoute(graph?.RouteAnalysis),
                nodeCount = graph?.Nodes.Count ?? 0,
                edgeCount = graph?.Edges.Count ?? 0,
                nodes = graph?.Nodes.Select(BuildNode).ToArray() ?? Array.Empty<NodeSnapshot>(),
                edges = graph?.Edges.Select(BuildEdge).ToArray() ?? Array.Empty<EdgeSnapshot>(),
                mapRows = mapRows,
                mapConnections = mapConnections,
                sceneSurfaces = PyralisAuthoringSetupGraphProjection.FindSceneSurfaceNodes(graph)
                    .Select(BuildNode)
                    .ToArray(),
                sceneSetupIssues = sceneSetupIssues
            };
        }

        private static CurrentRouteSnapshot BuildCurrentRoute(
            PyralisSetupRouteAnalysis route,
            PyralisAuthoringRouteWorkingProjection routeProjection = null)
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
                    participantTopology = PyralisParticipantTopology.Unknown.ToString(),
                    expectedJoinPolicy = PyralisParticipantJoinPolicy.Unknown.ToString(),
                    spawnPolicy = PyralisParticipantSpawnPolicy.Unknown.ToString(),
                    assignedParticipantCount = 0,
                    authoredParticipantCount = 0,
                    desiredParticipantCount = 0,
                    autoJoinParticipantCount = 0,
                    autoRegisterDefaultsWithoutPlayerInput = false,
                    hasPlayerInputManager = false,
                    spawnOnRegister = false,
                    hasLocalJoinPolicyConflict = false,
                    playerInputManagerIssue = string.Empty,
                    participantSeats = Array.Empty<ParticipantSeatSnapshot>(),
                    capabilityFamilies = Array.Empty<string>(),
                    routeFacts = Array.Empty<RouteFactSnapshot>(),
                    session = null,
                    mode = null,
                    participant = null,
                    pawn = null
                };
            }

            string participantPawnIssue = route.ParticipantPawnIssue ?? string.Empty;
            if (routeProjection != null && route.ParticipantPawnIssueKind != PyralisParticipantPawnIssueKind.None)
            {
                PyralisAuthoringRouteStepRow pawnStep = routeProjection.CriticalPath
                    .FirstOrDefault(row => row != null && string.Equals(row.StableId, "pawn.definition", StringComparison.Ordinal));
                if (pawnStep != null && !string.IsNullOrWhiteSpace(pawnStep.Message))
                    participantPawnIssue = pawnStep.Message;
            }

            return new CurrentRouteSnapshot
            {
                routeName = route.RouteName,
                hasSelectedCapabilities = route.HasSelectedCapabilities,
                requiresPawn = route.RequiresPawn,
                hasParticipants = route.HasParticipants,
                hasAnyDefaultPawn = route.HasAnyDefaultPawn,
                participantPawnIssue = participantPawnIssue,
                participantPawnIssueKind = route.ParticipantPawnIssueKind.ToString(),
                participantTopology = route.ParticipantTopology.ToString(),
                expectedJoinPolicy = route.ExpectedJoinPolicy.ToString(),
                spawnPolicy = route.SpawnPolicy.ToString(),
                assignedParticipantCount = route.AssignedParticipantCount,
                authoredParticipantCount = route.AuthoredParticipantCount,
                desiredParticipantCount = route.DesiredParticipantCount,
                autoJoinParticipantCount = route.AutoJoinParticipantCount,
                autoRegisterDefaultsWithoutPlayerInput = route.AutoRegisterDefaultsWithoutPlayerInput,
                hasPlayerInputManager = route.HasPlayerInputManager,
                spawnOnRegister = route.SpawnOnRegister,
                hasLocalJoinPolicyConflict = route.HasLocalJoinPolicyConflict(),
                playerInputManagerIssue = route.PlayerInputManagerIssue,
                participantSeats = (route.ParticipantSeats ?? Array.Empty<PyralisParticipantSeatReadiness>())
                    .Select(BuildParticipantSeat)
                    .ToArray(),
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

        private static ParticipantSeatSnapshot BuildParticipantSeat(PyralisParticipantSeatReadiness seat)
        {
            if (seat == null)
                return null;

            return new ParticipantSeatSnapshot
            {
                slotIndex = seat.SlotIndex,
                seatIndex = seat.SeatIndex,
                displayName = seat.DisplayName,
                requiresPawn = seat.RequiresPawn,
                hasParticipant = seat.HasParticipant,
                hasInputProfile = seat.HasInputProfile,
                hasPawnDefinition = seat.HasPawnDefinition,
                hasPawnPrefab = seat.HasPawnPrefab,
                isInputReady = seat.IsInputReady,
                isPawnReady = seat.IsPawnReady,
                isReady = seat.IsReady,
                inputIssue = seat.InputIssue,
                pawnIssue = seat.PawnIssue,
                pawnIssueKind = seat.PawnIssueKind.ToString(),
                participant = BuildSourceInfo(seat.Participant),
                pawn = BuildSourceInfo(seat.Pawn),
                inputProfile = BuildSourceInfo(seat.InputProfile)
            };
        }

        private static HygieneSnapshot BuildHygieneSnapshot(
            PyralisAuthoringSetupGraph graph,
            IReadOnlyList<PyralisSourceDependencyHygieneRecord> dependencyRecords)
        {
            IReadOnlyList<PyralisSourceDependencyHygieneRecord> safeDependencyRecords =
                dependencyRecords ?? Array.Empty<PyralisSourceDependencyHygieneRecord>();
            PyralisAuthoringGraphConnectionRow[] proofBlockers =
                PyralisAuthoringSetupGraphProjection.BuildHygieneProofBlockerConnectionRows(graph).ToArray();

            return new HygieneSnapshot
            {
                schema = "pyralis.authoring.hygieneSnapshot.v2",
                purpose = "Read-only Hygiene tab snapshot. Describes graph integrity, source origins, dependency pressure, and contract source pressure. Scene/setup repair issues belong to Map; route setup ordering belongs to Route Proof Trace.",
                view = "Hygiene",
                exportedAtUtc = DateTime.UtcNow.ToString("o"),
                source = BuildSourceInfo(graph?.Source),
                graphContext = BuildHygieneGraphContext(graph),
                graphSummary = BuildGraphSummary(graph, safeDependencyRecords, proofBlockers),
                summary = BuildHygieneSummary(graph, safeDependencyRecords, proofBlockers),
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
                watchList = BuildWatchList(safeDependencyRecords)
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

        private static RouteProofTraceSnapshot BuildRouteProofTraceSnapshot(PyralisAuthoringSetupGraph graph)
        {
            PyralisAuthoringRouteWorkingProjection route = PyralisAuthoringSetupGraphProjection.BuildRouteWorkingProjection(graph);
            RouteStepSnapshot[] orderedSteps = route.OrderedSteps.Select(BuildRouteStep).ToArray();
            RouteStepSnapshot[] criticalPath = route.CriticalPath.Select(BuildRouteStep).ToArray();
            RouteStepSnapshot[] proofEnhancers = route.ProofEnhancers.Select(BuildRouteStep).ToArray();
            RouteStepSnapshot[] canWait = route.CanWait.Select(BuildRouteStep).ToArray();
            ConnectionSnapshot[] proofBlockers = route.ProofBlockers.Select(BuildConnection).ToArray();
            ConnectionSnapshot[] proofSupport = route.ProofSupport.Select(BuildConnection).ToArray();

            return new RouteProofTraceSnapshot
            {
                schema = "pyralis.authoring.routeProofTrace.v1",
                purpose = "Developer-facing Route Proof Trace. Previews the ordered setup cards a fresh scene should follow to reach the selected first proof, including definitions, participants, pawns, prefabs, required validation cards, proof enhancers, and the final Play Mode proof. This is documentation/debug evidence, not a preset or scene generator.",
                view = "RouteProofTrace",
                routeName = graph != null ? graph.RouteName : "No setup route selected",
                exportedAtUtc = DateTime.UtcNow.ToString("o"),
                source = BuildSourceInfo(graph?.Source),
                summary = BuildRouteTraceSummary(graph, route),
                currentRoute = BuildCurrentRoute(graph?.RouteAnalysis, route),
                intentFocus = PyralisAuthoringSetupGraphProjection.BuildIntentFocusSummary(graph),
                routeShape = PyralisAuthoringSetupGraphProjection.BuildRouteShapeSummary(graph),
                proofPriority = PyralisAuthoringSetupGraphProjection.BuildFirstProofPrioritySummary(graph),
                proof = route.Proof != null ? BuildNode(route.Proof) : null,
                currentAction = route.CurrentAction != null ? BuildRouteStep(route.CurrentAction) : null,
                orderedSteps = orderedSteps,
                criticalPath = criticalPath,
                proofEnhancers = proofEnhancers,
                canWait = canWait,
                proofBlockers = proofBlockers,
                proofSupport = proofSupport,
                supportingContracts = BuildSupportingContracts(graph, route.OrderedSteps, route.ProofSupport),
                graphSummary = BuildGraphSummary(graph, Array.Empty<PyralisSourceDependencyHygieneRecord>(), route.ProofBlockers),
                diagnosticQuestions = BuildTraceDiagnosticQuestions(graph, route.CurrentAction, route.OrderedSteps, route.CriticalPath, route.ProofEnhancers, route.CanWait, route.ProofBlockers, route.ProofSupport)
            };
        }

        private static ExportSummarySnapshot BuildMapSummary(
            PyralisAuthoringSetupGraph graph,
            IReadOnlyList<MapRowSnapshot> mapRows,
            IReadOnlyList<MapIssueSnapshot> sceneSetupIssues)
        {
            return new ExportSummarySnapshot
            {
                routeName = graph != null ? graph.RouteName : "No setup route selected",
                currentActionLabel = string.Empty,
                currentActionNodeId = string.Empty,
                readyForFirstProof = false,
                nodeCount = graph?.Nodes.Count ?? 0,
                edgeCount = graph?.Edges.Count ?? 0,
                mapRowCount = mapRows?.Count ?? 0,
                readyMapRowCount = mapRows?.Count(row => row != null && row.isReady) ?? 0,
                missingMapRowCount = mapRows?.Count(row => row != null && row.isMissing) ?? 0,
                sceneSetupIssueCount = sceneSetupIssues?.Count ?? 0,
                hygieneRowCount = 0,
                cleanupFocusCount = 0,
                watchListCount = 0,
                criticalPathCount = 0,
                proofEnhancerCount = 0,
                proofBlockerCount = 0,
                proofSupportCount = 0
            };
        }

        private static HygieneSummarySnapshot BuildHygieneSummary(
            PyralisAuthoringSetupGraph graph,
            IReadOnlyList<PyralisSourceDependencyHygieneRecord> dependencyRecords,
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> proofBlockers)
        {
            PyralisSourceDependencyHygieneRecord[] pressureRecords = dependencyRecords == null
                ? Array.Empty<PyralisSourceDependencyHygieneRecord>()
                : dependencyRecords.Where(record => record != null && record.Risk != PyralisSourceDependencyRisk.Low).ToArray();

            return new HygieneSummarySnapshot
            {
                scanScope = "Package source plus resolved setup graph when available",
                graphContextName = graph != null ? graph.RouteName : "No active setup graph",
                hasGraphContext = graph != null,
                nodeCount = graph?.Nodes.Count ?? 0,
                edgeCount = graph?.Edges.Count ?? 0,
                hygieneRowCount = PyralisAuthoringSetupGraphProjection.BuildHygieneDetailRows(graph).Count,
                cleanupFocusCount = CountCleanupFocusRecords(pressureRecords),
                watchListCount = CountWatchListRecords(pressureRecords),
                exportedCleanupFocusCount = Math.Min(16, CountCleanupFocusRecords(pressureRecords)),
                exportedWatchListCount = Math.Min(16, CountWatchListRecords(pressureRecords)),
                omittedDependencyPressureCount = Math.Max(0, pressureRecords.Length - 32),
                proofBlockerCount = proofBlockers?.Count ?? 0,
                dependencyPressureCount = pressureRecords.Length,
                contractSourcePressureCount = BuildContractSourcePressure(graph).Length
            };
        }

        private static HygieneGraphContextSnapshot BuildHygieneGraphContext(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
            {
                return new HygieneGraphContextSnapshot
                {
                    hasGraph = false,
                    graphName = "No active setup graph",
                    source = null,
                    note = "Hygiene can still export package dependency pressure without an active setup graph."
                };
            }

            return new HygieneGraphContextSnapshot
            {
                hasGraph = true,
                graphName = graph.RouteName,
                source = BuildSourceInfo(graph.Source),
                note = "Passive context only. Hygiene does not own route setup ordering or scene repair actions."
            };
        }

        private static ExportSummarySnapshot BuildRouteTraceSummary(
            PyralisAuthoringSetupGraph graph,
            PyralisAuthoringRouteWorkingProjection route)
        {
            return new ExportSummarySnapshot
            {
                routeName = graph != null ? graph.RouteName : "No setup route selected",
                currentActionLabel = route?.CurrentAction != null ? route.CurrentAction.Label : string.Empty,
                currentActionNodeId = route?.CurrentAction != null ? route.CurrentAction.StableId : string.Empty,
                readyForFirstProof = route != null && route.ReadyForFirstProof,
                nodeCount = graph?.Nodes.Count ?? 0,
                edgeCount = graph?.Edges.Count ?? 0,
                mapRowCount = 0,
                readyMapRowCount = 0,
                missingMapRowCount = 0,
                sceneSetupIssueCount = 0,
                hygieneRowCount = 0,
                cleanupFocusCount = 0,
                watchListCount = 0,
                criticalPathCount = route?.CriticalPath.Count ?? 0,
                proofEnhancerCount = route?.ProofEnhancers.Count ?? 0,
                proofBlockerCount = route?.ProofBlockers.Count ?? 0,
                proofSupportCount = route?.ProofSupport.Count ?? 0
            };
        }

        private static RouteStepSnapshot BuildRouteStep(PyralisAuthoringRouteStepRow row)
        {
            PyralisAuthoringGraphNode node = row?.Node;
            return new RouteStepSnapshot
            {
                sequence = row != null ? row.Sequence : 0,
                nodeId = row?.StableId ?? string.Empty,
                label = row?.Label ?? string.Empty,
                phase = row?.PhaseLabel ?? string.Empty,
                role = row?.RoleLabel ?? string.Empty,
                evidenceState = row != null ? row.EvidenceState.ToString() : PyralisAuthoringGraphEvidenceState.Unknown.ToString(),
                sourceKind = node != null ? node.SourceKind.ToString() : string.Empty,
                sourceOrigin = row != null ? row.SourceOrigin.ToString() : string.Empty,
                workIntent = node != null ? node.WorkIntent.ToString() : string.Empty,
                issueSeverity = node != null ? node.IssueSeverity.ToString() : string.Empty,
                reason = row?.Reason ?? string.Empty,
                message = row?.Message ?? string.Empty,
                owner = BuildRouteStepOwner(row),
                unityAction = row?.UnityActionLabel ?? string.Empty,
                nativeAction = row != null ? BuildNativeAction(row.NativeAction) : null,
                assignmentFields = row?.AssignmentFields ?? Array.Empty<string>(),
                nativeSetup = row?.NativeSetup ?? Array.Empty<string>(),
                customizationMoments = row?.CustomizationMoments ?? Array.Empty<string>(),
                proofTargetId = node != null ? node.ProofTargetId : string.Empty,
                edge = row?.Edge != null ? BuildEdge(row.Edge) : null,
                sourceObject = BuildSourceInfo(node?.SourceObject)
            };
        }

        private static string BuildRouteStepOwner(PyralisAuthoringRouteStepRow row)
        {
            PyralisAuthoringGraphNode node = row?.Node;
            if (node == null)
                return string.Empty;

            if (row != null && row.NativeAction.HasValue)
            {
                PyralisAuthoringNativeAction action = row.NativeAction.Value;
                string target = !string.IsNullOrWhiteSpace(action.Target) ? action.Target : action.Surface.ToString();
                return !string.IsNullOrWhiteSpace(action.FieldOrComponent)
                    ? $"{target}.{action.FieldOrComponent}"
                    : target;
            }

            if (node.AssignmentFields != null && node.AssignmentFields.Length > 0)
                return node.AssignmentFields[0];

            if (node.SourceContract != null && !string.IsNullOrWhiteSpace(node.SourceContract.SetupNodeId))
                return node.SourceContract.SetupNodeId;

            if (node.SourceObject != null)
                return $"{node.SourceObject.name} ({node.SourceObject.GetType().Name})";

            return node.StableId;
        }

        private static ContractPressureSnapshot[] BuildSupportingContracts(
            PyralisAuthoringSetupGraph graph,
            IReadOnlyList<PyralisAuthoringRouteStepRow> orderedSteps,
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> proofSupport)
        {
            if (graph == null)
                return Array.Empty<ContractPressureSnapshot>();

            HashSet<string> contractNodeIds = new HashSet<string>(StringComparer.Ordinal);
            if (orderedSteps != null)
            {
                for (int i = 0; i < orderedSteps.Count; i++)
                {
                    PyralisAuthoringGraphNode node = orderedSteps[i]?.Node;
                    if (node != null && (node.Kind == PyralisAuthoringGraphNodeKind.Contract || node.SourceContract != null))
                        contractNodeIds.Add(node.StableId);
                }
            }

            if (proofSupport != null)
            {
                for (int i = 0; i < proofSupport.Count; i++)
                {
                    PyralisAuthoringGraphNode node = proofSupport[i]?.From;
                    if (node != null && (node.Kind == PyralisAuthoringGraphNodeKind.Contract || node.SourceContract != null))
                        contractNodeIds.Add(node.StableId);
                }
            }

            return graph.Nodes
                .Where(node => node != null
                    && contractNodeIds.Contains(node.StableId)
                    && (node.Kind == PyralisAuthoringGraphNodeKind.Contract || node.SourceContract != null))
                .Select(BuildContractPressure)
                .ToArray();
        }

        private static TraceDiagnosticQuestionSnapshot[] BuildTraceDiagnosticQuestions(
            PyralisAuthoringSetupGraph graph,
            PyralisAuthoringRouteStepRow currentAction,
            IReadOnlyList<PyralisAuthoringRouteStepRow> orderedSteps,
            IReadOnlyList<PyralisAuthoringRouteStepRow> criticalPath,
            IReadOnlyList<PyralisAuthoringRouteStepRow> proofEnhancers,
            IReadOnlyList<PyralisAuthoringRouteStepRow> canWait,
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> proofBlockers,
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> proofSupport)
        {
            List<TraceDiagnosticQuestionSnapshot> questions = new List<TraceDiagnosticQuestionSnapshot>
            {
                new TraceDiagnosticQuestionSnapshot
                {
                    question = "What is the next route action?",
                    answer = currentAction != null
                        ? $"{currentAction.Label}: {FirstNonEmpty(currentAction.UnityActionLabel, currentAction.Message, currentAction.Reason)}"
                        : orderedSteps != null && orderedSteps.Count > 0
                            ? "Required setup is clear for the projected proof. Use the full fresh-scene path to review how the route is assembled, then attempt the first Play Mode proof."
                            : "No ordered steps were generated. Check whether the active setup graph has a resolved setup context."
                },
                new TraceDiagnosticQuestionSnapshot
                {
                    question = "What blocks the first proof?",
                    answer = proofBlockers != null && proofBlockers.Count > 0
                        ? string.Join("; ", proofBlockers.Take(6).Select(row => row.ToLabel))
                        : "No proof blocker links are present in the current graph."
                },
                new TraceDiagnosticQuestionSnapshot
                {
                    question = "What is the full fresh-scene card path?",
                    answer = criticalPath != null && criticalPath.Count > 0
                        ? string.Join(" -> ", criticalPath.Select(row => row.Label).Concat(new[] { PyralisAuthoringSetupGraphProjection.GetOverviewFirstProofLabel(graph) }))
                        : "No setup-card path is present yet. Check setup-flow evidence, runtime validation evidence, and proof target resolution."
                },
                new TraceDiagnosticQuestionSnapshot
                {
                    question = "What can wait until after this proof?",
                    answer = canWait != null && canWait.Count > 0
                        ? string.Join("; ", canWait.Take(8).Select(row => row.Label))
                        : "No can-wait setup cards were projected for this proof route."
                },
                new TraceDiagnosticQuestionSnapshot
                {
                    question = "Which proof enhancers are useful but not blockers?",
                    answer = proofEnhancers != null && proofEnhancers.Count > 0
                        ? string.Join("; ", proofEnhancers.Take(6).Select(row => row.Label))
                        : "No proof enhancers were projected for this route."
                },
                new TraceDiagnosticQuestionSnapshot
                {
                    question = "Which contracts are proof context rather than route cards?",
                    answer = proofSupport != null && proofSupport.Count > 0
                        ? string.Join("; ", proofSupport.Take(8).Select(row => row.FromLabel))
                        : "No direct proof-support contracts are present yet. The ordered setup cards should still come from setup-flow and validation evidence."
                },
                new TraceDiagnosticQuestionSnapshot
                {
                    question = "Where should incorrect guidance be fixed?",
                    answer = "Fix the source that emitted the step: contract meaning, dependency reflection, setup-flow validation, scene-readiness validation, or graph projection. Do not hardcode a one-off Guide/Hygiene sentence."
                }
            };

            if (graph == null || graph.Source == null)
            {
                questions.Add(new TraceDiagnosticQuestionSnapshot
                {
                    question = "Why is the route empty?",
                    answer = "No active setup source was resolved. Select or pin a Bootstrap, SessionDefinition, GameModeDefinition, ParticipantDefinition, PawnDefinition, or FeatureModuleDefinition."
                });
            }

            return questions.ToArray();
        }

        private static PyralisSourceDependencyHygieneRecord[] BuildCleanupFocus(
            IReadOnlyList<PyralisSourceDependencyHygieneRecord> dependencyRecords)
        {
            if (dependencyRecords == null)
                return Array.Empty<PyralisSourceDependencyHygieneRecord>();

            return dependencyRecords
                .Where(record => record != null
                    && record.Risk != PyralisSourceDependencyRisk.Low
                    && IsCleanupFocus(record.PressureKind))
                .OrderBy(record => PyralisSourceDependencyHygieneScanner.GetCleanupPriority(record.PressureKind))
                .ThenByDescending(record => record.RiskScore)
                .ThenBy(record => record.FileName, StringComparer.Ordinal)
                .Take(16)
                .ToArray();
        }

        private static PyralisSourceDependencyHygieneRecord[] BuildWatchList(
            IReadOnlyList<PyralisSourceDependencyHygieneRecord> dependencyRecords)
        {
            if (dependencyRecords == null)
                return Array.Empty<PyralisSourceDependencyHygieneRecord>();

            return dependencyRecords
                .Where(record => record != null
                    && record.Risk != PyralisSourceDependencyRisk.Low
                    && !IsCleanupFocus(record.PressureKind))
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
                exportedWatchListCount = Math.Min(16, CountWatchListRecords(pressureRecords)),
                actionablePressureRecordCount = CountActionablePressureRecords(pressureRecords),
                acceptedPressureRecordCount = CountWatchListRecords(pressureRecords),
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

        private static int CountWatchListRecords(IReadOnlyList<PyralisSourceDependencyHygieneRecord> records)
        {
            if (records == null)
                return 0;

            int count = 0;
            for (int i = 0; i < records.Count; i++)
            {
                PyralisSourceDependencyHygieneRecord record = records[i];
                if (record != null && record.Risk != PyralisSourceDependencyRisk.Low && !IsCleanupFocus(record.PressureKind))
                    count++;
            }

            return count;
        }

        private static bool IsCleanupFocus(PyralisSourceDependencyPressureKind pressureKind)
        {
            return pressureKind == PyralisSourceDependencyPressureKind.RuntimeOwnership
                || pressureKind == PyralisSourceDependencyPressureKind.DirectSceneQuerySurface;
        }

        private static bool IsActionablePressure(PyralisSourceDependencyPressureKind pressureKind)
        {
            return pressureKind == PyralisSourceDependencyPressureKind.RuntimeOwnership
                || pressureKind == PyralisSourceDependencyPressureKind.DirectSceneQuerySurface;
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

        private static IntentDescriptorGroupSnapshot[] BuildIntentDescriptorGroups(
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors,
            IReadOnlyList<string> selectedIds)
        {
            if (descriptors == null || descriptors.Count == 0)
                return Array.Empty<IntentDescriptorGroupSnapshot>();

            HashSet<string> selected = new HashSet<string>(selectedIds ?? Array.Empty<string>(), StringComparer.Ordinal);
            return descriptors
                .Where(descriptor => descriptor != null)
                .GroupBy(descriptor => FirstNonEmpty(GetCapabilityPathPart(descriptor.CapabilityPath, 0), descriptor.Group, "General"))
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new IntentDescriptorGroupSnapshot
                {
                    group = group.Key,
                    descriptorCount = group.Count(),
                    selectedCount = group.Count(descriptor => selected.Contains(descriptor.StableId)),
                    subgroups = group
                        .GroupBy(descriptor => FirstNonEmpty(GetCapabilityPathPart(descriptor.CapabilityPath, 1), descriptor.Group, "General"))
                        .OrderBy(subgroup => subgroup.Key, StringComparer.Ordinal)
                        .Select(subgroup => new IntentDescriptorSubgroupSnapshot
                        {
                            subgroup = subgroup.Key,
                            descriptorCount = subgroup.Count(),
                            selectedCount = subgroup.Count(descriptor => selected.Contains(descriptor.StableId)),
                            descriptors = subgroup
                                .OrderBy(descriptor => descriptor.SortOrder)
                                .ThenBy(descriptor => descriptor.DisplayName, StringComparer.Ordinal)
                                .Select(descriptor => BuildIntentDescriptor(descriptor, selected.Contains(descriptor.StableId)))
                                .ToArray()
                        })
                        .ToArray()
                })
                .ToArray();
        }

        private static IntentDescriptorSnapshot BuildIntentDescriptor(PyralisAuthoringCapabilityDescriptor descriptor)
        {
            return BuildIntentDescriptor(descriptor, false);
        }

        private static IntentDescriptorSnapshot BuildIntentDescriptor(
            PyralisAuthoringCapabilityDescriptor descriptor,
            bool selected)
        {
            if (descriptor == null)
                return null;

            return new IntentDescriptorSnapshot
            {
                stableId = descriptor.StableId,
                displayName = descriptor.DisplayName,
                group = descriptor.Group,
                family = descriptor.Family.ToString(),
                capability = descriptor.Capability.ToString(),
                capabilityPath = descriptor.CapabilityPath,
                pathGroup = GetCapabilityPathPart(descriptor.CapabilityPath, 0),
                pathSubgroup = GetCapabilityPathPart(descriptor.CapabilityPath, 1),
                leafLabel = FirstNonEmpty(GetCapabilityPathPart(descriptor.CapabilityPath, 2), descriptor.DisplayName),
                sortOrder = descriptor.SortOrder,
                selectableIntent = descriptor.SelectableIntent,
                selected = selected,
                sourceOrigin = descriptor.SourceOrigin.ToString(),
                axioms = descriptor.Axioms.ToString(),
                proofTargetId = descriptor.ProofTargetId,
                summary = descriptor.Summary,
                routeRelevance = descriptor.RouteRelevance,
                goalTags = descriptor.GoalTags,
                laneTags = descriptor.LaneTags,
                unsupportedLaneTags = descriptor.UnsupportedLaneTags,
                roleTags = descriptor.RoleTags,
                requiredSetup = descriptor.RequiredSetup,
                assignmentFields = descriptor.AssignmentFields,
                customizationMoments = descriptor.CustomizationMoments,
                nativeActions = descriptor.NativeActions.Select(action => BuildNativeAction(action)).ToArray(),
                sourceFact = BuildFact(descriptor.SourceFact)
            };
        }

        private static IntentRowSnapshot BuildIntentRow(PyralisAuthoringIntentRow row)
        {
            if (row == null)
                return null;

            return new IntentRowSnapshot
            {
                score = row.Score,
                state = row.State.ToString(),
                tier = row.Tier.ToString(),
                reason = row.Reason,
                fact = BuildFact(row.Fact)
            };
        }

        private static FactSnapshot BuildFact(PyralisAuthoringFact fact)
        {
            if (fact == null)
                return null;

            return new FactSnapshot
            {
                stableId = fact.StableId,
                displayName = fact.DisplayName,
                kind = fact.Kind.ToString(),
                sourceKind = fact.SourceKind.ToString(),
                confidence = fact.Confidence.ToString(),
                capability = fact.Capability.ToString(),
                axioms = fact.Axioms.ToString(),
                priority = fact.Priority,
                priorityValueOverride = fact.PriorityValueOverride,
                summary = fact.Summary,
                routeRelevance = fact.RouteRelevance,
                firstProof = fact.FirstProof,
                workIntent = fact.WorkIntent,
                goalTags = fact.GoalTags,
                laneTags = fact.LaneTags,
                unsupportedLaneTags = fact.UnsupportedLaneTags,
                requiredDefinitions = fact.RequiredDefinitions,
                requiredProfiles = fact.RequiredProfiles,
                requiredSceneComponents = fact.RequiredSceneComponents,
                requiredUnitySurfaces = fact.RequiredUnitySurfaces,
                assignmentFields = fact.AssignmentFields,
                customizationMoments = fact.CustomizationMoments,
                canWait = fact.CanWait,
                nativeActions = fact.NativeActions.Select(action => BuildNativeAction(action)).ToArray(),
                relatedStableIds = fact.RelatedStableIds,
                deprecatedInVersion = fact.DeprecatedInVersion,
                removableInVersion = fact.RemovableInVersion,
                documentationURL = fact.DocumentationURL,
                expertAdvice = fact.ExpertAdvice
            };
        }

        private static ReflectiveContractSnapshot BuildReflectiveContract(PyralisAuthoringReflectiveContractGraphRow row)
        {
            if (row == null)
                return null;

            ResolvedAuthoringContract contract = row.Contract;
            return new ReflectiveContractSnapshot
            {
                nodeId = row.Node != null ? row.Node.StableId : string.Empty,
                label = row.Label,
                evidenceState = row.EvidenceState.ToString(),
                message = row.Message,
                stableId = contract != null ? contract.StableId : string.Empty,
                displayName = contract != null ? contract.DisplayName : string.Empty,
                category = contract != null ? contract.AuthoringCategory : string.Empty,
                moduleId = contract != null ? contract.ModuleId : string.Empty,
                setupNodeId = contract != null ? contract.SetupNodeId : string.Empty,
                sourceType = contract?.SourceType != null ? contract.SourceType.FullName : string.Empty,
                capability = contract != null ? contract.Capability.ToString() : string.Empty,
                axioms = contract != null ? contract.Axioms.ToString() : string.Empty,
                capabilityPath = contract != null ? contract.CapabilityPath : string.Empty,
                selectableIntent = contract?.SelectableIntent ?? false,
                authoringLane = contract != null ? contract.AuthoringLane : string.Empty,
                relevance = contract != null ? contract.Relevance : string.Empty,
                requiredProfileType = contract?.RequiredProfileType != null ? contract.RequiredProfileType.FullName : string.Empty,
                requiredRuntimeInterfaceNames = contract != null ? contract.RequiredRuntimeInterfaceNames : Array.Empty<string>(),
                requiredComponentNames = contract != null ? contract.RequiredComponentNames : Array.Empty<string>(),
                assignmentFields = contract != null ? contract.AssignmentFields : Array.Empty<string>(),
                customizationMoments = contract != null ? contract.CustomizationMoments : Array.Empty<string>(),
                nativeSetup = contract != null ? contract.NativeSetup : Array.Empty<string>(),
                roleTags = contract != null ? contract.RoleTags : Array.Empty<string>(),
                firstProofTargetId = contract != null ? contract.FirstProofTargetId : string.Empty,
                firstProofGuidance = contract != null ? contract.FirstProofGuidance : string.Empty,
                target = BuildSourceInfo(row.Target)
            };
        }

        private static CountSnapshot[] CountCapabilityFacts(IEnumerable<PyralisAuthoringFact> facts)
        {
            return CountBy(
                facts == null
                    ? Array.Empty<PyralisAuthoringFact>()
                    : facts.Where(fact => fact != null && fact.Capability != AuthoringCapability.None),
                fact => fact.Capability.ToString());
        }

        private static CountSnapshot[] CountBy<T>(
            IEnumerable<T> values,
            Func<T, string> selector)
        {
            if (values == null)
                return Array.Empty<CountSnapshot>();

            return values
                .Where(value => value != null)
                .GroupBy(value => NormalizeCountLabel(selector(value)))
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new CountSnapshot { label = group.Key, count = group.Count() })
                .ToArray();
        }

        private static string GetCapabilityPathPart(string capabilityPath, int index)
        {
            if (string.IsNullOrWhiteSpace(capabilityPath) || index < 0)
                return string.Empty;

            string[] parts = capabilityPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            return index < parts.Length ? parts[index].Trim() : string.Empty;
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

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null)
                return string.Empty;

            for (int i = 0; i < values.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(values[i]))
                    return values[i];
            }

            return string.Empty;
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
        private sealed class RouteProofTraceSnapshot
        {
            public string schema;
            public string purpose;
            public string view;
            public string routeName;
            public string exportedAtUtc;
            public SourceSnapshot source;
            public ExportSummarySnapshot summary;
            public CurrentRouteSnapshot currentRoute;
            public string intentFocus;
            public string routeShape;
            public string proofPriority;
            public NodeSnapshot proof;
            public RouteStepSnapshot currentAction;
            public RouteStepSnapshot[] orderedSteps;
            public RouteStepSnapshot[] criticalPath;
            public RouteStepSnapshot[] proofEnhancers;
            public RouteStepSnapshot[] canWait;
            public ConnectionSnapshot[] proofBlockers;
            public ConnectionSnapshot[] proofSupport;
            public ContractPressureSnapshot[] supportingContracts;
            public GraphSummarySnapshot graphSummary;
            public TraceDiagnosticQuestionSnapshot[] diagnosticQuestions;
        }

        [Serializable]
        private sealed class RouteStepSnapshot
        {
            public int sequence;
            public string nodeId;
            public string label;
            public string phase;
            public string role;
            public string evidenceState;
            public string sourceKind;
            public string sourceOrigin;
            public string workIntent;
            public string issueSeverity;
            public string reason;
            public string message;
            public string owner;
            public string unityAction;
            public NativeActionSnapshot nativeAction;
            public string[] assignmentFields;
            public string[] nativeSetup;
            public string[] customizationMoments;
            public string proofTargetId;
            public EdgeSnapshot edge;
            public SourceSnapshot sourceObject;
        }

        [Serializable]
        private sealed class TraceDiagnosticQuestionSnapshot
        {
            public string question;
            public string answer;
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
            public ExportSummarySnapshot summary;
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
            public string participantTopology;
            public string expectedJoinPolicy;
            public string spawnPolicy;
            public int assignedParticipantCount;
            public int authoredParticipantCount;
            public int desiredParticipantCount;
            public int autoJoinParticipantCount;
            public bool autoRegisterDefaultsWithoutPlayerInput;
            public bool hasPlayerInputManager;
            public bool spawnOnRegister;
            public bool hasLocalJoinPolicyConflict;
            public string playerInputManagerIssue;
            public ParticipantSeatSnapshot[] participantSeats;
            public string[] capabilityFamilies;
            public RouteFactSnapshot[] routeFacts;
            public SourceSnapshot session;
            public SourceSnapshot mode;
            public SourceSnapshot participant;
            public SourceSnapshot pawn;
        }

        [Serializable]
        private sealed class ParticipantSeatSnapshot
        {
            public int slotIndex;
            public int seatIndex;
            public string displayName;
            public bool requiresPawn;
            public bool hasParticipant;
            public bool hasInputProfile;
            public bool hasPawnDefinition;
            public bool hasPawnPrefab;
            public bool isInputReady;
            public bool isPawnReady;
            public bool isReady;
            public string inputIssue;
            public string pawnIssue;
            public string pawnIssueKind;
            public SourceSnapshot participant;
            public SourceSnapshot pawn;
            public SourceSnapshot inputProfile;
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
        private sealed class IntentSnapshot
        {
            public string schema;
            public string purpose;
            public string view;
            public string exportedAtUtc;
            public IntentSelectionSnapshot selection;
            public IntentSummarySnapshot summary;
            public string routeShapePreview;
            public string advisorSummary;
            public IntentDescriptorGroupSnapshot[] descriptorGroups;
            public IntentDescriptorSnapshot[] selectedDescriptors;
            public IntentRowSnapshot[] recommendations;
            public IntentRowSnapshot[] cautions;
            public FactSnapshot[] matchingIntents;
        }

        [Serializable]
        private sealed class IntentSelectionSnapshot
        {
            public string lane;
            public string axioms;
            public string capabilities;
            public string participantRoute;
            public string[] selectedDescriptorIds;
        }

        [Serializable]
        private sealed class IntentSummarySnapshot
        {
            public int descriptorCount;
            public int selectableDescriptorCount;
            public int selectedDescriptorCount;
            public int recommendationCount;
            public int cautionCount;
            public int matchingIntentCount;
        }

        [Serializable]
        private sealed class IntentDescriptorGroupSnapshot
        {
            public string group;
            public int descriptorCount;
            public int selectedCount;
            public IntentDescriptorSubgroupSnapshot[] subgroups;
        }

        [Serializable]
        private sealed class IntentDescriptorSubgroupSnapshot
        {
            public string subgroup;
            public int descriptorCount;
            public int selectedCount;
            public IntentDescriptorSnapshot[] descriptors;
        }

        [Serializable]
        private sealed class IntentDescriptorSnapshot
        {
            public string stableId;
            public string displayName;
            public string group;
            public string family;
            public string capability;
            public string capabilityPath;
            public string pathGroup;
            public string pathSubgroup;
            public string leafLabel;
            public int sortOrder;
            public bool selectableIntent;
            public bool selected;
            public string sourceOrigin;
            public string axioms;
            public string proofTargetId;
            public string summary;
            public string routeRelevance;
            public string[] goalTags;
            public string[] laneTags;
            public string[] unsupportedLaneTags;
            public string[] roleTags;
            public string[] requiredSetup;
            public string[] assignmentFields;
            public string[] customizationMoments;
            public NativeActionSnapshot[] nativeActions;
            public FactSnapshot sourceFact;
        }

        [Serializable]
        private sealed class IntentRowSnapshot
        {
            public int score;
            public string state;
            public string tier;
            public string reason;
            public FactSnapshot fact;
        }

        [Serializable]
        private sealed class FactsSnapshot
        {
            public string schema;
            public string purpose;
            public string view;
            public string routeName;
            public string exportedAtUtc;
            public SourceSnapshot source;
            public FactsSummarySnapshot summary;
            public CountSnapshot[] factKindCounts;
            public CountSnapshot[] sourceKindCounts;
            public CountSnapshot[] confidenceCounts;
            public CountSnapshot[] capabilityCounts;
            public ReflectiveContractSnapshot[] graphContractCoverage;
            public ConnectionSnapshot[] graphProofCoverage;
            public FactSnapshot[] facts;
        }

        [Serializable]
        private sealed class FactsSummarySnapshot
        {
            public int factCount;
            public int graphNodeCount;
            public int graphEdgeCount;
            public int graphContractCount;
            public int proofSupportCount;
        }

        [Serializable]
        private sealed class FactSnapshot
        {
            public string stableId;
            public string displayName;
            public string kind;
            public string sourceKind;
            public string confidence;
            public string capability;
            public string axioms;
            public int priority;
            public int priorityValueOverride;
            public string summary;
            public string routeRelevance;
            public string firstProof;
            public string workIntent;
            public string[] goalTags;
            public string[] laneTags;
            public string[] unsupportedLaneTags;
            public string[] requiredDefinitions;
            public string[] requiredProfiles;
            public string[] requiredSceneComponents;
            public string[] requiredUnitySurfaces;
            public string[] assignmentFields;
            public string[] customizationMoments;
            public string[] canWait;
            public NativeActionSnapshot[] nativeActions;
            public string[] relatedStableIds;
            public string deprecatedInVersion;
            public string removableInVersion;
            public string documentationURL;
            public string expertAdvice;
        }

        [Serializable]
        private sealed class ReflectiveContractSnapshot
        {
            public string nodeId;
            public string label;
            public string evidenceState;
            public string message;
            public string stableId;
            public string displayName;
            public string category;
            public string moduleId;
            public string setupNodeId;
            public string sourceType;
            public string capability;
            public string axioms;
            public string capabilityPath;
            public bool selectableIntent;
            public string authoringLane;
            public string relevance;
            public string requiredProfileType;
            public string[] requiredRuntimeInterfaceNames;
            public string[] requiredComponentNames;
            public string[] assignmentFields;
            public string[] customizationMoments;
            public string[] nativeSetup;
            public string[] roleTags;
            public string firstProofTargetId;
            public string firstProofGuidance;
            public SourceSnapshot target;
        }

        [Serializable]
        private sealed class HygieneSnapshot
        {
            public string schema;
            public string purpose;
            public string view;
            public string exportedAtUtc;
            public SourceSnapshot source;
            public HygieneGraphContextSnapshot graphContext;
            public GraphSummarySnapshot graphSummary;
            public HygieneSummarySnapshot summary;
            public HygieneSectionSnapshot[] hygieneSections;
            public HygieneRowSnapshot[] hygieneRows;
            public ConnectionSnapshot[] proofBlockers;
            public CountSnapshot[] sourceOriginCounts;
            public CountSnapshot[] sourceKindCounts;
            public CountSnapshot[] evidenceStateCounts;
            public DependencyPressureSummarySnapshot dependencyPressureSummary;
            public DependencyPressureSnapshot[] cleanupFocus;
            public DependencyPressureSnapshot[] watchList;
            public DependencyPressureSnapshot[] dependencyPressure;
            public ContractPressureSnapshot[] contractSourcePressure;
        }

        [Serializable]
        private sealed class HygieneGraphContextSnapshot
        {
            public bool hasGraph;
            public string graphName;
            public SourceSnapshot source;
            public string note;
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
        private sealed class ExportSummarySnapshot
        {
            public string routeName;
            public string currentActionLabel;
            public string currentActionNodeId;
            public bool readyForFirstProof;
            public int nodeCount;
            public int edgeCount;
            public int mapRowCount;
            public int readyMapRowCount;
            public int missingMapRowCount;
            public int sceneSetupIssueCount;
            public int hygieneRowCount;
            public int cleanupFocusCount;
            public int watchListCount;
            public int criticalPathCount;
            public int proofEnhancerCount;
            public int proofBlockerCount;
            public int proofSupportCount;
        }

        [Serializable]
        private sealed class HygieneSummarySnapshot
        {
            public string scanScope;
            public string graphContextName;
            public bool hasGraphContext;
            public int nodeCount;
            public int edgeCount;
            public int hygieneRowCount;
            public int cleanupFocusCount;
            public int watchListCount;
            public int exportedCleanupFocusCount;
            public int exportedWatchListCount;
            public int omittedDependencyPressureCount;
            public int proofBlockerCount;
            public int dependencyPressureCount;
            public int contractSourcePressureCount;
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
            public int exportedWatchListCount;
            public int actionablePressureRecordCount;
            public int acceptedPressureRecordCount;
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
