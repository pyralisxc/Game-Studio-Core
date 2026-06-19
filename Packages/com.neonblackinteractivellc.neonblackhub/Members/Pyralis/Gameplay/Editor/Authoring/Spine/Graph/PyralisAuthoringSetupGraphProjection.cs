using System;
using System.Collections.Generic;
using System.Linq;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
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
            Object fallbackTarget = null,
            PyralisAuthoringGraphEvidenceState? effectiveEvidenceState = null,
            string effectiveMessage = null)
        {
            Label = label ?? string.Empty;
            Node = node;
            IsOptional = isOptional;
            FallbackMessage = fallbackMessage ?? string.Empty;
            FallbackTarget = fallbackTarget;
            EffectiveEvidenceState = effectiveEvidenceState ?? (node != null ? node.EvidenceState : PyralisAuthoringGraphEvidenceState.Unknown);
            EffectiveMessage = effectiveMessage ?? string.Empty;
        }

        public string Label { get; }
        public PyralisAuthoringGraphNode Node { get; }
        public bool IsOptional { get; }
        public string FallbackMessage { get; }
        public Object FallbackTarget { get; }
        public PyralisAuthoringGraphEvidenceState EffectiveEvidenceState { get; }
        public string EffectiveMessage { get; }
        public Object Target => Node != null && Node.SourceObject != null ? Node.SourceObject : FallbackTarget;
        public string Message => !string.IsNullOrWhiteSpace(EffectiveMessage)
            ? EffectiveMessage
            : Node != null && !string.IsNullOrWhiteSpace(Node.Guidance)
                ? Node.Guidance
                : FallbackMessage;
        public bool IsReady => Node != null && (EffectiveEvidenceState == PyralisAuthoringGraphEvidenceState.Ready || EffectiveEvidenceState == PyralisAuthoringGraphEvidenceState.Optional);
        public bool IsMissing => Node != null && (EffectiveEvidenceState == PyralisAuthoringGraphEvidenceState.Missing || EffectiveEvidenceState == PyralisAuthoringGraphEvidenceState.Blocked);
    }

    public sealed class PyralisAuthoringGraphAuditRow
    {
        public PyralisAuthoringGraphAuditRow(PyralisAuthoringGraphNode node)
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
                PyralisAuthoringGraphSourceKind.CapabilityVocabulary => "Capability Vocabulary",
                PyralisAuthoringGraphSourceKind.AuthoringContract => "Authoring Contract",
                PyralisAuthoringGraphSourceKind.GrammarRegistry => "Grammar Registry",
                PyralisAuthoringGraphSourceKind.SetupFlow => "Setup Flow",
                PyralisAuthoringGraphSourceKind.RuntimeValidation => "Runtime Validation",
                PyralisAuthoringGraphSourceKind.SceneReadiness => "Scene Readiness",
                PyralisAuthoringGraphSourceKind.ProofVocabulary => "Proof Vocabulary",
                PyralisAuthoringGraphSourceKind.Reflection => "Reflection",
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

    public sealed class PyralisAuthoringGraphAuditSection
    {
        public PyralisAuthoringGraphAuditSection(
            string label,
            PyralisAuthoringGraphEvidenceState evidenceState,
            IReadOnlyList<PyralisAuthoringGraphAuditRow> rows)
        {
            Label = label ?? string.Empty;
            EvidenceState = evidenceState;
            Rows = rows ?? Array.Empty<PyralisAuthoringGraphAuditRow>();
        }

        public string Label { get; }
        public PyralisAuthoringGraphEvidenceState EvidenceState { get; }
        public IReadOnlyList<PyralisAuthoringGraphAuditRow> Rows { get; }
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

    public enum PyralisAuthoringRouteStepPhase
    {
        Foundation,
        SetupChain,
        Capability,
        SceneEvidence,
        FirstProof,
        Validation,
        Optional,
        Reference
    }

    public enum PyralisAuthoringRouteStepRole
    {
        DoThisFirst,
        BlocksProof,
        ProofTarget,
        SupportsProof,
        RouteContext,
        CanWait
    }

    public sealed class PyralisAuthoringRouteStepRow
    {
        public PyralisAuthoringRouteStepRow(
            PyralisAuthoringGraphNode node,
            int sequence,
            PyralisAuthoringRouteStepPhase phase,
            PyralisAuthoringRouteStepRole role,
            string reason,
            PyralisAuthoringGraphEdge edge = null,
            string labelOverride = null,
            string messageOverride = null,
            string[] nativeSetupOverride = null,
            string[] assignmentFieldsOverride = null,
            string[] customizationMomentsOverride = null,
            PyralisAuthoringNativeAction? nativeActionOverride = null)
        {
            Node = node;
            Sequence = sequence;
            Phase = phase;
            Role = role;
            Reason = reason ?? string.Empty;
            Edge = edge;
            LabelOverride = labelOverride ?? string.Empty;
            MessageOverride = messageOverride ?? string.Empty;
            NativeSetupOverride = nativeSetupOverride;
            AssignmentFieldsOverride = assignmentFieldsOverride;
            CustomizationMomentsOverride = customizationMomentsOverride;
            NativeActionOverride = nativeActionOverride;
        }

        public PyralisAuthoringGraphNode Node { get; }
        public int Sequence { get; }
        public PyralisAuthoringRouteStepPhase Phase { get; }
        public PyralisAuthoringRouteStepRole Role { get; }
        public string Reason { get; }
        public PyralisAuthoringGraphEdge Edge { get; }
        public string LabelOverride { get; }
        public string MessageOverride { get; }
        public string[] NativeSetupOverride { get; }
        public string[] AssignmentFieldsOverride { get; }
        public string[] CustomizationMomentsOverride { get; }
        public PyralisAuthoringNativeAction? NativeActionOverride { get; }
        public string StableId => Node != null ? Node.StableId : string.Empty;
        public string Label => !string.IsNullOrWhiteSpace(LabelOverride) ? LabelOverride : Node != null ? Node.Label : string.Empty;
        public string Message => !string.IsNullOrWhiteSpace(MessageOverride) ? MessageOverride : Node != null ? Node.Guidance : string.Empty;
        public string FirstProof => Node != null && !string.IsNullOrWhiteSpace(Node.BlockingReason) ? Node.BlockingReason : Message;
        public string[] NativeSetup => NativeSetupOverride ?? (Node != null ? Node.NativeSetup : Array.Empty<string>());
        public string[] AssignmentFields => AssignmentFieldsOverride ?? (Node != null ? Node.AssignmentFields : Array.Empty<string>());
        public string[] CustomizationMoments => CustomizationMomentsOverride ?? (Node != null ? Node.CustomizationMoments : Array.Empty<string>());
        public PyralisAuthoringNativeAction? NativeAction => NativeActionOverride ?? Node?.NativeAction;
        public PyralisAuthoringGraphEvidenceState EvidenceState => Node != null ? Node.EvidenceState : PyralisAuthoringGraphEvidenceState.Unknown;
        public PyralisAuthoringGraphSourceOrigin SourceOrigin => Node != null ? Node.SourceOrigin : PyralisAuthoringGraphSourceOrigin.Unknown;
        public bool IsCurrentAction => Role == PyralisAuthoringRouteStepRole.DoThisFirst || Role == PyralisAuthoringRouteStepRole.BlocksProof;
        public string PhaseLabel => FormatPhase(Phase);
        public string RoleLabel => FormatRole(Role);
        public string UnityActionLabel => NativeAction.HasValue
            ? NativeAction.Value.ToGuidanceSentence()
            : NativeSetup.Length > 0
                ? NativeSetup[0]
                : string.Empty;

        private static string FormatPhase(PyralisAuthoringRouteStepPhase phase)
        {
            return phase switch
            {
                PyralisAuthoringRouteStepPhase.Foundation => "Foundation",
                PyralisAuthoringRouteStepPhase.SetupChain => "Setup Chain",
                PyralisAuthoringRouteStepPhase.Capability => "Capability",
                PyralisAuthoringRouteStepPhase.SceneEvidence => "Scene Evidence",
                PyralisAuthoringRouteStepPhase.FirstProof => "First Proof",
                PyralisAuthoringRouteStepPhase.Validation => "Validation",
                PyralisAuthoringRouteStepPhase.Optional => "Can Wait",
                _ => "Reference"
            };
        }

        private static string FormatRole(PyralisAuthoringRouteStepRole role)
        {
            return role switch
            {
                PyralisAuthoringRouteStepRole.DoThisFirst => "Do This First",
                PyralisAuthoringRouteStepRole.BlocksProof => "Blocks Proof",
                PyralisAuthoringRouteStepRole.ProofTarget => "Proof Target",
                PyralisAuthoringRouteStepRole.SupportsProof => "Supports Proof",
                PyralisAuthoringRouteStepRole.CanWait => "Can Wait",
                _ => "Route Context"
            };
        }
    }

    public sealed class PyralisAuthoringRouteWorkingProjection
    {
        public PyralisAuthoringRouteWorkingProjection(
            string routeName,
            PyralisAuthoringGraphNode proof,
            IReadOnlyList<PyralisAuthoringRouteStepRow> orderedSteps,
            IReadOnlyList<PyralisAuthoringRouteStepRow> criticalPath,
            IReadOnlyList<PyralisAuthoringRouteStepRow> proofEnhancers,
            IReadOnlyList<PyralisAuthoringRouteStepRow> canWait,
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> proofBlockers,
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> proofSupport)
        {
            RouteName = routeName ?? "No setup route selected";
            Proof = proof;
            OrderedSteps = orderedSteps ?? Array.Empty<PyralisAuthoringRouteStepRow>();
            CriticalPath = criticalPath ?? Array.Empty<PyralisAuthoringRouteStepRow>();
            ProofEnhancers = proofEnhancers ?? Array.Empty<PyralisAuthoringRouteStepRow>();
            CanWait = canWait ?? Array.Empty<PyralisAuthoringRouteStepRow>();
            ProofBlockers = proofBlockers ?? Array.Empty<PyralisAuthoringGraphConnectionRow>();
            ProofSupport = proofSupport ?? Array.Empty<PyralisAuthoringGraphConnectionRow>();
            CurrentAction = FindCurrentAction(CriticalPath);
            ReadyForFirstProof = CurrentAction == null && ProofBlockers.Count == 0;
        }

        public string RouteName { get; }
        public PyralisAuthoringGraphNode Proof { get; }
        public IReadOnlyList<PyralisAuthoringRouteStepRow> OrderedSteps { get; }
        public IReadOnlyList<PyralisAuthoringRouteStepRow> CriticalPath { get; }
        public IReadOnlyList<PyralisAuthoringRouteStepRow> ProofEnhancers { get; }
        public IReadOnlyList<PyralisAuthoringRouteStepRow> CanWait { get; }
        public IReadOnlyList<PyralisAuthoringGraphConnectionRow> ProofBlockers { get; }
        public IReadOnlyList<PyralisAuthoringGraphConnectionRow> ProofSupport { get; }
        public PyralisAuthoringRouteStepRow CurrentAction { get; }
        public bool ReadyForFirstProof { get; }

        private static PyralisAuthoringRouteStepRow FindCurrentAction(IReadOnlyList<PyralisAuthoringRouteStepRow> criticalPath)
        {
            if (criticalPath == null)
                return null;

            for (int i = 0; i < criticalPath.Count; i++)
            {
                PyralisAuthoringRouteStepRow row = criticalPath[i];
                if (row == null)
                    continue;

                if ((row.Role == PyralisAuthoringRouteStepRole.DoThisFirst
                    || row.Role == PyralisAuthoringRouteStepRole.BlocksProof)
                    && (row.EvidenceState == PyralisAuthoringGraphEvidenceState.Missing
                        || row.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked))
                {
                    return row;
                }
            }

            return null;
        }
    }

    internal readonly struct PyralisRouteStepLens
    {
        public static readonly PyralisRouteStepLens Empty = new PyralisRouteStepLens(
            string.Empty,
            string.Empty,
            null,
            null,
            null,
            null);

        public PyralisRouteStepLens(
            string labelOverride,
            string messageOverride,
            string[] nativeSetupOverride,
            string[] assignmentFieldsOverride,
            string[] customizationMomentsOverride,
            PyralisAuthoringNativeAction? nativeActionOverride)
        {
            LabelOverride = labelOverride ?? string.Empty;
            MessageOverride = messageOverride ?? string.Empty;
            NativeSetupOverride = nativeSetupOverride;
            AssignmentFieldsOverride = assignmentFieldsOverride;
            CustomizationMomentsOverride = customizationMomentsOverride;
            NativeActionOverride = nativeActionOverride;
        }

        public string LabelOverride { get; }
        public string MessageOverride { get; }
        public string[] NativeSetupOverride { get; }
        public string[] AssignmentFieldsOverride { get; }
        public string[] CustomizationMomentsOverride { get; }
        public PyralisAuthoringNativeAction? NativeActionOverride { get; }
    }

    public sealed class PyralisAuthoringSelectedContextGraphRow
    {
        public PyralisAuthoringSelectedContextGraphRow(
            Object selection,
            PyralisAuthoringGraphNode node,
            string role,
            string nextCheck,
            IReadOnlyList<PyralisAuthoringSelectedContextDetail> details = null,
            string copyGuidance = null)
        {
            Selection = selection;
            Node = node;
            Role = role ?? string.Empty;
            NextCheck = nextCheck ?? string.Empty;
            Details = details ?? Array.Empty<PyralisAuthoringSelectedContextDetail>();
            CopyGuidance = copyGuidance ?? string.Empty;
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
            Object target,
            PyralisAuthoringNativeAction? nativeActionOverride = null)
        {
            RouteName = routeName ?? string.Empty;
            Node = node;
            Message = message ?? string.Empty;
            Detail = detail ?? string.Empty;
            Target = target;
            NativeActionOverride = nativeActionOverride;
        }

        public string RouteName { get; }
        public PyralisAuthoringGraphNode Node { get; }
        public string Label => Node != null ? Node.Label : "Create Setup Foundation";
        public string Message { get; }
        public string Detail { get; }
        public Object Target { get; }
        public PyralisAuthoringNativeAction? NativeActionOverride { get; }
        public PyralisAuthoringNativeAction? NativeAction => NativeActionOverride ?? Node?.NativeAction;
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
                Row(graph, "Gameplay Root", "bootstrap.root", "Scene object that starts the session."),
                Row(graph, "Session Definition", "session.definition", "Asset that names game rules and participants."),
                Row(graph, "Game Mode", "mode.definition", "Ruleset that owns rule-level defaults and feature modules."),
                BuildCapabilitiesRow(graph),
                Row(graph, "Control Shape", "route.shape", "Participant ownership shape compiled from route evidence."),
                Row(graph, "Participants", "participant.default", "Assign at least one default participant."),
                Row(graph, "Pawn Setup", "pawn.definition", "Pawn-backed routes need a ParticipantDefinition.defaultPawn.", isOptional: IsNodeOptional(graph, "pawn.definition")),
                Row(graph, "Camera Focus", "route.camera-focus", "Camera focus mode and target route.", isOptional: IsNodeOptional(graph, "route.camera-focus")),
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
                Row(graph, "Gameplay Root", "bootstrap.root"),
                Row(graph, "Session Definition", "session.definition"),
                Row(graph, "Game Mode", "mode.definition"),
                BuildCapabilitiesRow(graph),
                Row(graph, "Control Shape", "route.shape", isOptional: IsNodeOptional(graph, "route.shape")),
                Row(graph, "Players / Seats", "participant.default"),
                Row(graph, "Pawn Setup", "pawn.definition", isOptional: IsNodeOptional(graph, "pawn.definition")),
                Row(graph, "Camera Focus", "route.camera-focus", isOptional: IsNodeOptional(graph, "route.camera-focus")),
                Row(graph, "Scene Surfaces", "scene.surfaces", isOptional: true)
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

        public static IReadOnlyList<PyralisAuthoringGraphAuditRow> BuildMapSceneSetupIssueRows(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringGraphAuditRow>();

            return BuildReadinessAuditDetailRows(graph)
                .Where(IsMapSceneSetupIssue)
                .ToArray();
        }

        private static bool IsMapSceneSetupIssue(PyralisAuthoringGraphAuditRow row)
        {
            PyralisAuthoringGraphNode node = row?.Node;
            if (node == null)
                return false;

            if (node.EvidenceState == PyralisAuthoringGraphEvidenceState.Ready
                || node.EvidenceState == PyralisAuthoringGraphEvidenceState.Optional
                || node.EvidenceState == PyralisAuthoringGraphEvidenceState.Unknown)
            {
                return false;
            }

            if (node.Kind == PyralisAuthoringGraphNodeKind.SceneSurface
                || node.Kind == PyralisAuthoringGraphNodeKind.SetupChain
                || node.Kind == PyralisAuthoringGraphNodeKind.AssignmentField
                || node.Kind == PyralisAuthoringGraphNodeKind.UnitySurfaceRequirement)
            {
                return true;
            }

            if (node.SourceKind == PyralisAuthoringGraphSourceKind.SceneReadiness
                || node.SourceKind == PyralisAuthoringGraphSourceKind.RuntimeValidation
                || node.SourceKind == PyralisAuthoringGraphSourceKind.Reflection
                || node.SourceKind == PyralisAuthoringGraphSourceKind.SetupFlow)
            {
                return true;
            }

            return false;
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

        public static IReadOnlyList<PyralisAuthoringGraphConnectionRow> BuildDirectProofSupportRows(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringGraphConnectionRow>();

            return BuildProofSupportRows(graph)
                .Where(row => row != null
                    && row.Edge != null
                    && row.Edge.Kind != PyralisAuthoringGraphEdgeKind.BlockedBy
                    && IsRouteProofStepNode(row.From)
                    && IsDirectProofSupport(graph, row))
                .OrderBy(row => GetDirectProofSupportRank(row.From))
                .ThenBy(row => row.FromLabel, StringComparer.Ordinal)
                .ToArray();
        }

        public static IReadOnlyList<PyralisAuthoringGraphConnectionRow> BuildProofBlockerRows(PyralisAuthoringSetupGraph graph)
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

                if (row.Edge.Kind == PyralisAuthoringGraphEdgeKind.BlockedBy
                    && row.From != null
                    && IsResolvedProofNode(row.From))
                {
                    rows.Add(row);
                }
            }

            return rows.ToArray();
        }

        public static IReadOnlyList<PyralisAuthoringGraphConnectionRow> BuildHygieneProofBlockerConnectionRows(PyralisAuthoringSetupGraph graph)
        {
            return BuildProofBlockerRows(graph)
                .Where(row => row != null
                    && row.To != null
                    && !IsMapOwnedReadinessNode(row.To))
                .ToArray();
        }

        public static string BuildIntentFocusSummary(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null || graph.RouteAnalysis == null || graph.RouteAnalysis.CapabilityFamilies.Length == 0)
                return "No intent focus is shaping the graph yet. Open Intent and choose the smallest route you want to prove first.";

            RuntimeCapabilityFamily[] gameplayFamilies = graph.RouteAnalysis.CapabilityFamilies
                .Where(family => family != RuntimeCapabilityFamily.PlatformCore)
                .Distinct()
                .ToArray();
            if (gameplayFamilies.Length == 0)
                return $"{graph.RouteName}: setup foundation only. Open Intent and add the smallest playable ingredient, such as movement, selection/action targeting, or tabletop.";

            string families = string.Join(", ", gameplayFamilies.Select(GetRuntimeFamilyLabel));
            return $"{graph.RouteName}: {families}";
        }

        public static string BuildRouteShapeSummary(PyralisAuthoringSetupGraph graph)
        {
            PyralisAuthoringGraphNode routeShape = FindRouteShapeNode(graph);
            if (routeShape == null)
                return "Route shape has not been compiled yet.";

            string guidance = FirstNonEmpty(BuildRouteFacingMessage(graph, routeShape), routeShape.BlockingReason, routeShape.Guidance);
            return string.IsNullOrWhiteSpace(guidance)
                ? routeShape.Label
                : $"{routeShape.Label}: {guidance}";
        }

        public static string BuildRouteShapeSummary(PyralisAuthoringIntentSelection selection)
        {
            if (selection == null || selection.Capabilities == AuthoringCapability.None)
                return "Route shape: choose one capability ingredient so the graph can decide pawn, no-pawn, or action-surface ownership.";

            RuntimeCapabilityFamily[] families = PyralisAuthoringCapabilityDescriptorRegistry.BuildRuntimeFamilies(
                selection.Capabilities,
                selection.Lane,
                selection.Axioms);
            if (families.Any(family => family == RuntimeCapabilityFamily.CharacterPawnGameplay))
                return "Route shape: participant with pawn. Expect ParticipantDefinition -> PawnDefinition -> pawn prefab, with InputProfile on the participant controlling it.";
            if (families.Any(family => family == RuntimeCapabilityFamily.BoardCardTabletop))
                return "Route shape: participant without pawn. Expect seats, hands, board/card surfaces, cursor, UI, or action resolvers instead of a pawn prefab.";
            if (families.Any(family => family == RuntimeCapabilityFamily.ActionTargeting))
                return "Route shape: participant action surface. Expect an input or UI command surface that sends actions to a resolver.";

            return "Route shape: participant control surface. Wire at least one ParticipantDefinition, then add only the surfaces this intent actually needs.";
        }

        public static PyralisAuthoringGraphNode FindRouteShapeNode(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return null;

            return graph.TryFindNode("route.shape", out PyralisAuthoringGraphNode node) ? node : null;
        }

        public static string BuildFirstProofPrioritySummary(PyralisAuthoringSetupGraph graph)
        {
            PyralisAuthoringGraphNode proof = FindCurrentProofNode(graph);
            if (proof == null)
                return "No first proof yet. Choose one small Intent ingredient so Pyralis can name the first playable test.";
            if (!IsResolvedProofNode(proof))
                return "No first proof target yet. Assign an authored gameplay route or choose one small Intent ingredient so Pyralis can name the first playable test.";

            IReadOnlyList<PyralisAuthoringGraphConnectionRow> blockers = BuildProofBlockerRows(graph);
            if (blockers.Count > 0)
            {
                PyralisAuthoringGraphNode blocker = blockers[0].To;
                string blockerMessage = blocker != null
                    ? FirstNonEmpty(BuildRouteFacingMessage(graph, blocker), blocker.BlockingReason, blocker.Guidance, blocker.Label)
                    : "Clear the first missing setup blocker.";
                return $"{proof.Label}: fix this first - {blockerMessage}";
            }

            return $"{proof.Label}: blockers are clear enough for a narrow Play Mode test.";
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

        public static IReadOnlyList<PyralisAuthoringRouteStepRow> BuildRouteStepRows(PyralisAuthoringSetupGraph graph)
        {
            return BuildRouteWorkingProjection(graph).OrderedSteps;
        }

        public static PyralisAuthoringRouteWorkingProjection BuildRouteWorkingProjection(PyralisAuthoringSetupGraph graph)
        {
            if (!HasResolvedSetupContext(graph))
            {
                return new PyralisAuthoringRouteWorkingProjection(
                    graph != null ? graph.RouteName : "No setup route selected",
                    FindCurrentProofNode(graph),
                    Array.Empty<PyralisAuthoringRouteStepRow>(),
                    Array.Empty<PyralisAuthoringRouteStepRow>(),
                    Array.Empty<PyralisAuthoringRouteStepRow>(),
                    Array.Empty<PyralisAuthoringRouteStepRow>(),
                    Array.Empty<PyralisAuthoringGraphConnectionRow>(),
                    Array.Empty<PyralisAuthoringGraphConnectionRow>());
            }

            PyralisAuthoringRouteStepRow[] criticalPath = BuildRouteCriticalPathRows(graph).ToArray();
            PyralisAuthoringRouteStepRow[] proofEnhancers = BuildRouteProofEnhancerRows(graph).ToArray();
            PyralisAuthoringRouteStepRow[] canWait = BuildRouteCanWaitRows(graph).ToArray();
            PyralisAuthoringGraphNode proof = FindCurrentProofNode(graph);
            PyralisAuthoringRouteStepRow[] orderedSteps = BuildOrderedRouteStepRows(proof, criticalPath, proofEnhancers);
            PyralisAuthoringGraphConnectionRow[] proofBlockers = BuildProofBlockerRows(graph).ToArray();
            PyralisAuthoringGraphConnectionRow[] proofSupport = BuildDirectProofSupportRows(graph).ToArray();

            return new PyralisAuthoringRouteWorkingProjection(
                graph.RouteName,
                proof,
                orderedSteps,
                criticalPath,
                proofEnhancers,
                canWait,
                proofBlockers,
                proofSupport);
        }

        private static PyralisAuthoringRouteStepRow[] BuildOrderedRouteStepRows(
            PyralisAuthoringGraphNode proof,
            IReadOnlyList<PyralisAuthoringRouteStepRow> criticalPath,
            IReadOnlyList<PyralisAuthoringRouteStepRow> proofEnhancers)
        {
            List<PyralisAuthoringRouteStepRow> rows = new List<PyralisAuthoringRouteStepRow>();
            HashSet<string> added = new HashSet<string>(StringComparer.Ordinal);
            AddExistingRouteSteps(rows, added, criticalPath);
            int sequence = rows.Count + 1;

            AddExistingRouteSteps(rows, added, proofEnhancers);
            sequence = rows.Count + 1;
            AddRouteStep(
                rows,
                added,
                proof,
                ref sequence,
                PyralisAuthoringRouteStepPhase.FirstProof,
                PyralisAuthoringRouteStepRole.ProofTarget,
                "This is the Play Mode proof reached after the ordered setup cards above are clear.");

            return rows.ToArray();
        }

        public static IReadOnlyList<PyralisAuthoringRouteStepRow> BuildRouteCriticalPathRows(PyralisAuthoringSetupGraph graph)
        {
            if (!HasResolvedSetupContext(graph))
                return Array.Empty<PyralisAuthoringRouteStepRow>();

            List<PyralisAuthoringRouteStepRow> rows = new List<PyralisAuthoringRouteStepRow>();
            HashSet<string> added = new HashSet<string>(StringComparer.Ordinal);
            int sequence = 1;
            PyralisAuthoringGraphNode currentStep = FindFirstUnresolvedRouteProofNode(graph);

            AddCoreRouteContextSteps(graph, rows, added, currentStep, ref sequence);
            AddReflectedDependencyRouteSteps(graph, rows, added, currentStep, ref sequence);
            AddPrefabReadinessRouteSteps(graph, rows, added, currentStep, ref sequence);
            AddSetupFlowRouteSteps(graph, rows, added, currentStep, ref sequence);
            AddRuntimeValidationRouteSteps(graph, rows, added, currentStep, ref sequence);
            return rows.ToArray();
        }

        public static IReadOnlyList<PyralisAuthoringRouteStepRow> BuildRouteProofEnhancerRows(PyralisAuthoringSetupGraph graph)
        {
            if (!HasResolvedSetupContext(graph))
                return Array.Empty<PyralisAuthoringRouteStepRow>();

            List<PyralisAuthoringRouteStepRow> rows = new List<PyralisAuthoringRouteStepRow>();
            HashSet<string> added = new HashSet<string>(StringComparer.Ordinal);
            int sequence = 1;
            AddProofEnhancerRouteSteps(graph, rows, added, ref sequence);
            return rows.ToArray();
        }

        public static IReadOnlyList<PyralisAuthoringRouteStepRow> BuildRouteCanWaitRows(PyralisAuthoringSetupGraph graph)
        {
            if (!HasResolvedSetupContext(graph))
                return Array.Empty<PyralisAuthoringRouteStepRow>();

            List<PyralisAuthoringRouteStepRow> rows = new List<PyralisAuthoringRouteStepRow>();
            HashSet<string> added = new HashSet<string>(StringComparer.Ordinal);
            int sequence = 1;
            PyralisAuthoringGraphNode[] canWaitNodes = graph.Nodes
                .Where(node => node != null
                    && node.Kind == PyralisAuthoringGraphNodeKind.ValidationEvidence
                    && IsCanWaitRouteSetupCard(node))
                .OrderBy(GetRouteSetupCardRank)
                .ThenBy(node => node.Label, StringComparer.Ordinal)
                .ToArray();

            for (int i = 0; i < canWaitNodes.Length; i++)
            {
                PyralisAuthoringGraphNode node = canWaitNodes[i];
                AddRouteStep(
                    rows,
                    added,
                    node,
                    ref sequence,
                    GetPhase(node),
                    PyralisAuthoringRouteStepRole.CanWait,
                    "This card is useful vocabulary for the route, but it is not part of the first proof's critical path.",
                    graph: graph);
            }

            return rows.ToArray();
        }

        private static void AddSetupFlowRouteSteps(
            PyralisAuthoringSetupGraph graph,
            List<PyralisAuthoringRouteStepRow> rows,
            HashSet<string> added,
            PyralisAuthoringGraphNode currentStep,
            ref int sequence)
        {
            if (graph == null)
                return;

            PyralisAuthoringGraphNode[] setupFlowNodes = graph.Nodes
                .Where(node => node != null
                    && node.Kind == PyralisAuthoringGraphNodeKind.ValidationEvidence
                    && node.SourceKind == PyralisAuthoringGraphSourceKind.SetupFlow
                    && !IsScenePrefabReadinessAggregate(node)
                    && IsCriticalRouteSetupCard(node))
                .OrderBy(GetRouteSetupCardRank)
                .ThenBy(node => node.Label, StringComparer.Ordinal)
                .ToArray();

            for (int i = 0; i < setupFlowNodes.Length; i++)
            {
                PyralisAuthoringGraphNode node = setupFlowNodes[i];
                AddRouteStep(
                    rows,
                    added,
                    node,
                    ref sequence,
                    GetPhase(node),
                    GetRouteSetupCardRole(node, currentStep),
                    GetRouteSetupCardReason(node, currentStep),
                    graph: graph);
            }
        }

        private static void AddCoreRouteContextSteps(
            PyralisAuthoringSetupGraph graph,
            List<PyralisAuthoringRouteStepRow> rows,
            HashSet<string> added,
            PyralisAuthoringGraphNode currentStep,
            ref int sequence)
        {
            AddRouteStepById(graph, rows, added, "bootstrap.root", currentStep, ref sequence);
            AddRouteStepById(graph, rows, added, "session.definition", currentStep, ref sequence);
            AddRouteStepById(graph, rows, added, "mode.definition", currentStep, ref sequence);
            AddRouteStepById(graph, rows, added, "route.shape", currentStep, ref sequence);
            AddRouteStepById(graph, rows, added, "participant.default", currentStep, ref sequence);
            AddRouteStepById(graph, rows, added, "pawn.definition", currentStep, ref sequence);
        }

        private static void AddReflectedDependencyRouteSteps(
            PyralisAuthoringSetupGraph graph,
            List<PyralisAuthoringRouteStepRow> rows,
            HashSet<string> added,
            PyralisAuthoringGraphNode currentStep,
            ref int sequence)
        {
            if (graph == null)
                return;

            PyralisAuthoringGraphNode[] reflectedNodes = graph.Nodes
                .Where(node => node != null
                    && node.SourceKind == PyralisAuthoringGraphSourceKind.Reflection
                    && IsCriticalRouteSetupCard(node))
                .OrderBy(GetRouteSetupCardRank)
                .ThenBy(node => node.Label, StringComparer.Ordinal)
                .ToArray();

            for (int i = 0; i < reflectedNodes.Length; i++)
            {
                PyralisAuthoringGraphNode node = reflectedNodes[i];
                AddRouteStep(
                    rows,
                    added,
                    node,
                    ref sequence,
                    GetPhase(node),
                    GetRouteSetupCardRole(node, currentStep),
                    GetRouteSetupCardReason(node, currentStep),
                    graph: graph);
            }
        }

        private static void AddRuntimeValidationRouteSteps(
            PyralisAuthoringSetupGraph graph,
            List<PyralisAuthoringRouteStepRow> rows,
            HashSet<string> added,
            PyralisAuthoringGraphNode currentStep,
            ref int sequence)
        {
            if (graph == null)
                return;

            PyralisAuthoringGraphNode[] runtimeNodes = graph.Nodes
                .Where(node => node != null
                    && node.Kind == PyralisAuthoringGraphNodeKind.ValidationEvidence
                    && node.SourceKind == PyralisAuthoringGraphSourceKind.RuntimeValidation
                    && IsCriticalRouteSetupCard(node))
                .OrderBy(GetRouteSetupCardRank)
                .ThenBy(node => node.Label, StringComparer.Ordinal)
                .ToArray();

            for (int i = 0; i < runtimeNodes.Length; i++)
            {
                PyralisAuthoringGraphNode node = runtimeNodes[i];
                AddRouteStep(
                    rows,
                    added,
                    node,
                    ref sequence,
                    GetPhase(node),
                    GetRouteSetupCardRole(node, currentStep),
                    GetRouteSetupCardReason(node, currentStep),
                    graph: graph);
            }
        }

        private static void AddPrefabReadinessRouteSteps(
            PyralisAuthoringSetupGraph graph,
            List<PyralisAuthoringRouteStepRow> rows,
            HashSet<string> added,
            PyralisAuthoringGraphNode currentStep,
            ref int sequence)
        {
            if (graph == null)
                return;

            PyralisAuthoringGraphNode[] readinessNodes = graph.Nodes
                .Where(node => node != null
                    && node.Kind == PyralisAuthoringGraphNodeKind.ValidationEvidence
                    && node.SourceKind == PyralisAuthoringGraphSourceKind.SceneReadiness
                    && IsPrefabReadinessRouteCard(node)
                    && IsCriticalRouteSetupCard(node))
                .OrderBy(GetRouteSetupCardRank)
                .ThenBy(node => node.Label, StringComparer.Ordinal)
                .ToArray();

            for (int i = 0; i < readinessNodes.Length; i++)
            {
                PyralisAuthoringGraphNode node = readinessNodes[i];
                AddRouteStep(
                    rows,
                    added,
                    node,
                    ref sequence,
                    GetPhase(node),
                    GetRouteSetupCardRole(node, currentStep),
                    GetRouteSetupCardReason(node, currentStep),
                    graph: graph);
            }
        }

        private static void AddProofEnhancerRouteSteps(
            PyralisAuthoringSetupGraph graph,
            List<PyralisAuthoringRouteStepRow> rows,
            HashSet<string> added,
            ref int sequence)
        {
            if (graph == null)
                return;

            PyralisAuthoringGraphNode[] enhancerNodes = graph.Nodes
                .Where(node => node != null
                    && node.WorkIntent == PyralisAuthoringGraphWorkIntent.ProofEnhancer
                    && node.EvidenceState == PyralisAuthoringGraphEvidenceState.CandidateDetected
                    && IsRouteProofEnhancerNode(node))
                .OrderBy(GetRouteSetupCardRank)
                .ThenBy(node => node.Label, StringComparer.Ordinal)
                .Take(4)
                .ToArray();

            for (int i = 0; i < enhancerNodes.Length; i++)
            {
                PyralisAuthoringGraphNode node = enhancerNodes[i];
                AddRouteStep(
                    rows,
                    added,
                    node,
                    ref sequence,
                    GetPhase(node),
                    PyralisAuthoringRouteStepRole.CanWait,
                    "This proof enhancer can make the first Play Mode check clearer, but it should not block the route proof.",
                    graph: graph);
            }
        }

        private static void AddRouteStepById(
            PyralisAuthoringSetupGraph graph,
            List<PyralisAuthoringRouteStepRow> rows,
            HashSet<string> added,
            string stableId,
            PyralisAuthoringGraphNode currentStep,
            ref int sequence)
        {
            if (graph == null || string.IsNullOrWhiteSpace(stableId))
                return;

            if (!graph.TryFindNode(stableId, out PyralisAuthoringGraphNode node))
                return;

            AddRouteStep(
                rows,
                added,
                node,
                ref sequence,
                GetPhase(node),
                GetRouteSetupCardRole(node, currentStep),
                GetRouteSetupCardReason(node, currentStep),
                graph: graph);
        }

        private static void AddExistingRouteSteps(
            List<PyralisAuthoringRouteStepRow> rows,
            HashSet<string> added,
            IReadOnlyList<PyralisAuthoringRouteStepRow> sourceRows)
        {
            if (rows == null || added == null || sourceRows == null)
                return;

            for (int i = 0; i < sourceRows.Count; i++)
            {
                PyralisAuthoringRouteStepRow row = sourceRows[i];
                if (row == null || row.Node == null || string.IsNullOrWhiteSpace(row.StableId))
                    continue;

                if (!added.Add(row.StableId))
                    continue;

                string displayKey = BuildRouteStepDisplayKey(row);
                if (!string.IsNullOrWhiteSpace(displayKey) && !added.Add(displayKey))
                    continue;

                rows.Add(new PyralisAuthoringRouteStepRow(
                    row.Node,
                    rows.Count + 1,
                    row.Phase,
                    row.Role,
                    row.Reason,
                    row.Edge,
                    row.LabelOverride,
                    row.MessageOverride,
                    row.NativeSetupOverride,
                    row.AssignmentFieldsOverride,
                    row.CustomizationMomentsOverride,
                    row.NativeActionOverride));
            }
        }

        private static PyralisAuthoringGraphNode FindFirstUnresolvedRouteProofNode(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return null;

            PyralisAuthoringGraphNode node = FindFirstUnresolvedSetupFlowNode(graph);
            if (IsRouteProofStepNode(node))
                return node;

            node = FindFirstUnresolvedRouteProofNode(graph, PyralisAuthoringGraphNodeKind.SetupChain);
            if (node != null)
                return node;

            node = FindFirstUnresolvedRouteProofNode(graph, PyralisAuthoringGraphNodeKind.AssignmentField);
            if (node != null)
                return node;

            node = FindFirstUnresolvedRouteProofNode(graph, PyralisAuthoringGraphNodeKind.UnitySurfaceRequirement);
            if (node != null)
                return node;

            node = FindFirstUnresolvedRouteProofNode(graph, PyralisAuthoringGraphNodeKind.RouteShape);
            if (node != null)
                return node;

            return FindFirstUnresolvedRouteProofNode(graph, PyralisAuthoringGraphNodeKind.ValidationEvidence);
        }

        private static PyralisAuthoringGraphNode FindFirstUnresolvedRouteProofNode(
            PyralisAuthoringSetupGraph graph,
            PyralisAuthoringGraphNodeKind kind)
        {
            IReadOnlyList<PyralisAuthoringGraphNode> nodes = graph.FindNodes(kind);
            for (int i = 0; i < nodes.Count; i++)
            {
                PyralisAuthoringGraphNode node = nodes[i];
                if (!IsRouteProofStepNode(node))
                    continue;

                if (node.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked
                    || node.EvidenceState == PyralisAuthoringGraphEvidenceState.Missing)
                {
                    return node;
                }
            }

            return null;
        }

        private static bool IsRouteProofStepNode(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return false;

            if (node.Kind == PyralisAuthoringGraphNodeKind.SceneSurface
                || node.SourceKind == PyralisAuthoringGraphSourceKind.SceneReadiness)
            {
                return false;
            }

            return true;
        }

        private static void AddRouteContextRows(
            PyralisAuthoringSetupGraph graph,
            List<PyralisAuthoringRouteStepRow> rows,
            HashSet<string> added,
            ref int sequence)
        {
            if (graph == null)
                return;

            AddRouteStep(
                rows,
                added,
                FindRouteShapeNode(graph),
                ref sequence,
                PyralisAuthoringRouteStepPhase.SetupChain,
                PyralisAuthoringRouteStepRole.RouteContext,
                "This is the participant ownership shape compiled from intent, reflected setup, and route evidence.",
                graph: graph);

            PyralisAuthoringGraphNode proof = FindCurrentProofNode(graph);
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

                if (!IsDirectProofNode(graph, node, proof.StableId))
                    continue;

                AddRouteStep(
                    rows,
                    added,
                    node,
                    ref sequence,
                    PyralisAuthoringRouteStepPhase.Capability,
                    PyralisAuthoringRouteStepRole.RouteContext,
                    "This reflected contract matches the first proof target.",
                    graph: graph);
            }
        }

        private static bool IsCriticalRouteSetupCard(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return false;

            if (node.Kind == PyralisAuthoringGraphNodeKind.ValidationEvidence
                && node.EvidenceState == PyralisAuthoringGraphEvidenceState.Ready)
            {
                return false;
            }

            if (IsCanWaitRouteSetupCard(node))
                return false;

            if (node.EvidenceState == PyralisAuthoringGraphEvidenceState.CandidateDetected)
                return false;

            string id = node.StableId ?? string.Empty;
            if (id.Contains("setup-gameplay-root"))
                return false;

            return node.WorkIntent == PyralisAuthoringGraphWorkIntent.RequiredSetup
                || node.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked
                || node.EvidenceState == PyralisAuthoringGraphEvidenceState.Missing;
        }

        private static bool IsScenePrefabReadinessAggregate(PyralisAuthoringGraphNode node)
        {
            return string.Equals(node?.StableId, "setupflow.setup-scene-prefab-readiness", StringComparison.Ordinal)
                || string.Equals(node?.Label, "Scene And Prefab Readiness", StringComparison.Ordinal);
        }

        private static bool IsPrefabReadinessRouteCard(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return false;

            string combined = string.Join(
                " ",
                node.StableId ?? string.Empty,
                node.Label ?? string.Empty,
                node.Guidance ?? string.Empty,
                node.BlockingReason ?? string.Empty);

            if (ContainsIgnoreCase(combined, "SceneRoot")
                || ContainsIgnoreCase(combined, "Environment")
                || ContainsIgnoreCase(combined, "Playfield")
                || ContainsIgnoreCase(combined, "Canvas")
                || ContainsIgnoreCase(combined, "HUD")
                || ContainsIgnoreCase(combined, "Pickups")
                || ContainsIgnoreCase(combined, "Hazards")
                || ContainsIgnoreCase(combined, "Enemies"))
            {
                return false;
            }

            return ContainsIgnoreCase(combined, "pawn prefab")
                || ContainsIgnoreCase(combined, "IPawnMotor")
                || ContainsIgnoreCase(combined, "IPawnInputModule")
                || ContainsIgnoreCase(combined, "IPawnPresentationModule");
        }

        private static bool IsRouteProofEnhancerNode(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return false;

            if (node.SourceKind == PyralisAuthoringGraphSourceKind.AuthoringContract
                || node.SourceOrigin == PyralisAuthoringGraphSourceOrigin.Contract)
            {
                return false;
            }

            return node.SourceKind == PyralisAuthoringGraphSourceKind.SetupFlow
                || node.SourceKind == PyralisAuthoringGraphSourceKind.RuntimeValidation
                || node.SourceKind == PyralisAuthoringGraphSourceKind.SceneReadiness;
        }

        private static bool IsCanWaitRouteSetupCard(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return false;

            if (node.EvidenceState == PyralisAuthoringGraphEvidenceState.Optional)
                return true;

            if (node.WorkIntent == PyralisAuthoringGraphWorkIntent.Optional
                || node.WorkIntent == PyralisAuthoringGraphWorkIntent.FeatureCard)
            {
                return true;
            }

            if (node.WorkIntent == PyralisAuthoringGraphWorkIntent.ProofEnhancer)
                return false;

            string id = node.StableId ?? string.Empty;
            string label = node.Label ?? string.Empty;
            string combined = id + " " + label;
            if (ContainsIgnoreCase(combined, "scoring")
                || ContainsIgnoreCase(combined, "score")
                || ContainsIgnoreCase(combined, "tabletop")
                || ContainsIgnoreCase(combined, "settings")
                || ContainsIgnoreCase(combined, "playfield"))
            {
                return node.EvidenceState != PyralisAuthoringGraphEvidenceState.Missing
                    && node.EvidenceState != PyralisAuthoringGraphEvidenceState.Blocked;
            }

            return node.EvidenceState == PyralisAuthoringGraphEvidenceState.CandidateDetected
                && node.SourceKind == PyralisAuthoringGraphSourceKind.SetupFlow;
        }

        private static PyralisAuthoringRouteStepRole GetRouteSetupCardRole(
            PyralisAuthoringGraphNode node,
            PyralisAuthoringGraphNode currentStep)
        {
            if (node == null)
                return PyralisAuthoringRouteStepRole.RouteContext;

            if (string.Equals(node.StableId, "route.shape", StringComparison.Ordinal))
                return PyralisAuthoringRouteStepRole.RouteContext;

            if (currentStep != null && string.Equals(node.StableId, currentStep.StableId, StringComparison.Ordinal))
                return PyralisAuthoringRouteStepRole.DoThisFirst;

            if (node.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked
                || node.EvidenceState == PyralisAuthoringGraphEvidenceState.Missing)
            {
                return PyralisAuthoringRouteStepRole.BlocksProof;
            }

            if (node.WorkIntent == PyralisAuthoringGraphWorkIntent.ProofEnhancer
                || node.WorkIntent == PyralisAuthoringGraphWorkIntent.Optional
                || node.WorkIntent == PyralisAuthoringGraphWorkIntent.FeatureCard)
            {
                return PyralisAuthoringRouteStepRole.CanWait;
            }

            return PyralisAuthoringRouteStepRole.RouteContext;
        }

        private static string GetRouteSetupCardReason(
            PyralisAuthoringGraphNode node,
            PyralisAuthoringGraphNode currentStep)
        {
            if (node == null)
                return "This setup card belongs to the selected proof route.";

            if (string.Equals(node.StableId, "route.shape", StringComparison.Ordinal))
                return "This is the route ownership shape compiled from intent, reflected setup, and current evidence.";

            if (currentStep != null && string.Equals(node.StableId, currentStep.StableId, StringComparison.Ordinal))
                return "This is the next setup card to clear on the fresh-scene path.";

            if (node.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked
                || node.EvidenceState == PyralisAuthoringGraphEvidenceState.Missing)
            {
                return "This setup card must be cleared before the first proof is believable.";
            }

            if (node.WorkIntent == PyralisAuthoringGraphWorkIntent.ProofEnhancer
                || node.EvidenceState == PyralisAuthoringGraphEvidenceState.CandidateDetected)
            {
                return "This card can make the first proof easier to judge, but it can wait until required setup is clear.";
            }

            return "This setup card is already satisfied on the route toward the first proof.";
        }

        private static int GetRouteSetupCardRank(PyralisAuthoringGraphNode node)
        {
            string key = ((node?.StableId ?? string.Empty) + " " + (node?.Label ?? string.Empty)).ToLowerInvariant();

            if (key.Contains("bootstrap") || key.Contains("gameplay root") || key.Contains("lifetime scope"))
                return 0;
            if (key.Contains("session"))
                return 10;
            if (key.Contains("game mode") || key.Contains("gamemode") || key.Contains("rules"))
                return 20;
            if (key.Contains("capabilit") || key.Contains("route"))
                return 30;
            if (key.Contains("participant") && !key.Contains("pawn"))
                return 40;
            if (key.Contains("input") || key.Contains("player input"))
                return 50;
            if (key.Contains("pawn") || key.Contains("prefab"))
                return 60;
            if (key.Contains("spawn"))
                return 70;
            if (key.Contains("camera"))
                return 80;
            if (key.Contains("presentation") || key.Contains("animation") || key.Contains("visual"))
                return 90;
            if (key.Contains("movement") || key.Contains("physics") || key.Contains("collision"))
                return 100;
            if (key.Contains("scene") || key.Contains("readiness"))
                return 110;
            if (key.Contains("hud") || key.Contains("ui"))
                return 120;
            if (key.Contains("settings"))
                return 130;

            return 200;
        }

        private static void AddRouteStep(
            List<PyralisAuthoringRouteStepRow> rows,
            HashSet<string> added,
            PyralisAuthoringGraphNode node,
            ref int sequence,
            PyralisAuthoringRouteStepPhase phase,
            PyralisAuthoringRouteStepRole role,
            string reason,
            PyralisAuthoringGraphEdge edge = null,
            PyralisAuthoringSetupGraph graph = null)
        {
            if (rows == null || added == null || node == null || string.IsNullOrWhiteSpace(node.StableId))
                return;

            if (!added.Add(node.StableId))
                return;

            PyralisRouteStepLens lens = BuildRouteStepLens(graph, node);
            string displayKey = BuildRouteStepDisplayKey(node, lens);
            if (!string.IsNullOrWhiteSpace(displayKey) && !added.Add(displayKey))
                return;

            rows.Add(new PyralisAuthoringRouteStepRow(
                node,
                sequence++,
                phase,
                role,
                reason,
                edge,
                lens.LabelOverride,
                lens.MessageOverride,
                lens.NativeSetupOverride,
                lens.AssignmentFieldsOverride,
                lens.CustomizationMomentsOverride,
                lens.NativeActionOverride));
        }

        private static string BuildRouteStepDisplayKey(PyralisAuthoringRouteStepRow row)
        {
            if (row == null)
                return string.Empty;

            return "route-display:"
                + NormalizeRouteStepKey(row.Label)
                + "|"
                + NormalizeRouteStepKey(row.Message);
        }

        private static string BuildRouteStepDisplayKey(PyralisAuthoringGraphNode node, PyralisRouteStepLens lens)
        {
            string label = !string.IsNullOrWhiteSpace(lens.LabelOverride)
                ? lens.LabelOverride
                : node?.Label ?? string.Empty;
            string message = !string.IsNullOrWhiteSpace(lens.MessageOverride)
                ? lens.MessageOverride
                : node?.Guidance ?? node?.BlockingReason ?? string.Empty;

            if (string.IsNullOrWhiteSpace(label) && string.IsNullOrWhiteSpace(message))
                return string.Empty;

            return "route-display:"
                + NormalizeRouteStepKey(label)
                + "|"
                + NormalizeRouteStepKey(message);
        }

        private static string NormalizeRouteStepKey(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToLowerInvariant();
        }

        private static PyralisRouteStepLens BuildRouteStepLens(
            PyralisAuthoringSetupGraph graph,
            PyralisAuthoringGraphNode node)
        {
            if (node != null && string.Equals(node.StableId, "route.shape", StringComparison.Ordinal))
                return PyralisRouteStepLens.Empty;

            PyralisParticipantPawnIssueKind issueKind = ResolvePawnRouteIssueKind(graph, node);
            if (issueKind == PyralisParticipantPawnIssueKind.None
                || issueKind == PyralisParticipantPawnIssueKind.MissingParticipants
                || issueKind == PyralisParticipantPawnIssueKind.EmptyParticipantSlot
                || issueKind == PyralisParticipantPawnIssueKind.PawnValidation)
            {
                return PyralisRouteStepLens.Empty;
            }

            RuntimeCapabilityLaneTag laneTag = ResolvePresentationLane(graph);
            PyralisAuthoringNativeAction nativeAction = PyralisSetupFlowGuidance.GetPawnNativeAction(issueKind, laneTag);
            string label = GetPawnRouteStepLabel(issueKind, laneTag);
            string message = GetPawnRouteStepMessage(issueKind, laneTag, nativeAction);
            string[] nativeSetup = string.IsNullOrWhiteSpace(message)
                ? Array.Empty<string>()
                : new[] { message };
            string[] assignmentFields = GetPawnRouteStepAssignmentFields(issueKind);

            return new PyralisRouteStepLens(
                label,
                message,
                nativeSetup,
                assignmentFields,
                node != null ? node.CustomizationMoments : Array.Empty<string>(),
                nativeAction);
        }

        private static string BuildRouteFacingMessage(
            PyralisAuthoringSetupGraph graph,
            PyralisAuthoringGraphNode node)
        {
            if (graph?.RouteAnalysis == null || node == null)
                return string.Empty;

            bool isRouteShape = string.Equals(node.StableId, "route.shape", StringComparison.Ordinal);
            bool isPawnDefinition = string.Equals(node.StableId, "pawn.definition", StringComparison.Ordinal);
            if (isRouteShape)
                return string.Empty;

            if (!isPawnDefinition)
                return string.Empty;

            PyralisParticipantPawnIssueKind issueKind = graph.RouteAnalysis.ParticipantPawnIssueKind;
            if (issueKind == PyralisParticipantPawnIssueKind.None
                || issueKind == PyralisParticipantPawnIssueKind.MissingParticipants
                || issueKind == PyralisParticipantPawnIssueKind.EmptyParticipantSlot
                || issueKind == PyralisParticipantPawnIssueKind.PawnValidation)
            {
                return string.Empty;
            }

            RuntimeCapabilityLaneTag laneTag = ResolvePresentationLane(graph);
            PyralisAuthoringNativeAction nativeAction = PyralisSetupFlowGuidance.GetPawnNativeAction(issueKind, laneTag);
            return GetPawnRouteStepMessage(issueKind, laneTag, nativeAction);
        }

        private static PyralisParticipantPawnIssueKind ResolvePawnRouteIssueKind(
            PyralisAuthoringSetupGraph graph,
            PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return PyralisParticipantPawnIssueKind.None;

            if (string.Equals(node.StableId, "route.shape", StringComparison.Ordinal))
                return PyralisParticipantPawnIssueKind.None;

            if (string.Equals(node.StableId, "pawn.definition", StringComparison.Ordinal)
                && graph?.RouteAnalysis != null
                && graph.RouteAnalysis.ParticipantPawnIssueKind != PyralisParticipantPawnIssueKind.None)
            {
                return graph.RouteAnalysis.ParticipantPawnIssueKind;
            }

            string nativeAction = node.NativeAction.HasValue
                ? node.NativeAction.Value.ToGuidanceSentence()
                : string.Empty;
            string combined = string.Join(
                " ",
                node.Label ?? string.Empty,
                node.Guidance ?? string.Empty,
                node.BlockingReason ?? string.Empty,
                nativeAction,
                string.Join(" ", node.NativeSetup ?? Array.Empty<string>()),
                string.Join(" ", node.AssignmentFields ?? Array.Empty<string>()));

            return PyralisSetupFlowGuidance.InferPawnIssueKind(combined);
        }

        private static string GetPawnRouteStepLabel(
            PyralisParticipantPawnIssueKind issueKind,
            RuntimeCapabilityLaneTag laneTag)
        {
            switch (issueKind)
            {
                case PyralisParticipantPawnIssueKind.MissingPawnDefinition:
                    return "Assign Participant Pawn";
                case PyralisParticipantPawnIssueKind.MissingPawnPrefab:
                    return "Create Pawn Prefab";
                case PyralisParticipantPawnIssueKind.MissingPawnRoot:
                    return "Add Pawn Root";
                case PyralisParticipantPawnIssueKind.MissingMotor:
                    return laneTag == RuntimeCapabilityLaneTag.Sprite2D ? "Add Motor2D" : "Add Lane Motor";
                case PyralisParticipantPawnIssueKind.MissingPresentation:
                    return laneTag == RuntimeCapabilityLaneTag.Sprite2D ? "Wire 2D Presentation" : "Wire Pawn Presentation";
                case PyralisParticipantPawnIssueKind.MissingInputModule:
                    return laneTag == RuntimeCapabilityLaneTag.Sprite2D ? "Add Motor2D Input Adapter" : "Add Pawn Input Module";
                default:
                    return string.Empty;
            }
        }

        private static string GetPawnRouteStepMessage(
            PyralisParticipantPawnIssueKind issueKind,
            RuntimeCapabilityLaneTag laneTag,
            PyralisAuthoringNativeAction nativeAction)
        {
            switch (issueKind)
            {
                case PyralisParticipantPawnIssueKind.MissingPawnDefinition:
                    return "Create or assign a PawnDefinition, then assign it to ParticipantDefinition.defaultPawn for the participant who will control the pawn.";
                case PyralisParticipantPawnIssueKind.MissingPawnPrefab:
                    return nativeAction.ToGuidanceSentence();
                case PyralisParticipantPawnIssueKind.MissingPawnRoot:
                    return "Add PawnRoot to the pawn prefab root so Pyralis can bind participant, profiles, input, camera target, and runtime services to this actor.";
                case PyralisParticipantPawnIssueKind.MissingMotor:
                    if (laneTag == RuntimeCapabilityLaneTag.Sprite2D)
                        return "Add Motor2D to the pawn prefab root. Unity will add Pawn2DMovementComponent and Pawn2DPresentationComponent; then assign the movement profile and presentation/animation profiles on those sibling components.";
                    return "Add the lane motor component to the pawn prefab root, then assign the movement profile fields it exposes in the Inspector.";
                case PyralisParticipantPawnIssueKind.MissingPresentation:
                    if (laneTag == RuntimeCapabilityLaneTag.Sprite2D)
                        return "Wire Pawn2DPresentationComponent on the pawn prefab root or visual child, then assign the SpriteRenderer, Animator, and presentation/animation profiles used by this pawn.";
                    return "Wire the lane presentation component on the pawn prefab or visual child, then assign the renderer, animator, and presentation profile fields it exposes.";
                case PyralisParticipantPawnIssueKind.MissingInputModule:
                    if (laneTag == RuntimeCapabilityLaneTag.Sprite2D)
                        return "Add Motor2DInputAdapter to the pawn prefab root so the participant InputProfile can push movement, jump, dash, and action values into Motor2D.";
                    return "Add the lane input module to the pawn prefab root so the participant InputProfile can reach the pawn motor.";
                default:
                    return nativeAction.ToGuidanceSentence();
            }
        }

        private static string[] GetPawnRouteStepAssignmentFields(PyralisParticipantPawnIssueKind issueKind)
        {
            switch (issueKind)
            {
                case PyralisParticipantPawnIssueKind.MissingPawnDefinition:
                    return new[] { "ParticipantDefinition.defaultPawn" };
                case PyralisParticipantPawnIssueKind.MissingPawnPrefab:
                    return new[] { "PawnDefinition.pawnPrefab" };
                case PyralisParticipantPawnIssueKind.MissingMotor:
                    return new[] { "Pawn prefab root.Add Component" };
                case PyralisParticipantPawnIssueKind.MissingPresentation:
                    return new[] { "Pawn2DPresentationComponent.spriteRenderer", "Pawn2DPresentationComponent.animator" };
                case PyralisParticipantPawnIssueKind.MissingInputModule:
                    return new[] { "Pawn prefab root.Add Component" };
                default:
                    return Array.Empty<string>();
            }
        }

        private static bool IsDirectProofSupport(PyralisAuthoringSetupGraph graph, PyralisAuthoringGraphConnectionRow row)
        {
            PyralisAuthoringGraphNode node = row?.From;
            PyralisAuthoringGraphNode proof = row?.To != null && row.To.Kind == PyralisAuthoringGraphNodeKind.Proof
                ? row.To
                : FindCurrentProofNode(graph);
            if (node == null || proof == null)
                return false;

            return IsDirectProofNode(graph, node, proof.StableId);
        }

        private static bool IsDirectProofNode(PyralisAuthoringSetupGraph graph, PyralisAuthoringGraphNode node, string proofTargetId)
        {
            if (node == null || string.IsNullOrWhiteSpace(proofTargetId))
                return false;

            if (node.Kind == PyralisAuthoringGraphNodeKind.Capability)
                return IsDirectProofCapability(proofTargetId, node.CapabilityFamily);

            if (node.Kind != PyralisAuthoringGraphNodeKind.Contract && node.SourceContract == null)
                return false;

            if (!string.Equals(node.ProofTargetId, proofTargetId, StringComparison.Ordinal))
                return false;

            if (IsPresentationLaneMismatch(graph, node))
                return false;

            if (string.Equals(proofTargetId, "proof.1p-pawn-movement", StringComparison.Ordinal)
                && node.Kind == PyralisAuthoringGraphNodeKind.Contract)
            {
                return IsMovementProofContractName(proofTargetId, node.StableId, node.Label);
            }

            return IsDirectProofCapability(proofTargetId, node.CapabilityFamily)
                || IsDirectProofCapability(proofTargetId, node.AuthoringCapability)
                || IsMovementProofContractName(proofTargetId, node.StableId, node.Label);
        }

        private static bool IsDirectProofCapability(string proofTargetId, RuntimeCapabilityFamily family)
        {
            if (string.Equals(proofTargetId, "proof.1p-pawn-movement", StringComparison.Ordinal))
            {
                return family == RuntimeCapabilityFamily.PlatformCore
                    || family == RuntimeCapabilityFamily.CharacterPawnGameplay
                    || family == RuntimeCapabilityFamily.AnimationPresentation;
            }

            if (string.Equals(proofTargetId, "proof.board-card-action", StringComparison.Ordinal))
            {
                return family == RuntimeCapabilityFamily.PlatformCore
                    || family == RuntimeCapabilityFamily.BoardCardTabletop
                    || family == RuntimeCapabilityFamily.ActionTargeting
                    || family == RuntimeCapabilityFamily.CameraInput
                    || family == RuntimeCapabilityFamily.ScoringObjectives;
            }

            if (string.Equals(proofTargetId, "proof.action-selection", StringComparison.Ordinal))
            {
                return family == RuntimeCapabilityFamily.PlatformCore
                    || family == RuntimeCapabilityFamily.ActionTargeting
                    || family == RuntimeCapabilityFamily.CameraInput
                    || family == RuntimeCapabilityFamily.BoardCardTabletop;
            }

            if (string.Equals(proofTargetId, "proof.npc-enemy-behavior", StringComparison.Ordinal))
            {
                return family == RuntimeCapabilityFamily.PlatformCore
                    || family == RuntimeCapabilityFamily.CharacterPawnGameplay
                    || family == RuntimeCapabilityFamily.Combat
                    || family == RuntimeCapabilityFamily.AnimationPresentation;
            }

            if (string.Equals(proofTargetId, "proof.ui-hud-menu", StringComparison.Ordinal))
            {
                return family == RuntimeCapabilityFamily.PlatformCore
                    || family == RuntimeCapabilityFamily.ScoringObjectives
                    || family == RuntimeCapabilityFamily.ActionTargeting;
            }

            if (string.Equals(proofTargetId, "proof.generated-content", StringComparison.Ordinal))
            {
                return family == RuntimeCapabilityFamily.PlatformCore
                    || family == RuntimeCapabilityFamily.ProceduralGeneration;
            }

            if (string.Equals(proofTargetId, "proof.network-ownership", StringComparison.Ordinal))
            {
                return family == RuntimeCapabilityFamily.PlatformCore
                    || family == RuntimeCapabilityFamily.Networking;
            }

            return family == RuntimeCapabilityFamily.PlatformCore
                || family == RuntimeCapabilityFamily.Custom;
        }

        private static bool IsDirectProofCapability(string proofTargetId, AuthoringCapability capability)
        {
            if (capability == AuthoringCapability.None)
                return false;

            if (string.Equals(proofTargetId, "proof.1p-pawn-movement", StringComparison.Ordinal))
            {
                AuthoringCapability direct = AuthoringCapability.Setup
                    | AuthoringCapability.Session
                    | AuthoringCapability.Participants
                    | AuthoringCapability.Input
                    | AuthoringCapability.Movement
                    | AuthoringCapability.KineticMotor2D
                    | AuthoringCapability.KineticMotor3D
                    | AuthoringCapability.Animation;
                return (capability & direct) != 0;
            }

            return true;
        }

        private static int GetDirectProofSupportRank(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return 100;

            return node.Kind switch
            {
                PyralisAuthoringGraphNodeKind.Capability when node.CapabilityFamily == RuntimeCapabilityFamily.PlatformCore => 0,
                PyralisAuthoringGraphNodeKind.Capability when node.CapabilityFamily == RuntimeCapabilityFamily.CharacterPawnGameplay => 1,
                PyralisAuthoringGraphNodeKind.Contract when IsMovementOrInputLabel(node) => 2,
                PyralisAuthoringGraphNodeKind.Contract when IsPresentationLabel(node) => 3,
                PyralisAuthoringGraphNodeKind.Capability when node.CapabilityFamily == RuntimeCapabilityFamily.AnimationPresentation => 4,
                PyralisAuthoringGraphNodeKind.Capability when node.CapabilityFamily == RuntimeCapabilityFamily.CameraInput => 5,
                PyralisAuthoringGraphNodeKind.Capability => 8,
                _ => 10
            };
        }

        private static bool IsPresentationLaneMismatch(PyralisAuthoringSetupGraph graph, PyralisAuthoringGraphNode node)
        {
            string stableId = node?.StableId ?? string.Empty;
            string label = node?.Label ?? string.Empty;
            if (string.IsNullOrWhiteSpace(stableId) && string.IsNullOrWhiteSpace(label))
                return false;

            RuntimeCapabilityLaneTag lane = ResolvePresentationLane(graph);
            if (lane == RuntimeCapabilityLaneTag.Sprite2D)
                return ContainsIgnoreCase(stableId, "3d") || ContainsIgnoreCase(label, "3d");

            if (lane == RuntimeCapabilityLaneTag.ThirdPerson3D)
                return ContainsIgnoreCase(stableId, "2d") || ContainsIgnoreCase(label, "2d");

            return false;
        }

        private static RuntimeCapabilityLaneTag ResolvePresentationLane(PyralisAuthoringSetupGraph graph)
        {
            GameObject pawnPrefab = graph?.RouteAnalysis?.Pawn != null ? graph.RouteAnalysis.Pawn.pawnPrefab : null;
            if (pawnPrefab != null)
            {
                if (pawnPrefab.GetComponentInChildren<NeonBlack.Gameplay.Features.Characters.Pawn2DMovementComponent>(true) != null
                    || pawnPrefab.GetComponentInChildren<NeonBlack.Gameplay.Features.Characters.Pawn2DPresentationComponent>(true) != null
                    || pawnPrefab.GetComponentInChildren<Rigidbody2D>(true) != null)
                {
                    return RuntimeCapabilityLaneTag.Sprite2D;
                }

                if (pawnPrefab.GetComponentInChildren<Pawn3DMovementComponent>(true) != null
                    || pawnPrefab.GetComponentInChildren<Pawn3DPresentationComponent>(true) != null
                    || pawnPrefab.GetComponentInChildren<CharacterController>(true) != null)
                {
                    return RuntimeCapabilityLaneTag.ThirdPerson3D;
                }
            }

            if (graph?.IntentSelection != null && graph.IntentSelection.Lane != RuntimeCapabilityLaneTag.Mixed)
                return graph.IntentSelection.Lane;

            if (graph?.RouteAnalysis?.RouteFacts == null)
                return RuntimeCapabilityLaneTag.Mixed;

            for (int i = 0; i < graph.RouteAnalysis.RouteFacts.Length; i++)
            {
                string label = graph.RouteAnalysis.RouteFacts[i]?.Label ?? string.Empty;
                if (ContainsIgnoreCase(label, "2d"))
                    return RuntimeCapabilityLaneTag.Sprite2D;
                if (ContainsIgnoreCase(label, "3d"))
                    return RuntimeCapabilityLaneTag.ThirdPerson3D;
            }

            return RuntimeCapabilityLaneTag.Mixed;
        }

        private static bool IsMovementProofContractName(string proofTargetId, string stableId, string label)
        {
            if (!string.Equals(proofTargetId, "proof.1p-pawn-movement", StringComparison.Ordinal))
                return false;

            string combined = (stableId ?? string.Empty) + " " + (label ?? string.Empty);
            if (ContainsIgnoreCase(combined, "combat")
                || ContainsIgnoreCase(combined, "projectile")
                || ContainsIgnoreCase(combined, "pickup")
                || ContainsIgnoreCase(combined, "status")
                || ContainsIgnoreCase(combined, "network"))
            {
                return false;
            }

            return ContainsIgnoreCase(combined, "movement")
                || ContainsIgnoreCase(combined, "motor")
                || ContainsIgnoreCase(combined, "input")
                || ContainsIgnoreCase(combined, "presentation")
                || ContainsIgnoreCase(combined, "animation")
                || ContainsIgnoreCase(combined, "motor2dinputadapter")
                || ContainsIgnoreCase(combined, "pawn2dmovementcomponent")
                || ContainsIgnoreCase(combined, "pawn2dpresentationcomponent")
                || ContainsIgnoreCase(combined, "pawn3dmovementcomponent")
                || ContainsIgnoreCase(combined, "pawn3dinputmodule")
                || ContainsIgnoreCase(combined, "pawn3dpresentationcomponent");
        }

        private static bool IsMovementOrInputLabel(PyralisAuthoringGraphNode node)
        {
            string combined = (node?.StableId ?? string.Empty) + " " + (node?.Label ?? string.Empty);
            return ContainsIgnoreCase(combined, "movement")
                || ContainsIgnoreCase(combined, "motor")
                || ContainsIgnoreCase(combined, "input")
                || ContainsIgnoreCase(combined, "traversal")
                || ContainsIgnoreCase(combined, "hop");
        }

        private static bool IsPresentationLabel(PyralisAuthoringGraphNode node)
        {
            string combined = (node?.StableId ?? string.Empty) + " " + (node?.Label ?? string.Empty);
            return ContainsIgnoreCase(combined, "presentation")
                || ContainsIgnoreCase(combined, "animation")
                || ContainsIgnoreCase(combined, "camera");
        }

        private static bool ContainsIgnoreCase(string source, string value)
        {
            return !string.IsNullOrWhiteSpace(source)
                && !string.IsNullOrWhiteSpace(value)
                && source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static PyralisAuthoringRouteStepPhase GetPhase(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return PyralisAuthoringRouteStepPhase.Reference;

            if (node.WorkIntent == PyralisAuthoringGraphWorkIntent.Optional)
                return PyralisAuthoringRouteStepPhase.Optional;

            return node.Kind switch
            {
                PyralisAuthoringGraphNodeKind.SetupChain when string.Equals(node.StableId, "bootstrap.root", StringComparison.Ordinal) => PyralisAuthoringRouteStepPhase.Foundation,
                PyralisAuthoringGraphNodeKind.SetupChain => PyralisAuthoringRouteStepPhase.SetupChain,
                PyralisAuthoringGraphNodeKind.AssignmentField => PyralisAuthoringRouteStepPhase.SetupChain,
                PyralisAuthoringGraphNodeKind.RouteShape => PyralisAuthoringRouteStepPhase.SetupChain,
                PyralisAuthoringGraphNodeKind.Capability => PyralisAuthoringRouteStepPhase.Capability,
                PyralisAuthoringGraphNodeKind.Contract => PyralisAuthoringRouteStepPhase.Capability,
                PyralisAuthoringGraphNodeKind.Proof => PyralisAuthoringRouteStepPhase.FirstProof,
                PyralisAuthoringGraphNodeKind.SceneSurface => PyralisAuthoringRouteStepPhase.SceneEvidence,
                PyralisAuthoringGraphNodeKind.UnitySurfaceRequirement => PyralisAuthoringRouteStepPhase.SceneEvidence,
                PyralisAuthoringGraphNodeKind.ValidationEvidence => PyralisAuthoringRouteStepPhase.Validation,
                _ => PyralisAuthoringRouteStepPhase.Reference
            };
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

            node = FindFirstUnresolvedNode(graph, PyralisAuthoringGraphNodeKind.AssignmentField);
            if (node != null)
                return node;

            node = FindFirstUnresolvedNode(graph, PyralisAuthoringGraphNodeKind.UnitySurfaceRequirement);
            if (node != null)
                return node;

            node = FindFirstUnresolvedNode(graph, PyralisAuthoringGraphNodeKind.RouteShape);
            if (node != null)
                return node;

            node = FindFirstUnresolvedNode(graph, PyralisAuthoringGraphNodeKind.ValidationEvidence);
            if (node != null)
                return node;

            return FindFirstUnresolvedNode(graph, PyralisAuthoringGraphNodeKind.SceneSurface);
        }

        public static PyralisAuthoringCurrentStepGraphRow BuildCurrentStepRow(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return BuildFallbackCurrentStepRow();

            PyralisAuthoringRouteWorkingProjection route = BuildRouteWorkingProjection(graph);
            PyralisAuthoringRouteStepRow currentAction = route.CurrentAction;
            if (currentAction != null && currentAction.Node != null)
            {
                PyralisAuthoringGraphNode routeNode = currentAction.Node;
                string routeMessage = !string.IsNullOrWhiteSpace(currentAction.UnityActionLabel)
                    ? currentAction.UnityActionLabel
                    : !string.IsNullOrWhiteSpace(currentAction.Message)
                        ? currentAction.Message
                        : !string.IsNullOrWhiteSpace(currentAction.FirstProof)
                            ? currentAction.FirstProof
                            : "Inspect this route step and clear the missing setup evidence.";

                string routeDetail = !string.IsNullOrWhiteSpace(currentAction.Reason)
                    ? currentAction.Reason
                    : GetCurrentStepDetail(routeNode);

                return new PyralisAuthoringCurrentStepGraphRow(
                    route.RouteName,
                    routeNode,
                    routeMessage,
                    routeDetail,
                    routeNode.SourceObject,
                    currentAction.NativeAction);
            }

            if (route.ReadyForFirstProof && route.Proof != null)
            {
                string proofMessage = route.Proof.NativeSetup.Length > 0
                    ? route.Proof.NativeSetup[0]
                    : route.Proof.Guidance;
                if (string.IsNullOrWhiteSpace(proofMessage))
                    proofMessage = "Enter Play Mode and run the first playable proof.";

                string proofDetail = !string.IsNullOrWhiteSpace(route.Proof.BlockingReason)
                    ? route.Proof.BlockingReason
                    : "The graph has no required setup blockers for the selected first proof.";

                return new PyralisAuthoringCurrentStepGraphRow(route.RouteName, route.Proof, proofMessage, proofDetail, route.Proof.SourceObject);
            }

            PyralisAuthoringGraphNode node = FindFirstUnresolvedNode(graph);

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

        public static IReadOnlyList<PyralisAuthoringGraphAuditRow> BuildReadinessAuditRows(PyralisAuthoringSetupGraph graph, PyralisAuthoringGraphEvidenceState evidenceState)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringGraphAuditRow>();

            return graph.Nodes
                .Where(node => node != null
                    && IsReadinessNode(node)
                    && node.EvidenceState == evidenceState)
                .Select(node => new PyralisAuthoringGraphAuditRow(node))
                .ToArray();
        }

        public static IReadOnlyList<PyralisAuthoringGraphAuditSection> BuildReadinessAuditSections(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringGraphAuditSection>();

            return new[]
            {
                new PyralisAuthoringGraphAuditSection(
                    "Required Before Play",
                    PyralisAuthoringGraphEvidenceState.Blocked,
                    BuildReadinessAuditRows(graph, PyralisAuthoringGraphEvidenceState.Blocked)),
                new PyralisAuthoringGraphAuditSection(
                    "Recommended Before Play",
                    PyralisAuthoringGraphEvidenceState.Missing,
                    BuildReadinessAuditRows(graph, PyralisAuthoringGraphEvidenceState.Missing)),
                new PyralisAuthoringGraphAuditSection(
                    "Proof Enhancers",
                    PyralisAuthoringGraphEvidenceState.CandidateDetected,
                    BuildReadinessAuditRows(graph, PyralisAuthoringGraphEvidenceState.CandidateDetected))
            };
        }

        public static IReadOnlyList<PyralisAuthoringGraphAuditSection> BuildHygieneSections(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringGraphAuditSection>();

            return new[]
            {
                new PyralisAuthoringGraphAuditSection(
                    "Proof Blocker Links",
                    PyralisAuthoringGraphEvidenceState.Blocked,
                    BuildHygieneProofBlockerRows(graph)),
                new PyralisAuthoringGraphAuditSection(
                    "Unvalidated Graph Nodes",
                    PyralisAuthoringGraphEvidenceState.Unknown,
                    BuildHygieneUnknownRows(graph)),
                new PyralisAuthoringGraphAuditSection(
                    "Contract Inventory / Not Route-Evaluated",
                    PyralisAuthoringGraphEvidenceState.Unknown,
                    BuildHygieneContractInventoryRows(graph))
            };
        }

        public static IReadOnlyList<PyralisAuthoringGraphAuditRow> BuildHygieneDetailRows(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringGraphAuditRow>();

            List<PyralisAuthoringGraphAuditRow> rows = new List<PyralisAuthoringGraphAuditRow>();
            IReadOnlyList<PyralisAuthoringGraphAuditSection> sections = BuildHygieneSections(graph);
            for (int i = 0; i < sections.Count; i++)
            {
                PyralisAuthoringGraphAuditSection section = sections[i];
                if (section == null || !section.HasRows)
                    continue;

                if (ShouldIncludeInFocusedHygieneDetail(section))
                    rows.AddRange(section.Rows);
            }

            return rows.ToArray();
        }

        private static bool ShouldIncludeInFocusedHygieneDetail(PyralisAuthoringGraphAuditSection section)
        {
            if (section == null)
                return false;

            return string.Equals(section.Label, "Proof Blocker Links", StringComparison.Ordinal)
                || string.Equals(section.Label, "Unvalidated Graph Nodes", StringComparison.Ordinal);
        }

        private static IReadOnlyList<PyralisAuthoringGraphAuditRow> BuildHygieneUnknownRows(PyralisAuthoringSetupGraph graph)
        {
            return graph.Nodes
                .Where(node => node != null
                    && node.EvidenceState == PyralisAuthoringGraphEvidenceState.Unknown
                    && !IsMapOwnedReadinessNode(node)
                    && node.Kind != PyralisAuthoringGraphNodeKind.Contract
                    && node.Kind != PyralisAuthoringGraphNodeKind.Proof
                    && node.Kind != PyralisAuthoringGraphNodeKind.Capability)
                .Select(node => new PyralisAuthoringGraphAuditRow(node))
                .ToArray();
        }

        private static IReadOnlyList<PyralisAuthoringGraphAuditRow> BuildHygieneContractInventoryRows(PyralisAuthoringSetupGraph graph)
        {
            return graph.Nodes
                .Where(node => node != null
                    && node.Kind == PyralisAuthoringGraphNodeKind.Contract
                    && node.EvidenceState == PyralisAuthoringGraphEvidenceState.Unknown)
                .Select(node => new PyralisAuthoringGraphAuditRow(node))
                .ToArray();
        }

        private static IReadOnlyList<PyralisAuthoringGraphAuditRow> BuildHygieneProofBlockerRows(PyralisAuthoringSetupGraph graph)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            List<PyralisAuthoringGraphAuditRow> rows = new List<PyralisAuthoringGraphAuditRow>();

            for (int i = 0; i < graph.Edges.Count; i++)
            {
                PyralisAuthoringGraphEdge edge = graph.Edges[i];
                if (edge == null || edge.Kind != PyralisAuthoringGraphEdgeKind.BlockedBy)
                    continue;

                if (!graph.TryFindNode(edge.FromNodeId, out PyralisAuthoringGraphNode proof)
                    || proof == null
                    || !IsResolvedProofNode(proof))
                {
                    continue;
                }

                if (!graph.TryFindNode(edge.ToNodeId, out PyralisAuthoringGraphNode blocker)
                    || blocker == null
                    || blocker.EvidenceState == PyralisAuthoringGraphEvidenceState.Ready
                    || blocker.EvidenceState == PyralisAuthoringGraphEvidenceState.Optional
                    || IsMapOwnedReadinessNode(blocker)
                    || !seen.Add(blocker.StableId))
                {
                    continue;
                }

                rows.Add(new PyralisAuthoringGraphAuditRow(blocker));
            }

            return rows.ToArray();
        }

        private static bool IsMapOwnedReadinessNode(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return false;

            if (node.Kind == PyralisAuthoringGraphNodeKind.SceneSurface
                || node.Kind == PyralisAuthoringGraphNodeKind.SetupChain
                || node.Kind == PyralisAuthoringGraphNodeKind.UnitySurfaceRequirement)
            {
                return true;
            }

            return node.SourceKind == PyralisAuthoringGraphSourceKind.SceneReadiness
                || node.SourceKind == PyralisAuthoringGraphSourceKind.RuntimeValidation
                || node.SourceKind == PyralisAuthoringGraphSourceKind.SetupFlow;
        }

        private static bool IsResolvedProofNode(PyralisAuthoringGraphNode node)
        {
            return node != null
                && node.Kind == PyralisAuthoringGraphNodeKind.Proof
                && !string.Equals(node.StableId, "proof.unresolved-route", StringComparison.Ordinal);
        }

        public static IReadOnlyList<PyralisAuthoringGraphAuditRow> BuildReadinessAuditDetailRows(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringGraphAuditRow>();

            List<PyralisAuthoringGraphAuditRow> rows = new List<PyralisAuthoringGraphAuditRow>();
            IReadOnlyList<PyralisAuthoringGraphAuditSection> sections = BuildReadinessAuditSections(graph);
            for (int i = 0; i < sections.Count; i++)
            {
                PyralisAuthoringGraphAuditSection section = sections[i];
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
                GetSelectedContextCopyGuidance(selection));
        }

        public static IReadOnlyList<PyralisAuthoringOverviewIssue> BuildOverviewIssues(PyralisAuthoringSetupGraph graph, Object activeSetup)
        {
            List<PyralisAuthoringOverviewIssue> issues = new List<PyralisAuthoringOverviewIssue>();
            if (graph == null || graph.Source == null)
            {
                issues.Add(BuildNoActiveSetupOverviewIssue(graph, activeSetup));
                return issues;
            }

            PyralisAuthoringRouteWorkingProjection route = BuildRouteWorkingProjection(graph);
            for (int i = 0; i < route.CriticalPath.Count; i++)
            {
                PyralisAuthoringOverviewIssue issue = BuildOverviewIssue(route.CriticalPath[i]);
                if (issue != null)
                    issues.Add(issue);
            }

            for (int i = 0; i < route.ProofEnhancers.Count; i++)
            {
                PyralisAuthoringOverviewIssue issue = BuildOverviewIssue(route.ProofEnhancers[i]);
                if (issue != null)
                    issues.Add(issue);
            }

            for (int i = 0; i < route.CanWait.Count; i++)
            {
                PyralisAuthoringOverviewIssue issue = BuildOverviewIssue(route.CanWait[i]);
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
            PyralisAuthoringRouteWorkingProjection route = BuildRouteWorkingProjection(graph);
            bool requiredClear = route.ReadyForFirstProof;
            PyralisAuthoringRouteStepRow firstRequired = route.CurrentAction;
            PyralisAuthoringGraphNode proofNode = FindCurrentProofNode(graph);
            items.Add(new PyralisAuthoringPlayModeChecklistItem(
                "Required setup",
                requiredClear,
                requiredClear ? "Do Now is clear." : firstRequired?.Message ?? "Clear the selected route's Do Now setup before Play Mode."));

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

            return "Required setup is clear. Run the smallest Play Mode proof first, then add one feature at a time.";
        }

        public static bool IsOverviewReadyToPressPlay(PyralisAuthoringSetupGraph graph)
        {
            return graph != null && graph.Source != null && BuildRouteWorkingProjection(graph).ReadyForFirstProof;
        }

        public static string GetOverviewFirstProofLabel(PyralisAuthoringSetupGraph graph)
        {
            return FindCurrentProofNode(graph)?.Label ?? "Create Setup Foundation";
        }

        public static bool HasResolvedSetupContext(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return false;

            if (graph.Source is GameplaySessionBootstrap
                || graph.Source is SessionDefinition
                || graph.Source is GameModeDefinition
                || graph.Source is ParticipantDefinition
                || graph.Source is PawnDefinition)
            {
                return true;
            }

            if (graph.Source is GameObject gameObject
                && gameObject.GetComponent<GameplaySessionBootstrap>() != null)
            {
                return true;
            }

            return graph.RouteAnalysis != null
                && graph.RouteAnalysis.HasSelectedCapabilities
                && graph.Nodes.Count > 0
                && FindCurrentProofNode(graph) != null;
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
            PyralisAuthoringRouteStepRow currentAction = BuildRouteWorkingProjection(graph).CurrentAction;
            return currentAction != null
                ? "Defer expansion until this graph node is clear: " + currentAction.Label
                : "Defer broad polish until the graph-backed first proof runs in Play Mode.";
        }

        public static string GetOverviewFirstProofChainSummary(PyralisAuthoringSetupGraph graph)
        {
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> rows = BuildDirectProofSupportRows(graph);
            if (rows.Count == 0)
                return "Route proof chain: setup route -> active proof target.";

            return "Route proof chain: " + string.Join(
                " -> ",
                rows.Select(row => row.FromLabel).Distinct().Concat(new[] { rows[0].ToLabel }));
        }

        private static PyralisAuthoringSetupGraphRow BuildCapabilitiesRow(PyralisAuthoringSetupGraph graph)
        {
            return new PyralisAuthoringSetupGraphRow(
                "Capabilities",
                FindNode(graph, "capability.selected"),
                fallbackMessage: "Choose Intent capability ingredients, then create or wire gameplay assets so contracts and serialized references expose route capabilities.");
        }

        public static bool IsReadinessNode(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return false;

            if (node.Kind == PyralisAuthoringGraphNodeKind.ValidationEvidence
                || node.Kind == PyralisAuthoringGraphNodeKind.SetupChain
                || node.Kind == PyralisAuthoringGraphNodeKind.RouteShape
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

        private static PyralisAuthoringOverviewIssue BuildOverviewIssue(PyralisAuthoringRouteStepRow row)
        {
            PyralisAuthoringGraphNode node = row?.Node;
            if (node == null || node.EvidenceState == PyralisAuthoringGraphEvidenceState.Ready)
                return null;

            PyralisAuthoringOverviewLane lane = GetOverviewLane(row);
            return new PyralisAuthoringOverviewIssue(
                lane,
                row.Label,
                node.EvidenceState,
                row.Message,
                node.SourceObject,
                GetOverviewEvidence(node),
                row.NativeAction.HasValue ? row.NativeAction.Value.ToGuidanceSentence() : GetFirstNativeSetup(row),
                GetWorkIntentLabel(node.WorkIntent));
        }

        private static PyralisAuthoringOverviewLane GetOverviewLane(PyralisAuthoringRouteStepRow row)
        {
            PyralisAuthoringGraphNode node = row?.Node;
            if (node == null)
                return PyralisAuthoringOverviewLane.Later;

            if (row.Role == PyralisAuthoringRouteStepRole.DoThisFirst
                || row.Role == PyralisAuthoringRouteStepRole.BlocksProof)
            {
                return PyralisAuthoringOverviewLane.DoNow;
            }

            if (row.Role == PyralisAuthoringRouteStepRole.CanWait)
            {
                return node.WorkIntent == PyralisAuthoringGraphWorkIntent.ProofEnhancer
                    ? PyralisAuthoringOverviewLane.DoSoon
                    : PyralisAuthoringOverviewLane.Later;
            }

            if (node.EvidenceState == PyralisAuthoringGraphEvidenceState.CandidateDetected)
                return PyralisAuthoringOverviewLane.DoSoon;

            return PyralisAuthoringOverviewLane.Later;
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

        private static string GetFirstNativeSetup(PyralisAuthoringRouteStepRow row)
        {
            if (row == null || row.NativeSetup.Length == 0)
                return string.Empty;

            return row.NativeSetup[0];
        }

        private static string FormatOverviewBestNextAction(PyralisAuthoringOverviewIssue issue)
        {
            if (issue == null)
                return "Select a setup item and follow its native Unity action.";

            if (!string.IsNullOrWhiteSpace(issue.NativeActionGuidance))
                return issue.Label + ": " + issue.NativeActionGuidance;

            return issue.Label + ": " + issue.Message;
        }

        private static string GetRuntimeFamilyLabel(RuntimeCapabilityFamily family)
        {
            return family switch
            {
                RuntimeCapabilityFamily.PlatformCore => "setup/session",
                RuntimeCapabilityFamily.CharacterPawnGameplay => "pawn movement/control",
                RuntimeCapabilityFamily.ActionTargeting => "selection/action targeting",
                RuntimeCapabilityFamily.Combat => "combat",
                RuntimeCapabilityFamily.GunsProjectiles => "ranged/projectiles",
                RuntimeCapabilityFamily.ProceduralGeneration => "environment/procedural",
                RuntimeCapabilityFamily.BoardCardTabletop => "tabletop/board",
                RuntimeCapabilityFamily.AnimationPresentation => "animation/presentation",
                RuntimeCapabilityFamily.ScoringObjectives => "scoring/objectives",
                RuntimeCapabilityFamily.CameraInput => "camera/framing",
                RuntimeCapabilityFamily.Networking => "networking",
                _ => "custom"
            };
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
                if (node == null)
                    continue;

                bool setupFlowEvidence = node.Kind == PyralisAuthoringGraphNodeKind.ValidationEvidence
                    && node.SourceKind == PyralisAuthoringGraphSourceKind.SetupFlow;
                bool reflectedAssignment = node.Kind == PyralisAuthoringGraphNodeKind.AssignmentField
                    && node.SourceKind == PyralisAuthoringGraphSourceKind.Reflection;
                if (!setupFlowEvidence && !reflectedAssignment)
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
            const string detail = "Start with the native Unity setup chain: Hierarchy object, bootstrap component, SessionDefinition, GameModeDefinition, participants, and the assets or components your Intent route needs.";

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

            return "Use Map for scene/setup issues, Hygiene for code and graph audits, and the Inspector for field-level edits.";
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
            PyralisAuthoringGraphNode linkedIssue = FindFirstLinkedMapRowIssue(graph, nodeId);
            PyralisAuthoringGraphEvidenceState effectiveState = ResolveEffectiveEvidenceState(node, linkedIssue);
            string effectiveMessage = linkedIssue != null
                && node != null
                && (node.EvidenceState == PyralisAuthoringGraphEvidenceState.Ready || node.EvidenceState == PyralisAuthoringGraphEvidenceState.Optional)
                    ? linkedIssue.Guidance
                    : string.Empty;
            return new PyralisAuthoringSetupGraphRow(label, node, isOptional, fallbackMessage, effectiveEvidenceState: effectiveState, effectiveMessage: effectiveMessage);
        }

        private static PyralisAuthoringGraphNode FindNode(PyralisAuthoringSetupGraph graph, string nodeId)
        {
            return graph != null && graph.TryFindNode(nodeId, out PyralisAuthoringGraphNode node) ? node : null;
        }

        private static PyralisAuthoringGraphNode FindFirstLinkedMapRowIssue(PyralisAuthoringSetupGraph graph, string nodeId)
        {
            if (graph == null || string.IsNullOrWhiteSpace(nodeId))
                return null;

            IReadOnlyList<PyralisAuthoringGraphEdge> outgoing = graph.FindOutgoing(nodeId);
            for (int i = 0; i < outgoing.Count; i++)
            {
                PyralisAuthoringGraphEdge edge = outgoing[i];
                if (edge == null || edge.Kind != PyralisAuthoringGraphEdgeKind.RelatesTo)
                    continue;

                if (!graph.TryFindNode(edge.ToNodeId, out PyralisAuthoringGraphNode linkedNode)
                    || linkedNode == null
                    || !IsReadinessNode(linkedNode)
                    || !CanLinkedIssueAffectMapRow(nodeId, linkedNode))
                {
                    continue;
                }

                if (linkedNode.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked
                    || linkedNode.EvidenceState == PyralisAuthoringGraphEvidenceState.Missing)
                    return linkedNode;
            }

            return null;
        }

        private static bool CanLinkedIssueAffectMapRow(string nodeId, PyralisAuthoringGraphNode linkedNode)
        {
            if (linkedNode == null || string.IsNullOrWhiteSpace(nodeId))
                return false;

            if (string.Equals(nodeId, "scene.surfaces", StringComparison.Ordinal))
                return linkedNode.Kind == PyralisAuthoringGraphNodeKind.SceneSurface
                    || linkedNode.SourceKind == PyralisAuthoringGraphSourceKind.SceneReadiness;

            if (linkedNode.Kind == PyralisAuthoringGraphNodeKind.SceneSurface)
                return false;

            if (linkedNode.SourceKind == PyralisAuthoringGraphSourceKind.SceneReadiness)
                return false;

            if (string.Equals(nodeId, "bootstrap.root", StringComparison.Ordinal))
            {
                return linkedNode.SourceKind == PyralisAuthoringGraphSourceKind.SetupFlow
                    && !string.Equals(linkedNode.StableId, "scene.surfaces", StringComparison.Ordinal);
            }

            if (string.Equals(nodeId, "participant.default", StringComparison.Ordinal)
                || string.Equals(nodeId, "pawn.definition", StringComparison.Ordinal))
            {
                return linkedNode.SourceKind == PyralisAuthoringGraphSourceKind.RuntimeValidation
                    || linkedNode.SourceKind == PyralisAuthoringGraphSourceKind.SetupFlow;
            }

            return linkedNode.SourceKind == PyralisAuthoringGraphSourceKind.SetupFlow;
        }

        private static PyralisAuthoringGraphEvidenceState ResolveEffectiveEvidenceState(
            PyralisAuthoringGraphNode node,
            PyralisAuthoringGraphNode linkedIssue)
        {
            PyralisAuthoringGraphEvidenceState state = node != null
                ? node.EvidenceState
                : PyralisAuthoringGraphEvidenceState.Unknown;
            if (linkedIssue == null)
                return state;

            return IsMoreBlocking(linkedIssue.EvidenceState, state)
                ? linkedIssue.EvidenceState
                : state;
        }

        private static bool IsMoreBlocking(PyralisAuthoringGraphEvidenceState candidate, PyralisAuthoringGraphEvidenceState current)
        {
            return GetEvidenceRank(candidate) > GetEvidenceRank(current);
        }

        private static int GetEvidenceRank(PyralisAuthoringGraphEvidenceState state)
        {
            return state switch
            {
                PyralisAuthoringGraphEvidenceState.Blocked => 5,
                PyralisAuthoringGraphEvidenceState.Missing => 4,
                PyralisAuthoringGraphEvidenceState.CandidateDetected => 3,
                PyralisAuthoringGraphEvidenceState.Unknown => 2,
                PyralisAuthoringGraphEvidenceState.Ready => 1,
                PyralisAuthoringGraphEvidenceState.Optional => 0,
                _ => 0
            };
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
                GameModeDefinition => "Rules contract that owns rule-level defaults, feature modules, board/turn data, playfield, camera, and scene targets.",
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
                GameModeDefinition => "Inspect required feature modules, playfield, camera, board/turn data, and rule flags.",
                ParticipantDefinition => "Inspect default pawn, input profile, seat index, and auto-join ownership.",
                PawnDefinition => "Inspect pawn prefab, movement/input/presentation profiles, and feature modules.",
                Component => "Use the Inspector for field values and Map for concrete scene/setup readiness issues.",
                GameObject => "Select the most specific Pyralis component on this object when you need field-level meaning.",
                _ => "Inspect the selected asset fields in Unity."
            };
        }

        private static IReadOnlyList<PyralisAuthoringSelectedContextDetail> BuildSelectedContextDetails(
            Object selection,
            PyralisAuthoringSetupGraph graph)
        {
            return Array.Empty<PyralisAuthoringSelectedContextDetail>();
        }

        private static string GetSelectedContextCopyGuidance(Object selection)
        {
            return string.Empty;
        }
    }
}
