using System;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    public enum PyralisAuthoringGraphNodeKind
    {
        Unknown,
        SetupChain,
        Capability,
        Contract,
        Proof,
        SceneSurface,
        UnitySurfaceRequirement,
        AssignmentField,
        ValidationEvidence
    }

    public enum PyralisAuthoringGraphSourceKind
    {
        Unknown,
        SetupProfile,
        CapabilityVocabulary,
        RuntimePattern,
        AuthoringContract,
        GrammarRegistry,
        SetupFlow,
        SceneReadiness,
        ProofVocabulary
    }

    public enum PyralisAuthoringGraphSourceOrigin
    {
        Unknown,
        UserAuthoredSetup,
        Reflection,
        Contract,
        RuntimeEvidence,
        SpineGrammar,
        GrammarFallback
    }

    public enum PyralisAuthoringGraphEvidenceState
    {
        Unknown,
        Optional,
        Missing,
        CandidateDetected,
        Ready,
        Blocked
    }

    public enum PyralisAuthoringGraphEdgeKind
    {
        RelatesTo,
        DependsOn,
        Satisfies,
        Recommends,
        SupportsProof,
        BlockedBy
    }

    public enum PyralisAuthoringGraphWorkIntent
    {
        Unknown,
        RequiredSetup,
        ProofEnhancer,
        FeatureCard,
        Optional,
        Reference
    }

    public sealed class PyralisAuthoringGraphNode
    {
        public PyralisAuthoringGraphNode(
            string stableId,
            string label,
            PyralisAuthoringGraphNodeKind kind,
            PyralisAuthoringGraphSourceKind sourceKind,
            PyralisAuthoringGraphEvidenceState evidenceState = PyralisAuthoringGraphEvidenceState.Unknown,
            RuntimeCapabilityFamily capabilityFamily = RuntimeCapabilityFamily.PlatformCore,
            AuthoringCapability authoringCapability = AuthoringCapability.None,
            string proofTargetId = null,
            string guidance = null,
            string[] nativeSetup = null,
            string[] assignmentFields = null,
            string[] customizationMoments = null,
            string blockingReason = null,
            PyralisAuthoringNativeAction? nativeAction = null,
            ResolvedAuthoringContract sourceContract = null,
            UnityEngine.Object sourceObject = null,
            PyralisAuthoringGraphSourceOrigin sourceOrigin = PyralisAuthoringGraphSourceOrigin.Unknown,
            PyralisAuthoringGraphWorkIntent workIntent = PyralisAuthoringGraphWorkIntent.Unknown,
            PyralisAuthoringIssueSeverity issueSeverity = PyralisAuthoringIssueSeverity.Info)
        {
            StableId = stableId ?? string.Empty;
            Label = label ?? string.Empty;
            Kind = kind;
            SourceKind = sourceKind;
            EvidenceState = evidenceState;
            CapabilityFamily = capabilityFamily;
            AuthoringCapability = authoringCapability;
            ProofTargetId = proofTargetId ?? string.Empty;
            Guidance = guidance ?? string.Empty;
            NativeSetup = nativeSetup ?? Array.Empty<string>();
            AssignmentFields = assignmentFields ?? Array.Empty<string>();
            CustomizationMoments = customizationMoments ?? Array.Empty<string>();
            BlockingReason = blockingReason ?? string.Empty;
            NativeAction = nativeAction;
            SourceContract = sourceContract;
            SourceObject = sourceObject;
            SourceOrigin = sourceOrigin == PyralisAuthoringGraphSourceOrigin.Unknown
                ? InferSourceOrigin(sourceKind)
                : sourceOrigin;
            WorkIntent = workIntent == PyralisAuthoringGraphWorkIntent.Unknown
                ? InferWorkIntent(kind, EvidenceState)
                : workIntent;
            IssueSeverity = issueSeverity == PyralisAuthoringIssueSeverity.Info
                ? InferIssueSeverity(kind, EvidenceState)
                : issueSeverity;
        }

        public string StableId { get; }
        public string Label { get; }
        public PyralisAuthoringGraphNodeKind Kind { get; }
        public PyralisAuthoringGraphSourceKind SourceKind { get; }
        public PyralisAuthoringGraphEvidenceState EvidenceState { get; }
        public RuntimeCapabilityFamily CapabilityFamily { get; }
        public AuthoringCapability AuthoringCapability { get; }
        public string ProofTargetId { get; }
        public string Guidance { get; }
        public string[] NativeSetup { get; }
        public string[] AssignmentFields { get; }
        public string[] CustomizationMoments { get; }
        public string BlockingReason { get; }
        public PyralisAuthoringNativeAction? NativeAction { get; }
        public ResolvedAuthoringContract SourceContract { get; }
        public UnityEngine.Object SourceObject { get; }
        public PyralisAuthoringGraphSourceOrigin SourceOrigin { get; }
        public PyralisAuthoringGraphWorkIntent WorkIntent { get; }
        public PyralisAuthoringIssueSeverity IssueSeverity { get; }

        private static PyralisAuthoringGraphSourceOrigin InferSourceOrigin(PyralisAuthoringGraphSourceKind sourceKind)
        {
            return sourceKind switch
            {
                PyralisAuthoringGraphSourceKind.SetupProfile => PyralisAuthoringGraphSourceOrigin.UserAuthoredSetup,
                PyralisAuthoringGraphSourceKind.RuntimePattern => PyralisAuthoringGraphSourceOrigin.UserAuthoredSetup,
                PyralisAuthoringGraphSourceKind.AuthoringContract => PyralisAuthoringGraphSourceOrigin.Contract,
                PyralisAuthoringGraphSourceKind.SetupFlow => PyralisAuthoringGraphSourceOrigin.RuntimeEvidence,
                PyralisAuthoringGraphSourceKind.SceneReadiness => PyralisAuthoringGraphSourceOrigin.RuntimeEvidence,
                PyralisAuthoringGraphSourceKind.CapabilityVocabulary => PyralisAuthoringGraphSourceOrigin.SpineGrammar,
                PyralisAuthoringGraphSourceKind.GrammarRegistry => PyralisAuthoringGraphSourceOrigin.SpineGrammar,
                PyralisAuthoringGraphSourceKind.ProofVocabulary => PyralisAuthoringGraphSourceOrigin.GrammarFallback,
                _ => PyralisAuthoringGraphSourceOrigin.Unknown
            };
        }

        private static PyralisAuthoringGraphWorkIntent InferWorkIntent(
            PyralisAuthoringGraphNodeKind kind,
            PyralisAuthoringGraphEvidenceState evidenceState)
        {
            if (evidenceState == PyralisAuthoringGraphEvidenceState.Blocked
                || evidenceState == PyralisAuthoringGraphEvidenceState.Missing)
            {
                return kind == PyralisAuthoringGraphNodeKind.SetupChain
                    || kind == PyralisAuthoringGraphNodeKind.UnitySurfaceRequirement
                    || kind == PyralisAuthoringGraphNodeKind.ValidationEvidence
                        ? PyralisAuthoringGraphWorkIntent.RequiredSetup
                        : PyralisAuthoringGraphWorkIntent.ProofEnhancer;
            }

            if (evidenceState == PyralisAuthoringGraphEvidenceState.CandidateDetected)
                return PyralisAuthoringGraphWorkIntent.ProofEnhancer;

            if (evidenceState == PyralisAuthoringGraphEvidenceState.Optional)
                return PyralisAuthoringGraphWorkIntent.Optional;

            return PyralisAuthoringGraphWorkIntent.Reference;
        }

        private static PyralisAuthoringIssueSeverity InferIssueSeverity(
            PyralisAuthoringGraphNodeKind kind,
            PyralisAuthoringGraphEvidenceState evidenceState)
        {
            if (evidenceState == PyralisAuthoringGraphEvidenceState.Blocked)
                return PyralisAuthoringIssueSeverity.Blocked;

            if (evidenceState == PyralisAuthoringGraphEvidenceState.Missing)
            {
                return kind == PyralisAuthoringGraphNodeKind.SetupChain
                    || kind == PyralisAuthoringGraphNodeKind.UnitySurfaceRequirement
                    || kind == PyralisAuthoringGraphNodeKind.ValidationEvidence
                        ? PyralisAuthoringIssueSeverity.Required
                        : PyralisAuthoringIssueSeverity.Recommended;
            }

            if (evidenceState == PyralisAuthoringGraphEvidenceState.CandidateDetected)
                return PyralisAuthoringIssueSeverity.Recommended;

            if (evidenceState == PyralisAuthoringGraphEvidenceState.Optional)
                return PyralisAuthoringIssueSeverity.Optional;

            return PyralisAuthoringIssueSeverity.Info;
        }
    }

    public sealed class PyralisAuthoringGraphEdge
    {
        public PyralisAuthoringGraphEdge(
            string fromNodeId,
            string toNodeId,
            PyralisAuthoringGraphEdgeKind kind,
            string label = null)
        {
            FromNodeId = fromNodeId ?? string.Empty;
            ToNodeId = toNodeId ?? string.Empty;
            Kind = kind;
            Label = label ?? string.Empty;
        }

        public string FromNodeId { get; }
        public string ToNodeId { get; }
        public PyralisAuthoringGraphEdgeKind Kind { get; }
        public string Label { get; }
    }
}
