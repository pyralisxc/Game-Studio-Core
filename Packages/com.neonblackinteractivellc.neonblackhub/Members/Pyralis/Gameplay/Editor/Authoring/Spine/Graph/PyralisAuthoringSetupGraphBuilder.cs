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
            PyralisSetupRouteAnalysis route = BuildRoute(source);
            List<PyralisAuthoringGraphNode> nodes = new List<PyralisAuthoringGraphNode>();
            List<PyralisAuthoringGraphEdge> edges = new List<PyralisAuthoringGraphEdge>();

            AddSetupChainNodes(source, route, nodes, edges);
            AddCapabilityNodes(route, nodes, edges);
            AddRuntimePatternNodes(route, nodes, edges);
            AddParticipantNodes(route, nodes, edges);
            AddSceneSurfaceNodes(source, nodes, edges);
            string activeProofNodeId = AddProofNode(route, nodes, edges);
            AddContractNodes(nodes, edges, activeProofNodeId);
            AddSetupFlowEvidence(source, nodes, edges);
            AddSceneReadinessEvidence(source, nodes, edges);
            AddProofBlockerEdges(nodes, edges, activeProofNodeId);

            return new PyralisAuthoringSetupGraph(source, route, nodes, edges);
        }

        private static PyralisSetupRouteAnalysis BuildRoute(UnityEngine.Object source)
        {
            if (source is GameplaySessionBootstrap bootstrap)
                return PyralisSetupRouteAnalysis.Build(bootstrap);
            if (source is SessionDefinition session)
                return PyralisSetupRouteAnalysis.Build(session);
            if (source is GameModeDefinition mode)
                return PyralisSetupRouteAnalysis.Build(mode);
            if (source is GameSetupProfile setupProfile)
                return PyralisSetupRouteAnalysis.Build(setupProfile);

            return PyralisSetupRouteAnalysis.Build((GameSetupProfile)null);
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
                sourceObject: route?.Mode,
                sourceOrigin: route != null && route.Mode != null
                    ? PyralisAuthoringGraphSourceOrigin.UserAuthoredSetup
                    : PyralisAuthoringGraphSourceOrigin.SpineGrammar));

            AddNode(nodes, new PyralisAuthoringGraphNode(
                "setup.profile",
                "Game Setup Profile",
                PyralisAuthoringGraphNodeKind.SetupChain,
                PyralisAuthoringGraphSourceKind.SetupProfile,
                route != null && route.SetupProfile != null ? PyralisAuthoringGraphEvidenceState.Ready : PyralisAuthoringGraphEvidenceState.Missing,
                sourceObject: route?.SetupProfile,
                sourceOrigin: route != null && route.SetupProfile != null
                    ? PyralisAuthoringGraphSourceOrigin.UserAuthoredSetup
                    : PyralisAuthoringGraphSourceOrigin.SpineGrammar));

            AddEdge(edges, "bootstrap.root", "session.definition", PyralisAuthoringGraphEdgeKind.DependsOn, "reads");
            AddEdge(edges, "session.definition", "mode.definition", PyralisAuthoringGraphEdgeKind.DependsOn, "default mode");
            AddEdge(edges, "mode.definition", "setup.profile", PyralisAuthoringGraphEdgeKind.DependsOn, "setup profile");
        }

        private static void AddParticipantNodes(
            PyralisSetupRouteAnalysis route,
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges)
        {
            SessionDefinition session = route?.Session;
            ParticipantDefinition participant = GetFirstParticipant(session);
            PawnDefinition pawn = participant != null ? participant.defaultPawn : null;
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
                assignmentFields: new[] { "SessionDefinition.defaultParticipants" },
                sourceObject: participant != null ? participant : session,
                sourceOrigin: participant != null || session != null
                    ? PyralisAuthoringGraphSourceOrigin.UserAuthoredSetup
                    : PyralisAuthoringGraphSourceOrigin.SpineGrammar));

            AddNode(nodes, new PyralisAuthoringGraphNode(
                "pawn.definition",
                requiresPawn ? "Pawn Definition" : "Pawn / No Pawn",
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
                route != null && route.SetupProfile != null ? PyralisAuthoringGraphSourceKind.SetupProfile : PyralisAuthoringGraphSourceKind.Unknown,
                hasCapabilities ? PyralisAuthoringGraphEvidenceState.Ready : PyralisAuthoringGraphEvidenceState.Missing,
                guidance: GetCapabilitySummaryGuidance(route, hasCapabilities),
                sourceObject: route?.SetupProfile,
                sourceOrigin: route != null && route.SetupProfile != null
                    ? PyralisAuthoringGraphSourceOrigin.UserAuthoredSetup
                    : PyralisAuthoringGraphSourceOrigin.SpineGrammar));
            AddEdge(edges, "setup.profile", "capability.selected", PyralisAuthoringGraphEdgeKind.Satisfies, "selected capabilities");

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

                AddEdge(edges, "setup.profile", nodeId, PyralisAuthoringGraphEdgeKind.Satisfies, "selected capability");
                AddEdge(edges, "capability.selected", nodeId, PyralisAuthoringGraphEdgeKind.RelatesTo, "includes");
            }
        }

        private static PyralisAuthoringGraphSourceKind GetCapabilitySourceKind(PyralisAuthoringCapabilityDescriptor descriptor)
        {
            if (descriptor == null)
                return PyralisAuthoringGraphSourceKind.SetupProfile;

            return descriptor.SourceOrigin == PyralisAuthoringGraphSourceOrigin.Contract
                || descriptor.SourceOrigin == PyralisAuthoringGraphSourceOrigin.Reflection
                    ? PyralisAuthoringGraphSourceKind.AuthoringContract
                    : PyralisAuthoringGraphSourceKind.CapabilityVocabulary;
        }

        private static void AddRuntimePatternNodes(
            PyralisSetupRouteAnalysis route,
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges)
        {
            RuntimePatternDefinition[] patterns = route?.Patterns ?? Array.Empty<RuntimePatternDefinition>();
            for (int i = 0; i < patterns.Length; i++)
            {
                RuntimePatternDefinition pattern = patterns[i];
                if (pattern == null)
                    continue;

                string nodeId = "runtime-pattern." + NormalizeId(!string.IsNullOrWhiteSpace(pattern.patternId) ? pattern.patternId : pattern.name);
                string guidance = !string.IsNullOrWhiteSpace(pattern.setupNotes)
                    ? pattern.setupNotes
                    : pattern.description;
                AddNode(nodes, new PyralisAuthoringGraphNode(
                    nodeId,
                    !string.IsNullOrWhiteSpace(pattern.displayName) ? pattern.displayName : pattern.name,
                    PyralisAuthoringGraphNodeKind.Capability,
                    PyralisAuthoringGraphSourceKind.RuntimePattern,
                    PyralisAuthoringGraphEvidenceState.Ready,
                    pattern.capabilityFamily,
                    guidance: guidance,
                    nativeSetup: pattern.requiredRuntimeSystems ?? Array.Empty<string>(),
                    customizationMoments: pattern.optionalRuntimeSystems ?? Array.Empty<string>(),
                    sourceObject: pattern,
                    sourceOrigin: PyralisAuthoringGraphSourceOrigin.UserAuthoredSetup));

                AddEdge(edges, "setup.profile", nodeId, PyralisAuthoringGraphEdgeKind.Satisfies, "runtime pattern");
                AddEdge(edges, "capability.selected", nodeId, PyralisAuthoringGraphEdgeKind.RelatesTo, "advanced route metadata");
            }
        }

        private static void AddSceneSurfaceNodes(
            UnityEngine.Object source,
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges)
        {
            PyralisAuthoringSceneSurfaceSnapshot snapshot = PyralisAuthoringSceneSurfaceSnapshot.Build(source);
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
            AddNode(nodes, new PyralisAuthoringGraphNode(
                proofNodeId,
                !string.IsNullOrWhiteSpace(proofFact?.DisplayName) ? proofFact.DisplayName : "Unresolved Route Proof",
                PyralisAuthoringGraphNodeKind.Proof,
                proofFact != null ? PyralisAuthoringGraphSourceKind.ProofVocabulary : PyralisAuthoringGraphSourceKind.SetupFlow,
                PyralisAuthoringGraphEvidenceState.Unknown,
                proofTargetId: proofNodeId,
                guidance: GetProofGuidance(proofFact),
                nativeSetup: GetProofNativeSetup(proofFact),
                assignmentFields: proofFact != null ? proofFact.AssignmentFields : Array.Empty<string>(),
                customizationMoments: proofFact != null ? proofFact.CustomizationMoments : Array.Empty<string>(),
                blockingReason: proofFact != null ? proofFact.FirstProof : string.Empty,
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
            for (int i = 0; i < families.Length; i++)
            {
                PyralisAuthoringCapabilityDescriptor descriptor = PyralisAuthoringCapabilityDescriptorRegistry.FindPrimaryByFamily(families[i]);
                if (descriptor != null && !string.IsNullOrWhiteSpace(descriptor.ProofTargetId))
                    return descriptor.ProofTargetId;
            }

            string fallbackProofTargetId = PyralisProofFamilyVocabulary.GetFallbackProofTargetId(
                families,
                route != null && route.RequiresPawn);
            return string.IsNullOrWhiteSpace(fallbackProofTargetId)
                ? "proof.custom-object-effect"
                : fallbackProofTargetId;
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
                string[] actions = new string[proofFact.NativeActions.Length];
                for (int i = 0; i < proofFact.NativeActions.Length; i++)
                    actions[i] = proofFact.NativeActions[i].ToGuidanceSentence();

                return actions;
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

        private static void AddSetupFlowEvidence(UnityEngine.Object source, List<PyralisAuthoringGraphNode> nodes, List<PyralisAuthoringGraphEdge> edges)
        {
            if (source is not GameplaySessionBootstrap bootstrap)
                return;

            PyralisSetupFlowReport report = PyralisSetupFlowValidator.BuildReport(bootstrap);
            IReadOnlyList<PyralisSetupFlowStep> steps = report.Steps;
            for (int i = 0; i < steps.Count; i++)
            {
                PyralisSetupFlowStep step = steps[i];
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
                || node.Kind == PyralisAuthoringGraphNodeKind.UnitySurfaceRequirement
                || node.Kind == PyralisAuthoringGraphNodeKind.ValidationEvidence;
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

        private static string GetCapabilitySummaryGuidance(PyralisSetupRouteAnalysis route, bool hasCapabilities)
        {
            if (route == null || route.SetupProfile == null)
                return "Create or assign the setup profile before choosing capabilities.";

            if (!hasCapabilities || !route.HasAssignedPatterns)
                return "Choose capability ingredients before scene wiring.";

            if (!route.HasValidPatterns)
                return "Fix setup capability validation before trusting route guidance.";

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

            IReadOnlyList<PyralisAuthoringFact> proofFacts =
                PyralisProofFamilyVocabulary.GetAuthoringFacts();
            for (int i = 0; i < proofFacts.Count; i++)
            {
                PyralisAuthoringFact fact = proofFacts[i];
                if (fact != null && string.Equals(fact.StableId, proofTargetId, StringComparison.Ordinal))
                    return fact;
            }

            return null;
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
