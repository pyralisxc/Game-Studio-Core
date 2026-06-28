using System;
using System.Collections.Generic;
using System.Reflection;
using Pys.Authoring.Contracts;
using Pys.Authoring.Editor.Contracts;
using UnityEditor;
using UnityEngine;

namespace Pys.Authoring.Editor.Scanning
{
    internal static class UnityTypeScanner
    {
        public static IReadOnlyList<UnityTypeObservation> Scan(UnityCodebaseScanRequest request)
        {
            List<UnityTypeObservation> observations = new List<UnityTypeObservation>();
            HashSet<Type> seen = new HashSet<Type>();

            string[] scriptGuids = AssetDatabase.FindAssets("t:MonoScript", new[] { UnityScanPathUtility.NormalizeRoot(request) });
            foreach (string guid in scriptGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                Type type = script != null ? script.GetClass() : null;
                if (type == null || type.IsAbstract || !seen.Add(type))
                    continue;

                UnityTypeObservation observation = new UnityTypeObservation(type, path);
                AddInterfaces(type, observation);
                AddSerializedFields(type, observation);
                AddRequiredComponents(type, observation);
                AddContracts(type, observation);
                IReadOnlyList<MethodInfo> validationMethods = ReflectiveRuntimeValidationObserver.FindValidationMethods(type, request.RuntimeValidationMethodNames);
                observation.HasRuntimeValidationMethod = validationMethods.Count > 0;
                for (int methodIndex = 0; methodIndex < validationMethods.Count; methodIndex++)
                    observation.RuntimeValidationMethods.Add(validationMethods[methodIndex].Name);
                observations.Add(observation);
            }

            return observations;
        }

        private static void AddInterfaces(Type type, UnityTypeObservation observation)
        {
            foreach (Type interfaceType in type.GetInterfaces())
                observation.ImplementedInterfaces.Add(interfaceType.FullName);
        }

        private static void AddSerializedFields(Type type, UnityTypeObservation observation)
        {
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (FieldInfo field in type.GetFields(Flags))
            {
                bool unitySerialized = field.IsPublic || field.GetCustomAttribute<SerializeField>() != null;
                if (!unitySerialized || field.IsStatic)
                    continue;

                observation.SerializedFields.Add(field.Name);
            }
        }

        private static void AddRequiredComponents(Type type, UnityTypeObservation observation)
        {
            foreach (RequireComponent attribute in type.GetCustomAttributes<RequireComponent>(true))
            {
                AddRequiredComponent(attribute.m_Type0, observation);
                AddRequiredComponent(attribute.m_Type1, observation);
                AddRequiredComponent(attribute.m_Type2, observation);
            }
        }

        private static void AddRequiredComponent(Type componentType, UnityTypeObservation observation)
        {
            if (componentType != null)
                observation.RequiredComponents.Add(componentType.FullName);
        }

        private static void AddContracts(Type type, UnityTypeObservation observation)
        {
            observation.Contracts.AddRange(AuthoringContractResolver.Resolve(type));
        }
    }
}
