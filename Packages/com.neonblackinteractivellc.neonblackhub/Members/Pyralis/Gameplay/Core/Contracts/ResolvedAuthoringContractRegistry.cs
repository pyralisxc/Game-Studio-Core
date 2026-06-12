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

            var providerTypes = TypeCache.GetTypesDerivedFrom<IAuthoringContractProvider>();
            foreach (var type in providerTypes)
            {
                if (type.IsAbstract || type.IsInterface)
                    continue;

                ProcessProvider(type, AddContract);
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

                    if (typeof(IAuthoringContractProvider).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                    {
                        ProcessProvider(type, AddContract);
                    }
                }
            }
#endif

            return contracts;
        }

        private static void ProcessProvider(Type type, Action<ResolvedAuthoringContract> addContract)
        {
            try
            {
            var provider = Activator.CreateInstance(type) as IAuthoringContractProvider;
                if (provider == null) return;

                foreach (var attr in provider.GetAuthoringContracts())
                {
                    if (attr == null)
                        continue;

                    addContract(CreateFromAttribute(type, attr));
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to create authoring contract provider instance for {type.Name}: {ex.Message}");
            }
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

            string stableId = !string.IsNullOrWhiteSpace(attr.ModuleId)
                ? $"feature.{attr.ModuleId}"
                : $"feature.{type.FullName}";
            string firstProofTargetId = ResolveFirstProofTargetId(type, attr);
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
                firstProofTargetId: firstProofTargetId,
                sourceType: type,
                axioms: attr.Axioms,
                workIntent: attr.AxiomKeywords,
                confidence: PyralisAuthoringConfidence.Explicit,
                assignmentFields: assignmentFields,
                customizationMoments: customizationMoments,
                requiredComponentNames: componentNames.ToArray(),
                capability: attr.Capability,
                priority: attr.Priority,
                documentationURL: attr.DocumentationURL,
                expertAdvice: attr.ExpertAdvice,
                moduleId: attr.ModuleId,
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

        private static string ResolveFirstProofTargetId(Type type, AuthoringContractAttribute attr)
        {
            if (!string.IsNullOrWhiteSpace(attr.FirstProof) &&
                attr.FirstProof.StartsWith("proof.", StringComparison.Ordinal))
            {
                return attr.FirstProof;
            }

            string moduleId = attr.ModuleId ?? string.Empty;
            string lookup = (moduleId + " " + (type != null ? type.FullName : string.Empty)).ToLowerInvariant();
            if (lookup.Contains("traversal.topdown-hop") ||
                lookup.Contains("movement") ||
                lookup.Contains("motor") ||
                lookup.Contains("pawn"))
            {
                return "proof.1p-pawn-movement";
            }

            if (lookup.Contains("pickup") ||
                lookup.Contains("collectible") ||
                lookup.Contains("status"))
            {
                return "proof.custom-object-effect";
            }

            if (lookup.Contains("interaction"))
                return "proof.action-selection";

            if (lookup.Contains("feedback") ||
                lookup.Contains("hud") ||
                lookup.Contains("ui"))
            {
                return "proof.ui-hud-menu";
            }

            if (lookup.Contains("enemy") ||
                lookup.Contains("traversal.3d") ||
                lookup.Contains("combat") ||
                lookup.Contains("projectile") ||
                lookup.Contains("health") ||
                lookup.Contains("hitbox") ||
                lookup.Contains("damage") ||
                lookup.Contains("guard"))
            {
                return "proof.npc-enemy-behavior";
            }

            if ((attr.Capability & AuthoringCapability.Tabletop) != 0)
                return "proof.board-card-action";

            if ((attr.Capability & AuthoringCapability.Networking) != 0)
                return "proof.network-ownership";

            if ((attr.Capability & AuthoringCapability.Camera) != 0)
                return "proof.camera-cursor-world";

            if ((attr.Capability & AuthoringCapability.UI) != 0 ||
                (attr.Capability & AuthoringCapability.Scoring) != 0 ||
                (attr.Capability & AuthoringCapability.Dialogue) != 0 ||
                (attr.Capability & AuthoringCapability.Quests) != 0 ||
                (attr.Capability & AuthoringCapability.Vendors) != 0 ||
                (attr.Capability & AuthoringCapability.SkillTree) != 0)
            {
                return "proof.ui-hud-menu";
            }

            if ((attr.Capability & AuthoringCapability.Puzzle) != 0 ||
                (attr.Capability & AuthoringCapability.Input) != 0)
            {
                return "proof.action-selection";
            }

            if ((attr.Capability & AuthoringCapability.Inventory) != 0 ||
                (attr.Capability & AuthoringCapability.Stats) != 0 ||
                (attr.Capability & AuthoringCapability.Progression) != 0 ||
                (attr.Capability & AuthoringCapability.Rpg) != 0)
            {
                return "proof.custom-object-effect";
            }

            if ((attr.Capability & AuthoringCapability.Setup) != 0 ||
                (attr.Capability & AuthoringCapability.Session) != 0 ||
                (attr.Capability & AuthoringCapability.Rules) != 0 ||
                (attr.Capability & AuthoringCapability.Participants) != 0)
            {
                return "proof.custom-object-effect";
            }

            if ((attr.Capability & AuthoringCapability.Combat) != 0)
                return "proof.npc-enemy-behavior";

            if ((attr.Capability & AuthoringCapability.CombatState) != 0 ||
                (attr.Capability & AuthoringCapability.CombatSensors) != 0 ||
                (attr.Capability & AuthoringCapability.MeleeFlow) != 0 ||
                (attr.Capability & AuthoringCapability.RangedFlow) != 0 ||
                (attr.Capability & AuthoringCapability.TacticsAggressive) != 0 ||
                (attr.Capability & AuthoringCapability.TacticsDefensive) != 0 ||
                (attr.Capability & AuthoringCapability.Steering2D) != 0 ||
                (attr.Capability & AuthoringCapability.Steering3D) != 0)
            {
                return "proof.npc-enemy-behavior";
            }

            if ((attr.Capability & AuthoringCapability.Animation) != 0 ||
                (attr.Capability & AuthoringCapability.VFX) != 0 ||
                (attr.Capability & AuthoringCapability.Audio) != 0)
            {
                return "proof.ui-hud-menu";
            }

            if ((attr.Capability & AuthoringCapability.Movement) != 0 ||
                (attr.Capability & AuthoringCapability.KineticMotor2D) != 0 ||
                (attr.Capability & AuthoringCapability.KineticMotor3D) != 0 ||
                (attr.Capability & AuthoringCapability.Traversal) != 0)
            {
                return "proof.1p-pawn-movement";
            }

            return "proof.custom-object-effect";
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
                relevance: FirstNonEmpty(current.Relevance, incoming.Relevance),
                manualPath: FirstNonEmpty(current.ManualPath, incoming.ManualPath));
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
