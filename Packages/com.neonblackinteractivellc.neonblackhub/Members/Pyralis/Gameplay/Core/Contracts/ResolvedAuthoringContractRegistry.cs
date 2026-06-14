using System;
using System.Collections.Generic;
using System.Reflection;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NeonBlack.Gameplay.Core.Contracts
{
    /// <summary>
    /// Registry for all reflective authoring contracts discovered in the project.
    /// Works in both Editor (using TypeCache) and Runtime (using standard reflection).
    /// </summary>
    public static class ResolvedAuthoringContractRegistry
    {
        private static readonly Lazy<IReadOnlyList<ResolvedAuthoringContract>> _allContracts =
            new Lazy<IReadOnlyList<ResolvedAuthoringContract>>(BuildContracts);

        public static IReadOnlyList<ResolvedAuthoringContract> All => _allContracts.Value;

        public static ResolvedAuthoringContract FindByType(Type type)
        {
            if (type == null)
                return null;

            for (int i = 0; i < All.Count; i++)
            {
                if (All[i].SourceType == type || All[i].StableId.EndsWith(type.FullName))
                    return All[i];
            }

            return null;
        }

        public static ResolvedAuthoringContract FindByModuleId(string moduleId)
        {
            if (string.IsNullOrWhiteSpace(moduleId))
                return null;

            string targetStableId = moduleId.StartsWith("feature.") ? moduleId : $"feature.{moduleId}";

            for (int i = 0; i < All.Count; i++)
            {
                if (string.Equals(All[i].StableId, targetStableId, StringComparison.Ordinal) ||
                    string.Equals(All[i].ModuleId, moduleId, StringComparison.Ordinal))
                {
                    return All[i];
                }
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

        private static IReadOnlyList<ResolvedAuthoringContract> BuildContracts()
        {
            List<ResolvedAuthoringContract> contracts = new List<ResolvedAuthoringContract>();
            Dictionary<string, int> indexByStableId = new Dictionary<string, int>(StringComparer.Ordinal);

            void AddContract(ResolvedAuthoringContract contract)
            {
                if (contract == null)
                    return;

                if (indexByStableId.TryGetValue(contract.StableId, out int existingIndex))
                {
                    contracts[existingIndex] = MergeContracts(contracts[existingIndex], contract);
                    return;
                }

                indexByStableId.Add(contract.StableId, contracts.Count);
                contracts.Add(contract);
            }

#if UNITY_EDITOR
            // Fast discovery using TypeCache in the Editor
            var typesWithAttribute = TypeCache.GetTypesWithAttribute<AuthoringContractAttribute>();
            foreach (var type in typesWithAttribute)
            {
                var attributes = type.GetCustomAttributes<AuthoringContractAttribute>();
                foreach (var attr in attributes)
                {
                    AddContract(CreateFromAttribute(type, attr));
                }
            }

#else
            // Standard reflection for runtime environments
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // Skip system assemblies to save time
                if (assembly.FullName.StartsWith("System") || assembly.FullName.StartsWith("Unity") || assembly.FullName.StartsWith("mscorlib"))
                    continue;

                foreach (var type in assembly.GetTypes())
                {
                    var attributes = type.GetCustomAttributes<AuthoringContractAttribute>();
                    foreach (var attr in attributes)
                    {
                        AddContract(CreateFromAttribute(type, attr));
                    }
                }
            }
#endif

            ResolveDependencyProofTargets(contracts);
            return contracts;
        }

        private static void ResolveDependencyProofTargets(List<ResolvedAuthoringContract> contracts)
        {
            if (contracts == null || contracts.Count == 0)
                return;

            for (int i = 0; i < contracts.Count; i++)
            {
                ResolvedAuthoringContract contract = contracts[i];
                if (contract == null || !string.IsNullOrWhiteSpace(contract.FirstProofTargetId))
                    continue;

                string inferredProofTargetId = InferDependencyProofTargetId(contract, contracts);
                if (string.IsNullOrWhiteSpace(inferredProofTargetId))
                    continue;

                contracts[i] = WithFirstProofTargetId(contract, inferredProofTargetId);
            }
        }

        private static string InferDependencyProofTargetId(
            ResolvedAuthoringContract contract,
            IReadOnlyList<ResolvedAuthoringContract> contracts)
        {
            HashSet<string> proofTargets = new HashSet<string>(StringComparer.Ordinal);

            for (int i = 0; i < contracts.Count; i++)
            {
                ResolvedAuthoringContract candidate = contracts[i];
                if (candidate == null
                    || candidate == contract
                    || string.IsNullOrWhiteSpace(candidate.FirstProofTargetId))
                {
                    continue;
                }

                if (ContractsAreDependencyConnected(contract, candidate))
                    proofTargets.Add(candidate.FirstProofTargetId);
            }

            if (proofTargets.Count != 1)
                return string.Empty;

            foreach (string proofTarget in proofTargets)
                return proofTarget;

            return string.Empty;
        }

        private static bool ContractsAreDependencyConnected(
            ResolvedAuthoringContract first,
            ResolvedAuthoringContract second)
        {
            return ContractReferencesType(first, second.SourceType)
                || ContractReferencesType(second, first.SourceType)
                || TypeConnectsToContract(first.SourceType, second)
                || TypeConnectsToContract(second.SourceType, first);
        }

        private static bool ContractReferencesType(ResolvedAuthoringContract contract, Type type)
        {
            if (contract == null || type == null)
                return false;

            if (contract.RequiredProfileType == type)
                return true;

            string fullName = type.FullName;
            if (string.IsNullOrWhiteSpace(fullName))
                return false;

            return ContainsTypeName(contract.RequiredRuntimeInterfaceNames, fullName);
        }

        private static bool ContainsTypeName(string[] typeNames, string fullName)
        {
            if (typeNames == null || string.IsNullOrWhiteSpace(fullName))
                return false;

            for (int i = 0; i < typeNames.Length; i++)
            {
                if (string.Equals(typeNames[i], fullName, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static bool TypeConnectsToContract(Type type, ResolvedAuthoringContract contract)
        {
            if (type == null || contract == null || contract.SourceType == null)
                return false;

            if (contract.SourceType.IsAssignableFrom(type) || type.IsAssignableFrom(contract.SourceType))
                return true;

            return ContractReferencesType(contract, type);
        }

        private static ResolvedAuthoringContract WithFirstProofTargetId(
            ResolvedAuthoringContract contract,
            string firstProofTargetId)
        {
            return new ResolvedAuthoringContract(
                stableId: contract.StableId,
                displayName: contract.DisplayName,
                authoringCategory: contract.AuthoringCategory,
                requiredProfileType: contract.RequiredProfileType,
                requiredRuntimeInterfaceNames: contract.RequiredRuntimeInterfaceNames,
                supportedPresentationModes: contract.SupportedPresentationModes,
                unsupportedPresentationModes: contract.UnsupportedPresentationModes,
                unsupportedLaneMessage: contract.UnsupportedLaneMessage,
                consumedActionRoles: contract.ConsumedActionRoles,
                nativeSetup: contract.NativeSetup,
                firstProofTargetId: firstProofTargetId,
                firstProofGuidance: contract.FirstProofGuidance,
                sourceType: contract.SourceType,
                axioms: contract.Axioms,
                workIntent: contract.WorkIntent,
                confidence: contract.Confidence,
                assignmentFields: contract.AssignmentFields,
                customizationMoments: contract.CustomizationMoments,
                requiredComponentNames: contract.RequiredComponentNames,
                capability: contract.Capability,
                priority: (AuthoringPriority)contract.Priority,
                priorityValueOverride: contract.PriorityValueOverride,
                deprecatedInVersion: contract.DeprecatedInVersion,
                removableInVersion: contract.RemovableInVersion,
                documentationURL: contract.DocumentationURL,
                expertAdvice: contract.ExpertAdvice,
                moduleId: contract.ModuleId,
                setupNodeId: contract.SetupNodeId,
                authoringLane: contract.AuthoringLane,
                relevance: contract.Relevance,
                manualPath: contract.ManualPath);
        }

        private static ResolvedAuthoringContract CreateFromAttribute(Type type, AuthoringContractAttribute attr)
        {
            List<string> interfaceNames = new List<string>();
            if (attr.RequiredInterfaces != null)
            {
                foreach (var iface in attr.RequiredInterfaces)
                {
                    if (iface != null)
                        interfaceNames.Add(iface.FullName);
                }
            }

            if (attr.RequiredInterfaceNames != null)
            {
                foreach (var ifaceName in attr.RequiredInterfaceNames)
                {
                    if (!string.IsNullOrWhiteSpace(ifaceName) && !interfaceNames.Contains(ifaceName))
                        interfaceNames.Add(ifaceName);
                }
            }

            AddImplementedInterfaceNames(type, interfaceNames);

            List<string> componentNames = new List<string>();
            if (attr.RequiredComponents != null)
            {
                foreach (var comp in attr.RequiredComponents)
                {
                    if (comp != null)
                        componentNames.Add(comp.FullName);
                }
            }

            if (attr.RequiredComponentNames != null)
            {
                foreach (var compName in attr.RequiredComponentNames)
                {
                    if (!string.IsNullOrWhiteSpace(compName) && !componentNames.Contains(compName))
                        componentNames.Add(compName);
                }
            }

            AddRequireComponentNames(type, componentNames);

#pragma warning disable CS0618 // Type or member is obsolete
            string categoryLabel = attr.Capability != AuthoringCapability.None
                ? GetCapabilityDisplayNames(attr.Capability)
                : string.Empty;
#pragma warning restore CS0618

            string stableId = !string.IsNullOrWhiteSpace(attr.ModuleId)
                ? $"feature.{attr.ModuleId}"
                : $"feature.{type.FullName}";
            string[] nativeSetup = NormalizeNativeSetup(type, attr);
            string[] assignmentFields = NormalizeAssignmentFields(type, attr);
            string[] customizationMoments = NormalizeCustomizationMoments(type, attr, interfaceNames, componentNames);

            return new ResolvedAuthoringContract(
                stableId: stableId,
                displayName: AuthoringCapabilityRegistry.PrettifyTypeName(type.Name),
                authoringCategory: categoryLabel,
                requiredProfileType: attr.ProfileType,
                requiredRuntimeInterfaceNames: interfaceNames.ToArray(),
                supportedPresentationModes: attr.SupportedLanes,
                unsupportedPresentationModes: attr.UnsupportedLanes,
                unsupportedLaneMessage: attr.UnsupportedLaneMessage,
                consumedActionRoles: attr.ConsumedRoles,
                nativeSetup: nativeSetup,
                firstProofTargetId: NormalizeFirstProofTargetId(attr),
                firstProofGuidance: attr.FirstProof,
                sourceType: type,
                axioms: attr.Axioms,
                workIntent: attr.AxiomKeywords,
                confidence: PyralisAuthoringConfidence.Explicit,
                assignmentFields: assignmentFields,
                customizationMoments: customizationMoments,
                requiredComponentNames: componentNames.ToArray(),
                capability: attr.Capability,
                priority: attr.Priority,
                priorityValueOverride: attr.PriorityValueOverride > 0 ? attr.PriorityValueOverride : 0,
                deprecatedInVersion: attr.DeprecatedInVersion,
                removableInVersion: attr.RemovableInVersion,
                documentationURL: attr.DocumentationURL,
                expertAdvice: attr.ExpertAdvice,
                moduleId: attr.ModuleId,
                setupNodeId: attr.SetupNodeId,
                authoringLane: attr.Lane,
                relevance: attr.Relevance,
                manualPath: attr.ManualPath
            );
        }

        private static string[] NormalizeNativeSetup(Type type, AuthoringContractAttribute attr)
        {
            if (attr.NativeSetup != null && attr.NativeSetup.Length > 0)
                return attr.NativeSetup;

            string displayName = AuthoringCapabilityRegistry.PrettifyTypeName(type.Name);
            if (typeof(MonoBehaviour).IsAssignableFrom(type))
                return new[] { $"Add or assign {displayName} on the relevant scene or prefab object." };

            if (typeof(ScriptableObject).IsAssignableFrom(type))
                return new[] { $"Create or assign a {displayName} asset in the project-owned setup folder." };

            if (type.IsInterface)
                return new[] { $"Provide a concrete {displayName} implementation through the active setup or feature module." };

            return new[] { $"Reference {displayName} from the setup object, definition, or feature that owns this route." };
        }

        private static string[] NormalizeAssignmentFields(Type type, AuthoringContractAttribute attr)
        {
            if (attr.AssignmentFields != null && attr.AssignmentFields.Length > 0)
                return attr.AssignmentFields;

            if (attr.CustomizationMoments != null && attr.CustomizationMoments.Length > 0)
                return Array.Empty<string>();

            string displayName = AuthoringCapabilityRegistry.PrettifyTypeName(type.Name);
            if (typeof(MonoBehaviour).IsAssignableFrom(type) || typeof(ScriptableObject).IsAssignableFrom(type))
                return new[] { $"{displayName} Inspector fields" };

            return Array.Empty<string>();
        }

        private static string[] NormalizeCustomizationMoments(
            Type type,
            AuthoringContractAttribute attr,
            List<string> interfaceNames,
            List<string> componentNames)
        {
            if (attr.CustomizationMoments != null && attr.CustomizationMoments.Length > 0)
                return attr.CustomizationMoments;

            if ((attr.AssignmentFields != null && attr.AssignmentFields.Length > 0) ||
                interfaceNames.Count > 0 ||
                componentNames.Count > 0)
            {
                return Array.Empty<string>();
            }

            string displayName = AuthoringCapabilityRegistry.PrettifyTypeName(type.Name);
            if (type.IsInterface)
                return new[] { $"Choose the {displayName} implementation that matches the route." };

            return new[] { $"Customize {displayName} only after the route's required setup is visible." };
        }

        private static string NormalizeFirstProofTargetId(AuthoringContractAttribute attr)
        {
            return !string.IsNullOrWhiteSpace(attr.FirstProofTargetId) ? attr.FirstProofTargetId : string.Empty;
        }

        private static void AddImplementedInterfaceNames(Type type, List<string> interfaceNames)
        {
            if (type == null || interfaceNames == null || type.IsInterface)
                return;

            Type[] interfaces = type.GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
            {
                Type iface = interfaces[i];
                if (iface == null || string.IsNullOrWhiteSpace(iface.FullName))
                    continue;

                AddDistinct(interfaceNames, iface.FullName);
            }
        }

        private static void AddRequireComponentNames(Type type, List<string> componentNames)
        {
            if (type == null || componentNames == null || !typeof(MonoBehaviour).IsAssignableFrom(type))
                return;

            object[] attributes = type.GetCustomAttributes(typeof(RequireComponent), true);
            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i] is not RequireComponent attribute)
                    continue;

                AddRequireComponentName(attribute, "m_Type0", componentNames);
                AddRequireComponentName(attribute, "m_Type1", componentNames);
                AddRequireComponentName(attribute, "m_Type2", componentNames);
            }
        }

        private static void AddRequireComponentName(RequireComponent attribute, string fieldName, List<string> componentNames)
        {
            if (attribute == null || componentNames == null)
                return;

            FieldInfo field = typeof(RequireComponent).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null || field.GetValue(attribute) is not Type componentType || string.IsNullOrWhiteSpace(componentType.FullName))
                return;

            AddDistinct(componentNames, componentType.FullName);
        }

        private static ResolvedAuthoringContract MergeContracts(ResolvedAuthoringContract current, ResolvedAuthoringContract incoming)
        {
            return new ResolvedAuthoringContract(
                stableId: current.StableId,
                displayName: FirstNonEmpty(current.DisplayName, incoming.DisplayName),
                authoringCategory: FirstNonEmpty(current.AuthoringCategory, incoming.AuthoringCategory),
                requiredProfileType: current.RequiredProfileType ?? incoming.RequiredProfileType,
                requiredRuntimeInterfaceNames: MergeDistinct(current.RequiredRuntimeInterfaceNames, incoming.RequiredRuntimeInterfaceNames),
                supportedPresentationModes: MergeDistinct(current.SupportedPresentationModes, incoming.SupportedPresentationModes),
                unsupportedPresentationModes: MergeDistinct(current.UnsupportedPresentationModes, incoming.UnsupportedPresentationModes),
                unsupportedLaneMessage: FirstNonEmpty(current.UnsupportedLaneMessage, incoming.UnsupportedLaneMessage),
                consumedActionRoles: MergeDistinct(current.ConsumedActionRoles, incoming.ConsumedActionRoles),
                nativeSetup: MergeDistinct(current.NativeSetup, incoming.NativeSetup),
                firstProofTargetId: FirstNonEmpty(current.FirstProofTargetId, incoming.FirstProofTargetId),
                firstProofGuidance: FirstNonEmpty(current.FirstProofGuidance, incoming.FirstProofGuidance),
                sourceType: current.SourceType ?? incoming.SourceType,
                axioms: current.Axioms | incoming.Axioms,
                workIntent: FirstNonEmpty(current.WorkIntent, incoming.WorkIntent),
                confidence: current.Confidence >= incoming.Confidence ? current.Confidence : incoming.Confidence,
                assignmentFields: MergeDistinct(current.AssignmentFields, incoming.AssignmentFields),
                customizationMoments: MergeDistinct(current.CustomizationMoments, incoming.CustomizationMoments),
                requiredComponentNames: MergeDistinct(current.RequiredComponentNames, incoming.RequiredComponentNames),
                capability: current.Capability | incoming.Capability,
                priority: current.Priority != (int)AuthoringPriority.Unspecified ? (AuthoringPriority)current.Priority : (AuthoringPriority)incoming.Priority,
                priorityValueOverride: current.PriorityValueOverride != 0 ? current.PriorityValueOverride : incoming.PriorityValueOverride,
                deprecatedInVersion: FirstNonEmpty(current.DeprecatedInVersion, incoming.DeprecatedInVersion),
                removableInVersion: FirstNonEmpty(current.RemovableInVersion, incoming.RemovableInVersion),
                documentationURL: FirstNonEmpty(current.DocumentationURL, incoming.DocumentationURL),
                expertAdvice: FirstNonEmpty(current.ExpertAdvice, incoming.ExpertAdvice),
                moduleId: FirstNonEmpty(current.ModuleId, incoming.ModuleId),
                setupNodeId: FirstNonEmpty(current.SetupNodeId, incoming.SetupNodeId),
                authoringLane: FirstNonEmpty(current.AuthoringLane, incoming.AuthoringLane),
                relevance: FirstNonEmpty(current.Relevance, incoming.Relevance),
                manualPath: FirstNonEmpty(current.ManualPath, incoming.ManualPath));
        }

        private static string GetCapabilityDisplayNames(AuthoringCapability capability)
        {
            if (capability == AuthoringCapability.None)
                return string.Empty;

            List<string> names = new List<string>();
            foreach (AuthoringCapability individual in AuthoringCapabilityRegistry.GetAllIndividualCapabilities())
            {
                if ((capability & individual) != 0)
                    names.Add(AuthoringCapabilityRegistry.GetDisplayName(individual));
            }

            return names.Count > 0 ? string.Join(", ", names) : AuthoringCapabilityRegistry.GetDisplayName(capability);
        }

        private static string FirstNonEmpty(string first, string second)
        {
            return string.IsNullOrWhiteSpace(first) ? second : first;
        }

        private static string[] MergeDistinct(string[] first, string[] second)
        {
            HashSet<string> values = new HashSet<string>(StringComparer.Ordinal);
            List<string> merged = new List<string>();
            AddRange(first);
            AddRange(second);
            return merged.ToArray();

            void AddRange(string[] source)
            {
                if (source == null)
                    return;

                for (int i = 0; i < source.Length; i++)
                {
                    string value = source[i];
                    if (!string.IsNullOrWhiteSpace(value) && values.Add(value))
                        merged.Add(value);
                }
            }
        }

        private static void AddDistinct(List<string> values, string value)
        {
            if (values == null || string.IsNullOrWhiteSpace(value) || values.Contains(value))
                return;

            values.Add(value);
        }

        private static ActorPresentationMode[] MergeDistinct(ActorPresentationMode[] first, ActorPresentationMode[] second)
        {
            HashSet<ActorPresentationMode> values = new HashSet<ActorPresentationMode>();
            List<ActorPresentationMode> merged = new List<ActorPresentationMode>();
            AddRange(first);
            AddRange(second);
            return merged.ToArray();

            void AddRange(ActorPresentationMode[] source)
            {
                if (source == null)
                    return;

                for (int i = 0; i < source.Length; i++)
                {
                    if (values.Add(source[i]))
                        merged.Add(source[i]);
                }
            }
        }
    }
}
