using System;
using System.Collections.Generic;
using System.Linq;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    public sealed class PyralisAuthoringCapabilityDescriptor
    {
        public PyralisAuthoringCapabilityDescriptor(
            string stableId,
            string displayName,
            RuntimeCapabilityFamily family,
            AuthoringCapability capability,
            string group,
            int sortOrder,
            string summary,
            string routeRelevance,
            string proofTargetId,
            string[] goalTags,
            string[] laneTags,
            string[] unsupportedLaneTags,
            AuthoringWorldAxiom axioms,
            string[] requiredSetup,
            string[] assignmentFields,
            string[] customizationMoments,
            PyralisAuthoringNativeAction[] nativeActions,
            PyralisAuthoringGraphSourceOrigin sourceOrigin,
            string capabilityPath = null,
            AuthoringContractSurface surface = AuthoringContractSurface.Auto,
            string[] roleTags = null,
            bool selectableIntent = true,
            PyralisAuthoringFact sourceFact = null)
        {
            StableId = stableId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            Family = family;
            Capability = capability;
            Group = group ?? "General";
            SortOrder = sortOrder;
            Summary = summary ?? string.Empty;
            RouteRelevance = routeRelevance ?? string.Empty;
            ProofTargetId = proofTargetId ?? string.Empty;
            GoalTags = goalTags ?? Array.Empty<string>();
            LaneTags = laneTags ?? Array.Empty<string>();
            UnsupportedLaneTags = unsupportedLaneTags ?? Array.Empty<string>();
            Axioms = axioms;
            RequiredSetup = requiredSetup ?? Array.Empty<string>();
            AssignmentFields = assignmentFields ?? Array.Empty<string>();
            CustomizationMoments = customizationMoments ?? Array.Empty<string>();
            NativeActions = nativeActions ?? Array.Empty<PyralisAuthoringNativeAction>();
            SourceOrigin = sourceOrigin;
            CapabilityPath = capabilityPath ?? string.Empty;
            Surface = surface;
            RoleTags = roleTags ?? Array.Empty<string>();
            SelectableIntent = selectableIntent;
            SourceFact = sourceFact;
        }

        public string StableId { get; }
        public string DisplayName { get; }
        public RuntimeCapabilityFamily Family { get; }
        public AuthoringCapability Capability { get; }
        public string Group { get; }
        public int SortOrder { get; }
        public string Summary { get; }
        public string RouteRelevance { get; }
        public string ProofTargetId { get; }
        public string[] GoalTags { get; }
        public string[] LaneTags { get; }
        public string[] UnsupportedLaneTags { get; }
        public AuthoringWorldAxiom Axioms { get; }
        public string[] RequiredSetup { get; }
        public string[] AssignmentFields { get; }
        public string[] CustomizationMoments { get; }
        public PyralisAuthoringNativeAction[] NativeActions { get; }
        public PyralisAuthoringGraphSourceOrigin SourceOrigin { get; }
        public string CapabilityPath { get; }
        public AuthoringContractSurface Surface { get; }
        public string[] RoleTags { get; }
        public bool SelectableIntent { get; }
        public PyralisAuthoringFact SourceFact { get; }
        public bool HasSemanticCapabilityPath => !string.IsNullOrWhiteSpace(CapabilityPath);
        public bool IsContractSemanticSource => SourceOrigin == PyralisAuthoringGraphSourceOrigin.Contract
            || SourceOrigin == PyralisAuthoringGraphSourceOrigin.Reflection;

        public bool Matches(AuthoringCapability capabilities, RuntimeCapabilityLaneTag lane, AuthoringWorldAxiom axioms)
        {
            if (capabilities != AuthoringCapability.None && (Capability & capabilities) == 0)
                return false;

            if (lane != RuntimeCapabilityLaneTag.Mixed)
            {
                string laneName = lane.ToString();
                if (Contains(UnsupportedLaneTags, laneName))
                    return false;

                if (LaneTags.Length > 0 && !Contains(LaneTags, laneName) && !Contains(LaneTags, ToPresentationLaneName(lane)))
                    return false;
            }

            return Axioms == AuthoringWorldAxiom.None
                || axioms == AuthoringWorldAxiom.None
                || (Axioms & axioms) != 0;
        }

        private static bool Contains(string[] values, string expected)
        {
            if (values == null || string.IsNullOrWhiteSpace(expected))
                return false;

            for (int i = 0; i < values.Length; i++)
            {
                string value = values[i];
                if (string.Equals(value, expected, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static string ToPresentationLaneName(RuntimeCapabilityLaneTag lane)
        {
            return lane switch
            {
                RuntimeCapabilityLaneTag.Sprite2D => "Sprite2D",
                RuntimeCapabilityLaneTag.Billboard2_5D => "Billboard2_5D",
                RuntimeCapabilityLaneTag.ThirdPerson3D => "Rigged3D",
                _ => lane.ToString()
            };
        }
    }

    public static class PyralisAuthoringCapabilityDescriptorRegistry
    {
        private static readonly Lazy<IReadOnlyList<PyralisAuthoringCapabilityDescriptor>> _allDescriptors =
            new Lazy<IReadOnlyList<PyralisAuthoringCapabilityDescriptor>>(BuildDescriptors);

        public static IReadOnlyList<PyralisAuthoringCapabilityDescriptor> All
        {
            get
            {
                return _allDescriptors.Value;
            }
        }

        public static RuntimeCapabilityFamily[] BuildRuntimeFamilies(
            AuthoringCapability capabilities,
            RuntimeCapabilityLaneTag lane,
            AuthoringWorldAxiom axioms)
        {
            if (capabilities == AuthoringCapability.None)
                return Array.Empty<RuntimeCapabilityFamily>();

            List<RuntimeCapabilityFamily> families = new List<RuntimeCapabilityFamily>();
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors = All;
            for (int i = 0; i < descriptors.Count; i++)
            {
                PyralisAuthoringCapabilityDescriptor descriptor = descriptors[i];
                if (descriptor != null
                    && descriptor.IsContractSemanticSource
                    && descriptor.Family != RuntimeCapabilityFamily.Custom
                    && IsCapabilitySatisfiedBySelection(descriptor.Capability, capabilities)
                    && descriptor.Matches(capabilities, lane, axioms))
                {
                    AddDistinct(families, descriptor.Family);
                }
            }

            return families.ToArray();
        }

        public static RuntimeCapabilityFamily[] BuildRuntimeFamiliesForDescriptors(
            IReadOnlyList<string> descriptorIds,
            RuntimeCapabilityLaneTag lane,
            AuthoringWorldAxiom axioms)
        {
            if (descriptorIds == null || descriptorIds.Count == 0)
                return Array.Empty<RuntimeCapabilityFamily>();

            HashSet<string> selected = new HashSet<string>(descriptorIds, StringComparer.Ordinal);
            List<RuntimeCapabilityFamily> families = new List<RuntimeCapabilityFamily>();
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors = All;
            for (int i = 0; i < descriptors.Count; i++)
            {
                PyralisAuthoringCapabilityDescriptor descriptor = descriptors[i];
                if (descriptor == null || !selected.Contains(descriptor.StableId))
                    continue;

                if (!IsGameplayIngredientDescriptor(descriptor))
                    continue;

                if (descriptor.Family != RuntimeCapabilityFamily.Custom
                    && descriptor.Matches(descriptor.Capability, lane, axioms))
                    AddDistinct(families, descriptor.Family);
            }

            return families.ToArray();
        }

        public static AuthoringCapability BuildCapabilitiesForDescriptors(IReadOnlyList<string> descriptorIds)
        {
            if (descriptorIds == null || descriptorIds.Count == 0)
                return AuthoringCapability.None;

            HashSet<string> selected = new HashSet<string>(descriptorIds, StringComparer.Ordinal);
            AuthoringCapability capabilities = AuthoringCapability.None;
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors = All;
            for (int i = 0; i < descriptors.Count; i++)
            {
                PyralisAuthoringCapabilityDescriptor descriptor = descriptors[i];
                if (descriptor != null
                    && selected.Contains(descriptor.StableId)
                    && IsGameplayIngredientDescriptor(descriptor))
                {
                    capabilities |= descriptor.Capability;
                }
            }

            return capabilities;
        }

        public static string[] FilterGameplayIntentDescriptorIds(IReadOnlyList<string> descriptorIds)
        {
            if (descriptorIds == null || descriptorIds.Count == 0)
                return Array.Empty<string>();

            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            List<string> filtered = new List<string>();
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors = All;
            for (int i = 0; i < descriptorIds.Count; i++)
            {
                string descriptorId = descriptorIds[i];
                if (string.IsNullOrWhiteSpace(descriptorId) || !seen.Add(descriptorId))
                    continue;

                PyralisAuthoringCapabilityDescriptor descriptor = descriptors.FirstOrDefault(candidate =>
                    candidate != null && string.Equals(candidate.StableId, descriptorId, StringComparison.Ordinal));
                if (descriptor == null || !IsGameplayIngredientDescriptor(descriptor))
                    continue;

                filtered.Add(descriptorId);
            }

            filtered.Sort(StringComparer.Ordinal);
            return filtered.ToArray();
        }

        public static string[] FilterGameplayIntentDescriptorIds(
            IReadOnlyList<string> descriptorIds,
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> allowedDescriptors)
        {
            string[] filtered = FilterGameplayIntentDescriptorIds(descriptorIds);
            if (filtered.Length == 0 || allowedDescriptors == null)
                return filtered;

            HashSet<string> allowed = new HashSet<string>(
                allowedDescriptors
                    .Where(IsGameplayIngredientDescriptor)
                    .Select(descriptor => descriptor.StableId),
                StringComparer.Ordinal);
            if (allowed.Count == 0)
                return Array.Empty<string>();

            return filtered
                .Where(allowed.Contains)
                .ToArray();
        }

        public static bool IsIntentRouteEssential(PyralisAuthoringCapabilityDescriptor descriptor)
        {
            if (descriptor == null)
                return false;

            return HasExactRoleTag(descriptor, AuthoringContractRoleTags.IntentRouteEssential);
        }

        public static bool IsIntentRouteEssentialExpected(
            PyralisAuthoringCapabilityDescriptor descriptor,
            PyralisAuthoringIntentSelection selection)
        {
            if (!IsIntentRouteEssential(descriptor))
                return false;

            selection ??= new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.None,
                AuthoringWorldAxiom.None);

            AuthoringCapability selectedCapabilities = selection.Capabilities;
            bool routeIsShaped =
                selectedCapabilities != AuthoringCapability.None
                || selection.Axioms != AuthoringWorldAxiom.None
                || selection.ParticipantRoute != PyralisIntentParticipantRoute.InferFromSetup;
            if (!routeIsShaped)
                return false;

            bool networkRoute = selection.ParticipantRoute == PyralisIntentParticipantRoute.Networked
                || selection.ParticipantRoute == PyralisIntentParticipantRoute.HybridLocalNetworked;
            if (HasExactRoleTag(descriptor, AuthoringContractRoleTags.NetworkRouteSupport))
                return networkRoute;

            if (HasExactRoleTag(descriptor, AuthoringContractRoleTags.CoreRouteAnchor))
                return true;

            RuntimeCapabilityFamily[] selectedFamilies =
                BuildSelectedRuntimeFamilies(selection, selectedCapabilities);
            bool participantRoute = selection.ParticipantRoute != PyralisIntentParticipantRoute.InferFromSetup
                || ContainsFamily(selectedFamilies, RuntimeCapabilityFamily.CharacterPawnGameplay)
                || ContainsFamily(selectedFamilies, RuntimeCapabilityFamily.PlatformCore);
            if (participantRoute && HasExactRoleTag(descriptor, AuthoringContractRoleTags.ParticipantRouteSupport))
                return true;

            bool inputRoute = ContainsFamily(selectedFamilies, RuntimeCapabilityFamily.ActionTargeting)
                || selection.ParticipantRoute == PyralisIntentParticipantRoute.SoloLocal
                || selection.ParticipantRoute == PyralisIntentParticipantRoute.TwoLocalPlayers
                || selection.ParticipantRoute == PyralisIntentParticipantRoute.ThreeLocalPlayers
                || selection.ParticipantRoute == PyralisIntentParticipantRoute.FourLocalPlayers;
            if (inputRoute && HasExactRoleTag(descriptor, AuthoringContractRoleTags.InputRouteSupport))
                return true;

            if (ContainsFamily(selectedFamilies, RuntimeCapabilityFamily.AnimationPresentation)
                && HasExactRoleTag(descriptor, AuthoringContractRoleTags.AnimationDefinitionRouteSupport))
                return true;

            if ((ContainsFamily(selectedFamilies, RuntimeCapabilityFamily.Combat)
                    || ContainsFamily(selectedFamilies, RuntimeCapabilityFamily.GunsProjectiles))
                && HasExactRoleTag(descriptor, AuthoringContractRoleTags.CombatDefinitionRouteSupport))
                return true;

            bool moduleCapabilityRoute =
                ContainsFamily(selectedFamilies, RuntimeCapabilityFamily.CharacterPawnGameplay)
                || ContainsFamily(selectedFamilies, RuntimeCapabilityFamily.Combat)
                || ContainsFamily(selectedFamilies, RuntimeCapabilityFamily.GunsProjectiles)
                || ContainsFamily(selectedFamilies, RuntimeCapabilityFamily.ActionTargeting)
                || ContainsFamily(selectedFamilies, RuntimeCapabilityFamily.BoardCardTabletop);
            if (!moduleCapabilityRoute || !HasExactRoleTag(descriptor, AuthoringContractRoleTags.ModuleCapabilityRouteSupport))
                return false;

            return descriptor.Family == RuntimeCapabilityFamily.Custom
                || ContainsFamily(selectedFamilies, descriptor.Family);
        }

        private static RuntimeCapabilityFamily[] BuildSelectedRuntimeFamilies(
            PyralisAuthoringIntentSelection selection,
            AuthoringCapability selectedCapabilities)
        {
            RuntimeCapabilityFamily[] descriptorFamilies =
                BuildRuntimeFamiliesForDescriptors(
                    selection?.DescriptorIds,
                    selection?.Lane ?? RuntimeCapabilityLaneTag.Sprite2D,
                    selection?.Axioms ?? AuthoringWorldAxiom.None);
            if (descriptorFamilies.Length > 0)
                return descriptorFamilies;

            return BuildRuntimeFamilies(
                selectedCapabilities,
                selection?.Lane ?? RuntimeCapabilityLaneTag.Sprite2D,
                selection?.Axioms ?? AuthoringWorldAxiom.None);
        }

        public static IReadOnlyList<PyralisAuthoringCapabilityDescriptor> BuildIntentDescriptors(
            RuntimeCapabilityLaneTag lane,
            AuthoringWorldAxiom axioms)
        {
            return All
                .Where(descriptor => descriptor != null
                    && descriptor.Capability != AuthoringCapability.None
                    && IsIntentSurfaceDescriptor(descriptor)
                    && descriptor.Matches(descriptor.Capability, lane, axioms))
                .OrderBy(descriptor => descriptor.Group, StringComparer.Ordinal)
                .ThenBy(descriptor => descriptor.SortOrder)
                .ThenBy(descriptor => descriptor.DisplayName, StringComparer.Ordinal)
                .ToArray();
        }

        public static IReadOnlyList<PyralisAuthoringCapabilityDescriptor> BuildIntentProjectionDescriptors(
            RuntimeCapabilityLaneTag lane,
            AuthoringWorldAxiom axioms)
        {
            return All
                .Where(descriptor => descriptor != null
                    && descriptor.Capability != AuthoringCapability.None
                    && (IsIntentSurfaceDescriptor(descriptor) || IsIntentMetadataBacklogDescriptor(descriptor))
                    && descriptor.Matches(descriptor.Capability, lane, axioms))
                .OrderBy(descriptor => descriptor.Group, StringComparer.Ordinal)
                .ThenBy(descriptor => descriptor.SortOrder)
                .ThenBy(descriptor => descriptor.DisplayName, StringComparer.Ordinal)
                .ToArray();
        }

        public static bool IsIntentSurfaceDescriptor(PyralisAuthoringCapabilityDescriptor descriptor)
        {
            if (descriptor == null)
                return false;

            return IsIntentRouteEssential(descriptor)
                || IsGameplayIngredientDescriptor(descriptor);
        }

        public static bool IsGameplayIngredientDescriptor(PyralisAuthoringCapabilityDescriptor descriptor)
        {
            if (descriptor == null)
                return false;

            return descriptor.IsContractSemanticSource
                && descriptor.SelectableIntent
                && descriptor.Surface == AuthoringContractSurface.GameplayIngredient
                && descriptor.HasSemanticCapabilityPath
                && !IsIntentRouteEssential(descriptor);
        }

        public static bool IsIntentMetadataBacklogDescriptor(PyralisAuthoringCapabilityDescriptor descriptor)
        {
            if (descriptor == null)
                return false;

            return descriptor.IsContractSemanticSource
                && descriptor.SelectableIntent
                && descriptor.Surface == AuthoringContractSurface.GameplayIngredient
                && !IsIntentRouteEssential(descriptor)
                && (!descriptor.HasSemanticCapabilityPath
                    || descriptor.Family == RuntimeCapabilityFamily.Custom);
        }

        public static bool RequiresGameplayIngredientCapabilityPath(ResolvedAuthoringContract contract)
        {
            return contract != null
                && contract.Capability != AuthoringCapability.None
                && contract.SelectableIntent
                && contract.Surface == AuthoringContractSurface.GameplayIngredient
                && IsIntentSelectableContract(contract)
                && !HasExactRoleTag(contract.RoleTags, AuthoringContractRoleTags.IntentRouteEssential);
        }

        private static bool ContainsFamily(RuntimeCapabilityFamily[] families, RuntimeCapabilityFamily expected)
        {
            if (families == null)
                return false;

            for (int i = 0; i < families.Length; i++)
            {
                if (families[i] == expected)
                    return true;
            }

            return false;
        }

        private static bool HasExactRoleTag(PyralisAuthoringCapabilityDescriptor descriptor, string expected)
        {
            if (descriptor?.RoleTags == null || string.IsNullOrWhiteSpace(expected))
                return false;

            for (int i = 0; i < descriptor.RoleTags.Length; i++)
            {
                if (string.Equals(descriptor.RoleTags[i], expected, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static bool HasExactRoleTag(string[] roleTags, string expected)
        {
            if (roleTags == null || string.IsNullOrWhiteSpace(expected))
                return false;

            for (int i = 0; i < roleTags.Length; i++)
            {
                if (string.Equals(roleTags[i], expected, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        public static PyralisAuthoringCapabilityDescriptor FindPrimaryByFamily(RuntimeCapabilityFamily family)
        {
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors = All;
            for (int i = 0; i < descriptors.Count; i++)
            {
                PyralisAuthoringCapabilityDescriptor descriptor = descriptors[i];
                if (descriptor == null || descriptor.Family != family)
                    continue;

                if (descriptor.SourceOrigin == PyralisAuthoringGraphSourceOrigin.Contract
                    || descriptor.SourceOrigin == PyralisAuthoringGraphSourceOrigin.Reflection)
                {
                    return descriptor;
                }
            }

            return null;
        }

        public static PyralisAuthoringCapabilityDescriptor FindBestForCapability(
            AuthoringCapability capability,
            RuntimeCapabilityLaneTag lane,
            AuthoringWorldAxiom axioms)
        {
            if (capability == AuthoringCapability.None)
                return null;

            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors = All;
            PyralisAuthoringCapabilityDescriptor best = null;
            int bestScore = int.MinValue;
            for (int i = 0; i < descriptors.Count; i++)
            {
                PyralisAuthoringCapabilityDescriptor descriptor = descriptors[i];
                if (descriptor == null || !descriptor.IsContractSemanticSource || (descriptor.Capability & capability) == 0)
                    continue;

                if (!descriptor.Matches(capability, lane, axioms))
                    continue;

                int score = GetCapabilityDescriptorScore(descriptor, capability, lane, axioms);
                if (score > bestScore)
                {
                    best = descriptor;
                    bestScore = score;
                }
            }

            return best;
        }

        private static int GetCapabilityDescriptorScore(
            PyralisAuthoringCapabilityDescriptor descriptor,
            AuthoringCapability capability,
            RuntimeCapabilityLaneTag lane,
            AuthoringWorldAxiom axioms)
        {
            int score = 0;
            if (descriptor.Capability == capability)
                score += 1000;
            else
                score += Math.Max(0, 200 - CountIndividualCapabilities(descriptor.Capability));

            if (IsContractOwned(descriptor))
                score += 200;

            if (lane != RuntimeCapabilityLaneTag.Mixed
                && (Contains(descriptor.LaneTags, lane.ToString())
                    || Contains(descriptor.LaneTags, ToRegistryPresentationLaneName(lane))))
            {
                score += 50;
            }

            if (axioms != AuthoringWorldAxiom.None && descriptor.Axioms != AuthoringWorldAxiom.None)
                score += CountMatchingAxioms(descriptor.Axioms, axioms);

            score -= descriptor.SortOrder;
            return score;
        }

        private static int CountIndividualCapabilities(AuthoringCapability capabilities)
        {
            int count = 0;
            foreach (AuthoringCapability capability in AuthoringCapabilityRegistry.GetAllIndividualCapabilities())
            {
                if ((capabilities & capability) != 0)
                    count++;
            }

            return count;
        }

        private static int CountMatchingAxioms(AuthoringWorldAxiom descriptorAxioms, AuthoringWorldAxiom selectedAxioms)
        {
            int count = 0;
            AuthoringWorldAxiom overlap = descriptorAxioms & selectedAxioms;
            IReadOnlyList<PyralisAuthoringAxiomGroup> groups = PyralisAuthoringVocabulary.GetAxiomGroups();
            for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++)
            {
                AuthoringWorldAxiom[] options = groups[groupIndex].Options;
                for (int optionIndex = 0; optionIndex < options.Length; optionIndex++)
                {
                    if ((overlap & options[optionIndex]) != 0)
                        count++;
                }
            }

            return count;
        }

        private static bool IsCapabilitySatisfiedBySelection(AuthoringCapability descriptorCapability, AuthoringCapability selectedCapabilities)
        {
            if (descriptorCapability == AuthoringCapability.None)
                return false;

            return (descriptorCapability & selectedCapabilities) == descriptorCapability;
        }

        private static string ToRegistryPresentationLaneName(RuntimeCapabilityLaneTag lane)
        {
            return lane switch
            {
                RuntimeCapabilityLaneTag.Sprite2D => "Sprite2D",
                RuntimeCapabilityLaneTag.Billboard2_5D => "Billboard2_5D",
                RuntimeCapabilityLaneTag.ThirdPerson3D => "Rigged3D",
                _ => lane.ToString()
            };
        }

        public static bool CapabilityMatchesFamily(AuthoringCapability capability, RuntimeCapabilityFamily family)
        {
            if (capability == AuthoringCapability.None)
                return false;

            RuntimeCapabilityFamily[] families = BuildRuntimeFamilies(
                capability,
                RuntimeCapabilityLaneTag.Mixed,
                AuthoringWorldAxiom.None);
            for (int i = 0; i < families.Length; i++)
            {
                if (families[i] == family)
                    return true;
            }

            return false;
        }

        public static IReadOnlyList<PyralisAuthoringFact> BuildFactsForCapability(AuthoringCapability capability)
        {
            if (capability == AuthoringCapability.None)
                return Array.Empty<PyralisAuthoringFact>();

            return All
                .Where(descriptor => descriptor != null && (descriptor.Capability & capability) != 0)
                .Select(BuildFact)
                .Where(fact => fact != null)
                .ToArray();
        }

        public static IReadOnlyList<PyralisAuthoringFact> BuildFactsForLane(RuntimeCapabilityLaneTag lane)
        {
            string laneName = lane.ToString();
            return All
                .Where(descriptor => descriptor != null
                    && (Contains(descriptor.LaneTags, laneName)
                        || Contains(descriptor.UnsupportedLaneTags, laneName)))
                .Select(BuildFact)
                .Where(fact => fact != null)
                .ToArray();
        }

        public static PyralisAuthoringFact BuildFact(PyralisAuthoringCapabilityDescriptor descriptor)
        {
            if (descriptor == null)
                return null;

            return descriptor.SourceFact ?? new PyralisAuthoringFact(
                descriptor.StableId,
                descriptor.DisplayName,
                PyralisAuthoringFactKind.RuntimeCapability,
                descriptor.SourceOrigin == PyralisAuthoringGraphSourceOrigin.Contract
                    ? PyralisAuthoringFactSourceKind.FeatureContract
                    : PyralisAuthoringFactSourceKind.Convention,
                descriptor.SourceOrigin == PyralisAuthoringGraphSourceOrigin.Contract
                    ? PyralisAuthoringConfidence.Explicit
                    : PyralisAuthoringConfidence.ConventionDerived,
                descriptor.Summary,
                descriptor.RouteRelevance,
                string.Empty,
                descriptor.GoalTags,
                descriptor.LaneTags,
                descriptor.UnsupportedLaneTags,
                requiredUnitySurfaces: descriptor.RequiredSetup,
                assignmentFields: descriptor.AssignmentFields,
                customizationMoments: descriptor.CustomizationMoments,
                nativeActions: descriptor.NativeActions,
                relatedStableIds: string.IsNullOrWhiteSpace(descriptor.ProofTargetId)
                    ? Array.Empty<string>()
                    : new[] { descriptor.ProofTargetId },
                axioms: descriptor.Axioms,
                capability: descriptor.Capability,
                priority: AuthoringPriority.Primary);
        }

        private static bool Contains(string[] values, string expected)
        {
            if (values == null || string.IsNullOrWhiteSpace(expected))
                return false;

            for (int i = 0; i < values.Length; i++)
            {
                if (string.Equals(values[i], expected, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool Contains(string value, string expected)
        {
            return !string.IsNullOrWhiteSpace(value)
                && !string.IsNullOrWhiteSpace(expected)
                && value.IndexOf(expected, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static IReadOnlyList<PyralisAuthoringCapabilityDescriptor> BuildDescriptors()
        {
            List<PyralisAuthoringCapabilityDescriptor> descriptors = new List<PyralisAuthoringCapabilityDescriptor>();
            AddContractDescriptors(descriptors);
            SortDescriptors(descriptors);
            return descriptors.ToArray();
        }

        private static void AddContractDescriptors(List<PyralisAuthoringCapabilityDescriptor> descriptors)
        {
            IReadOnlyList<ResolvedAuthoringContract> contracts = ResolvedAuthoringContractRegistry.ProductContracts;
            for (int i = 0; i < contracts.Count; i++)
            {
                ResolvedAuthoringContract contract = contracts[i];
                if (contract == null || contract.Capability == AuthoringCapability.None)
                    continue;

                RuntimeCapabilityFamily[] families =
                    PyralisAuthoringContractMetadataPolicy.BuildDescriptorRuntimeFamilies(contract);
                for (int familyIndex = 0; familyIndex < families.Length; familyIndex++)
                {
                    RuntimeCapabilityFamily family = families[familyIndex];
                    AddOrMerge(descriptors, new PyralisAuthoringCapabilityDescriptor(
                        contract.StableId,
                        FirstNonEmpty(GetContractDisplayName(contract), GetFamilyDisplayName(family)),
                        family,
                        contract.Capability,
                        GetDescriptorGroup(contract),
                        GetSortOrder(contract.Capability),
                        FirstNonEmpty(contract.Relevance, contract.DisplayName),
                        contract.Relevance,
                        contract.ProofTargetId,
                        BuildGoalTags(contract.Capability, contract.AuthoringCategory, contract.AuthoringLane),
                        BuildLaneTags(contract),
                        BuildUnsupportedLaneTags(contract),
                        contract.Axioms,
                        SimplifyTypeNames(contract.RequiredRuntimeInterfaceNames, contract.RequiredComponentNames),
                        contract.AssignmentFields,
                        contract.CustomizationMoments,
                        BuildNativeActions(contract),
                        GetContractSourceOrigin(contract),
                        capabilityPath: BuildCapabilityPath(contract, family),
                        surface: contract.Surface,
                        roleTags: BuildRoleTags(contract),
                        selectableIntent: contract.SelectableIntent && IsIntentSelectableContract(contract)));
                }
            }
        }

        private static void AddOrMerge(List<PyralisAuthoringCapabilityDescriptor> descriptors, PyralisAuthoringCapabilityDescriptor incoming)
        {
            if (incoming == null || string.IsNullOrWhiteSpace(incoming.StableId))
                return;

            for (int i = 0; i < descriptors.Count; i++)
            {
                PyralisAuthoringCapabilityDescriptor current = descriptors[i];
                if (current == null || !string.Equals(current.StableId, incoming.StableId, StringComparison.Ordinal))
                    continue;

                descriptors[i] = Merge(current, incoming);
                return;
            }

            descriptors.Add(incoming);
        }

        private static PyralisAuthoringCapabilityDescriptor Merge(
            PyralisAuthoringCapabilityDescriptor current,
            PyralisAuthoringCapabilityDescriptor incoming)
        {
            bool incomingIsContract = IsContractOwned(incoming);
            bool currentIsContract = IsContractOwned(current);

            PyralisAuthoringCapabilityDescriptor labelSource = currentIsContract || !incomingIsContract ? current : incoming;

            return new PyralisAuthoringCapabilityDescriptor(
                labelSource.StableId,
                labelSource.DisplayName,
                current.Family,
                current.Capability | incoming.Capability,
                labelSource.Group,
                Math.Min(current.SortOrder, incoming.SortOrder),
                FirstNonEmpty(current.Summary, incoming.Summary),
                FirstNonEmpty(current.RouteRelevance, incoming.RouteRelevance),
                FirstNonEmpty(current.ProofTargetId, incoming.ProofTargetId),
                MergeDistinct(current.GoalTags, incoming.GoalTags),
                MergeDistinct(current.LaneTags, incoming.LaneTags),
                MergeDistinct(current.UnsupportedLaneTags, incoming.UnsupportedLaneTags),
                current.Axioms | incoming.Axioms,
                MergeDistinct(current.RequiredSetup, incoming.RequiredSetup),
                MergeDistinct(current.AssignmentFields, incoming.AssignmentFields),
                MergeDistinct(current.CustomizationMoments, incoming.CustomizationMoments),
                MergeDistinct(current.NativeActions, incoming.NativeActions),
                currentIsContract ? current.SourceOrigin : incoming.SourceOrigin,
                FirstNonEmpty(current.CapabilityPath, incoming.CapabilityPath),
                current.Surface != AuthoringContractSurface.Auto ? current.Surface : incoming.Surface,
                MergeDistinct(current.RoleTags, incoming.RoleTags),
                current.SelectableIntent || incoming.SelectableIntent,
                current.SourceFact ?? incoming.SourceFact);
        }

        private static bool IsContractOwned(PyralisAuthoringCapabilityDescriptor descriptor)
        {
            if (descriptor == null)
                return false;

            return descriptor.SourceOrigin == PyralisAuthoringGraphSourceOrigin.Contract
                || descriptor.SourceOrigin == PyralisAuthoringGraphSourceOrigin.Reflection;
        }

        private static string GetFamilyDisplayName(RuntimeCapabilityFamily family)
        {
            return family switch
            {
                RuntimeCapabilityFamily.PlatformCore => "Platform Core",
                RuntimeCapabilityFamily.CharacterPawnGameplay => "Character / Pawn Gameplay",
                RuntimeCapabilityFamily.ActionTargeting => "Action Targeting",
                RuntimeCapabilityFamily.Combat => "Combat",
                RuntimeCapabilityFamily.GunsProjectiles => "Guns / Projectiles",
                RuntimeCapabilityFamily.ProceduralGeneration => "Procedural Generation",
                RuntimeCapabilityFamily.BoardCardTabletop => "Board / Card / Tabletop",
                RuntimeCapabilityFamily.AnimationPresentation => "Animation / Presentation",
                RuntimeCapabilityFamily.ScoringObjectives => "Scoring / Objectives",
                RuntimeCapabilityFamily.CameraInput => "Camera / Cursor",
                RuntimeCapabilityFamily.Networking => "Networking",
                _ => "Custom Capability"
            };
        }

        private static string GetGroup(ResolvedAuthoringContract contract)
        {
            if (contract == null)
                return "General";

            return FirstNonEmpty(contract.AuthoringCategory, FirstNonEmpty(contract.AuthoringLane, "Uncategorized Contract"));
        }

        private static string GetDescriptorGroup(ResolvedAuthoringContract contract)
        {
            if (contract == null)
                return "General";

            if (!string.IsNullOrWhiteSpace(contract.CapabilityPath))
            {
                string[] parts = contract.CapabilityPath.Split('/');
                if (parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]))
                    return AuthoringCapabilityRegistry.PrettifyTypeName(parts[0].Trim());
            }

            return GetGroup(contract);
        }

        private static string GetContractDisplayName(ResolvedAuthoringContract contract)
        {
            if (contract == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(contract.CapabilityPath))
            {
                string[] parts = contract.CapabilityPath.Split('/');
                for (int i = parts.Length - 1; i >= 0; i--)
                {
                    if (!string.IsNullOrWhiteSpace(parts[i]))
                        return AuthoringCapabilityRegistry.PrettifyTypeName(parts[i].Trim());
                }
            }

            return contract.DisplayName;
        }

        private static string BuildCapabilityPath(ResolvedAuthoringContract contract, RuntimeCapabilityFamily family)
        {
            if (contract == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(contract.CapabilityPath))
                return contract.CapabilityPath;

            return string.Empty;
        }

        private static string[] BuildRoleTags(ResolvedAuthoringContract contract)
        {
            List<string> tags = new List<string>();
            AddRangeDistinct(tags, contract.RoleTags);
            return tags.ToArray();
        }

        private static bool IsIntentSelectableContract(ResolvedAuthoringContract contract)
        {
            if (contract == null || contract.SourceType == null)
                return true;

            Type type = contract.SourceType;
            if (type.IsInterface || type.IsAbstract)
                return false;

            string sourceName = type.FullName ?? string.Empty;
            if (Contains(sourceName, ".Runtime.Shared.Contracts."))
                return false;
            if (Contains(sourceName, ".Core.Contracts."))
                return false;
            if (Contains(sourceName, ".Tests."))
                return false;

            return true;
        }

        private static int GetSortOrder(AuthoringCapability capability)
        {
            int index = 0;
            foreach (AuthoringCapability individual in AuthoringCapabilityRegistry.GetAllIndividualCapabilities())
            {
                if ((capability & individual) != 0)
                    return index;

                index++;
            }

            return int.MaxValue;
        }

        private static string[] BuildGoalTags(AuthoringCapability capability, params string[] additional)
        {
            List<string> tags = new List<string>();
            foreach (AuthoringCapability individual in AuthoringCapabilityRegistry.GetAllIndividualCapabilities())
            {
                if ((capability & individual) != 0)
                    AddDistinct(tags, PyralisAuthoringVocabulary.GetCapabilityDisplayName(individual));
            }

            AddRangeDistinct(tags, additional);
            return tags.ToArray();
        }

        private static string[] BuildLaneTags(ResolvedAuthoringContract contract)
        {
            List<string> tags = new List<string>();
            AddDistinct(tags, contract.AuthoringLane);
            if (contract.SupportedPresentationModes != null)
            {
                for (int i = 0; i < contract.SupportedPresentationModes.Length; i++)
                    AddDistinct(tags, contract.SupportedPresentationModes[i].ToString());
            }

            return tags.ToArray();
        }

        private static string[] BuildUnsupportedLaneTags(ResolvedAuthoringContract contract)
        {
            List<string> tags = new List<string>();
            if (contract.UnsupportedPresentationModes != null)
            {
                for (int i = 0; i < contract.UnsupportedPresentationModes.Length; i++)
                    AddDistinct(tags, contract.UnsupportedPresentationModes[i].ToString());
            }

            return tags.ToArray();
        }

        private static PyralisAuthoringNativeAction[] BuildNativeActions(ResolvedAuthoringContract contract)
        {
            if (contract.NativeSetup == null || contract.NativeSetup.Length == 0)
                return Array.Empty<PyralisAuthoringNativeAction>();

            List<PyralisAuthoringNativeAction> actions = new List<PyralisAuthoringNativeAction>();
            for (int i = 0; i < contract.NativeSetup.Length; i++)
            {
                actions.Add(PyralisAuthoringNativeActionFactory.ConfigureInspectorAction(
                    contract.DisplayName,
                    contract.NativeSetup[i],
                    "contract NativeSetup",
                    "the contract setup is visible in graph evidence"));
            }

            return actions.ToArray();
        }

        private static PyralisAuthoringGraphSourceOrigin GetContractSourceOrigin(ResolvedAuthoringContract contract)
        {
            if (contract == null)
                return PyralisAuthoringGraphSourceOrigin.Contract;

            return contract.Confidence == PyralisAuthoringConfidence.Inferred
                || contract.Confidence == PyralisAuthoringConfidence.ConventionDerived
                    ? PyralisAuthoringGraphSourceOrigin.Reflection
                    : PyralisAuthoringGraphSourceOrigin.Contract;
        }

        private static string[] SimplifyTypeNames(string[] interfaceNames, string[] componentNames)
        {
            List<string> names = new List<string>();
            AddSimplifiedNames(names, interfaceNames);
            AddSimplifiedNames(names, componentNames);
            return names.ToArray();
        }

        private static void AddSimplifiedNames(List<string> target, string[] names)
        {
            if (names == null)
                return;

            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                int lastDot = name.LastIndexOf('.');
                AddDistinct(target, lastDot >= 0 && lastDot < name.Length - 1 ? name.Substring(lastDot + 1) : name);
            }
        }

        private static void SortDescriptors(List<PyralisAuthoringCapabilityDescriptor> descriptors)
        {
            descriptors.Sort((left, right) =>
            {
                int groupCompare = string.Compare(left.Group, right.Group, StringComparison.Ordinal);
                if (groupCompare != 0)
                    return groupCompare;

                int orderCompare = left.SortOrder.CompareTo(right.SortOrder);
                return orderCompare != 0
                    ? orderCompare
                    : string.Compare(left.DisplayName, right.DisplayName, StringComparison.Ordinal);
            });
        }

        private static string NormalizeId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "unknown";

            char[] chars = value.ToLowerInvariant().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]))
                    chars[i] = '-';
            }

            return new string(chars).Trim('-');
        }

        private static string FirstNonEmpty(string first, string second)
        {
            return string.IsNullOrWhiteSpace(first) ? second : first;
        }

        private static string[] MergeDistinct(string[] first, string[] second)
        {
            List<string> values = new List<string>();
            AddRangeDistinct(values, first);
            AddRangeDistinct(values, second);
            return values.ToArray();
        }

        private static PyralisAuthoringNativeAction[] MergeDistinct(
            PyralisAuthoringNativeAction[] first,
            PyralisAuthoringNativeAction[] second)
        {
            List<PyralisAuthoringNativeAction> values = new List<PyralisAuthoringNativeAction>();
            AddRangeDistinct(values, first);
            AddRangeDistinct(values, second);
            return values.ToArray();
        }

        private static void AddRangeDistinct(List<string> target, string[] values)
        {
            if (values == null)
                return;

            for (int i = 0; i < values.Length; i++)
                AddDistinct(target, values[i]);
        }

        private static void AddRangeDistinct(List<PyralisAuthoringNativeAction> target, PyralisAuthoringNativeAction[] values)
        {
            if (values == null)
                return;

            for (int i = 0; i < values.Length; i++)
            {
                PyralisAuthoringNativeAction value = values[i];
                if (!target.Contains(value))
                    target.Add(value);
            }
        }

        private static void AddDistinct(List<RuntimeCapabilityFamily> target, RuntimeCapabilityFamily value)
        {
            if (!target.Contains(value))
                target.Add(value);
        }

        private static void AddDistinct(List<string> target, string value)
        {
            if (string.IsNullOrWhiteSpace(value) || target.Contains(value))
                return;

            target.Add(value);
        }
    }

    public sealed class PyralisAuthoringIntentDescriptorProjection
    {
        public PyralisAuthoringIntentDescriptorProjection(
            PyralisAuthoringCapabilityDescriptor descriptor,
            bool selected,
            bool inferred,
            string intentLayer)
        {
            Descriptor = descriptor;
            Selected = selected;
            Inferred = inferred;
            IntentLayer = intentLayer ?? string.Empty;
        }

        public PyralisAuthoringCapabilityDescriptor Descriptor { get; }
        public string StableId => Descriptor?.StableId ?? string.Empty;
        public bool Selected { get; }
        public bool Inferred { get; }
        public string IntentLayer { get; }
        public string Group => PyralisAuthoringIntentProjection.GetGroup(Descriptor);
        public string Subgroup => PyralisAuthoringIntentProjection.GetSubgroup(Descriptor);
        public string LeafLabel => PyralisAuthoringIntentProjection.GetLeafLabel(Descriptor);
    }

    public sealed class PyralisAuthoringIntentDescriptorSubgroupProjection
    {
        public PyralisAuthoringIntentDescriptorSubgroupProjection(
            string subgroup,
            IReadOnlyList<PyralisAuthoringIntentDescriptorProjection> descriptors)
        {
            Subgroup = subgroup ?? string.Empty;
            Descriptors = descriptors ?? Array.Empty<PyralisAuthoringIntentDescriptorProjection>();
        }

        public string Subgroup { get; }
        public IReadOnlyList<PyralisAuthoringIntentDescriptorProjection> Descriptors { get; }
        public int DescriptorCount => Descriptors.Count;
        public int SelectedCount => Descriptors.Count(descriptor => descriptor.Selected);
        public int InferredCount => Descriptors.Count(descriptor => descriptor.Inferred);
    }

    public sealed class PyralisAuthoringIntentDescriptorGroupProjection
    {
        public PyralisAuthoringIntentDescriptorGroupProjection(
            string group,
            IReadOnlyList<PyralisAuthoringIntentDescriptorSubgroupProjection> subgroups)
        {
            Group = group ?? "General";
            Subgroups = subgroups ?? Array.Empty<PyralisAuthoringIntentDescriptorSubgroupProjection>();
        }

        public string Group { get; }
        public IReadOnlyList<PyralisAuthoringIntentDescriptorSubgroupProjection> Subgroups { get; }
        public int DescriptorCount => Subgroups.Sum(subgroup => subgroup.DescriptorCount);
        public int SelectedCount => Subgroups.Sum(subgroup => subgroup.SelectedCount);
        public int InferredCount => Subgroups.Sum(subgroup => subgroup.InferredCount);
    }

    public sealed class PyralisAuthoringIntentProjection
    {
        public PyralisAuthoringIntentProjection(
            PyralisAuthoringIntentSelection selection,
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors,
            IReadOnlyList<string> selectedDescriptorIds)
        {
            Selection = selection ?? new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.None,
                AuthoringWorldAxiom.None);
            Descriptors = descriptors ?? Array.Empty<PyralisAuthoringCapabilityDescriptor>();
            SelectedDescriptorIds = PyralisAuthoringCapabilityDescriptorRegistry.FilterGameplayIntentDescriptorIds(
                selectedDescriptorIds ?? Array.Empty<string>(),
                Descriptors);
            SelectedDescriptorIdSet = new HashSet<string>(SelectedDescriptorIds, StringComparer.Ordinal);
            InferredRouteEssentialIds = Descriptors
                .Where(descriptor => PyralisAuthoringCapabilityDescriptorRegistry.IsIntentRouteEssentialExpected(descriptor, Selection))
                .Select(descriptor => descriptor.StableId)
                .ToArray();
            InferredRouteEssentialIdSet = new HashSet<string>(InferredRouteEssentialIds, StringComparer.Ordinal);
            GameplayIngredientDescriptors = Descriptors
                .Where(PyralisAuthoringCapabilityDescriptorRegistry.IsGameplayIngredientDescriptor)
                .ToArray();
            MetadataBacklogDescriptors = Descriptors
                .Where(PyralisAuthoringCapabilityDescriptorRegistry.IsIntentMetadataBacklogDescriptor)
                .ToArray();
            RouteEssentialDescriptors = Descriptors
                .Where(descriptor => descriptor != null && InferredRouteEssentialIdSet.Contains(descriptor.StableId))
                .ToArray();
            AllGroups = BuildGroups(Descriptors, "Available");
            GameplayIngredientGroups = BuildGroups(GameplayIngredientDescriptors, "GameplayIngredient");
            MetadataBacklogGroups = BuildGroups(MetadataBacklogDescriptors, "MetadataBacklog");
            RouteEssentialGroups = BuildGroups(RouteEssentialDescriptors, "RouteEssential");
            SelectedDescriptors = GameplayIngredientDescriptors
                .Where(descriptor => SelectedDescriptorIdSet.Contains(descriptor.StableId))
                .Select(descriptor => new PyralisAuthoringIntentDescriptorProjection(descriptor, true, false, "GameplayIngredient"))
                .ToArray();
            RouteShapePreview = PyralisAuthoringSetupGraphProjection.BuildRouteShapeSummary(Selection);
            LensSummary = BuildLensSummary();
        }

        public PyralisAuthoringIntentSelection Selection { get; }
        public IReadOnlyList<PyralisAuthoringCapabilityDescriptor> Descriptors { get; }
        public string[] SelectedDescriptorIds { get; }
        public string[] InferredRouteEssentialIds { get; }
        public IReadOnlyList<PyralisAuthoringCapabilityDescriptor> GameplayIngredientDescriptors { get; }
        public IReadOnlyList<PyralisAuthoringCapabilityDescriptor> MetadataBacklogDescriptors { get; }
        public IReadOnlyList<PyralisAuthoringCapabilityDescriptor> RouteEssentialDescriptors { get; }
        public IReadOnlyList<PyralisAuthoringIntentDescriptorGroupProjection> AllGroups { get; }
        public IReadOnlyList<PyralisAuthoringIntentDescriptorGroupProjection> GameplayIngredientGroups { get; }
        public IReadOnlyList<PyralisAuthoringIntentDescriptorGroupProjection> MetadataBacklogGroups { get; }
        public IReadOnlyList<PyralisAuthoringIntentDescriptorGroupProjection> RouteEssentialGroups { get; }
        public IReadOnlyList<PyralisAuthoringIntentDescriptorProjection> SelectedDescriptors { get; }
        public string RouteShapePreview { get; }
        public string LensSummary { get; }
        public int GameplayIngredientCount => GameplayIngredientDescriptors.Count;
        public int SelectedGameplayIngredientCount => SelectedDescriptors.Count;
        public int MetadataBacklogCount => MetadataBacklogDescriptors.Count;
        public int RouteEssentialCount => RouteEssentialDescriptors.Count;
        private HashSet<string> SelectedDescriptorIdSet { get; }
        private HashSet<string> InferredRouteEssentialIdSet { get; }

        public static PyralisAuthoringIntentProjection Build(
            PyralisAuthoringIntentSelection selection,
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors)
        {
            return new PyralisAuthoringIntentProjection(
                selection,
                descriptors,
                selection?.DescriptorIds ?? Array.Empty<string>());
        }

        public static string GetGroup(PyralisAuthoringCapabilityDescriptor descriptor)
        {
            if (descriptor == null)
                return "Shared Ingredients";

            if (!string.IsNullOrWhiteSpace(descriptor.CapabilityPath))
            {
                string[] parts = descriptor.CapabilityPath.Split('/');
                if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[0]))
                    return parts[0].Trim();
            }

            return string.IsNullOrWhiteSpace(descriptor.Group) ? "Shared Ingredients" : descriptor.Group;
        }

        public static string GetSubgroup(PyralisAuthoringCapabilityDescriptor descriptor)
        {
            if (descriptor == null || string.IsNullOrWhiteSpace(descriptor.CapabilityPath))
                return string.Empty;

            string[] parts = descriptor.CapabilityPath.Split('/');
            return parts.Length > 2 && !string.IsNullOrWhiteSpace(parts[1])
                ? parts[1].Trim()
                : string.Empty;
        }

        public static string GetLeafLabel(PyralisAuthoringCapabilityDescriptor descriptor)
        {
            if (descriptor == null)
                return "Unknown";

            if (string.IsNullOrWhiteSpace(descriptor.CapabilityPath))
                return descriptor.DisplayName;

            string[] parts = descriptor.CapabilityPath.Split('/');
            if (parts.Length > 2 && !string.IsNullOrWhiteSpace(parts[2]))
                return string.Join(" / ", GetNonEmptyPathParts(parts, 2));
            if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
                return parts[1].Trim();

            return descriptor.DisplayName;
        }

        private IReadOnlyList<PyralisAuthoringIntentDescriptorGroupProjection> BuildGroups(
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors,
            string intentLayer)
        {
            if (descriptors == null || descriptors.Count == 0)
                return Array.Empty<PyralisAuthoringIntentDescriptorGroupProjection>();

            return descriptors
                .Where(descriptor => descriptor != null)
                .Select(descriptor => new PyralisAuthoringIntentDescriptorProjection(
                    descriptor,
                    SelectedDescriptorIdSet.Contains(descriptor.StableId),
                    InferredRouteEssentialIdSet.Contains(descriptor.StableId),
                    intentLayer))
                .GroupBy(descriptor => descriptor.Group)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new PyralisAuthoringIntentDescriptorGroupProjection(
                    group.Key,
                    group
                        .GroupBy(descriptor => descriptor.Subgroup)
                        .OrderBy(subgroup => subgroup.Key, StringComparer.Ordinal)
                        .Select(subgroup => new PyralisAuthoringIntentDescriptorSubgroupProjection(
                            subgroup.Key,
                            subgroup
                                .OrderBy(descriptor => descriptor.Descriptor.SortOrder)
                                .ThenBy(descriptor => descriptor.Descriptor.DisplayName, StringComparer.Ordinal)
                                .ToArray()))
                        .ToArray()))
                .ToArray();
        }

        private string BuildLensSummary()
        {
            return $"Lenses: {SelectedGameplayIngredientCount}/{GameplayIngredientCount} gameplay ingredients selected, {RouteEssentialCount} route essentials inferred, {MetadataBacklogCount} metadata backlog.";
        }

        private static string[] GetNonEmptyPathParts(string[] parts, int startIndex)
        {
            List<string> labels = new List<string>();
            if (parts == null)
                return labels.ToArray();

            for (int i = startIndex; i < parts.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(parts[i]))
                    labels.Add(parts[i].Trim());
            }

            return labels.ToArray();
        }
    }

    public sealed class PyralisAuthoringContractMetadataClassification
    {
        public PyralisAuthoringContractMetadataClassification(
            string ownershipBucket,
            string repairOwner,
            string ownershipAdvice)
        {
            OwnershipBucket = ownershipBucket ?? string.Empty;
            RepairOwner = repairOwner ?? string.Empty;
            OwnershipAdvice = ownershipAdvice ?? string.Empty;
        }

        public string OwnershipBucket { get; }
        public string RepairOwner { get; }
        public string OwnershipAdvice { get; }
    }

    public static class PyralisAuthoringContractMetadataPolicy
    {
        public const string ContractOwnedSemanticMetadata = "ContractOwnedSemanticMetadata";
        public const string ReflectionInferredRuntimeFamily = "ReflectionInferredRuntimeFamily";
        public const string SupportOnlyContract = "SupportOnlyContract";
        public const string DuplicateOwnershipClaim = "DuplicateOwnershipClaim";
        public const string NeedsFeatureProofTarget = "NeedsFeatureProofTarget";
        public const string ValidationMetadata = "ValidationMetadata";

        public static bool ShouldEmitRuntimeFamiliesMissing(ResolvedAuthoringContract contract)
        {
            if (contract == null || contract.Capability == AuthoringCapability.None)
                return false;
            if (contract.RuntimeFamilies != null && contract.RuntimeFamilies.Length > 0)
                return false;
            if (IsSupportOnlyContract(contract))
                return false;

            return InferRuntimeFamilies(contract).Length == 0;
        }

        public static RuntimeCapabilityFamily[] BuildDescriptorRuntimeFamilies(ResolvedAuthoringContract contract)
        {
            if (contract == null)
                return Array.Empty<RuntimeCapabilityFamily>();
            if (contract.RuntimeFamilies != null && contract.RuntimeFamilies.Length > 0)
                return contract.RuntimeFamilies;

            RuntimeCapabilityFamily[] inferred = InferRuntimeFamilies(contract);
            return inferred.Length > 0 ? inferred : new[] { RuntimeCapabilityFamily.Custom };
        }

        public static PyralisAuthoringContractMetadataClassification Classify(
            ResolvedAuthoringContract contract,
            string issueCode)
        {
            if (string.IsNullOrWhiteSpace(issueCode))
                return EmptyClassification();

            switch (issueCode)
            {
                case "ContractMetadata.CapabilityPathMissing":
                case "ContractMetadata.RouteEssentialCapabilityPathMissing":
                    return new PyralisAuthoringContractMetadataClassification(
                        ContractOwnedSemanticMetadata,
                        "Contract",
                        "This is semantic Intent or route grouping metadata. Add it to the feature-owned contract only when the contract is meant to steer authoring.");
                case "ContractMetadata.RuntimeFamiliesMissing":
                    if (IsSupportOnlyContract(contract))
                    {
                        return new PyralisAuthoringContractMetadataClassification(
                            SupportOnlyContract,
                            "Projection",
                            "This contract is support-only evidence. It should not ask humans for route metadata; keep it out of selectable Intent or leave it as graph support.");
                    }

                    if (InferRuntimeFamilies(contract).Length > 0)
                    {
                        return new PyralisAuthoringContractMetadataClassification(
                            ReflectionInferredRuntimeFamily,
                            "Reflection",
                            "The runtime family can be inferred from reflected type, interface, required-component, or concrete runtime-surface evidence. Do not duplicate it in the contract.");
                    }

                    return new PyralisAuthoringContractMetadataClassification(
                        ContractOwnedSemanticMetadata,
                        "Contract",
                        "This contract carries semantic runtime family meaning that reflection cannot prove. Add the semantic runtime family to the feature-owned contract.");
                case "ContractMetadata.DuplicateOwnershipClaim":
                    return new PyralisAuthoringContractMetadataClassification(
                        DuplicateOwnershipClaim,
                        "Contract",
                        "Ownership claims are semantic responsibility keys. Narrow or remove the duplicate claim in the competing contracts.");
                case "ContractMetadata.ProofTargetGenericTemplate":
                    return new PyralisAuthoringContractMetadataClassification(
                        NeedsFeatureProofTarget,
                        "Contract",
                        "Proof ownership is semantic. Replace generic proof fallback metadata with a feature-owned proof target only where the feature owns that proof.");
                case "ValidationEvidence.MetadataMissing":
                    return new PyralisAuthoringContractMetadataClassification(
                        ValidationMetadata,
                        "Validator",
                        "Validation must emit stable issue metadata so the graph can route the finding without reading prose.");
                default:
                    return EmptyClassification();
            }
        }

        public static RuntimeCapabilityFamily[] InferRuntimeFamilies(ResolvedAuthoringContract contract)
        {
            if (contract == null || contract.Capability == AuthoringCapability.None)
                return Array.Empty<RuntimeCapabilityFamily>();

            Type sourceType = contract.SourceType;
            if (sourceType != null && typeof(ScriptableObject).IsAssignableFrom(sourceType))
                return Array.Empty<RuntimeCapabilityFamily>();

            bool concreteUnitySurface = sourceType != null && typeof(MonoBehaviour).IsAssignableFrom(sourceType);
            List<RuntimeCapabilityFamily> families = new List<RuntimeCapabilityFamily>();
            string evidence = BuildEvidenceText(contract, concreteUnitySurface);

            AddFromEvidence(families, evidence);

            if (concreteUnitySurface)
                AddFromCapability(families, contract.Capability);

            return families.ToArray();
        }

        public static bool IsSupportOnlyContract(ResolvedAuthoringContract contract)
        {
            if (contract == null)
                return true;

            if (HasRouteRoleTag(contract))
                return false;

            Type type = contract.SourceType;
            if (type != null && (type.IsInterface || type.IsAbstract))
                return true;

            string sourceName = type?.FullName ?? string.Empty;
            return Contains(sourceName, ".Core.Contracts.")
                || Contains(sourceName, ".Runtime.Shared.Contracts.")
                || !contract.SelectableIntent;
        }

        private static PyralisAuthoringContractMetadataClassification EmptyClassification()
        {
            return new PyralisAuthoringContractMetadataClassification(string.Empty, string.Empty, string.Empty);
        }

        private static string BuildEvidenceText(ResolvedAuthoringContract contract, bool includeSemanticLabels)
        {
            List<string> values = new List<string>();
            if (includeSemanticLabels)
            {
                Add(values, contract.DisplayName);
                Add(values, contract.ModuleId);
                Add(values, contract.AuthoringCategory);
                Add(values, contract.AuthoringLane);
                Add(values, contract.SetupNodeId);
                Add(values, contract.SourceType?.FullName);
                Add(values, contract.RequiredProfileType?.FullName);
            }

            AddRange(values, contract.RequiredRuntimeInterfaceNames);
            AddRange(values, contract.RequiredComponentNames);
            if (includeSemanticLabels)
            {
                AddRange(values, contract.AssignmentFields);
                AddRange(values, contract.NativeSetup);
            }

            Type sourceType = contract.SourceType;
            if (sourceType != null)
            {
                Type[] interfaces = sourceType.GetInterfaces();
                for (int i = 0; i < interfaces.Length; i++)
                    Add(values, interfaces[i].FullName);
            }

            return string.Join(" ", values);
        }

        private static void AddFromEvidence(List<RuntimeCapabilityFamily> families, string evidence)
        {
            if (string.IsNullOrWhiteSpace(evidence))
                return;

            if (ContainsAny(evidence, "network", "authority", "clientid", "transport"))
                AddDistinct(families, RuntimeCapabilityFamily.Networking);
            if (ContainsAny(evidence, "camera", "cinemachine", "occlusion", "fader"))
                AddDistinct(families, RuntimeCapabilityFamily.CameraInput);
            if (ContainsAny(evidence, "score", "objective", "leaderboard"))
                AddDistinct(families, RuntimeCapabilityFamily.ScoringObjectives);
            if (ContainsAny(evidence, "tabletop", "board", "grid", "card", "turnorder", "turnruntime", "phase"))
                AddDistinct(families, RuntimeCapabilityFamily.BoardCardTabletop);
            if (ContainsAny(evidence, "projectile", "launcher", "ranged", "firemode", "weapon"))
                AddDistinct(families, RuntimeCapabilityFamily.GunsProjectiles);
            if (ContainsAny(evidence, "combat", "damage", "health", "hitbox", "status", "guard", "melee", "attack"))
                AddDistinct(families, RuntimeCapabilityFamily.Combat);
            if (ContainsAny(evidence, "input", "action", "target", "interaction"))
                AddDistinct(families, RuntimeCapabilityFamily.ActionTargeting);
            if (ContainsAny(evidence, "pawn", "motor", "movement", "traversal", "steering", "charactercontroller"))
                AddDistinct(families, RuntimeCapabilityFamily.CharacterPawnGameplay);
            if (ContainsAny(evidence, "animation", "animator", "presentation", "sprite", "billboard", "vfx", "feedback", "shake", "shadow", "audio"))
                AddDistinct(families, RuntimeCapabilityFamily.AnimationPresentation);
            if (ContainsAny(evidence, "session", "bootstrap", "participant", "roster", "spawn", "lifetime", "setup", "settings", "gameconfig"))
                AddDistinct(families, RuntimeCapabilityFamily.PlatformCore);
        }

        private static void AddFromCapability(List<RuntimeCapabilityFamily> families, AuthoringCapability capability)
        {
            if (HasAnyCapability(capability, AuthoringCapability.Networking))
                AddDistinct(families, RuntimeCapabilityFamily.Networking);
            if (HasAnyCapability(capability, AuthoringCapability.Camera))
                AddDistinct(families, RuntimeCapabilityFamily.CameraInput);
            if (HasAnyCapability(capability, AuthoringCapability.Scoring))
                AddDistinct(families, RuntimeCapabilityFamily.ScoringObjectives);
            if (HasAnyCapability(capability, AuthoringCapability.Tabletop | AuthoringCapability.Grid | AuthoringCapability.TurnBased))
                AddDistinct(families, RuntimeCapabilityFamily.BoardCardTabletop);
            if (HasAnyCapability(capability, AuthoringCapability.RangedFlow))
                AddDistinct(families, RuntimeCapabilityFamily.GunsProjectiles);
            if (HasAnyCapability(capability, AuthoringCapability.Combat | AuthoringCapability.CombatState | AuthoringCapability.CombatSensors | AuthoringCapability.MeleeFlow | AuthoringCapability.TacticsAggressive | AuthoringCapability.TacticsDefensive))
                AddDistinct(families, RuntimeCapabilityFamily.Combat);
            if (HasAnyCapability(capability, AuthoringCapability.Input))
                AddDistinct(families, RuntimeCapabilityFamily.ActionTargeting);
            if (HasAnyCapability(capability, AuthoringCapability.Movement | AuthoringCapability.Traversal | AuthoringCapability.KineticMotor2D | AuthoringCapability.KineticMotor3D | AuthoringCapability.Steering2D | AuthoringCapability.Steering3D))
                AddDistinct(families, RuntimeCapabilityFamily.CharacterPawnGameplay);
            if (HasAnyCapability(capability, AuthoringCapability.Animation | AuthoringCapability.VFX | AuthoringCapability.Audio))
                AddDistinct(families, RuntimeCapabilityFamily.AnimationPresentation);
            if (HasAnyCapability(capability, AuthoringCapability.Setup | AuthoringCapability.Session | AuthoringCapability.Participants | AuthoringCapability.Rules))
                AddDistinct(families, RuntimeCapabilityFamily.PlatformCore);
        }

        private static bool HasRouteRoleTag(ResolvedAuthoringContract contract)
        {
            return HasRoleTag(contract, AuthoringContractRoleTags.IntentRouteEssential)
                || HasRoleTag(contract, AuthoringContractRoleTags.CoreRouteAnchor)
                || HasRoleTag(contract, AuthoringContractRoleTags.ParticipantRouteSupport)
                || HasRoleTag(contract, AuthoringContractRoleTags.InputRouteSupport)
                || HasRoleTag(contract, AuthoringContractRoleTags.NetworkRouteSupport)
                || HasRoleTag(contract, AuthoringContractRoleTags.CombatDefinitionRouteSupport)
                || HasRoleTag(contract, AuthoringContractRoleTags.ModuleCapabilityRouteSupport)
                || HasRoleTag(contract, AuthoringContractRoleTags.AnimationDefinitionRouteSupport);
        }

        private static bool HasRoleTag(ResolvedAuthoringContract contract, string expected)
        {
            if (contract?.RoleTags == null || string.IsNullOrWhiteSpace(expected))
                return false;

            for (int i = 0; i < contract.RoleTags.Length; i++)
            {
                if (string.Equals(contract.RoleTags[i], expected, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static bool HasAnyCapability(AuthoringCapability capability, AuthoringCapability expected)
        {
            return (capability & expected) != 0;
        }

        private static bool ContainsAny(string value, params string[] needles)
        {
            for (int i = 0; i < needles.Length; i++)
            {
                if (Contains(value, needles[i]))
                    return true;
            }

            return false;
        }

        private static bool Contains(string value, string expected)
        {
            return !string.IsNullOrWhiteSpace(value)
                && !string.IsNullOrWhiteSpace(expected)
                && value.IndexOf(expected, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void Add(List<string> values, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                values.Add(value);
        }

        private static void AddRange(List<string> values, string[] source)
        {
            if (source == null)
                return;

            for (int i = 0; i < source.Length; i++)
                Add(values, source[i]);
        }

        private static void AddDistinct(List<RuntimeCapabilityFamily> families, RuntimeCapabilityFamily family)
        {
            if (!families.Contains(family))
                families.Add(family);
        }
    }
}
