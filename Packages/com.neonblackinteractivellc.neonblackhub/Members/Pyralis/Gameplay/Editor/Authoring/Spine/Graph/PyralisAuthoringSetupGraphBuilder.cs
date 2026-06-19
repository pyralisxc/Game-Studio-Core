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
            AddCapabilityNodes(route, nodes, edges);
            AddRouteShapeNode(route, nodes, edges);
            AddParticipantNodes(route, nodes, edges);
            AddSceneSurfaceNodes(source, route, nodes, edges);
            string activeProofNodeId = AddProofNode(route, nodes, edges);
            AddContractNodes(nodes, edges, activeProofNodeId);
            AddRuntimeValidationEvidence(source, route, nodes, edges);
            AddReflectedDependencyEvidence(route, nodes, edges);
            AddSetupFlowEvidence(source, route, nodes, edges);
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

            return PyralisSetupRouteAnalysis.WithAdditionalCapabilityFamilies(route, focusedFamilies);
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
                PyralisAuthoringGraphSourceKind.SetupFlow,
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
                sourceOrigin: PyralisAuthoringGraphSourceOrigin.SpineGrammar));

            AddNode(nodes, new PyralisAuthoringGraphNode(
                "session.definition",
                "Session Definition",
                PyralisAuthoringGraphNodeKind.SetupChain,
                PyralisAuthoringGraphSourceKind.SetupFlow,
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
                    : PyralisAuthoringGraphSourceOrigin.SpineGrammar));

            AddNode(nodes, new PyralisAuthoringGraphNode(
                "mode.definition",
                "Game Mode Definition",
                PyralisAuthoringGraphNodeKind.SetupChain,
                PyralisAuthoringGraphSourceKind.SetupFlow,
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
                    : PyralisAuthoringGraphSourceOrigin.SpineGrammar));

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
                PyralisAuthoringGraphSourceKind.SetupFlow,
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
                    : PyralisAuthoringGraphSourceOrigin.SpineGrammar));

            AddNode(nodes, new PyralisAuthoringGraphNode(
                "pawn.definition",
                requiresPawn ? "Pawn Definition" : "No Pawn Needed",
                PyralisAuthoringGraphNodeKind.UnitySurfaceRequirement,
                PyralisAuthoringGraphSourceKind.SetupFlow,
                pawnReady ? PyralisAuthoringGraphEvidenceState.Ready : PyralisAuthoringGraphEvidenceState.Missing,
                guidance: GetPawnGuidance(route),
                assignmentFields: new[] { "ParticipantDefinition.defaultPawn" },
                blockingReason: pawnReady ? string.Empty : route?.ParticipantPawnIssue,
                sourceObject: pawn != null ? pawn : participant != null ? participant : session,
                sourceOrigin: pawn != null || participant != null || session != null
                    ? PyralisAuthoringGraphSourceOrigin.UserAuthoredSetup
                    : PyralisAuthoringGraphSourceOrigin.SpineGrammar));

            AddEdge(edges, "session.definition", "participant.default", PyralisAuthoringGraphEdgeKind.DependsOn, "default participants");
            AddEdge(edges, "participant.default", "pawn.definition", PyralisAuthoringGraphEdgeKind.DependsOn, "pawn route");
        }

        private static void AddReflectedDependencyEvidence(
            PyralisSetupRouteAnalysis route,
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges)
        {
            if (route == null)
                return;

            SessionDefinition session = route.Session;
            GameModeDefinition mode = route.Mode;
            ParticipantDefinition participant = route.Participant;
            PawnDefinition pawn = route.Pawn;

            if (session != null && mode == null)
            {
                AddMissingReflectedReference(
                    nodes,
                    edges,
                    "dependency.session.default-game-mode",
                    "Assign Default Game Mode",
                    "SessionDefinition",
                    "defaultGameMode",
                    "GameModeDefinition",
                    session,
                    "session.definition",
                    "mode.definition",
                    "Create or assign a GameModeDefinition on SessionDefinition.defaultGameMode so the graph can reflect route rules and feature requirements.",
                    PyralisAuthoringGraphEvidenceState.Missing,
                    PyralisAuthoringGraphWorkIntent.RequiredSetup,
                    PyralisAuthoringIssueSeverity.Required);
            }

            if (session != null && !route.HasParticipants)
            {
                AddMissingReflectedReference(
                    nodes,
                    edges,
                    "dependency.session.default-participants",
                    "Assign Default Participant",
                    "SessionDefinition",
                    "defaultParticipants",
                    "ParticipantDefinition",
                    session,
                    "session.definition",
                    "participant.default",
                    "Create or assign at least one ParticipantDefinition in SessionDefinition.defaultParticipants so the route has a player, AI, seat, hand, faction, or command owner.",
                    PyralisAuthoringGraphEvidenceState.Missing,
                    PyralisAuthoringGraphWorkIntent.RequiredSetup,
                    PyralisAuthoringIssueSeverity.Required);
            }

            if (route.RequiresPawn && participant != null && participant.defaultPawn == null)
            {
                AddMissingReflectedReference(
                    nodes,
                    edges,
                    "dependency.participant.default-pawn",
                    "Assign Participant Pawn",
                    "ParticipantDefinition",
                    "defaultPawn",
                    "PawnDefinition",
                    participant,
                    "participant.default",
                    "pawn.definition",
                    "Create or assign PawnDefinition on ParticipantDefinition.defaultPawn because this reflected route needs a pawn-backed participant.",
                    PyralisAuthoringGraphEvidenceState.Missing,
                    PyralisAuthoringGraphWorkIntent.RequiredSetup,
                    PyralisAuthoringIssueSeverity.Required);
            }

            if (route.RequiresPawn && participant != null && participant.inputProfile == null)
            {
                AddMissingReflectedReference(
                    nodes,
                    edges,
                    "dependency.participant.input-profile",
                    "Assign Input Profile",
                    "ParticipantDefinition",
                    "inputProfile",
                    "InputProfile",
                    participant,
                    "participant.default",
                    "route.shape",
                    "Create or assign InputProfile on ParticipantDefinition.inputProfile so the participant controlling this pawn can route movement input.",
                    PyralisAuthoringGraphEvidenceState.Missing,
                    PyralisAuthoringGraphWorkIntent.RequiredSetup,
                    PyralisAuthoringIssueSeverity.Required);
            }

            if (route.RequiresPawn && pawn != null && pawn.pawnPrefab == null)
            {
                AddMissingReflectedReference(
                    nodes,
                    edges,
                    "dependency.pawn.pawn-prefab",
                    "Assign Pawn Prefab",
                    "PawnDefinition",
                    "pawnPrefab",
                    "GameObject prefab",
                    pawn,
                    "pawn.definition",
                    "route.shape",
                    "Assign the authored pawn prefab on PawnDefinition.pawnPrefab so the participant spawn path has a visible runtime body.",
                    PyralisAuthoringGraphEvidenceState.Missing,
                    PyralisAuthoringGraphWorkIntent.RequiredSetup,
                    PyralisAuthoringIssueSeverity.Required);
            }

            if (route.UsesPawnGameplay() && pawn != null && pawn.movementProfile == null)
            {
                AddMissingReflectedReference(
                    nodes,
                    edges,
                    "dependency.pawn.movement-profile",
                    "Assign Pawn Movement Profile",
                    "PawnDefinition",
                    "movementProfile",
                    "PawnMovementProfile",
                    pawn,
                    "pawn.definition",
                    "route.shape",
                    "Create or assign PawnMovementProfile on PawnDefinition.movementProfile so the pawn has movement tuning for the first movement proof.",
                    PyralisAuthoringGraphEvidenceState.Missing,
                    PyralisAuthoringGraphWorkIntent.RequiredSetup,
                    PyralisAuthoringIssueSeverity.Required);
            }

            if (route.UsesPawnGameplay() && pawn != null && pawn.presentationProfile == null)
            {
                AddMissingReflectedReference(
                    nodes,
                    edges,
                    "dependency.pawn.presentation-profile",
                    "Assign Pawn Presentation Profile",
                    "PawnDefinition",
                    "presentationProfile",
                    "PawnPresentationProfile",
                    pawn,
                    "pawn.definition",
                    "route.shape",
                    "Create or assign PawnPresentationProfile on PawnDefinition.presentationProfile so the spawned pawn knows its Sprite2D, Billboard2_5D, or Rigged3D presentation lane.",
                    PyralisAuthoringGraphEvidenceState.Missing,
                    PyralisAuthoringGraphWorkIntent.RequiredSetup,
                    PyralisAuthoringIssueSeverity.Required);
            }

            if (ContainsFamily(route.CapabilityFamilies, RuntimeCapabilityFamily.AnimationPresentation)
                && pawn != null
                && pawn.animationProfile == null)
            {
                AddMissingReflectedReference(
                    nodes,
                    edges,
                    "dependency.pawn.animation-profile",
                    "Assign Pawn Animation Profile",
                    "PawnDefinition",
                    "animationProfile",
                    "PawnAnimationProfile",
                    pawn,
                    "pawn.definition",
                    "route.shape",
                    "Create or assign PawnAnimationProfile on PawnDefinition.animationProfile when this route includes animation/presentation intent.",
                    PyralisAuthoringGraphEvidenceState.Missing,
                    PyralisAuthoringGraphWorkIntent.RequiredSetup,
                    PyralisAuthoringIssueSeverity.Required);
            }

            if ((route.UsesPawnGameplay() || route.UsesCamera()) && mode != null && mode.cameraRigProfile == null)
            {
                AddMissingReflectedReference(
                    nodes,
                    edges,
                    "dependency.mode.camera-rig-profile",
                    "Assign Camera Rig Profile",
                    "GameModeDefinition",
                    "cameraRigProfile",
                    "CameraRigProfile",
                    mode,
                    "mode.definition",
                    "route.shape",
                    "Create or assign CameraRigProfile on GameModeDefinition.cameraRigProfile so the scene camera route has an explicit focus mode and framing profile.",
                    PyralisAuthoringGraphEvidenceState.Missing,
                    PyralisAuthoringGraphWorkIntent.RequiredSetup,
                    PyralisAuthoringIssueSeverity.Required);
            }

            if (route.UsesPlayfield() && mode != null && mode.playfieldProfile == null)
            {
                AddMissingReflectedReference(
                    nodes,
                    edges,
                    "dependency.mode.playfield-profile",
                    "Assign Playfield Profile",
                    "GameModeDefinition",
                    "playfieldProfile",
                    "PlayfieldProfile",
                    mode,
                    "mode.definition",
                    "route.shape",
                    "Create or assign PlayfieldProfile on GameModeDefinition.playfieldProfile when the route needs authored movement bounds, board space, arena depth, or playfield rules.",
                    PyralisAuthoringGraphEvidenceState.CandidateDetected,
                    PyralisAuthoringGraphWorkIntent.ProofEnhancer,
                    PyralisAuthoringIssueSeverity.Recommended);
            }
        }

        private static void AddMissingReflectedReference(
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges,
            string nodeId,
            string label,
            string ownerType,
            string fieldName,
            string expectedType,
            UnityEngine.Object owner,
            string ownerNodeId,
            string routeNodeId,
            string guidance,
            PyralisAuthoringGraphEvidenceState evidenceState,
            PyralisAuthoringGraphWorkIntent workIntent,
            PyralisAuthoringIssueSeverity issueSeverity)
        {
            string fieldPath = ownerType + "." + fieldName;
            PyralisAuthoringNativeAction nativeAction = new PyralisAuthoringNativeAction(
                "Create or assign",
                PyralisAuthoringActionSurface.Inspector,
                ownerType,
                fieldName,
                fieldPath + " references a " + expectedType);

            AddNode(nodes, new PyralisAuthoringGraphNode(
                nodeId,
                label,
                PyralisAuthoringGraphNodeKind.AssignmentField,
                PyralisAuthoringGraphSourceKind.Reflection,
                evidenceState,
                guidance: guidance,
                nativeSetup: new[] { FormatNativeAction(nativeAction) },
                assignmentFields: new[] { fieldPath },
                blockingReason: evidenceState == PyralisAuthoringGraphEvidenceState.Missing ? guidance : string.Empty,
                nativeAction: nativeAction,
                sourceObject: owner,
                sourceOrigin: PyralisAuthoringGraphSourceOrigin.Reflection,
                workIntent: workIntent,
                issueSeverity: issueSeverity));

            AddEdge(edges, ownerNodeId, nodeId, PyralisAuthoringGraphEdgeKind.DependsOn, fieldName);
            AddEdge(edges, routeNodeId, nodeId, PyralisAuthoringGraphEdgeKind.DependsOn, "reflected route dependency");
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
                PyralisAuthoringGraphSourceKind.SetupFlow,
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
                        : PyralisAuthoringIssueSeverity.Info));

            AddEdge(edges, "mode.definition", "route.camera-focus", PyralisAuthoringGraphEdgeKind.DependsOn, "camera focus mode");
            if (requiresPawnTarget)
                AddEdge(edges, "pawn.definition", "route.camera-focus", PyralisAuthoringGraphEdgeKind.DependsOn, "pawn camera target");
        }

        private static void AddCapabilityNodes(
            PyralisSetupRouteAnalysis route,
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
                    : PyralisAuthoringGraphSourceOrigin.SpineGrammar));
            AddEdge(edges, "mode.definition", "capability.selected", PyralisAuthoringGraphEdgeKind.Satisfies, "reflected capabilities");

            for (int i = 0; i < families.Length; i++)
            {
                RuntimeCapabilityFamily family = families[i];
                PyralisAuthoringCapabilityDescriptor descriptor = PyralisAuthoringCapabilityDescriptorRegistry.FindPrimaryByFamily(family);
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
                        : PyralisAuthoringGraphSourceOrigin.UserAuthoredSetup));

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
                PyralisAuthoringGraphSourceKind.SetupFlow,
                state,
                guidance: guidance,
                nativeSetup: Array.Empty<string>(),
                assignmentFields: GetRouteShapeAssignmentFields(requiresPawn, hasGameplayFocus),
                blockingReason: hasOwnershipIssue
                    ? FirstNonEmpty(route?.ParticipantPawnIssue, "Assign at least one ParticipantDefinition so the route has a player, AI, seat, hand, faction, or control owner.")
                    : string.Empty,
                sourceObject: route?.Session != null ? route.Session : route?.Mode,
                sourceOrigin: PyralisAuthoringGraphSourceOrigin.SpineGrammar));

            AddEdge(edges, "capability.selected", "route.shape", PyralisAuthoringGraphEdgeKind.Satisfies, "compiles ownership shape");
            AddEdge(edges, "route.shape", "participant.default", PyralisAuthoringGraphEdgeKind.DependsOn, "participants own control");
            if (requiresPawn)
                AddEdge(edges, "route.shape", "pawn.definition", PyralisAuthoringGraphEdgeKind.DependsOn, "pawn-backed control surface");
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

                string nodeId = "scene." + NormalizeId(row.Surface);
                AddNode(nodes, new PyralisAuthoringGraphNode(
                    nodeId,
                    row.Surface,
                PyralisAuthoringGraphNodeKind.SceneSurface,
                PyralisAuthoringGraphSourceKind.SceneReadiness,
                ConvertSceneSurfaceEvidence(row.EvidenceState),
                guidance: row.Current,
                nativeSetup: !string.IsNullOrWhiteSpace(row.NextFix) ? new[] { row.NextFix } : Array.Empty<string>(),
                blockingReason: row.SupportsFirstProofAttempt ? string.Empty : row.NextFix,
                sourceOrigin: PyralisAuthoringGraphSourceOrigin.RuntimeEvidence));
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
                sourceOrigin: PyralisAuthoringGraphSourceOrigin.RuntimeEvidence));
            AddEdge(edges, "bootstrap.root", "scene.surfaces", PyralisAuthoringGraphEdgeKind.RelatesTo, "scene surface summary");
        }

        private static string AddProofNode(
            PyralisSetupRouteAnalysis route,
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges)
        {
            string selectedProofTargetId = ResolveProofTargetId(route);
            PyralisAuthoringFact proofFact = ResolveProofFact(selectedProofTargetId);
            string proofNodeId = proofFact != null && !string.IsNullOrWhiteSpace(proofFact.StableId)
                ? proofFact.StableId
                : selectedProofTargetId;
            if (string.IsNullOrWhiteSpace(proofNodeId))
                proofNodeId = "proof.unresolved-route";
            bool unresolvedProof = string.Equals(proofNodeId, "proof.unresolved-route", StringComparison.Ordinal);
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
                    : PyralisAuthoringGraphSourceOrigin.GrammarFallback));

            RuntimeCapabilityFamily[] families = route?.CapabilityFamilies ?? Array.Empty<RuntimeCapabilityFamily>();
            for (int i = 0; i < families.Length; i++)
            {
                PyralisAuthoringCapabilityDescriptor descriptor = PyralisAuthoringCapabilityDescriptorRegistry.FindPrimaryByFamily(families[i]);
                AddEdge(edges, GetCapabilityNodeId(families[i], descriptor), proofNodeId, PyralisAuthoringGraphEdgeKind.SupportsProof, "supports proof");
            }

            return proofNodeId;
        }

        private static string ResolveProofTargetId(PyralisSetupRouteAnalysis route)
        {
            RuntimeCapabilityFamily[] families = route?.CapabilityFamilies ?? Array.Empty<RuntimeCapabilityFamily>();
            if (families.Length == 0 || route == null || !route.HasSelectedCapabilities)
                return "proof.unresolved-route";

            for (int i = 0; i < families.Length; i++)
            {
                PyralisAuthoringCapabilityDescriptor descriptor = PyralisAuthoringCapabilityDescriptorRegistry.FindPrimaryByFamily(families[i]);
                if (descriptor != null && !string.IsNullOrWhiteSpace(descriptor.ProofTargetId))
                    return descriptor.ProofTargetId;
            }

            string fallbackProofTargetId = PyralisProofFamilyVocabulary.GetFallbackProofTargetId(
                families,
                route.RequiresPawn);
            if (!string.IsNullOrWhiteSpace(fallbackProofTargetId))
                return fallbackProofTargetId;

            return "proof.custom-object-effect";
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
                    sourceOrigin: GetContractSourceOrigin(contract)));

                if (!string.IsNullOrWhiteSpace(contract.FirstProofTargetId)
                    && string.Equals(contract.FirstProofTargetId, activeProofNodeId, StringComparison.Ordinal))
                {
                    AddEdge(edges, nodeId, activeProofNodeId, PyralisAuthoringGraphEdgeKind.Recommends, "proof guidance");
                }

                if (!string.IsNullOrWhiteSpace(contract.SetupNodeId))
                    AddEdge(edges, nodeId, contract.SetupNodeId, PyralisAuthoringGraphEdgeKind.RelatesTo, "setup node");
            }
        }

        private static void AddSetupFlowEvidence(
            UnityEngine.Object source,
            PyralisSetupRouteAnalysis route,
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges)
        {
            if (source is not GameplaySessionBootstrap bootstrap)
                return;

            PyralisSetupFlowReport report = PyralisSetupFlowValidator.BuildReport(bootstrap);
            IReadOnlyList<PyralisSetupFlowStep> steps = report.Steps;
            for (int i = 0; i < steps.Count; i++)
            {
                PyralisSetupFlowStep step = steps[i];
                if (IsSetupFlowStepReplacedByReflection(step, route))
                    continue;

                string setupNodeId = step.StepId != PyralisSetupFlowStepId.Unknown
                    ? PyralisSetupFlowGuidance.GetStableId(step.StepId)
                    : string.Empty;
                string nodeId = BuildSetupFlowEvidenceNodeId(step, setupNodeId);
                bool reflectedContractEvidence = step.StepId == PyralisSetupFlowStepId.Unknown
                    && step.WorkIntent == PyralisSetupFlowWorkIntent.ProofEnhancer;
                AddNode(nodes, new PyralisAuthoringGraphNode(
                    nodeId,
                    step.Label,
                    reflectedContractEvidence ? PyralisAuthoringGraphNodeKind.Contract : PyralisAuthoringGraphNodeKind.ValidationEvidence,
                    reflectedContractEvidence ? PyralisAuthoringGraphSourceKind.AuthoringContract : PyralisAuthoringGraphSourceKind.SetupFlow,
                    ConvertSetupFlowStatus(step.Status),
                    guidance: step.Message,
                    nativeSetup: step.NativeAction.HasValue ? new[] { FormatNativeAction(step.NativeAction.Value) } : Array.Empty<string>(),
                    blockingReason: step.IsRequiredIssue ? step.Message : string.Empty,
                    nativeAction: step.NativeAction,
                    sourceObject: step.ReferencedObject,
                    sourceOrigin: reflectedContractEvidence ? PyralisAuthoringGraphSourceOrigin.Contract : PyralisAuthoringGraphSourceOrigin.RuntimeEvidence,
                    workIntent: ConvertSetupFlowWorkIntent(step.WorkIntent),
                    issueSeverity: ConvertSetupFlowSeverity(step.Status)));
                AddEdge(edges, "bootstrap.root", nodeId, PyralisAuthoringGraphEdgeKind.RelatesTo, "setup evidence");
                AddEdge(edges, setupNodeId, nodeId, PyralisAuthoringGraphEdgeKind.RelatesTo, "setup flow evidence");
            }
        }

        private static bool IsSetupFlowStepReplacedByReflection(
            PyralisSetupFlowStep step,
            PyralisSetupRouteAnalysis route)
        {
            if (step == null || route == null)
                return false;

            switch (step.StepId)
            {
                case PyralisSetupFlowStepId.AssignDefaultGameMode:
                    return route.Session != null;
                case PyralisSetupFlowStepId.AssignDefaultParticipants:
                    return route.Session != null;
                case PyralisSetupFlowStepId.AssignParticipantPawn:
                    return route.RequiresPawn && route.Session != null;
                case PyralisSetupFlowStepId.AssignInputProfile:
                    return route.RequiresPawn && route.Participant != null;
                case PyralisSetupFlowStepId.AssignCameraRig:
                    return (route.UsesPawnGameplay() || route.UsesCamera()) && route.Mode != null;
                case PyralisSetupFlowStepId.AssignPlayfieldProfile:
                    return route.UsesPlayfield() && route.Mode != null;
                default:
                    return false;
            }
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

                    string nodeId = BuildRuntimeValidationNodeId(dependencyNode, issue.Message);
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
                        issueSeverity: ConvertRuntimeValidationIssueSeverity(issue.Severity)));

                    string anchorNodeId = ResolveDependencyAnchorNodeId(dependencyNode);
                    AddEdge(edges, anchorNodeId, nodeId, PyralisAuthoringGraphEdgeKind.RelatesTo, "runtime validation");
                }
            }
        }

        private static PyralisSetupDependencyTree BuildDependencyTree(UnityEngine.Object source, PyralisSetupRouteAnalysis route)
        {
            UnityEngine.Object treeSource = source;
            if (treeSource == null && route != null)
                treeSource = route.Session != null ? route.Session : route.Mode;

            return treeSource != null ? PyralisSetupDependencyTree.Build(treeSource) : null;
        }

        private static string BuildRuntimeValidationNodeId(PyralisSetupDependencyNode dependencyNode, string issue)
        {
            string anchor = dependencyNode != null && !string.IsNullOrWhiteSpace(dependencyNode.StableId)
                ? dependencyNode.StableId
                : "unknown";
            return "runtimevalidation." + NormalizeId(anchor) + "." + ComputeStableHash(issue);
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
                    issueSeverity: ConvertSceneReadinessIssueSeverity(issue.Severity)));
                AddEdge(edges, "bootstrap.root", nodeId, PyralisAuthoringGraphEdgeKind.RelatesTo, "scene readiness");
            }
        }

        private static string BuildSetupFlowEvidenceNodeId(PyralisSetupFlowStep step, string setupNodeId)
        {
            if (!string.IsNullOrWhiteSpace(setupNodeId))
                return "setupflow." + NormalizeId(setupNodeId);

            string label = step != null ? step.Label : string.Empty;
            return "setupflow." + NormalizeId(label);
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

        private static PyralisAuthoringGraphEvidenceState ConvertSetupFlowStatus(PyralisSetupFlowStepStatus status)
        {
            return status switch
            {
                PyralisSetupFlowStepStatus.Ready => PyralisAuthoringGraphEvidenceState.Ready,
                PyralisSetupFlowStepStatus.Recommended => PyralisAuthoringGraphEvidenceState.CandidateDetected,
                PyralisSetupFlowStepStatus.Optional => PyralisAuthoringGraphEvidenceState.Optional,
                PyralisSetupFlowStepStatus.Missing => PyralisAuthoringGraphEvidenceState.Missing,
                PyralisSetupFlowStepStatus.Blocked => PyralisAuthoringGraphEvidenceState.Blocked,
                _ => PyralisAuthoringGraphEvidenceState.Unknown
            };
        }

        private static PyralisAuthoringGraphWorkIntent ConvertSetupFlowWorkIntent(PyralisSetupFlowWorkIntent workIntent)
        {
            return workIntent switch
            {
                PyralisSetupFlowWorkIntent.Foundation => PyralisAuthoringGraphWorkIntent.RequiredSetup,
                PyralisSetupFlowWorkIntent.RequiredSetup => PyralisAuthoringGraphWorkIntent.RequiredSetup,
                PyralisSetupFlowWorkIntent.ProofEnhancer => PyralisAuthoringGraphWorkIntent.ProofEnhancer,
                PyralisSetupFlowWorkIntent.FeatureCard => PyralisAuthoringGraphWorkIntent.FeatureCard,
                _ => PyralisAuthoringGraphWorkIntent.Unknown
            };
        }

        private static PyralisAuthoringIssueSeverity ConvertSetupFlowSeverity(PyralisSetupFlowStepStatus status)
        {
            return status switch
            {
                PyralisSetupFlowStepStatus.Blocked => PyralisAuthoringIssueSeverity.Blocked,
                PyralisSetupFlowStepStatus.Missing => PyralisAuthoringIssueSeverity.Required,
                PyralisSetupFlowStepStatus.Recommended => PyralisAuthoringIssueSeverity.Recommended,
                PyralisSetupFlowStepStatus.Optional => PyralisAuthoringIssueSeverity.Optional,
                PyralisSetupFlowStepStatus.Ready => PyralisAuthoringIssueSeverity.Info,
                _ => PyralisAuthoringIssueSeverity.Info
            };
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
                return PyralisAuthoringGraphSourceKind.SetupFlow;

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
