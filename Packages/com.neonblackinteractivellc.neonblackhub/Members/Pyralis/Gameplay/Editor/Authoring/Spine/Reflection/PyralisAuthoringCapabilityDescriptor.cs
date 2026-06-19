using System;
using System.Collections.Generic;
using System.Linq;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;

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
        public string[] RoleTags { get; }
        public bool SelectableIntent { get; }
        public PyralisAuthoringFact SourceFact { get; }

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
            {
                if (lane == RuntimeCapabilityLaneTag.Mixed && axioms == AuthoringWorldAxiom.None)
                    return Array.Empty<RuntimeCapabilityFamily>();

                return InferFamiliesFromCapability(capabilities, lane.ToString(), axioms);
            }

            List<RuntimeCapabilityFamily> families = new List<RuntimeCapabilityFamily>();
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors = All;
            for (int i = 0; i < descriptors.Count; i++)
            {
                PyralisAuthoringCapabilityDescriptor descriptor = descriptors[i];
                if (descriptor != null
                    && IsCapabilitySatisfiedBySelection(descriptor.Capability, capabilities)
                    && descriptor.Matches(capabilities, lane, axioms))
                {
                    AddDistinct(families, descriptor.Family);
                }
            }

            RuntimeCapabilityFamily[] inferredFamilies = InferFamiliesFromCapability(capabilities, lane.ToString(), axioms);
            for (int i = 0; i < inferredFamilies.Length; i++)
                AddDistinct(families, inferredFamilies[i]);

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

                if (descriptor.Matches(descriptor.Capability, lane, axioms))
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
                if (descriptor != null && selected.Contains(descriptor.StableId))
                    capabilities |= descriptor.Capability;
            }

            return capabilities;
        }

        public static IReadOnlyList<PyralisAuthoringCapabilityDescriptor> BuildIntentDescriptors(
            RuntimeCapabilityLaneTag lane,
            AuthoringWorldAxiom axioms)
        {
            return All
                .Where(descriptor => descriptor != null
                    && descriptor.SelectableIntent
                    && descriptor.Capability != AuthoringCapability.None
                    && descriptor.Matches(descriptor.Capability, lane, axioms))
                .OrderBy(descriptor => descriptor.Group, StringComparer.Ordinal)
                .ThenBy(descriptor => descriptor.SortOrder)
                .ThenBy(descriptor => descriptor.DisplayName, StringComparer.Ordinal)
                .ToArray();
        }

        public static PyralisAuthoringCapabilityDescriptor FindPrimaryByFamily(RuntimeCapabilityFamily family)
        {
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors = All;
            PyralisAuthoringCapabilityDescriptor fallback = null;
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

                fallback ??= descriptor;
            }

            return fallback;
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
                if (descriptor == null || (descriptor.Capability & capability) == 0)
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
            AddCapabilityVocabularyFallbacks(descriptors);
            SortDescriptors(descriptors);
            return descriptors.ToArray();
        }

        private static void AddContractDescriptors(List<PyralisAuthoringCapabilityDescriptor> descriptors)
        {
            IReadOnlyList<ResolvedAuthoringContract> contracts = ResolvedAuthoringContractRegistry.All;
            for (int i = 0; i < contracts.Count; i++)
            {
                ResolvedAuthoringContract contract = contracts[i];
                if (contract == null || contract.Capability == AuthoringCapability.None)
                    continue;

                RuntimeCapabilityFamily[] families = InferFamiliesFromCapability(contract.Capability, contract.AuthoringLane, contract.Axioms);
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
                        contract.FirstProofTargetId,
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
                        roleTags: BuildRoleTags(contract, family),
                        selectableIntent: contract.SelectableIntent && IsIntentSelectableContract(contract)));
                }
            }
        }

        private static void AddCapabilityVocabularyFallbacks(List<PyralisAuthoringCapabilityDescriptor> descriptors)
        {
            IReadOnlyList<PyralisCapabilityVocabularyCard> cards = PyralisCapabilityVocabulary.All;
            for (int i = 0; i < cards.Count; i++)
            {
                PyralisCapabilityVocabularyCard card = cards[i];
                if (card == null)
                    continue;

                PyralisAuthoringFact fact = card.Fact;
                if (fact == null)
                    continue;

                AddOrMerge(descriptors, new PyralisAuthoringCapabilityDescriptor(
                    card.StableId,
                    card.DisplayName,
                    card.CapabilityFamily,
                    fact.Capability,
                    GetGroup(fact),
                    GetSortOrder(fact.Capability),
                    fact.Summary,
                    fact.RouteRelevance,
                    GetProofTargetId(fact),
                    fact.GoalTags,
                    fact.LaneTags,
                    fact.UnsupportedLaneTags,
                    fact.Axioms,
                    Combine(fact.RequiredDefinitions, fact.RequiredProfiles, fact.RequiredSceneComponents, fact.RequiredUnitySurfaces),
                    fact.AssignmentFields,
                    fact.CustomizationMoments,
                    fact.NativeActions,
                    PyralisAuthoringGraphSourceOrigin.SpineGrammar,
                    capabilityPath: GetFallbackCapabilityPath(card, fact),
                    roleTags: fact.GoalTags,
                    selectableIntent: true,
                    sourceFact: fact));
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

            if (currentIsContract != incomingIsContract)
            {
                PyralisAuthoringCapabilityDescriptor contractDescriptor = currentIsContract ? current : incoming;
                PyralisAuthoringCapabilityDescriptor fallbackDescriptor = currentIsContract ? incoming : current;
                return MergeContractWithFallback(contractDescriptor, fallbackDescriptor);
            }

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
                MergeDistinct(current.RoleTags, incoming.RoleTags),
                current.SelectableIntent || incoming.SelectableIntent,
                current.SourceFact ?? incoming.SourceFact);
        }

        private static PyralisAuthoringCapabilityDescriptor MergeContractWithFallback(
            PyralisAuthoringCapabilityDescriptor contractDescriptor,
            PyralisAuthoringCapabilityDescriptor fallbackDescriptor)
        {
            return new PyralisAuthoringCapabilityDescriptor(
                contractDescriptor.StableId,
                FirstNonEmpty(contractDescriptor.DisplayName, fallbackDescriptor.DisplayName),
                contractDescriptor.Family,
                contractDescriptor.Capability,
                FirstNonEmpty(contractDescriptor.Group, fallbackDescriptor.Group),
                Math.Min(contractDescriptor.SortOrder, fallbackDescriptor.SortOrder),
                FirstNonEmpty(contractDescriptor.Summary, fallbackDescriptor.Summary),
                FirstNonEmpty(contractDescriptor.RouteRelevance, fallbackDescriptor.RouteRelevance),
                contractDescriptor.ProofTargetId,
                contractDescriptor.GoalTags,
                contractDescriptor.LaneTags,
                contractDescriptor.UnsupportedLaneTags,
                contractDescriptor.Axioms,
                contractDescriptor.RequiredSetup,
                contractDescriptor.AssignmentFields,
                contractDescriptor.CustomizationMoments,
                contractDescriptor.NativeActions,
                contractDescriptor.SourceOrigin,
                FirstNonEmpty(contractDescriptor.CapabilityPath, fallbackDescriptor.CapabilityPath),
                MergeDistinct(contractDescriptor.RoleTags, fallbackDescriptor.RoleTags),
                contractDescriptor.SelectableIntent || fallbackDescriptor.SelectableIntent,
                contractDescriptor.SourceFact);
        }

        private static bool IsContractOwned(PyralisAuthoringCapabilityDescriptor descriptor)
        {
            if (descriptor == null)
                return false;

            return descriptor.SourceOrigin == PyralisAuthoringGraphSourceOrigin.Contract
                || descriptor.SourceOrigin == PyralisAuthoringGraphSourceOrigin.Reflection;
        }

        private static RuntimeCapabilityFamily[] InferFamiliesFromCapability(
            AuthoringCapability capability,
            string lane,
            AuthoringWorldAxiom axioms)
        {
            List<RuntimeCapabilityFamily> families = new List<RuntimeCapabilityFamily>();

            if (HasAnyCapability(capability, AuthoringCapability.Setup, AuthoringCapability.Session, AuthoringCapability.Participants))
                AddDistinct(families, RuntimeCapabilityFamily.PlatformCore);
            if (HasAnyCapability(capability, AuthoringCapability.Movement, AuthoringCapability.KineticMotor2D, AuthoringCapability.KineticMotor3D, AuthoringCapability.Steering2D, AuthoringCapability.Steering3D, AuthoringCapability.Traversal, AuthoringCapability.Participants))
                AddDistinct(families, RuntimeCapabilityFamily.CharacterPawnGameplay);
            if (HasAnyCapability(capability, AuthoringCapability.Combat, AuthoringCapability.CombatState, AuthoringCapability.CombatSensors, AuthoringCapability.MeleeFlow, AuthoringCapability.TacticsAggressive, AuthoringCapability.TacticsDefensive))
                AddDistinct(families, RuntimeCapabilityFamily.Combat);
            if (HasAnyCapability(capability, AuthoringCapability.RangedFlow))
            {
                AddDistinct(families, RuntimeCapabilityFamily.GunsProjectiles);
                AddDistinct(families, RuntimeCapabilityFamily.Combat);
            }
            if (HasAnyCapability(capability, AuthoringCapability.Rules, AuthoringCapability.TurnBased, AuthoringCapability.Puzzle, AuthoringCapability.Input, AuthoringCapability.UI))
                AddDistinct(families, RuntimeCapabilityFamily.ActionTargeting);
            if (HasAnyCapability(capability, AuthoringCapability.Tabletop, AuthoringCapability.Grid))
                AddDistinct(families, RuntimeCapabilityFamily.BoardCardTabletop);
            if (HasAnyCapability(capability, AuthoringCapability.Camera))
                AddDistinct(families, RuntimeCapabilityFamily.CameraInput);
            if (HasAnyCapability(capability, AuthoringCapability.Animation, AuthoringCapability.VFX))
                AddDistinct(families, RuntimeCapabilityFamily.AnimationPresentation);
            if (HasAnyCapability(capability, AuthoringCapability.Scoring, AuthoringCapability.UI))
                AddDistinct(families, RuntimeCapabilityFamily.ScoringObjectives);
            if (HasAnyCapability(capability, AuthoringCapability.Environment) || (axioms & AuthoringWorldAxiom.InfiniteSpace) != 0)
                AddDistinct(families, RuntimeCapabilityFamily.ProceduralGeneration);
            if (HasAnyCapability(capability, AuthoringCapability.Networking) || (axioms & AuthoringWorldAxiom.Networked) != 0)
                AddDistinct(families, RuntimeCapabilityFamily.Networking);

            if (string.Equals(lane, RuntimeCapabilityLaneTag.TabletopBoard.ToString(), StringComparison.OrdinalIgnoreCase))
                AddDistinct(families, RuntimeCapabilityFamily.BoardCardTabletop);
            if (string.Equals(lane, RuntimeCapabilityLaneTag.CameraCursor.ToString(), StringComparison.OrdinalIgnoreCase))
                AddDistinct(families, RuntimeCapabilityFamily.CameraInput);
            if (string.Equals(lane, "Combat", StringComparison.OrdinalIgnoreCase))
                AddDistinct(families, RuntimeCapabilityFamily.Combat);
            if (string.Equals(lane, "Projectile", StringComparison.OrdinalIgnoreCase)
                || string.Equals(lane, "Projectiles", StringComparison.OrdinalIgnoreCase))
                AddDistinct(families, RuntimeCapabilityFamily.GunsProjectiles);
            if (string.Equals(lane, "Movement", StringComparison.OrdinalIgnoreCase)
                || string.Equals(lane, "Traversal", StringComparison.OrdinalIgnoreCase))
                AddDistinct(families, RuntimeCapabilityFamily.CharacterPawnGameplay);
            if (string.Equals(lane, "Animation", StringComparison.OrdinalIgnoreCase)
                || string.Equals(lane, "Presentation", StringComparison.OrdinalIgnoreCase))
                AddDistinct(families, RuntimeCapabilityFamily.AnimationPresentation);
            if (string.Equals(lane, "Camera", StringComparison.OrdinalIgnoreCase))
                AddDistinct(families, RuntimeCapabilityFamily.CameraInput);
            if (string.Equals(lane, "Setup", StringComparison.OrdinalIgnoreCase)
                || string.Equals(lane, "Session", StringComparison.OrdinalIgnoreCase))
                AddDistinct(families, RuntimeCapabilityFamily.PlatformCore);

            return families.Count > 0 ? families.ToArray() : new[] { RuntimeCapabilityFamily.Custom };
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

            return FirstNonEmpty(contract.AuthoringCategory, FirstNonEmpty(contract.AuthoringLane, GetFallbackGroup(contract.Capability)));
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

            string domainPath = BuildDomainPath(contract, family);
            string leaf = BuildContractPathLeaf(contract, family);

            return domainPath + "/" + leaf;
        }

        private static string BuildDomainPath(ResolvedAuthoringContract contract, RuntimeCapabilityFamily family)
        {
            string sourceName = contract.SourceType != null ? contract.SourceType.FullName ?? string.Empty : string.Empty;
            string category = FirstNonEmpty(contract.AuthoringCategory, contract.AuthoringLane);

            if (Contains(sourceName, ".Networking."))
                return "Networking";
            if (Contains(sourceName, ".Data.Definitions.Rpg.") || Contains(sourceName, ".Core.Rpg.") || Contains(sourceName, ".Features.Rpg."))
                return "RPG & Narrative/" + GetRpgSubdomain(contract);
            if (Contains(sourceName, ".Data.Definitions.Rules.") || Contains(sourceName, ".Core.Rules.Board.") || Contains(sourceName, ".Features.Tabletop."))
                return "Strategy & Board/" + GetBoardSubdomain(contract);
            if (Contains(sourceName, ".Data.Definitions.Combat.") || Contains(sourceName, ".Data.Profiles.Combat.") || Contains(sourceName, ".Features.Combat."))
                return "Combat/" + GetCombatSubdomain(contract);
            if (Contains(sourceName, ".Features.Enemies."))
                return "NPC & AI/" + GetEnemySubdomain(contract);
            if (Contains(sourceName, ".Features.Traversal."))
                return "Movement/Traversal";
            if (Contains(sourceName, ".Features.Input.") || Contains(sourceName, ".Core.Input"))
                return "Core Setup/Input";
            if (Contains(sourceName, ".Features.Characters.2D.") || Contains(sourceName, ".Features.Characters.Runtime.Shared.Movement.2D."))
                return HasAnyCapability(contract.Capability, AuthoringCapability.Combat, AuthoringCapability.CombatState, AuthoringCapability.CombatSensors)
                    ? "Combat/2D Pawn"
                    : "Movement/2D";
            if (Contains(sourceName, ".Features.Characters.3D.") || Contains(sourceName, ".Features.Characters.Runtime.Shared.Components.3D.") || Contains(sourceName, ".Features.Characters.Runtime.Shared.Movement.3D."))
                return HasAnyCapability(contract.Capability, AuthoringCapability.Combat, AuthoringCapability.CombatState, AuthoringCapability.CombatSensors)
                    ? "Combat/3D Pawn"
                    : "Movement/3D";
            if (Contains(sourceName, ".Features.Characters.Runtime.Shared.Components."))
                return "Character / Pawn Gameplay/Shared Components";
            if (Contains(sourceName, ".Features.Characters.Runtime.Shared.Contracts."))
                return "Character / Pawn Gameplay/Runtime Contracts";
            if (Contains(sourceName, ".Features.Characters."))
                return HasAnyCapability(contract.Capability, AuthoringCapability.Combat, AuthoringCapability.CombatState, AuthoringCapability.CombatSensors)
                    ? "Combat/Pawn Modules"
                    : "Character / Pawn Gameplay/Pawn Modules";
            if (Contains(sourceName, ".Presentation.Camera."))
                return "World & Meta/Camera";
            if (Contains(sourceName, ".Presentation.Animation."))
                return "Presentation/Animation";
            if (Contains(sourceName, ".Presentation.Visuals."))
                return "Presentation/Visuals";
            if (Contains(sourceName, ".Features.Feedback."))
                return "Presentation/Feedback";
            if (Contains(sourceName, ".Features.Pickups."))
                return "Interaction/Pickups";
            if (Contains(sourceName, ".Features.Interaction."))
                return "Interaction/Actor Interaction";
            if (Contains(sourceName, ".Features.Hazards."))
                return "World & Meta/Hazards";
            if (Contains(sourceName, ".Features.Scoring."))
                return "Scoring";
            if (Contains(sourceName, ".Features.Settings."))
                return "Core Setup/Settings";
            if (Contains(sourceName, ".Features.Platform.") || Contains(sourceName, ".Core.Local") || Contains(sourceName, ".Data.Definitions.Session") || Contains(sourceName, ".Data.Definitions.GameMode") || Contains(sourceName, ".Data.Config."))
                return "Core Setup/Platform";
            if (Contains(sourceName, ".Core.Navigation."))
                return "Core Setup/Scenes & Menus";
            if (Contains(sourceName, ".Core.Actions.") || Contains(sourceName, ".Data.Definitions.Actions."))
                return "Core Setup/Actions";
            if (Contains(sourceName, ".Data.Profiles."))
                return GetProfileDomain(contract, category, family);
            if (Contains(sourceName, ".Data.Definitions."))
                return GetDefinitionDomain(contract, category, family);

            return GetFallbackGroup(contract.Capability);
        }

        private static string BuildContractPathLeaf(ResolvedAuthoringContract contract, RuntimeCapabilityFamily family)
        {
            string leaf = !string.IsNullOrWhiteSpace(contract.DisplayName)
                ? contract.DisplayName
                : contract.SourceType != null
                    ? contract.SourceType.Name
                    : !string.IsNullOrWhiteSpace(contract.ModuleId)
                        ? contract.ModuleId
                        : family.ToString();

            return NormalizePathSegment(leaf);
        }

        private static string[] BuildRoleTags(ResolvedAuthoringContract contract, RuntimeCapabilityFamily family)
        {
            List<string> tags = new List<string>();
            AddRangeDistinct(tags, contract.RoleTags);

            if (contract.SourceType != null)
            {
                AddDistinct(tags, contract.SourceType.Name);
                if (contract.SourceType.IsInterface)
                    AddDistinct(tags, "InterfaceContract");
                if (typeof(UnityEngine.ScriptableObject).IsAssignableFrom(contract.SourceType))
                    AddDistinct(tags, "ScriptableObject");
                if (typeof(UnityEngine.MonoBehaviour).IsAssignableFrom(contract.SourceType))
                    AddDistinct(tags, "SceneComponent");
            }

            AddDistinct(tags, family.ToString());
            AddDistinct(tags, contract.AuthoringLane);
            AddDistinct(tags, contract.AuthoringCategory);
            AddDistinct(tags, GetFallbackGroup(contract.Capability));

            if (contract.RequiredProfileType != null)
                AddDistinct(tags, contract.RequiredProfileType.Name);
            if (!string.IsNullOrWhiteSpace(contract.SetupNodeId))
                AddDistinct(tags, contract.SetupNodeId);
            if (!string.IsNullOrWhiteSpace(contract.ModuleId))
                AddDistinct(tags, contract.ModuleId);

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
            if (Contains(sourceName, ".Core.ContractInterfaces."))
                return false;
            if (Contains(sourceName, ".Tests."))
                return false;

            return true;
        }

        private static string GetRpgSubdomain(ResolvedAuthoringContract contract)
        {
            string name = contract.SourceType != null ? contract.SourceType.Name : contract.DisplayName;
            if (Contains(name, "Dialogue")) return "Dialogue";
            if (Contains(name, "Quest")) return "Quests";
            if (Contains(name, "Vendor")) return "Vendors";
            if (Contains(name, "Skill")) return "Skill Tree";
            if (Contains(name, "Progress")) return "Progression";
            if (Contains(name, "Inventory") || Contains(name, "Item") || Contains(name, "Equipment")) return "Inventory";
            if (Contains(name, "Npc")) return "NPC";
            return "RPG";
        }

        private static string GetBoardSubdomain(ResolvedAuthoringContract contract)
        {
            string name = contract.SourceType != null ? contract.SourceType.Name : contract.DisplayName;
            if (Contains(name, "Turn") || Contains(name, "Phase")) return "Turn Based";
            if (Contains(name, "Board") || Contains(name, "Grid")) return "Board & Grid";
            if (Contains(name, "Piece")) return "Board Pieces";
            return "Rules";
        }

        private static string GetCombatSubdomain(ResolvedAuthoringContract contract)
        {
            AuthoringCapability capability = contract.Capability;
            string name = contract.SourceType != null ? contract.SourceType.Name : contract.DisplayName;
            if (HasAnyCapability(capability, AuthoringCapability.CombatSensors) || Contains(name, "HitBox") || Contains(name, "Detection"))
                return "Sensors";
            if (HasAnyCapability(capability, AuthoringCapability.CombatState) || Contains(name, "Health") || Contains(name, "Status"))
                return "State";
            if (HasAnyCapability(capability, AuthoringCapability.RangedFlow) || Contains(name, "Projectile") || Contains(name, "Fire"))
                return "Ranged";
            if (HasAnyCapability(capability, AuthoringCapability.MeleeFlow) || Contains(name, "Weapon") || Contains(name, "Sequence"))
                return "Melee";
            return "Core";
        }

        private static string GetEnemySubdomain(ResolvedAuthoringContract contract)
        {
            string name = contract.SourceType != null ? contract.SourceType.Name : contract.DisplayName;
            if (Contains(name, "Movement")) return "Movement";
            if (Contains(name, "Detection")) return "Detection";
            if (Contains(name, "Combat")) return "Combat";
            if (Contains(name, "Animation")) return "Animation";
            if (Contains(name, "Ambient")) return "Ambient";
            return "Enemy Runtime";
        }

        private static string GetProfileDomain(ResolvedAuthoringContract contract, string category, RuntimeCapabilityFamily family)
        {
            string name = contract.SourceType != null ? contract.SourceType.Name : contract.DisplayName;
            if (Contains(name, "Input")) return "Core Setup/Input";
            if (Contains(name, "Camera")) return "World & Meta/Camera";
            if (Contains(name, "Movement")) return "Movement/Profiles";
            if (Contains(name, "Traversal") || Contains(name, "Hop")) return "Movement/Traversal";
            if (Contains(name, "Animation")) return "Presentation/Animation";
            if (Contains(name, "Presentation") || Contains(name, "Feedback")) return "Presentation/Profiles";
            if (Contains(name, "Combat") || Contains(name, "Status") || Contains(name, "Reaction")) return "Combat/Profiles";
            if (Contains(name, "Enemy")) return "NPC & AI/Profiles";
            if (Contains(name, "Hazard")) return "World & Meta/Hazards";
            if (Contains(name, "Pickup") || Contains(name, "Interaction")) return "Interaction/Profiles";
            if (Contains(name, "Playfield")) return "World & Meta/Playfield";
            if (Contains(name, "Settings")) return "Core Setup/Settings";
            return GetFamilyDisplayName(family);
        }

        private static string GetDefinitionDomain(ResolvedAuthoringContract contract, string category, RuntimeCapabilityFamily family)
        {
            string name = contract.SourceType != null ? contract.SourceType.Name : contract.DisplayName;
            if (Contains(name, "Session") || Contains(name, "GameMode") || Contains(name, "Participant"))
                return "Core Setup/Definitions";
            if (Contains(name, "Pawn"))
                return "Character / Pawn Gameplay/Definitions";
            return GetFamilyDisplayName(family) + "/Definitions";
        }

        private static string GetFallbackCapabilityPath(PyralisCapabilityVocabularyCard card, PyralisAuthoringFact fact)
        {
            string group = GetFallbackGroup(fact != null ? fact.Capability : AuthoringCapability.None);
            if (card != null && !string.IsNullOrWhiteSpace(card.DisplayName))
                return group + "/" + NormalizePathSegment(card.DisplayName);

            return fact != null && !string.IsNullOrWhiteSpace(fact.DisplayName)
                ? group + "/" + NormalizePathSegment(fact.DisplayName)
                : string.Empty;
        }

        private static string NormalizePathSegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "General";

            string normalized = value.Replace('.', '/').Replace('-', ' ').Replace('_', ' ');
            string[] parts = normalized.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string last = parts.Length > 0 ? parts[parts.Length - 1] : normalized;
            return AuthoringCapabilityRegistry.PrettifyTypeName(last.Trim());
        }

        private static string GetGroup(PyralisAuthoringFact fact)
        {
            if (fact == null)
                return "General";

            if (fact.GoalTags != null && fact.GoalTags.Length > 0)
                return fact.GoalTags[0];

            if (fact.LaneTags != null && fact.LaneTags.Length > 0)
                return fact.LaneTags[0];

            return GetFallbackGroup(fact.Capability);
        }

        private static string GetFallbackGroup(AuthoringCapability capability)
        {
            if (HasAnyCapability(capability, AuthoringCapability.Setup, AuthoringCapability.Session, AuthoringCapability.Rules, AuthoringCapability.Participants, AuthoringCapability.Scoring, AuthoringCapability.Input, AuthoringCapability.UI, AuthoringCapability.Audio))
                return "Core Setup";
            if (HasAnyCapability(capability, AuthoringCapability.Movement, AuthoringCapability.KineticMotor2D, AuthoringCapability.KineticMotor3D, AuthoringCapability.Steering2D, AuthoringCapability.Steering3D, AuthoringCapability.Traversal, AuthoringCapability.Combat, AuthoringCapability.CombatState, AuthoringCapability.CombatSensors, AuthoringCapability.MeleeFlow, AuthoringCapability.RangedFlow, AuthoringCapability.TacticsAggressive, AuthoringCapability.TacticsDefensive, AuthoringCapability.Animation, AuthoringCapability.VFX))
                return "Actor & Action";
            if (HasAnyCapability(capability, AuthoringCapability.Stats, AuthoringCapability.Inventory, AuthoringCapability.Dialogue, AuthoringCapability.Quests, AuthoringCapability.Vendors, AuthoringCapability.SkillTree, AuthoringCapability.Progression, AuthoringCapability.Tabletop, AuthoringCapability.Grid))
                return "RPG & Narrative";
            return "World & Meta";
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
                    "contract NativeSetup fallback",
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

        private static string GetProofTargetId(PyralisAuthoringFact fact)
        {
            if (fact == null || fact.RelatedStableIds == null)
                return string.Empty;

            for (int i = 0; i < fact.RelatedStableIds.Length; i++)
            {
                string id = fact.RelatedStableIds[i];
                if (!string.IsNullOrWhiteSpace(id) && id.StartsWith("proof.", StringComparison.Ordinal))
                    return id;
            }

            return string.Empty;
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

        private static bool HasAnyCapability(AuthoringCapability selected, params AuthoringCapability[] candidates)
        {
            for (int i = 0; i < candidates.Length; i++)
            {
                if ((selected & candidates[i]) != 0)
                    return true;
            }

            return false;
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

        private static string[] Combine(params string[][] groups)
        {
            List<string> values = new List<string>();
            for (int i = 0; i < groups.Length; i++)
                AddRangeDistinct(values, groups[i]);

            return values.ToArray();
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
}
