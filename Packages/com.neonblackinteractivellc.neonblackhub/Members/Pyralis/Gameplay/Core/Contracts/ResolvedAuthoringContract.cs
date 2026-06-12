using System;
using NeonBlack.Gameplay.Presentation.Animation;

namespace NeonBlack.Gameplay.Core.Contracts
{
    public enum PyralisAuthoringConfidence
    {
        Unknown,
        Inferred,
        ConventionDerived,
        Explicit
    }

    /// <summary>
    /// The runtime-safe data model for an authoring contract.
    /// Derived from AuthoringContractAttribute or provided by IAuthoringContractProvider.
    /// </summary>
    public sealed class ResolvedAuthoringContract
    {
        public ResolvedAuthoringContract(
            string stableId,
            string displayName,
            string authoringCategory,
            Type requiredProfileType,
            string[] requiredRuntimeInterfaceNames = null,
            ActorPresentationMode[] supportedPresentationModes = null,
            ActorPresentationMode[] unsupportedPresentationModes = null,
            string unsupportedLaneMessage = null,
            string[] consumedActionRoles = null,
            string[] nativeSetup = null,
            string firstProofTargetId = null,
            Type sourceType = null,
            AuthoringWorldAxiom axioms = AuthoringWorldAxiom.None,
            string workIntent = null,
            PyralisAuthoringConfidence confidence = PyralisAuthoringConfidence.Unknown,
            string[] assignmentFields = null,
            string[] customizationMoments = null,
            string[] requiredComponentNames = null,
            AuthoringCapability capability = AuthoringCapability.None,
            AuthoringPriority priority = AuthoringPriority.Unspecified,
            int priorityValueOverride = 0,
            string deprecatedInVersion = null,
            string removableInVersion = null,
            string documentationURL = null,
            string expertAdvice = null,
            string moduleId = null,
            string relevance = null,
            string manualPath = null)
        {
            StableId = stableId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            AuthoringCategory = authoringCategory ?? string.Empty;
            RequiredProfileType = requiredProfileType;
            RequiredRuntimeInterfaceNames = requiredRuntimeInterfaceNames ?? Array.Empty<string>();
            SupportedPresentationModes = supportedPresentationModes ?? Array.Empty<ActorPresentationMode>();
            UnsupportedPresentationModes = unsupportedPresentationModes ?? Array.Empty<ActorPresentationMode>();
            UnsupportedLaneMessage = unsupportedLaneMessage ?? string.Empty;
            ConsumedActionRoles = consumedActionRoles ?? Array.Empty<string>();
            NativeSetup = nativeSetup ?? Array.Empty<string>();
            FirstProofTargetId = firstProofTargetId ?? string.Empty;
            SourceType = sourceType;
            Axioms = axioms;
            WorkIntent = workIntent ?? string.Empty;
            Confidence = confidence;
            AssignmentFields = assignmentFields ?? Array.Empty<string>();
            CustomizationMoments = customizationMoments ?? Array.Empty<string>();
            RequiredComponentNames = requiredComponentNames ?? Array.Empty<string>();
            Capability = capability;
            PriorityValueOverride = priorityValueOverride;
            DeprecatedInVersion = deprecatedInVersion ?? string.Empty;
            RemovableInVersion = removableInVersion ?? string.Empty;
            Priority = (int)priority;
            DocumentationURL = documentationURL ?? string.Empty;
            ExpertAdvice = expertAdvice ?? string.Empty;
            ModuleId = moduleId ?? string.Empty;
            Relevance = relevance ?? string.Empty;
            ManualPath = manualPath ?? string.Empty;
        }

        public string StableId { get; }
        public string DisplayName { get; }
        public string AuthoringCategory { get; }
        public string ModuleId { get; }
        public Type RequiredProfileType { get; }
        public string[] RequiredRuntimeInterfaceNames { get; }
        public ActorPresentationMode[] SupportedPresentationModes { get; }
        public ActorPresentationMode[] UnsupportedPresentationModes { get; }
        public string UnsupportedLaneMessage { get; }
        public string[] ConsumedActionRoles { get; }
        public string[] NativeSetup { get; }
        public string FirstProofTargetId { get; }
        public Type SourceType { get; }
        public AuthoringWorldAxiom Axioms { get; }
        public string WorkIntent { get; }
        public PyralisAuthoringConfidence Confidence { get; }
        public string[] AssignmentFields { get; }
        public string[] CustomizationMoments { get; }
        public string[] RequiredComponentNames { get; }
        public AuthoringCapability Capability { get; }
        public int Priority { get; }
        public int PriorityValueOverride { get; }
        public string DeprecatedInVersion { get; }
        public string RemovableInVersion { get; }
        public string DocumentationURL { get; }
        public string ExpertAdvice { get; }
        public string Relevance { get; }
        public string ManualPath { get; }

        public bool SupportsPresentationMode(ActorPresentationMode mode)
        {
            if (SupportedPresentationModes == null || SupportedPresentationModes.Length == 0)
                return true;

            for (int i = 0; i < SupportedPresentationModes.Length; i++)
            {
                if (SupportedPresentationModes[i] == mode)
                    return true;
            }

            return false;
        }

        public bool IsExplicitlyUnsupported(ActorPresentationMode mode)
        {
            if (UnsupportedPresentationModes == null || UnsupportedPresentationModes.Length == 0)
                return false;

            for (int i = 0; i < UnsupportedPresentationModes.Length; i++)
            {
                if (UnsupportedPresentationModes[i] == mode)
                    return true;
            }

            return false;
        }
    }
}
