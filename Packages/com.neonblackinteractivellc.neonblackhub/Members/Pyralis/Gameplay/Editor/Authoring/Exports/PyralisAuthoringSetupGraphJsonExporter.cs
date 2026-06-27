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
            MapSnapshot snapshot = BuildMapSnapshot(PyralisAuthoringSetupGraphProjection.BuildMapExportProjection(graph));
            return JsonUtility.ToJson(snapshot, true);
        }

        public static string ToHygieneJson(PyralisAuthoringHygieneProjection projection)
        {
            HygieneSnapshot snapshot = BuildHygieneSnapshot(projection);
            return JsonUtility.ToJson(snapshot, true);
        }

        public static string ToGuideJson(PyralisAuthoringGuideTraceProjection projection)
        {
            GuideSnapshot snapshot = BuildGuideSnapshot(projection);
            return JsonUtility.ToJson(snapshot, true);
        }

        public static string ToIntentJson(
            PyralisAuthoringIntentSelection selection,
            PyralisAuthoringIntentModel model,
            PyralisAuthoringIntentProjection projection)
        {
            return ToIntentJson(selection, model, projection, null);
        }

        public static string ToIntentJson(
            PyralisAuthoringIntentSelection selection,
            PyralisAuthoringIntentModel model,
            PyralisAuthoringIntentProjection projection,
            PyralisAuthoringSetupGraph graph)
        {
            IntentSnapshot snapshot = BuildIntentSnapshot(selection, model, projection, graph);
            return JsonUtility.ToJson(snapshot, true);
        }

        public static string ToFactsJson(PyralisAuthoringSetupGraph graph)
        {
            FactsSnapshot snapshot = BuildFactsSnapshot(graph);
            return JsonUtility.ToJson(snapshot, true);
        }

        private static IntentSnapshot BuildIntentSnapshot(
            PyralisAuthoringIntentSelection selection,
            PyralisAuthoringIntentModel model,
            PyralisAuthoringIntentProjection intentProjection,
            PyralisAuthoringSetupGraph graph = null)
        {
            selection ??= new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.None,
                AuthoringWorldAxiom.None);

            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> projectionDescriptors =
                PyralisAuthoringCapabilityDescriptorRegistry.BuildIntentProjectionDescriptors(selection.Lane, selection.Axioms);
            string[] selectedIds = PyralisAuthoringCapabilityDescriptorRegistry.FilterGameplayIntentDescriptorIds(
                selection.DescriptorIds ?? Array.Empty<string>(),
                projectionDescriptors);
            AuthoringCapability selectedCapabilities =
                PyralisAuthoringCapabilityDescriptorRegistry.BuildCapabilitiesForDescriptors(selectedIds);
            if (selectedCapabilities == AuthoringCapability.None && selectedIds.Length == 0)
                selectedCapabilities = selection.Capabilities;
            var exportSelection = new PyralisAuthoringIntentSelection(
                selection.Lane,
                selectedCapabilities,
                selection.Axioms,
                selectedIds,
                selection.ParticipantRoute);
            model ??= PyralisAuthoringSetupGraphProjection.BuildIntentModel(exportSelection);
            intentProjection ??= PyralisAuthoringIntentProjection.Build(
                exportSelection,
                projectionDescriptors);

            return new IntentSnapshot
            {
                schema = "pyralis.authoring.intentSnapshot.v1",
                purpose = "Read-only Intent tab snapshot. Describes current route steering only: DNA axioms, presentation lane, participant route, user-selected gameplay ingredients, contract-metadata backlog, inferred route essentials, and advisor rows. It does not describe scene/setup reality or proof/setup status.",
                view = "Intent",
                exportedAtUtc = DateTime.UtcNow.ToString("o"),
                selection = new IntentSelectionSnapshot
                {
                    lane = selection.Lane.ToString(),
                    axioms = selection.Axioms.ToString(),
                    capabilities = selectedCapabilities.ToString(),
                    participantRoute = selection.ParticipantRoute.ToString(),
                    selectedDescriptorIds = intentProjection.SelectedDescriptorIds
                },
                summary = new IntentSummarySnapshot
                {
                    descriptorCount = intentProjection.Descriptors.Count,
                    selectableDescriptorCount = intentProjection.Descriptors.Count(descriptor => descriptor != null && descriptor.SelectableIntent),
                    metadataBacklogCount = intentProjection.MetadataBacklogCount,
                    selectedDescriptorCount = intentProjection.SelectedGameplayIngredientCount,
                    recommendationCount = model.Recommendations.Count,
                    cautionCount = model.Cautions.Count,
                    matchingIntentCount = model.MatchingIntents.Count
                },
                shapeSummary = model.ShapeSummary,
                routeShapePreview = intentProjection.RouteShapePreview,
                lensSummary = intentProjection.LensSummary,
                advisorSummary = model.Summary,
                targetProofFocus = BuildIntentProofFocusLabel(graph, model),
                targetProofAdvice = BuildIntentProofFocusDetail(graph, model),
                targetProofSummary = BuildIntentProofFocusSummary(graph, model),
                descriptorGroups = BuildIntentDescriptorGroups(intentProjection.AllGroups),
                gameplayIngredientGroups = BuildIntentDescriptorGroups(intentProjection.GameplayIngredientGroups),
                metadataBacklogGroups = BuildIntentDescriptorGroups(intentProjection.MetadataBacklogGroups),
                routeEssentialGroups = BuildIntentDescriptorGroups(intentProjection.RouteEssentialGroups),
                selectedDescriptors = intentProjection.SelectedDescriptors.Select(BuildIntentDescriptor).ToArray(),
                recommendations = model.Recommendations.Select(BuildIntentRow).ToArray(),
                cautions = model.Cautions.Select(BuildIntentRow).ToArray(),
                matchingIntents = model.MatchingIntents.Select(BuildFact).ToArray()
            };
        }

        private static FactsSnapshot BuildFactsSnapshot(PyralisAuthoringSetupGraph graph)
        {
            IReadOnlyList<PyralisAuthoringFact> facts = PyralisAuthoringSetupGraphProjection.BuildCookbookFacts(graph)
                ?? Array.Empty<PyralisAuthoringFact>();
            PyralisAuthoringFact[] exportedFacts = facts
                .Where(IsFactsExportFact)
                .ToArray();
            IReadOnlyList<PyralisAuthoringReflectiveContractGraphRow> contractRows =
                PyralisAuthoringSetupGraphProjection.BuildReflectiveContractRows(graph);

            return new FactsSnapshot
            {
                schema = "pyralis.authoring.factsSnapshot.v1",
                purpose = "Read-only Facts tab snapshot. Describes dictionary/provenance facts and reflected contract coverage. Route intent, runtime capability routing, proof workflow, customization, and setup actions belong to Intent, Guide, or Map.",
                view = "Facts",
                routeName = graph != null ? graph.RouteName : "No setup route selected",
                exportedAtUtc = DateTime.UtcNow.ToString("o"),
                source = BuildSourceInfo(graph?.Source),
                summary = new FactsSummarySnapshot
                {
                    factCount = exportedFacts.Length,
                    graphNodeCount = graph?.Nodes.Count ?? 0,
                    graphEdgeCount = graph?.Edges.Count ?? 0,
                    graphContractCount = contractRows.Count
                },
                factKindCounts = CountBy(exportedFacts, fact => fact.Kind.ToString()),
                sourceKindCounts = CountBy(exportedFacts, fact => fact.SourceKind.ToString()),
                confidenceCounts = CountBy(exportedFacts, fact => fact.Confidence.ToString()),
                graphContractCoverage = contractRows.Select(BuildReflectiveContract).ToArray(),
                facts = exportedFacts.Select(BuildDictionaryFact).ToArray()
            };
        }

        private static bool IsFactsExportFact(PyralisAuthoringFact fact)
        {
            if (fact == null)
                return false;

            return fact.Kind != PyralisAuthoringFactKind.RouteIntent
                && fact.Kind != PyralisAuthoringFactKind.RuntimeCapability
                && fact.Kind != PyralisAuthoringFactKind.CustomizationMoment
                && fact.Kind != PyralisAuthoringFactKind.Proof;
        }

        private static MapSnapshot BuildMapSnapshot(PyralisAuthoringMapExportProjection projection)
        {
            PyralisAuthoringSetupGraph graph = projection?.Graph;
            MapRowSnapshot[] mapRows = (projection?.MapRows ?? Array.Empty<PyralisAuthoringSetupGraphRow>())
                .Select(BuildMapRow)
                .ToArray();
            ConnectionSnapshot[] mapConnections = (projection?.MapConnections ?? Array.Empty<PyralisAuthoringGraphConnectionRow>())
                .Select(BuildConnection)
                .ToArray();
            MapIssueSnapshot[] sceneSetupIssues = (projection?.SceneSetupIssues ?? Array.Empty<PyralisAuthoringGraphAuditRow>())
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
                nodeCount = projection?.Nodes.Count ?? 0,
                edgeCount = projection?.Edges.Count ?? 0,
                nodes = (projection?.Nodes ?? Array.Empty<PyralisAuthoringGraphNode>()).Select(BuildNode).ToArray(),
                edges = (projection?.Edges ?? Array.Empty<PyralisAuthoringGraphEdge>()).Select(BuildEdge).ToArray(),
                mapRows = mapRows,
                mapConnections = mapConnections,
                sceneSurfaces = (projection?.SceneSurfaces ?? Array.Empty<PyralisAuthoringGraphNode>())
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

        private static HygieneSnapshot BuildHygieneSnapshot(PyralisAuthoringHygieneProjection projection)
        {
            projection ??= PyralisAuthoringHygieneProjection.Build(null, null, Array.Empty<PyralisSourceDependencyHygieneRecord>());
            PyralisAuthoringSetupGraph graph = projection.Graph;
            IReadOnlyList<PyralisSourceDependencyHygieneRecord> safeDependencyRecords = projection.DependencyRecords;

            return new HygieneSnapshot
            {
                schema = "pyralis.authoring.hygieneSnapshot.v2",
                purpose = "Read-only Hygiene tab snapshot. Describes graph integrity, source origins, dependency pressure, and contract source pressure. Scene/setup repair issues belong to Map; route setup ordering belongs to Guide.",
                view = "Hygiene",
                exportedAtUtc = DateTime.UtcNow.ToString("o"),
                source = BuildSourceInfo(graph?.Source),
                graphContext = BuildHygieneGraphContext(graph),
                graphSummary = BuildGraphSummary(PyralisAuthoringSetupGraphProjection.BuildGraphSummaryProjection(graph, safeDependencyRecords, projection.ProofBlockers)),
                summary = BuildHygieneSummary(projection),
                hygieneSections = projection.Sections
                    .Select(BuildHygieneSection)
                    .ToArray(),
                hygieneRows = projection.DetailRows
                    .Select(BuildHygieneRow)
                    .ToArray(),
                proofBlockers = projection.ProofBlockers.Select(BuildConnection).ToArray(),
                sourceOriginCounts = CountBy(graph?.Nodes, node => node.SourceOrigin.ToString()),
                sourceKindCounts = CountBy(graph?.Nodes, node => node.SourceKind.ToString()),
                evidenceStateCounts = CountBy(graph?.Nodes, node => node.EvidenceState.ToString()),
                ownershipBucketCounts = CountBy(projection.DetailRows, row => row.OwnershipBucket),
                dependencyPressureSummary = BuildDependencyPressureSummary(safeDependencyRecords),
                cleanupFocus = projection.CleanupFocus
                    .Select(BuildDependencyPressure)
                    .ToArray(),
                watchList = projection.WatchList
                    .Select(BuildDependencyPressure)
                    .ToArray(),
                dependencyPressure = projection.DependencyPressureRows
                    .Select(BuildDependencyPressure)
                    .ToArray(),
                contractSourcePressure = projection.ContractSourcePressureRows
                    .Select(node => BuildContractPressure(node))
                    .ToArray()
            };
        }

        private static GuideSnapshot BuildGuideSnapshot(PyralisAuthoringGuideTraceProjection trace)
        {
            PyralisAuthoringSetupGraph graph = trace?.Graph;
            PyralisAuthoringRouteWorkingProjection route = trace?.Route
                ?? PyralisAuthoringSetupGraphProjection.BuildRouteWorkingProjection(graph);
            RouteStepSnapshot[] orderedSteps = route.OrderedSteps.Select(BuildRouteStep).ToArray();
            RouteStepSnapshot[] criticalPath = route.CriticalPath.Select(BuildRouteStep).ToArray();
            RouteStepSnapshot[] proofEnhancers = route.ProofEnhancers.Select(BuildRouteStep).ToArray();
            RouteStepSnapshot[] canWait = route.CanWait.Select(BuildRouteStep).ToArray();
            ConnectionSnapshot[] proofBlockers = route.ProofBlockers.Select(BuildConnection).ToArray();
            ConnectionSnapshot[] proofSupport = route.ProofSupport.Select(BuildConnection).ToArray();

            return new GuideSnapshot
            {
                schema = "pyralis.authoring.guide.v1",
                purpose = "Read-only Guide tab snapshot. Saves the same Guide projection rendered by the tab: ordered setup cards, the selected proof, current action, blockers, proof support, Guide Trace details, and the final Play Mode attempt. This is documentation/debug evidence, not a preset or scene generator.",
                view = "Guide",
                routeName = graph != null ? graph.RouteName : "No setup route selected",
                exportedAtUtc = DateTime.UtcNow.ToString("o"),
                source = BuildSourceInfo(graph?.Source),
                summary = BuildGuideTraceSummary(graph, route),
                currentRoute = BuildCurrentRoute(graph?.RouteAnalysis, route),
                intentFocus = PyralisAuthoringSetupGraphProjection.BuildIntentFocusSummary(graph),
                routeShape = PyralisAuthoringSetupGraphProjection.BuildRouteShapeSummary(graph),
                proofPriority = PyralisAuthoringSetupGraphProjection.BuildProofPrioritySummary(graph),
                proof = BuildProofTarget(route.Proof, route),
                currentAction = BuildCurrentAction(route.CurrentAction, route),
                orderedSteps = orderedSteps,
                criticalPath = criticalPath,
                proofEnhancers = proofEnhancers,
                canWait = canWait,
                proofBlockers = proofBlockers,
                proofSupport = proofSupport,
                supportingContracts = (trace?.SupportingContracts ?? Array.Empty<PyralisAuthoringGraphNode>())
                    .Select(node => BuildContractPressure(node, includeInferredRuntimeFamilies: false))
                    .ToArray(),
                graphSummary = BuildGraphSummary(PyralisAuthoringSetupGraphProjection.BuildGraphSummaryProjection(graph, Array.Empty<PyralisSourceDependencyHygieneRecord>(), route.ProofBlockers)),
                diagnosticQuestions = (trace?.DiagnosticQuestions ?? Array.Empty<PyralisAuthoringRouteDiagnosticQuestionRow>())
                    .Select(BuildTraceDiagnosticQuestion)
                    .ToArray()
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
                readyForProof = false,
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

        private static HygieneSummarySnapshot BuildHygieneSummary(PyralisAuthoringHygieneProjection projection)
        {
            PyralisAuthoringSetupGraph graph = projection?.Graph;
            IReadOnlyList<PyralisSourceDependencyHygieneRecord> dependencyRecords = projection?.DependencyRecords;
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
                hygieneRowCount = projection?.DetailRows.Count ?? 0,
                cleanupFocusCount = projection?.CleanupFocusCount ?? 0,
                watchListCount = projection?.WatchListCount ?? 0,
                exportedCleanupFocusCount = projection?.CleanupFocus.Count ?? 0,
                exportedWatchListCount = projection?.WatchList.Count ?? 0,
                omittedDependencyPressureCount = Math.Max(0, pressureRecords.Length - (projection?.DependencyPressureRows.Count ?? 0)),
                proofBlockerCount = projection?.ProofBlockers.Count ?? 0,
                dependencyPressureCount = pressureRecords.Length,
                contractSourcePressureCount = projection?.ContractSourcePressureRows.Count ?? 0
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

        private static ExportSummarySnapshot BuildGuideTraceSummary(
            PyralisAuthoringSetupGraph graph,
            PyralisAuthoringRouteWorkingProjection route)
        {
            return new ExportSummarySnapshot
            {
                routeName = graph != null ? graph.RouteName : "No setup route selected",
                currentActionLabel = route?.CurrentAction != null ? route.CurrentAction.Label : string.Empty,
                currentActionNodeId = route?.CurrentAction != null ? route.CurrentAction.StableId : string.Empty,
                readyForProof = route != null && route.ReadyForProof,
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
                evidenceState = row != null ? row.EvidenceState.ToString() : string.Empty,
                sourceKind = node != null ? node.SourceKind.ToString() : string.Empty,
                sourceOrigin = row != null ? row.SourceOrigin.ToString() : string.Empty,
                workIntent = node != null ? node.WorkIntent.ToString() : string.Empty,
                issueSeverity = node != null ? node.IssueSeverity.ToString() : string.Empty,
                setupDomain = node != null ? node.SetupDomain.ToString() : string.Empty,
                issueCode = node != null ? node.IssueCode : string.Empty,
                reason = row?.Reason ?? string.Empty,
                message = row?.Message ?? string.Empty,
                owner = row?.OwnerLabel ?? string.Empty,
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

        private static string BuildIntentProofFocusLabel(
            PyralisAuthoringSetupGraph graph,
            PyralisAuthoringIntentModel model)
        {
            PyralisAuthoringGraphNode proof = PyralisAuthoringSetupGraphProjection.FindCurrentProofNode(graph);
            if (proof != null && !string.IsNullOrWhiteSpace(proof.Label))
                return proof.Label;

            return model != null ? model.ProofFocusLabel : string.Empty;
        }

        private static string BuildIntentProofFocusDetail(
            PyralisAuthoringSetupGraph graph,
            PyralisAuthoringIntentModel model)
        {
            PyralisAuthoringGraphNode proof = PyralisAuthoringSetupGraphProjection.FindCurrentProofNode(graph);
            if (proof != null)
            {
                string detail = FirstNonEmpty(proof.Guidance, proof.BlockingReason);
                if (!string.IsNullOrWhiteSpace(detail))
                    return detail;
            }

            return model != null ? model.ProofFocusDetail : string.Empty;
        }

        private static string BuildIntentProofFocusSummary(
            PyralisAuthoringSetupGraph graph,
            PyralisAuthoringIntentModel model)
        {
            string label = BuildIntentProofFocusLabel(graph, model);
            string detail = BuildIntentProofFocusDetail(graph, model);
            if (string.IsNullOrWhiteSpace(label))
                return detail;

            return string.IsNullOrWhiteSpace(detail)
                ? "Target proof: " + label
                : "Target proof: " + label + ". " + detail;
        }

        private static CurrentActionSnapshot BuildCurrentAction(
            PyralisAuthoringRouteStepRow row,
            PyralisAuthoringRouteWorkingProjection route)
        {
            if (row == null)
            {
                return new CurrentActionSnapshot
                {
                    hasAction = false,
                    status = route != null && route.ReadyForProof
                        ? "Required setup is clear for the selected proof."
                        : "No current action is projected.",
                    sequence = 0,
                    nodeId = string.Empty,
                    label = string.Empty,
                    phase = string.Empty,
                    role = string.Empty,
                    evidenceState = string.Empty,
                    issueSeverity = string.Empty,
                    setupDomain = string.Empty,
                    issueCode = string.Empty,
                    reason = string.Empty,
                    message = string.Empty,
                    owner = string.Empty,
                    unityAction = string.Empty,
                    nativeAction = null,
                    assignmentFields = Array.Empty<string>(),
                    nativeSetup = Array.Empty<string>(),
                    proofTargetId = string.Empty
                };
            }

            PyralisAuthoringGraphNode node = row.Node;
            return new CurrentActionSnapshot
            {
                hasAction = true,
                status = "Next route-required action before the selected proof.",
                sequence = row.Sequence,
                nodeId = row.StableId,
                label = row.Label,
                phase = row.PhaseLabel,
                role = row.RoleLabel,
                evidenceState = row.EvidenceState.ToString(),
                issueSeverity = node != null ? node.IssueSeverity.ToString() : string.Empty,
                setupDomain = node != null ? node.SetupDomain.ToString() : string.Empty,
                issueCode = node != null ? node.IssueCode : string.Empty,
                reason = row.Reason,
                message = row.Message,
                owner = row.OwnerLabel,
                unityAction = row.UnityActionLabel,
                nativeAction = BuildNativeAction(row.NativeAction),
                assignmentFields = row.AssignmentFields,
                nativeSetup = row.NativeSetup,
                proofTargetId = node != null ? node.ProofTargetId : string.Empty
            };
        }

        private static ProofTargetSnapshot BuildProofTarget(
            PyralisAuthoringGraphNode proof,
            PyralisAuthoringRouteWorkingProjection route)
        {
            if (proof == null)
            {
                return new ProofTargetSnapshot
                {
                    hasProof = false,
                    id = string.Empty,
                    label = string.Empty,
                    proofTargetId = string.Empty,
                    evidenceState = string.Empty,
                    readyForAttempt = false,
                    guidance = "No proof target was projected.",
                    playModeAction = string.Empty,
                    sourceKind = string.Empty,
                    sourceOrigin = string.Empty,
                    supportingContractCount = route?.ProofSupport.Count ?? 0,
                    proofBlockerCount = route?.ProofBlockers.Count ?? 0
                };
            }

            return new ProofTargetSnapshot
            {
                hasProof = true,
                id = proof.StableId,
                label = proof.Label,
                proofTargetId = proof.ProofTargetId,
                evidenceState = proof.EvidenceState.ToString(),
                readyForAttempt = route != null && route.ReadyForProof,
                guidance = proof.Guidance,
                playModeAction = FirstNonEmpty(proof.NativeSetup),
                sourceKind = proof.SourceKind.ToString(),
                sourceOrigin = proof.SourceOrigin.ToString(),
                supportingContractCount = route?.ProofSupport.Count ?? 0,
                proofBlockerCount = route?.ProofBlockers.Count ?? 0
            };
        }

        private static string FirstNonEmpty(IReadOnlyList<string> values)
        {
            if (values == null)
                return string.Empty;

            for (int i = 0; i < values.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(values[i]))
                    return values[i];
            }

            return string.Empty;
        }

        private static TraceDiagnosticQuestionSnapshot BuildTraceDiagnosticQuestion(PyralisAuthoringRouteDiagnosticQuestionRow row)
        {
            return new TraceDiagnosticQuestionSnapshot
            {
                question = row != null ? row.Question : string.Empty,
                answer = row != null ? row.Answer : string.Empty
            };
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
                exportedTopRecordCount = pressureRecords.Length,
                exportedCleanupFocusCount = CountCleanupFocusRecords(pressureRecords),
                exportedWatchListCount = CountWatchListRecords(pressureRecords),
                actionablePressureRecordCount = CountActionablePressureRecords(pressureRecords),
                acceptedPressureRecordCount = CountWatchListRecords(pressureRecords),
                expectedPressureRecordCount = Math.Max(0, pressureRecords.Length - CountActionablePressureRecords(pressureRecords)),
                omittedRecordCount = 0,
                highestRiskScore = pressureRecords.Length > 0 ? pressureRecords[0].RiskScore : 0,
                riskCounts = CountBy(pressureRecords, record => record.Risk.ToString()),
                pressureKindCounts = CountBy(pressureRecords, record => record.PressureKind.ToString()),
                ownerDomainCounts = CountBy(pressureRecords, record => record.OwnerDomain),
                touchedDomainCounts = CountDomains(pressureRecords)
            };
        }

        private static GraphSummarySnapshot BuildGraphSummary(PyralisAuthoringGraphSummaryProjection projection)
        {
            return new GraphSummarySnapshot
            {
                nodeCount = projection?.NodeCount ?? 0,
                edgeCount = projection?.EdgeCount ?? 0,
                unknownNodeCount = projection?.UnknownNodeCount ?? 0,
                missingNodeCount = projection?.MissingNodeCount ?? 0,
                blockedNodeCount = projection?.BlockedNodeCount ?? 0,
                setupReadinessUnknownNodeCount = projection?.SetupReadinessUnknownNodeCount ?? 0,
                setupReadinessMissingNodeCount = projection?.SetupReadinessMissingNodeCount ?? 0,
                setupReadinessBlockedNodeCount = projection?.SetupReadinessBlockedNodeCount ?? 0,
                contractMetadataIssueCount = projection?.ContractMetadataIssueCount ?? 0,
                contractInventoryNodeCount = projection?.ContractInventoryNodeCount ?? 0,
                proofBlockerCount = projection?.ProofBlockerCount ?? 0,
                dependencyPressureCount = projection?.DependencyPressureCount ?? 0,
                contractNodeCount = projection?.ContractNodeCount ?? 0,
                hygieneUnknownRowCount = projection?.HygieneUnknownRowCount ?? 0,
                hygieneMissingRowCount = projection?.HygieneMissingRowCount ?? 0,
                hygieneBlockedRowCount = projection?.HygieneBlockedRowCount ?? 0
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
                setupDomain = node.SetupDomain.ToString(),
                issueCode = node.IssueCode,
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
                issueCode = row.IssueCode,
                triageBucket = row.TriageBucket,
                triageAdvice = row.TriageAdvice,
                ownershipBucket = row.OwnershipBucket,
                repairOwner = row.RepairOwner,
                ownershipAdvice = row.OwnershipAdvice,
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
                || pressureKind == PyralisSourceDependencyPressureKind.DirectSceneQuerySurface
                || PyralisSourceDependencyHygieneScanner.IsOwnershipLeakPressure(pressureKind);
        }

        private static bool IsActionablePressure(PyralisSourceDependencyPressureKind pressureKind)
        {
            return pressureKind == PyralisSourceDependencyPressureKind.RuntimeOwnership
                || pressureKind == PyralisSourceDependencyPressureKind.DirectSceneQuerySurface
                || PyralisSourceDependencyHygieneScanner.IsOwnershipLeakPressure(pressureKind);
        }

        private static ContractPressureSnapshot BuildContractPressure(
            PyralisAuthoringGraphNode node,
            bool includeInferredRuntimeFamilies = true)
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
                declaredRuntimeFamilies = contract != null
                    ? contract.RuntimeFamilies.Select(family => family.ToString()).ToArray()
                    : Array.Empty<string>(),
                inferredRuntimeFamilies = contract != null && includeInferredRuntimeFamilies
                    ? PyralisAuthoringContractMetadataPolicy.InferRuntimeFamilies(contract)
                        .Select(family => family.ToString())
                        .ToArray()
                    : Array.Empty<string>(),
                supportOnly = contract != null && PyralisAuthoringContractMetadataPolicy.IsSupportOnlyContract(contract),
                assignmentFieldCount = node.AssignmentFields.Length,
                customizationMomentCount = node.CustomizationMoments.Length,
                nativeSetupCount = node.NativeSetup.Length
            };
        }

        private static IntentDescriptorGroupSnapshot[] BuildIntentDescriptorGroups(
            IReadOnlyList<PyralisAuthoringIntentDescriptorGroupProjection> groups)
        {
            if (groups == null || groups.Count == 0)
                return Array.Empty<IntentDescriptorGroupSnapshot>();

            return groups
                .Select(group => new IntentDescriptorGroupSnapshot
                {
                    group = group.Group,
                    descriptorCount = group.DescriptorCount,
                    selectedCount = group.SelectedCount,
                    inferredCount = group.InferredCount,
                    subgroups = group.Subgroups
                        .Select(subgroup => new IntentDescriptorSubgroupSnapshot
                        {
                            subgroup = subgroup.Subgroup,
                            descriptorCount = subgroup.DescriptorCount,
                            selectedCount = subgroup.SelectedCount,
                            inferredCount = subgroup.InferredCount,
                            descriptors = subgroup.Descriptors.Select(BuildIntentDescriptor).ToArray()
                        })
                        .ToArray()
                })
                .ToArray();
        }

        private static IntentDescriptorSnapshot BuildIntentDescriptor(PyralisAuthoringCapabilityDescriptor descriptor)
        {
            return BuildIntentDescriptor(descriptor, false, false, string.Empty);
        }

        private static IntentDescriptorSnapshot BuildIntentDescriptor(
            PyralisAuthoringCapabilityDescriptor descriptor,
            bool selected)
        {
            return BuildIntentDescriptor(descriptor, selected, false, string.Empty);
        }

        private static IntentDescriptorSnapshot BuildIntentDescriptor(
            PyralisAuthoringCapabilityDescriptor descriptor,
            bool selected,
            bool inferred,
            string intentLayer)
        {
            if (descriptor == null)
                return null;

            return new IntentDescriptorSnapshot
            {
                stableId = descriptor.StableId,
                displayName = descriptor.DisplayName,
                group = descriptor.Group,
                family = descriptor.Family.ToString(),
                runtimeFamilies = new[] { descriptor.Family.ToString() },
                capability = descriptor.Capability.ToString(),
                capabilityPath = descriptor.CapabilityPath,
                pathGroup = GetCapabilityPathPart(descriptor.CapabilityPath, 0),
                pathSubgroup = GetCapabilityPathPart(descriptor.CapabilityPath, 1),
                leafLabel = FirstNonEmpty(GetCapabilityPathPart(descriptor.CapabilityPath, 2), descriptor.DisplayName),
                sortOrder = descriptor.SortOrder,
                selectableIntent = descriptor.SelectableIntent,
                surface = descriptor.Surface.ToString(),
                selected = selected,
                inferred = inferred,
                intentLayer = intentLayer ?? string.Empty,
                sourceOrigin = descriptor.SourceOrigin.ToString(),
                axioms = descriptor.Axioms.ToString(),
                proofTargetId = descriptor.ProofTargetId,
                summary = descriptor.Summary,
                routeRelevance = descriptor.RouteRelevance,
                goalTags = descriptor.GoalTags,
                laneTags = descriptor.LaneTags,
                unsupportedLaneTags = descriptor.UnsupportedLaneTags,
                roleTags = descriptor.RoleTags,
                sourceFact = BuildFact(descriptor.SourceFact)
            };
        }

        private static IntentDescriptorSnapshot BuildIntentDescriptor(
            PyralisAuthoringIntentDescriptorProjection projected)
        {
            if (projected == null)
                return null;

            IntentDescriptorSnapshot snapshot = BuildIntentDescriptor(
                projected.Descriptor,
                projected.Selected,
                projected.Inferred,
                projected.IntentLayer);
            if (snapshot != null)
            {
                snapshot.pathGroup = projected.Group;
                snapshot.pathSubgroup = projected.Subgroup;
                snapshot.leafLabel = projected.LeafLabel;
            }

            return snapshot;
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
            return BuildDictionaryFact(fact);
        }

        private static FactSnapshot BuildDictionaryFact(PyralisAuthoringFact fact)
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
                priority = fact.Priority,
                priorityValueOverride = fact.PriorityValueOverride,
                summary = fact.Summary,
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
                requiredProfileType = contract?.RequiredProfileType != null ? contract.RequiredProfileType.FullName : string.Empty,
                requiredRuntimeInterfaceNames = contract != null ? contract.RequiredRuntimeInterfaceNames : Array.Empty<string>(),
                requiredComponentNames = contract != null ? contract.RequiredComponentNames : Array.Empty<string>(),
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
                .Select(group => new CountSnapshot { label = group.Key, count = group.Count() })
                .ToArray();
        }

        private static CountSnapshot[] CountBy(
            IEnumerable<PyralisAuthoringGraphAuditRow> rows,
            Func<PyralisAuthoringGraphAuditRow, string> selector)
        {
            if (rows == null)
                return Array.Empty<CountSnapshot>();

            return rows
                .Where(row => row != null)
                .GroupBy(row => NormalizeCountLabel(selector(row)))
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
                fieldOrComponent = PyralisAuthoringLabelUtility.GetNativeActionFieldOrComponentName(action),
                fieldOrComponentInstruction = action.FieldOrComponent,
                displayLabel = PyralisAuthoringLabelUtility.GetNativeActionDisplayLabel(action),
                owner = PyralisAuthoringLabelUtility.GetNativeActionOwnerLabel(action),
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
        private sealed class GuideSnapshot
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
            public ProofTargetSnapshot proof;
            public CurrentActionSnapshot currentAction;
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
            public string setupDomain;
            public string issueCode;
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
        private sealed class CurrentActionSnapshot
        {
            public bool hasAction;
            public string status;
            public int sequence;
            public string nodeId;
            public string label;
            public string phase;
            public string role;
            public string evidenceState;
            public string issueSeverity;
            public string setupDomain;
            public string issueCode;
            public string reason;
            public string message;
            public string owner;
            public string unityAction;
            public NativeActionSnapshot nativeAction;
            public string[] assignmentFields;
            public string[] nativeSetup;
            public string proofTargetId;
        }

        [Serializable]
        private sealed class ProofTargetSnapshot
        {
            public bool hasProof;
            public string id;
            public string label;
            public string proofTargetId;
            public string evidenceState;
            public bool readyForAttempt;
            public string guidance;
            public string playModeAction;
            public string sourceKind;
            public string sourceOrigin;
            public int supportingContractCount;
            public int proofBlockerCount;
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
            public string shapeSummary;
            public string routeShapePreview;
            public string lensSummary;
            public string advisorSummary;
            public string targetProofFocus;
            public string targetProofAdvice;
            public string targetProofSummary;
            public IntentDescriptorGroupSnapshot[] descriptorGroups;
            public IntentDescriptorGroupSnapshot[] gameplayIngredientGroups;
            public IntentDescriptorGroupSnapshot[] metadataBacklogGroups;
            public IntentDescriptorGroupSnapshot[] routeEssentialGroups;
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
            public int metadataBacklogCount;
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
            public int inferredCount;
            public IntentDescriptorSubgroupSnapshot[] subgroups;
        }

        [Serializable]
        private sealed class IntentDescriptorSubgroupSnapshot
        {
            public string subgroup;
            public int descriptorCount;
            public int selectedCount;
            public int inferredCount;
            public IntentDescriptorSnapshot[] descriptors;
        }

        [Serializable]
        private sealed class IntentDescriptorSnapshot
        {
            public string stableId;
            public string displayName;
            public string group;
            public string family;
            public string[] runtimeFamilies;
            public string capability;
            public string capabilityPath;
            public string pathGroup;
            public string pathSubgroup;
            public string leafLabel;
            public int sortOrder;
            public bool selectableIntent;
            public string surface;
            public bool selected;
            public bool inferred;
            public string intentLayer;
            public string sourceOrigin;
            public string axioms;
            public string proofTargetId;
            public string summary;
            public string routeRelevance;
            public string[] goalTags;
            public string[] laneTags;
            public string[] unsupportedLaneTags;
            public string[] roleTags;
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
            public ReflectiveContractSnapshot[] graphContractCoverage;
            public FactSnapshot[] facts;
        }

        [Serializable]
        private sealed class FactsSummarySnapshot
        {
            public int factCount;
            public int graphNodeCount;
            public int graphEdgeCount;
            public int graphContractCount;
        }

        [Serializable]
        private sealed class FactSnapshot
        {
            public string stableId;
            public string displayName;
            public string kind;
            public string sourceKind;
            public string confidence;
            public int priority;
            public int priorityValueOverride;
            public string summary;
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
            public string requiredProfileType;
            public string[] requiredRuntimeInterfaceNames;
            public string[] requiredComponentNames;
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
            public CountSnapshot[] ownershipBucketCounts;
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
            public int setupReadinessUnknownNodeCount;
            public int setupReadinessMissingNodeCount;
            public int setupReadinessBlockedNodeCount;
            public int contractMetadataIssueCount;
            public int contractInventoryNodeCount;
            public int proofBlockerCount;
            public int dependencyPressureCount;
            public int contractNodeCount;
            public int hygieneUnknownRowCount;
            public int hygieneMissingRowCount;
            public int hygieneBlockedRowCount;
        }

        [Serializable]
        private sealed class ExportSummarySnapshot
        {
            public string routeName;
            public string currentActionLabel;
            public string currentActionNodeId;
            public bool readyForProof;
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
            public string setupDomain;
            public string issueCode;
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
            public string issueCode;
            public string triageBucket;
            public string triageAdvice;
            public string ownershipBucket;
            public string repairOwner;
            public string ownershipAdvice;
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
            public string[] declaredRuntimeFamilies;
            public string[] inferredRuntimeFamilies;
            public bool supportOnly;
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
            public string fieldOrComponentInstruction;
            public string displayLabel;
            public string owner;
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
