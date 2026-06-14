using System;
using System.Collections.Generic;
using System.Reflection;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
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

            foreach (var contract in ResolvedAuthoringContractRegistry.All)
            {
                var fact = CreateFactFromContract(contract);
                if (fact != null && seenStableIds.Add(fact.StableId))
                    facts.Add(fact);
            }

            AddUnityMetadataFacts(facts, seenStableIds);

            return facts;
        }

        private static void AddUnityMetadataFacts(List<PyralisAuthoringFact> facts, HashSet<string> seenStableIds)
        {
            foreach (Type type in GetGameplayObjectTypes())
            {
                if (type == null || type.IsAbstract || !typeof(ScriptableObject).IsAssignableFrom(type) || !IsGameplayType(type))
                    continue;

                CreateAssetMenuAttribute attribute = type.GetCustomAttribute<CreateAssetMenuAttribute>();
                if (attribute == null || string.IsNullOrWhiteSpace(attribute.menuName))
                    continue;

                string stableId = "reflection.create-asset-menu." + ToStableSlug(type.Name);
                if (!seenStableIds.Add(stableId))
                    continue;

                PyralisAuthoringFactKind kind = type.Name.EndsWith("Profile", StringComparison.Ordinal)
                    ? PyralisAuthoringFactKind.Profile
                    : PyralisAuthoringFactKind.Definition;
                string fileName = string.IsNullOrWhiteSpace(attribute.fileName) ? type.Name : attribute.fileName;
                string createPath = "Assets/Create/" + attribute.menuName;

                facts.Add(new PyralisAuthoringFact(
                    stableId,
                    AuthoringCapabilityRegistry.PrettifyTypeName(type.Name),
                    kind,
                    PyralisAuthoringFactSourceKind.Reflection,
                    PyralisAuthoringConfidence.Explicit,
                    $"Unity CreateAssetMenu path for {type.Name}.",
                    $"Create {type.Name} through the Project window when this route needs the asset.",
                    string.Empty,
                    requiredDefinitions: kind == PyralisAuthoringFactKind.Definition ? new[] { type.Name } : null,
                    requiredProfiles: kind == PyralisAuthoringFactKind.Profile ? new[] { type.Name } : null,
                    nativeActions: new[]
                    {
                        new PyralisAuthoringNativeAction(
                            "Create",
                            PyralisAuthoringActionSurface.ProjectWindow,
                            createPath,
                            fileName,
                            type.Name + " asset exists in the chosen project folder")
                    },
                    workIntent: "NativeCreatePath",
                    relatedStableIds: BuildDependencyRelatedStableIds(type)));
            }

            foreach (Type type in GetGameplayObjectTypes())
            {
                if (type == null || type.IsAbstract || !typeof(Component).IsAssignableFrom(type) || !IsGameplayType(type))
                    continue;

                AddComponentMenu attribute = type.GetCustomAttribute<AddComponentMenu>();
                if (attribute != null && !string.IsNullOrWhiteSpace(attribute.componentMenu))
                {
                    string stableId = "reflection.add-component-menu." + ToStableSlug(type.Name);
                    if (seenStableIds.Add(stableId))
                    {
                        PyralisAuthoringFactKind kind = IsSceneComponent(type, attribute.componentMenu)
                            ? PyralisAuthoringFactKind.SceneComponent
                            : PyralisAuthoringFactKind.UnitySurface;

                        facts.Add(new PyralisAuthoringFact(
                            stableId,
                            AuthoringCapabilityRegistry.PrettifyTypeName(type.Name),
                            kind,
                            PyralisAuthoringFactSourceKind.Reflection,
                            PyralisAuthoringConfidence.Explicit,
                            $"Unity AddComponentMenu path for {type.Name}.",
                            $"Add {type.Name} through the Inspector when this route needs the component.",
                            string.Empty,
                            requiredSceneComponents: kind == PyralisAuthoringFactKind.SceneComponent ? new[] { type.Name } : null,
                            requiredUnitySurfaces: kind == PyralisAuthoringFactKind.UnitySurface ? new[] { type.Name } : null,
                            nativeActions: new[]
                            {
                                new PyralisAuthoringNativeAction(
                                    "Add Component",
                                    PyralisAuthoringActionSurface.Inspector,
                                    type.Name,
                                    attribute.componentMenu,
                                    type.Name + " is present on the selected scene object or prefab")
                            },
                            workIntent: "NativeComponentMenu",
                            relatedStableIds: BuildDependencyRelatedStableIds(type)));
                    }
                }

                AddRequireComponentFact(type, facts, seenStableIds);
            }

            foreach (Type type in TypeCache.GetTypesDerivedFrom<UnityEngine.Object>())
            {
                if (type == null || type.IsAbstract || !IsGameplayType(type))
                    continue;

                AddSerializedFieldFacts(type, facts, seenStableIds);
            }
        }

        private static IEnumerable<Type> GetGameplayObjectTypes()
        {
            HashSet<Type> seen = new HashSet<Type>();
            foreach (Type type in TypeCache.GetTypesDerivedFrom<UnityEngine.Object>())
            {
                if (type != null && typeof(UnityEngine.Object).IsAssignableFrom(type) && seen.Add(type))
                    yield return type;
            }

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly == null || assembly.IsDynamic)
                    continue;

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types;
                }

                if (types == null)
                    continue;

                for (int i = 0; i < types.Length; i++)
                {
                    Type type = types[i];
                    if (type != null && typeof(UnityEngine.Object).IsAssignableFrom(type) && seen.Add(type))
                        yield return type;
                }
            }
        }

        private static void AddRequireComponentFact(Type type, List<PyralisAuthoringFact> facts, HashSet<string> seenStableIds)
        {
            object[] attributes = type.GetCustomAttributes(typeof(RequireComponent), false);
            if (attributes == null || attributes.Length == 0)
                return;

            List<string> requiredComponents = new List<string>();
            for (int i = 0; i < attributes.Length; i++)
            {
                RequireComponent attribute = attributes[i] as RequireComponent;
                if (attribute == null)
                    continue;

                AddRequireComponentType(attribute, 0, requiredComponents);
                AddRequireComponentType(attribute, 1, requiredComponents);
                AddRequireComponentType(attribute, 2, requiredComponents);
            }

            if (requiredComponents.Count == 0)
                return;

            string stableId = "reflection.require-component." + ToStableSlug(type.Name);
            if (!seenStableIds.Add(stableId))
                return;

            facts.Add(new PyralisAuthoringFact(
                stableId,
                AuthoringCapabilityRegistry.PrettifyTypeName(type.Name) + " Requirements",
                PyralisAuthoringFactKind.UnitySurface,
                PyralisAuthoringFactSourceKind.Reflection,
                PyralisAuthoringConfidence.Explicit,
                $"{type.Name} declares required Unity components through RequireComponent metadata.",
                "Unity component requirements that shape Unity object composition.",
                string.Empty,
                requiredUnitySurfaces: requiredComponents.ToArray(),
                nativeActions: new[]
                {
                    new PyralisAuthoringNativeAction(
                        "Inspect",
                        PyralisAuthoringActionSurface.Inspector,
                        type.Name,
                        string.Join(", ", requiredComponents),
                        "Unity can satisfy or preserve the required component stack")
                },
                workIntent: "RequiredComponentContract",
                relatedStableIds: BuildDependencyRelatedStableIds(type)));
        }

        private static void AddSerializedFieldFacts(Type type, List<PyralisAuthoringFact> facts, HashSet<string> seenStableIds)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                bool serialized = (field.IsPublic && !field.IsInitOnly && field.GetCustomAttribute<NonSerializedAttribute>() == null)
                    || field.GetCustomAttribute<SerializeField>() != null;
                if (!serialized || field.GetCustomAttribute<HideInInspector>() != null)
                    continue;

                string stableId = "convention.serialized-field." + ToStableSlug(type.Name) + "." + ToStableSlug(field.Name);
                if (!seenStableIds.Add(stableId))
                    continue;

                string fieldDescription = type.Name + "." + field.Name + " -> " + field.FieldType.Name;
                facts.Add(new PyralisAuthoringFact(
                    stableId,
                    AuthoringCapabilityRegistry.PrettifyTypeName(type.Name) + " " + AuthoringCapabilityRegistry.PrettifyTypeName(field.Name),
                    PyralisAuthoringFactKind.AssignmentField,
                    PyralisAuthoringFactSourceKind.Convention,
                    PyralisAuthoringConfidence.ConventionDerived,
                    $"Serialized Inspector field discovered on {type.Name}.",
                    "Inspector field assignment generated from Unity serialization metadata.",
                    string.Empty,
                    assignmentFields: new[] { fieldDescription },
                    nativeActions: new[]
                    {
                        new PyralisAuthoringNativeAction(
                            "Assign",
                            PyralisAuthoringActionSurface.Inspector,
                            type.Name,
                            field.Name,
                            "the serialized Inspector field holds the user's authored value")
                    },
                    workIntent: "InspectorFieldConvention",
                    relatedStableIds: BuildDependencyRelatedStableIds(type, field)));
            }
        }

        public static PyralisAuthoringFact CreateFactFromContract(ResolvedAuthoringContract contract, Type type = null)
        {
            type = type ?? contract.SourceType;
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
                goalTags: BuildGoalTags(contract),
                requiredProfiles: GetRequiredProfiles(contract.RequiredProfileType),
                requiredUnitySurfaces: NormalizeTypeNames(contract.RequiredRuntimeInterfaceNames, contract.RequiredComponentNames),
                laneTags: ToStringArray(contract.SupportedPresentationModes),
                unsupportedLaneTags: ToStringArray(contract.UnsupportedPresentationModes),
                assignmentFields: MergeAndDeDuplicateFields(contract.AssignmentFields, discoveredAssignment),
                customizationMoments: MergeAndDeDuplicateFields(contract.CustomizationMoments, discoveredCustomization),
                nativeActions: BuildNativeActions(type, contract.NativeSetup),
                workIntent: contract.WorkIntent,
                axioms: contract.Axioms,
                relatedStableIds: BuildRelatedStableIds(contract.FirstProofTargetId, contract.SetupNodeId),
                capability: contract.Capability,
                priority: (AuthoringPriority)contract.Priority,
                priorityValueOverride: contract.PriorityValueOverride,
                deprecatedInVersion: contract.DeprecatedInVersion,
                removableInVersion: contract.RemovableInVersion,
                documentationURL: contract.DocumentationURL,
                expertAdvice: contract.ExpertAdvice);
        }

        private static string[] BuildGoalTags(ResolvedAuthoringContract contract)
        {
            List<string> tags = new List<string>();
            if (contract == null)
                return Array.Empty<string>();

            foreach (AuthoringCapability capability in AuthoringCapabilityRegistry.GetAllIndividualCapabilities())
            {
                if ((contract.Capability & capability) != 0)
                    AddGoalTag(tags, AuthoringCapabilityRegistry.GetDisplayName(capability));
            }

            AddGoalTag(tags, contract.AuthoringCategory);
            AddGoalTag(tags, contract.AuthoringLane);
            return tags.ToArray();
        }

        private static void AddGoalTag(List<string> tags, string value)
        {
            if (string.IsNullOrWhiteSpace(value) || tags.Contains(value))
                return;

            tags.Add(value);
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

        private static string[] NormalizeTypeNames(string[] first, string[] second)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            List<string> normalized = new List<string>();
            AddNames(first);
            AddNames(second);
            return normalized.ToArray();

            void AddNames(string[] source)
            {
                if (source == null)
                    return;

                for (int i = 0; i < source.Length; i++)
                {
                    string name = SimplifyTypeName(source[i]);
                    if (!string.IsNullOrWhiteSpace(name) && seen.Add(name))
                        normalized.Add(name);
                }
            }
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

        private static string[] BuildRelatedStableIds(string firstProofTargetId, string setupNodeId = null)
        {
            List<string> related = new List<string>();
            if (!string.IsNullOrWhiteSpace(firstProofTargetId))
                related.Add(firstProofTargetId);

            if (!string.IsNullOrWhiteSpace(setupNodeId) && !related.Contains(setupNodeId))
                related.Add(setupNodeId);

            return related.ToArray();
        }

        private static bool IsGameplayType(Type type)
        {
            return type != null
                && type.Namespace != null
                && type.Namespace.StartsWith("NeonBlack.Gameplay", StringComparison.Ordinal)
                && (typeof(Component).IsAssignableFrom(type) || typeof(ScriptableObject).IsAssignableFrom(type));
        }

        private static bool IsSceneComponent(Type type, string menuPath)
        {
            string value = ((type != null ? type.Name : string.Empty) + " " + (menuPath ?? string.Empty)).ToLowerInvariant();
            return value.Contains("bootstrap")
                || value.Contains("lifetime")
                || value.Contains("camera")
                || value.Contains("tabletop")
                || value.Contains("ui")
                || value.Contains("hud")
                || value.Contains("score")
                || value.Contains("spawner")
                || value.Contains("manager")
                || value.Contains("service")
                || value.Contains("scene")
                || value.Contains("game flow");
        }

        private static void AddRequireComponentType(RequireComponent attribute, int index, List<string> requiredComponents)
        {
            Type type = GetRequireComponentType(attribute, index);
            if (type == null)
                return;

            string typeName = type.Name;
            if (!requiredComponents.Contains(typeName))
                requiredComponents.Add(typeName);
        }

        private static string[] BuildDependencyRelatedStableIds(Type type, FieldInfo field = null)
        {
            List<string> related = new List<string>();

            AddRelatedContracts(type, related);
            if (field != null)
                AddRelatedContracts(GetAuthoringFieldType(field.FieldType), related);
            AddCoreSetupFallback(type, field, related);

            return related.Count > 0 ? related.ToArray() : Array.Empty<string>();
        }

        private static void AddRelatedContracts(Type type, List<string> related)
        {
            if (type == null)
                return;

            IReadOnlyList<ResolvedAuthoringContract> contracts = ResolvedAuthoringContractRegistry.All;
            for (int i = 0; i < contracts.Count; i++)
            {
                ResolvedAuthoringContract contract = contracts[i];
                if (contract == null || !ContractDependsOnType(contract, type))
                    continue;

                AddRelatedStableId(related, contract.StableId);
                AddRelatedStableId(related, contract.SetupNodeId);
                AddRelatedStableId(related, contract.FirstProofTargetId);
                AddCapabilityFactStableIds(contract, related);
            }
        }

        private static void AddCapabilityFactStableIds(ResolvedAuthoringContract contract, List<string> related)
        {
            if (contract == null || contract.Capability == AuthoringCapability.None)
                return;

            RuntimeCapabilityFamily[] families = PyralisAuthoringCapabilityDescriptorRegistry.BuildRuntimeFamilies(
                contract.Capability,
                RuntimeCapabilityLaneTag.Mixed,
                contract.Axioms);
            for (int i = 0; i < families.Length; i++)
            {
                PyralisAuthoringCapabilityDescriptor descriptor = PyralisAuthoringCapabilityDescriptorRegistry.FindPrimaryByFamily(families[i]);
                if (descriptor != null)
                    AddRelatedStableId(related, descriptor.StableId);
            }
        }

        private static bool ContractDependsOnType(ResolvedAuthoringContract contract, Type type)
        {
            if (contract.SourceType == type || contract.RequiredProfileType == type)
                return true;

            string fullName = type.FullName;
            if (string.IsNullOrWhiteSpace(fullName))
                return false;

            return ContainsTypeName(contract.RequiredRuntimeInterfaceNames, fullName);
        }

        private static bool ContainsTypeName(string[] names, string fullName)
        {
            if (names == null)
                return false;

            for (int i = 0; i < names.Length; i++)
            {
                if (string.Equals(names[i], fullName, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static Type GetAuthoringFieldType(Type fieldType)
        {
            if (fieldType == null)
                return null;

            if (fieldType.IsArray)
                return fieldType.GetElementType();

            if (fieldType.IsGenericType)
            {
                Type[] arguments = fieldType.GetGenericArguments();
                if (arguments != null && arguments.Length == 1)
                    return arguments[0];
            }

            return fieldType;
        }

        private static void AddCoreSetupFallback(Type type, FieldInfo field, List<string> related)
        {
            if (type == null)
                return;

            AddKnownSetupType(type, related);
            if (field != null)
                AddKnownSetupType(GetAuthoringFieldType(field.FieldType), related);
        }

        private static void AddKnownSetupType(Type type, List<string> related)
        {
            if (type == null)
                return;

            string name = type.Name;
            if (name == "SessionDefinition")
                AddRelatedStableId(related, "setup.assign-session-definition");
            else if (name == "GameModeDefinition")
                AddRelatedStableId(related, "setup.assign-game-mode");
            else if (name == "GameSetupProfile")
                AddRelatedStableId(related, "setup.assign-setup-profile");
            else if (name == "ParticipantDefinition")
                AddRelatedStableId(related, "setup.assign-default-participants");
            else if (name == "InputProfile")
                AddRelatedStableId(related, "inspector.input-profile.gameplay-action-names");
        }

        private static void AddRelatedStableId(List<string> related, string stableId)
        {
            if (string.IsNullOrWhiteSpace(stableId) || related.Contains(stableId))
                return;

            related.Add(stableId);
        }

        private static string ToStableSlug(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "unnamed";

            List<char> chars = new List<char>();
            char previous = '\0';
            for (int i = 0; i < value.Length; i++)
            {
                char current = value[i];
                if (current == '_')
                {
                    AppendDash(chars);
                    previous = current;
                    continue;
                }

                if (char.IsDigit(current) && i > 0 && char.IsLetter(previous) && previous != '-' && previous != '_')
                    AppendDash(chars);
                else if (char.IsUpper(current) && i > 0 && previous != '-' && previous != '_')
                {
                    bool startsNewWord = !char.IsUpper(previous) && !char.IsDigit(previous);
                    bool closesAcronym = char.IsUpper(previous)
                        && i + 1 < value.Length
                        && char.IsLower(value[i + 1]);
                    if (startsNewWord || closesAcronym)
                        AppendDash(chars);
                }

                if (char.IsLetterOrDigit(current))
                    chars.Add(char.ToLowerInvariant(current));
                else
                    AppendDash(chars);

                previous = current;
            }

            while (chars.Count > 0 && chars[chars.Count - 1] == '-')
                chars.RemoveAt(chars.Count - 1);

            return chars.Count > 0 ? new string(chars.ToArray()) : "unnamed";
        }

        private static void AppendDash(List<char> chars)
        {
            if (chars.Count == 0 || chars[chars.Count - 1] == '-')
                return;

            chars.Add('-');
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

    }
}
