using System;
using System.Collections.Generic;
using System.Reflection;
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
    public static class PyralisAuthoringContractRegistry
    {
        private static readonly Lazy<IReadOnlyList<PyralisAuthoringContract>> _allContracts =
            new Lazy<IReadOnlyList<PyralisAuthoringContract>>(BuildContracts);

        public static IReadOnlyList<PyralisAuthoringContract> All => _allContracts.Value;

        public static PyralisAuthoringContract FindByModuleId(string moduleId)
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

        private static IReadOnlyList<PyralisAuthoringContract> BuildContracts()
        {
            List<PyralisAuthoringContract> contracts = new List<PyralisAuthoringContract>();

#if UNITY_EDITOR
            // Fast discovery using TypeCache in the Editor
            var typesWithAttribute = TypeCache.GetTypesWithAttribute<AuthoringContractAttribute>();
            foreach (var type in typesWithAttribute)
            {
                var attributes = type.GetCustomAttributes<AuthoringContractAttribute>();
                foreach (var attr in attributes)
                {
                    contracts.Add(CreateFromAttribute(type, attr));
                }
            }

            var providerTypes = TypeCache.GetTypesDerivedFrom<IAuthoringContractProvider>();
            foreach (var type in providerTypes)
            {
                if (type.IsAbstract || type.IsInterface)
                    continue;

                ProcessProvider(type, contracts);
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
                        contracts.Add(CreateFromAttribute(type, attr));
                    }

                    if (typeof(IAuthoringContractProvider).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                    {
                        ProcessProvider(type, contracts);
                    }
                }
            }
#endif

            return contracts;
        }

        private static void ProcessProvider(Type type, List<PyralisAuthoringContract> contracts)
        {
            try
            {
                var provider = Activator.CreateInstance(type) as IAuthoringContractProvider;
                if (provider == null) return;

                foreach (var attr in provider.GetAuthoringContracts())
                {
                    if (attr == null)
                        continue;

                    contracts.Add(CreateFromAttribute(type, attr));
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to create authoring contract provider instance for {type.Name}: {ex.Message}");
            }
        }

        private static PyralisAuthoringContract CreateFromAttribute(Type type, AuthoringContractAttribute attr)
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

            if (attr.ProfileType != null)
            {
                const string runtimeInterface = "NeonBlack.Gameplay.Features.Composition.IFeatureModuleRuntime";
                if (!interfaceNames.Contains(runtimeInterface))
                    interfaceNames.Add(runtimeInterface);
            }

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

            #pragma warning disable CS0618 // Type or member is obsolete
                        string categoryLabel = attr.Capability != AuthoringCapability.None 
                            ? AuthoringCapabilityRegistry.GetDisplayName(attr.Capability) 
                            : string.Empty;
#pragma warning restore CS0618

                        return new PyralisAuthoringContract(
                stableId: $"feature.{type.FullName}",
                displayName: AuthoringCapabilityRegistry.PrettifyTypeName(type.Name),
                authoringCategory: categoryLabel,
                requiredProfileType: attr.ProfileType,
                requiredRuntimeInterfaceNames: interfaceNames.ToArray(),
                supportedPresentationModes: attr.SupportedLanes,
                unsupportedPresentationModes: attr.UnsupportedLanes,
                unsupportedLaneMessage: attr.UnsupportedLaneMessage,
                consumedActionRoles: attr.ConsumedRoles,
                nativeSetup: attr.NativeSetup,
                firstProofTargetId: attr.FirstProof,
                confidence: PyralisAuthoringConfidence.Explicit,
                assignmentFields: attr.AssignmentFields,
                customizationMoments: attr.CustomizationMoments,
                requiredComponentNames: componentNames.ToArray(),
                capability: attr.Capability,
                priority: attr.Priority,
                moduleId: attr.ModuleId
            );
        }
    }
}
