using System;
using System.Collections.Generic;
using System.Linq;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Editor.Inspectors;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    public sealed class PyralisAuthoringSetupGraphRow
    {
        public PyralisAuthoringSetupGraphRow(
            string label,
            PyralisAuthoringGraphNode node,
            bool isOptional = false,
            string fallbackMessage = null,
            Object fallbackTarget = null)
        {
            Label = label ?? string.Empty;
            Node = node;
            IsOptional = isOptional;
            FallbackMessage = fallbackMessage ?? string.Empty;
            FallbackTarget = fallbackTarget;
        }

        public string Label { get; }
        public PyralisAuthoringGraphNode Node { get; }
        public bool IsOptional { get; }
        public string FallbackMessage { get; }
        public Object FallbackTarget { get; }
        public Object Target => Node != null && Node.SourceObject != null ? Node.SourceObject : FallbackTarget;
        public string Message => Node != null && !string.IsNullOrWhiteSpace(Node.Guidance) ? Node.Guidance : FallbackMessage;
        public bool IsReady => Node != null && (Node.EvidenceState == PyralisAuthoringGraphEvidenceState.Ready || Node.EvidenceState == PyralisAuthoringGraphEvidenceState.Optional);
        public bool IsMissing => Node != null && (Node.EvidenceState == PyralisAuthoringGraphEvidenceState.Missing || Node.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked);
    }

    public sealed class PyralisAuthoringValidationGraphRow
    {
        public PyralisAuthoringValidationGraphRow(PyralisAuthoringGraphNode node)
        {
            Node = node;
        }

        public PyralisAuthoringGraphNode Node { get; }
        public string NodeId => Node != null ? Node.StableId : string.Empty;
        public string Label => Node != null ? Node.Label : string.Empty;
        public string Message => Node != null ? Node.Guidance : string.Empty;
        public string NativeAction => Node != null && Node.NativeSetup.Length > 0 ? Node.NativeSetup[0] : string.Empty;
        public Object Target => Node != null ? Node.SourceObject : null;
        public bool CanInspectTarget => Target != null;
        public string SourceLabel => Node != null ? FormatSourceKind(Node.SourceKind) : string.Empty;
        public string OriginLabel => Node != null ? FormatSourceOrigin(Node.SourceOrigin) : string.Empty;
        public PyralisAuthoringGraphEvidenceState EvidenceState => Node != null ? Node.EvidenceState : PyralisAuthoringGraphEvidenceState.Unknown;

        private static string FormatSourceKind(PyralisAuthoringGraphSourceKind sourceKind)
        {
            return sourceKind switch
            {
                PyralisAuthoringGraphSourceKind.SetupProfile => "Setup Profile",
                PyralisAuthoringGraphSourceKind.CapabilityVocabulary => "Capability Vocabulary",
                PyralisAuthoringGraphSourceKind.RuntimePattern => "Runtime Pattern",
                PyralisAuthoringGraphSourceKind.AuthoringContract => "Authoring Contract",
                PyralisAuthoringGraphSourceKind.GrammarRegistry => "Grammar Registry",
                PyralisAuthoringGraphSourceKind.SetupFlow => "Setup Flow",
                PyralisAuthoringGraphSourceKind.SceneReadiness => "Scene Readiness",
                PyralisAuthoringGraphSourceKind.ProofVocabulary => "Proof Vocabulary",
                _ => "Graph"
            };
        }

        private static string FormatSourceOrigin(PyralisAuthoringGraphSourceOrigin sourceOrigin)
        {
            return sourceOrigin switch
            {
                PyralisAuthoringGraphSourceOrigin.UserAuthoredSetup => "User Authored Setup",
                PyralisAuthoringGraphSourceOrigin.Reflection => "Reflection",
                PyralisAuthoringGraphSourceOrigin.Contract => "Contract",
                PyralisAuthoringGraphSourceOrigin.RuntimeEvidence => "Runtime Evidence",
                PyralisAuthoringGraphSourceOrigin.SpineGrammar => "Spine Grammar",
                PyralisAuthoringGraphSourceOrigin.GrammarFallback => "Grammar Fallback",
                _ => "Unknown"
            };
        }
    }

    public sealed class PyralisAuthoringValidationGraphSection
    {
        public PyralisAuthoringValidationGraphSection(
            string label,
            PyralisAuthoringGraphEvidenceState evidenceState,
            IReadOnlyList<PyralisAuthoringValidationGraphRow> rows)
        {
            Label = label ?? string.Empty;
            EvidenceState = evidenceState;
            Rows = rows ?? Array.Empty<PyralisAuthoringValidationGraphRow>();
        }

        public string Label { get; }
        public PyralisAuthoringGraphEvidenceState EvidenceState { get; }
        public IReadOnlyList<PyralisAuthoringValidationGraphRow> Rows { get; }
        public bool HasRows => Rows.Count > 0;
    }

    public sealed class PyralisAuthoringGraphConnectionRow
    {
        public PyralisAuthoringGraphConnectionRow(
            PyralisAuthoringGraphNode from,
            PyralisAuthoringGraphNode to,
            PyralisAuthoringGraphEdge edge)
        {
            From = from;
            To = to;
            Edge = edge;
        }

        public PyralisAuthoringGraphNode From { get; }
        public PyralisAuthoringGraphNode To { get; }
        public PyralisAuthoringGraphEdge Edge { get; }
        public string FromLabel => From != null ? From.Label : Edge != null ? Edge.FromNodeId : string.Empty;
        public string ToLabel => To != null ? To.Label : Edge != null ? Edge.ToNodeId : string.Empty;
        public string Relationship => Edge != null ? FormatEdgeKind(Edge.Kind) : string.Empty;
        public string Detail => Edge != null ? Edge.Label : string.Empty;
        public string FromOrigin => From != null ? From.SourceOrigin.ToString() : string.Empty;
        public string ToOrigin => To != null ? To.SourceOrigin.ToString() : string.Empty;

        private static string FormatEdgeKind(PyralisAuthoringGraphEdgeKind kind)
        {
            return kind switch
            {
                PyralisAuthoringGraphEdgeKind.DependsOn => "depends on",
                PyralisAuthoringGraphEdgeKind.Satisfies => "satisfies",
                PyralisAuthoringGraphEdgeKind.Recommends => "recommends",
                PyralisAuthoringGraphEdgeKind.SupportsProof => "supports proof",
                PyralisAuthoringGraphEdgeKind.BlockedBy => "blocked by",
                _ => "relates to"
            };
        }
    }

    public sealed class PyralisAuthoringReflectiveContractGraphRow
    {
        public PyralisAuthoringReflectiveContractGraphRow(PyralisAuthoringGraphNode node)
        {
            Node = node;
        }

        public PyralisAuthoringGraphNode Node { get; }
        public string Label => Node != null ? Node.Label : string.Empty;
        public string Message => Node != null ? Node.Guidance : string.Empty;
        public Object Target => Node != null ? Node.SourceObject : null;
        public ResolvedAuthoringContract Contract => Node != null ? Node.SourceContract : null;
        public PyralisAuthoringGraphEvidenceState EvidenceState => Node != null ? Node.EvidenceState : PyralisAuthoringGraphEvidenceState.Unknown;
    }

    public sealed class PyralisAuthoringGuideGraphRow
    {
        public PyralisAuthoringGuideGraphRow(
            PyralisAuthoringGraphNode node,
            PyralisAuthoringIntentGuideTier tier,
            PyralisAuthoringIntentRowState state,
            string reason)
        {
            Node = node;
            Tier = tier;
            State = state;
            Reason = reason ?? string.Empty;
        }

        public PyralisAuthoringGraphNode Node { get; }
        public string StableId => Node != null ? Node.StableId : string.Empty;
        public string Label => Node != null ? Node.Label : string.Empty;
        public string Message => Node != null ? Node.Guidance : string.Empty;
        public string FirstProof => Node != null && !string.IsNullOrWhiteSpace(Node.BlockingReason) ? Node.BlockingReason : Message;
        public string[] NativeSetup => Node != null ? Node.NativeSetup : Array.Empty<string>();
        public string[] AssignmentFields => Node != null ? Node.AssignmentFields : Array.Empty<string>();
        public string[] CustomizationMoments => Node != null ? Node.CustomizationMoments : Array.Empty<string>();
        public PyralisAuthoringIntentGuideTier Tier { get; }
        public PyralisAuthoringIntentRowState State { get; }
        public string Reason { get; }
        public PyralisAuthoringGraphEvidenceState EvidenceState => Node != null ? Node.EvidenceState : PyralisAuthoringGraphEvidenceState.Unknown;
        public PyralisAuthoringGraphSourceOrigin SourceOrigin => Node != null ? Node.SourceOrigin : PyralisAuthoringGraphSourceOrigin.Unknown;
    }

    public sealed class PyralisAuthoringSelectedContextGraphRow
    {
        public PyralisAuthoringSelectedContextGraphRow(
            Object selection,
            PyralisAuthoringGraphNode node,
            string role,
            string nextCheck,
            IReadOnlyList<PyralisAuthoringSelectedContextDetail> details = null,
            string copyGuidance = null,
            bool hasMissingRuntimePatternText = false)
        {
            Selection = selection;
            Node = node;
            Role = role ?? string.Empty;
            NextCheck = nextCheck ?? string.Empty;
            Details = details ?? Array.Empty<PyralisAuthoringSelectedContextDetail>();
            CopyGuidance = copyGuidance ?? string.Empty;
            HasMissingRuntimePatternText = hasMissingRuntimePatternText;
        }

        public Object Selection { get; }
        public PyralisAuthoringGraphNode Node { get; }
        public string NodeId => Node != null ? Node.StableId : string.Empty;
        public string Label => Node != null ? Node.Label : Selection != null ? Selection.GetType().Name : "No Selection";
        public string Role { get; }
        public string NextCheck { get; }
        public string RuntimeMeaning => Node != null && !string.IsNullOrWhiteSpace(Node.Guidance) ? Node.Guidance : Role;
        public string NativeSetup => Node != null && Node.NativeSetup.Length > 0 ? string.Join("; ", Node.NativeSetup) : string.Empty;
        public PyralisAuthoringGraphEvidenceState EvidenceState => Node != null ? Node.EvidenceState : PyralisAuthoringGraphEvidenceState.Unknown;
        public IReadOnlyList<PyralisAuthoringSelectedContextDetail> Details { get; }
        public string CopyGuidance { get; }
        public bool HasMissingRuntimePatternText { get; }
    }

    public sealed class PyralisAuthoringSelectedContextDetail
    {
        public PyralisAuthoringSelectedContextDetail(string label, string value, Object target = null)
        {
            Label = label ?? string.Empty;
            Value = value ?? string.Empty;
            Target = target;
        }

        public string Label { get; }
        public string Value { get; }
        public Object Target { get; }
        public bool CanSelectTarget => Target != null;
    }

    public sealed class PyralisAuthoringCurrentStepGraphRow
    {
        public PyralisAuthoringCurrentStepGraphRow(
            string routeName,
            PyralisAuthoringGraphNode node,
            string message,
            string detail,
            Object target)
        {
            RouteName = routeName ?? string.Empty;
            Node = node;
            Message = message ?? string.Empty;
            Detail = detail ?? string.Empty;
            Target = target;
        }

        public string RouteName { get; }
        public PyralisAuthoringGraphNode Node { get; }
        public string Label => Node != null ? Node.Label : "Create Setup Foundation";
        public string Message { get; }
        public string Detail { get; }
        public Object Target { get; }
        public PyralisAuthoringNativeAction? NativeAction => Node?.NativeAction;
        public PyralisAuthoringGraphEvidenceState EvidenceState => Node != null ? Node.EvidenceState : PyralisAuthoringGraphEvidenceState.Missing;
        public bool HasNode => Node != null;
    }

    public static class PyralisAuthoringSetupGraphProjection
    {
        public static IReadOnlyList<PyralisAuthoringSetupGraphRow> BuildSetupMapRows(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringSetupGraphRow>();

            List<PyralisAuthoringSetupGraphRow> rows = new List<PyralisAuthoringSetupGraphRow>
            {
                Row(graph, "Scene Root", "bootstrap.root", "Scene object that starts the session."),
                Row(graph, "Session", "session.definition", "Asset that names game rules and participants."),
                Row(graph, "Game Rules", "mode.definition", "Ruleset that chooses the setup profile."),
                Row(graph, "Setup Profile", "setup.profile", "Editable capability contract for this route."),
                BuildCapabilitiesRow(graph),
                Row(graph, "Participants", "participant.default", "Assign at least one default participant."),
                Row(graph, "Pawn / No Pawn", "pawn.definition", "Pawn-backed routes need a ParticipantDefinition.defaultPawn.", isOptional: IsNodeOptional(graph, "pawn.definition")),
                Row(graph, "Scene Surfaces", "scene.surfaces", "Route-recommended scene surface evidence is present or not needed yet.")
            };

            return rows;
        }

        public static IReadOnlyList<PyralisAuthoringSetupGraphRow> BuildReadinessRows(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringSetupGraphRow>();

            return new[]
            {
                Row(graph, "Scene Root", "bootstrap.root"),
                Row(graph, "Session", "session.definition"),
                Row(graph, "Game Rules", "mode.definition"),
                Row(graph, "Setup Profile", "setup.profile"),
                BuildCapabilitiesRow(graph),
                Row(graph, "Players / Seats", "participant.default"),
                Row(graph, "Pawn / No Pawn", "pawn.definition", isOptional: IsNodeOptional(graph, "pawn.definition")),
                Row(graph, "Scene Roots", "scene.surfaces", isOptional: true)
            };
        }

        private static bool IsNodeOptional(PyralisAuthoringSetupGraph graph, string nodeId)
        {
            return graph == null
                || !graph.TryFindNode(nodeId, out PyralisAuthoringGraphNode node)
                || node == null
                || node.EvidenceState == PyralisAuthoringGraphEvidenceState.Optional
                || node.WorkIntent == PyralisAuthoringGraphWorkIntent.Optional;
        }

        public static IReadOnlyList<PyralisAuthoringGraphNode> FindSceneSurfaceNodes(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringGraphNode>();

            return graph.FindNodes(PyralisAuthoringGraphNodeKind.SceneSurface)
                .Where(node => node != null && !string.Equals(node.StableId, "scene.surfaces", StringComparison.Ordinal))
                .ToArray();
        }

        public static IReadOnlyList<PyralisAuthoringGraphConnectionRow> BuildMapConnectionRows(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringGraphConnectionRow>();

            List<PyralisAuthoringGraphConnectionRow> rows = new List<PyralisAuthoringGraphConnectionRow>();
            for (int i = 0; i < graph.Edges.Count; i++)
            {
                PyralisAuthoringGraphEdge edge = graph.Edges[i];
                if (edge == null)
                    continue;

                if (!graph.TryFindNode(edge.FromNodeId, out PyralisAuthoringGraphNode from)
                    || !graph.TryFindNode(edge.ToNodeId, out PyralisAuthoringGraphNode to))
                {
                    continue;
                }

                rows.Add(new PyralisAuthoringGraphConnectionRow(from, to, edge));
            }

            return rows.ToArray();
        }

        public static IReadOnlyList<PyralisAuthoringGraphConnectionRow> BuildProofSupportRows(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringGraphConnectionRow>();

            List<PyralisAuthoringGraphConnectionRow> rows = new List<PyralisAuthoringGraphConnectionRow>();
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> connections = BuildMapConnectionRows(graph);
            for (int i = 0; i < connections.Count; i++)
            {
                PyralisAuthoringGraphConnectionRow row = connections[i];
                if (row == null || row.Edge == null)
                    continue;

                if (row.Edge.Kind == PyralisAuthoringGraphEdgeKind.SupportsProof
                    || row.Edge.Kind == PyralisAuthoringGraphEdgeKind.Recommends)
                {
                    if (row.To != null && row.To.Kind == PyralisAuthoringGraphNodeKind.Proof)
                        rows.Add(row);
                }
                else if (row.Edge.Kind == PyralisAuthoringGraphEdgeKind.BlockedBy)
                {
                    if (row.From != null && row.From.Kind == PyralisAuthoringGraphNodeKind.Proof)
                        rows.Add(row);
                }
            }

            return rows.ToArray();
        }

        public static IReadOnlyList<PyralisAuthoringFact> BuildCookbookFacts(PyralisAuthoringSetupGraph graph)
        {
            return PyralisAuthoringGrammarRegistry.AllFacts;
        }

        public static IReadOnlyList<PyralisAuthoringFact> BuildRuntimeCapabilityFactsForCapability(
            PyralisAuthoringSetupGraph graph,
            AuthoringCapability capability)
        {
            return PyralisAuthoringCapabilityDescriptorRegistry.BuildFactsForCapability(capability);
        }

        public static IReadOnlyList<PyralisAuthoringFact> BuildRuntimeCapabilityFactsForLane(
            PyralisAuthoringSetupGraph graph,
            RuntimeCapabilityLaneTag laneTag)
        {
            return PyralisAuthoringCapabilityDescriptorRegistry.BuildFactsForLane(laneTag);
        }

        public static PyralisAuthoringIntentModel BuildIntentModel(PyralisAuthoringIntentSelection selection)
        {
            return PyralisAuthoringIntentAdvisor.Build(selection);
        }

        public static IReadOnlyList<PyralisAuthoringGuideGraphRow> BuildCurrentIntentGuideRows(PyralisAuthoringSetupGraph graph)
        {
            if (!HasResolvedSetupContext(graph))
                return Array.Empty<PyralisAuthoringGuideGraphRow>();

            List<PyralisAuthoringGuideGraphRow> rows = new List<PyralisAuthoringGuideGraphRow>();
            HashSet<string> added = new HashSet<string>(StringComparer.Ordinal);

            PyralisAuthoringGraphNode unresolved = FindFirstUnresolvedSetupFlowNode(graph)
                ?? FindFirstUnresolvedNode(graph);
            AddGuideRow(
                rows,
                added,
                unresolved,
                PyralisAuthoringIntentGuideTier.Primary,
                PyralisAuthoringIntentRowState.Recommended,
                "This is the first unresolved graph node for the active route.");

            PyralisAuthoringGraphNode proof = FindCurrentProofNode(graph);
            AddGuideRow(
                rows,
                added,
                proof,
                unresolved == null ? PyralisAuthoringIntentGuideTier.Primary : PyralisAuthoringIntentGuideTier.SuggestedNext,
                PyralisAuthoringIntentRowState.Related,
                "This is the active proof target compiled from the selected setup graph.");

            IReadOnlyList<PyralisAuthoringGraphConnectionRow> proofRows = BuildProofSupportRows(graph);
            for (int i = 0; i < proofRows.Count; i++)
            {
                PyralisAuthoringGraphConnectionRow proofRow = proofRows[i];
                if (proofRow == null || proofRow.Edge == null)
                    continue;

                if (proofRow.Edge.Kind == PyralisAuthoringGraphEdgeKind.BlockedBy)
                {
                    AddGuideRow(
                        rows,
                        added,
                        proofRow.To,
                        PyralisAuthoringIntentGuideTier.Primary,
                        PyralisAuthoringIntentRowState.Recommended,
                        "This graph evidence blocks the active proof until it is cleared.");
                    continue;
                }

                AddGuideRow(
                    rows,
                    added,
                    proofRow.From,
                    proofRow.Edge.Kind == PyralisAuthoringGraphEdgeKind.Recommends
                        ? PyralisAuthoringIntentGuideTier.SuggestedNext
                        : PyralisAuthoringIntentGuideTier.Primary,
                    PyralisAuthoringIntentRowState.Related,
                    proofRow.Edge.Kind == PyralisAuthoringGraphEdgeKind.Recommends
                        ? "This reflected contract contributes guidance to the active proof."
                        : "This selected capability supports the active proof.");
            }

            IReadOnlyList<PyralisAuthoringGraphNode> capabilityNodes = graph.FindNodes(PyralisAuthoringGraphNodeKind.Capability);
            for (int i = 0; i < capabilityNodes.Count; i++)
            {
                PyralisAuthoringGraphNode node = capabilityNodes[i];
                if (node == null || string.Equals(node.StableId, "capability.selected", StringComparison.Ordinal))
                    continue;

                AddGuideRow(
                    rows,
                    added,
                    node,
                    PyralisAuthoringIntentGuideTier.SuggestedNext,
                    PyralisAuthoringIntentRowState.Related,
                    "This selected capability is part of the active setup graph.");
            }

            IReadOnlyList<PyralisAuthoringGraphNode> contractNodes = graph.FindNodes(PyralisAuthoringGraphNodeKind.Contract);
            for (int i = 0; i < contractNodes.Count; i++)
            {
                PyralisAuthoringGraphNode node = contractNodes[i];
                if (node == null || node.SourceOrigin != PyralisAuthoringGraphSourceOrigin.Contract)
                    continue;

                if (string.IsNullOrWhiteSpace(node.ProofTargetId)
                    || proof == null
                    || !string.Equals(node.ProofTargetId, proof.StableId, StringComparison.Ordinal))
                {
                    continue;
                }

                AddGuideRow(
                    rows,
                    added,
                    node,
                    PyralisAuthoringIntentGuideTier.SuggestedNext,
                    PyralisAuthoringIntentRowState.Related,
                    "This reflected contract matches the active proof target.");
            }

            return rows.ToArray();
        }

        public static PyralisAuthoringGraphNode FindCurrentProofNode(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return null;

            IReadOnlyList<PyralisAuthoringGraphNode> proofNodes = graph.FindNodes(PyralisAuthoringGraphNodeKind.Proof);
            return proofNodes.Count > 0 ? proofNodes[0] : null;
        }

        public static PyralisAuthoringGraphNode FindFirstUnresolvedNode(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return null;

            PyralisAuthoringGraphNode node = FindFirstUnresolvedNode(graph, PyralisAuthoringGraphNodeKind.SetupChain);
            if (node != null)
                return node;

            node = FindFirstUnresolvedNode(graph, PyralisAuthoringGraphNodeKind.UnitySurfaceRequirement);
            if (node != null)
                return node;

            node = FindFirstUnresolvedNode(graph, PyralisAuthoringGraphNodeKind.ValidationEvidence);
            if (node != null)
                return node;

            return FindFirstUnresolvedNode(graph, PyralisAuthoringGraphNodeKind.SceneSurface);
        }

        public static PyralisAuthoringCurrentStepGraphRow BuildCurrentStepRow(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null || graph.Source == null)
                return BuildFallbackCurrentStepRow();

            PyralisAuthoringGraphNode node = FindFirstUnresolvedSetupFlowNode(graph)
                ?? FindFirstUnresolvedNode(graph);

            if (node != null)
            {
                string message = !string.IsNullOrWhiteSpace(node.Guidance)
                    ? node.Guidance
                    : !string.IsNullOrWhiteSpace(node.BlockingReason)
                        ? node.BlockingReason
                        : "Inspect this graph node and clear its missing setup evidence.";

                string detail = GetCurrentStepDetail(node);
                string routeName = GetCurrentRouteName(graph);
                return new PyralisAuthoringCurrentStepGraphRow(routeName, node, message, detail, node.SourceObject);
            }

            return BuildFallbackCurrentStepRow();
        }

        public static int CountNodes(PyralisAuthoringSetupGraph graph, PyralisAuthoringGraphEvidenceState evidenceState)
        {
            if (graph == null)
                return 0;

            return graph.Nodes.Count(node => node != null && node.EvidenceState == evidenceState);
        }

        public static IReadOnlyList<PyralisAuthoringValidationGraphRow> BuildValidationRows(PyralisAuthoringSetupGraph graph, PyralisAuthoringGraphEvidenceState evidenceState)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringValidationGraphRow>();

            return graph.Nodes
                .Where(node => node != null
                    && IsReadinessNode(node)
                    && node.EvidenceState == evidenceState)
                .Select(node => new PyralisAuthoringValidationGraphRow(node))
                .ToArray();
        }

        public static IReadOnlyList<PyralisAuthoringValidationGraphSection> BuildValidationSections(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringValidationGraphSection>();

            return new[]
            {
                new PyralisAuthoringValidationGraphSection(
                    "Required Before Play",
                    PyralisAuthoringGraphEvidenceState.Blocked,
                    BuildValidationRows(graph, PyralisAuthoringGraphEvidenceState.Blocked)),
                new PyralisAuthoringValidationGraphSection(
                    "Recommended Before Play",
                    PyralisAuthoringGraphEvidenceState.Missing,
                    BuildValidationRows(graph, PyralisAuthoringGraphEvidenceState.Missing)),
                new PyralisAuthoringValidationGraphSection(
                    "Proof Enhancers",
                    PyralisAuthoringGraphEvidenceState.CandidateDetected,
                    BuildValidationRows(graph, PyralisAuthoringGraphEvidenceState.CandidateDetected))
            };
        }

        public static IReadOnlyList<PyralisAuthoringValidationGraphRow> BuildValidationDetailRows(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringValidationGraphRow>();

            List<PyralisAuthoringValidationGraphRow> rows = new List<PyralisAuthoringValidationGraphRow>();
            IReadOnlyList<PyralisAuthoringValidationGraphSection> sections = BuildValidationSections(graph);
            for (int i = 0; i < sections.Count; i++)
            {
                PyralisAuthoringValidationGraphSection section = sections[i];
                if (section == null || !section.HasRows)
                    continue;

                rows.AddRange(section.Rows);
            }

            return rows.ToArray();
        }

        public static IReadOnlyList<PyralisAuthoringIssue> BuildTypedValidationIssues(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringIssue>();

            List<PyralisAuthoringIssue> issues = new List<PyralisAuthoringIssue>();
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                PyralisAuthoringGraphNode node = graph.Nodes[i];
                if (node == null || !IsReadinessNode(node))
                    continue;

                if (node.EvidenceState == PyralisAuthoringGraphEvidenceState.Ready
                    || node.EvidenceState == PyralisAuthoringGraphEvidenceState.Optional
                    || node.EvidenceState == PyralisAuthoringGraphEvidenceState.Unknown)
                {
                    continue;
                }

                issues.Add(new PyralisAuthoringIssue(
                    node.StableId,
                    GetTypedIssueSeverity(node),
                    GetTypedIssueWorkIntent(node),
                    GetTypedIssueEvidenceState(node),
                    GetTypedIssueTargetObject(node),
                    GetTypedIssueFieldOrComponent(node),
                    node.NativeAction,
                    !string.IsNullOrWhiteSpace(node.BlockingReason) ? node.BlockingReason : node.Guidance));
            }

            return issues;
        }

        public static IReadOnlyList<PyralisAuthoringReflectiveContractGraphRow> BuildReflectiveContractRows(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringReflectiveContractGraphRow>();

            return graph.FindNodes(PyralisAuthoringGraphNodeKind.Contract)
                .Where(node => node != null
                    && node.SourceKind == PyralisAuthoringGraphSourceKind.AuthoringContract
                    && node.EvidenceState != PyralisAuthoringGraphEvidenceState.Unknown)
                .Select(node => new PyralisAuthoringReflectiveContractGraphRow(node))
                .ToArray();
        }

        public static PyralisAuthoringSelectedContextGraphRow BuildSelectedContextRow(PyralisAuthoringSetupGraph graph, Object selection)
        {
            if (selection == null)
            {
                return new PyralisAuthoringSelectedContextGraphRow(
                    null,
                    null,
                    "Select a Pyralis setup asset, scene root, pawn prefab, or component to see its authoring meaning.",
                    "Select an object that participates in the setup route.");
            }

            PyralisAuthoringGraphNode node = FindSelectedNode(graph, selection);
            return new PyralisAuthoringSelectedContextGraphRow(
                selection,
                node,
                GetSelectionRole(selection, node),
                GetSelectionNextCheck(selection, node),
                BuildSelectedContextDetails(selection, graph),
                GetSelectedContextCopyGuidance(selection),
                HasMissingRuntimePatternText(selection));
        }

        public static IReadOnlyList<PyralisAuthoringOverviewIssue> BuildOverviewIssues(PyralisAuthoringSetupGraph graph, Object activeSetup)
        {
            List<PyralisAuthoringOverviewIssue> issues = new List<PyralisAuthoringOverviewIssue>();
            if (graph == null || graph.Source == null)
            {
                issues.Add(BuildNoActiveSetupOverviewIssue(graph, activeSetup));
                return issues;
            }

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                PyralisAuthoringOverviewIssue issue = BuildOverviewIssue(graph.Nodes[i]);
                if (issue != null)
                    issues.Add(issue);
            }

            return issues;
        }

        public static IReadOnlyList<PyralisAuthoringPlayModeChecklistItem> BuildOverviewPlayModeChecklist(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null || graph.Source == null)
            {
                return new[]
                {
                    new PyralisAuthoringPlayModeChecklistItem(
                        "Setup foundation",
                        false,
                        "Create a Gameplay Root scene object with GameplaySessionBootstrap, then assign a SessionDefinition before Play Mode guidance can describe a proof.")
                };
            }

            List<PyralisAuthoringPlayModeChecklistItem> items = new List<PyralisAuthoringPlayModeChecklistItem>();
            bool requiredClear = CountOverviewDoNowBlockers(graph) == 0;
            PyralisAuthoringGraphNode firstRequired = FindFirstOverviewDoNowBlocker(graph);
            PyralisAuthoringGraphNode proofNode = FindCurrentProofNode(graph);
            items.Add(new PyralisAuthoringPlayModeChecklistItem(
                "Required setup",
                requiredClear,
                requiredClear ? "Do Now is clear." : firstRequired?.Guidance ?? "Clear the selected intent's Do Now setup before Play Mode."));

            items.Add(new PyralisAuthoringPlayModeChecklistItem(
                "First proof target",
                proofNode != null,
                proofNode != null && !string.IsNullOrWhiteSpace(proofNode.BlockingReason)
                    ? proofNode.BlockingReason
                    : "Select a route so Pyralis can name the smallest proof."));

            AddOverviewReadinessChecklistItem(
                items,
                "Scene visibility",
                graph,
                "CameraAudio",
                "Camera/audio checks are clear enough for a narrow visual proof.");
            AddOverviewReadinessChecklistItem(
                items,
                "Input route",
                graph,
                "Input",
                "InputProfile, action map, Move action, and UI input module checks are clear.");
            AddOverviewReadinessChecklistItem(
                items,
                "Presentation",
                graph,
                "Presentation",
                "Visible sprites/renderers and presentation-route checks are clear.");
            AddOverviewReadinessChecklistItem(
                items,
                "Physics feel",
                graph,
                "Physics",
                "Physics lane and collider checks are clear enough to judge movement feel.");

            return items;
        }

        public static string BuildOverviewBestNextAction(IReadOnlyList<PyralisAuthoringOverviewIssue> doNow, IReadOnlyList<PyralisAuthoringOverviewIssue> doSoon)
        {
            if (doNow != null && doNow.Count > 0)
                return FormatOverviewBestNextAction(doNow[0]);

            if (doSoon != null && doSoon.Count > 0)
                return "Optional proof enhancer: " + FormatOverviewBestNextAction(doSoon[0]);

            return "Required setup is clear. Run the minimal route proof in Play mode first, then add one feature at a time.";
        }

        public static bool IsOverviewReadyToPressPlay(PyralisAuthoringSetupGraph graph)
        {
            return graph != null && graph.Source != null && CountOverviewDoNowBlockers(graph) == 0;
        }

        public static string GetOverviewFirstProofLabel(PyralisAuthoringSetupGraph graph)
        {
            return FindCurrentProofNode(graph)?.Label ?? "Create Setup Foundation";
        }

        public static bool HasResolvedSetupContext(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null || graph.Source == null)
                return false;

            if (graph.Source is GameplaySessionBootstrap
                || graph.Source is SessionDefinition
                || graph.Source is GameModeDefinition
                || graph.Source is GameSetupProfile
                || graph.Source is ParticipantDefinition
                || graph.Source is PawnDefinition)
            {
                return true;
            }

            return graph.Source is GameObject gameObject
                && gameObject.GetComponent<GameplaySessionBootstrap>() != null;
        }

        public static string GetOverviewFirstProofGuidance(PyralisAuthoringSetupGraph graph)
        {
            return FindCurrentProofNode(graph)?.Guidance
                ?? "Create a Gameplay Root scene object with GameplaySessionBootstrap, then create and assign the first SessionDefinition asset.";
        }

        public static string GetOverviewFirstProofSetupSurface(PyralisAuthoringSetupGraph graph)
        {
            PyralisAuthoringGraphNode proofNode = FindCurrentProofNode(graph);
            return proofNode != null && proofNode.NativeSetup.Length > 0
                ? proofNode.NativeSetup[0]
                : "Hierarchy object plus Project asset foundation.";
        }

        public static string GetOverviewFirstProofSuccessCriteria(PyralisAuthoringSetupGraph graph)
        {
            return FindCurrentProofNode(graph)?.BlockingReason
                ?? "Overview can inspect the bootstrap route and name the first playable proof.";
        }

        public static string GetOverviewFirstProofDeferUntilAfter(PyralisAuthoringSetupGraph graph)
        {
            PyralisAuthoringGraphNode unresolved = FindFirstUnresolvedNode(graph);
            return unresolved != null
                ? "Defer expansion until this graph node is clear: " + unresolved.Label
                : "Defer broad polish until the graph-backed first proof runs in Play Mode.";
        }

        public static string GetOverviewFirstProofChainSummary(PyralisAuthoringSetupGraph graph)
        {
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> rows = BuildProofSupportRows(graph);
            if (rows.Count == 0)
                return "Graph proof chain: setup route -> active proof target.";

            return "Graph proof chain: " + string.Join(
                " -> ",
                rows.Select(row => row.FromLabel).Distinct().Concat(new[] { rows[0].ToLabel }));
        }

        private static PyralisAuthoringSetupGraphRow BuildCapabilitiesRow(PyralisAuthoringSetupGraph graph)
        {
            PyralisAuthoringGraphNode setupNode = FindNode(graph, "setup.profile");
            return new PyralisAuthoringSetupGraphRow(
                "Capabilities",
                FindNode(graph, "capability.selected"),
                fallbackMessage: "Choose capability ingredients before scene wiring.",
                fallbackTarget: setupNode?.SourceObject);
        }

        private static void AddGuideRow(
            List<PyralisAuthoringGuideGraphRow> rows,
            HashSet<string> added,
            PyralisAuthoringGraphNode node,
            PyralisAuthoringIntentGuideTier tier,
            PyralisAuthoringIntentRowState state,
            string reason)
        {
            if (rows == null || added == null || node == null || string.IsNullOrWhiteSpace(node.StableId))
                return;

            if (!added.Add(node.StableId))
                return;

            rows.Add(new PyralisAuthoringGuideGraphRow(node, tier, state, reason));
        }

        public static bool IsReadinessNode(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return false;

            if (node.Kind == PyralisAuthoringGraphNodeKind.ValidationEvidence
                || node.Kind == PyralisAuthoringGraphNodeKind.SetupChain
                || node.Kind == PyralisAuthoringGraphNodeKind.UnitySurfaceRequirement
                || node.Kind == PyralisAuthoringGraphNodeKind.SceneSurface)
            {
                return true;
            }

            return node.Kind == PyralisAuthoringGraphNodeKind.Capability
                && string.Equals(node.StableId, "capability.selected", StringComparison.Ordinal);
        }

        private static PyralisAuthoringIssueSeverity GetTypedIssueSeverity(PyralisAuthoringGraphNode node)
        {
            return node != null ? node.IssueSeverity : PyralisAuthoringIssueSeverity.Info;
        }

        private static string GetTypedIssueWorkIntent(PyralisAuthoringGraphNode node)
        {
            return node != null
                ? GetWorkIntentLabel(node.WorkIntent)
                : GetWorkIntentLabel(PyralisAuthoringGraphWorkIntent.Reference);
        }

        private static PyralisAuthoringEvidenceState GetTypedIssueEvidenceState(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return PyralisAuthoringEvidenceState.Missing;

            return node.EvidenceState switch
            {
                PyralisAuthoringGraphEvidenceState.Ready => PyralisAuthoringEvidenceState.Validated,
                PyralisAuthoringGraphEvidenceState.CandidateDetected => PyralisAuthoringEvidenceState.CandidateDetected,
                PyralisAuthoringGraphEvidenceState.Optional => PyralisAuthoringEvidenceState.NotRelevant,
                PyralisAuthoringGraphEvidenceState.Blocked => PyralisAuthoringEvidenceState.Conflict,
                PyralisAuthoringGraphEvidenceState.Missing => PyralisAuthoringEvidenceState.Missing,
                _ => PyralisAuthoringEvidenceState.Missing
            };
        }

        private static string GetTypedIssueTargetObject(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return string.Empty;

            if (node.SourceObject != null)
                return node.SourceObject.GetType().Name;

            return node.SourceKind switch
            {
                PyralisAuthoringGraphSourceKind.SetupFlow => "Setup Flow",
                PyralisAuthoringGraphSourceKind.SceneReadiness => "Scene Readiness",
                PyralisAuthoringGraphSourceKind.SetupProfile => "GameSetupProfile",
                PyralisAuthoringGraphSourceKind.CapabilityVocabulary => "Runtime Capability",
                PyralisAuthoringGraphSourceKind.ProofVocabulary => "Route Proof",
                _ => "Resolved Setup Graph"
            };
        }

        private static string GetTypedIssueFieldOrComponent(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return string.Empty;

            if (node.AssignmentFields.Length > 0)
                return node.AssignmentFields[0];

            if (node.NativeAction.HasValue && !string.IsNullOrWhiteSpace(node.NativeAction.Value.FieldOrComponent))
                return node.NativeAction.Value.FieldOrComponent;

            return node.StableId;
        }

        private static PyralisAuthoringOverviewIssue BuildNoActiveSetupOverviewIssue(PyralisAuthoringSetupGraph graph, Object activeSetup)
        {
            PyralisAuthoringGraphNode bootstrapNode = null;
            graph?.TryFindNode("bootstrap.root", out bootstrapNode);
            string message = bootstrapNode != null && !string.IsNullOrWhiteSpace(bootstrapNode.Guidance)
                ? bootstrapNode.Guidance
                : "Create a Gameplay Root scene object with GameplaySessionBootstrap, then create and assign a SessionDefinition.";
            return new PyralisAuthoringOverviewIssue(
                PyralisAuthoringOverviewLane.DoNow,
                "Create Gameplay Root",
                PyralisAuthoringGraphEvidenceState.Missing,
                message,
                activeSetup,
                "Overview needs a GameplaySessionBootstrap route before it can judge scene readiness.",
                bootstrapNode != null && bootstrapNode.NativeAction.HasValue
                    ? bootstrapNode.NativeAction.Value.ToGuidanceSentence()
                    : GetGameplayRootNativeActionGuidance(),
                GetWorkIntentLabel(PyralisAuthoringGraphWorkIntent.RequiredSetup));
        }

        private static PyralisAuthoringOverviewIssue BuildOverviewIssue(PyralisAuthoringGraphNode node)
        {
            if (node == null || node.EvidenceState == PyralisAuthoringGraphEvidenceState.Ready)
                return null;

            if (!IsReadinessNode(node))
                return null;

            PyralisAuthoringOverviewLane lane = GetOverviewLane(node);
            return new PyralisAuthoringOverviewIssue(
                lane,
                node.Label,
                node.EvidenceState,
                node.Guidance,
                node.SourceObject,
                GetOverviewEvidence(node),
                node.NativeAction.HasValue ? node.NativeAction.Value.ToGuidanceSentence() : GetFirstNativeSetup(node),
                GetWorkIntentLabel(node.WorkIntent));
        }

        private static PyralisAuthoringOverviewLane GetOverviewLane(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return PyralisAuthoringOverviewLane.Later;

            if (node.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked)
                return PyralisAuthoringOverviewLane.DoNow;

            if (node.EvidenceState == PyralisAuthoringGraphEvidenceState.Missing)
            {
                if (IsOverviewDoNowBlocker(node))
                    return PyralisAuthoringOverviewLane.DoNow;

                return PyralisAuthoringOverviewLane.DoSoon;
            }

            if (node.EvidenceState == PyralisAuthoringGraphEvidenceState.CandidateDetected)
                return PyralisAuthoringOverviewLane.DoSoon;

            return PyralisAuthoringOverviewLane.Later;
        }

        private static int CountOverviewDoNowBlockers(PyralisAuthoringSetupGraph graph)
        {
            int count = 0;
            if (graph == null)
                return count;

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                if (IsOverviewDoNowBlocker(graph.Nodes[i]))
                    count++;
            }

            return count;
        }

        private static PyralisAuthoringGraphNode FindFirstOverviewDoNowBlocker(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return null;

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                if (IsOverviewDoNowBlocker(graph.Nodes[i]))
                    return graph.Nodes[i];
            }

            return null;
        }

        private static bool IsOverviewDoNowBlocker(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return false;

            if (node.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked)
                return true;

            if (node.EvidenceState != PyralisAuthoringGraphEvidenceState.Missing)
                return false;

            return node.WorkIntent == PyralisAuthoringGraphWorkIntent.RequiredSetup;
        }

        private static string GetWorkIntentLabel(PyralisAuthoringGraphWorkIntent workIntent)
        {
            switch (workIntent)
            {
                case PyralisAuthoringGraphWorkIntent.RequiredSetup:
                    return "Required Setup";
                case PyralisAuthoringGraphWorkIntent.ProofEnhancer:
                    return "Proof Enhancer";
                case PyralisAuthoringGraphWorkIntent.FeatureCard:
                    return "Feature Card";
                case PyralisAuthoringGraphWorkIntent.Optional:
                    return "Optional";
                default:
                    return "Later";
            }
        }

        private static string GetOverviewEvidence(PyralisAuthoringGraphNode node)
        {
            if (node.SourceObject != null)
                return "Evidence: " + node.SourceObject.name + " (" + node.SourceObject.GetType().Name + ")";

            return "Evidence: " + node.SourceKind + " / " + node.StableId;
        }

        private static string GetFirstNativeSetup(PyralisAuthoringGraphNode node)
        {
            if (node == null || node.NativeSetup.Length == 0)
                return string.Empty;

            return node.NativeSetup[0];
        }

        private static string FormatOverviewBestNextAction(PyralisAuthoringOverviewIssue issue)
        {
            if (issue == null)
                return "Select a setup item and follow its native Unity action.";

            if (!string.IsNullOrWhiteSpace(issue.NativeActionGuidance))
                return issue.Label + ": " + issue.NativeActionGuidance;

            return issue.Label + ": " + issue.Message;
        }

        private static string GetGameplayRootNativeActionGuidance()
        {
            return new PyralisAuthoringNativeAction(
                "Create or select",
                PyralisAuthoringActionSurface.Hierarchy,
                "Gameplay Root",
                "right-click -> Create Empty, name it Gameplay Root, then use Inspector -> Add Component -> GameplaySessionBootstrap",
                "Overview shows Gameplay Root as the active setup").ToGuidanceSentence();
        }

        private static void AddOverviewReadinessChecklistItem(
            List<PyralisAuthoringPlayModeChecklistItem> items,
            string label,
            PyralisAuthoringSetupGraph graph,
            string category,
            string readyDetail)
        {
            PyralisAuthoringGraphNode issue = FindFirstSceneReadinessNode(graph, category);
            items.Add(new PyralisAuthoringPlayModeChecklistItem(
                label,
                issue == null || issue.EvidenceState != PyralisAuthoringGraphEvidenceState.Blocked,
                issue == null ? readyDetail : issue.Guidance));
        }

        private static PyralisAuthoringGraphNode FindFirstSceneReadinessNode(PyralisAuthoringSetupGraph graph, string category)
        {
            if (graph == null || string.IsNullOrWhiteSpace(category))
                return null;

            PyralisAuthoringGraphNode fallback = null;
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                PyralisAuthoringGraphNode issue = graph.Nodes[i];
                if (issue == null || issue.SourceKind != PyralisAuthoringGraphSourceKind.SceneReadiness)
                    continue;

                if (!string.Equals(issue.Label, category, StringComparison.Ordinal))
                    continue;

                if (issue.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked)
                    return issue;

                fallback ??= issue;
            }

            return fallback;
        }

        private static PyralisAuthoringGraphNode FindFirstUnresolvedNode(PyralisAuthoringSetupGraph graph, PyralisAuthoringGraphNodeKind kind)
        {
            IReadOnlyList<PyralisAuthoringGraphNode> nodes = graph.FindNodes(kind);
            for (int i = 0; i < nodes.Count; i++)
            {
                PyralisAuthoringGraphNode node = nodes[i];
                if (node == null)
                    continue;

                if (node.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked || node.EvidenceState == PyralisAuthoringGraphEvidenceState.Missing)
                    return node;
            }

            return null;
        }

        private static PyralisAuthoringGraphNode FindFirstUnresolvedSetupFlowNode(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return null;

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                PyralisAuthoringGraphNode node = graph.Nodes[i];
                if (node == null || node.Kind != PyralisAuthoringGraphNodeKind.ValidationEvidence)
                    continue;

                if (node.SourceKind != PyralisAuthoringGraphSourceKind.SetupFlow)
                    continue;

                if (node.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked || node.EvidenceState == PyralisAuthoringGraphEvidenceState.Missing)
                    return node;
            }

            return null;
        }

        private static PyralisAuthoringCurrentStepGraphRow BuildFallbackCurrentStepRow()
        {
            const string routeName = "No setup route selected";
            const string message = "Create a Gameplay Root scene object with GameplaySessionBootstrap, then create and assign the first SessionDefinition asset.";
            const string detail = "Start with the native Unity setup chain: Hierarchy object, bootstrap component, SessionDefinition, GameModeDefinition, GameSetupProfile, capability intent, then participants.";

            return new PyralisAuthoringCurrentStepGraphRow(routeName, null, message, detail, null);
        }

        private static string GetCurrentStepDetail(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(node.BlockingReason) && !string.Equals(node.BlockingReason, node.Guidance, StringComparison.Ordinal))
                return node.BlockingReason;

            if (node.AssignmentFields.Length > 0)
                return "Inspector field: " + string.Join(", ", node.AssignmentFields);

            if (node.NativeSetup.Length > 0)
                return node.NativeSetup[0];

            return "Use Map for topology, Validate for the full issue list, and the Inspector for field-level edits.";
        }

        private static string GetCurrentRouteName(PyralisAuthoringSetupGraph graph)
        {
            return graph != null && !string.IsNullOrWhiteSpace(graph.RouteName)
                ? graph.RouteName
                : "No setup route selected";
        }

        private static PyralisAuthoringSetupGraphRow Row(
            PyralisAuthoringSetupGraph graph,
            string label,
            string nodeId,
            string fallbackMessage = null,
            bool isOptional = false)
        {
            PyralisAuthoringGraphNode node = FindNode(graph, nodeId);
            return new PyralisAuthoringSetupGraphRow(label, node, isOptional, fallbackMessage);
        }

        private static PyralisAuthoringGraphNode FindNode(PyralisAuthoringSetupGraph graph, string nodeId)
        {
            return graph != null && graph.TryFindNode(nodeId, out PyralisAuthoringGraphNode node) ? node : null;
        }

        private static PyralisAuthoringGraphNode FindSelectedNode(PyralisAuthoringSetupGraph graph, Object selection)
        {
            if (graph == null || selection == null)
                return null;

            string stableId = GetKnownSelectionNodeId(selection);
            if (!string.IsNullOrWhiteSpace(stableId) && graph.TryFindNode(stableId, out PyralisAuthoringGraphNode knownNode))
                return knownNode;

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                PyralisAuthoringGraphNode node = graph.Nodes[i];
                if (node != null && node.SourceObject == selection)
                    return node;
            }

            ResolvedAuthoringContract contract = ResolveSelectionContract(selection);
            if (contract != null)
            {
                if (!string.IsNullOrWhiteSpace(contract.SetupNodeId) && graph.TryFindNode(contract.SetupNodeId, out PyralisAuthoringGraphNode setupNode))
                    return setupNode;

                if (graph.TryFindNode("contract." + contract.StableId, out PyralisAuthoringGraphNode contractNode))
                    return contractNode;
            }

            return null;
        }

        private static string GetKnownSelectionNodeId(Object selection)
        {
            if (selection is GameplaySessionBootstrap)
                return "bootstrap.root";

            if (selection is GameObject gameObject)
            {
                if (gameObject.GetComponent<GameplaySessionBootstrap>() != null)
                    return "bootstrap.root";
                if (gameObject.GetComponent<PawnRoot>() != null)
                    return "pawn.definition";
            }

            if (selection is Component component)
            {
                if (component is GameplaySessionBootstrap)
                    return "bootstrap.root";
                if (component is PawnRoot)
                    return "pawn.definition";
            }

            return selection switch
            {
                SessionDefinition => "session.definition",
                GameModeDefinition => "mode.definition",
                GameSetupProfile => "setup.profile",
                ParticipantDefinition => "participant.default",
                PawnDefinition => "pawn.definition",
                _ => string.Empty
            };
        }

        private static ResolvedAuthoringContract ResolveSelectionContract(Object selection)
        {
            if (selection is FeatureModuleDefinition module && !string.IsNullOrWhiteSpace(module.moduleId))
                return ResolvedAuthoringContractRegistry.FindByModuleId(module.moduleId);

            if (selection is Component component)
                return ResolvedAuthoringContractRegistry.FindByType(component.GetType());

            return ResolvedAuthoringContractRegistry.FindByType(selection.GetType());
        }

        private static string GetSelectionRole(Object selection, PyralisAuthoringGraphNode node)
        {
            if (node != null && !string.IsNullOrWhiteSpace(node.Guidance))
                return node.Guidance;

            return selection switch
            {
                GameplaySessionBootstrap => "Scene startup and setup-flow root.",
                SessionDefinition => "Session contract for game rules, participants, local/network mode, and participant limits.",
                GameModeDefinition => "Rules contract that chooses the setup profile and rule-level defaults.",
                GameSetupProfile => "Capability ingredients that describe what this game route needs before scene wiring starts.",
                RuntimePatternDefinition => "Optional advanced route contract for reusable metadata beyond generic capability rows.",
                ParticipantDefinition => "Seat, player, NPC, hand, faction, or command owner in the session.",
                PawnDefinition => "Pawn prefab, profiles, feature modules, and presentation setup.",
                FeatureModuleDefinition => "Feature module contract selected by the route.",
                Component component => component is PawnRoot
                    ? "PawnRoot marks the prefab root that Pyralis treats as a pawn actor."
                    : "Runtime or authoring component participating in the selected GameObject.",
                GameObject => "Scene or prefab object. Pyralis components on it determine how it participates in the route.",
                _ => "Use this asset's Inspector for fields, and use Guide to understand how it fits into setup."
            };
        }

        private static string GetSelectionNextCheck(Object selection, PyralisAuthoringGraphNode node)
        {
            if (node != null)
            {
                if (!string.IsNullOrWhiteSpace(node.BlockingReason))
                    return node.BlockingReason;
                if (node.AssignmentFields.Length > 0)
                    return "Inspect: " + string.Join(", ", node.AssignmentFields);
                if (node.NativeSetup.Length > 0)
                    return "Native setup: " + string.Join("; ", node.NativeSetup);
            }

            return selection switch
            {
                GameplaySessionBootstrap => "Inspect session definition, spawn points, input manager, and camera rig references.",
                SessionDefinition => "Inspect default game mode and default participants.",
                GameModeDefinition => "Inspect setup profile and required feature modules.",
                GameSetupProfile => "Open Intent when changing capability ingredients; keep Guide read-only for this selection.",
                ParticipantDefinition => "Inspect default pawn, input profile, seat index, and auto-join ownership.",
                PawnDefinition => "Inspect pawn prefab, movement/input/presentation profiles, and feature modules.",
                Component => "Use the Inspector for field values and Validate for concrete readiness issues.",
                GameObject => "Select the most specific Pyralis component on this object when you need field-level meaning.",
                _ => "Inspect the selected asset fields in Unity."
            };
        }

        private static IReadOnlyList<PyralisAuthoringSelectedContextDetail> BuildSelectedContextDetails(
            Object selection,
            PyralisAuthoringSetupGraph graph)
        {
            if (selection is RuntimePatternDefinition pattern)
                return BuildRuntimePatternContextDetails(pattern);

            if (selection is GameSetupProfile setupProfile)
                return BuildSetupProfileContextDetails(setupProfile, graph);

            return Array.Empty<PyralisAuthoringSelectedContextDetail>();
        }

        private static IReadOnlyList<PyralisAuthoringSelectedContextDetail> BuildRuntimePatternContextDetails(RuntimePatternDefinition pattern)
        {
            if (pattern == null)
                return Array.Empty<PyralisAuthoringSelectedContextDetail>();

            string description = GetRuntimePatternDescription(pattern);
            string setupNotes = GetRuntimePatternSetupNotes(pattern);
            return new[]
            {
                new PyralisAuthoringSelectedContextDetail("Description", description),
                new PyralisAuthoringSelectedContextDetail("Presentation Lanes", FormatPresentationLanes(pattern.presentationLanes)),
                new PyralisAuthoringSelectedContextDetail("First Proof Requirements", pattern.firstProofRequirements.ToString()),
                new PyralisAuthoringSelectedContextDetail("Setup Notes", setupNotes)
            };
        }

        private static IReadOnlyList<PyralisAuthoringSelectedContextDetail> BuildSetupProfileContextDetails(
            GameSetupProfile setupProfile,
            PyralisAuthoringSetupGraph graph)
        {
            if (setupProfile == null)
                return Array.Empty<PyralisAuthoringSelectedContextDetail>();

            List<PyralisAuthoringSelectedContextDetail> details = new List<PyralisAuthoringSelectedContextDetail>
            {
                new PyralisAuthoringSelectedContextDetail(
                    "Route Shaping",
                    "Open Intent to choose or revise setup profile capability ingredients. Guide keeps this selected-profile view read-only so route shaping stays in one place.")
            };

            IReadOnlyList<PyralisAuthoringGraphNode> patternNodes = FindRuntimePatternNodes(graph);
            if (patternNodes.Count == 0 && (setupProfile.runtimePatterns == null || setupProfile.runtimePatterns.Length == 0))
            {
                details.Add(new PyralisAuthoringSelectedContextDetail(
                    "Optional Route Contracts",
                    "No optional runtime pattern assets are assigned. That is fine for generic capability-first setup; add one only when a route needs reusable advanced metadata."));
                return details;
            }

            if (patternNodes.Count > 0)
            {
                for (int i = 0; i < patternNodes.Count; i++)
                {
                    PyralisAuthoringGraphNode node = patternNodes[i];
                    details.Add(new PyralisAuthoringSelectedContextDetail(
                        "Runtime Pattern " + i,
                        node.Label + " - " + node.Guidance,
                        node.SourceObject));
                }

                return details.ToArray();
            }

            for (int i = 0; i < setupProfile.runtimePatterns.Length; i++)
            {
                RuntimePatternDefinition pattern = setupProfile.runtimePatterns[i];
                details.Add(new PyralisAuthoringSelectedContextDetail(
                    "Runtime Pattern " + i,
                    pattern == null
                        ? "Pattern slot " + i + " is empty."
                        : GetRuntimePatternLabel(pattern) + " - " + pattern.capabilityFamily + " / " + pattern.participantEmbodiment,
                    pattern));
            }

            return details.ToArray();
        }

        private static IReadOnlyList<PyralisAuthoringGraphNode> FindRuntimePatternNodes(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringGraphNode>();

            return graph.Nodes
                .Where(node => node != null && node.SourceKind == PyralisAuthoringGraphSourceKind.RuntimePattern)
                .ToArray();
        }

        private static string GetSelectedContextCopyGuidance(Object selection)
        {
            if (selection is RuntimePatternDefinition pattern)
                return GetRuntimePatternDescription(pattern) + "\n\nSetup notes:\n" + GetRuntimePatternSetupNotes(pattern);

            return string.Empty;
        }

        private static bool HasMissingRuntimePatternText(Object selection)
        {
            return selection is RuntimePatternDefinition pattern
                && (string.IsNullOrWhiteSpace(pattern.description) || string.IsNullOrWhiteSpace(pattern.setupNotes));
        }

        private static string GetRuntimePatternDescription(RuntimePatternDefinition pattern)
        {
            if (pattern == null)
                return string.Empty;

            return !string.IsNullOrWhiteSpace(pattern.description)
                ? pattern.description
                : RuntimePatternAuthoringText.GetSuggestedDescription(pattern);
        }

        private static string GetRuntimePatternSetupNotes(RuntimePatternDefinition pattern)
        {
            if (pattern == null)
                return string.Empty;

            return !string.IsNullOrWhiteSpace(pattern.setupNotes)
                ? pattern.setupNotes
                : RuntimePatternAuthoringText.GetSuggestedSetupNotes(pattern);
        }

        private static string GetRuntimePatternLabel(RuntimePatternDefinition pattern)
        {
            if (pattern == null)
                return "Missing pattern";

            if (!string.IsNullOrWhiteSpace(pattern.displayName))
                return pattern.displayName;

            if (!string.IsNullOrWhiteSpace(pattern.patternId))
                return pattern.patternId;

            return pattern.name;
        }

        private static string FormatPresentationLanes(RuntimePatternPresentationLane[] lanes)
        {
            if (lanes == null || lanes.Length == 0)
                return "None assigned";

            string[] labels = new string[lanes.Length];
            for (int i = 0; i < lanes.Length; i++)
                labels[i] = lanes[i].ToString();

            return string.Join(", ", labels);
        }
    }
}
