using System;
using System.Collections.Generic;
using System.Reflection;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    public enum PyralisAuthoringFactKind
    {
        RouteFamily,
        RouteIntent,
        RuntimeCapability,
        FeatureContract,
        SetupNode,
        Definition,
        Profile,
        SceneComponent,
        PrefabComponent,
        AssignmentField,
        CustomizationMoment,
        Issue,
        Proof
    }

    public enum PyralisAuthoringFactSourceKind
    {
        HandAuthoredGuideCard,
        SetupFlow,
        FeatureContract,
        Validator,
        InspectorGuide,
        Reflection,
        Convention,
        SceneEvidence
    }

    public enum PyralisAuthoringConfidence
    {
        Unknown,
        Inferred,
        ConventionDerived,
        Explicit
    }

    public enum PyralisAuthoringIssueSeverity
    {
        Info,
        Optional,
        Recommended,
        Required,
        Blocked
    }

    public sealed class PyralisAuthoringFact
    {
        public PyralisAuthoringFact(
            string stableId,
            string displayName,
            PyralisAuthoringFactKind kind,
            PyralisAuthoringFactSourceKind sourceKind,
            PyralisAuthoringConfidence confidence,
            string summary,
            string routeRelevance,
            string firstProof,
            string[] goalTags = null,
            string[] laneTags = null,
            string[] unsupportedLaneTags = null,
            string[] requiredDefinitions = null,
            string[] requiredProfiles = null,
            string[] requiredSceneComponents = null,
            string[] requiredPrefabComponents = null,
            string[] assignmentFields = null,
            string[] customizationMoments = null,
            string[] canWait = null,
            PyralisAuthoringNativeAction[] nativeActions = null,
            string workIntent = null,
            string[] relatedStableIds = null)
        {
            StableId = stableId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            Kind = kind;
            SourceKind = sourceKind;
            Confidence = confidence;
            Summary = summary ?? string.Empty;
            RouteRelevance = routeRelevance ?? string.Empty;
            FirstProof = firstProof ?? string.Empty;
            GoalTags = goalTags ?? System.Array.Empty<string>();
            LaneTags = laneTags ?? System.Array.Empty<string>();
            UnsupportedLaneTags = unsupportedLaneTags ?? System.Array.Empty<string>();
            RequiredDefinitions = requiredDefinitions ?? System.Array.Empty<string>();
            RequiredProfiles = requiredProfiles ?? System.Array.Empty<string>();
            RequiredSceneComponents = requiredSceneComponents ?? System.Array.Empty<string>();
            RequiredPrefabComponents = requiredPrefabComponents ?? System.Array.Empty<string>();
            AssignmentFields = assignmentFields ?? System.Array.Empty<string>();
            CustomizationMoments = customizationMoments ?? System.Array.Empty<string>();
            CanWait = canWait ?? System.Array.Empty<string>();
            NativeActions = nativeActions ?? System.Array.Empty<PyralisAuthoringNativeAction>();
            WorkIntent = workIntent ?? string.Empty;
            RelatedStableIds = relatedStableIds ?? System.Array.Empty<string>();
        }

        public string StableId { get; }
        public string DisplayName { get; }
        public PyralisAuthoringFactKind Kind { get; }
        public PyralisAuthoringFactSourceKind SourceKind { get; }
        public PyralisAuthoringConfidence Confidence { get; }
        public string Summary { get; }
        public string RouteRelevance { get; }
        public string FirstProof { get; }
        public string[] GoalTags { get; }
        public string[] LaneTags { get; }
        public string[] UnsupportedLaneTags { get; }
        public string[] RequiredDefinitions { get; }
        public string[] RequiredProfiles { get; }
        public string[] RequiredSceneComponents { get; }
        public string[] RequiredPrefabComponents { get; }
        public string[] AssignmentFields { get; }
        public string[] CustomizationMoments { get; }
        public string[] CanWait { get; }
        public PyralisAuthoringNativeAction[] NativeActions { get; }
        public string WorkIntent { get; }
        public string[] RelatedStableIds { get; }

        public bool MatchesStableId(string stableId)
        {
            return string.Equals(StableId, stableId, System.StringComparison.Ordinal);
        }
    }

    public sealed class PyralisAuthoringIssue
    {
        public PyralisAuthoringIssue(
            string issueCode,
            PyralisAuthoringIssueSeverity severity,
            string workIntent,
            PyralisAuthoringEvidenceState evidenceState,
            string targetObject,
            string fieldOrComponent,
            PyralisAuthoringNativeAction? nativeAction,
            string reason)
        {
            IssueCode = issueCode ?? string.Empty;
            Severity = severity;
            WorkIntent = workIntent ?? string.Empty;
            EvidenceState = evidenceState;
            TargetObject = targetObject ?? string.Empty;
            FieldOrComponent = fieldOrComponent ?? string.Empty;
            NativeAction = nativeAction;
            Reason = reason ?? string.Empty;
        }

        public string IssueCode { get; }
        public PyralisAuthoringIssueSeverity Severity { get; }
        public string WorkIntent { get; }
        public PyralisAuthoringEvidenceState EvidenceState { get; }
        public string TargetObject { get; }
        public string FieldOrComponent { get; }
        public PyralisAuthoringNativeAction? NativeAction { get; }
        public string Reason { get; }
    }

    public sealed class PyralisAuthoringContract
    {
        public PyralisAuthoringContract(
            string stableId,
            string moduleId,
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
            PyralisAuthoringConfidence confidence = PyralisAuthoringConfidence.Unknown,
            string[] assignmentFields = null,
            string[] customizationMoments = null)
        {
            StableId = stableId ?? string.Empty;
            ModuleId = moduleId ?? string.Empty;
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
            Confidence = confidence;
            AssignmentFields = assignmentFields ?? Array.Empty<string>();
            CustomizationMoments = customizationMoments ?? Array.Empty<string>();
        }

        public string StableId { get; }
        public string ModuleId { get; }
        public string DisplayName { get; }
        public string AuthoringCategory { get; }
        public Type RequiredProfileType { get; }
        public string[] RequiredRuntimeInterfaceNames { get; }
        public ActorPresentationMode[] SupportedPresentationModes { get; }
        public ActorPresentationMode[] UnsupportedPresentationModes { get; }
        public string UnsupportedLaneMessage { get; }
        public string[] ConsumedActionRoles { get; }
        public string[] NativeSetup { get; }
        public string FirstProofTargetId { get; }
        public PyralisAuthoringConfidence Confidence { get; }
        public string[] AssignmentFields { get; }
        public string[] CustomizationMoments { get; }

        public bool MatchesModuleId(string moduleId)
        {
            return string.Equals(ModuleId, moduleId, StringComparison.Ordinal);
        }

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

    public interface IAuthoringContractProvider
    {
        IReadOnlyList<PyralisAuthoringContract> GetAuthoringContracts();
    }

    public static class PyralisAuthoringContractRegistry
    {
        private static readonly Lazy<IReadOnlyList<PyralisAuthoringContract>> _allContracts =
            new Lazy<IReadOnlyList<PyralisAuthoringContract>>(BuildContracts);

        public static IReadOnlyList<PyralisAuthoringContract> All => _allContracts.Value;

        public static PyralisAuthoringContract FindByModuleId(string moduleId)
        {
            if (string.IsNullOrWhiteSpace(moduleId))
                return null;

            for (int i = 0; i < All.Count; i++)
            {
                if (All[i].MatchesModuleId(moduleId))
                    return All[i];
            }

            return null;
        }

        public static bool HasDuplicateStableIds(out string duplicateStableId)
        {
            duplicateStableId = null;
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            for (int i = 0; i < All.Count; i++)
            {
                string stableId = All[i].StableId;
                if (string.IsNullOrWhiteSpace(stableId))
                    continue;

                if (!seen.Add(stableId))
                {
                    duplicateStableId = stableId;
                    return true;
                }
            }

            return false;
        }

        private static IReadOnlyList<PyralisAuthoringContract> BuildContracts()
        {
            List<PyralisAuthoringContract> contracts = new List<PyralisAuthoringContract>();
            Type providerType = typeof(IAuthoringContractProvider);
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int assemblyIndex = 0; assemblyIndex < assemblies.Length; assemblyIndex++)
            {
                Assembly assembly = assemblies[assemblyIndex];
                if (!ShouldScanAssembly(assembly))
                    continue;

                Type[] assemblyTypes = GetLoadableTypes(assembly);
                for (int i = 0; i < assemblyTypes.Length; i++)
                {
                    Type candidateType = assemblyTypes[i];
                    if (candidateType == null)
                        continue;

                    if (candidateType == providerType)
                        continue;

                    if (candidateType.IsAbstract || candidateType.IsInterface)
                        continue;

                    if (!providerType.IsAssignableFrom(candidateType))
                        continue;

                    if (candidateType.GetConstructor(Type.EmptyTypes) == null)
                        continue;

                    IAuthoringContractProvider provider;
                    try
                    {
                        provider = Activator.CreateInstance(candidateType) as IAuthoringContractProvider;
                    }
                    catch (Exception exception)
                    {
                        Debug.LogWarning($"Pyralis authoring contract provider '{candidateType.FullName}' could not be created: {exception.Message}");
                        continue;
                    }

                    if (provider == null)
                        continue;

                    IReadOnlyList<PyralisAuthoringContract> provided;
                    try
                    {
                        provided = provider.GetAuthoringContracts();
                    }
                    catch (Exception exception)
                    {
                        Debug.LogWarning($"Pyralis authoring contract provider '{candidateType.FullName}' failed while returning contracts: {exception.Message}");
                        continue;
                    }

                    if (provided == null)
                        continue;

                    for (int j = 0; j < provided.Count; j++)
                    {
                        if (provided[j] != null)
                            contracts.Add(provided[j]);
                    }
                }
            }

            return contracts;
        }

        private static bool ShouldScanAssembly(Assembly assembly)
        {
            if (assembly == null || assembly.IsDynamic)
                return false;

            string assemblyName = assembly.GetName().Name;
            return !string.IsNullOrWhiteSpace(assemblyName)
                && assemblyName.StartsWith("NeonBlack.Gameplay", StringComparison.Ordinal);
        }

        private static Type[] GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                Debug.LogWarning($"Pyralis authoring contract registry loaded partial editor assembly types: {exception.Message}");
                return exception.Types ?? Array.Empty<Type>();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Pyralis authoring contract registry could not inspect assembly '{assembly.GetName().Name}': {exception.Message}");
                return Array.Empty<Type>();
            }
        }
    }
}
