using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    public interface IAuthoringConventionFactProvider
    {
        IReadOnlyList<PyralisAuthoringFact> GetAuthoringFacts();
    }

    public sealed class PyralisConventionAuthoringFactBridgeProvider : IAuthoringConventionFactProvider
    {
        public IReadOnlyList<PyralisAuthoringFact> GetAuthoringFacts()
        {
            return PyralisConventionAuthoringFacts.GetAuthoringFacts();
        }
    }

    public static class PyralisAuthoringConventionFactRegistry
    {
        private sealed class RegistryResult
        {
            public RegistryResult(IReadOnlyList<PyralisAuthoringFact> facts, IReadOnlyList<string> providerTypeNames)
            {
                Facts = facts;
                ProviderTypeNames = providerTypeNames;
            }

            public IReadOnlyList<PyralisAuthoringFact> Facts { get; }
            public IReadOnlyList<string> ProviderTypeNames { get; }
        }

        private static readonly Lazy<RegistryResult> _result =
            new Lazy<RegistryResult>(BuildFacts);

        public static IReadOnlyList<PyralisAuthoringFact> AllFacts => _result.Value.Facts;

        public static IReadOnlyList<string> ProviderTypeNames => _result.Value.ProviderTypeNames;

        public static bool HasDuplicateStableIds(out string duplicateStableId)
        {
            duplicateStableId = null;
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            IReadOnlyList<PyralisAuthoringFact> facts = AllFacts;
            for (int i = 0; i < facts.Count; i++)
            {
                string stableId = facts[i].StableId;
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

        private static RegistryResult BuildFacts()
        {
            List<PyralisAuthoringFact> facts = new List<PyralisAuthoringFact>();
            List<string> providerTypeNames = new List<string>();
            List<Type> providerTypes = FindProviderTypes();

            for (int i = 0; i < providerTypes.Count; i++)
            {
                Type providerType = providerTypes[i];
                IAuthoringConventionFactProvider provider;
                try
                {
                    provider = Activator.CreateInstance(providerType) as IAuthoringConventionFactProvider;
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"Pyralis authoring convention fact provider '{providerType.FullName}' could not be created: {exception.Message}");
                    continue;
                }

                if (provider == null)
                    continue;

                IReadOnlyList<PyralisAuthoringFact> provided;
                try
                {
                    provided = provider.GetAuthoringFacts();
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"Pyralis authoring convention fact provider '{providerType.FullName}' failed while returning facts: {exception.Message}");
                    continue;
                }

                if (provided == null)
                    continue;

                providerTypeNames.Add(providerType.FullName);
                for (int factIndex = 0; factIndex < provided.Count; factIndex++)
                {
                    if (provided[factIndex] != null)
                        facts.Add(provided[factIndex]);
                }
            }

            return new RegistryResult(facts, providerTypeNames);
        }

        private static List<Type> FindProviderTypes()
        {
            List<Type> providerTypes = new List<Type>();
            Type providerInterface = typeof(IAuthoringConventionFactProvider);
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int assemblyIndex = 0; assemblyIndex < assemblies.Length; assemblyIndex++)
            {
                Assembly assembly = assemblies[assemblyIndex];
                if (!ShouldScanAssembly(assembly))
                    continue;

                Type[] assemblyTypes = GetLoadableTypes(assembly);
                for (int typeIndex = 0; typeIndex < assemblyTypes.Length; typeIndex++)
                {
                    Type candidateType = assemblyTypes[typeIndex];
                    if (candidateType == null)
                        continue;

                    if (candidateType == providerInterface)
                        continue;

                    if (candidateType.IsAbstract || candidateType.IsInterface)
                        continue;

                    if (!providerInterface.IsAssignableFrom(candidateType))
                        continue;

                    if (candidateType.GetConstructor(Type.EmptyTypes) == null)
                        continue;

                    providerTypes.Add(candidateType);
                }
            }

            providerTypes.Sort((left, right) => string.Compare(left.FullName, right.FullName, StringComparison.Ordinal));
            return providerTypes;
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
                Debug.LogWarning($"Pyralis authoring convention fact registry loaded partial editor assembly types: {exception.Message}");
                return exception.Types ?? Array.Empty<Type>();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Pyralis authoring convention fact registry could not inspect assembly '{assembly.GetName().Name}': {exception.Message}");
                return Array.Empty<Type>();
            }
        }
    }
}
