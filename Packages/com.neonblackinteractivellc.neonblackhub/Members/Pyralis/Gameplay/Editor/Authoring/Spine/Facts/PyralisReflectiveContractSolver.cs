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
    /// Provides a reflective way to validate game setup contracts tagged with <see cref="AuthoringContractAttribute"/>.
    /// This unifies authoring logic into a single reflective spine, allowing new features to declare their
    /// own validation requirements without modifying the core validator.
    /// </summary>
    public static class PyralisReflectiveContractSolver
    {
        public static PyralisSetupFlowReport BuildReport(GameplaySessionBootstrap bootstrap)
        {
            List<PyralisSetupFlowStep> steps = new List<PyralisSetupFlowStep>();
            if (bootstrap == null) return new PyralisSetupFlowReport(steps);

            // Analysis of the current setup to filter relevant contracts
            var routeAnalysis = PyralisSetupRouteAnalysis.Build(bootstrap);
            var activeAxioms = DeriveAxiomsFromRoute(routeAnalysis);

            // Use TypeCache for fast discovery of types with the AuthoringContract attribute
            var typesWithAttribute = TypeCache.GetTypesWithAttribute<AuthoringContractAttribute>();
            foreach (var type in typesWithAttribute)
            {
                var attributes = type.GetCustomAttributes<AuthoringContractAttribute>();
                foreach (var attr in attributes)
                {
                    if (attr == null) continue;

                    // Only include contracts relevant to the current gameplay route and world intent
                    if (!IsRelevant(attr, routeAnalysis, activeAxioms))
                        continue;

                    Object reference = null;
                    bool satisfied = true;
                    List<string> missingTypes = new List<string>();

                    // Validate Required Interfaces
                    if (attr.RequiredInterfaces != null)
                    {
                        foreach (var iface in attr.RequiredInterfaces)
                        {
                            var found = FindTypeInContext(iface, bootstrap);
                            if (found != null)
                            {
                                if (reference == null) reference = found;
                            }
                            else
                            {
                                satisfied = false;
                                missingTypes.Add(iface.Name);
                            }
                        }
                    }

                    if (attr.RequiredInterfaceNames != null)
                    {
                        foreach (var ifaceName in attr.RequiredInterfaceNames)
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

                    // Validate Required Components
                    if (attr.RequiredComponents != null)
                    {
                        foreach (var comp in attr.RequiredComponents)
                        {
                            var found = FindTypeInContext(comp, bootstrap);
                            if (found != null)
                            {
                                if (reference == null) reference = found;
                            }
                            else
                            {
                                satisfied = false;
                                missingTypes.Add(comp.Name);
                            }
                        }
                    }

                    if (attr.RequiredComponentNames != null)
                    {
                        foreach (var compName in attr.RequiredComponentNames)
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

                    // If no explicit requirements specified in the attribute, automatically check for the type itself
                    // if it's an interface or a component implementation.
                    if (satisfied && (attr.RequiredInterfaces == null || attr.RequiredInterfaces.Length == 0) && 
                        (attr.RequiredComponents == null || attr.RequiredComponents.Length == 0))
                    {
                        if (type.IsInterface || typeof(Component).IsAssignableFrom(type))
                        {
                            var found = FindTypeInContext(type, bootstrap);
                            if (found != null)
                            {
                                if (reference == null) reference = found;
                            }
                            else
                            {
                                satisfied = false;
                                missingTypes.Add(type.Name);
                            }
                        }
                    }

                    string categoryLabel = attr.Capability != AuthoringCapability.None 
                        ? AuthoringCapabilityRegistry.GetDisplayName(attr.Capability) 
                        : string.Empty;

                                        string label = string.IsNullOrWhiteSpace(categoryLabel) ? AuthoringCapabilityRegistry.PrettifyTypeName(type.Name) : categoryLabel;
                    PyralisSetupFlowStepStatus status = satisfied ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Missing;
                    
                    string message;
                    if (satisfied)
                    {
                        message = attr.SatisfactionReason ?? $"Contract for {label} is satisfied.";

                        // Surfacing expert advice and first proof even when satisfied
                        if (!string.IsNullOrEmpty(attr.FirstProof))
                            message += $"\n\n<b>First Proof:</b> {attr.FirstProof}";
                        
                        // NEW: Validate AssignmentFields if satisfied
                        if (reference != null && attr.AssignmentFields != null && attr.AssignmentFields.Length > 0)
                        {
                            var missingFields = ValidateAssignmentFields(reference, attr.AssignmentFields);
                            if (missingFields.Count > 0)
                            {
                                satisfied = false;
                                status = PyralisSetupFlowStepStatus.Missing;
                                message = $"Contract {label} is present, but missing field assignments: {string.Join(", ", missingFields)}.";
                                
                                if (!string.IsNullOrEmpty(attr.ExpertAdvice))
                                    message += $"\n\n<b>Expert Advice:</b> {attr.ExpertAdvice}";
                            }
                        }
                    }
                    else
                    {
                        message = $"Missing required types for {label}: {string.Join(", ", missingTypes)}.";
                        
                        if (!string.IsNullOrEmpty(attr.ExpertAdvice))
                            message += $"\n\n<b>Expert Advice:</b> {attr.ExpertAdvice}";
                    }

                    PyralisSetupFlowActionKind actionKind = PyralisSetupFlowActionKind.None;
if (!satisfied && attr.ProfileType != null)
                    {
                        actionKind = PyralisSetupFlowActionKind.CreateProfile;
                    }

                    steps.Add(new PyralisSetupFlowStep(
                        label,
                        status,
                        message,
                        reference ?? bootstrap,
                        actionKind,
                        PyralisSetupFlowStepId.Unknown,
                        PyralisSetupFlowWorkIntent.RequiredSetup,
                        null,
                        attr.ProfileType
                    ));
}
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

        public static int ResolveReflectivePriority(AuthoringContractAttribute attr, AuthoringWorldAxiom activeIntentAxioms)
        {
            int basePriority = attr.PriorityValueOverride > 0 ? attr.PriorityValueOverride : (int)attr.Priority;
            if (attr.Priority == AuthoringPriority.Deprecated) return (int)AuthoringPriority.Deprecated;
            if (attr.Priority == AuthoringPriority.Primary) return (int)AuthoringPriority.Primary;

            if (attr.Axioms != AuthoringWorldAxiom.None)
            {
                int matches = CountAxiomFlags(attr.Axioms & activeIntentAxioms);
                int clashes = CountAxiomFlags(attr.Axioms & ~activeIntentAxioms);
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

        private static bool IsRelevant(AuthoringContractAttribute attr, PyralisSetupRouteAnalysis route, AuthoringWorldAxiom activeAxioms)
        {
            // If no filtering criteria specified, it's always relevant
            if (attr.Capability == AuthoringCapability.None && attr.Axioms == AuthoringWorldAxiom.None)
                return true;

            // Check Capability relevance (matches route facts or capability families)
            string categoryLabel = attr.Capability != AuthoringCapability.None 
                ? AuthoringCapabilityRegistry.GetDisplayName(attr.Capability) 
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

            // Check Axiom relevance (overlap with derived world axioms)
            if (attr.Axioms != AuthoringWorldAxiom.None)
            {
                if ((attr.Axioms & activeAxioms) != 0)
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
            var componentOnBootstrap = bootstrap.GetComponent(type);
            if (componentOnBootstrap != null) return componentOnBootstrap;

            // 2. Check in the scene (active and inactive)
            var objects = Object.FindObjectsByType(type, FindObjectsInactive.Include);
            if (objects != null && objects.Length > 0)
            {
                // Prefer objects in the same scene as bootstrap if possible
                foreach (var obj in objects)
                {
                    if (obj is Component comp && comp.gameObject.scene == bootstrap.gameObject.scene)
                        return obj;
                }
                return objects[0];
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

