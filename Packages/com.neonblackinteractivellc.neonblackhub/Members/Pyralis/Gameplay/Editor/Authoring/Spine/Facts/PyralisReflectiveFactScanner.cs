using System;
using System.Collections.Generic;
using System.Reflection;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    public static class PyralisReflectiveFactScanner
    {
        public static IReadOnlyList<PyralisAuthoringFact> ScanProject()
        {
            List<PyralisAuthoringFact> facts = new List<PyralisAuthoringFact>();
            HashSet<string> seenStableIds = new HashSet<string>(StringComparer.Ordinal);

            // 1. Singular reflective stream for attributes
            var typesWithAttribute = TypeCache.GetTypesWithAttribute<AuthoringContractAttribute>();
            foreach (var type in typesWithAttribute)
            {
                var attributes = type.GetCustomAttributes<AuthoringContractAttribute>();
                foreach (var attr in attributes)
                {
                    var fact = CreateFact(type, attr);
                    if (fact != null && seenStableIds.Add(fact.StableId))
                        facts.Add(fact);
                }
            }

            // 2. Process any remaining contracts from the registry (e.g., from IAuthoringContractProvider)
            foreach (var contract in PyralisAuthoringContractRegistry.All)
            {
                if (seenStableIds.Contains(contract.StableId))
                    continue;

                var fact = CreateFactFromContract(contract);
                if (fact != null && seenStableIds.Add(fact.StableId))
                    facts.Add(fact);
            }

            return facts;
        }

        public static PyralisAuthoringFact CreateFact(Type type, AuthoringContractAttribute attr)
        {
            if (!string.IsNullOrEmpty(attr.ModuleId))
            {
                var contract = PyralisAuthoringContractRegistry.FindByModuleId(attr.ModuleId);
                if (contract != null)
                    return CreateFactFromContract(contract, type);
            }

            return CreateFactFromAttribute(type, attr);
        }

        public static PyralisAuthoringFact CreateFactFromContract(PyralisAuthoringContract contract, Type type = null)
        {
            List<string> discoveredAssignment = new List<string>();
            List<string> discoveredCustomization = new List<string>();
            DiscoverAuthoringFields(type, discoveredAssignment, discoveredCustomization);
            DiscoverAuthoringFields(contract.RequiredProfileType, discoveredAssignment, discoveredCustomization);

            return new PyralisAuthoringFact(
                contract.StableId,
                contract.DisplayName,
                PyralisAuthoringFactKind.FeatureContract,
                PyralisAuthoringFactSourceKind.FeatureContract,
                contract.Confidence,
                $"Feature contract for {contract.AuthoringCategory} module setup and lane compatibility.",
                $"Feature contract for {contract.AuthoringCategory} lane coverage and compatibility.",
                contract.FirstProofTargetId,
                requiredProfiles: GetRequiredProfiles(contract.RequiredProfileType),
                requiredPrefabComponents: NormalizeTypeNames(contract.RequiredRuntimeInterfaceNames),
                laneTags: ToStringArray(contract.SupportedPresentationModes),
                unsupportedLaneTags: ToStringArray(contract.UnsupportedPresentationModes),
                assignmentFields: MergeAndDeDuplicateFields(contract.AssignmentFields, discoveredAssignment),
                customizationMoments: MergeAndDeDuplicateFields(contract.CustomizationMoments, discoveredCustomization),
                nativeActions: BuildNativeActions(type, contract.NativeSetup),
                relatedStableIds: BuildRelatedStableIds(contract.FirstProofTargetId),
                capability: contract.Capability,
                priority: (AuthoringPriority)contract.Priority,
                priorityValueOverride: contract.PriorityValueOverride,
                deprecatedInVersion: contract.DeprecatedInVersion,
                removableInVersion: contract.RemovableInVersion,
                documentationURL: contract.DocumentationURL,
                expertAdvice: contract.ExpertAdvice);
        }

        public static PyralisAuthoringFact CreateFactFromAttribute(Type type, AuthoringContractAttribute attr)
        {
            string categoryLabel = attr.Capability != AuthoringCapability.None 
                ? AuthoringCapabilityRegistry.GetDisplayName(attr.Capability) 
                : string.Empty;

            string stableId = $"reflective.{type.FullName.ToLowerInvariant()}.{categoryLabel?.ToLowerInvariant() ?? "none"}";
            string displayName = string.IsNullOrWhiteSpace(categoryLabel) 
                ? AuthoringCapabilityRegistry.PrettifyTypeName(type.Name) 
                : $"{AuthoringCapabilityRegistry.PrettifyTypeName(type.Name)} ({categoryLabel})";

            List<string> discoveredAssignment = new List<string>();
            List<string> discoveredCustomization = new List<string>();
            DiscoverAuthoringFields(type, discoveredAssignment, discoveredCustomization);
            DiscoverAuthoringFields(attr.ProfileType, discoveredAssignment, discoveredCustomization);

            List<string> requiredPrefabComponents = new List<string>();
            if (attr.RequiredInterfaces != null)
            {
                foreach (var iface in attr.RequiredInterfaces)
                    if (iface != null) requiredPrefabComponents.Add(iface.Name);
            }
            if (attr.RequiredInterfaceNames != null)
            {
                foreach (var name in attr.RequiredInterfaceNames)
                    if (!string.IsNullOrWhiteSpace(name)) requiredPrefabComponents.Add(SimplifyTypeName(name));
            }

            // Synthesize from RequireComponent
            var requireCompAttrs = type.GetCustomAttributes<RequireComponent>(true);
            foreach (var req in requireCompAttrs)
            {
                var t0 = GetRequireComponentType(req, 0);
                if (t0 != null) requiredPrefabComponents.Add(t0.Name);
                var t1 = GetRequireComponentType(req, 1);
                if (t1 != null) requiredPrefabComponents.Add(t1.Name);
                var t2 = GetRequireComponentType(req, 2);
                if (t2 != null) requiredPrefabComponents.Add(t2.Name);
            }

            string[] finalRequiredPrefabComponents = null;
            if (requiredPrefabComponents.Count > 0)
            {
                HashSet<string> unique = new HashSet<string>(StringComparer.Ordinal);
                foreach (var name in requiredPrefabComponents)
                    if (!string.IsNullOrWhiteSpace(name)) unique.Add(name);

                finalRequiredPrefabComponents = new string[unique.Count];
                unique.CopyTo(finalRequiredPrefabComponents);
            }

            List<string> requiredSceneComponents = new List<string>();
            if (attr.RequiredComponents != null)
            {
                foreach (var comp in attr.RequiredComponents)
                    if (comp != null) requiredSceneComponents.Add(comp.Name);
            }
            if (attr.RequiredComponentNames != null)
            {
                foreach (var name in attr.RequiredComponentNames)
                    if (!string.IsNullOrWhiteSpace(name)) requiredSceneComponents.Add(SimplifyTypeName(name));
            }

            return new PyralisAuthoringFact(
                stableId: stableId,
                displayName: displayName,
                kind: DetermineFactKind(type),
                sourceKind: PyralisAuthoringFactSourceKind.Reflection,
                confidence: PyralisAuthoringConfidence.Explicit,
                summary: attr.Relevance ?? $"Reflective authoring contract discovered for {type.Name}.",
                routeRelevance: $"Directly tagged reflective contract for {categoryLabel ?? "general"} gameplay.",
                firstProof: attr.FirstProof ?? string.Empty,
                goalTags: !string.IsNullOrWhiteSpace(categoryLabel) ? new[] { categoryLabel } : null,
                laneTags: ToStringArray(attr.SupportedLanes),
                unsupportedLaneTags: ToStringArray(attr.UnsupportedLanes),
                requiredProfiles: GetRequiredProfiles(attr.ProfileType),
                requiredPrefabComponents: finalRequiredPrefabComponents,
                requiredSceneComponents: requiredSceneComponents.Count > 0 ? requiredSceneComponents.ToArray() : null,
                assignmentFields: MergeAndDeDuplicateFields(attr.AssignmentFields, discoveredAssignment),
                customizationMoments: MergeAndDeDuplicateFields(attr.CustomizationMoments, discoveredCustomization),
                nativeActions: BuildNativeActions(type, attr.NativeSetup),
                workIntent: attr.AxiomKeywords,
                axioms: attr.Axioms,
                capability: attr.Capability,
                priority: attr.Priority,
                priorityValueOverride: attr.PriorityValueOverride,
                deprecatedInVersion: attr.DeprecatedInVersion,
                removableInVersion: attr.RemovableInVersion,
                documentationURL: attr.DocumentationURL,
                expertAdvice: attr.ExpertAdvice
            );
        }

        private static void DiscoverAuthoringFields(Type type, List<string> assignmentFields, List<string> customizationMoments)
        {
            if (type == null) return;

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                // Serialized if public and not [NonSerialized], or if [SerializeField]
                bool isSerialized = (field.IsPublic && !field.IsInitOnly && field.GetCustomAttribute<NonSerializedAttribute>() == null)
                                    || field.GetCustomAttribute<SerializeField>() != null;

                if (!isSerialized || field.GetCustomAttribute<HideInInspector>() != null)
                    continue;

                // Support for PropertyOrder (common in many Unity inspector frameworks like Odin or custom ones)
                // If the field has a high order or is marked as a "Customization" moment by naming or type, categorize it accordingly.
                bool isCustomization = IsCustomizationMoment(field.FieldType);
                
                // Heuristic: Check for common "Customization" naming patterns in properties or attributes
                if (field.Name.IndexOf("custom", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    field.Name.IndexOf("moment", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    field.Name.IndexOf("variant", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    isCustomization = true;
                }

                string fieldEntry = $"{type.Name}.{field.Name} -> {field.FieldType.Name}";

                if (isCustomization)
                    customizationMoments.Add(fieldEntry);
                else
                    assignmentFields.Add(fieldEntry);
            }
        }

        private static bool IsCustomizationMoment(Type type)
        {
            if (type == null || type == typeof(string)) return false;

            // Heuristic 1: Collections (Array or List/IEnumerable)
            if (type.IsArray || typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
                return true;

            // Heuristic 2: Specific gameplay-related types (Feature or Action)
            Type current = type;
            while (current != null && current != typeof(object))
            {
                if (current.Name.Contains("Feature") || current.Name.Contains("Action"))
                    return true;
                current = current.BaseType;
            }

            return false;
        }

        private static string[] MergeAndDeDuplicateFields(string[] manual, List<string> discovered)
        {
            HashSet<string> seenBaseFields = new HashSet<string>(StringComparer.Ordinal);
            List<string> finalFields = new List<string>();

            // Prioritize manual entries
            if (manual != null)
            {
                foreach (var field in manual)
                {
                    if (string.IsNullOrWhiteSpace(field)) continue;
                    
                    // Extract base "TypeName.FieldName" for de-duplication
                    string baseField = field.Split(new[] { " -> " }, StringSplitOptions.None)[0].Trim();
                    if (seenBaseFields.Add(baseField))
                        finalFields.Add(field);
                }
            }

            // Add discovered entries if not already present
            if (discovered != null)
            {
                foreach (var field in discovered)
                {
                    string baseField = field.Split(new[] { " -> " }, StringSplitOptions.None)[0].Trim();
                    if (seenBaseFields.Add(baseField))
                        finalFields.Add(field);
                }
            }

            return finalFields.Count > 0 ? finalFields.ToArray() : Array.Empty<string>();
        }

        private static Type GetRequireComponentType(RequireComponent attr, int index)
        {
            var field = typeof(RequireComponent).GetField($"m_Type{index}", BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.GetValue(attr) as Type;
        }

        internal static string[] GetRequiredProfiles(Type requiredProfileType)
        {
            if (requiredProfileType == null)
                return Array.Empty<string>();

            return new[] { requiredProfileType.Name };
        }

        private static string[] NormalizeTypeNames(string[] typeNames)
        {
            if (typeNames == null || typeNames.Length == 0)
                return Array.Empty<string>();

            string[] normalized = new string[typeNames.Length];
            for (int i = 0; i < typeNames.Length; i++)
                normalized[i] = SimplifyTypeName(typeNames[i]);

            return normalized;
        }

        private static string SimplifyTypeName(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return string.Empty;

            int lastDot = typeName.LastIndexOf('.');
            if (lastDot < 0 || lastDot >= typeName.Length - 1)
                return typeName;

            return typeName.Substring(lastDot + 1);
        }

        private static string[] BuildRelatedStableIds(string firstProofTargetId)
        {
            if (string.IsNullOrWhiteSpace(firstProofTargetId))
                return Array.Empty<string>();

            return new[] { firstProofTargetId };
        }

        internal static string[] ToStringArray(ActorPresentationMode[] presentationModes)
        {
            if (presentationModes == null || presentationModes.Length == 0)
                return Array.Empty<string>();

            string[] values = new string[presentationModes.Length];
            for (int i = 0; i < presentationModes.Length; i++)
                values[i] = presentationModes[i].ToString();

            return values;
        }

        internal static PyralisAuthoringNativeAction[] BuildNativeActions(Type type, string[] nativeSetup)
        {
            List<PyralisAuthoringNativeAction> actions = new List<PyralisAuthoringNativeAction>();

            if (nativeSetup != null && nativeSetup.Length > 0)
            {
                for (int i = 0; i < nativeSetup.Length; i++)
                {
                    string step = nativeSetup[i];
                    if (string.IsNullOrWhiteSpace(step))
                        continue;

                    actions.Add(BuildNativeActionFromStep(step));
                }
            }

            if (type == null)
                return actions.ToArray();

            var createAssetAttr = type.GetCustomAttribute<CreateAssetMenuAttribute>();
            if (createAssetAttr != null)
            {
                string menuName = string.IsNullOrWhiteSpace(createAssetAttr.menuName) 
                    ? AuthoringCapabilityRegistry.PrettifyTypeName(type.Name) 
                    : createAssetAttr.menuName;
                
                actions.Add(new PyralisAuthoringNativeAction(
                    "Create", 
                    PyralisAuthoringActionSurface.ProjectWindow, 
                    AuthoringCapabilityRegistry.PrettifyTypeName(type.Name), 
                    menuName, 
                    "the asset exists in the project"));
            }

            var addComponentAttr = type.GetCustomAttribute<AddComponentMenu>();
            if (addComponentAttr != null && !string.IsNullOrWhiteSpace(addComponentAttr.componentMenu))
            {
                actions.Add(new PyralisAuthoringNativeAction(
                    "Add Component", 
                    PyralisAuthoringActionSurface.Inspector, 
                    "selected GameObject", 
                    addComponentAttr.componentMenu, 
                    "the component is added to the Inspector"));
            }

            return actions.ToArray();
        }

        private static PyralisAuthoringNativeAction BuildNativeActionFromStep(string step)
        {
            string action = step.Trim();
            string lowered = action.ToLowerInvariant();

            string verb = "Inspect";
            PyralisAuthoringActionSurface surface = PyralisAuthoringActionSurface.Inspector;
            string target = "feature setup";
            string field = action;
            string success = "the setup step is captured and verified in the editor";

            if (lowered.StartsWith("create", StringComparison.Ordinal))
            {
                verb = "Create";
                surface = PyralisAuthoringActionSurface.ProjectWindow;
                success = "the referenced item exists in the chosen project folder";
            }
            else if (lowered.StartsWith("assign ", StringComparison.Ordinal) || lowered.StartsWith("add ", StringComparison.Ordinal))
            {
                verb = "Assign";
                target = "Feature module setup";
                success = "the value is assigned in the authored definition";
            }
            else if (lowered.StartsWith("bind ", StringComparison.Ordinal))
            {
                verb = "Bind";
                target = "Action binding";
                success = "the action role is connected in the authored action graph";
            }

            return new PyralisAuthoringNativeAction(verb, surface, target, field, success);
        }

        private static PyralisAuthoringFactKind DetermineFactKind(Type type)
        {
            if (typeof(ScriptableObject).IsAssignableFrom(type))
            {
                if (type.Name.EndsWith("Profile")) return PyralisAuthoringFactKind.Profile;
                return PyralisAuthoringFactKind.Definition;
            }
            
            if (typeof(Component).IsAssignableFrom(type))
            {
                return PyralisAuthoringFactKind.PrefabComponent;
            }

            if (type.IsInterface) return PyralisAuthoringFactKind.FeatureContract;

            return PyralisAuthoringFactKind.RuntimeCapability;
        }
    }
}
