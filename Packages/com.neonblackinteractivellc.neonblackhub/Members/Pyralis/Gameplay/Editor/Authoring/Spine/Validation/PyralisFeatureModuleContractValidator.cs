using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    /// <summary>
    /// Validates FeatureModuleDefinition assets against their resolved authoring contract.
    /// </summary>
    public static class PyralisFeatureModuleContractValidator
    {
        public static List<string> GetValidationIssues(FeatureModuleDefinition definition)
        {
            List<string> issues = new List<string>();
            if (definition == null) return issues;

            ResolvedAuthoringContract contract = ResolvedAuthoringContractRegistry.FindByModuleId(definition.moduleId);
            if (contract == null)
                return issues;

            ValidateProfile(definition, contract, issues);
            ValidateUnsupportedLanes(definition, contract, issues);
            ValidateRuntimeInterfaces(definition, contract, issues);

            return issues;
        }

        private static void ValidateProfile(FeatureModuleDefinition definition, ResolvedAuthoringContract contract, List<string> issues)
        {
            System.Type requiredProfileType = contract.RequiredProfileType;
            if (requiredProfileType == null)
                return;

            if (definition.profileAsset == null)
            {
                issues.Add($"Feature module `{definition.moduleId}` requires a `{requiredProfileType.Name}` profile asset.");
            }
            else if (!requiredProfileType.IsInstanceOfType(definition.profileAsset))
            {
                issues.Add($"Feature module `{definition.moduleId}` profile asset is not `{requiredProfileType.Name}`.");
            }
        }

        private static void ValidateUnsupportedLanes(FeatureModuleDefinition definition, ResolvedAuthoringContract contract, List<string> issues)
        {
            if (definition.supportedPresentationModes == null ||
                contract.UnsupportedPresentationModes == null ||
                contract.UnsupportedPresentationModes.Length == 0)
            {
                return;
            }

            for (int i = 0; i < definition.supportedPresentationModes.Length; i++)
            {
                if (!contract.IsExplicitlyUnsupported(definition.supportedPresentationModes[i]))
                    continue;

                string message = !string.IsNullOrWhiteSpace(contract.UnsupportedLaneMessage)
                    ? contract.UnsupportedLaneMessage
                    : $"{definition.supportedPresentationModes[i]} is not supported by `{definition.moduleId}`.";
                issues.Add(message);
            }
        }

        private static void ValidateRuntimeInterfaces(FeatureModuleDefinition definition, ResolvedAuthoringContract contract, List<string> issues)
        {
            if (definition.runtimePrefab == null || contract.RequiredRuntimeInterfaceNames == null)
                return;

            for (int i = 0; i < contract.RequiredRuntimeInterfaceNames.Length; i++)
            {
                string interfaceName = contract.RequiredRuntimeInterfaceNames[i];
                if (string.IsNullOrWhiteSpace(interfaceName))
                    continue;

                if (!RuntimePrefabHasInterface(definition.runtimePrefab, interfaceName))
                    issues.Add($"Feature module `{definition.moduleId}` runtime prefab should expose `{GetShortTypeName(interfaceName)}`.");
            }
        }

        private static bool RuntimePrefabHasInterface(GameObject runtimePrefab, string interfaceName)
        {
            MonoBehaviour[] behaviours = runtimePrefab.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                    continue;

                System.Type[] interfaces = behaviour.GetType().GetInterfaces();
                for (int j = 0; j < interfaces.Length; j++)
                {
                    if (interfaces[j].FullName == interfaceName || interfaces[j].Name == interfaceName)
                        return true;
                }
            }

            return false;
        }

        private static string GetShortTypeName(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return string.Empty;

            int lastDot = typeName.LastIndexOf('.');
            return lastDot >= 0 && lastDot < typeName.Length - 1
                ? typeName.Substring(lastDot + 1)
                : typeName;
        }
    }
}
