using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Editor.Inspectors;

namespace NeonBlack.Gameplay.Editor
{
    public static class PyralisAuthoringSetupGraphBuilder
    {
        public static PyralisAuthoringSetupGraph Build(UnityEngine.Object source)
        {
            return Build(source, default);
        }

        public static PyralisAuthoringSetupGraph Build(
            UnityEngine.Object source,
            PyralisAuthoringIntentSelection intentSelection)
        {
            PyralisSetupRouteAnalysis route = BuildRoute(source, intentSelection);
            List<PyralisAuthoringGraphNode> nodes = new List<PyralisAuthoringGraphNode>();
            List<PyralisAuthoringGraphEdge> edges = new List<PyralisAuthoringGraphEdge>();

            AddSetupChainNodes(source, route, nodes, edges);
            AddCapabilityNodes(route, intentSelection, nodes, edges);
            AddRouteShapeNode(route, nodes, edges);
            AddParticipantTopologyNode(route, nodes, edges);
            AddParticipantNodes(route, nodes, edges);
            AddParticipantSeatNodes(route, nodes, edges);
            AddSceneSurfaceNodes(source, route, nodes, edges);
            string activeProofNodeId = AddProofNode(route, intentSelection, nodes, edges);
            AddContractNodes(nodes, edges, activeProofNodeId);
            AddRuntimeValidationEvidence(source, route, nodes, edges);
            AddReflectedDependencyEvidence(source, route, nodes, edges);
            AddSceneReadinessEvidence(source, nodes, edges);
            AddCameraFocusEvidence(route, nodes, edges);
            AddProofBlockerEdges(nodes, edges, activeProofNodeId);
            ResolveProofReadiness(nodes, edges, activeProofNodeId);

            return new PyralisAuthoringSetupGraph(source, route, nodes, edges, intentSelection);
        }

        private static PyralisSetupRouteAnalysis BuildRoute(
            UnityEngine.Object source,
            PyralisAuthoringIntentSelection intentSelection)
        {
            RuntimeCapabilityFamily[] focusedFamilies = BuildIntentFocusedFamilies(intentSelection);
            PyralisSetupRouteAnalysis route;
            if (source is GameplaySessionBootstrap bootstrap)
                route = PyralisSetupRouteAnalysis.Build(bootstrap);
            else if (source is SessionDefinition session)
                route = PyralisSetupRouteAnalysis.Build(session);
            else if (source is GameModeDefinition mode)
                route = PyralisSetupRouteAnalysis.Build(mode);
            else
                route = PyralisSetupRouteAnalysis.Build(source);

            return PyralisSetupRouteAnalysis.WithIntentFocus(route, focusedFamilies, intentSelection);
        }

        private static RuntimeCapabilityFamily[] BuildIntentFocusedFamilies(PyralisAuthoringIntentSelection intentSelection)
        {
            if (intentSelection == null)
                return Array.Empty<RuntimeCapabilityFamily>();

            if (intentSelection.DescriptorIds != null && intentSelection.DescriptorIds.Length > 0)
            {
                RuntimeCapabilityFamily[] descriptorFamilies =
                    PyralisAuthoringCapabilityDescriptorRegistry.BuildRuntimeFamiliesForDescriptors(
                        intentSelection.DescriptorIds,
                        intentSelection.Lane,
                        intentSelection.Axioms);
                if (descriptorFamilies.Length > 0)
                    return descriptorFamilies;
            }

            if (intentSelection.Capabilities == AuthoringCapability.None)
                return Array.Empty<RuntimeCapabilityFamily>();

            return PyralisAuthoringCapabilityDescriptorRegistry.BuildRuntimeFamilies(
                intentSelection.Capabilities,
                intentSelection.Lane,
                intentSelection.Axioms);
        }

        private static void AddSetupChainNodes(
            UnityEngine.Object source,
            PyralisSetupRouteAnalysis route,
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges)
        {
            AddNode(nodes, new PyralisAuthoringGraphNode(
                "bootstrap.root",
                "Gameplay Root",
                PyralisAuthoringGraphNodeKind.SetupChain,
                PyralisAuthoringGraphSourceKind.CoreSetup,
                source is GameplaySessionBootstrap ? PyralisAuthoringGraphEvidenceState.Ready : PyralisAuthoringGraphEvidenceState.Missing,
                guidance: source is GameplaySessionBootstrap
                    ? "GameplaySessionBootstrap is the active setup root."
                    : "Create or select a Gameplay Root scene object with GameplaySessionBootstrap before wiring SessionDefinition, participants, pawn, input, and camera.",
                nativeSetup: new[] { "Hierarchy -> Create Empty -> name it Gameplay Root; Inspector -> Add Component -> GameplaySessionBootstrap" },
                nativeAction: source is GameplaySessionBootstrap
                    ? null
                    : new PyralisAuthoringNativeAction(
                        "Create or select",
                        PyralisAuthoringActionSurface.Hierarchy,
                        "Gameplay Root",
                        "right-click -> Create Empty, name it Gameplay Root, then use Inspector -> Add Component -> GameplaySessionBootstrap",
                        "Overview can inspect the active setup route"),
                sourceObject: source as GameplaySessionBootstrap,
                sourceOrigin: PyralisAuthoringGraphSourceOrigin.SpineGrammar,
                setupDomain: PyralisAuthoringGraphSetupDomain.GameplayRoot));

            AddNode(nodes, new PyralisAuthoringGraphNode(
                "session.definition",
                "Session Definition",
                PyralisAuthoringGraphNodeKind.SetupChain,
                PyralisAuthoringGraphSourceKind.CoreSetup,
                route != null && route.Session != null ? PyralisAuthoringGraphEvidenceState.Ready : PyralisAuthoringGraphEvidenceState.Missing,
                guidance: route != null && route.Session != null
                    ? "SessionDefinition is assigned and can provide mode and participant setup."
                    : "Create a SessionDefinition asset and assign it to GameplaySessionBootstrap.sessionDefinition.",
                nativeSetup: route != null && route.Session != null
                    ? Array.Empty<string>()
                    : new[] { "Project -> Create -> NeonBlack -> Definitions -> Session Definition; assign it to GameplaySessionBootstrap.sessionDefinition." },
                assignmentFields: new[] { "GameplaySessionBootstrap.sessionDefinition" },
                blockingReason: route != null && route.Session != null
                    ? string.Empty
                    : "The route needs a SessionDefinition before it can inspect mode, participants, pawns, input, and camera setup.",
                nativeAction: route != null && route.Session != null
                    ? null
                    : new PyralisAuthoringNativeAction(
                        "Create or assign",
                        PyralisAuthoringActionSurface.Inspector,
                        "GameplaySessionBootstrap",
                        "sessionDefinition",
                        "GameplaySessionBootstrap references the SessionDefinition asset"),
                sourceObject: route?.Session,
                sourceOrigin: route != null && route.Session != null
                    ? PyralisAuthoringGraphSourceOrigin.UserAuthoredSetup
                    : PyralisAuthoringGraphSourceOrigin.SpineGrammar,
                setupDomain: PyralisAuthoringGraphSetupDomain.Session));

            AddNode(nodes, new PyralisAuthoringGraphNode(
                "mode.definition",
                "Game Mode Definition",
                PyralisAuthoringGraphNodeKind.SetupChain,
                PyralisAuthoringGraphSourceKind.CoreSetup,
                route != null && route.Mode != null ? PyralisAuthoringGraphEvidenceState.Ready : PyralisAuthoringGraphEvidenceState.Missing,
                guidance: route != null && route.Mode != null
                    ? "GameModeDefinition is assigned and can expose route rules, feature modules, camera, playfield, and proof context."
                    : "Create a GameModeDefinition asset and assign it to SessionDefinition.defaultGameMode.",
                nativeSetup: route != null && route.Mode != null
                    ? Array.Empty<string>()
                    : new[] { "Project -> Create -> NeonBlack -> Definitions -> Game Mode Definition; assign it to SessionDefinition.defaultGameMode." },
                assignmentFields: new[] { "SessionDefinition.defaultGameMode" },
                blockingReason: route != null && route.Mode != null
                    ? string.Empty
                    : "The route needs a GameModeDefinition before it can reflect gameplay rules, camera profile, playfield, and feature requirements.",
                nativeAction: route != null && route.Mode != null
                    ? null
                    : new PyralisAuthoringNativeAction(
                        "Create or assign",
                        PyralisAuthoringActionSurface.Inspector,
                        "SessionDefinition",
                        "defaultGameMode",
                        "SessionDefinition references the GameModeDefinition asset"),
                sourceObject: route?.Mode,
                sourceOrigin: route != null && route.Mode != null
                    ? PyralisAuthoringGraphSourceOrigin.UserAuthoredSetup
                    : PyralisAuthoringGraphSourceOrigin.SpineGrammar,
                setupDomain: PyralisAuthoringGraphSetupDomain.GameMode));

            AddEdge(edges, "bootstrap.root", "session.definition", PyralisAuthoringGraphEdgeKind.DependsOn, "reads");
            AddEdge(edges, "session.definition", "mode.definition", PyralisAuthoringGraphEdgeKind.DependsOn, "default mode");
        }

        private static void AddParticipantNodes(
            PyralisSetupRouteAnalysis route,
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges)
        {
            SessionDefinition session = route?.Session;
            ParticipantDefinition participant = route?.Participant != null ? route.Participant : GetFirstParticipant(session);
            PawnDefinition pawn = route?.Pawn != null ? route.Pawn : participant != null ? participant.defaultPawn : null;
            bool hasParticipants = route != null && route.HasParticipants;
            bool requiresPawn = route != null && route.RequiresPawn;
            bool pawnReady = !requiresPawn || (hasParticipants && string.IsNullOrWhiteSpace(route.ParticipantPawnIssue));

            AddNode(nodes, new PyralisAuthoringGraphNode(
                "participant.default",
                "Participants",
                PyralisAuthoringGraphNodeKind.SetupChain,
                PyralisAuthoringGraphSourceKind.CoreSetup,
                hasParticipants ? PyralisAuthoringGraphEvidenceState.Ready : PyralisAuthoringGraphEvidenceState.Missing,
                guidance: hasParticipants
                    ? "Players, seats, hands, factions, or command owners are assigned."
                    : "Assign at least one default participant.",
                nativeSetup: hasParticipants
                    ? Array.Empty<string>()
                    : new[] { "Project -> Create -> NeonBlack -> Definitions -> Participant Definition; assign it to SessionDefinition.defaultParticipants." },
                assignmentFields: new[] { "SessionDefinition.defaultParticipants" },
                blockingReason: hasParticipants
                    ? string.Empty
                    : "The route needs at least one ParticipantDefinition so a player, AI, seat, hand, faction, or command owner exists.",
                nativeAction: hasParticipants
                    ? null
                    : new PyralisAuthoringNativeAction(
                        "Create or assign",
                        PyralisAuthoringActionSurface.Inspector,
                        "SessionDefinition",
                        "defaultParticipants",
                        "SessionDefinition has at least one default participant"),
                sourceObject: participant != null ? participant : session,
                sourceOrigin: participant != null || session != null
                    ? PyralisAuthoringGraphSourceOrigin.UserAuthoredSetup
                    : PyralisAuthoringGraphSourceOrigin.SpineGrammar,
                setupDomain: PyralisAuthoringGraphSetupDomain.Participant));

            AddNode(nodes, new PyralisAuthoringGraphNode(
                "pawn.definition",
                requiresPawn ? "Pawn Definition" : "No Pawn Needed",
                PyralisAuthoringGraphNodeKind.UnitySurfaceRequirement,
                PyralisAuthoringGraphSourceKind.CoreSetup,
                pawnReady ? PyralisAuthoringGraphEvidenceState.Ready : PyralisAuthoringGraphEvidenceState.Missing,
                guidance: GetPawnGuidance(route),
                assignmentFields: new[] { "ParticipantDefinition.defaultPawn" },
                blockingReason: pawnReady ? string.Empty : route?.ParticipantPawnIssue,
                sourceObject: pawn != null ? pawn : participant != null ? participant : session,
                sourceOrigin: pawn != null || participant != null || session != null
                    ? PyralisAuthoringGraphSourceOrigin.UserAuthoredSetup
                    : PyralisAuthoringGraphSourceOrigin.SpineGrammar,
                setupDomain: PyralisAuthoringGraphSetupDomain.PawnDefinition,
                issueCode: BuildParticipantPawnIssueCode(route?.ParticipantPawnIssueKind ?? PyralisParticipantPawnIssueKind.None)));

            AddEdge(edges, "session.definition", "participant.default", PyralisAuthoringGraphEdgeKind.DependsOn, "default participants");
            AddEdge(edges, "participant.default", "pawn.definition", PyralisAuthoringGraphEdgeKind.DependsOn, "pawn route");
        }

        private static void AddParticipantSeatNodes(
            PyralisSetupRouteAnalysis route,
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges)
        {
            if (route == null)
                return;

            for (int i = 0; i < route.ParticipantSeats.Length; i++)
            {
                PyralisParticipantSeatReadiness seat = route.ParticipantSeats[i];
                if (seat == null)
                    continue;

                string seatNodeId = $"participant.seat.{seat.StableIdSuffix}";
                PyralisAuthoringGraphEvidenceState seatState = seat.IsReady
                    ? PyralisAuthoringGraphEvidenceState.Ready
                    : PyralisAuthoringGraphEvidenceState.Missing;
                AddNode(nodes, new PyralisAuthoringGraphNode(
                    seatNodeId,
                    $"{seat.DisplayName} Seat",
                    PyralisAuthoringGraphNodeKind.SetupChain,
                    PyralisAuthoringGraphSourceKind.CoreSetup,
                    seatState,
                    guidance: BuildParticipantSeatGuidance(seat),
                    assignmentFields: new[] { $"SessionDefinition.defaultParticipants[{seat.SlotIndex}]" },
                    blockingReason: seatState == PyralisAuthoringGraphEvidenceState.Ready
                        ? string.Empty
                        : BuildParticipantSeatGuidance(seat),
                    nativeAction: seatState == PyralisAuthoringGraphEvidenceState.Ready
                        ? null
                        : new PyralisAuthoringNativeAction(
                            "Inspect",
                            PyralisAuthoringActionSurface.Inspector,
                            "SessionDefinition",
                            $"defaultParticipants[{seat.SlotIndex}]",
                            "this participant slot has its authored participant, input, and pawn route ready"),
                    sourceObject: seat.Participant,
                    sourceOrigin: seat.HasParticipant
                        ? PyralisAuthoringGraphSourceOrigin.UserAuthoredSetup
                        : PyralisAuthoringGraphSourceOrigin.SpineGrammar,
                workIntent: seatState == PyralisAuthoringGraphEvidenceState.Ready
                    ? PyralisAuthoringGraphWorkIntent.Reference
                    : PyralisAuthoringGraphWorkIntent.RequiredSetup,
                issueSeverity: seatState == PyralisAuthoringGraphEvidenceState.Ready
                    ? PyralisAuthoringIssueSeverity.Info
                    : PyralisAuthoringIssueSeverity.Required,
                setupDomain: PyralisAuthoringGraphSetupDomain.Participant,
                issueCode: seatState == PyralisAuthoringGraphEvidenceState.Ready
                    ? string.Empty
                    : "ParticipantSeat.NotReady"));

                AddEdge(edges, "participant.default", seatNodeId, PyralisAuthoringGraphEdgeKind.DependsOn, "participant seat");

                if (route.RequiresPawn)
                {
                    AddParticipantSeatRequirementNode(
                        nodes,
                        edges,
                        seat,
                        seatNodeId,
                        $"participant.seat.{seat.StableIdSuffix}.input-profile",
                        "Assign Input Profile",
                        seat.IsInputReady,
                        string.IsNullOrWhiteSpace(seat.InputIssue)
                            ? $"Participant `{seat.DisplayName}` has an InputProfile."
                            : seat.InputIssue,
                        "ParticipantDefinition.inputProfile",
                        seat.Participant,
                        "InputProfile");
                    AddParticipantSeatRequirementNode(
                        nodes,
                        edges,
                        seat,
                        seatNodeId,
                        $"participant.seat.{seat.StableIdSuffix}.pawn",
                        "Assign Participant Pawn",
                        seat.IsPawnReady,
                        string.IsNullOrWhiteSpace(seat.PawnIssue)
                            ? $"Participant `{seat.DisplayName}` has a ready PawnDefinition and pawn prefab."
                            : seat.PawnIssue,
                        "ParticipantDefinition.defaultPawn",
                        seat.Pawn != null ? seat.Pawn : seat.Participant,
                        "PawnDefinition");
                }
            }

            if (!string.IsNullOrWhiteSpace(route.PlayerInputManagerIssue))
            {
                AddNode(nodes, new PyralisAuthoringGraphNode(
                    "route.player-input-manager-prefab",
                    "Local Join Player Prefab",
                    PyralisAuthoringGraphNodeKind.ValidationEvidence,
                    PyralisAuthoringGraphSourceKind.CoreSetup,
                    PyralisAuthoringGraphEvidenceState.Missing,
                    guidance: route.PlayerInputManagerIssue,
                    nativeSetup: new[]
                    {
                        "Create or select the local join pawn prefab.",
                        "Add Unity PlayerInput to the prefab root.",
                        "Add PawnRoot/IPawnParticipantInitializer to the same prefab shape so ParticipantSpawnService reuses the joined instance.",
                        "Assign that prefab to PlayerInputManager.playerPrefab and GameplaySessionBootstrap.playerInputManager."
                    },
                    assignmentFields: new[] { "PlayerInputManager.playerPrefab", "GameplaySessionBootstrap.playerInputManager" },
                    blockingReason: route.PlayerInputManagerIssue,
                    nativeAction: new PyralisAuthoringNativeAction(
                        "Assign",
                        PyralisAuthoringActionSurface.Inspector,
                        "PlayerInputManager",
                        "playerPrefab",
                        "joined PlayerInput prefab is the pawn shape that owns one participant"),
                    sourceObject: route.Bootstrap,
                    sourceOrigin: PyralisAuthoringGraphSourceOrigin.RuntimeEvidence,
                    workIntent: PyralisAuthoringGraphWorkIntent.RequiredSetup,
                    issueSeverity: PyralisAuthoringIssueSeverity.Required,
                    setupDomain: PyralisAuthoringGraphSetupDomain.PlayerInputManager,
                    issueCode: "PlayerInputManager.PlayerPrefabMissing"));

                AddEdge(edges, "route.participant-topology", "route.player-input-manager-prefab", PyralisAuthoringGraphEdgeKind.DependsOn, "local join prefab");
            }
        }

        private static void AddParticipantSeatRequirementNode(
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges,
            PyralisParticipantSeatReadiness seat,
            string seatNodeId,
            string nodeId,
            string label,
            bool ready,
            string guidance,
            string assignmentField,
            UnityEngine.Object sourceObject,
            string expectedType)
        {
            PyralisAuthoringGraphEvidenceState state = ready
                ? PyralisAuthoringGraphEvidenceState.Ready
                : PyralisAuthoringGraphEvidenceState.Missing;
            string fieldName = assignmentField;
            int dotIndex = assignmentField.LastIndexOf('.');
            if (dotIndex >= 0 && dotIndex < assignmentField.Length - 1)
                fieldName = assignmentField.Substring(dotIndex + 1);

            AddNode(nodes, new PyralisAuthoringGraphNode(
                nodeId,
                $"{label}: {seat.DisplayName}",
                PyralisAuthoringGraphNodeKind.AssignmentField,
                PyralisAuthoringGraphSourceKind.Reflection,
                state,
                guidance: guidance,
                nativeSetup: ready
                    ? Array.Empty<string>()
                    : new[] { $"Inspector -> {assignmentField}; assign or create {expectedType} for {seat.DisplayName}." },
                assignmentFields: new[] { assignmentField },
                blockingReason: ready ? string.Empty : guidance,
                nativeAction: ready
                    ? null
                    : new PyralisAuthoringNativeAction(
                        "Create or assign",
                        PyralisAuthoringActionSurface.Inspector,
                        "ParticipantDefinition",
                        fieldName,
                        $"{assignmentField} references a {expectedType}"),
                sourceObject: sourceObject,
                sourceOrigin: sourceObject != null
                    ? PyralisAuthoringGraphSourceOrigin.UserAuthoredSetup
                    : PyralisAuthoringGraphSourceOrigin.Reflection,
                workIntent: ready
                    ? PyralisAuthoringGraphWorkIntent.Reference
                    : PyralisAuthoringGraphWorkIntent.RequiredSetup,
                issueSeverity: ready
                    ? PyralisAuthoringIssueSeverity.Info
                    : PyralisAuthoringIssueSeverity.Required,
                setupDomain: GetSetupDomainForAssignmentField(assignmentField),
                issueCode: ready
                    ? string.Empty
                    : BuildAssignmentIssueCode(assignmentField)));

            AddEdge(edges, seatNodeId, nodeId, PyralisAuthoringGraphEdgeKind.DependsOn, "seat requirement");
            AddEdge(edges, nodeId, "route.shape", PyralisAuthoringGraphEdgeKind.Satisfies, "route requirement");
        }

        private static string BuildParticipantSeatGuidance(PyralisParticipantSeatReadiness seat)
        {
            if (seat == null)
                return string.Empty;

            if (!seat.HasParticipant)
                return $"Default participant slot {seat.SlotIndex} is empty. Assign a ParticipantDefinition before this seat can join, spawn, or receive input.";

            if (seat.RequiresPawn && !seat.IsInputReady)
                return seat.InputIssue;

            if (seat.RequiresPawn && !seat.IsPawnReady)
                return seat.PawnIssue;

            return $"Participant `{seat.DisplayName}` is ready for seat {seat.SeatIndex}.";
        }

        private static void AddReflectedDependencyEvidence(
            UnityEngine.Object source,
            PyralisSetupRouteAnalysis route,
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges)
        {
            PyralisSetupDependencyTree tree = BuildDependencyTree(source, route);
            if (tree == null)
                return;

            for (int i = 0; i < tree.AssignmentRecords.Count; i++)
            {
                PyralisSetupAssignmentRecord assignment = tree.AssignmentRecords[i];
                if (assignment == null || assignment.IsResolved || !assignment.DeclaredByContract)
                    continue;

                PyralisReflectedAssignmentState assignmentState = ResolveReflectedAssignmentState(assignment, route);
                if (assignmentState.EvidenceState == PyralisAuthoringGraphEvidenceState.Optional)
                    continue;

                AddMissingReflectedAssignment(nodes, edges, assignment, assignmentState);
            }
        }

        private readonly struct PyralisReflectedAssignmentState
        {
            public PyralisReflectedAssignmentState(
                PyralisAuthoringGraphEvidenceState evidenceState,
                PyralisAuthoringGraphWorkIntent workIntent,
                PyralisAuthoringIssueSeverity issueSeverity,
                string routeNodeId,
                string guidance)
            {
                EvidenceState = evidenceState;
                WorkIntent = workIntent;
                IssueSeverity = issueSeverity;
                RouteNodeId = routeNodeId ?? string.Empty;
                Guidance = guidance ?? string.Empty;
            }

            public PyralisAuthoringGraphEvidenceState EvidenceState { get; }
            public PyralisAuthoringGraphWorkIntent WorkIntent { get; }
            public PyralisAuthoringIssueSeverity IssueSeverity { get; }
            public string RouteNodeId { get; }
            public string Guidance { get; }
        }

        private static void AddMissingReflectedAssignment(
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges,
            PyralisSetupAssignmentRecord assignment,
            PyralisReflectedAssignmentState assignmentState)
        {
            string fieldPath = assignment.QualifiedFieldPath;
            string fieldName = GetFieldName(assignment.FieldPath);
            string nodeId = "dependency.assignment." + NormalizeId(fieldPath);
            string expectedType = FirstNonEmpty(assignment.ExpectedTypeName, "assigned object");
            PyralisAuthoringNativeAction nativeAction = new PyralisAuthoringNativeAction(
                "Create or assign",
                PyralisAuthoringActionSurface.Inspector,
                assignment.OwnerTypeName,
                fieldName,
                fieldPath + " references a " + expectedType);

            AddNode(nodes, new PyralisAuthoringGraphNode(
                nodeId,
                "Assign " + AuthoringCapabilityRegistry.PrettifyTypeName(fieldName),
                PyralisAuthoringGraphNodeKind.AssignmentField,
                PyralisAuthoringGraphSourceKind.Reflection,
                assignmentState.EvidenceState,
                guidance: assignmentState.Guidance,
                nativeSetup: new[] { FormatNativeAction(nativeAction) },
                assignmentFields: new[] { fieldPath },
                blockingReason: assignmentState.EvidenceState == PyralisAuthoringGraphEvidenceState.Missing
                    ? assignmentState.Guidance
                    : string.Empty,
                nativeAction: nativeAction,
                sourceObject: assignment.OwnerObject,
                sourceOrigin: PyralisAuthoringGraphSourceOrigin.Reflection,
                workIntent: assignmentState.WorkIntent,
                issueSeverity: assignmentState.IssueSeverity,
                setupDomain: GetSetupDomainForAssignmentField(fieldPath),
                issueCode: BuildAssignmentIssueCode(fieldPath)));

            string ownerNodeId = ResolveAssignmentOwnerNodeId(assignment);
            string routeNodeId = !string.IsNullOrWhiteSpace(assignmentState.RouteNodeId)
                ? assignmentState.RouteNodeId
                : "route.shape";
            AddEdge(edges, ownerNodeId, nodeId, PyralisAuthoringGraphEdgeKind.DependsOn, fieldName);
            AddEdge(edges, routeNodeId, nodeId, PyralisAuthoringGraphEdgeKind.DependsOn, "reflected route dependency");
        }

        private static PyralisReflectedAssignmentState ResolveReflectedAssignmentState(
            PyralisSetupAssignmentRecord assignment,
            PyralisSetupRouteAnalysis route)
        {
            string fieldPath = assignment?.QualifiedFieldPath ?? string.Empty;
            string fieldName = assignment != null ? GetFieldName(assignment.FieldPath) : string.Empty;
            bool required = IsRequiredReflectedAssignment(assignment, route);
            bool recommended = required || IsRecommendedReflectedAssignment(assignment, route);
            if (!recommended)
            {
                return new PyralisReflectedAssignmentState(
                    PyralisAuthoringGraphEvidenceState.Optional,
                    PyralisAuthoringGraphWorkIntent.Optional,
                    PyralisAuthoringIssueSeverity.Info,
                    string.Empty,
                    string.Empty);
            }

            string guidance = $"Assign {fieldPath} so the route can use the contract-declared {AuthoringCapabilityRegistry.PrettifyTypeName(fieldName)} surface.";
            return new PyralisReflectedAssignmentState(
                required ? PyralisAuthoringGraphEvidenceState.Missing : PyralisAuthoringGraphEvidenceState.CandidateDetected,
                required ? PyralisAuthoringGraphWorkIntent.RequiredSetup : PyralisAuthoringGraphWorkIntent.ProofEnhancer,
                required ? PyralisAuthoringIssueSeverity.Required : PyralisAuthoringIssueSeverity.Recommended,
                GetRouteNodeForAssignment(assignment.OwnerTypeName),
                guidance);
        }

        private static bool IsRequiredReflectedAssignment(PyralisSetupAssignmentRecord assignment, PyralisSetupRouteAnalysis route)
        {
            if (assignment == null || route == null)
                return false;

            string ownerType = assignment.OwnerTypeName;
            string fieldName = GetFieldName(assignment.FieldPath);

            if (ownerType == nameof(SessionDefinition)
                && (fieldName == nameof(SessionDefinition.defaultGameMode)
                    || fieldName.StartsWith(nameof(SessionDefinition.defaultParticipants), StringComparison.Ordinal)))
                return true;

            if (!route.RequiresPawn)
                return false;

            if (ownerType == nameof(ParticipantDefinition)
                && (fieldName == nameof(ParticipantDefinition.defaultPawn)
                    || fieldName == nameof(ParticipantDefinition.inputProfile)))
                return true;

            if (ownerType == nameof(PawnDefinition) && fieldName == nameof(PawnDefinition.pawnPrefab))
                return true;

            if (ownerType == nameof(GameModeDefinition)
                && fieldName == nameof(GameModeDefinition.cameraRigProfile)
                && (route.UsesPawnGameplay() || route.UsesCamera()))
                return true;

            return false;
        }

        private static bool IsRecommendedReflectedAssignment(PyralisSetupAssignmentRecord assignment, PyralisSetupRouteAnalysis route)
        {
            if (assignment == null || route == null)
                return false;

            string ownerType = assignment.OwnerTypeName;
            string fieldName = GetFieldName(assignment.FieldPath);

            if (ownerType == nameof(GameModeDefinition)
                && fieldName == nameof(GameModeDefinition.playfieldProfile))
                return route.UsesPlayfield();

            if (ownerType == nameof(PawnDefinition)
                && (fieldName == nameof(PawnDefinition.movementProfile)
                    || fieldName == nameof(PawnDefinition.presentationProfile)))
                return route.UsesPawnGameplay();

            if (ownerType == nameof(PawnDefinition)
                && fieldName == nameof(PawnDefinition.animationProfile))
                return ContainsFamily(route.CapabilityFamilies, RuntimeCapabilityFamily.AnimationPresentation);

            return false;
        }

        private static string ResolveAssignmentOwnerNodeId(PyralisSetupAssignmentRecord assignment)
        {
            return assignment != null ? GetRouteNodeForAssignment(assignment.OwnerTypeName) : string.Empty;
        }

        private static string GetRouteNodeForAssignment(string ownerType)
        {
            if (ownerType == nameof(SessionDefinition))
                return "session.definition";
            if (ownerType == nameof(GameModeDefinition))
                return "mode.definition";
            if (ownerType == nameof(ParticipantDefinition))
                return "participant.default";
            if (ownerType == nameof(PawnDefinition))
                return "pawn.definition";

            return "route.shape";
        }

        private static string GetFieldName(string fieldPath)
        {
            if (string.IsNullOrWhiteSpace(fieldPath))
                return string.Empty;

            string field = fieldPath;
            int bracket = field.IndexOf('[', StringComparison.Ordinal);
            if (bracket > 0)
                field = field.Substring(0, bracket);

            int dot = field.LastIndexOf('.');
            if (dot >= 0 && dot < field.Length - 1)
                field = field.Substring(dot + 1);

            return field;
        }

        private static void AddCameraFocusEvidence(
            PyralisSetupRouteAnalysis route,
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges)
        {
            if (route == null || route.Mode == null || route.Mode.cameraRigProfile == null)
                return;

            CameraRigProfile profile = route.Mode.cameraRigProfile;
            bool pawnFocus = profile.focusMode == CameraRigProfile.CameraFocusMode.ParticipantPawns
                || profile.focusMode == CameraRigProfile.CameraFocusMode.ParticipantGroup;
            bool requiresPawnTarget = pawnFocus && route.RequiresPawn;
            bool hasPawnTarget = HasPawnCameraTarget(route.Pawn);
            PyralisAuthoringGraphEvidenceState state = ResolveCameraFocusEvidence(profile, requiresPawnTarget, hasPawnTarget);
            string guidance = BuildCameraFocusGuidance(profile, requiresPawnTarget, hasPawnTarget, route.Pawn);
            string blockingReason = state == PyralisAuthoringGraphEvidenceState.Missing
                ? guidance
                : string.Empty;

            AddNode(nodes, new PyralisAuthoringGraphNode(
                "route.camera-focus",
                "Camera Focus",
                PyralisAuthoringGraphNodeKind.UnitySurfaceRequirement,
                PyralisAuthoringGraphSourceKind.CoreSetup,
                state,
                RuntimeCapabilityFamily.CameraInput,
                AuthoringCapability.Camera,
                guidance: guidance,
                nativeSetup: BuildCameraFocusNativeSetup(profile, requiresPawnTarget, hasPawnTarget),
                assignmentFields: BuildCameraFocusAssignmentFields(profile, requiresPawnTarget),
                blockingReason: blockingReason,
                sourceObject: route.Pawn != null && route.Pawn.pawnPrefab != null
                    ? route.Pawn.pawnPrefab
                    : route.Mode.cameraRigProfile,
                sourceOrigin: PyralisAuthoringGraphSourceOrigin.RuntimeEvidence,
                workIntent: state == PyralisAuthoringGraphEvidenceState.Missing
                    ? PyralisAuthoringGraphWorkIntent.RequiredSetup
                    : state == PyralisAuthoringGraphEvidenceState.CandidateDetected
                        ? PyralisAuthoringGraphWorkIntent.ProofEnhancer
                        : PyralisAuthoringGraphWorkIntent.Reference,
                issueSeverity: state == PyralisAuthoringGraphEvidenceState.Missing
                    ? PyralisAuthoringIssueSeverity.Required
                    : state == PyralisAuthoringGraphEvidenceState.CandidateDetected
                        ? PyralisAuthoringIssueSeverity.Recommended
                        : PyralisAuthoringIssueSeverity.Info,
                setupDomain: PyralisAuthoringGraphSetupDomain.Camera,
                issueCode: state == PyralisAuthoringGraphEvidenceState.Missing
                    ? "Camera.FocusTargetMissing"
                    : string.Empty));

            AddEdge(edges, "mode.definition", "route.camera-focus", PyralisAuthoringGraphEdgeKind.DependsOn, "camera focus mode");
            if (requiresPawnTarget)
                AddEdge(edges, "pawn.definition", "route.camera-focus", PyralisAuthoringGraphEdgeKind.DependsOn, "pawn camera target");
        }

        private static void AddCapabilityNodes(
            PyralisSetupRouteAnalysis route,
            PyralisAuthoringIntentSelection intentSelection,
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges)
        {
            RuntimeCapabilityFamily[] families = route?.CapabilityFamilies ?? Array.Empty<RuntimeCapabilityFamily>();
            bool hasCapabilities = families.Length > 0;
            AddNode(nodes, new PyralisAuthoringGraphNode(
                "capability.selected",
                "Capabilities",
                PyralisAuthoringGraphNodeKind.Capability,
                PyralisAuthoringGraphSourceKind.AuthoringContract,
                hasCapabilities ? PyralisAuthoringGraphEvidenceState.Ready : PyralisAuthoringGraphEvidenceState.Missing,
                guidance: GetCapabilitySummaryGuidance(route, hasCapabilities),
                sourceObject: route?.Mode != null ? route.Mode : route?.Session,
                sourceOrigin: hasCapabilities
                    ? PyralisAuthoringGraphSourceOrigin.Reflection
                    : PyralisAuthoringGraphSourceOrigin.SpineGrammar,
                setupDomain: PyralisAuthoringGraphSetupDomain.RouteCapabilities));
            AddEdge(edges, "mode.definition", "capability.selected", PyralisAuthoringGraphEdgeKind.Satisfies, "reflected capabilities");

            for (int i = 0; i < families.Length; i++)
            {
                RuntimeCapabilityFamily family = families[i];
                PyralisAuthoringCapabilityDescriptor descriptor = ResolveCapabilityDescriptorForFamily(family, intentSelection);
                string nodeId = GetCapabilityNodeId(family, descriptor);
                string proofTarget = descriptor?.ProofTargetId ?? string.Empty;

                AddNode(nodes, new PyralisAuthoringGraphNode(
                    nodeId,
                    descriptor != null ? descriptor.DisplayName : family.ToString(),
                    PyralisAuthoringGraphNodeKind.Capability,
                    GetCapabilitySourceKind(descriptor),
                    PyralisAuthoringGraphEvidenceState.Ready,
                    family,
                    descriptor != null ? descriptor.Capability : AuthoringCapability.None,
                    proofTarget,
                    descriptor != null ? descriptor.Summary : string.Empty,
                    descriptor != null ? descriptor.RequiredSetup : Array.Empty<string>(),
                    descriptor != null ? descriptor.AssignmentFields : Array.Empty<string>(),
                    descriptor != null ? descriptor.CustomizationMoments : Array.Empty<string>(),
                    sourceOrigin: descriptor != null
                        ? descriptor.SourceOrigin
                        : PyralisAuthoringGraphSourceOrigin.UserAuthoredSetup,
                    setupDomain: GetSetupDomain(family, descriptor != null ? descriptor.Capability : AuthoringCapability.None)));

                AddEdge(edges, "capability.selected", nodeId, PyralisAuthoringGraphEdgeKind.Satisfies, "reflected capability");
                AddEdge(edges, "capability.selected", nodeId, PyralisAuthoringGraphEdgeKind.RelatesTo, "includes");
            }
        }

        private static void AddRouteShapeNode(
            PyralisSetupRouteAnalysis route,
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges)
        {
            RuntimeCapabilityFamily[] families = route?.CapabilityFamilies ?? Array.Empty<RuntimeCapabilityFamily>();
            bool hasGameplayFocus = HasGameplayFocus(families);
            bool requiresPawn = route != null && route.RequiresPawn;
            bool hasParticipants = route != null && route.HasParticipants;
            bool hasOwnershipIssue = hasGameplayFocus && (!hasParticipants || requiresPawn && IsRouteShapeOwnershipIssue(route));
            PyralisAuthoringGraphEvidenceState state = !hasGameplayFocus
                ? PyralisAuthoringGraphEvidenceState.Optional
                : hasOwnershipIssue
                    ? PyralisAuthoringGraphEvidenceState.Missing
                    : PyralisAuthoringGraphEvidenceState.Ready;
            string label = GetRouteShapeLabel(route, hasGameplayFocus, requiresPawn);
            string guidance = GetRouteShapeGuidance(route, hasGameplayFocus, requiresPawn, hasParticipants);

            AddNode(nodes, new PyralisAuthoringGraphNode(
                "route.shape",
                label,
                PyralisAuthoringGraphNodeKind.RouteShape,
                PyralisAuthoringGraphSourceKind.CoreSetup,
                state,
                guidance: guidance,
                nativeSetup: Array.Empty<string>(),
                assignmentFields: GetRouteShapeAssignmentFields(requiresPawn, hasGameplayFocus),
                blockingReason: hasOwnershipIssue
                    ? FirstNonEmpty(route?.ParticipantPawnIssue, "Assign at least one ParticipantDefinition so the route has a player, AI, seat, hand, faction, or control owner.")
                    : string.Empty,
                sourceObject: route?.Session != null ? route.Session : route?.Mode,
                sourceOrigin: PyralisAuthoringGraphSourceOrigin.SpineGrammar,
                setupDomain: PyralisAuthoringGraphSetupDomain.RouteShape,
                issueCode: hasOwnershipIssue ? "RouteShape.OwnershipMissing" : string.Empty));

            AddEdge(edges, "capability.selected", "route.shape", PyralisAuthoringGraphEdgeKind.Satisfies, "compiles ownership shape");
            AddEdge(edges, "route.shape", "participant.default", PyralisAuthoringGraphEdgeKind.DependsOn, "participants own control");
            if (requiresPawn)
                AddEdge(edges, "route.shape", "pawn.definition", PyralisAuthoringGraphEdgeKind.DependsOn, "pawn-backed control surface");
        }

        private static void AddParticipantTopologyNode(
            PyralisSetupRouteAnalysis route,
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges)
        {
            if (route == null)
                return;

            PyralisAuthoringGraphEvidenceState state = GetParticipantTopologyEvidenceState(route);
            PyralisAuthoringGraphWorkIntent workIntent = state == PyralisAuthoringGraphEvidenceState.Missing
                || state == PyralisAuthoringGraphEvidenceState.Blocked
                    ? PyralisAuthoringGraphWorkIntent.RequiredSetup
                    : PyralisAuthoringGraphWorkIntent.Reference;
            PyralisAuthoringIssueSeverity issueSeverity = state == PyralisAuthoringGraphEvidenceState.Missing
                || state == PyralisAuthoringGraphEvidenceState.Blocked
                    ? PyralisAuthoringIssueSeverity.Required
                    : PyralisAuthoringIssueSeverity.Info;

            AddNode(nodes, new PyralisAuthoringGraphNode(
                "route.participant-topology",
                "Participant Join Policy",
                PyralisAuthoringGraphNodeKind.RouteShape,
                PyralisAuthoringGraphSourceKind.CoreSetup,
                state,
                guidance: GetParticipantTopologyGuidance(route),
                nativeSetup: BuildParticipantTopologyNativeSetup(route),
                assignmentFields: BuildParticipantTopologyAssignmentFields(route),
                blockingReason: route.HasLocalJoinPolicyConflict()
                    ? "Local join should wait for Unity PlayerInputManager joins; auto-registering default participants can spawn every seat before controllers join."
                    : string.Empty,
                nativeAction: GetParticipantTopologyNativeAction(route),
                sourceObject: route.Bootstrap != null ? route.Bootstrap : route.Session,
                sourceOrigin: PyralisAuthoringGraphSourceOrigin.RuntimeEvidence,
                workIntent: workIntent,
                issueSeverity: issueSeverity,
                setupDomain: PyralisAuthoringGraphSetupDomain.ParticipantTopology,
                issueCode: state == PyralisAuthoringGraphEvidenceState.Missing
                    || state == PyralisAuthoringGraphEvidenceState.Blocked
                        ? "ParticipantTopology.JoinPolicyConflict"
                        : string.Empty));

            AddEdge(edges, "route.shape", "route.participant-topology", PyralisAuthoringGraphEdgeKind.DependsOn, "participant topology");
            AddEdge(edges, "participant.default", "route.participant-topology", PyralisAuthoringGraphEdgeKind.DependsOn, "participant seats");
        }

        private static PyralisAuthoringGraphEvidenceState GetParticipantTopologyEvidenceState(PyralisSetupRouteAnalysis route)
        {
            if (route == null || !route.HasParticipants)
                return PyralisAuthoringGraphEvidenceState.Blocked;

            return route.HasLocalJoinPolicyConflict()
                ? PyralisAuthoringGraphEvidenceState.Missing
                : PyralisAuthoringGraphEvidenceState.Ready;
        }

        private static string GetParticipantTopologyGuidance(PyralisSetupRouteAnalysis route)
        {
            if (route == null)
                return "Participant topology is not resolved yet.";

            string summary = $"Topology: {route.ParticipantTopology}. Expected join: {route.ExpectedJoinPolicy}. Spawn: {route.SpawnPolicy}. {GetParticipantCountSummary(route)} Auto-join seats: {route.AutoJoinParticipantCount}.";
            if (route.HasLocalJoinPolicyConflict())
            {
                return summary + " This is a local join route, but ParticipantInputRouter can auto-register default participants without PlayerInput. Disable that policy so each Unity PlayerInput join owns one participant and pawn.";
            }

            if (route.ParticipantTopology == PyralisParticipantTopology.LocalJoin)
            {
                return summary + " Unity PlayerInputManager should own controller pairing; ParticipantSpawnService may spawn when each joined participant registers.";
            }

            if (route.ParticipantTopology == PyralisParticipantTopology.SoloLocal)
            {
                return summary + " Solo local routes can auto-register the default participant or deliberately wait for PlayerInput join.";
            }

            if (route.ParticipantTopology == PyralisParticipantTopology.Networked
                || route.ParticipantTopology == PyralisParticipantTopology.HybridLocalNetworked)
            {
                return summary + " Networking is transport/authority; local device join remains a separate participant topology concern.";
            }

            return summary;
        }

        private static string GetParticipantCountSummary(PyralisSetupRouteAnalysis route)
        {
            if (route == null)
                return "Participants: 0.";

            if (route.DesiredParticipantCount > 0 && route.AuthoredParticipantCount > 0 && route.DesiredParticipantCount != route.AuthoredParticipantCount)
            {
                return $"Participants: {route.AssignedParticipantCount} effective ({route.AuthoredParticipantCount} authored in SessionDefinition, {route.DesiredParticipantCount} requested by Intent).";
            }

            if (route.DesiredParticipantCount > 0 && route.AuthoredParticipantCount == 0)
                return $"Participants: {route.AssignedParticipantCount} previewed from Intent.";

            return $"Participants: {route.AssignedParticipantCount} authored.";
        }

        private static string[] BuildParticipantTopologyNativeSetup(PyralisSetupRouteAnalysis route)
        {
            if (route == null)
                return Array.Empty<string>();

            if (route.HasLocalJoinPolicyConflict())
            {
                return new[]
                {
                    "Inspector -> ParticipantInputRouter -> disable Auto Register Default Participants Without Player Input for local co-op join routes.",
                    "Inspector -> GameplaySessionBootstrap -> assign PlayerInputManager so Unity pairs each controller with one joined PlayerInput.",
                    "Inspector -> ParticipantSpawnService -> keep Spawn On Register enabled if pawns should appear when each PlayerInput joins; disable it only for manual spawn flows."
                };
            }

            if (route.ParticipantTopology == PyralisParticipantTopology.LocalJoin)
            {
                return new[]
                {
                    "Use Unity PlayerInputManager for local join.",
                    "Set PlayerInputManager.playerPrefab to a prefab containing PlayerInput and PawnRoot/IPawnParticipantInitializer.",
                    "Use ParticipantDefinition entries as seat templates; do not auto-register all seats before controller join."
                };
            }

            return Array.Empty<string>();
        }

        private static string[] BuildParticipantTopologyAssignmentFields(PyralisSetupRouteAnalysis route)
        {
            if (route == null)
                return Array.Empty<string>();

            List<string> fields = new List<string>
            {
                "SessionDefinition.defaultParticipants",
                "ParticipantDefinition.autoJoin",
                "ParticipantInputRouter.autoRegisterDefaultParticipantsWithoutPlayerInput",
                "ParticipantSpawnService.spawnOnRegister"
            };

            if (route.ParticipantTopology == PyralisParticipantTopology.LocalJoin)
                fields.Add("GameplaySessionBootstrap.playerInputManager");

            return fields.ToArray();
        }

        private static PyralisAuthoringNativeAction? GetParticipantTopologyNativeAction(PyralisSetupRouteAnalysis route)
        {
            if (route == null)
                return null;

            if (route.HasLocalJoinPolicyConflict())
            {
                return new PyralisAuthoringNativeAction(
                    "Disable",
                    PyralisAuthoringActionSurface.Inspector,
                    "ParticipantInputRouter",
                    "autoRegisterDefaultParticipantsWithoutPlayerInput",
                    "local co-op waits for PlayerInputManager joins instead of auto-spawning every default participant");
            }

            if (route.ParticipantTopology == PyralisParticipantTopology.LocalJoin && !route.HasPlayerInputManager)
            {
                return new PyralisAuthoringNativeAction(
                    "Create or assign",
                    PyralisAuthoringActionSurface.Inspector,
                    "GameplaySessionBootstrap",
                    "playerInputManager",
                    "Unity PlayerInputManager is assigned for local join");
            }

            return null;
        }

        private static bool HasGameplayFocus(RuntimeCapabilityFamily[] families)
        {
            if (families == null)
                return false;

            for (int i = 0; i < families.Length; i++)
            {
                if (families[i] != RuntimeCapabilityFamily.PlatformCore)
                    return true;
            }

            return false;
        }

        private static string GetRouteShapeLabel(PyralisSetupRouteAnalysis route, bool hasGameplayFocus, bool requiresPawn)
        {
            if (!hasGameplayFocus)
                return "Setup Foundation";
            if (requiresPawn)
                return "Participant With Pawn";
            if (route != null && ContainsFamily(route.CapabilityFamilies, RuntimeCapabilityFamily.BoardCardTabletop))
                return "Participant Without Pawn";
            if (route != null && ContainsFamily(route.CapabilityFamilies, RuntimeCapabilityFamily.ActionTargeting))
                return "Participant Action Surface";

            return "Participant Control Surface";
        }

        private static string GetRouteShapeGuidance(PyralisSetupRouteAnalysis route, bool hasGameplayFocus, bool requiresPawn, bool hasParticipants)
        {
            if (!hasGameplayFocus)
                return "Core setup only. Choose one small Intent capability so Pyralis can explain whether the participant controls a pawn, cursor, board seat, hand, faction, or menu/action surface.";

            if (!hasParticipants)
                return "Every route starts with at least one ParticipantDefinition: player, AI, seat, hand, faction, or command owner.";

            if (requiresPawn)
                return FirstNonEmpty(
                    IsRouteShapeOwnershipIssue(route) ? route?.ParticipantPawnIssue : string.Empty,
                    "This intent is pawn-backed. The participant owns a PawnDefinition, the PawnDefinition owns the prefab, and participant input is the preferred control profile.");

            return "This intent does not require a pawn. Participants still own control, but the control surface can be a board seat, hand, cursor, camera, UI, or action resolver.";
        }

        private static string[] GetRouteShapeAssignmentFields(bool requiresPawn, bool hasGameplayFocus)
        {
            if (!hasGameplayFocus)
                return Array.Empty<string>();

            if (requiresPawn)
            {
                return new[]
                {
                    "SessionDefinition.defaultParticipants",
                    "ParticipantDefinition.defaultPawn"
                };
            }

            return new[] { "SessionDefinition.defaultParticipants" };
        }

        private static bool IsRouteShapeOwnershipIssue(PyralisSetupRouteAnalysis route)
        {
            if (route == null)
                return true;

            switch (route.ParticipantPawnIssueKind)
            {
                case PyralisParticipantPawnIssueKind.MissingParticipants:
                case PyralisParticipantPawnIssueKind.EmptyParticipantSlot:
                case PyralisParticipantPawnIssueKind.MissingPawnDefinition:
                    return true;
                default:
                    return false;
            }
        }

        private static string BuildParticipantPawnIssueCode(PyralisParticipantPawnIssueKind issueKind)
        {
            return issueKind == PyralisParticipantPawnIssueKind.None
                ? string.Empty
                : "ParticipantPawn." + issueKind;
        }

        private static string BuildAssignmentIssueCode(string assignmentField)
        {
            if (string.IsNullOrWhiteSpace(assignmentField))
                return "Assignment.Missing";

            return "Assignment." + NormalizeId(assignmentField);
        }

        private static string BuildRuntimeValidationIssueCode(PyralisSetupDependencyNode dependencyNode, PyralisRuntimeValidationIssue issue)
        {
            if (!string.IsNullOrWhiteSpace(issue?.IssueCode))
                return "RuntimeValidation." + NormalizeId(issue.IssueCode);

            string source = FirstNonEmpty(
                dependencyNode?.StableId,
                dependencyNode?.SourceObject != null ? dependencyNode.SourceObject.GetType().Name : string.Empty,
                "runtime-validation");
            string field = FirstNonEmpty(issue?.FieldPath, dependencyNode?.SourceFieldPath, issue?.Message, "issue");
            return "RuntimeValidation." + NormalizeId(source) + "." + NormalizeId(field);
        }

        private static PyralisAuthoringGraphSetupDomain GetSetupDomain(RuntimeCapabilityFamily family, AuthoringCapability capability)
        {
            if (capability != AuthoringCapability.None)
                return GetSetupDomain(capability);

            switch (family)
            {
                case RuntimeCapabilityFamily.PlatformCore:
                    return PyralisAuthoringGraphSetupDomain.RouteCapabilities;
                case RuntimeCapabilityFamily.CharacterPawnGameplay:
                    return PyralisAuthoringGraphSetupDomain.PawnDefinition;
                case RuntimeCapabilityFamily.ActionTargeting:
                    return PyralisAuthoringGraphSetupDomain.FeatureContract;
                case RuntimeCapabilityFamily.Combat:
                case RuntimeCapabilityFamily.GunsProjectiles:
                    return PyralisAuthoringGraphSetupDomain.FeatureContract;
                case RuntimeCapabilityFamily.BoardCardTabletop:
                    return PyralisAuthoringGraphSetupDomain.Tabletop;
                case RuntimeCapabilityFamily.AnimationPresentation:
                    return PyralisAuthoringGraphSetupDomain.PawnPresentation;
                case RuntimeCapabilityFamily.ScoringObjectives:
                    return PyralisAuthoringGraphSetupDomain.Scoring;
                case RuntimeCapabilityFamily.CameraInput:
                    return PyralisAuthoringGraphSetupDomain.Camera;
                case RuntimeCapabilityFamily.Networking:
                    return PyralisAuthoringGraphSetupDomain.Networking;
                default:
                    return PyralisAuthoringGraphSetupDomain.FeatureContract;
            }
        }

        private static PyralisAuthoringGraphSetupDomain GetSetupDomain(AuthoringCapability capability)
        {
            if ((capability & AuthoringCapability.Input) != 0)
                return PyralisAuthoringGraphSetupDomain.Input;
            if ((capability & AuthoringCapability.Participants) != 0)
                return PyralisAuthoringGraphSetupDomain.Participant;
            if ((capability & AuthoringCapability.Session) != 0 || (capability & AuthoringCapability.Rules) != 0)
                return PyralisAuthoringGraphSetupDomain.Session;
            if ((capability & AuthoringCapability.Camera) != 0)
                return PyralisAuthoringGraphSetupDomain.Camera;
            if ((capability & AuthoringCapability.Animation) != 0 || (capability & AuthoringCapability.VFX) != 0)
                return PyralisAuthoringGraphSetupDomain.PawnPresentation;
            if ((capability & AuthoringCapability.Movement) != 0
                || (capability & AuthoringCapability.KineticMotor2D) != 0
                || (capability & AuthoringCapability.KineticMotor3D) != 0
                || (capability & AuthoringCapability.Steering2D) != 0
                || (capability & AuthoringCapability.Steering3D) != 0
                || (capability & AuthoringCapability.Traversal) != 0)
            {
                return PyralisAuthoringGraphSetupDomain.PawnMotor;
            }

            if ((capability & AuthoringCapability.UI) != 0)
                return PyralisAuthoringGraphSetupDomain.UserInterface;
            if ((capability & AuthoringCapability.Scoring) != 0)
                return PyralisAuthoringGraphSetupDomain.Scoring;
            if ((capability & AuthoringCapability.Tabletop) != 0
                || (capability & AuthoringCapability.Grid) != 0
                || (capability & AuthoringCapability.TurnBased) != 0)
            {
                return PyralisAuthoringGraphSetupDomain.Tabletop;
            }

            if ((capability & AuthoringCapability.Networking) != 0)
                return PyralisAuthoringGraphSetupDomain.Networking;
            if ((capability & AuthoringCapability.Environment) != 0)
                return PyralisAuthoringGraphSetupDomain.Playfield;
            if ((capability & AuthoringCapability.Setup) != 0)
                return PyralisAuthoringGraphSetupDomain.RouteCapabilities;

            return PyralisAuthoringGraphSetupDomain.FeatureContract;
        }

        private static PyralisAuthoringGraphSetupDomain GetSetupDomainForAssignmentField(string assignmentField)
        {
            if (string.IsNullOrWhiteSpace(assignmentField))
                return PyralisAuthoringGraphSetupDomain.Unknown;

            string field = assignmentField.Trim();
            if (field.EndsWith(".sessionDefinition", StringComparison.Ordinal)
                || field.EndsWith(".defaultGameMode", StringComparison.Ordinal))
            {
                return field.EndsWith(".sessionDefinition", StringComparison.Ordinal)
                    ? PyralisAuthoringGraphSetupDomain.Session
                    : PyralisAuthoringGraphSetupDomain.GameMode;
            }

            if (field.EndsWith(".defaultParticipants", StringComparison.Ordinal)
                || field.Contains(".defaultParticipants["))
                return PyralisAuthoringGraphSetupDomain.Participant;
            if (field.EndsWith(".inputProfile", StringComparison.Ordinal))
                return PyralisAuthoringGraphSetupDomain.Input;
            if (field.EndsWith(".defaultPawn", StringComparison.Ordinal)
                || field.EndsWith(".pawnDefinition", StringComparison.Ordinal))
                return PyralisAuthoringGraphSetupDomain.PawnDefinition;
            if (field.EndsWith(".pawnPrefab", StringComparison.Ordinal)
                || field.EndsWith(".runtimePrefab", StringComparison.Ordinal))
                return PyralisAuthoringGraphSetupDomain.PawnPrefab;
            if (field.EndsWith(".playerPrefab", StringComparison.Ordinal)
                || field.EndsWith(".playerInputManager", StringComparison.Ordinal))
                return PyralisAuthoringGraphSetupDomain.PlayerInputManager;
            if (field.EndsWith(".spawnPoint", StringComparison.Ordinal)
                || field.EndsWith(".spawnPoints", StringComparison.Ordinal))
                return PyralisAuthoringGraphSetupDomain.Spawn;
            if (field.EndsWith(".cameraRigProfile", StringComparison.Ordinal)
                || field.EndsWith(".cameraRig", StringComparison.Ordinal)
                || field.EndsWith(".targetCamera", StringComparison.Ordinal))
                return PyralisAuthoringGraphSetupDomain.Camera;
            if (field.EndsWith(".playfieldProfile", StringComparison.Ordinal))
                return PyralisAuthoringGraphSetupDomain.Playfield;
            if (field.EndsWith(".animationProfile", StringComparison.Ordinal))
                return PyralisAuthoringGraphSetupDomain.PawnAnimation;
            if (field.EndsWith(".presentationProfile", StringComparison.Ordinal))
                return PyralisAuthoringGraphSetupDomain.PawnPresentation;

            return PyralisAuthoringGraphSetupDomain.FeatureContract;
        }

        private static PyralisAuthoringGraphSetupDomain GetSetupDomain(PyralisSceneReadinessCategory category)
        {
            switch (category)
            {
                case PyralisSceneReadinessCategory.SceneRoot:
                    return PyralisAuthoringGraphSetupDomain.GameplayRoot;
                case PyralisSceneReadinessCategory.CameraAudio:
                    return PyralisAuthoringGraphSetupDomain.Camera;
                case PyralisSceneReadinessCategory.Input:
                    return PyralisAuthoringGraphSetupDomain.Input;
                case PyralisSceneReadinessCategory.UserInterface:
                    return PyralisAuthoringGraphSetupDomain.UserInterface;
                case PyralisSceneReadinessCategory.Presentation:
                    return PyralisAuthoringGraphSetupDomain.PawnPresentation;
                case PyralisSceneReadinessCategory.Physics:
                    return PyralisAuthoringGraphSetupDomain.PawnMotor;
                case PyralisSceneReadinessCategory.PrefabContract:
                    return PyralisAuthoringGraphSetupDomain.PawnPrefab;
                case PyralisSceneReadinessCategory.Networking:
                    return PyralisAuthoringGraphSetupDomain.Networking;
                default:
                    return PyralisAuthoringGraphSetupDomain.SceneReadiness;
            }
        }

        private static PyralisAuthoringGraphSetupDomain GetSetupDomain(
            PyralisSetupDependencyNode dependencyNode,
            PyralisRuntimeValidationIssue issue)
        {
            PyralisAuthoringGraphSetupDomain fieldDomain = GetSetupDomainForAssignmentField(
                FirstNonEmpty(issue?.FieldPath, dependencyNode?.SourceFieldPath));
            if (fieldDomain != PyralisAuthoringGraphSetupDomain.Unknown
                && fieldDomain != PyralisAuthoringGraphSetupDomain.FeatureContract)
            {
                return fieldDomain;
            }

            string stableId = dependencyNode?.StableId ?? string.Empty;
            if (stableId.StartsWith("pawn.definition", StringComparison.Ordinal))
                return PyralisAuthoringGraphSetupDomain.PawnDefinition;
            if (stableId.StartsWith("participant.default", StringComparison.Ordinal))
                return PyralisAuthoringGraphSetupDomain.Participant;
            if (stableId.StartsWith("mode.", StringComparison.Ordinal))
                return PyralisAuthoringGraphSetupDomain.GameMode;
            if (stableId.StartsWith("session.", StringComparison.Ordinal))
                return PyralisAuthoringGraphSetupDomain.Session;

            return fieldDomain == PyralisAuthoringGraphSetupDomain.Unknown
                ? PyralisAuthoringGraphSetupDomain.SceneReadiness
                : fieldDomain;
        }

        private static PyralisAuthoringGraphSourceKind GetCapabilitySourceKind(PyralisAuthoringCapabilityDescriptor descriptor)
        {
            if (descriptor == null)
                return PyralisAuthoringGraphSourceKind.Unknown;

            return descriptor.SourceOrigin == PyralisAuthoringGraphSourceOrigin.Contract
                || descriptor.SourceOrigin == PyralisAuthoringGraphSourceOrigin.Reflection
                    ? PyralisAuthoringGraphSourceKind.AuthoringContract
                    : PyralisAuthoringGraphSourceKind.CapabilityVocabulary;
        }

        private static void AddSceneSurfaceNodes(
            UnityEngine.Object source,
            PyralisSetupRouteAnalysis route,
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges)
        {
            PyralisAuthoringSceneSurfaceSnapshot snapshot = PyralisAuthoringSceneSurfaceSnapshot.Build(source, route);
            if (snapshot == null || snapshot.Rows.Count == 0)
                return;

            int missingRecommended = 0;
            List<string> missingSurfaces = new List<string>();
            for (int i = 0; i < snapshot.Rows.Count; i++)
            {
                PyralisAuthoringSceneSurfaceRow row = snapshot.Rows[i];
                if (row == null)
                    continue;

                string nodeId = row.IssueCode == "SceneSurface.FallbackTypeName"
                    ? "scene." + NormalizeId(row.Surface + "." + row.DetectorId + "." + (row.CandidateObject != null ? row.CandidateObject.name : "unknown"))
                    : "scene." + NormalizeId(row.Surface);
                AddNode(nodes, new PyralisAuthoringGraphNode(
                    nodeId,
                    row.Surface,
                    PyralisAuthoringGraphNodeKind.SceneSurface,
                    PyralisAuthoringGraphSourceKind.SceneReadiness,
                    ConvertSceneSurfaceEvidence(row.EvidenceState),
                    guidance: row.Current,
                    nativeSetup: !string.IsNullOrWhiteSpace(row.NextFix) ? new[] { row.NextFix } : Array.Empty<string>(),
                    blockingReason: row.SupportsFirstProofAttempt ? string.Empty : row.NextFix,
                    nativeAction: row.NativeAction,
                    sourceObject: row.CandidateObject,
                    sourceOrigin: PyralisAuthoringGraphSourceOrigin.RuntimeEvidence,
                    workIntent: row.IssueCode == "SceneSurface.FallbackTypeName"
                        ? PyralisAuthoringGraphWorkIntent.Reference
                        : PyralisAuthoringGraphWorkIntent.Unknown,
                    issueSeverity: row.IssueCode == "SceneSurface.FallbackTypeName"
                        ? PyralisAuthoringIssueSeverity.Recommended
                        : PyralisAuthoringIssueSeverity.Info,
                    setupDomain: PyralisAuthoringGraphSetupDomain.SceneSurface,
                    issueCode: row.IssueCode));
                AddEdge(edges, "bootstrap.root", nodeId, PyralisAuthoringGraphEdgeKind.RelatesTo, "scene surface");

                if (!row.SupportsFirstProofAttempt)
                {
                    missingRecommended++;
                    if (!string.IsNullOrWhiteSpace(row.Surface))
                        missingSurfaces.Add(row.Surface);
                }
            }

            string sceneSurfaceMessage = missingRecommended == 0
                ? "Route-recommended scene surface evidence is present or not needed yet. Play Mode still proves behavior."
                : $"{missingRecommended} proof enhancer scene surface(s) are not detected yet: {string.Join(", ", missingSurfaces)}.";
            AddNode(nodes, new PyralisAuthoringGraphNode(
                "scene.surfaces",
                "Scene Surfaces",
                PyralisAuthoringGraphNodeKind.SceneSurface,
                PyralisAuthoringGraphSourceKind.SceneReadiness,
                missingRecommended == 0 ? PyralisAuthoringGraphEvidenceState.Ready : PyralisAuthoringGraphEvidenceState.Missing,
                guidance: sceneSurfaceMessage,
                blockingReason: missingRecommended == 0 ? string.Empty : sceneSurfaceMessage,
                sourceOrigin: PyralisAuthoringGraphSourceOrigin.RuntimeEvidence,
                setupDomain: PyralisAuthoringGraphSetupDomain.SceneSurface,
                issueCode: missingRecommended == 0 ? string.Empty : "SceneSurface.RecommendedMissing"));
            AddEdge(edges, "bootstrap.root", "scene.surfaces", PyralisAuthoringGraphEdgeKind.RelatesTo, "scene surface summary");
        }

        private static string AddProofNode(
            PyralisSetupRouteAnalysis route,
            PyralisAuthoringIntentSelection intentSelection,
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges)
        {
            string selectedProofTargetId = ResolveProofTargetId(route, intentSelection);
            PyralisAuthoringFact proofFact = ResolveProofFact(selectedProofTargetId);
            string proofNodeId = proofFact != null && !string.IsNullOrWhiteSpace(proofFact.StableId)
                ? proofFact.StableId
                : selectedProofTargetId;
            if (string.IsNullOrWhiteSpace(proofNodeId))
                proofNodeId = "proof.unresolved-route";
            bool unresolvedProof = string.Equals(proofNodeId, "proof.unresolved-route", StringComparison.Ordinal);
            bool genericProofTemplate = proofFact != null
                && proofFact.SourceKind != PyralisAuthoringFactSourceKind.FeatureContract;
            AddNode(nodes, new PyralisAuthoringGraphNode(
                proofNodeId,
                unresolvedProof
                    ? "No Active Proof Target"
                    : !string.IsNullOrWhiteSpace(proofFact?.DisplayName) ? proofFact.DisplayName : "Unresolved Route Proof",
                PyralisAuthoringGraphNodeKind.Proof,
                GetProofSourceKind(proofFact),
                unresolvedProof ? PyralisAuthoringGraphEvidenceState.Optional : PyralisAuthoringGraphEvidenceState.Unknown,
                proofTargetId: proofNodeId,
                guidance: unresolvedProof
                    ? "No first proof is selected yet. Assign a meaningful authored route or use Intent to choose the smallest capability to prove."
                    : GetProofGuidance(proofFact),
                nativeSetup: GetProofNativeSetup(proofFact),
                assignmentFields: Array.Empty<string>(),
                customizationMoments: Array.Empty<string>(),
                blockingReason: unresolvedProof ? string.Empty : proofFact != null ? proofFact.FirstProof : string.Empty,
                sourceOrigin: proofFact != null && proofFact.SourceKind == PyralisAuthoringFactSourceKind.FeatureContract
                    ? PyralisAuthoringGraphSourceOrigin.Contract
                    : PyralisAuthoringGraphSourceOrigin.GrammarFallback,
                setupDomain: PyralisAuthoringGraphSetupDomain.FeatureContract,
                issueCode: unresolvedProof ? "Proof.UnresolvedRoute" : string.Empty));

            if (genericProofTemplate)
            {
                string metadataNodeId = "proof-metadata." + NormalizeId(proofNodeId) + ".generic-template";
                AddNode(nodes, new PyralisAuthoringGraphNode(
                    metadataNodeId,
                    "Generic Proof Template",
                    PyralisAuthoringGraphNodeKind.Proof,
                    PyralisAuthoringGraphSourceKind.ProofVocabulary,
                    PyralisAuthoringGraphEvidenceState.Missing,
                    proofTargetId: proofNodeId,
                    guidance: $"{proofFact.DisplayName} is using generic proof vocabulary. Add FirstProofTargetId to a feature-owned contract when this route needs contract-owned proof meaning.",
                    blockingReason: "The graph can render this proof, but the proof target is still grammar-owned rather than contract-owned.",
                    sourceOrigin: PyralisAuthoringGraphSourceOrigin.GrammarFallback,
                    workIntent: PyralisAuthoringGraphWorkIntent.Reference,
                    issueSeverity: PyralisAuthoringIssueSeverity.Recommended,
                    setupDomain: PyralisAuthoringGraphSetupDomain.FeatureContract,
                    issueCode: "ContractMetadata.ProofTargetGenericTemplate"));
                AddEdge(edges, proofNodeId, metadataNodeId, PyralisAuthoringGraphEdgeKind.RelatesTo, "generic proof metadata");
            }

            RuntimeCapabilityFamily[] families = route?.CapabilityFamilies ?? Array.Empty<RuntimeCapabilityFamily>();
            for (int i = 0; i < families.Length; i++)
            {
                PyralisAuthoringCapabilityDescriptor descriptor = ResolveCapabilityDescriptorForFamily(families[i], intentSelection);
                AddEdge(edges, GetCapabilityNodeId(families[i], descriptor), proofNodeId, PyralisAuthoringGraphEdgeKind.SupportsProof, "supports proof");
            }

            return proofNodeId;
        }

        private static string ResolveProofTargetId(
            PyralisSetupRouteAnalysis route,
            PyralisAuthoringIntentSelection intentSelection)
        {
            RuntimeCapabilityFamily[] families = route?.CapabilityFamilies ?? Array.Empty<RuntimeCapabilityFamily>();
            if (families.Length == 0 || route == null || !route.HasSelectedCapabilities)
                return "proof.unresolved-route";

            string selectedDescriptorProofTargetId = ResolveSelectedDescriptorProofTargetId(intentSelection);
            if (!string.IsNullOrWhiteSpace(selectedDescriptorProofTargetId))
                return selectedDescriptorProofTargetId;

            for (int i = 0; i < families.Length; i++)
            {
                PyralisAuthoringCapabilityDescriptor descriptor = ResolveCapabilityDescriptorForFamily(families[i], intentSelection);
                if (descriptor != null && !string.IsNullOrWhiteSpace(descriptor.ProofTargetId))
                    return descriptor.ProofTargetId;
            }

            string genericProofTargetId = PyralisProofFamilyVocabulary.GetGenericProofTargetId(
                families,
                route.RequiresPawn,
                route.ParticipantTopology);
            if (!string.IsNullOrWhiteSpace(genericProofTargetId))
                return genericProofTargetId;

            return "proof.custom-object-effect";
        }

        private static string ResolveSelectedDescriptorProofTargetId(PyralisAuthoringIntentSelection intentSelection)
        {
            if (intentSelection?.DescriptorIds == null || intentSelection.DescriptorIds.Length == 0)
                return string.Empty;

            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors =
                PyralisAuthoringCapabilityDescriptorRegistry.All;
            for (int selectedIndex = 0; selectedIndex < intentSelection.DescriptorIds.Length; selectedIndex++)
            {
                string selectedId = intentSelection.DescriptorIds[selectedIndex];
                if (string.IsNullOrWhiteSpace(selectedId))
                    continue;

                for (int descriptorIndex = 0; descriptorIndex < descriptors.Count; descriptorIndex++)
                {
                    PyralisAuthoringCapabilityDescriptor descriptor = descriptors[descriptorIndex];
                    if (descriptor == null
                        || !string.Equals(descriptor.StableId, selectedId, StringComparison.Ordinal)
                        || !descriptor.IsContractSemanticSource
                        || string.IsNullOrWhiteSpace(descriptor.ProofTargetId))
                    {
                        continue;
                    }

                    return descriptor.ProofTargetId;
                }
            }

            return string.Empty;
        }

        private static string GetProofGuidance(PyralisAuthoringFact proofFact)
        {
            if (proofFact == null)
                return "Use the selected graph route to produce one small observable Play Mode result.";

            return FirstNonEmpty(
                proofFact.FirstProof,
                proofFact.RouteRelevance,
                proofFact.Summary);
        }

        private static string[] GetProofNativeSetup(PyralisAuthoringFact proofFact)
        {
            if (proofFact != null && proofFact.NativeActions.Length > 0)
            {
                List<string> actions = new List<string>();
                for (int i = 0; i < proofFact.NativeActions.Length; i++)
                {
                    PyralisAuthoringNativeAction action = proofFact.NativeActions[i];
                    if (action.Surface == PyralisAuthoringActionSurface.PlayMode)
                        actions.Add(action.ToGuidanceSentence());
                }

                if (actions.Count == 0)
                    return Array.Empty<string>();

                return actions.ToArray();
            }

            return Array.Empty<string>();
        }

        private static void AddContractNodes(
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges,
            string activeProofNodeId)
        {
            foreach (ResolvedAuthoringContract contract in ResolvedAuthoringContractRegistry.All)
            {
                if (contract == null || string.IsNullOrWhiteSpace(contract.StableId))
                    continue;

                string nodeId = "contract." + contract.StableId;
                AddNode(nodes, new PyralisAuthoringGraphNode(
                    nodeId,
                    contract.DisplayName,
                    PyralisAuthoringGraphNodeKind.Contract,
                    PyralisAuthoringGraphSourceKind.AuthoringContract,
                    PyralisAuthoringGraphEvidenceState.Unknown,
                    authoringCapability: contract.Capability,
                    proofTargetId: contract.FirstProofTargetId,
                    guidance: contract.Relevance,
                    nativeSetup: contract.NativeSetup,
                    assignmentFields: contract.AssignmentFields,
                    customizationMoments: contract.CustomizationMoments,
                    sourceContract: contract,
                    sourceOrigin: GetContractSourceOrigin(contract),
                    setupDomain: GetSetupDomain(contract.Capability)));

                AddContractMetadataEvidence(contract, nodeId, nodes, edges);

                if (!string.IsNullOrWhiteSpace(contract.FirstProofTargetId)
                    && string.Equals(contract.FirstProofTargetId, activeProofNodeId, StringComparison.Ordinal))
                {
                    AddEdge(edges, nodeId, activeProofNodeId, PyralisAuthoringGraphEdgeKind.Recommends, "proof guidance");
                }

                if (!string.IsNullOrWhiteSpace(contract.SetupNodeId))
                    AddEdge(edges, nodeId, contract.SetupNodeId, PyralisAuthoringGraphEdgeKind.RelatesTo, "setup node");
            }
        }

        private static void AddContractMetadataEvidence(
            ResolvedAuthoringContract contract,
            string contractNodeId,
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges)
        {
            if (contract == null || string.IsNullOrWhiteSpace(contractNodeId))
                return;

            bool missingCapabilityPath = string.IsNullOrWhiteSpace(contract.CapabilityPath);
            if (PyralisAuthoringCapabilityDescriptorRegistry.RequiresGameplayIngredientCapabilityPath(contract)
                && missingCapabilityPath)
            {
                string nodeId = "contract-metadata." + NormalizeId(contract.StableId) + ".capability-path";
                AddNode(nodes, new PyralisAuthoringGraphNode(
                    nodeId,
                    "Missing Capability Path",
                    PyralisAuthoringGraphNodeKind.Contract,
                    PyralisAuthoringGraphSourceKind.AuthoringContract,
                    PyralisAuthoringGraphEvidenceState.Missing,
                    authoringCapability: contract.Capability,
                    guidance: $"{contract.DisplayName} is marked selectable for Intent but does not declare AuthoringContract.CapabilityPath. Add a stable semantic path such as 'Movement/2D/Kinetic Motor', or set SelectableIntent = false if this contract is support-only.",
                    blockingReason: "Intent cannot safely expose this contract as a gameplay ingredient until the contract declares its semantic CapabilityPath.",
                    sourceContract: contract,
                    sourceOrigin: GetContractSourceOrigin(contract),
                    workIntent: PyralisAuthoringGraphWorkIntent.Reference,
                    issueSeverity: PyralisAuthoringIssueSeverity.Recommended,
                    setupDomain: PyralisAuthoringGraphSetupDomain.FeatureContract,
                    issueCode: "ContractMetadata.CapabilityPathMissing"));

                AddEdge(edges, contractNodeId, nodeId, PyralisAuthoringGraphEdgeKind.BlockedBy, "missing semantic metadata");
            }

            if (HasRoleTag(contract, AuthoringContractRoleTags.IntentRouteEssential)
                && missingCapabilityPath)
            {
                string nodeId = "contract-metadata." + NormalizeId(contract.StableId) + ".route-essential-capability-path";
                AddNode(nodes, new PyralisAuthoringGraphNode(
                    nodeId,
                    "Missing Route Essential Capability Path",
                    PyralisAuthoringGraphNodeKind.Contract,
                    PyralisAuthoringGraphSourceKind.AuthoringContract,
                    PyralisAuthoringGraphEvidenceState.Missing,
                    authoringCapability: contract.Capability,
                    guidance: $"{contract.DisplayName} is marked as an Intent route essential but does not declare AuthoringContract.CapabilityPath. Add a stable semantic path such as 'Core Setup/Input/Participant Input Router' so Intent can group route infrastructure without display-name guessing.",
                    blockingReason: "Intent can infer this route essential, but it cannot group it cleanly without a semantic CapabilityPath.",
                    sourceContract: contract,
                    sourceOrigin: GetContractSourceOrigin(contract),
                    workIntent: PyralisAuthoringGraphWorkIntent.Reference,
                    issueSeverity: PyralisAuthoringIssueSeverity.Recommended,
                    setupDomain: PyralisAuthoringGraphSetupDomain.FeatureContract,
                    issueCode: "ContractMetadata.RouteEssentialCapabilityPathMissing"));

                AddEdge(edges, contractNodeId, nodeId, PyralisAuthoringGraphEdgeKind.BlockedBy, "missing route essential metadata");
            }

            if (contract.Capability != AuthoringCapability.None
                && (contract.RuntimeFamilies == null || contract.RuntimeFamilies.Length == 0))
            {
                string nodeId = "contract-metadata." + NormalizeId(contract.StableId) + ".runtime-families";
                AddNode(nodes, new PyralisAuthoringGraphNode(
                    nodeId,
                    "Missing Runtime Families",
                    PyralisAuthoringGraphNodeKind.Contract,
                    PyralisAuthoringGraphSourceKind.AuthoringContract,
                    PyralisAuthoringGraphEvidenceState.Missing,
                    authoringCapability: contract.Capability,
                    guidance: $"{contract.DisplayName} declares authoring capability meaning but does not declare AuthoringContract.RuntimeFamilies. Add the runtime family or set Capability = AuthoringCapability.None if this contract should not steer routes.",
                    blockingReason: "Route analysis and proof selection will not infer runtime family from capability flags.",
                    sourceContract: contract,
                    sourceOrigin: GetContractSourceOrigin(contract),
                    workIntent: PyralisAuthoringGraphWorkIntent.Reference,
                    issueSeverity: PyralisAuthoringIssueSeverity.Recommended,
                    setupDomain: PyralisAuthoringGraphSetupDomain.FeatureContract,
                    issueCode: "ContractMetadata.RuntimeFamiliesMissing"));

                AddEdge(edges, contractNodeId, nodeId, PyralisAuthoringGraphEdgeKind.BlockedBy, "missing runtime family metadata");
            }
        }

        private static bool HasRoleTag(ResolvedAuthoringContract contract, string roleTag)
        {
            if (contract?.RoleTags == null || string.IsNullOrWhiteSpace(roleTag))
                return false;

            for (int i = 0; i < contract.RoleTags.Length; i++)
            {
                if (string.Equals(contract.RoleTags[i], roleTag, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static void AddRuntimeValidationEvidence(
            UnityEngine.Object source,
            PyralisSetupRouteAnalysis route,
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges)
        {
            PyralisSetupDependencyTree tree = BuildDependencyTree(source, route);
            if (tree == null)
                return;

            HashSet<UnityEngine.Object> visited = new HashSet<UnityEngine.Object>();
            for (int i = 0; i < tree.Nodes.Count; i++)
            {
                PyralisSetupDependencyNode dependencyNode = tree.Nodes[i];
                if (dependencyNode == null || dependencyNode.SourceObject == null)
                    continue;

                if (!visited.Add(dependencyNode.SourceObject))
                    continue;

                if (dependencyNode.SourceObject is not IRuntimeValidationProvider provider)
                    continue;

                IEnumerable<PyralisRuntimeValidationIssue> issues = provider.GetRuntimeValidationIssues();
                if (issues == null)
                    continue;

                foreach (PyralisRuntimeValidationIssue issue in issues)
                {
                    if (issue == null || string.IsNullOrWhiteSpace(issue.Message))
                        continue;

                    string nodeId = BuildRuntimeValidationNodeId(dependencyNode, issue);
                    AddNode(nodes, new PyralisAuthoringGraphNode(
                        nodeId,
                        dependencyNode.Label + " Setup",
                        PyralisAuthoringGraphNodeKind.ValidationEvidence,
                        PyralisAuthoringGraphSourceKind.RuntimeValidation,
                        ConvertRuntimeValidationSeverity(issue.Severity),
                        guidance: issue.Message,
                        nativeSetup: new[] { BuildRuntimeValidationNativeSetup(dependencyNode, issue) },
                        assignmentFields: BuildRuntimeValidationAssignmentFields(dependencyNode, issue),
                        blockingReason: issue.Severity == PyralisRuntimeValidationSeverity.Required ? issue.Message : string.Empty,
                        nativeAction: BuildRuntimeValidationNativeAction(dependencyNode, issue),
                        sourceObject: dependencyNode.SourceObject,
                        sourceOrigin: PyralisAuthoringGraphSourceOrigin.RuntimeEvidence,
                        workIntent: ConvertRuntimeValidationWorkIntent(issue.Severity),
                        issueSeverity: ConvertRuntimeValidationIssueSeverity(issue.Severity),
                        setupDomain: GetSetupDomain(dependencyNode, issue),
                        issueCode: BuildRuntimeValidationIssueCode(dependencyNode, issue)));

                    string anchorNodeId = ResolveDependencyAnchorNodeId(dependencyNode);
                    AddEdge(edges, anchorNodeId, nodeId, PyralisAuthoringGraphEdgeKind.RelatesTo, "runtime validation");
                    AddRuntimeValidationMetadataEvidence(dependencyNode, issue, nodeId, nodes, edges);
                }
            }
        }

        private static void AddRuntimeValidationMetadataEvidence(
            PyralisSetupDependencyNode dependencyNode,
            PyralisRuntimeValidationIssue issue,
            string validationNodeId,
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges)
        {
            if (dependencyNode == null || issue == null || string.IsNullOrWhiteSpace(validationNodeId))
                return;

            AddRuntimeValidationMetadataIssue(
                dependencyNode,
                validationNodeId,
                nodes,
                edges,
                string.IsNullOrWhiteSpace(issue.IssueCode),
                "issue-code",
                "Runtime validation issue is missing an explicit IssueCode.",
                "Add a stable issueCode to the local PyralisRuntimeValidationIssue so Hygiene and exports can track this setup rule without relying on message text.",
                "ValidationMetadata.IssueCodeMissing");
        }

        private static void AddRuntimeValidationMetadataIssue(
            PyralisSetupDependencyNode dependencyNode,
            string validationNodeId,
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges,
            bool shouldAdd,
            string suffix,
            string label,
            string guidance,
            string issueCode)
        {
            if (!shouldAdd)
                return;

            string nodeId = validationNodeId + ".metadata." + suffix;
            AddNode(nodes, new PyralisAuthoringGraphNode(
                nodeId,
                label,
                PyralisAuthoringGraphNodeKind.ValidationEvidence,
                PyralisAuthoringGraphSourceKind.RuntimeValidation,
                PyralisAuthoringGraphEvidenceState.Missing,
                guidance: guidance,
                blockingReason: guidance,
                nativeAction: new PyralisAuthoringNativeAction(
                    "Edit",
                    PyralisAuthoringActionSurface.ProjectWindow,
                    dependencyNode.SourceObject != null ? dependencyNode.SourceObject.GetType().Name : dependencyNode.Label,
                    "GetRuntimeValidationIssues",
                    "local validation emits structured graph evidence"),
                sourceObject: dependencyNode.SourceObject,
                sourceOrigin: PyralisAuthoringGraphSourceOrigin.RuntimeEvidence,
                workIntent: PyralisAuthoringGraphWorkIntent.Reference,
                issueSeverity: PyralisAuthoringIssueSeverity.Recommended,
                setupDomain: GetSetupDomain(dependencyNode, null),
                issueCode: issueCode));

            AddEdge(edges, validationNodeId, nodeId, PyralisAuthoringGraphEdgeKind.RelatesTo, "validation metadata");
        }

        private static PyralisSetupDependencyTree BuildDependencyTree(UnityEngine.Object source, PyralisSetupRouteAnalysis route)
        {
            UnityEngine.Object treeSource = source;
            if (treeSource == null && route != null)
                treeSource = route.Session != null ? route.Session : route.Mode;

            return treeSource != null ? PyralisSetupDependencyTree.Build(treeSource) : null;
        }

        private static string BuildRuntimeValidationNodeId(PyralisSetupDependencyNode dependencyNode, PyralisRuntimeValidationIssue issue)
        {
            string anchor = dependencyNode != null && !string.IsNullOrWhiteSpace(dependencyNode.StableId)
                ? dependencyNode.StableId
                : "unknown";
            string issueKey = !string.IsNullOrWhiteSpace(issue?.IssueCode)
                ? NormalizeId(issue.IssueCode)
                : ComputeStableHash(issue?.Message);
            return "runtimevalidation." + NormalizeId(anchor) + "." + issueKey;
        }

        private static string BuildRuntimeValidationNativeSetup(
            PyralisSetupDependencyNode dependencyNode,
            PyralisRuntimeValidationIssue issue)
        {
            string target = FirstNonEmpty(
                issue?.TargetLabel,
                dependencyNode != null && dependencyNode.SourceObject != null
                ? dependencyNode.SourceObject.GetType().Name
                : "the selected setup asset");
            if (!string.IsNullOrWhiteSpace(issue?.NativeAction))
                return issue.NativeAction;

            return "Open " + target + " in the Inspector and resolve this validation issue: " + issue?.Message;
        }

        private static string[] BuildRuntimeValidationAssignmentFields(
            PyralisSetupDependencyNode dependencyNode,
            PyralisRuntimeValidationIssue issue)
        {
            string field = FirstNonEmpty(issue?.FieldPath, dependencyNode?.SourceFieldPath);
            return !string.IsNullOrWhiteSpace(field) ? new[] { field } : Array.Empty<string>();
        }

        private static PyralisAuthoringNativeAction BuildRuntimeValidationNativeAction(
            PyralisSetupDependencyNode dependencyNode,
            PyralisRuntimeValidationIssue issue)
        {
            string target = FirstNonEmpty(
                issue?.TargetLabel,
                dependencyNode != null && dependencyNode.SourceObject != null
                ? dependencyNode.SourceObject.GetType().Name
                : "selected setup asset");
            string field = FirstNonEmpty(
                issue?.FieldPath,
                dependencyNode?.SourceFieldPath,
                "the Inspector field or component named by the validation message");
            string success = FirstNonEmpty(
                issue?.SuccessCheck,
                "the issue is gone from Map and the Inspector");
            return new PyralisAuthoringNativeAction(
                "Inspect",
                PyralisAuthoringActionSurface.Inspector,
                target,
                field,
                success);
        }

        private static PyralisAuthoringGraphEvidenceState ConvertRuntimeValidationSeverity(PyralisRuntimeValidationSeverity severity)
        {
            switch (severity)
            {
                case PyralisRuntimeValidationSeverity.Info:
                    return PyralisAuthoringGraphEvidenceState.Optional;
                case PyralisRuntimeValidationSeverity.Optional:
                    return PyralisAuthoringGraphEvidenceState.Optional;
                case PyralisRuntimeValidationSeverity.Recommended:
                    return PyralisAuthoringGraphEvidenceState.CandidateDetected;
                default:
                    return PyralisAuthoringGraphEvidenceState.Missing;
            }
        }

        private static PyralisAuthoringGraphWorkIntent ConvertRuntimeValidationWorkIntent(PyralisRuntimeValidationSeverity severity)
        {
            switch (severity)
            {
                case PyralisRuntimeValidationSeverity.Info:
                    return PyralisAuthoringGraphWorkIntent.Reference;
                case PyralisRuntimeValidationSeverity.Optional:
                    return PyralisAuthoringGraphWorkIntent.Optional;
                case PyralisRuntimeValidationSeverity.Recommended:
                    return PyralisAuthoringGraphWorkIntent.ProofEnhancer;
                default:
                    return PyralisAuthoringGraphWorkIntent.RequiredSetup;
            }
        }

        private static PyralisAuthoringIssueSeverity ConvertRuntimeValidationIssueSeverity(PyralisRuntimeValidationSeverity severity)
        {
            switch (severity)
            {
                case PyralisRuntimeValidationSeverity.Required:
                    return PyralisAuthoringIssueSeverity.Required;
                case PyralisRuntimeValidationSeverity.Recommended:
                    return PyralisAuthoringIssueSeverity.Recommended;
                default:
                    return PyralisAuthoringIssueSeverity.Info;
            }
        }

        private static string ResolveDependencyAnchorNodeId(PyralisSetupDependencyNode dependencyNode)
        {
            if (dependencyNode == null || string.IsNullOrWhiteSpace(dependencyNode.StableId))
                return string.Empty;

            string stableId = dependencyNode.StableId;
            if (stableId.StartsWith("pawn.definition.", StringComparison.Ordinal))
                return "pawn.definition";
            if (stableId.StartsWith("participant.default.", StringComparison.Ordinal))
                return "participant.default";
            if (stableId.StartsWith("mode.", StringComparison.Ordinal))
                return "mode.definition";

            return stableId;
        }

        private static void AddSceneReadinessEvidence(UnityEngine.Object source, List<PyralisAuthoringGraphNode> nodes, List<PyralisAuthoringGraphEdge> edges)
        {
            if (source is not GameplaySessionBootstrap bootstrap)
                return;

            PyralisSceneReadinessReport report = PyralisSceneReadinessValidator.BuildReport(bootstrap);
            IReadOnlyList<PyralisSceneReadinessIssue> issues = report.Issues;
            for (int i = 0; i < issues.Count; i++)
            {
                PyralisSceneReadinessIssue issue = issues[i];
                string nodeId = BuildSceneReadinessEvidenceNodeId(issue);
                AddNode(nodes, new PyralisAuthoringGraphNode(
                    nodeId,
                    issue.Category.ToString(),
                    PyralisAuthoringGraphNodeKind.ValidationEvidence,
                    PyralisAuthoringGraphSourceKind.SceneReadiness,
                    ConvertSceneReadinessSeverity(issue.Severity),
                    guidance: issue.Message,
                    nativeSetup: !string.IsNullOrWhiteSpace(issue.NativeAction) ? new[] { issue.NativeAction } : Array.Empty<string>(),
                    blockingReason: issue.Severity == PyralisSceneReadinessSeverity.RequiredBeforePlay ? issue.Message : string.Empty,
                    sourceOrigin: PyralisAuthoringGraphSourceOrigin.RuntimeEvidence,
                    workIntent: ConvertSceneReadinessWorkIntent(issue.Severity),
                    issueSeverity: ConvertSceneReadinessIssueSeverity(issue.Severity),
                    setupDomain: GetSetupDomain(issue.Category),
                    issueCode: "SceneReadiness." + issue.Category));
                AddEdge(edges, "bootstrap.root", nodeId, PyralisAuthoringGraphEdgeKind.RelatesTo, "scene readiness");
            }
        }

        private static string BuildSceneReadinessEvidenceNodeId(PyralisSceneReadinessIssue issue)
        {
            if (issue == null)
                return "scenereadiness.unknown";

            string category = NormalizeId(issue.Category.ToString());
            string severity = NormalizeId(issue.Severity.ToString());
            string messageHash = ComputeStableHash(issue.Category + "|" + issue.Severity + "|" + issue.Message);
            return "scenereadiness." + category + "." + severity + "." + messageHash;
        }

        private static void AddProofBlockerEdges(
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges,
            string activeProofNodeId)
        {
            if (string.IsNullOrWhiteSpace(activeProofNodeId))
                return;
            if (string.Equals(activeProofNodeId, "proof.unresolved-route", StringComparison.Ordinal))
                return;

            for (int i = 0; i < nodes.Count; i++)
            {
                PyralisAuthoringGraphNode node = nodes[i];
                if (node == null || string.Equals(node.StableId, activeProofNodeId, StringComparison.Ordinal))
                    continue;

                if (!BlocksProof(node))
                    continue;

                AddEdge(edges, activeProofNodeId, node.StableId, PyralisAuthoringGraphEdgeKind.BlockedBy, "missing required setup");
            }
        }

        private static bool BlocksProof(PyralisAuthoringGraphNode node)
        {
            bool missing = node.EvidenceState == PyralisAuthoringGraphEvidenceState.Missing
                || node.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked;
            if (!missing)
                return false;

            return node.Kind == PyralisAuthoringGraphNodeKind.SetupChain
                || node.Kind == PyralisAuthoringGraphNodeKind.RouteShape
                || node.Kind == PyralisAuthoringGraphNodeKind.UnitySurfaceRequirement
                || node.Kind == PyralisAuthoringGraphNodeKind.ValidationEvidence;
        }

        private static void ResolveProofReadiness(
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges,
            string activeProofNodeId)
        {
            if (string.IsNullOrWhiteSpace(activeProofNodeId))
                return;

            int proofIndex = -1;
            PyralisAuthoringGraphNode proofNode = null;
            for (int i = 0; i < nodes.Count; i++)
            {
                PyralisAuthoringGraphNode node = nodes[i];
                if (node != null && string.Equals(node.StableId, activeProofNodeId, StringComparison.Ordinal))
                {
                    proofIndex = i;
                    proofNode = node;
                    break;
                }
            }

            if (proofNode == null || proofIndex < 0)
                return;

            PyralisAuthoringGraphEvidenceState readiness = ResolveProofEvidenceState(nodes, edges, activeProofNodeId);
            nodes[proofIndex] = new PyralisAuthoringGraphNode(
                proofNode.StableId,
                proofNode.Label,
                proofNode.Kind,
                proofNode.SourceKind,
                readiness,
                proofNode.CapabilityFamily,
                proofNode.AuthoringCapability,
                proofNode.ProofTargetId,
                proofNode.Guidance,
                proofNode.NativeSetup,
                proofNode.AssignmentFields,
                proofNode.CustomizationMoments,
                proofNode.BlockingReason,
                proofNode.NativeAction,
                proofNode.SourceContract,
                proofNode.SourceObject,
                proofNode.SourceOrigin,
                ResolveProofWorkIntent(readiness),
                ResolveProofIssueSeverity(readiness));
        }

        private static PyralisAuthoringGraphEvidenceState ResolveProofEvidenceState(
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges,
            string activeProofNodeId)
        {
            bool hasSupport = false;
            bool hasMissingBlocker = false;
            bool hasBlockedBlocker = false;

            for (int i = 0; i < edges.Count; i++)
            {
                PyralisAuthoringGraphEdge edge = edges[i];
                if (edge == null)
                    continue;

                if (edge.Kind == PyralisAuthoringGraphEdgeKind.SupportsProof
                    && string.Equals(edge.ToNodeId, activeProofNodeId, StringComparison.Ordinal))
                {
                    hasSupport = true;
                    continue;
                }

                if (edge.Kind != PyralisAuthoringGraphEdgeKind.BlockedBy
                    || !string.Equals(edge.FromNodeId, activeProofNodeId, StringComparison.Ordinal))
                {
                    continue;
                }

                PyralisAuthoringGraphNode blocker = FindNode(nodes, edge.ToNodeId);
                if (blocker == null)
                    continue;

                if (blocker.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked)
                    hasBlockedBlocker = true;
                else if (blocker.EvidenceState == PyralisAuthoringGraphEvidenceState.Missing)
                    hasMissingBlocker = true;
            }

            if (hasBlockedBlocker)
                return PyralisAuthoringGraphEvidenceState.Blocked;
            if (hasMissingBlocker)
                return PyralisAuthoringGraphEvidenceState.Missing;
            if (hasSupport)
                return PyralisAuthoringGraphEvidenceState.CandidateDetected;

            return PyralisAuthoringGraphEvidenceState.Unknown;
        }

        private static PyralisAuthoringGraphNode FindNode(List<PyralisAuthoringGraphNode> nodes, string stableId)
        {
            if (string.IsNullOrWhiteSpace(stableId))
                return null;

            for (int i = 0; i < nodes.Count; i++)
            {
                PyralisAuthoringGraphNode node = nodes[i];
                if (node != null && string.Equals(node.StableId, stableId, StringComparison.Ordinal))
                    return node;
            }

            return null;
        }

        private static PyralisAuthoringGraphWorkIntent ResolveProofWorkIntent(PyralisAuthoringGraphEvidenceState readiness)
        {
            return readiness == PyralisAuthoringGraphEvidenceState.Missing
                || readiness == PyralisAuthoringGraphEvidenceState.Blocked
                    ? PyralisAuthoringGraphWorkIntent.RequiredSetup
                    : PyralisAuthoringGraphWorkIntent.ProofEnhancer;
        }

        private static PyralisAuthoringIssueSeverity ResolveProofIssueSeverity(PyralisAuthoringGraphEvidenceState readiness)
        {
            return readiness switch
            {
                PyralisAuthoringGraphEvidenceState.Blocked => PyralisAuthoringIssueSeverity.Blocked,
                PyralisAuthoringGraphEvidenceState.Missing => PyralisAuthoringIssueSeverity.Required,
                PyralisAuthoringGraphEvidenceState.CandidateDetected => PyralisAuthoringIssueSeverity.Recommended,
                PyralisAuthoringGraphEvidenceState.Optional => PyralisAuthoringIssueSeverity.Optional,
                _ => PyralisAuthoringIssueSeverity.Info
            };
        }

        private static void AddNode(List<PyralisAuthoringGraphNode> nodes, PyralisAuthoringGraphNode node)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (string.Equals(nodes[i].StableId, node.StableId, StringComparison.Ordinal))
                    return;
            }

            nodes.Add(node);
        }

        private static void AddEdge(List<PyralisAuthoringGraphEdge> edges, string fromNodeId, string toNodeId, PyralisAuthoringGraphEdgeKind kind, string label)
        {
            if (string.IsNullOrWhiteSpace(fromNodeId) || string.IsNullOrWhiteSpace(toNodeId))
                return;

            edges.Add(new PyralisAuthoringGraphEdge(fromNodeId, toNodeId, kind, label));
        }

        private static PyralisAuthoringGraphEvidenceState ConvertSceneReadinessSeverity(PyralisSceneReadinessSeverity severity)
        {
            return severity switch
            {
                PyralisSceneReadinessSeverity.RequiredBeforePlay => PyralisAuthoringGraphEvidenceState.Blocked,
                PyralisSceneReadinessSeverity.RecommendedBeforePlay => PyralisAuthoringGraphEvidenceState.Missing,
                PyralisSceneReadinessSeverity.ProofEnhancer => PyralisAuthoringGraphEvidenceState.CandidateDetected,
                _ => PyralisAuthoringGraphEvidenceState.Unknown
            };
        }

        private static PyralisAuthoringGraphWorkIntent ConvertSceneReadinessWorkIntent(PyralisSceneReadinessSeverity severity)
        {
            return severity switch
            {
                PyralisSceneReadinessSeverity.RequiredBeforePlay => PyralisAuthoringGraphWorkIntent.RequiredSetup,
                PyralisSceneReadinessSeverity.RecommendedBeforePlay => PyralisAuthoringGraphWorkIntent.ProofEnhancer,
                PyralisSceneReadinessSeverity.ProofEnhancer => PyralisAuthoringGraphWorkIntent.ProofEnhancer,
                _ => PyralisAuthoringGraphWorkIntent.Unknown
            };
        }

        private static PyralisAuthoringIssueSeverity ConvertSceneReadinessIssueSeverity(PyralisSceneReadinessSeverity severity)
        {
            return severity switch
            {
                PyralisSceneReadinessSeverity.RequiredBeforePlay => PyralisAuthoringIssueSeverity.Required,
                PyralisSceneReadinessSeverity.RecommendedBeforePlay => PyralisAuthoringIssueSeverity.Recommended,
                PyralisSceneReadinessSeverity.ProofEnhancer => PyralisAuthoringIssueSeverity.Recommended,
                _ => PyralisAuthoringIssueSeverity.Info
            };
        }

        private static PyralisAuthoringGraphEvidenceState ConvertSceneSurfaceEvidence(PyralisAuthoringEvidenceState evidenceState)
        {
            return evidenceState switch
            {
                PyralisAuthoringEvidenceState.Validated => PyralisAuthoringGraphEvidenceState.Ready,
                PyralisAuthoringEvidenceState.PlayProven => PyralisAuthoringGraphEvidenceState.Ready,
                PyralisAuthoringEvidenceState.LinkedToActiveSetup => PyralisAuthoringGraphEvidenceState.Ready,
                PyralisAuthoringEvidenceState.CandidateDetected => PyralisAuthoringGraphEvidenceState.CandidateDetected,
                PyralisAuthoringEvidenceState.Missing => PyralisAuthoringGraphEvidenceState.Missing,
                PyralisAuthoringEvidenceState.Conflict => PyralisAuthoringGraphEvidenceState.Blocked,
                PyralisAuthoringEvidenceState.NotRelevant => PyralisAuthoringGraphEvidenceState.Optional,
                _ => PyralisAuthoringGraphEvidenceState.Unknown
            };
        }

        private static ParticipantDefinition GetFirstParticipant(SessionDefinition session)
        {
            if (session == null || session.defaultParticipants == null || session.defaultParticipants.Length == 0)
                return null;

            return session.defaultParticipants[0];
        }

        private static string GetPawnGuidance(PyralisSetupRouteAnalysis route)
        {
            if (route == null || !route.RequiresPawn)
                return "No-pawn route: empty PawnDefinition fields are correct unless you intentionally add actor bodies.";

            if (string.IsNullOrWhiteSpace(route.ParticipantPawnIssue))
                return "Pawn-backed route has participant pawn setup.";

            return route.ParticipantPawnIssue;
        }

        private static PyralisAuthoringGraphEvidenceState ResolveCameraFocusEvidence(
            CameraRigProfile profile,
            bool requiresPawnTarget,
            bool hasPawnTarget)
        {
            if (profile == null)
                return PyralisAuthoringGraphEvidenceState.Optional;

            if (profile.focusMode == CameraRigProfile.CameraFocusMode.ManualCinemachine
                || profile.focusMode == CameraRigProfile.CameraFocusMode.PlayfieldCenter)
            {
                return PyralisAuthoringGraphEvidenceState.Ready;
            }

            if (profile.focusMode == CameraRigProfile.CameraFocusMode.ExplicitSceneTarget)
                return PyralisAuthoringGraphEvidenceState.CandidateDetected;

            if (!requiresPawnTarget)
                return PyralisAuthoringGraphEvidenceState.Ready;

            return hasPawnTarget
                ? PyralisAuthoringGraphEvidenceState.Ready
                : PyralisAuthoringGraphEvidenceState.CandidateDetected;
        }

        private static string BuildCameraFocusGuidance(
            CameraRigProfile profile,
            bool requiresPawnTarget,
            bool hasPawnTarget,
            PawnDefinition pawn)
        {
            if (profile == null)
                return "Camera focus waits until GameModeDefinition.cameraRigProfile is assigned.";

            switch (profile.focusMode)
            {
                case CameraRigProfile.CameraFocusMode.ManualCinemachine:
                    return "CameraRigProfile uses Manual Cinemachine. Pyralis will not overwrite Cinemachine Follow or LookAt; wire those directly in the scene.";
                case CameraRigProfile.CameraFocusMode.PlayfieldCenter:
                    return "CameraRigProfile uses Playfield Center. A pawn camera target is not required; Cinemachine follows the playfield center computed from the active PlayfieldProfile.";
                case CameraRigProfile.CameraFocusMode.ExplicitSceneTarget:
                    return "CameraRigProfile uses Explicit Scene Target. Assign CinemachineCameraRigController.explicitFocusTarget when the route should frame a menu, cursor, board, or authored scene anchor.";
            }

            if (!requiresPawnTarget)
                return "CameraRigProfile is participant-focused, but this route does not currently require actor bodies.";

            if (hasPawnTarget)
                return "Pawn prefab exposes PawnCameraTarget, so Pyralis can route participant pawn focus into Cinemachine.";

            string prefabName = pawn != null && pawn.pawnPrefab != null ? pawn.pawnPrefab.name : "pawn prefab";
            return $"Pawn-focused camera can use `{prefabName}` root as a fallback, but adding PawnCameraTarget makes follow/look-at sockets explicit for beginners and avoids hidden camera pivots.";
        }

        private static string[] BuildCameraFocusNativeSetup(
            CameraRigProfile profile,
            bool requiresPawnTarget,
            bool hasPawnTarget)
        {
            if (profile == null)
                return Array.Empty<string>();

            if (profile.focusMode == CameraRigProfile.CameraFocusMode.ManualCinemachine)
                return new[] { "Select the Cinemachine Camera and assign Follow/LookAt manually; Pyralis will leave those fields alone." };

            if (profile.focusMode == CameraRigProfile.CameraFocusMode.PlayfieldCenter)
                return new[] { "Assign GameModeDefinition.playfieldProfile when this route frames authored playfield bounds." };

            if (profile.focusMode == CameraRigProfile.CameraFocusMode.ExplicitSceneTarget)
                return new[] { "Select Camera Root and assign CinemachineCameraRigController.explicitFocusTarget to the scene anchor the camera should frame." };

            if (requiresPawnTarget && !hasPawnTarget)
                return new[] { "Open the pawn prefab, add NeonBlack/Pyralis/Pawn Camera Target, then assign Follow Target and optional Look At Target." };

            return new[] { "Cinemachine follows the resolved participant pawn camera target." };
        }

        private static string[] BuildCameraFocusAssignmentFields(CameraRigProfile profile, bool requiresPawnTarget)
        {
            if (profile == null)
                return Array.Empty<string>();

            if (profile.focusMode == CameraRigProfile.CameraFocusMode.ExplicitSceneTarget)
                return new[] { "CinemachineCameraRigController.explicitFocusTarget" };

            if (requiresPawnTarget)
                return new[] { "PawnCameraTarget.followTarget", "PawnCameraTarget.lookAtTarget" };

            return new[] { "CameraRigProfile.focusMode" };
        }

        private static bool HasPawnCameraTarget(PawnDefinition pawn)
        {
            if (pawn == null || pawn.pawnPrefab == null)
                return false;

            return pawn.pawnPrefab.GetComponentInChildren<PawnCameraTarget>(true) != null;
        }

        private static string GetCapabilitySummaryGuidance(PyralisSetupRouteAnalysis route, bool hasCapabilities)
        {
            if (!hasCapabilities || route == null || !route.HasSelectedCapabilities)
                return "Choose Intent capability ingredients or create gameplay assets that expose capabilities through contracts and serialized references.";

            return route.RouteName;
        }

        private static PyralisAuthoringCapabilityDescriptor ResolveCapabilityDescriptorForFamily(
            RuntimeCapabilityFamily family,
            PyralisAuthoringIntentSelection intentSelection)
        {
            if (intentSelection?.DescriptorIds != null && intentSelection.DescriptorIds.Length > 0)
            {
                IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors =
                    PyralisAuthoringCapabilityDescriptorRegistry.All;
                for (int selectedIndex = 0; selectedIndex < intentSelection.DescriptorIds.Length; selectedIndex++)
                {
                    string selectedId = intentSelection.DescriptorIds[selectedIndex];
                    if (string.IsNullOrWhiteSpace(selectedId))
                        continue;

                    for (int descriptorIndex = 0; descriptorIndex < descriptors.Count; descriptorIndex++)
                    {
                        PyralisAuthoringCapabilityDescriptor descriptor = descriptors[descriptorIndex];
                        if (descriptor != null
                            && descriptor.Family == family
                            && descriptor.IsContractSemanticSource
                            && string.Equals(descriptor.StableId, selectedId, StringComparison.Ordinal))
                        {
                            return descriptor;
                        }
                    }
                }
            }

            return PyralisAuthoringCapabilityDescriptorRegistry.FindPrimaryByFamily(family);
        }

        private static string GetCapabilityNodeId(RuntimeCapabilityFamily family, PyralisAuthoringCapabilityDescriptor descriptor)
        {
            if (descriptor != null && !string.IsNullOrWhiteSpace(descriptor.StableId))
                return descriptor.StableId;

            return "capability." + NormalizeId(family.ToString());
        }

        private static PyralisAuthoringGraphSourceOrigin GetContractSourceOrigin(ResolvedAuthoringContract contract)
        {
            if (contract == null)
                return PyralisAuthoringGraphSourceOrigin.Contract;

            return contract.Confidence == PyralisAuthoringConfidence.Inferred
                || contract.Confidence == PyralisAuthoringConfidence.ConventionDerived
                    ? PyralisAuthoringGraphSourceOrigin.Reflection
                    : PyralisAuthoringGraphSourceOrigin.Contract;
        }

        private static bool ContainsFamily(RuntimeCapabilityFamily[] families, RuntimeCapabilityFamily family)
        {
            if (families == null)
                return false;

            for (int i = 0; i < families.Length; i++)
            {
                if (families[i] == family)
                    return true;
            }

            return false;
        }

        private static PyralisAuthoringFact ResolveProofFact(string proofTargetId)
        {
            if (string.IsNullOrWhiteSpace(proofTargetId))
                return null;

            return PyralisProofFamilyVocabulary.FindProofFact(proofTargetId);
        }

        private static PyralisAuthoringGraphSourceKind GetProofSourceKind(PyralisAuthoringFact proofFact)
        {
            if (proofFact == null)
                return PyralisAuthoringGraphSourceKind.CoreSetup;

            return proofFact.SourceKind == PyralisAuthoringFactSourceKind.FeatureContract
                ? PyralisAuthoringGraphSourceKind.AuthoringContract
                : PyralisAuthoringGraphSourceKind.ProofVocabulary;
        }

        private static string NormalizeId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "unknown";

            char[] chars = value.ToLowerInvariant().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]))
                    chars[i] = '-';
            }

            return new string(chars).Trim('-');
        }

        private static string ComputeStableHash(string value)
        {
            unchecked
            {
                uint hash = 2166136261;
                string normalized = value ?? string.Empty;
                for (int i = 0; i < normalized.Length; i++)
                {
                    hash ^= normalized[i];
                    hash *= 16777619;
                }

                return hash.ToString("x8");
            }
        }

        private static string[] Combine(params string[][] groups)
        {
            List<string> values = new List<string>();
            if (groups == null)
                return Array.Empty<string>();

            for (int i = 0; i < groups.Length; i++)
            {
                string[] group = groups[i];
                if (group == null)
                    continue;

                for (int j = 0; j < group.Length; j++)
                {
                    if (!string.IsNullOrWhiteSpace(group[j]))
                        values.Add(group[j]);
                }
            }

            return values.ToArray();
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

        private static string FormatNativeAction(PyralisAuthoringNativeAction action)
        {
            List<string> parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(action.Verb))
                parts.Add(action.Verb);
            if (!string.IsNullOrWhiteSpace(action.Target))
                parts.Add(action.Target);
            if (!string.IsNullOrWhiteSpace(action.FieldOrComponent))
                parts.Add(action.FieldOrComponent);
            if (!string.IsNullOrWhiteSpace(action.SuccessCheck))
                parts.Add(action.SuccessCheck);

            return string.Join(" - ", parts);
        }
    }
}
