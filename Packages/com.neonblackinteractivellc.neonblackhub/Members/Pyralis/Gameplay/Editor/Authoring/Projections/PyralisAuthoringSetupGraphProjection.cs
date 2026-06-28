using System;
using System.Collections.Generic;
using System.Linq;
using NeonBlack.Gameplay.Core.Types.Animation;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Glue.Session;
using NeonBlack.Gameplay.Glue.Spawning;
using NeonBlack.Gameplay.Modules.Character;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Editor.Inspectors;
using UnityEngine;
using Object = UnityEngine.Object;
using NeonBlack.Gameplay.Glue.Bootstrap;

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
        public string IssueCode => Node != null ? Node.IssueCode : string.Empty;
        public string TriageBucket => BuildTriageBucket(Node);
        public string TriageAdvice => BuildTriageAdvice(Node);
        public string OwnershipBucket => BuildOwnershipClassification(Node).OwnershipBucket;
        public string RepairOwner => BuildOwnershipClassification(Node).RepairOwner;
        public string OwnershipAdvice => BuildOwnershipClassification(Node).OwnershipAdvice;

        private static PyralisAuthoringContractMetadataClassification BuildOwnershipClassification(PyralisAuthoringGraphNode node)
        {
            return PyralisAuthoringContractMetadataPolicy.Classify(node?.SourceContract, node?.IssueCode);
        }

        private static string BuildTriageBucket(PyralisAuthoringGraphNode node)
        {
            if (node == null || string.IsNullOrWhiteSpace(node.IssueCode))
                return string.Empty;

            return node.IssueCode switch
            {
                "ContractMetadata.CapabilityPathMissing" => "IntentMetadataDecision",
                "ContractMetadata.RouteEssentialCapabilityPathMissing" => "RouteEssentialNeedsPath",
                "ContractMetadata.RuntimeFamiliesMissing" => "RuntimeFamilyDecision",
                "ContractMetadata.DuplicateOwnershipClaim" => "DuplicateOwnershipClaim",
                "ContractMetadata.ProofTargetGenericTemplate" => "NeedsFeatureProofTarget",
                "ValidationEvidence.MetadataMissing" => "ValidationMetadata",
                _ => string.Empty
            };
        }

        private static string BuildTriageAdvice(PyralisAuthoringGraphNode node)
        {
            if (node == null || string.IsNullOrWhiteSpace(node.IssueCode))
                return string.Empty;

            string sourceName = node.SourceContract != null ? node.SourceContract.DisplayName : node.Label;
            return node.IssueCode switch
            {
                "ContractMetadata.CapabilityPathMissing" => $"{sourceName} is selectable Intent metadata. Either give it a semantic CapabilityPath so users can focus it, or mark it support-only.",
                "ContractMetadata.RouteEssentialCapabilityPathMissing" => $"{sourceName} is route-essential infrastructure. Give it a Core Setup path so Intent can group it without display-name guessing.",
                "ContractMetadata.RuntimeFamiliesMissing" => $"{sourceName} has capability meaning but no runtime family. Add the runtime family that owns the behavior, or set capability to None if it is metadata-only.",
                "ContractMetadata.DuplicateOwnershipClaim" => "Pick one canonical owner for this responsibility, or split the ownership claim into narrower contract responsibilities.",
                "ContractMetadata.ProofTargetGenericTemplate" => "Replace the generic proof target with the specific feature proof this contract should support.",
                "ValidationEvidence.MetadataMissing" => "Validation is witnessing readiness without enough metadata for Hygiene to route the repair cleanly.",
                _ => string.Empty
            };
        }

        private static string FormatSourceKind(PyralisAuthoringGraphSourceKind sourceKind)
        {
            return sourceKind switch
            {
                PyralisAuthoringGraphSourceKind.CapabilityVocabulary => "Capability Vocabulary",
                PyralisAuthoringGraphSourceKind.AuthoringContract => "Authoring Contract",
                PyralisAuthoringGraphSourceKind.VocabularyRegistry => "Vocabulary Registry",
                PyralisAuthoringGraphSourceKind.CoreSetup => "Core Setup",
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
                PyralisAuthoringGraphSourceOrigin.Vocabulary => "Vocabulary",
                PyralisAuthoringGraphSourceOrigin.VocabularyFallback => "Vocabulary Fallback",
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
        Proof,
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
            PyralisAuthoringNativeAction? nativeActionOverride = null,
            string ownerLabelOverride = null)
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
            OwnerLabelOverride = ownerLabelOverride ?? string.Empty;
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
        public string OwnerLabelOverride { get; }
        public string StableId => Node != null ? Node.StableId : string.Empty;
        public string Label => !string.IsNullOrWhiteSpace(LabelOverride) ? LabelOverride : Node != null ? Node.Label : string.Empty;
        public string Message => !string.IsNullOrWhiteSpace(MessageOverride) ? MessageOverride : Node != null ? Node.Guidance : string.Empty;
        public string Proof => Node != null && !string.IsNullOrWhiteSpace(Node.BlockingReason) ? Node.BlockingReason : Message;
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
        public string OwnerLabel => BuildOwnerLabel(this);

        private static string BuildOwnerLabel(PyralisAuthoringRouteStepRow row)
        {
            PyralisAuthoringGraphNode node = row?.Node;
            if (node == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(row.OwnerLabelOverride))
                return row.OwnerLabelOverride;

            if (row.NativeAction.HasValue)
            {
                PyralisAuthoringNativeAction action = row.NativeAction.Value;
                return PyralisAuthoringLabelUtility.GetNativeActionOwnerLabel(action);
            }

            if (node.AssignmentFields != null && node.AssignmentFields.Length > 0)
                return node.AssignmentFields[0];

            if (node.SourceContract != null && !string.IsNullOrWhiteSpace(node.SourceContract.SetupNodeId))
                return node.SourceContract.SetupNodeId;

            if (node.SourceObject != null)
                return $"{node.SourceObject.name} ({node.SourceObject.GetType().Name})";

            return node.StableId;
        }

        private static string FormatPhase(PyralisAuthoringRouteStepPhase phase)
        {
            return phase switch
            {
                PyralisAuthoringRouteStepPhase.Foundation => "Foundation",
                PyralisAuthoringRouteStepPhase.SetupChain => "Setup Chain",
                PyralisAuthoringRouteStepPhase.Capability => "Capability",
                PyralisAuthoringRouteStepPhase.SceneEvidence => "Scene Evidence",
                PyralisAuthoringRouteStepPhase.Proof => "Proof",
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

    public sealed class PyralisAuthoringResolvedSetupRequirement
    {
        public PyralisAuthoringResolvedSetupRequirement(
            PyralisAuthoringGraphNode node,
            string stableId,
            string label,
            string message,
            string ownerLabel,
            string[] nativeSetup,
            string[] assignmentFields,
            PyralisAuthoringNativeAction? nativeAction)
        {
            Node = node;
            StableId = stableId ?? node?.StableId ?? string.Empty;
            Label = label ?? node?.Label ?? string.Empty;
            Message = message ?? node?.Guidance ?? string.Empty;
            OwnerLabel = ownerLabel ?? string.Empty;
            NativeSetup = nativeSetup ?? node?.NativeSetup ?? Array.Empty<string>();
            AssignmentFields = assignmentFields ?? node?.AssignmentFields ?? Array.Empty<string>();
            NativeAction = nativeAction;
        }

        public PyralisAuthoringGraphNode Node { get; }
        public string StableId { get; }
        public string Label { get; }
        public string Message { get; }
        public string OwnerLabel { get; }
        public string[] NativeSetup { get; }
        public string[] AssignmentFields { get; }
        public PyralisAuthoringNativeAction? NativeAction { get; }

        public PyralisAuthoringRouteStepRow ToRouteStep(int sequence, PyralisAuthoringGraphEdge edge = null)
        {
            return new PyralisAuthoringRouteStepRow(
                Node,
                sequence,
                PyralisAuthoringSetupGraphProjection.GetPhase(Node),
                PyralisAuthoringRouteStepRole.BlocksProof,
                "This resolved setup requirement must be cleared before the selected proof is believable.",
                edge,
                labelOverride: Label,
                messageOverride: Message,
                nativeSetupOverride: NativeSetup,
                assignmentFieldsOverride: AssignmentFields,
                nativeActionOverride: NativeAction,
                ownerLabelOverride: OwnerLabel);
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
            IReadOnlyList<PyralisAuthoringResolvedSetupRequirement> requiredSetup,
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> proofBlockers,
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> proofSupport)
        {
            RouteName = routeName ?? "No setup route selected";
            Proof = proof;
            OrderedSteps = orderedSteps ?? Array.Empty<PyralisAuthoringRouteStepRow>();
            CriticalPath = criticalPath ?? Array.Empty<PyralisAuthoringRouteStepRow>();
            ProofEnhancers = proofEnhancers ?? Array.Empty<PyralisAuthoringRouteStepRow>();
            CanWait = canWait ?? Array.Empty<PyralisAuthoringRouteStepRow>();
            RequiredSetup = requiredSetup ?? Array.Empty<PyralisAuthoringResolvedSetupRequirement>();
            ProofBlockers = proofBlockers ?? Array.Empty<PyralisAuthoringGraphConnectionRow>();
            ProofSupport = proofSupport ?? Array.Empty<PyralisAuthoringGraphConnectionRow>();
            CurrentAction = FindCurrentAction(CriticalPath)
                ?? FindCurrentActionFromResolvedRequirements(RequiredSetup, ProofBlockers)
                ?? FindCurrentActionFromProofBlockers(ProofBlockers);
            ReadyForProof = CurrentAction == null && ProofBlockers.Count == 0;
        }

        public string RouteName { get; }
        public PyralisAuthoringGraphNode Proof { get; }
        public IReadOnlyList<PyralisAuthoringRouteStepRow> OrderedSteps { get; }
        public IReadOnlyList<PyralisAuthoringRouteStepRow> CriticalPath { get; }
        public IReadOnlyList<PyralisAuthoringRouteStepRow> ProofEnhancers { get; }
        public IReadOnlyList<PyralisAuthoringRouteStepRow> CanWait { get; }
        public IReadOnlyList<PyralisAuthoringResolvedSetupRequirement> RequiredSetup { get; }
        public IReadOnlyList<PyralisAuthoringGraphConnectionRow> ProofBlockers { get; }
        public IReadOnlyList<PyralisAuthoringGraphConnectionRow> ProofSupport { get; }
        public PyralisAuthoringRouteStepRow CurrentAction { get; }
        public bool ReadyForProof { get; }

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

        private static PyralisAuthoringRouteStepRow FindCurrentActionFromResolvedRequirements(
            IReadOnlyList<PyralisAuthoringResolvedSetupRequirement> requirements,
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> proofBlockers)
        {
            if (requirements == null)
                return null;

            for (int i = 0; i < requirements.Count; i++)
            {
                PyralisAuthoringResolvedSetupRequirement requirement = requirements[i];
                if (requirement?.Node == null)
                    continue;

                PyralisAuthoringGraphConnectionRow blocker = FindProofBlockerForNode(proofBlockers, requirement.Node.StableId);
                return requirement.ToRouteStep(0, blocker?.Edge);
            }

            return null;
        }

        private static PyralisAuthoringGraphConnectionRow FindProofBlockerForNode(
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> proofBlockers,
            string nodeId)
        {
            if (proofBlockers == null || string.IsNullOrWhiteSpace(nodeId))
                return null;

            for (int i = 0; i < proofBlockers.Count; i++)
            {
                PyralisAuthoringGraphConnectionRow row = proofBlockers[i];
                if (string.Equals(row?.To?.StableId, nodeId, StringComparison.Ordinal))
                    return row;
            }

            return null;
        }

        private static PyralisAuthoringRouteStepRow FindCurrentActionFromProofBlockers(
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> proofBlockers)
        {
            if (proofBlockers == null)
                return null;

            for (int i = 0; i < proofBlockers.Count; i++)
            {
                PyralisAuthoringGraphConnectionRow blocker = proofBlockers[i];
                PyralisAuthoringGraphNode node = blocker?.To;
                if (node == null)
                    continue;

                if (node.EvidenceState != PyralisAuthoringGraphEvidenceState.Missing
                    && node.EvidenceState != PyralisAuthoringGraphEvidenceState.Blocked)
                {
                    continue;
                }

                return new PyralisAuthoringRouteStepRow(
                    node,
                    0,
                    PyralisAuthoringSetupGraphProjection.GetPhase(node),
                    PyralisAuthoringRouteStepRole.BlocksProof,
                    "This proof blocker must be cleared before the selected proof is believable.",
                    blocker.Edge);
            }

            return null;
        }
    }

    public sealed class PyralisAuthoringRouteDiagnosticQuestionRow
    {
        public PyralisAuthoringRouteDiagnosticQuestionRow(string question, string answer)
        {
            Question = question ?? string.Empty;
            Answer = answer ?? string.Empty;
        }

        public string Question { get; }
        public string Answer { get; }
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

    internal enum PyralisAuthoringProjectionGroup
    {
        Reference,
        Foundation,
        SetupChain,
        ReflectedAssignment,
        PrefabReadiness,
        RuntimeValidation,
        SceneEvidence,
        Contract,
        Capability,
        Proof
    }

    internal enum PyralisAuthoringProjectionAudience
    {
        Reference,
        Map
    }

    internal readonly struct PyralisAuthoringProjectionMetadata
    {
        public PyralisAuthoringProjectionMetadata(
            PyralisAuthoringProjectionGroup group,
            PyralisAuthoringProjectionAudience audience,
            PyralisAuthoringRouteStepPhase phase,
            PyralisAuthoringGraphSetupDomain ownerDomain,
            int sortRank)
        {
            Group = group;
            Audience = audience;
            Phase = phase;
            OwnerDomain = ownerDomain;
            SortRank = sortRank;
        }

        public PyralisAuthoringProjectionGroup Group { get; }
        public PyralisAuthoringProjectionAudience Audience { get; }
        public PyralisAuthoringRouteStepPhase Phase { get; }
        public PyralisAuthoringGraphSetupDomain OwnerDomain { get; }
        public int SortRank { get; }
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

    public sealed class PyralisAuthoringOverviewProjection
    {
        private PyralisAuthoringOverviewProjection(
            Object activeSetup,
            PyralisAuthoringSetupGraph graph,
            PyralisAuthoringOverviewModel model,
            PyralisAuthoringCurrentStepGraphRow currentStep,
            PyralisAuthoringGraphNode proofNode,
            IReadOnlyList<PyralisAuthoringSelectedIntentStatusRow> selectedIntentStatus)
        {
            ActiveSetup = activeSetup;
            Graph = graph;
            Model = model;
            CurrentStep = currentStep;
            ProofNode = proofNode;
            SelectedIntentStatus = selectedIntentStatus ?? Array.Empty<PyralisAuthoringSelectedIntentStatusRow>();
        }

        public Object ActiveSetup { get; }
        public PyralisAuthoringSetupGraph Graph { get; }
        public PyralisAuthoringOverviewModel Model { get; }
        public PyralisAuthoringCurrentStepGraphRow CurrentStep { get; }
        public PyralisAuthoringGraphNode ProofNode { get; }
        public IReadOnlyList<PyralisAuthoringSelectedIntentStatusRow> SelectedIntentStatus { get; }

        public static PyralisAuthoringOverviewProjection Build(Object activeSetup, PyralisAuthoringSetupGraph graph)
        {
            return new PyralisAuthoringOverviewProjection(
                activeSetup,
                graph,
                PyralisAuthoringOverviewModel.Build(activeSetup, graph),
                PyralisAuthoringSetupGraphProjection.BuildCurrentStepRow(graph),
                PyralisAuthoringSetupGraphProjection.FindCurrentProofNode(graph),
                PyralisAuthoringSetupGraphProjection.BuildSelectedIntentStatusRows(graph));
        }
    }

    public sealed class PyralisAuthoringGuideProjection
    {
        private PyralisAuthoringGuideProjection(
            Object selection,
            Object activeSetup,
            PyralisAuthoringSetupGraph graph,
            PyralisAuthoringCurrentStepGraphRow currentStep,
            PyralisAuthoringGuideTraceProjection guideTrace,
            IReadOnlyList<PyralisAuthoringReflectiveContractGraphRow> contracts,
            PyralisAuthoringSelectedContextGraphRow selectedContext,
            IReadOnlyList<PyralisAuthoringSelectedIntentStatusRow> selectedIntentStatus,
            bool selectionFirst)
        {
            Selection = selection;
            ActiveSetup = activeSetup;
            Graph = graph;
            CurrentStep = currentStep;
            GuideTrace = guideTrace ?? PyralisAuthoringSetupGraphProjection.BuildGuideTraceProjection(graph);
            Route = GuideTrace.Route;
            Contracts = contracts ?? Array.Empty<PyralisAuthoringReflectiveContractGraphRow>();
            SelectedContext = selectedContext;
            SelectedIntentStatus = selectedIntentStatus ?? Array.Empty<PyralisAuthoringSelectedIntentStatusRow>();
            SelectionFirst = selectionFirst;
        }

        public Object Selection { get; }
        public Object ActiveSetup { get; }
        public PyralisAuthoringSetupGraph Graph { get; }
        public PyralisAuthoringCurrentStepGraphRow CurrentStep { get; }
        public PyralisAuthoringGuideTraceProjection GuideTrace { get; }
        public PyralisAuthoringRouteWorkingProjection Route { get; }
        public IReadOnlyList<PyralisAuthoringReflectiveContractGraphRow> Contracts { get; }
        public PyralisAuthoringSelectedContextGraphRow SelectedContext { get; }
        public IReadOnlyList<PyralisAuthoringSelectedIntentStatusRow> SelectedIntentStatus { get; }
        public bool SelectionFirst { get; }

        public static PyralisAuthoringGuideProjection Build(Object selection, Object activeSetup, PyralisAuthoringSetupGraph graph)
        {
            return new PyralisAuthoringGuideProjection(
                selection,
                activeSetup,
                graph,
                PyralisAuthoringSetupGraphProjection.BuildCurrentStepRow(graph),
                PyralisAuthoringSetupGraphProjection.BuildGuideTraceProjection(graph),
                PyralisAuthoringSetupGraphProjection.BuildReflectiveContractRows(graph),
                PyralisAuthoringSetupGraphProjection.BuildSelectedContextRow(graph, selection),
                PyralisAuthoringSetupGraphProjection.BuildSelectedIntentStatusRows(graph),
                activeSetup == null
                    && selection is GameObject selectedGameObject
                    && selectedGameObject.GetComponent<GameplaySessionBootstrap>() == null);
        }
    }

    public sealed class PyralisAuthoringMapProjection
    {
        private PyralisAuthoringMapProjection(
            Object activeSetup,
            Object selection,
            PyralisAuthoringSetupGraph graph,
            IReadOnlyList<PyralisAuthoringSetupGraphRow> setupRows,
            IReadOnlyList<PyralisAuthoringGraphNode> sceneSurfaces,
            IReadOnlyList<PyralisAuthoringGraphAuditRow> sceneSetupIssues,
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> connections)
        {
            ActiveSetup = activeSetup;
            Selection = selection;
            Graph = graph;
            SetupRows = setupRows ?? Array.Empty<PyralisAuthoringSetupGraphRow>();
            SceneSurfaces = sceneSurfaces ?? Array.Empty<PyralisAuthoringGraphNode>();
            SceneSetupIssues = sceneSetupIssues ?? Array.Empty<PyralisAuthoringGraphAuditRow>();
            Connections = connections ?? Array.Empty<PyralisAuthoringGraphConnectionRow>();
        }

        public Object ActiveSetup { get; }
        public Object Selection { get; }
        public PyralisAuthoringSetupGraph Graph { get; }
        public IReadOnlyList<PyralisAuthoringSetupGraphRow> SetupRows { get; }
        public IReadOnlyList<PyralisAuthoringGraphNode> SceneSurfaces { get; }
        public IReadOnlyList<PyralisAuthoringGraphAuditRow> SceneSetupIssues { get; }
        public IReadOnlyList<PyralisAuthoringGraphConnectionRow> Connections { get; }

        public static PyralisAuthoringMapProjection Build(Object activeSetup, Object selection, PyralisAuthoringSetupGraph graph)
        {
            return new PyralisAuthoringMapProjection(
                activeSetup,
                selection,
                graph,
                PyralisAuthoringSetupGraphProjection.BuildSetupMapRows(graph),
                PyralisAuthoringSetupGraphProjection.FindSceneSurfaceNodes(graph),
                PyralisAuthoringSetupGraphProjection.BuildMapSceneSetupIssueRows(graph),
                PyralisAuthoringSetupGraphProjection.BuildMapConnectionRows(graph));
        }
    }

    public sealed class PyralisAuthoringHygieneProjection
    {
        private PyralisAuthoringHygieneProjection(
            Object activeSetup,
            PyralisAuthoringSetupGraph graph,
            IReadOnlyList<PyralisAuthoringGraphAuditSection> sections,
            IReadOnlyList<PyralisAuthoringGraphAuditRow> detailRows,
            IReadOnlyList<PyralisSourceDependencyHygieneRecord> dependencyRecords)
        {
            ActiveSetup = activeSetup;
            Graph = graph;
            Sections = sections ?? Array.Empty<PyralisAuthoringGraphAuditSection>();
            DetailRows = detailRows ?? Array.Empty<PyralisAuthoringGraphAuditRow>();
            DependencyRecords = dependencyRecords ?? Array.Empty<PyralisSourceDependencyHygieneRecord>();
            ProofBlockers = PyralisAuthoringSetupGraphProjection.BuildHygieneProofBlockerConnectionRows(Graph);
            ContractSourcePressureRows = BuildContractSourcePressureRows(Graph);
            DependencyPressureRows = BuildDependencyPressureRows(DependencyRecords);
            CleanupFocus = BuildCleanupFocus(DependencyRecords);
            WatchList = BuildWatchList(DependencyRecords);
        }

        public Object ActiveSetup { get; }
        public PyralisAuthoringSetupGraph Graph { get; }
        public IReadOnlyList<PyralisAuthoringGraphAuditSection> Sections { get; }
        public IReadOnlyList<PyralisAuthoringGraphAuditRow> DetailRows { get; }
        public IReadOnlyList<PyralisSourceDependencyHygieneRecord> DependencyRecords { get; }
        public IReadOnlyList<PyralisAuthoringGraphConnectionRow> ProofBlockers { get; }
        public IReadOnlyList<PyralisAuthoringGraphNode> ContractSourcePressureRows { get; }
        public IReadOnlyList<PyralisSourceDependencyHygieneRecord> DependencyPressureRows { get; }
        public IReadOnlyList<PyralisSourceDependencyHygieneRecord> CleanupFocus { get; }
        public IReadOnlyList<PyralisSourceDependencyHygieneRecord> WatchList { get; }
        public int DependencyPressureCount => DependencyRecords.Count(record => record != null && record.Risk != PyralisSourceDependencyRisk.Low);
        public int CleanupFocusCount => DependencyRecords.Count(record => record != null && record.Risk != PyralisSourceDependencyRisk.Low && IsCleanupFocus(record.PressureKind));
        public int WatchListCount => DependencyRecords.Count(record => record != null && record.Risk != PyralisSourceDependencyRisk.Low && !IsCleanupFocus(record.PressureKind));
        public int WatchCount => CountRisk(PyralisSourceDependencyRisk.Watch);
        public int HeavyCount => CountRisk(PyralisSourceDependencyRisk.Heavy);
        public int BoundaryRiskCount => CountRisk(PyralisSourceDependencyRisk.BoundaryRisk);
        public int ActionablePressureCount => DependencyRecords.Count(IsActionablePressure);
        public int ExpectedPressureCount => DependencyRecords.Count(record => record != null && record.Risk != PyralisSourceDependencyRisk.Low && !IsActionablePressure(record));

        public static PyralisAuthoringHygieneProjection Build(Object activeSetup, PyralisAuthoringSetupGraph graph, IReadOnlyList<PyralisSourceDependencyHygieneRecord> dependencyRecords)
        {
            return new PyralisAuthoringHygieneProjection(
                activeSetup,
                graph,
                PyralisAuthoringSetupGraphProjection.BuildHygieneSections(graph),
                PyralisAuthoringSetupGraphProjection.BuildHygieneDetailRows(graph),
                dependencyRecords);
        }

        public IReadOnlyList<string> BuildPressureKindSummary()
        {
            return DependencyRecords
                .Where(record => record != null && record.Risk != PyralisSourceDependencyRisk.Low)
                .GroupBy(record => record.PressureKind)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key.ToString(), StringComparer.Ordinal)
                .Select(group => group.Key + ": " + group.Count())
                .ToArray();
        }

        public IReadOnlyList<string> BuildOwnershipBucketSummary()
        {
            return DetailRows
                .Where(row => row != null && !string.IsNullOrWhiteSpace(row.OwnershipBucket))
                .GroupBy(row => row.OwnershipBucket)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => group.Key + ": " + group.Count())
                .ToArray();
        }

        private int CountRisk(PyralisSourceDependencyRisk risk)
        {
            return DependencyRecords.Count(record => record != null && record.Risk == risk);
        }

        private static IReadOnlyList<PyralisSourceDependencyHygieneRecord> BuildCleanupFocus(IReadOnlyList<PyralisSourceDependencyHygieneRecord> records)
        {
            return records
                .Where(record => record != null && record.Risk != PyralisSourceDependencyRisk.Low && IsCleanupFocus(record.PressureKind))
                .OrderBy(record => PyralisSourceDependencyHygieneScanner.GetCleanupPriority(record.PressureKind))
                .ThenByDescending(record => record.RiskScore)
                .ThenBy(record => record.FileName, StringComparer.Ordinal)
                .ToArray();
        }

        private static IReadOnlyList<PyralisSourceDependencyHygieneRecord> BuildWatchList(IReadOnlyList<PyralisSourceDependencyHygieneRecord> records)
        {
            return records
                .Where(record => record != null && record.Risk != PyralisSourceDependencyRisk.Low && !IsCleanupFocus(record.PressureKind))
                .OrderBy(record => PyralisSourceDependencyHygieneScanner.GetCleanupPriority(record.PressureKind))
                .ThenByDescending(record => record.RiskScore)
                .ThenBy(record => record.FileName, StringComparer.Ordinal)
                .ToArray();
        }

        private static IReadOnlyList<PyralisSourceDependencyHygieneRecord> BuildDependencyPressureRows(IReadOnlyList<PyralisSourceDependencyHygieneRecord> records)
        {
            return records
                .Where(record => record != null && record.Risk != PyralisSourceDependencyRisk.Low)
                .OrderBy(record => PyralisSourceDependencyHygieneScanner.GetCleanupPriority(record.PressureKind))
                .ThenByDescending(record => record.RiskScore)
                .ThenBy(record => record.FileName, StringComparer.Ordinal)
                .ToArray();
        }

        private static IReadOnlyList<PyralisAuthoringGraphNode> BuildContractSourcePressureRows(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringGraphNode>();

            return graph.Nodes
                .Where(node => node != null && (node.Kind == PyralisAuthoringGraphNodeKind.Contract || node.SourceContract != null))
                .ToArray();
        }

        public static bool IsCleanupFocus(PyralisSourceDependencyPressureKind pressureKind)
        {
            return pressureKind == PyralisSourceDependencyPressureKind.RuntimeOwnership
                || pressureKind == PyralisSourceDependencyPressureKind.DirectSceneQuerySurface
                || PyralisSourceDependencyHygieneScanner.IsOwnershipLeakPressure(pressureKind);
        }

        private static bool IsActionablePressure(PyralisSourceDependencyHygieneRecord record)
        {
            return record != null
                && record.Risk != PyralisSourceDependencyRisk.Low
                && IsCleanupFocus(record.PressureKind);
        }
    }

    public sealed class PyralisAuthoringFactKindSummaryRow
    {
        public PyralisAuthoringFactKindSummaryRow(PyralisAuthoringFactKind kind, int count)
        {
            Kind = kind;
            Count = count;
            Label = kind.ToString();
        }

        public PyralisAuthoringFactKind Kind { get; }
        public string Label { get; }
        public int Count { get; }
    }

    public sealed class PyralisAuthoringFactGroupRow
    {
        public PyralisAuthoringFactGroupRow(PyralisAuthoringFactKind kind, IReadOnlyList<PyralisAuthoringFact> facts)
        {
            Kind = kind;
            Label = kind.ToString();
            Facts = facts ?? Array.Empty<PyralisAuthoringFact>();
        }

        public PyralisAuthoringFactKind Kind { get; }
        public string Label { get; }
        public IReadOnlyList<PyralisAuthoringFact> Facts { get; }
    }

    public sealed class PyralisAuthoringContractGroupRow
    {
        public PyralisAuthoringContractGroupRow(string category, IReadOnlyList<PyralisAuthoringReflectiveContractGraphRow> contracts)
        {
            Category = string.IsNullOrWhiteSpace(category) ? "Uncategorized" : category;
            Contracts = contracts ?? Array.Empty<PyralisAuthoringReflectiveContractGraphRow>();
        }

        public string Category { get; }
        public IReadOnlyList<PyralisAuthoringReflectiveContractGraphRow> Contracts { get; }
    }

    public sealed class PyralisAuthoringFactsProjection
    {
        private PyralisAuthoringFactsProjection(
            Object activeSetup,
            PyralisAuthoringSetupGraph graph,
            IReadOnlyList<PyralisAuthoringFact> facts,
            IReadOnlyList<PyralisAuthoringReflectiveContractGraphRow> contracts)
        {
            ActiveSetup = activeSetup;
            Graph = graph;
            Facts = facts ?? Array.Empty<PyralisAuthoringFact>();
            Contracts = contracts ?? Array.Empty<PyralisAuthoringReflectiveContractGraphRow>();
            FactKindRows = BuildFactKindRows(Facts);
            FactGroups = BuildFactGroups(Facts);
            ContractGroups = BuildContractGroups(Contracts);
        }

        public Object ActiveSetup { get; }
        public PyralisAuthoringSetupGraph Graph { get; }
        public IReadOnlyList<PyralisAuthoringFact> Facts { get; }
        public IReadOnlyList<PyralisAuthoringReflectiveContractGraphRow> Contracts { get; }
        public IReadOnlyList<PyralisAuthoringFactKindSummaryRow> FactKindRows { get; }
        public IReadOnlyList<PyralisAuthoringFactGroupRow> FactGroups { get; }
        public IReadOnlyList<PyralisAuthoringContractGroupRow> ContractGroups { get; }

        public static PyralisAuthoringFactsProjection Build(Object activeSetup, PyralisAuthoringSetupGraph graph)
        {
            return new PyralisAuthoringFactsProjection(
                activeSetup,
                graph,
                PyralisAuthoringSetupGraphProjection.BuildCookbookFacts(graph),
                PyralisAuthoringSetupGraphProjection.BuildReflectiveContractRows(graph));
        }

        private static IReadOnlyList<PyralisAuthoringFactKindSummaryRow> BuildFactKindRows(IReadOnlyList<PyralisAuthoringFact> facts)
        {
            if (facts == null)
                return Array.Empty<PyralisAuthoringFactKindSummaryRow>();

            List<PyralisAuthoringFactKindSummaryRow> rows = new List<PyralisAuthoringFactKindSummaryRow>();
            Array kinds = Enum.GetValues(typeof(PyralisAuthoringFactKind));
            for (int i = 0; i < kinds.Length; i++)
            {
                PyralisAuthoringFactKind kind = (PyralisAuthoringFactKind)kinds.GetValue(i);
                int count = facts.Count(fact => fact != null && fact.Kind == kind);
                if (count > 0)
                    rows.Add(new PyralisAuthoringFactKindSummaryRow(kind, count));
            }

            return rows;
        }

        private static IReadOnlyList<PyralisAuthoringFactGroupRow> BuildFactGroups(IReadOnlyList<PyralisAuthoringFact> facts)
        {
            if (facts == null)
                return Array.Empty<PyralisAuthoringFactGroupRow>();

            List<PyralisAuthoringFactGroupRow> rows = new List<PyralisAuthoringFactGroupRow>();
            Array kinds = Enum.GetValues(typeof(PyralisAuthoringFactKind));
            for (int i = 0; i < kinds.Length; i++)
            {
                PyralisAuthoringFactKind kind = (PyralisAuthoringFactKind)kinds.GetValue(i);
                PyralisAuthoringFact[] group = facts
                    .Where(fact => fact != null && fact.Kind == kind)
                    .ToArray();
                if (group.Length > 0)
                    rows.Add(new PyralisAuthoringFactGroupRow(kind, group));
            }

            return rows;
        }

        private static IReadOnlyList<PyralisAuthoringContractGroupRow> BuildContractGroups(
            IReadOnlyList<PyralisAuthoringReflectiveContractGraphRow> contracts)
        {
            if (contracts == null)
                return Array.Empty<PyralisAuthoringContractGroupRow>();

            return contracts
                .Where(row => row?.Contract != null)
                .GroupBy(row => string.IsNullOrWhiteSpace(row.Contract.AuthoringCategory) ? "Uncategorized" : row.Contract.AuthoringCategory)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new PyralisAuthoringContractGroupRow(group.Key, group.ToArray()))
                .ToArray();
        }
    }

    public enum PyralisAuthoringSelectedIntentStatus
    {
        MissingGraphEvidence,
        NextAfterProof,
        ProofEnhancer,
        SupportsProofNeedsSetup,
        NeedsSetupBeforeProof,
        InProofPath,
        SupportsProof
    }

    public sealed class PyralisAuthoringSelectedIntentStatusRow
    {
        public PyralisAuthoringSelectedIntentStatusRow(
            PyralisAuthoringIntentDescriptorProjection descriptor,
            PyralisAuthoringSelectedIntentStatus status,
            string routeEvidence,
            PyralisAuthoringGraphEvidenceState evidenceState,
            string summary,
            string nextSurface)
        {
            Descriptor = descriptor;
            Status = status;
            RouteEvidence = routeEvidence ?? string.Empty;
            EvidenceState = evidenceState;
            Summary = summary ?? string.Empty;
            NextSurface = nextSurface ?? string.Empty;
        }

        public PyralisAuthoringIntentDescriptorProjection Descriptor { get; }
        public string StableId => Descriptor?.StableId ?? string.Empty;
        public string Label => Descriptor?.LeafLabel ?? "Unknown";
        public string Group => Descriptor?.Group ?? string.Empty;
        public string Subgroup => Descriptor?.Subgroup ?? string.Empty;
        public PyralisAuthoringSelectedIntentStatus Status { get; }
        public string RouteEvidence { get; }
        public PyralisAuthoringGraphEvidenceState EvidenceState { get; }
        public string Summary { get; }
        public string NextSurface { get; }
    }

    public sealed class PyralisAuthoringMapExportProjection
    {
        public PyralisAuthoringMapExportProjection(
            PyralisAuthoringSetupGraph graph,
            IReadOnlyList<PyralisAuthoringGraphNode> nodes,
            IReadOnlyList<PyralisAuthoringGraphEdge> edges,
            IReadOnlyList<PyralisAuthoringSetupGraphRow> mapRows,
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> mapConnections,
            IReadOnlyList<PyralisAuthoringGraphNode> sceneSurfaces,
            IReadOnlyList<PyralisAuthoringGraphAuditRow> sceneSetupIssues)
        {
            Graph = graph;
            Nodes = nodes ?? Array.Empty<PyralisAuthoringGraphNode>();
            Edges = edges ?? Array.Empty<PyralisAuthoringGraphEdge>();
            MapRows = mapRows ?? Array.Empty<PyralisAuthoringSetupGraphRow>();
            MapConnections = mapConnections ?? Array.Empty<PyralisAuthoringGraphConnectionRow>();
            SceneSurfaces = sceneSurfaces ?? Array.Empty<PyralisAuthoringGraphNode>();
            SceneSetupIssues = sceneSetupIssues ?? Array.Empty<PyralisAuthoringGraphAuditRow>();
        }

        public PyralisAuthoringSetupGraph Graph { get; }
        public IReadOnlyList<PyralisAuthoringGraphNode> Nodes { get; }
        public IReadOnlyList<PyralisAuthoringGraphEdge> Edges { get; }
        public IReadOnlyList<PyralisAuthoringSetupGraphRow> MapRows { get; }
        public IReadOnlyList<PyralisAuthoringGraphConnectionRow> MapConnections { get; }
        public IReadOnlyList<PyralisAuthoringGraphNode> SceneSurfaces { get; }
        public IReadOnlyList<PyralisAuthoringGraphAuditRow> SceneSetupIssues { get; }
    }

    public sealed class PyralisAuthoringGraphSummaryProjection
    {
        public PyralisAuthoringGraphSummaryProjection(
            int nodeCount,
            int edgeCount,
            int unknownNodeCount,
            int missingNodeCount,
            int blockedNodeCount,
            int contractMetadataIssueCount,
            int contractInventoryNodeCount,
            int proofBlockerCount,
            int dependencyPressureCount,
            int contractNodeCount,
            int hygieneUnknownRowCount,
            int hygieneMissingRowCount,
            int hygieneBlockedRowCount)
        {
            NodeCount = nodeCount;
            EdgeCount = edgeCount;
            UnknownNodeCount = unknownNodeCount;
            MissingNodeCount = missingNodeCount;
            BlockedNodeCount = blockedNodeCount;
            ContractMetadataIssueCount = contractMetadataIssueCount;
            ContractInventoryNodeCount = contractInventoryNodeCount;
            ProofBlockerCount = proofBlockerCount;
            DependencyPressureCount = dependencyPressureCount;
            ContractNodeCount = contractNodeCount;
            HygieneUnknownRowCount = hygieneUnknownRowCount;
            HygieneMissingRowCount = hygieneMissingRowCount;
            HygieneBlockedRowCount = hygieneBlockedRowCount;
        }

        public int NodeCount { get; }
        public int EdgeCount { get; }
        public int UnknownNodeCount { get; }
        public int MissingNodeCount { get; }
        public int BlockedNodeCount { get; }
        public int SetupReadinessUnknownNodeCount => UnknownNodeCount;
        public int SetupReadinessMissingNodeCount => MissingNodeCount;
        public int SetupReadinessBlockedNodeCount => BlockedNodeCount;
        public int ContractMetadataIssueCount { get; }
        public int ContractInventoryNodeCount { get; }
        public int ProofBlockerCount { get; }
        public int DependencyPressureCount { get; }
        public int ContractNodeCount { get; }
        public int HygieneUnknownRowCount { get; }
        public int HygieneMissingRowCount { get; }
        public int HygieneBlockedRowCount { get; }
    }

    public sealed class PyralisAuthoringGuideTraceProjection
    {
        public PyralisAuthoringGuideTraceProjection(
            PyralisAuthoringSetupGraph graph,
            PyralisAuthoringRouteWorkingProjection route,
            IReadOnlyList<PyralisAuthoringGraphNode> supportingContracts,
            IReadOnlyList<PyralisAuthoringRouteDiagnosticQuestionRow> diagnosticQuestions)
        {
            Graph = graph;
            Route = route;
            SupportingContracts = supportingContracts ?? Array.Empty<PyralisAuthoringGraphNode>();
            DiagnosticQuestions = diagnosticQuestions ?? Array.Empty<PyralisAuthoringRouteDiagnosticQuestionRow>();
        }

        public PyralisAuthoringSetupGraph Graph { get; }
        public PyralisAuthoringRouteWorkingProjection Route { get; }
        public IReadOnlyList<PyralisAuthoringGraphNode> SupportingContracts { get; }
        public IReadOnlyList<PyralisAuthoringRouteDiagnosticQuestionRow> DiagnosticQuestions { get; }
    }

    public static class PyralisAuthoringSetupGraphProjection
    {
        public static PyralisAuthoringMapExportProjection BuildMapExportProjection(PyralisAuthoringSetupGraph graph)
        {
            PyralisAuthoringGraphNode[] mapNodes = BuildMapExportNodes(graph);
            HashSet<string> mapNodeIds = new HashSet<string>(
                mapNodes.Select(node => node.StableId),
                StringComparer.Ordinal);
            PyralisAuthoringGraphEdge[] mapEdges = BuildMapExportEdges(graph, mapNodeIds);

            return new PyralisAuthoringMapExportProjection(
                graph,
                mapNodes,
                mapEdges,
                BuildSetupMapRows(graph),
                BuildMapConnectionRows(graph),
                FindSceneSurfaceNodes(graph),
                BuildMapSceneSetupIssueRows(graph));
        }

        public static PyralisAuthoringGuideTraceProjection BuildGuideTraceProjection(PyralisAuthoringSetupGraph graph)
        {
            PyralisAuthoringRouteWorkingProjection route = BuildRouteWorkingProjection(graph);
            return new PyralisAuthoringGuideTraceProjection(
                graph,
                route,
                BuildRouteSupportingContractNodes(graph, route),
                BuildRouteDiagnosticQuestions(graph, route));
        }

        public static PyralisAuthoringGraphSummaryProjection BuildGraphSummaryProjection(
            PyralisAuthoringSetupGraph graph,
            IReadOnlyList<PyralisSourceDependencyHygieneRecord> dependencyRecords,
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> proofBlockers)
        {
            PyralisAuthoringGraphNode[] nodes = graph?.Nodes.ToArray() ?? Array.Empty<PyralisAuthoringGraphNode>();
            PyralisAuthoringGraphNode[] integrityNodes = nodes
                .Where(node => node != null && IsGraphIntegrityNode(node))
                .ToArray();
            PyralisAuthoringGraphAuditRow[] hygieneRows = BuildHygieneDetailRows(graph).ToArray();

            return new PyralisAuthoringGraphSummaryProjection(
                nodes.Length,
                graph?.Edges.Count ?? 0,
                integrityNodes.Count(node => node.EvidenceState == PyralisAuthoringGraphEvidenceState.Unknown),
                integrityNodes.Count(node => node.EvidenceState == PyralisAuthoringGraphEvidenceState.Missing),
                integrityNodes.Count(node => node.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked),
                nodes.Count(IsContractMetadataIssueNode),
                nodes.Count(node => node != null && (node.Kind == PyralisAuthoringGraphNodeKind.Contract || node.SourceContract != null)),
                proofBlockers?.Count ?? 0,
                dependencyRecords?.Count(record => record != null && record.Risk != PyralisSourceDependencyRisk.Low) ?? 0,
                nodes.Count(node => node != null && (node.Kind == PyralisAuthoringGraphNodeKind.Contract || node.SourceContract != null)),
                hygieneRows.Count(row => row != null && row.EvidenceState == PyralisAuthoringGraphEvidenceState.Unknown),
                hygieneRows.Count(row => row != null && row.EvidenceState == PyralisAuthoringGraphEvidenceState.Missing),
                hygieneRows.Count(row => row != null && row.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked));
        }

        private static PyralisAuthoringGraphNode[] BuildMapExportNodes(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringGraphNode>();

            return graph.Nodes
                .Where(IsMapExportNode)
                .OrderBy(node => node.StableId, StringComparer.Ordinal)
                .ToArray();
        }

        private static PyralisAuthoringGraphEdge[] BuildMapExportEdges(
            PyralisAuthoringSetupGraph graph,
            HashSet<string> mapNodeIds)
        {
            if (graph == null || mapNodeIds == null || mapNodeIds.Count == 0)
                return Array.Empty<PyralisAuthoringGraphEdge>();

            return graph.Edges
                .Where(edge => edge != null
                    && mapNodeIds.Contains(edge.FromNodeId)
                    && mapNodeIds.Contains(edge.ToNodeId))
                .OrderBy(edge => edge.FromNodeId, StringComparer.Ordinal)
                .ThenBy(edge => edge.ToNodeId, StringComparer.Ordinal)
                .ToArray();
        }

        private static bool IsMapExportNode(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return false;

            if (node.Kind == PyralisAuthoringGraphNodeKind.Contract
                || node.SourceContract != null
                || node.SourceKind == PyralisAuthoringGraphSourceKind.AuthoringContract)
            {
                return false;
            }

            if (IsContractMetadataIssueNode(node))
                return false;

            switch (node.Kind)
            {
                case PyralisAuthoringGraphNodeKind.SetupChain:
                case PyralisAuthoringGraphNodeKind.RouteShape:
                case PyralisAuthoringGraphNodeKind.SceneSurface:
                case PyralisAuthoringGraphNodeKind.UnitySurfaceRequirement:
                case PyralisAuthoringGraphNodeKind.ValidationEvidence:
                    return true;
                case PyralisAuthoringGraphNodeKind.AssignmentField:
                    return node.SourceKind == PyralisAuthoringGraphSourceKind.Reflection
                        || node.SourceKind == PyralisAuthoringGraphSourceKind.RuntimeValidation
                        || node.SourceKind == PyralisAuthoringGraphSourceKind.CoreSetup;
                default:
                    return false;
            }
        }

        private static bool IsGraphIntegrityNode(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return false;

            return !IsContractMetadataIssueNode(node)
                && node.Kind != PyralisAuthoringGraphNodeKind.Contract
                && node.SourceContract == null;
        }

        private static bool IsContractMetadataIssueNode(PyralisAuthoringGraphNode node)
        {
            return node != null
                && !string.IsNullOrWhiteSpace(node.IssueCode)
                && node.IssueCode.StartsWith("ContractMetadata.", StringComparison.Ordinal);
        }

        private static IReadOnlyList<PyralisAuthoringGraphNode> BuildRouteSupportingContractNodes(
            PyralisAuthoringSetupGraph graph,
            PyralisAuthoringRouteWorkingProjection route)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringGraphNode>();

            HashSet<string> contractNodeIds = new HashSet<string>(StringComparer.Ordinal);
            AddRouteContractNodeIds(contractNodeIds, route?.OrderedSteps);
            AddProofSupportContractNodeIds(contractNodeIds, route?.ProofSupport);

            return graph.Nodes
                .Where(node => node != null
                    && contractNodeIds.Contains(node.StableId)
                    && (node.Kind == PyralisAuthoringGraphNodeKind.Contract || node.SourceContract != null))
                .ToArray();
        }

        private static void AddRouteContractNodeIds(
            HashSet<string> contractNodeIds,
            IReadOnlyList<PyralisAuthoringRouteStepRow> orderedSteps)
        {
            if (contractNodeIds == null || orderedSteps == null)
                return;

            for (int i = 0; i < orderedSteps.Count; i++)
            {
                PyralisAuthoringGraphNode node = orderedSteps[i]?.Node;
                if (node != null && (node.Kind == PyralisAuthoringGraphNodeKind.Contract || node.SourceContract != null))
                    contractNodeIds.Add(node.StableId);
            }
        }

        private static void AddProofSupportContractNodeIds(
            HashSet<string> contractNodeIds,
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> proofSupport)
        {
            if (contractNodeIds == null || proofSupport == null)
                return;

            for (int i = 0; i < proofSupport.Count; i++)
            {
                PyralisAuthoringGraphNode node = proofSupport[i]?.From;
                if (node != null && (node.Kind == PyralisAuthoringGraphNodeKind.Contract || node.SourceContract != null))
                    contractNodeIds.Add(node.StableId);
            }
        }

        public static IReadOnlyList<PyralisAuthoringSetupGraphRow> BuildSetupMapRows(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringSetupGraphRow>();

            List<PyralisAuthoringSetupGraphRow> rows = new List<PyralisAuthoringSetupGraphRow>
            {
                Row(graph, "Gameplay Root", "bootstrap.root", "Scene object that starts the session."),
                Row(graph, "Session Definition", "session.definition", "Asset that names game rules and participants."),
                Row(graph, "Game Mode", "mode.definition", "Ruleset that owns rule-level defaults and module-owned capabilities."),
                BuildCapabilitiesRow(graph),
                Row(graph, "Control Shape", "route.shape", "Participant ownership shape compiled from route evidence."),
                Row(graph, "Join Policy", "route.participant-topology", "Participant topology, join policy, and spawn timing compiled from session/input/spawn evidence."),
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
                Row(graph, "Join Policy", "route.participant-topology", isOptional: IsNodeOptional(graph, "route.participant-topology")),
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
                .Where(row => IsMapSceneSetupIssue(graph, row))
                .ToArray();
        }

        private static bool IsMapSceneSetupIssue(PyralisAuthoringSetupGraph graph, PyralisAuthoringGraphAuditRow row)
        {
            PyralisAuthoringGraphNode node = row?.Node;
            if (node == null)
                return false;

            if (ShouldSuppressParticipantTopologyRouteContext(graph, node.StableId))
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

            return GetProjectionMetadata(node).Audience == PyralisAuthoringProjectionAudience.Map;
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

        public static IReadOnlyList<PyralisAuthoringResolvedSetupRequirement> BuildResolvedSetupRequirements(
            PyralisAuthoringSetupGraph graph)
        {
            return BuildResolvedSetupRequirements(graph, BuildProofBlockerRows(graph));
        }

        private static IReadOnlyList<PyralisAuthoringResolvedSetupRequirement> BuildResolvedSetupRequirements(
            PyralisAuthoringSetupGraph graph,
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> proofBlockers)
        {
            if (graph == null)
                return Array.Empty<PyralisAuthoringResolvedSetupRequirement>();

            List<PyralisAuthoringResolvedSetupRequirement> requirements = new List<PyralisAuthoringResolvedSetupRequirement>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            AddResolvedSetupRequirements(requirements, seen, proofBlockers);

            foreach (PyralisAuthoringGraphNode node in graph.Nodes)
            {
                if (!IsResolvedSetupRequirementNode(node))
                    continue;

                AddResolvedSetupRequirement(requirements, seen, node);
            }

            return requirements.ToArray();
        }

        private static void AddResolvedSetupRequirements(
            List<PyralisAuthoringResolvedSetupRequirement> requirements,
            HashSet<string> seen,
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> proofBlockers)
        {
            if (proofBlockers == null)
                return;

            for (int i = 0; i < proofBlockers.Count; i++)
            {
                PyralisAuthoringGraphNode node = proofBlockers[i]?.To;
                if (!IsResolvedSetupRequirementNode(node))
                    continue;

                AddResolvedSetupRequirement(requirements, seen, node);
            }
        }

        private static void AddResolvedSetupRequirement(
            List<PyralisAuthoringResolvedSetupRequirement> requirements,
            HashSet<string> seen,
            PyralisAuthoringGraphNode node)
        {
            if (requirements == null || seen == null || node == null)
                return;

            string key = node.StableId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(key) || !seen.Add(key))
                return;

            requirements.Add(BuildResolvedSetupRequirement(node));
        }

        private static PyralisAuthoringResolvedSetupRequirement BuildResolvedSetupRequirement(PyralisAuthoringGraphNode node)
        {
            if (TryBuildSceneRootServiceRequirement(node, out PyralisAuthoringResolvedSetupRequirement requirement))
                return requirement;

            PyralisAuthoringNativeAction? nativeAction = node.NativeAction;
            return new PyralisAuthoringResolvedSetupRequirement(
                node,
                node.StableId,
                GetRequirementLabel(node),
                FirstNonEmpty(node.Guidance, node.BlockingReason, node.Label),
                FirstNonEmpty(GetRequirementOwnerLabel(node), node.StableId),
                node.NativeSetup,
                node.AssignmentFields,
                nativeAction);
        }

        private static bool TryBuildSceneRootServiceRequirement(
            PyralisAuthoringGraphNode node,
            out PyralisAuthoringResolvedSetupRequirement requirement)
        {
            requirement = null;
            if (node == null
                || node.SourceKind != PyralisAuthoringGraphSourceKind.SceneReadiness
                || !string.Equals(node.IssueCode, "SceneReadiness.SceneRoot", StringComparison.Ordinal)
                || !node.NativeAction.HasValue)
            {
                return false;
            }

            PyralisAuthoringNativeAction nativeAction = node.NativeAction.Value;
            string serviceName = PyralisAuthoringLabelUtility.GetNativeActionFieldOrComponentName(nativeAction);
            if (string.IsNullOrWhiteSpace(serviceName))
                return false;

            string label = FirstNonEmpty(node.Label, PyralisAuthoringLabelUtility.GetNativeActionDisplayLabel(nativeAction));
            string ownerLabel = PyralisAuthoringLabelUtility.GetNativeActionOwnerLabel(nativeAction);
            string message = FirstNonEmpty(node.Guidance, node.BlockingReason, label);

            requirement = new PyralisAuthoringResolvedSetupRequirement(
                node,
                node.StableId,
                label,
                message,
                ownerLabel,
                node.NativeSetup,
                Array.Empty<string>(),
                nativeAction);
            return true;
        }

        private static bool IsResolvedSetupRequirementNode(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return false;

            if (node.EvidenceState != PyralisAuthoringGraphEvidenceState.Blocked
                && node.EvidenceState != PyralisAuthoringGraphEvidenceState.Missing)
            {
                return false;
            }

            return node.WorkIntent == PyralisAuthoringGraphWorkIntent.RequiredSetup
                || (node.Kind == PyralisAuthoringGraphNodeKind.ValidationEvidence
                    && node.IssueSeverity == PyralisAuthoringIssueSeverity.Required);
        }

        private static string GetRequirementLabel(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return "Required setup";

            if (node.NativeAction.HasValue)
            {
                PyralisAuthoringNativeAction action = node.NativeAction.Value;
                string label = PyralisAuthoringLabelUtility.GetNativeActionDisplayLabel(action);
                if (!string.IsNullOrWhiteSpace(label))
                    return label;
            }

            return FirstNonEmpty(node.Label, "Required setup");
        }

        private static string GetRequirementOwnerLabel(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return string.Empty;

            if (node.NativeAction.HasValue)
            {
                PyralisAuthoringNativeAction action = node.NativeAction.Value;
                return PyralisAuthoringLabelUtility.GetNativeActionOwnerLabel(action);
            }

            if (node.AssignmentFields != null && node.AssignmentFields.Length > 0)
                return node.AssignmentFields[0];

            if (node.SourceContract != null && !string.IsNullOrWhiteSpace(node.SourceContract.SetupNodeId))
                return node.SourceContract.SetupNodeId;

            if (node.SourceObject != null)
                return node.SourceObject.name + " (" + node.SourceObject.GetType().Name + ")";

            return node.StableId;
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

            RuntimeCapabilityFamily[] families = selection.DescriptorIds != null && selection.DescriptorIds.Length > 0
                ? PyralisAuthoringCapabilityDescriptorRegistry.BuildRuntimeFamiliesForDescriptors(
                    selection.DescriptorIds,
                    selection.Lane,
                    selection.Axioms)
                : PyralisAuthoringCapabilityDescriptorRegistry.BuildRuntimeFamilies(
                    selection.Capabilities,
                    selection.Lane,
                    selection.Axioms);
            string participantSummary = GetIntentParticipantRouteSummary(selection.ParticipantRoute);
            if (families.Any(family => family == RuntimeCapabilityFamily.CharacterPawnGameplay))
                return $"Route shape: participant with pawn. {participantSummary} Expect ParticipantDefinition -> PawnDefinition -> pawn prefab, with InputProfile on the participant controlling it.";
            if (families.Any(family => family == RuntimeCapabilityFamily.BoardCardTabletop))
                return $"Route shape: participant without pawn. {participantSummary} Expect seats, hands, board/card surfaces, cursor, UI, or action resolvers instead of a pawn prefab.";
            if (families.Any(family => family == RuntimeCapabilityFamily.ActionTargeting))
                return $"Route shape: participant action surface. {participantSummary} Expect an input or UI command surface that sends actions to a resolver.";

            return $"Route shape: participant control surface. {participantSummary} Wire at least one ParticipantDefinition, then add only the surfaces this intent actually needs.";
        }

        private static string GetIntentParticipantRouteSummary(PyralisIntentParticipantRoute route)
        {
            switch (route)
            {
                case PyralisIntentParticipantRoute.SoloLocal:
                    return "Intent is steering toward one local participant.";
                case PyralisIntentParticipantRoute.TwoLocalPlayers:
                    return "Intent is steering toward two local player seats and Unity PlayerInputManager join.";
                case PyralisIntentParticipantRoute.ThreeLocalPlayers:
                    return "Intent is steering toward three local player seats and Unity PlayerInputManager join.";
                case PyralisIntentParticipantRoute.FourLocalPlayers:
                    return "Intent is steering toward four local player seats and Unity PlayerInputManager join.";
                case PyralisIntentParticipantRoute.Networked:
                    return "Intent is steering toward network-authority participants.";
                case PyralisIntentParticipantRoute.HybridLocalNetworked:
                    return "Intent is steering toward local player seats plus network authority.";
                default:
                    return "Participant count is inferred from authored setup.";
            }
        }

        public static PyralisAuthoringGraphNode FindRouteShapeNode(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return null;

            return graph.TryFindNode("route.shape", out PyralisAuthoringGraphNode node) ? node : null;
        }

        public static string BuildProofPrioritySummary(PyralisAuthoringSetupGraph graph)
        {
            PyralisAuthoringGraphNode proof = FindCurrentProofNode(graph);
            if (proof == null)
                return "No proof yet. Choose one Intent ingredient so Pyralis can name the playable test.";
            if (!IsResolvedProofNode(proof))
                return "No proof target yet. Assign an authored gameplay route or choose one Intent ingredient so Pyralis can name the playable test.";

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
            return PyralisAuthoringVocabularyRegistry.AllFacts
                .Where(IsFactsCookbookFact)
                .ToArray();
        }

        private static bool IsFactsCookbookFact(PyralisAuthoringFact fact)
        {
            if (fact == null)
                return false;

            return fact.Kind != PyralisAuthoringFactKind.RouteIntent
                && fact.Kind != PyralisAuthoringFactKind.RuntimeCapability
                && fact.Kind != PyralisAuthoringFactKind.CustomizationMoment
                && fact.Kind != PyralisAuthoringFactKind.Proof;
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

        public static IReadOnlyList<PyralisAuthoringSelectedIntentStatusRow> BuildSelectedIntentStatusRows(
            PyralisAuthoringSetupGraph graph)
        {
            PyralisAuthoringIntentSelection selection = graph?.IntentSelection;
            if (selection == null)
                return Array.Empty<PyralisAuthoringSelectedIntentStatusRow>();

            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors =
                PyralisAuthoringCapabilityDescriptorRegistry.BuildIntentProjectionDescriptors(selection.Lane, selection.Axioms);
            PyralisAuthoringIntentProjection projection =
                PyralisAuthoringIntentProjection.Build(selection, descriptors);
            return BuildSelectedIntentStatusRows(graph, projection);
        }

        public static IReadOnlyList<PyralisAuthoringSelectedIntentStatusRow> BuildSelectedIntentStatusRows(
            PyralisAuthoringSetupGraph graph,
            PyralisAuthoringIntentProjection intentProjection)
        {
            IReadOnlyList<PyralisAuthoringIntentDescriptorProjection> selectedDescriptors =
                intentProjection?.SelectedDescriptors ?? Array.Empty<PyralisAuthoringIntentDescriptorProjection>();
            if (selectedDescriptors.Count == 0)
                return Array.Empty<PyralisAuthoringSelectedIntentStatusRow>();

            PyralisAuthoringRouteWorkingProjection route = BuildRouteWorkingProjection(graph);
            List<PyralisAuthoringSelectedIntentStatusRow> rows = new List<PyralisAuthoringSelectedIntentStatusRow>();
            for (int i = 0; i < selectedDescriptors.Count; i++)
            {
                PyralisAuthoringIntentDescriptorProjection descriptor = selectedDescriptors[i];
                if (descriptor == null)
                    continue;

                rows.Add(BuildSelectedIntentStatusRow(graph, route, descriptor));
            }

            return rows
                .OrderBy(row => GetSelectedIntentStatusRank(row.Status))
                .ThenBy(row => row.Group, StringComparer.Ordinal)
                .ThenBy(row => row.Label, StringComparer.Ordinal)
                .ToArray();
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
                    Array.Empty<PyralisAuthoringResolvedSetupRequirement>(),
                    Array.Empty<PyralisAuthoringGraphConnectionRow>(),
                    Array.Empty<PyralisAuthoringGraphConnectionRow>());
            }

            PyralisAuthoringRouteStepRow[] criticalPath = BuildRouteCriticalPathRows(graph).ToArray();
            PyralisAuthoringRouteStepRow[] proofEnhancers = BuildRouteProofEnhancerRows(graph).ToArray();
            PyralisAuthoringRouteStepRow[] canWait = BuildRouteCanWaitRows(graph).ToArray();
            PyralisAuthoringGraphNode proof = FindCurrentProofNode(graph);
            PyralisAuthoringRouteStepRow[] orderedSteps = BuildOrderedRouteStepRows(proof, criticalPath, proofEnhancers);
            PyralisAuthoringGraphConnectionRow[] proofBlockers = BuildProofBlockerRows(graph).ToArray();
            PyralisAuthoringResolvedSetupRequirement[] requiredSetup = BuildResolvedSetupRequirements(graph, proofBlockers).ToArray();
            PyralisAuthoringGraphConnectionRow[] proofSupport = BuildDirectProofSupportRows(graph).ToArray();

            return new PyralisAuthoringRouteWorkingProjection(
                graph.RouteName,
                proof,
                orderedSteps,
                criticalPath,
                proofEnhancers,
                canWait,
                requiredSetup,
                proofBlockers,
                proofSupport);
        }

        public static IReadOnlyList<PyralisAuthoringRouteDiagnosticQuestionRow> BuildRouteDiagnosticQuestions(
            PyralisAuthoringSetupGraph graph,
            PyralisAuthoringRouteWorkingProjection route)
        {
            PyralisAuthoringRouteStepRow currentAction = route?.CurrentAction;
            IReadOnlyList<PyralisAuthoringRouteStepRow> orderedSteps = route?.OrderedSteps ?? Array.Empty<PyralisAuthoringRouteStepRow>();
            IReadOnlyList<PyralisAuthoringRouteStepRow> criticalPath = route?.CriticalPath ?? Array.Empty<PyralisAuthoringRouteStepRow>();
            IReadOnlyList<PyralisAuthoringRouteStepRow> proofEnhancers = route?.ProofEnhancers ?? Array.Empty<PyralisAuthoringRouteStepRow>();
            IReadOnlyList<PyralisAuthoringRouteStepRow> canWait = route?.CanWait ?? Array.Empty<PyralisAuthoringRouteStepRow>();
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> proofBlockers = route?.ProofBlockers ?? Array.Empty<PyralisAuthoringGraphConnectionRow>();
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> proofSupport = route?.ProofSupport ?? Array.Empty<PyralisAuthoringGraphConnectionRow>();

            List<PyralisAuthoringRouteDiagnosticQuestionRow> questions = new List<PyralisAuthoringRouteDiagnosticQuestionRow>
            {
                new PyralisAuthoringRouteDiagnosticQuestionRow(
                    "What is the next route action?",
                    currentAction != null
                        ? $"{currentAction.Label}: {FirstNonEmpty(currentAction.UnityActionLabel, currentAction.Message, currentAction.Reason)}"
                        : orderedSteps.Count > 0
                            ? "Required setup is clear for the projected proof. Use the full fresh-scene path to review how the route is assembled, then attempt the Play Mode proof."
                            : "No ordered steps were generated. Check whether the active setup graph has a resolved setup context."),
                new PyralisAuthoringRouteDiagnosticQuestionRow(
                    "What blocks the selected proof?",
                    proofBlockers.Count > 0
                        ? string.Join("; ", proofBlockers.Take(6).Select(row => row.ToLabel))
                        : "No proof blocker links are present in the current graph."),
                new PyralisAuthoringRouteDiagnosticQuestionRow(
                    "What is the full fresh-scene card path?",
                    criticalPath.Count > 0
                        ? string.Join(" -> ", criticalPath.Select(row => row.Label).Concat(new[] { GetOverviewProofLabel(graph) }))
                        : "No setup-card path is present yet. Check core setup graph nodes, runtime validation evidence, and proof target resolution."),
                new PyralisAuthoringRouteDiagnosticQuestionRow(
                    "What can wait until after this proof?",
                    canWait.Count > 0
                        ? string.Join("; ", canWait.Take(8).Select(row => row.Label))
                        : "No can-wait setup cards were projected for this proof route."),
                new PyralisAuthoringRouteDiagnosticQuestionRow(
                    "Which proof enhancers are useful but not blockers?",
                    proofEnhancers.Count > 0
                        ? string.Join("; ", proofEnhancers.Take(6).Select(row => row.Label))
                        : "No proof enhancers were projected for this route."),
                new PyralisAuthoringRouteDiagnosticQuestionRow(
                    "Which contracts are proof context rather than route cards?",
                    proofSupport.Count > 0
                        ? string.Join("; ", proofSupport.Take(8).Select(row => row.FromLabel))
                        : "No direct proof-support contracts are present yet. The ordered setup cards should still come from core setup graph nodes and validation evidence."),
                new PyralisAuthoringRouteDiagnosticQuestionRow(
                    "Where should incorrect guidance be fixed?",
                    "Fix the source that emitted the step: contract meaning, dependency reflection, local runtime validation, scene-readiness validation, or graph projection. Do not hardcode a one-off Guide/Hygiene sentence.")
            };

            if (orderedSteps.Count == 0)
            {
                questions.Add(new PyralisAuthoringRouteDiagnosticQuestionRow(
                    "Why is the route empty?",
                    graph == null || graph.Source == null
                        ? "No active setup source was resolved and no Intent-projected route cards were generated. Select or pin a Bootstrap, SessionDefinition, GameModeDefinition, ParticipantDefinition, PawnDefinition, component, or profile, or choose Intent settings that imply a proof."
                        : "No ordered setup cards were generated for the active graph. Check route analysis, proof target resolution, and critical-path projection metadata."));
            }

            return questions;
        }

        private static PyralisAuthoringSelectedIntentStatusRow BuildSelectedIntentStatusRow(
            PyralisAuthoringSetupGraph graph,
            PyralisAuthoringRouteWorkingProjection route,
            PyralisAuthoringIntentDescriptorProjection descriptor)
        {
            if (descriptor == null)
            {
                return new PyralisAuthoringSelectedIntentStatusRow(
                    descriptor,
                    PyralisAuthoringSelectedIntentStatus.MissingGraphEvidence,
                    "None",
                    PyralisAuthoringGraphEvidenceState.Unknown,
                    "No selected descriptor was available.",
                    "Intent");
            }

            if (TryFindMatchingRouteStep(route?.CriticalPath, descriptor.StableId, out PyralisAuthoringRouteStepRow criticalStep))
            {
                PyralisAuthoringSelectedIntentStatus status =
                    criticalStep.EvidenceState == PyralisAuthoringGraphEvidenceState.Missing
                    || criticalStep.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked
                        ? PyralisAuthoringSelectedIntentStatus.NeedsSetupBeforeProof
                        : PyralisAuthoringSelectedIntentStatus.InProofPath;
                return new PyralisAuthoringSelectedIntentStatusRow(
                    descriptor,
                    status,
                    "CriticalPath",
                    criticalStep.EvidenceState,
                    FirstNonEmpty(criticalStep.Message, criticalStep.Reason, $"{descriptor.LeafLabel} is part of the selected proof route."),
                    status == PyralisAuthoringSelectedIntentStatus.NeedsSetupBeforeProof ? "Guide" : "Route");
            }

            if (TryFindMatchingRouteStep(route?.ProofEnhancers, descriptor.StableId, out PyralisAuthoringRouteStepRow enhancerStep))
            {
                return new PyralisAuthoringSelectedIntentStatusRow(
                    descriptor,
                    PyralisAuthoringSelectedIntentStatus.ProofEnhancer,
                    "ProofEnhancer",
                    enhancerStep.EvidenceState,
                    FirstNonEmpty(enhancerStep.Message, enhancerStep.Reason, $"{descriptor.LeafLabel} can make the selected proof clearer."),
                    "Guide");
            }

            if (TryFindMatchingRouteStep(route?.CanWait, descriptor.StableId, out PyralisAuthoringRouteStepRow canWaitStep))
            {
                return new PyralisAuthoringSelectedIntentStatusRow(
                    descriptor,
                    PyralisAuthoringSelectedIntentStatus.NextAfterProof,
                    "CanWait",
                    canWaitStep.EvidenceState,
                    FirstNonEmpty(canWaitStep.Message, canWaitStep.Reason, $"{descriptor.LeafLabel} is selected intent follow-through after the current proof checkpoint."),
                    "Guide");
            }

            if (TryFindMatchingProofSupport(route?.ProofSupport, descriptor.StableId, out PyralisAuthoringGraphConnectionRow proofSupport))
            {
                PyralisAuthoringGraphEvidenceState evidenceState = proofSupport.From != null
                    ? proofSupport.From.EvidenceState
                    : PyralisAuthoringGraphEvidenceState.Unknown;
                PyralisAuthoringSelectedIntentStatus status =
                    evidenceState == PyralisAuthoringGraphEvidenceState.Ready
                        ? PyralisAuthoringSelectedIntentStatus.SupportsProof
                        : PyralisAuthoringSelectedIntentStatus.SupportsProofNeedsSetup;
                return new PyralisAuthoringSelectedIntentStatusRow(
                    descriptor,
                    status,
                    "ProofSupport",
                    evidenceState,
                    status == PyralisAuthoringSelectedIntentStatus.SupportsProof
                        ? $"{descriptor.LeafLabel} is represented in the current graph and supports the selected proof."
                        : $"{descriptor.LeafLabel} supports the selected proof, but the graph has not promoted it to ready setup evidence yet.",
                    status == PyralisAuthoringSelectedIntentStatus.SupportsProof ? "Route" : "Guide");
            }

            PyralisAuthoringGraphNode graphNode = FindSelectedIntentGraphNode(graph, descriptor.StableId);
            if (graphNode != null)
            {
                return new PyralisAuthoringSelectedIntentStatusRow(
                    descriptor,
                    PyralisAuthoringSelectedIntentStatus.NextAfterProof,
                    "GraphNode",
                    graphNode.EvidenceState,
                    FirstNonEmpty(graphNode.Guidance, $"{descriptor.LeafLabel} is visible in the graph as selected intent follow-through after the current proof checkpoint."),
                    "Guide");
            }

            return new PyralisAuthoringSelectedIntentStatusRow(
                descriptor,
                PyralisAuthoringSelectedIntentStatus.MissingGraphEvidence,
                "None",
                PyralisAuthoringGraphEvidenceState.Unknown,
                $"{descriptor.LeafLabel} is selected in Intent, but no concrete graph evidence is linked yet. Check whether the capability contract, reflection, or validation should expose setup evidence.",
                "Facts");
        }

        private static bool TryFindMatchingRouteStep(
            IReadOnlyList<PyralisAuthoringRouteStepRow> rows,
            string stableId,
            out PyralisAuthoringRouteStepRow match)
        {
            match = null;
            if (rows == null || string.IsNullOrWhiteSpace(stableId))
                return false;

            for (int i = 0; i < rows.Count; i++)
            {
                PyralisAuthoringRouteStepRow row = rows[i];
                if (row?.Node == null)
                    continue;

                if (IsSelectedIntentNodeMatch(row.Node, stableId))
                {
                    match = row;
                    return true;
                }
            }

            return false;
        }

        private static bool TryFindMatchingProofSupport(
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> proofSupport,
            string stableId,
            out PyralisAuthoringGraphConnectionRow match)
        {
            match = null;
            if (proofSupport == null || string.IsNullOrWhiteSpace(stableId))
                return false;

            for (int i = 0; i < proofSupport.Count; i++)
            {
                PyralisAuthoringGraphConnectionRow row = proofSupport[i];
                if (row == null)
                    continue;

                if (IsSelectedIntentNodeMatch(row.From, stableId))
                {
                    match = row;
                    return true;
                }
            }

            return false;
        }

        private static PyralisAuthoringGraphNode FindSelectedIntentGraphNode(PyralisAuthoringSetupGraph graph, string stableId)
        {
            if (graph == null || string.IsNullOrWhiteSpace(stableId))
                return null;

            if (graph.TryFindNode(stableId, out PyralisAuthoringGraphNode direct))
                return direct;

            string contractId = "contract." + stableId;
            if (graph.TryFindNode(contractId, out PyralisAuthoringGraphNode contractNode))
                return contractNode;

            return graph.Nodes.FirstOrDefault(node => IsSelectedIntentNodeMatch(node, stableId));
        }

        private static bool IsSelectedIntentNodeMatch(PyralisAuthoringGraphNode node, string stableId)
        {
            if (node == null || string.IsNullOrWhiteSpace(stableId))
                return false;

            return string.Equals(node.StableId, stableId, StringComparison.Ordinal)
                || string.Equals(node.StableId, "contract." + stableId, StringComparison.Ordinal)
                || (node.SourceContract != null
                    && string.Equals(node.SourceContract.StableId, stableId, StringComparison.Ordinal));
        }

        private static int GetSelectedIntentStatusRank(PyralisAuthoringSelectedIntentStatus status)
        {
            return status switch
            {
                PyralisAuthoringSelectedIntentStatus.NeedsSetupBeforeProof => 0,
                PyralisAuthoringSelectedIntentStatus.SupportsProofNeedsSetup => 1,
                PyralisAuthoringSelectedIntentStatus.ProofEnhancer => 2,
                PyralisAuthoringSelectedIntentStatus.InProofPath => 3,
                PyralisAuthoringSelectedIntentStatus.SupportsProof => 4,
                PyralisAuthoringSelectedIntentStatus.NextAfterProof => 5,
                _ => 6
            };
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
                PyralisAuthoringRouteStepPhase.Proof,
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
            AddCoreSetupRouteSteps(graph, rows, added, currentStep, ref sequence);
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
                    "This card is useful vocabulary for the route, but it is not part of the selected proof's critical path.",
                    graph: graph);
            }

            return rows.ToArray();
        }

        private static void AddCoreSetupRouteSteps(
            PyralisAuthoringSetupGraph graph,
            List<PyralisAuthoringRouteStepRow> rows,
            HashSet<string> added,
            PyralisAuthoringGraphNode currentStep,
            ref int sequence)
        {
            if (graph == null)
                return;

            PyralisAuthoringGraphNode[] coreSetupNodes = graph.Nodes
                .Where(node => node != null
                    && GetProjectionMetadata(node).Group == PyralisAuthoringProjectionGroup.SetupChain
                    && IsCriticalRouteSetupCard(node))
                .OrderBy(GetRouteSetupCardRank)
                .ThenBy(node => node.Label, StringComparer.Ordinal)
                .ToArray();

            for (int i = 0; i < coreSetupNodes.Length; i++)
            {
                PyralisAuthoringGraphNode node = coreSetupNodes[i];
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
            if (!ShouldSuppressParticipantTopologyRouteContext(graph, "route.participant-topology"))
                AddRouteStepById(graph, rows, added, "route.participant-topology", currentStep, ref sequence);
            AddRouteStepById(graph, rows, added, "participant.default", currentStep, ref sequence);
            AddRouteStepById(graph, rows, added, "pawn.definition", currentStep, ref sequence);
        }

        private static bool ShouldSuppressParticipantTopologyRouteContext(PyralisAuthoringSetupGraph graph, string stableId)
        {
            if (!string.Equals(stableId, "route.participant-topology", StringComparison.Ordinal))
                return false;

            PyralisSetupRouteAnalysis route = graph?.RouteAnalysis;
            if (route == null
                || !route.HasPlayerInputManager
                || !route.HasLocalJoinPolicyConflict())
            {
                return false;
            }

            return graph.TryFindNode("setup.resolve-participant-join-policy", out PyralisAuthoringGraphNode coreSetupNode)
                && coreSetupNode != null
                && (coreSetupNode.EvidenceState == PyralisAuthoringGraphEvidenceState.Missing
                    || coreSetupNode.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked);
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
                    && GetProjectionMetadata(node).Group == PyralisAuthoringProjectionGroup.ReflectedAssignment
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
                    && GetProjectionMetadata(node).Group == PyralisAuthoringProjectionGroup.RuntimeValidation
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
                    && GetProjectionMetadata(node).Group == PyralisAuthoringProjectionGroup.PrefabReadiness
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

            PyralisAuthoringGraphNode node = FindFirstUnresolvedCoreSetupNode(graph);
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

            PyralisAuthoringProjectionGroup group = GetProjectionMetadata(node).Group;
            return group != PyralisAuthoringProjectionGroup.SceneEvidence;
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

            if (node.SetupDomain == PyralisAuthoringGraphSetupDomain.GameplayRoot
                && node.Kind == PyralisAuthoringGraphNodeKind.ValidationEvidence
                && GetProjectionMetadata(node).Group == PyralisAuthoringProjectionGroup.SetupChain)
            {
                return false;
            }

            return node.WorkIntent == PyralisAuthoringGraphWorkIntent.RequiredSetup
                || node.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked
                || node.EvidenceState == PyralisAuthoringGraphEvidenceState.Missing;
        }

        private static bool IsPrefabReadinessRouteCard(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return false;

            return node.SetupDomain == PyralisAuthoringGraphSetupDomain.PawnPrefab
                || node.SetupDomain == PyralisAuthoringGraphSetupDomain.PawnMotor
                || node.SetupDomain == PyralisAuthoringGraphSetupDomain.PawnInput
                || node.SetupDomain == PyralisAuthoringGraphSetupDomain.PawnPresentation
                || node.SetupDomain == PyralisAuthoringGraphSetupDomain.PawnAnimation;
        }

        private static bool IsRouteProofEnhancerNode(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return false;

            PyralisAuthoringProjectionGroup group = GetProjectionMetadata(node).Group;
            return group == PyralisAuthoringProjectionGroup.SetupChain
                || group == PyralisAuthoringProjectionGroup.PrefabReadiness
                || group == PyralisAuthoringProjectionGroup.RuntimeValidation;
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

            if (node.SetupDomain == PyralisAuthoringGraphSetupDomain.Scoring
                || node.SetupDomain == PyralisAuthoringGraphSetupDomain.Tabletop
                || node.SetupDomain == PyralisAuthoringGraphSetupDomain.Settings
                || node.SetupDomain == PyralisAuthoringGraphSetupDomain.Playfield)
            {
                return node.EvidenceState != PyralisAuthoringGraphEvidenceState.Missing
                    && node.EvidenceState != PyralisAuthoringGraphEvidenceState.Blocked;
            }

            return node.EvidenceState == PyralisAuthoringGraphEvidenceState.CandidateDetected
                && GetProjectionMetadata(node).Group == PyralisAuthoringProjectionGroup.SetupChain;
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
                return "This setup card must be cleared before the selected proof is believable.";
            }

            if (node.WorkIntent == PyralisAuthoringGraphWorkIntent.ProofEnhancer
                || node.EvidenceState == PyralisAuthoringGraphEvidenceState.CandidateDetected)
            {
                return "This card can make the selected proof easier to judge, but it can wait until required setup is clear.";
            }

            return "This setup card is already satisfied on the route toward the selected proof.";
        }

        private static int GetRouteSetupCardRank(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return 200;

            return GetProjectionMetadata(node).SortRank;
        }

        private static PyralisAuthoringProjectionMetadata GetProjectionMetadata(PyralisAuthoringGraphNode node)
        {
            if (node == null)
            {
                return new PyralisAuthoringProjectionMetadata(
                    PyralisAuthoringProjectionGroup.Reference,
                    PyralisAuthoringProjectionAudience.Reference,
                    PyralisAuthoringRouteStepPhase.Reference,
                    PyralisAuthoringGraphSetupDomain.Unknown,
                    200);
            }

            PyralisAuthoringProjectionGroup group = GetProjectionGroup(node);
            PyralisAuthoringRouteStepPhase phase = GetProjectionPhase(group);
            PyralisAuthoringProjectionAudience audience = GetProjectionAudience(node, group);
            return new PyralisAuthoringProjectionMetadata(
                group,
                audience,
                phase,
                node.SetupDomain,
                GetSetupDomainRank(node.SetupDomain));
        }

        private static PyralisAuthoringProjectionGroup GetProjectionGroup(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return PyralisAuthoringProjectionGroup.Reference;

            switch (node.Kind)
            {
                case PyralisAuthoringGraphNodeKind.SetupChain:
                    return node.SetupDomain == PyralisAuthoringGraphSetupDomain.GameplayRoot
                        ? PyralisAuthoringProjectionGroup.Foundation
                        : PyralisAuthoringProjectionGroup.SetupChain;
                case PyralisAuthoringGraphNodeKind.RouteShape:
                    return PyralisAuthoringProjectionGroup.SetupChain;
                case PyralisAuthoringGraphNodeKind.AssignmentField:
                    return PyralisAuthoringProjectionGroup.ReflectedAssignment;
                case PyralisAuthoringGraphNodeKind.SceneSurface:
                case PyralisAuthoringGraphNodeKind.UnitySurfaceRequirement:
                    return PyralisAuthoringProjectionGroup.SceneEvidence;
                case PyralisAuthoringGraphNodeKind.Contract:
                    return PyralisAuthoringProjectionGroup.Contract;
                case PyralisAuthoringGraphNodeKind.Capability:
                    return PyralisAuthoringProjectionGroup.Capability;
                case PyralisAuthoringGraphNodeKind.Proof:
                    return PyralisAuthoringProjectionGroup.Proof;
                case PyralisAuthoringGraphNodeKind.ValidationEvidence:
                    if (IsPrefabReadinessRouteCard(node))
                        return PyralisAuthoringProjectionGroup.PrefabReadiness;
                    if (IsCoreSetupDomain(node.SetupDomain))
                        return PyralisAuthoringProjectionGroup.SetupChain;
                    if (IsSceneEvidenceDomain(node.SetupDomain))
                        return PyralisAuthoringProjectionGroup.SceneEvidence;
                    return PyralisAuthoringProjectionGroup.RuntimeValidation;
                default:
                    return PyralisAuthoringProjectionGroup.Reference;
            }
        }

        private static PyralisAuthoringRouteStepPhase GetProjectionPhase(PyralisAuthoringProjectionGroup group)
        {
            switch (group)
            {
                case PyralisAuthoringProjectionGroup.Foundation:
                    return PyralisAuthoringRouteStepPhase.Foundation;
                case PyralisAuthoringProjectionGroup.SetupChain:
                case PyralisAuthoringProjectionGroup.ReflectedAssignment:
                    return PyralisAuthoringRouteStepPhase.SetupChain;
                case PyralisAuthoringProjectionGroup.PrefabReadiness:
                case PyralisAuthoringProjectionGroup.SceneEvidence:
                    return PyralisAuthoringRouteStepPhase.SceneEvidence;
                case PyralisAuthoringProjectionGroup.RuntimeValidation:
                    return PyralisAuthoringRouteStepPhase.Validation;
                case PyralisAuthoringProjectionGroup.Contract:
                case PyralisAuthoringProjectionGroup.Capability:
                    return PyralisAuthoringRouteStepPhase.Capability;
                case PyralisAuthoringProjectionGroup.Proof:
                    return PyralisAuthoringRouteStepPhase.Proof;
                default:
                    return PyralisAuthoringRouteStepPhase.Reference;
            }
        }

        private static PyralisAuthoringProjectionAudience GetProjectionAudience(
            PyralisAuthoringGraphNode node,
            PyralisAuthoringProjectionGroup group)
        {
            if (node == null)
                return PyralisAuthoringProjectionAudience.Reference;

            if (group == PyralisAuthoringProjectionGroup.Contract
                || group == PyralisAuthoringProjectionGroup.Capability
                || group == PyralisAuthoringProjectionGroup.Proof)
            {
                return PyralisAuthoringProjectionAudience.Reference;
            }

            if (node.Kind == PyralisAuthoringGraphNodeKind.ValidationEvidence
                || node.Kind == PyralisAuthoringGraphNodeKind.SetupChain
                || node.Kind == PyralisAuthoringGraphNodeKind.RouteShape
                || node.Kind == PyralisAuthoringGraphNodeKind.UnitySurfaceRequirement
                || node.Kind == PyralisAuthoringGraphNodeKind.SceneSurface
                || node.Kind == PyralisAuthoringGraphNodeKind.AssignmentField)
            {
                return PyralisAuthoringProjectionAudience.Map;
            }

            return PyralisAuthoringProjectionAudience.Reference;
        }

        private static bool IsCoreSetupDomain(PyralisAuthoringGraphSetupDomain setupDomain)
        {
            switch (setupDomain)
            {
                case PyralisAuthoringGraphSetupDomain.GameplayRoot:
                case PyralisAuthoringGraphSetupDomain.LifetimeScope:
                case PyralisAuthoringGraphSetupDomain.Session:
                case PyralisAuthoringGraphSetupDomain.GameMode:
                case PyralisAuthoringGraphSetupDomain.RouteCapabilities:
                case PyralisAuthoringGraphSetupDomain.RouteShape:
                case PyralisAuthoringGraphSetupDomain.Participant:
                case PyralisAuthoringGraphSetupDomain.ParticipantTopology:
                case PyralisAuthoringGraphSetupDomain.Input:
                case PyralisAuthoringGraphSetupDomain.PlayerInputManager:
                case PyralisAuthoringGraphSetupDomain.Spawn:
                case PyralisAuthoringGraphSetupDomain.PawnDefinition:
                case PyralisAuthoringGraphSetupDomain.PawnPrefab:
                case PyralisAuthoringGraphSetupDomain.Camera:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsSceneEvidenceDomain(PyralisAuthoringGraphSetupDomain setupDomain)
        {
            return setupDomain == PyralisAuthoringGraphSetupDomain.SceneSurface
                || setupDomain == PyralisAuthoringGraphSetupDomain.SceneReadiness
                || setupDomain == PyralisAuthoringGraphSetupDomain.UserInterface
                || setupDomain == PyralisAuthoringGraphSetupDomain.Playfield;
        }

        private static int GetSetupDomainRank(PyralisAuthoringGraphSetupDomain setupDomain)
        {
            switch (setupDomain)
            {
                case PyralisAuthoringGraphSetupDomain.GameplayRoot:
                    return 0;
                case PyralisAuthoringGraphSetupDomain.LifetimeScope:
                    return 5;
                case PyralisAuthoringGraphSetupDomain.Session:
                    return 10;
                case PyralisAuthoringGraphSetupDomain.GameMode:
                    return 20;
                case PyralisAuthoringGraphSetupDomain.RouteCapabilities:
                case PyralisAuthoringGraphSetupDomain.RouteShape:
                    return 30;
                case PyralisAuthoringGraphSetupDomain.Participant:
                case PyralisAuthoringGraphSetupDomain.ParticipantTopology:
                    return 40;
                case PyralisAuthoringGraphSetupDomain.Input:
                case PyralisAuthoringGraphSetupDomain.PlayerInputManager:
                    return 50;
                case PyralisAuthoringGraphSetupDomain.PawnDefinition:
                case PyralisAuthoringGraphSetupDomain.PawnPrefab:
                    return 60;
                case PyralisAuthoringGraphSetupDomain.Spawn:
                    return 70;
                case PyralisAuthoringGraphSetupDomain.Camera:
                    return 80;
                case PyralisAuthoringGraphSetupDomain.PawnPresentation:
                case PyralisAuthoringGraphSetupDomain.PawnAnimation:
                    return 90;
                case PyralisAuthoringGraphSetupDomain.PawnMotor:
                case PyralisAuthoringGraphSetupDomain.PawnInput:
                    return 100;
                case PyralisAuthoringGraphSetupDomain.SceneSurface:
                case PyralisAuthoringGraphSetupDomain.SceneReadiness:
                    return 110;
                case PyralisAuthoringGraphSetupDomain.UserInterface:
                    return 120;
                case PyralisAuthoringGraphSetupDomain.Settings:
                    return 130;
                case PyralisAuthoringGraphSetupDomain.Playfield:
                    return 140;
                case PyralisAuthoringGraphSetupDomain.Scoring:
                case PyralisAuthoringGraphSetupDomain.Tabletop:
                    return 150;
                case PyralisAuthoringGraphSetupDomain.Networking:
                    return 160;
                case PyralisAuthoringGraphSetupDomain.FeatureContract:
                    return 170;
                default:
                    return 200;
            }
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

            return BuildRouteStepDisplayKey(row.Node, PyralisRouteStepLens.Empty);
        }

        private static string BuildRouteStepDisplayKey(PyralisAuthoringGraphNode node, PyralisRouteStepLens lens)
        {
            if (node == null)
                return string.Empty;

            string issueCode = node.IssueCode ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(issueCode))
                return "route-issue:" + issueCode;

            if (!string.IsNullOrWhiteSpace(node.StableId))
                return "route-node:" + node.StableId;

            string label = !string.IsNullOrWhiteSpace(lens.LabelOverride) ? lens.LabelOverride : node.Label;
            return "route-domain:"
                + node.SetupDomain
                + "|"
                + NormalizeRouteStepKey(label);
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
            PyralisAuthoringNativeAction nativeAction = PyralisPawnNativeActionVocabulary.GetNativeAction(issueKind, laneTag);
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
            PyralisAuthoringNativeAction nativeAction = PyralisPawnNativeActionVocabulary.GetNativeAction(issueKind, laneTag);
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

            if (TryGetParticipantPawnIssueKind(node.IssueCode, out PyralisParticipantPawnIssueKind issueKind))
                return issueKind;

            if (string.Equals(node.StableId, "pawn.definition", StringComparison.Ordinal)
                && graph?.RouteAnalysis != null
                && graph.RouteAnalysis.ParticipantPawnIssueKind != PyralisParticipantPawnIssueKind.None)
            {
                return graph.RouteAnalysis.ParticipantPawnIssueKind;
            }

            return PyralisParticipantPawnIssueKind.None;
        }

        private static bool TryGetParticipantPawnIssueKind(string issueCode, out PyralisParticipantPawnIssueKind issueKind)
        {
            issueKind = PyralisParticipantPawnIssueKind.None;
            if (string.IsNullOrWhiteSpace(issueCode))
                return false;

            const string prefix = "ParticipantPawn.";
            string value = issueCode.StartsWith(prefix, StringComparison.Ordinal)
                ? issueCode.Substring(prefix.Length)
                : issueCode;
            return Enum.TryParse(value, false, out issueKind);
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

            if (node.SourceContract == null)
                return false;

            if (IsPresentationLaneMismatch(graph, node))
                return false;

            if (IsPawnMovementProof(proofTargetId) && node.Kind == PyralisAuthoringGraphNodeKind.Contract)
            {
                return IsMovementProofContract(node);
            }

            return IsDirectProofCapability(proofTargetId, node.CapabilityFamily)
                || IsDirectProofCapability(proofTargetId, node.AuthoringCapability)
                || IsMovementProofContract(node);
        }

        private static bool IsDirectProofCapability(string proofTargetId, RuntimeCapabilityFamily family)
        {
            if (IsPawnMovementProof(proofTargetId))
            {
                return family == RuntimeCapabilityFamily.PlatformCore
                    || family == RuntimeCapabilityFamily.CharacterPawnGameplay
                    || family == RuntimeCapabilityFamily.CameraInput
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

            if (IsPawnMovementProof(proofTargetId))
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

        private static bool IsPawnMovementProof(string proofTargetId)
        {
            return string.Equals(proofTargetId, "proof.1p-pawn-movement", StringComparison.Ordinal)
                || string.Equals(proofTargetId, "proof.local-pawn-join", StringComparison.Ordinal);
        }

        private static int GetDirectProofSupportRank(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return 100;

            return node.Kind switch
            {
                PyralisAuthoringGraphNodeKind.Capability when node.CapabilityFamily == RuntimeCapabilityFamily.PlatformCore => 0,
                PyralisAuthoringGraphNodeKind.Capability when node.CapabilityFamily == RuntimeCapabilityFamily.CharacterPawnGameplay => 1,
                PyralisAuthoringGraphNodeKind.Contract when IsMovementOrInputDomain(node) => 2,
                PyralisAuthoringGraphNodeKind.Contract when IsPresentationDomain(node) => 3,
                PyralisAuthoringGraphNodeKind.Capability when node.CapabilityFamily == RuntimeCapabilityFamily.AnimationPresentation => 4,
                PyralisAuthoringGraphNodeKind.Capability when node.CapabilityFamily == RuntimeCapabilityFamily.CameraInput => 5,
                PyralisAuthoringGraphNodeKind.Capability => 8,
                _ => 10
            };
        }

        private static bool IsPresentationLaneMismatch(PyralisAuthoringSetupGraph graph, PyralisAuthoringGraphNode node)
        {
            if (node == null || node.SourceContract == null)
                return false;

            RuntimeCapabilityLaneTag lane = ResolvePresentationLane(graph);
            if (lane == RuntimeCapabilityLaneTag.Mixed)
                return false;

            ActorPresentationMode presentationMode = ToPresentationMode(lane);
            if (node.SourceContract.SupportedPresentationModes.Length > 0)
                return !node.SourceContract.SupportsPresentationMode(presentationMode);

            if ((node.SourceContract.Axioms & AuthoringWorldAxiom.Dimensions3D) != 0)
                return lane != RuntimeCapabilityLaneTag.ThirdPerson3D;

            if ((node.SourceContract.Axioms & AuthoringWorldAxiom.Dimensions2D) != 0)
                return lane == RuntimeCapabilityLaneTag.ThirdPerson3D;

            return false;
        }

        private static RuntimeCapabilityLaneTag ResolvePresentationLane(PyralisAuthoringSetupGraph graph)
        {
            GameObject pawnPrefab = graph?.RouteAnalysis?.Pawn != null ? graph.RouteAnalysis.Pawn.pawnPrefab : null;
            if (pawnPrefab != null)
            {
                if (pawnPrefab.GetComponentInChildren<NeonBlack.Gameplay.Modules.Character.Pawn2DMovementComponent>(true) != null
                    || pawnPrefab.GetComponentInChildren<NeonBlack.Gameplay.Modules.Character.Pawn2DPresentationComponent>(true) != null
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

            return RuntimeCapabilityLaneTag.Mixed;
        }

        private static ActorPresentationMode ToPresentationMode(RuntimeCapabilityLaneTag lane)
        {
            switch (lane)
            {
                case RuntimeCapabilityLaneTag.Billboard2_5D:
                    return ActorPresentationMode.Billboard2_5D;
                case RuntimeCapabilityLaneTag.ThirdPerson3D:
                    return ActorPresentationMode.ThirdPerson3D;
                default:
                    return ActorPresentationMode.Sprite2D;
            }
        }

        private static bool IsMovementProofContract(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return false;

            AuthoringCapability excluded = AuthoringCapability.Combat
                | AuthoringCapability.MeleeFlow
                | AuthoringCapability.RangedFlow
                | AuthoringCapability.CombatState
                | AuthoringCapability.CombatSensors
                | AuthoringCapability.Inventory
                | AuthoringCapability.Stats
                | AuthoringCapability.Networking;
            if ((node.AuthoringCapability & excluded) != 0)
            {
                return false;
            }

            return IsMovementOrInputDomain(node)
                || IsPresentationDomain(node)
                || node.SetupDomain == PyralisAuthoringGraphSetupDomain.PawnDefinition
                || node.SetupDomain == PyralisAuthoringGraphSetupDomain.PawnPrefab;
        }

        private static bool IsMovementOrInputDomain(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return false;

            AuthoringCapability direct = AuthoringCapability.Input
                | AuthoringCapability.Movement
                | AuthoringCapability.KineticMotor2D
                | AuthoringCapability.KineticMotor3D
                | AuthoringCapability.Steering2D
                | AuthoringCapability.Steering3D
                | AuthoringCapability.Traversal;
            return (node.AuthoringCapability & direct) != 0
                || node.SetupDomain == PyralisAuthoringGraphSetupDomain.Input
                || node.SetupDomain == PyralisAuthoringGraphSetupDomain.PlayerInputManager
                || node.SetupDomain == PyralisAuthoringGraphSetupDomain.PawnInput
                || node.SetupDomain == PyralisAuthoringGraphSetupDomain.PawnMotor;
        }

        private static bool IsPresentationDomain(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return false;

            AuthoringCapability direct = AuthoringCapability.Animation
                | AuthoringCapability.VFX
                | AuthoringCapability.Camera;
            return (node.AuthoringCapability & direct) != 0
                || node.SetupDomain == PyralisAuthoringGraphSetupDomain.PawnPresentation
                || node.SetupDomain == PyralisAuthoringGraphSetupDomain.PawnAnimation
                || node.SetupDomain == PyralisAuthoringGraphSetupDomain.Camera;
        }

        internal static PyralisAuthoringRouteStepPhase GetPhase(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return PyralisAuthoringRouteStepPhase.Reference;

            if (node.WorkIntent == PyralisAuthoringGraphWorkIntent.Optional)
                return PyralisAuthoringRouteStepPhase.Optional;

            return GetProjectionMetadata(node).Phase;
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
                        : !string.IsNullOrWhiteSpace(currentAction.Proof)
                            ? currentAction.Proof
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

            if (route.ReadyForProof && route.Proof != null)
            {
                string proofMessage = route.Proof.NativeSetup.Length > 0
                    ? route.Proof.NativeSetup[0]
                    : route.Proof.Guidance;
                if (string.IsNullOrWhiteSpace(proofMessage))
                    proofMessage = "Enter Play Mode and run the selected proof.";

                string proofDetail = !string.IsNullOrWhiteSpace(route.Proof.BlockingReason)
                    ? route.Proof.BlockingReason
                    : "The graph has no required setup blockers for the selected proof.";

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
                    "Missing Contract Metadata",
                    PyralisAuthoringGraphEvidenceState.Missing,
                    BuildHygieneMissingContractMetadataRows(graph)),
                new PyralisAuthoringGraphAuditSection(
                    "Validation Evidence Missing Metadata",
                    PyralisAuthoringGraphEvidenceState.Missing,
                    BuildHygieneRuntimeValidationMissingMetadataRows(graph)),
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
                || string.Equals(section.Label, "Unvalidated Graph Nodes", StringComparison.Ordinal)
                || string.Equals(section.Label, "Missing Contract Metadata", StringComparison.Ordinal)
                || string.Equals(section.Label, "Validation Evidence Missing Metadata", StringComparison.Ordinal);
        }

        private static IReadOnlyList<PyralisAuthoringGraphAuditRow> BuildHygieneMissingContractMetadataRows(PyralisAuthoringSetupGraph graph)
        {
            return graph.Nodes
                .Where(node => node != null
                    && node.IssueCode.StartsWith("ContractMetadata.", StringComparison.Ordinal))
                .Select(node => new PyralisAuthoringGraphAuditRow(node))
                .ToArray();
        }

        private static IReadOnlyList<PyralisAuthoringGraphAuditRow> BuildHygieneRuntimeValidationMissingMetadataRows(PyralisAuthoringSetupGraph graph)
        {
            return graph.Nodes
                .Where(node => node != null
                    && node.Kind == PyralisAuthoringGraphNodeKind.ValidationEvidence
                    && GetProjectionMetadata(node).Group == PyralisAuthoringProjectionGroup.RuntimeValidation
                    && node.IssueCode.StartsWith("ValidationMetadata.", StringComparison.Ordinal))
                .Select(node => new PyralisAuthoringGraphAuditRow(node))
                .ToArray();
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

            return GetProjectionMetadata(node).Audience == PyralisAuthoringProjectionAudience.Map;
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
                    && node.SourceContract != null
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
            HashSet<string> added = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < route.RequiredSetup.Count; i++)
            {
                PyralisAuthoringResolvedSetupRequirement requirement = route.RequiredSetup[i];
                PyralisAuthoringOverviewIssue issue = BuildOverviewIssue(requirement?.ToRouteStep(i));
                if (issue != null && added.Add(requirement.StableId))
                    issues.Add(issue);
            }

            for (int i = 0; i < route.CriticalPath.Count; i++)
            {
                PyralisAuthoringOverviewIssue issue = BuildOverviewIssue(route.CriticalPath[i]);
                if (issue != null && added.Add(route.CriticalPath[i].StableId))
                    issues.Add(issue);
            }

            for (int i = 0; i < route.ProofEnhancers.Count; i++)
            {
                PyralisAuthoringOverviewIssue issue = BuildOverviewIssue(route.ProofEnhancers[i]);
                if (issue != null && added.Add(route.ProofEnhancers[i].StableId))
                    issues.Add(issue);
            }

            for (int i = 0; i < route.CanWait.Count; i++)
            {
                PyralisAuthoringOverviewIssue issue = BuildOverviewIssue(route.CanWait[i]);
                if (issue != null && added.Add(route.CanWait[i].StableId))
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
            bool requiredClear = route.ReadyForProof;
            PyralisAuthoringRouteStepRow firstRequired = route.CurrentAction;
            PyralisAuthoringGraphNode proofNode = FindCurrentProofNode(graph);
            items.Add(new PyralisAuthoringPlayModeChecklistItem(
                "Required setup",
                requiredClear,
                requiredClear ? "Do Now is clear." : firstRequired?.Message ?? "Clear the selected route's Do Now setup before Play Mode."));

            items.Add(new PyralisAuthoringPlayModeChecklistItem(
                "Proof target",
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
            return graph != null && graph.Source != null && BuildRouteWorkingProjection(graph).ReadyForProof;
        }

        public static string GetOverviewProofLabel(PyralisAuthoringSetupGraph graph)
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

        public static string GetOverviewProofGuidance(PyralisAuthoringSetupGraph graph)
        {
            return FindCurrentProofNode(graph)?.Guidance
                ?? "Create a Gameplay Root scene object with GameplaySessionBootstrap, then create and assign the first SessionDefinition asset.";
        }

        public static string GetOverviewProofSetupSurface(PyralisAuthoringSetupGraph graph)
        {
            PyralisAuthoringGraphNode proofNode = FindCurrentProofNode(graph);
            return proofNode != null && proofNode.NativeSetup.Length > 0
                ? proofNode.NativeSetup[0]
                : "Hierarchy object plus Project asset foundation.";
        }

        public static string GetOverviewProofSuccessCriteria(PyralisAuthoringSetupGraph graph)
        {
            return FindCurrentProofNode(graph)?.BlockingReason
                ?? "Overview can inspect the bootstrap route and name the active proof.";
        }

        public static string GetOverviewProofDeferUntilAfter(PyralisAuthoringSetupGraph graph)
        {
            PyralisAuthoringRouteStepRow currentAction = BuildRouteWorkingProjection(graph).CurrentAction;
            return currentAction != null
                ? "Defer expansion until this graph node is clear: " + currentAction.Label
                : "Defer broad polish until the graph-backed proof runs in Play Mode.";
        }

        public static string GetOverviewProofChainSummary(PyralisAuthoringSetupGraph graph)
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
                PyralisAuthoringGraphSourceKind.CoreSetup => "Core Setup",
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
                return PyralisAuthoringLabelUtility.GetNativeActionFieldOrComponentName(node.NativeAction.Value);

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

        private static PyralisAuthoringGraphNode FindFirstUnresolvedCoreSetupNode(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return null;

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                PyralisAuthoringGraphNode node = graph.Nodes[i];
                if (node == null)
                    continue;

                PyralisAuthoringProjectionGroup group = GetProjectionMetadata(node).Group;
                bool coreSetupEvidence = group == PyralisAuthoringProjectionGroup.SetupChain;
                bool reflectedAssignment = group == PyralisAuthoringProjectionGroup.ReflectedAssignment;
                if (!coreSetupEvidence && !reflectedAssignment)
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

            PyralisAuthoringProjectionMetadata metadata = GetProjectionMetadata(linkedNode);
            if (string.Equals(nodeId, "scene.surfaces", StringComparison.Ordinal))
                return metadata.Group == PyralisAuthoringProjectionGroup.SceneEvidence;

            if (metadata.Group == PyralisAuthoringProjectionGroup.SceneEvidence)
                return false;

            if (string.Equals(nodeId, "bootstrap.root", StringComparison.Ordinal))
                return IsCoreSetupDomain(metadata.OwnerDomain);

            if (string.Equals(nodeId, "participant.default", StringComparison.Ordinal)
                || string.Equals(nodeId, "pawn.definition", StringComparison.Ordinal))
            {
                return IsParticipantOrPawnDomain(metadata.OwnerDomain)
                    || metadata.Group == PyralisAuthoringProjectionGroup.RuntimeValidation
                    || metadata.Group == PyralisAuthoringProjectionGroup.SetupChain
                    || metadata.Group == PyralisAuthoringProjectionGroup.ReflectedAssignment;
            }

            return IsCoreSetupDomain(metadata.OwnerDomain);
        }

        private static bool IsParticipantOrPawnDomain(PyralisAuthoringGraphSetupDomain setupDomain)
        {
            switch (setupDomain)
            {
                case PyralisAuthoringGraphSetupDomain.Participant:
                case PyralisAuthoringGraphSetupDomain.ParticipantTopology:
                case PyralisAuthoringGraphSetupDomain.Input:
                case PyralisAuthoringGraphSetupDomain.PlayerInputManager:
                case PyralisAuthoringGraphSetupDomain.PawnDefinition:
                case PyralisAuthoringGraphSetupDomain.PawnPrefab:
                case PyralisAuthoringGraphSetupDomain.PawnMotor:
                case PyralisAuthoringGraphSetupDomain.PawnInput:
                case PyralisAuthoringGraphSetupDomain.PawnPresentation:
                case PyralisAuthoringGraphSetupDomain.PawnAnimation:
                    return true;
                default:
                    return false;
            }
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
                GameplaySessionBootstrap => "Scene startup and core setup root.",
                SessionDefinition => "Session contract for game rules, participants, local/network mode, and participant limits.",
                GameModeDefinition => "Rules contract that owns rule-level defaults, system switches, board/turn data, playfield, camera, and scene targets.",
                ParticipantDefinition => "Seat, player, NPC, hand, faction, or command owner in the session.",
                PawnDefinition => "Pawn prefab, profiles, direct module components, and presentation setup.",
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
                GameModeDefinition => "Inspect enabled systems, playfield, camera, board/turn data, and rule flags.",
                ParticipantDefinition => "Inspect default pawn, input profile, seat index, and auto-join ownership.",
                PawnDefinition => "Inspect pawn prefab and movement/input/presentation profiles.",
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
