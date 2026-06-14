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
        public static IReadOnlyList<PyralisAuthoringCapabilityDescriptor> All
        {
            get
            {
                return BuildDescriptors();
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
                if (descriptor != null && descriptor.Matches(capabilities, lane, axioms))
                    AddDistinct(families, descriptor.Family);
            }

            return families.ToArray();
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
                        "capability." + NormalizeId(family.ToString()),
                        GetFamilyDisplayName(family),
                        family,
                        contract.Capability,
                        GetGroup(contract),
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
                        GetContractSourceOrigin(contract)));
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
                    fact));
            }
        }

        private static void AddOrMerge(List<PyralisAuthoringCapabilityDescriptor> descriptors, PyralisAuthoringCapabilityDescriptor incoming)
        {
            if (incoming == null || string.IsNullOrWhiteSpace(incoming.StableId))
                return;

            for (int i = 0; i < descriptors.Count; i++)
            {
                PyralisAuthoringCapabilityDescriptor current = descriptors[i];
                if (current == null || current.Family != incoming.Family)
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
            bool incomingIsContract = incoming.SourceOrigin == PyralisAuthoringGraphSourceOrigin.Contract
                || incoming.SourceOrigin == PyralisAuthoringGraphSourceOrigin.Reflection;
            bool currentIsContract = current.SourceOrigin == PyralisAuthoringGraphSourceOrigin.Contract
                || current.SourceOrigin == PyralisAuthoringGraphSourceOrigin.Reflection;
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
                current.SourceFact ?? incoming.SourceFact);
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
            if (HasAnyCapability(capability, AuthoringCapability.Camera, AuthoringCapability.Input))
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
            if (string.Equals(lane, "Camera", StringComparison.OrdinalIgnoreCase)
                || string.Equals(lane, "Input", StringComparison.OrdinalIgnoreCase))
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
                RuntimeCapabilityFamily.CameraInput => "Camera / Input",
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
                return "Core";
            if (HasAnyCapability(capability, AuthoringCapability.Movement, AuthoringCapability.KineticMotor2D, AuthoringCapability.KineticMotor3D, AuthoringCapability.Steering2D, AuthoringCapability.Steering3D, AuthoringCapability.Traversal, AuthoringCapability.Combat, AuthoringCapability.CombatState, AuthoringCapability.CombatSensors, AuthoringCapability.MeleeFlow, AuthoringCapability.RangedFlow, AuthoringCapability.TacticsAggressive, AuthoringCapability.TacticsDefensive, AuthoringCapability.Animation, AuthoringCapability.VFX))
                return "Actor";
            if (HasAnyCapability(capability, AuthoringCapability.Stats, AuthoringCapability.Inventory, AuthoringCapability.Dialogue, AuthoringCapability.Quests, AuthoringCapability.Vendors, AuthoringCapability.SkillTree, AuthoringCapability.Progression, AuthoringCapability.Tabletop, AuthoringCapability.Grid))
                return "Progression";
            return "World";
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
                    AddDistinct(tags, AuthoringCapabilityRegistry.GetDisplayName(individual));
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
                actions.Add(new PyralisAuthoringNativeAction(
                    "Configure",
                    PyralisAuthoringActionSurface.Inspector,
                    contract.DisplayName,
                    contract.NativeSetup[i],
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
