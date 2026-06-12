using System;
using System.Collections.Generic;
using System.Reflection;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Editor.Inspectors;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    /// <summary>
    /// Validates setup contracts after they have been resolved by the central contract registry.
    /// Attribute-backed and provider-backed contracts use the same path here.
    /// </summary>
    public static class PyralisReflectiveContractSolver
    {
        public static PyralisSetupFlowReport BuildReport(GameplaySessionBootstrap bootstrap)
        {
            List<PyralisSetupFlowStep> steps = new List<PyralisSetupFlowStep>();
            if (bootstrap == null) return new PyralisSetupFlowReport(steps);

            var routeAnalysis = PyralisSetupRouteAnalysis.Build(bootstrap);
            var activeAxioms = DeriveAxiomsFromRoute(routeAnalysis);

            foreach (ResolvedAuthoringContract contract in ResolvedAuthoringContractRegistry.All)
            {
                if (contract == null || !IsRelevant(contract, routeAnalysis, activeAxioms))
                    continue;

                Object reference = null;
                bool satisfied = true;
                List<string> missingTypes = new List<string>();

                if (contract.RequiredRuntimeInterfaceNames != null)
                {
                    foreach (var ifaceName in contract.RequiredRuntimeInterfaceNames)
                    {
                        var resolvedType = FindTypeByName(ifaceName);
                        var found = resolvedType != null ? FindTypeInContext(resolvedType, bootstrap) : null;
                        if (found != null)
                        {
                            if (reference == null) reference = found;
                        }
                        else
                        {
                            satisfied = false;
                            missingTypes.Add(GetShortTypeName(ifaceName));
                        }
                    }
                }

                if (contract.RequiredComponentNames != null)
                {
                    foreach (var compName in contract.RequiredComponentNames)
                    {
                        var resolvedType = FindTypeByName(compName);
                        var found = resolvedType != null ? FindTypeInContext(resolvedType, bootstrap) : null;
                        if (found != null)
                        {
                            if (reference == null) reference = found;
                        }
                        else
                        {
                            satisfied = false;
                            missingTypes.Add(GetShortTypeName(compName));
                        }
                    }
                }

                if (satisfied &&
                    contract.RequiredRuntimeInterfaceNames.Length == 0 &&
                    contract.RequiredComponentNames.Length == 0 &&
                    contract.SourceType != null &&
                    (contract.SourceType.IsInterface || typeof(Component).IsAssignableFrom(contract.SourceType)))
                {
                    var found = FindTypeInContext(contract.SourceType, bootstrap);
                    if (found != null)
                    {
                        if (reference == null) reference = found;
                    }
                    else
                    {
                        satisfied = false;
                        missingTypes.Add(contract.SourceType.Name);
                    }
                }

                string label = !string.IsNullOrWhiteSpace(contract.AuthoringCategory)
                    ? contract.AuthoringCategory
                    : contract.DisplayName;
                PyralisSetupFlowStepStatus status = satisfied ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Recommended;
                    
                string message;
                if (satisfied)
                {
                    message = $"Contract for {label} is satisfied.";

                    if (!string.IsNullOrEmpty(contract.FirstProofTargetId))
                        message += $"\n\n<b>First Proof:</b> {contract.FirstProofTargetId}";
                        
                    if (reference != null && contract.AssignmentFields != null && contract.AssignmentFields.Length > 0)
                    {
                        var missingFields = ValidateAssignmentFields(reference, contract.AssignmentFields);
                        if (missingFields.Count > 0)
                        {
                            satisfied = false;
                            status = PyralisSetupFlowStepStatus.Recommended;
                            message = $"Contract {label} is present, but missing field assignments: {string.Join(", ", missingFields)}.";
                                
                            if (!string.IsNullOrEmpty(contract.ExpertAdvice))
                                message += $"\n\n<b>Expert Advice:</b> {contract.ExpertAdvice}";
                        }
                    }
                }
                else
                {
                    message = $"Missing required types for {label}: {string.Join(", ", missingTypes)}.";
                        
                    if (!string.IsNullOrEmpty(contract.ExpertAdvice))
                        message += $"\n\n<b>Expert Advice:</b> {contract.ExpertAdvice}";
                }

                PyralisSetupFlowActionKind actionKind = PyralisSetupFlowActionKind.None;
                if (!satisfied && contract.RequiredProfileType != null)
                    actionKind = PyralisSetupFlowActionKind.CreateProfile;

                steps.Add(new PyralisSetupFlowStep(
                    label,
                    status,
                    message,
                    reference ?? bootstrap,
                    actionKind,
                    PyralisSetupFlowStepId.Unknown,
                    PyralisSetupFlowWorkIntent.ProofEnhancer,
                    null,
                    contract.RequiredProfileType
                ));
            }

            return new PyralisSetupFlowReport(steps);
        }

        public static bool IsSatisfied(PyralisAuthoringFact fact, out string message, out Object reference)
        {
            message = string.Empty;
            reference = null;

            if (fact == null) return false;

            var bootstrap = Object.FindAnyObjectByType<GameplaySessionBootstrap>();
            if (bootstrap == null)
            {
                message = "No GameplaySessionBootstrap found in scene to validate requirements.";
                return false;
            }

            bool satisfied = true;
            List<string> missing = new List<string>();

            // Check Scene Components
            if (fact.RequiredSceneComponents != null)
            {
                foreach (var compName in fact.RequiredSceneComponents)
                {
                    var type = FindTypeByName(compName);
                    var found = type != null ? FindTypeInContext(type, bootstrap) : null;
                    if (found != null)
                    {
                        if (reference == null) reference = found;
                    }
                    else
                    {
                        satisfied = false;
                        missing.Add(GetShortTypeName(compName));
                    }
                }
            }

            // Check Interfaces/Components from FeatureContract/PrefabComponent kind
            if (fact.Kind == PyralisAuthoringFactKind.FeatureContract || fact.Kind == PyralisAuthoringFactKind.PrefabComponent)
            {
                // We don't easily have the original Type here unless we look it up from the StableId or Registry.
                // For now, if it's already satisfied by scene components, we are good.
            }

            if (satisfied)
            {
                message = "Requirements satisfied in current scene.";
                return true;
            }

            message = $"Missing: {string.Join(", ", missing)}";
            return false;
        }

        public static int ResolveReflectivePriority(ResolvedAuthoringContract contract, AuthoringWorldAxiom activeIntentAxioms)
        {
            int basePriority = contract.PriorityValueOverride > 0 ? contract.PriorityValueOverride : contract.Priority;
            if (contract.Priority == (int)AuthoringPriority.Deprecated) return (int)AuthoringPriority.Deprecated;
            if (contract.Priority == (int)AuthoringPriority.Primary) return (int)AuthoringPriority.Primary;

            if (contract.Axioms != AuthoringWorldAxiom.None)
            {
                int matches = CountAxiomFlags(contract.Axioms & activeIntentAxioms);
                int clashes = CountAxiomFlags(contract.Axioms & ~activeIntentAxioms);
                return basePriority + (matches * 50) - (clashes * 25);
            }
            return basePriority;
        }

        private static int CountAxiomFlags(AuthoringWorldAxiom axiom)
        {
            int count = 0;
            int value = (int)axiom;
            while (value > 0)
            {
                count += value & 1;
                value >>= 1;
            }
            return count;
        }

        private static List<string> ValidateAssignmentFields(Object target, string[] fields)
        {
            List<string> missing = new List<string>();
            if (target == null || fields == null) return missing;

            SerializedObject so = new SerializedObject(target);
            foreach (var field in fields)
            {
                SerializedProperty prop = so.FindProperty(field);
                if (prop == null) continue;

                bool isMissing = false;
                switch (prop.propertyType)
                {
                    case SerializedPropertyType.ObjectReference:
                        isMissing = prop.objectReferenceValue == null;
                        break;
                    case SerializedPropertyType.String:
                        isMissing = string.IsNullOrEmpty(prop.stringValue);
                        break;
                    case SerializedPropertyType.ArraySize:
                        isMissing = prop.arraySize == 0;
                        break;
                }

                if (isMissing)
                    missing.Add(prop.displayName);
            }
            return missing;
        }

        private static bool IsRelevant(ResolvedAuthoringContract contract, PyralisSetupRouteAnalysis route, AuthoringWorldAxiom activeAxioms)
        {
            if (contract.Capability == AuthoringCapability.None && contract.Axioms == AuthoringWorldAxiom.None)
                return true;

            string categoryLabel = contract.Capability != AuthoringCapability.None 
                ? AuthoringCapabilityRegistry.GetDisplayName(contract.Capability) 
                : string.Empty;

            if (!string.IsNullOrEmpty(categoryLabel))
            {
                foreach (var fact in route.RouteFacts)
                {
                    if (string.Equals(fact.Label, categoryLabel, StringComparison.OrdinalIgnoreCase))
                        return true;

                    // Hierarchical match: e.g. 'Combat/Reaction' matches 'Combat'
                    if (categoryLabel.StartsWith(fact.Label + "/", StringComparison.OrdinalIgnoreCase) ||
                        fact.Label.StartsWith(categoryLabel + "/", StringComparison.OrdinalIgnoreCase))
                        return true;
                }

                if (route.Patterns != null)
                {
                    foreach (var pattern in route.Patterns)
                    {
                        if (pattern == null) continue;
                        string family = pattern.capabilityFamily.ToString();
                        
                        if (string.Equals(family, categoryLabel, StringComparison.OrdinalIgnoreCase))
                            return true;

                        // Hierarchical match: e.g. 'Combat/Reaction' matches 'Combat' family
                        if (categoryLabel.StartsWith(family + "/", StringComparison.OrdinalIgnoreCase) ||
                            family.StartsWith(categoryLabel + "/", StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                }
            }

            if (contract.Axioms != AuthoringWorldAxiom.None)
            {
                if ((contract.Axioms & activeAxioms) != 0)
                    return true;
            }

            return false;
        }

        private static AuthoringWorldAxiom DeriveAxiomsFromRoute(PyralisSetupRouteAnalysis route)
        {
            AuthoringWorldAxiom axioms = AuthoringWorldAxiom.None;

            if (route.Requires2DCameraBounds()) axioms |= AuthoringWorldAxiom.Dimensions2D;
            if (route.UsesPawnGameplay()) axioms |= AuthoringWorldAxiom.Realtime;
            if (route.UsesTabletopContract()) axioms |= AuthoringWorldAxiom.TurnBased;

            // Map presentation lanes to mechanical axioms (Dimensions)
            if (route.Patterns != null)
            {
                foreach (var pattern in route.Patterns)
                {
                    if (pattern == null) continue;

                    foreach (var lane in pattern.presentationLanes)
                    {
                        switch (lane)
                        {
                            case RuntimePatternPresentationLane.Sprite2D:
                                axioms |= AuthoringWorldAxiom.Dimensions2D;
                                break;
                            case RuntimePatternPresentationLane.Billboard2_5D:
                                axioms |= AuthoringWorldAxiom.Dimensions2D;
                                break;
                            case RuntimePatternPresentationLane.Rigged3D:
                                axioms |= AuthoringWorldAxiom.Dimensions3D;
                                break;
                        }
                    }
                }
            }

            return axioms;
        }

        private static Object FindTypeInContext(Type type, GameplaySessionBootstrap bootstrap)
        {
            if (type == null) return null;

            // 1. Check if it's on the bootstrap itself
            if (bootstrap != null && (typeof(Component).IsAssignableFrom(type) || type.IsInterface))
            {
                var componentOnBootstrap = bootstrap.GetComponent(type);
                if (componentOnBootstrap != null) return componentOnBootstrap;
            }

            if (type.IsInterface)
                return FindInterfaceInScene(type, bootstrap);

            if (!typeof(Object).IsAssignableFrom(type))
                return null;

            // 2. Check in the scene (active and inactive)
            var objects = Object.FindObjectsByType(type, FindObjectsInactive.Include);
            if (objects != null && objects.Length > 0)
            {
                // Prefer objects in the same scene as bootstrap if possible
                foreach (var obj in objects)
                {
                    if (bootstrap != null && obj is Component comp && comp.gameObject.scene == bootstrap.gameObject.scene)
                        return obj;
                }
                return objects[0];
            }

            return null;
        }

        private static Object FindInterfaceInScene(Type interfaceType, GameplaySessionBootstrap bootstrap)
        {
            MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null || !interfaceType.IsAssignableFrom(behaviour.GetType()))
                    continue;

                if (bootstrap == null || behaviour.gameObject.scene == bootstrap.gameObject.scene)
                    return behaviour;
            }

            ScriptableObject[] assets = Resources.FindObjectsOfTypeAll<ScriptableObject>();
            for (int i = 0; i < assets.Length; i++)
            {
                ScriptableObject asset = assets[i];
                if (asset != null && interfaceType.IsAssignableFrom(asset.GetType()))
                    return asset;
            }

            return null;
        }

        private static Type FindTypeByName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return null;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName);
                if (type != null) return type;
            }

            return null;
        }

        private static string GetShortTypeName(string fullTypeName)
        {
            if (string.IsNullOrWhiteSpace(fullTypeName)) return string.Empty;
            int lastDot = fullTypeName.LastIndexOf('.');
            return lastDot >= 0 ? fullTypeName.Substring(lastDot + 1) : fullTypeName;
        }
    }
}

