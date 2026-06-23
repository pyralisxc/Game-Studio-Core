using System;
using System.Collections.Generic;
using System.Linq;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Editor
{
    public enum PyralisAuthoringIntentRowState
    {
        Recommended,
        Related,
        Caution
    }

    public enum PyralisAuthoringIntentGuideTier
    {
        Primary,
        SuggestedNext,
        OptionalEnhancer,
        Caution
    }

    public enum PyralisIntentParticipantRoute
    {
        InferFromSetup,
        SoloLocal,
        TwoLocalPlayers,
        ThreeLocalPlayers,
        FourLocalPlayers,
        Networked,
        HybridLocalNetworked
    }

    public sealed class PyralisAuthoringIntentSelection
    {
        public PyralisAuthoringIntentSelection(
            RuntimeCapabilityLaneTag lane,
            AuthoringCapability capabilities,
            AuthoringWorldAxiom axioms,
            string[] descriptorIds = null,
            PyralisIntentParticipantRoute participantRoute = PyralisIntentParticipantRoute.InferFromSetup)
        {
            Lane = lane;
            Capabilities = capabilities;
            Axioms = axioms;
            DescriptorIds = descriptorIds ?? Array.Empty<string>();
            ParticipantRoute = participantRoute;
        }

        public RuntimeCapabilityLaneTag Lane { get; }
        public AuthoringCapability Capabilities { get; }
        public AuthoringWorldAxiom Axioms { get; }
        public string[] DescriptorIds { get; }
        public PyralisIntentParticipantRoute ParticipantRoute { get; }
    }

    public sealed class PyralisAuthoringIntentRow
    {
        public PyralisAuthoringIntentRow(
            PyralisAuthoringFact fact,
            int score,
            PyralisAuthoringIntentRowState state,
            string reason,
            PyralisAuthoringIntentGuideTier tier = PyralisAuthoringIntentGuideTier.SuggestedNext)
        {
            Fact = fact;
            Score = score;
            State = state;
            Reason = reason ?? string.Empty;
            Tier = tier;
        }

        public PyralisAuthoringFact Fact { get; }
        public int Score { get; }
        public PyralisAuthoringIntentRowState State { get; }
        public string Reason { get; }
        public PyralisAuthoringIntentGuideTier Tier { get; }
    }

    public sealed class PyralisAuthoringIntentModel
    {
        public PyralisAuthoringIntentModel(
            string summary,
            string shapeSummary,
            string proofFocusLabel,
            string proofFocusDetail,
            string proofFocusSummary,
            IReadOnlyList<PyralisAuthoringIntentRow> recommendations,
            IReadOnlyList<PyralisAuthoringIntentRow> cautions,
            IReadOnlyList<PyralisAuthoringFact> matchingIntents)
        {
            Summary = summary ?? string.Empty;
            ShapeSummary = shapeSummary ?? string.Empty;
            ProofFocusLabel = proofFocusLabel ?? string.Empty;
            ProofFocusDetail = proofFocusDetail ?? string.Empty;
            ProofFocusSummary = proofFocusSummary ?? string.Empty;
            Recommendations = recommendations ?? Array.Empty<PyralisAuthoringIntentRow>();
            Cautions = cautions ?? Array.Empty<PyralisAuthoringIntentRow>();
            MatchingIntents = matchingIntents ?? Array.Empty<PyralisAuthoringFact>();
        }

        public string Summary { get; }
        public string ShapeSummary { get; }
        public string ProofFocusLabel { get; }
        public string ProofFocusDetail { get; }
        public string ProofFocusSummary { get; }
        public IReadOnlyList<PyralisAuthoringIntentRow> Recommendations { get; }
        public IReadOnlyList<PyralisAuthoringIntentRow> Cautions { get; }
        public IReadOnlyList<PyralisAuthoringFact> MatchingIntents { get; }
    }

    public static class PyralisAuthoringGuidance
    {
        public const string RelatedByIntent = "Related by the selected route intent.";
        public const string MatchesCapabilities = "Matches the selected capability ingredients.";
        public const string MatchesLane = "Matches the selected lane.";
        public const string GeneralReflectiveFact = "Relevant reflective authoring fact.";
        public const string CautionAgainstLane = "Useful context, but this fact cautions against {0}.";
        public const string MatchingIntentSummary = "Active focus currently resembles {0} for {1}. DNA Axioms provide {2} grounding.";
        public const string AxiomFoundationSummary = "DNA Axioms define the project as {0}. Capability ingredients: {1}.";
    }

    public static class PyralisAuthoringIntentAdvisor
    {
        public static PyralisAuthoringIntentModel Build(PyralisAuthoringIntentSelection selection)
        {
            return Build(selection, PyralisAuthoringGrammarRegistry.AllFacts);
        }

        public static PyralisAuthoringIntentModel Build(PyralisAuthoringIntentSelection selection, IReadOnlyList<PyralisAuthoringFact> facts)
        {
            selection ??= new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.None,
                AuthoringWorldAxiom.None);
            facts ??= Array.Empty<PyralisAuthoringFact>();

            List<PyralisAuthoringFact> matchingIntents = FindMatchingIntentFacts(selection, facts);
            HashSet<string> relatedStableIds = BuildRelatedStableIdSet(matchingIntents);
            HashSet<string> visibleDescriptorFactIds = BuildVisibleDescriptorFactIdSet(selection);
            List<PyralisAuthoringIntentRow> recommendations = new List<PyralisAuthoringIntentRow>();
            List<PyralisAuthoringIntentRow> cautions = new List<PyralisAuthoringIntentRow>();

            for (int i = 0; i < facts.Count; i++)
            {
                PyralisAuthoringFact fact = facts[i];
                if (fact == null || !IsIntentVisibleKind(fact.Kind))
                    continue;

                bool unsupported = HasUnsupportedLane(fact, selection.Lane);
                bool axiomContradiction = IsAxiomContradiction(selection.Axioms, fact.Axioms);
                int score = ScoreFact(selection, fact, relatedStableIds, unsupported);
                if (!IsRouteRelevantFact(selection, fact, score, relatedStableIds, visibleDescriptorFactIds))
                    continue;
                
                if (score <= 0 && !unsupported && !HasCapabilityOverlap(selection, fact) && !HasGoalOverlap(selection, fact))
                    continue;

                if (axiomContradiction && fact.Kind == PyralisAuthoringFactKind.RouteIntent)
                    continue;

                if (unsupported && (score > 0 || HasCapabilityOverlap(selection, fact) || HasGoalOverlap(selection, fact)))
                {
                    cautions.Add(new PyralisAuthoringIntentRow(
                        fact,
                        score,
                        PyralisAuthoringIntentRowState.Caution,
                        string.Format(PyralisAuthoringGuidance.CautionAgainstLane, selection.Lane),
                        PyralisAuthoringIntentGuideTier.Caution));
                    continue;
                }

                if (score <= 0 && !HasCapabilityOverlap(selection, fact) && !HasGoalOverlap(selection, fact))
                    continue;

                recommendations.Add(new PyralisAuthoringIntentRow(
                    fact,
                    score,
                    relatedStableIds.Contains(fact.StableId) ? PyralisAuthoringIntentRowState.Related : PyralisAuthoringIntentRowState.Recommended,
                    BuildReason(selection, fact, relatedStableIds),
                    GetTier(selection, fact, score, relatedStableIds)));
            }

            SortRows(recommendations);
            SortRows(cautions);

            return new PyralisAuthoringIntentModel(
                BuildSummary(selection, matchingIntents),
                BuildShapeSummary(selection, matchingIntents),
                BuildProofFocusLabel(selection),
                BuildProofFocusDetail(selection),
                BuildProofFocusSummary(selection),
                recommendations,
                cautions,
                matchingIntents);
        }

        private static List<PyralisAuthoringFact> FindMatchingIntentFacts(PyralisAuthoringIntentSelection selection, IReadOnlyList<PyralisAuthoringFact> facts)
        {
            List<ScoredIntentFact> matches = new List<ScoredIntentFact>();
            for (int i = 0; i < facts.Count; i++)
            {
                PyralisAuthoringFact fact = facts[i];
                if (fact == null || fact.Kind != PyralisAuthoringFactKind.RouteIntent)
                    continue;

                if (IsAxiomContradiction(selection.Axioms, fact.Axioms))
                    continue;

                int score = ScoreFact(selection, fact, new HashSet<string>(StringComparer.Ordinal), false);
                if (score >= 40)
                    matches.Add(new ScoredIntentFact(fact, score));
            }

            matches.Sort((left, right) =>
            {
                int scoreCompare = right.Score.CompareTo(left.Score);
                return scoreCompare != 0
                    ? scoreCompare
                    : string.Compare(left.Fact.DisplayName, right.Fact.DisplayName, StringComparison.Ordinal);
            });

            List<PyralisAuthoringFact> factsOnly = new List<PyralisAuthoringFact>();
            int count = Math.Min(matches.Count, 3);
            for (int i = 0; i < count; i++)
                factsOnly.Add(matches[i].Fact);

            return factsOnly;
        }

        private static int ScoreFact(PyralisAuthoringIntentSelection selection, PyralisAuthoringFact fact, HashSet<string> relatedStableIds, bool unsupported)
        {
            // Dynamic Priority calculation: P_final = P_base + (N_matches * 50) - (N_clashes * 25)
            int score = fact.Priority; 
            
            if (fact.Kind == PyralisAuthoringFactKind.RouteIntent)
                score += 20;

            if (HasLane(fact, selection.Lane))
                score += 35;
            else if (fact.LaneTags.Length > 0)
                score -= 15;

            // Axiom/DNA Matching
            if (selection.Axioms != AuthoringWorldAxiom.None && fact.Axioms != AuthoringWorldAxiom.None)
            {
                int matches = CountAxiomOverlap(selection.Axioms, fact.Axioms);
                int clashes = IsAxiomContradiction(selection.Axioms, fact.Axioms) ? 1 : 0;
                
                score += (matches * 50);
                score -= (clashes * 25);
            }

            // Capability alignment
            if (selection.Capabilities != AuthoringCapability.None)
            {
                int capabilityOverlap = CountCapabilityMatches(selection.Capabilities, fact.Capability);
                score += capabilityOverlap * 20;
                score += CountGoalMatches(selection, fact) * 10;
            }

            if (relatedStableIds.Contains(fact.StableId))
                score += 30;

            if (unsupported)
                score -= 40;

            return score;
        }

        private static PyralisAuthoringIntentGuideTier GetTier(
            PyralisAuthoringIntentSelection selection,
            PyralisAuthoringFact fact,
            int score,
            HashSet<string> relatedStableIds)
        {
            if (relatedStableIds.Contains(fact.StableId) || fact.Kind == PyralisAuthoringFactKind.RouteIntent || score >= 85)
                return PyralisAuthoringIntentGuideTier.Primary;

            if (HasCapabilityOverlap(selection, fact) || HasGoalOverlap(selection, fact) || score >= 55)
                return PyralisAuthoringIntentGuideTier.SuggestedNext;

            return PyralisAuthoringIntentGuideTier.OptionalEnhancer;
        }

        private static string BuildSummary(PyralisAuthoringIntentSelection selection, IReadOnlyList<PyralisAuthoringFact> matchingIntents)
        {
            if (matchingIntents != null && matchingIntents.Count > 0)
                return string.Format(PyralisAuthoringGuidance.MatchingIntentSummary, JoinFactNames(matchingIntents), selection.Lane, selection.Axioms);

            return string.Format(PyralisAuthoringGuidance.AxiomFoundationSummary, selection.Axioms, selection.Capabilities);
        }

        private static string BuildShapeSummary(PyralisAuthoringIntentSelection selection, IReadOnlyList<PyralisAuthoringFact> matchingIntents)
        {
            string focus = matchingIntents != null && matchingIntents.Count > 0
                ? JoinFactNames(matchingIntents)
                : "Custom route";
            return $"Shape: {selection.Lane} / {GetParticipantRouteDisplayName(selection.ParticipantRoute)} / {focus}";
        }

        private static string BuildProofFocusLabel(PyralisAuthoringIntentSelection selection)
        {
            string proofId = ResolveProofFocusId(selection);
            PyralisAuthoringFact proof = PyralisAuthoringGrammarRegistry.AllFacts.FirstOrDefault(fact =>
                fact != null && string.Equals(fact.StableId, proofId, StringComparison.Ordinal));
            return proof != null && !string.IsNullOrWhiteSpace(proof.DisplayName)
                ? proof.DisplayName
                : "First Proof";
        }

        private static string BuildProofFocusDetail(PyralisAuthoringIntentSelection selection)
        {
            string proofId = ResolveProofFocusId(selection);
            if (string.Equals(proofId, "proof.local-pawn-join", StringComparison.Ordinal))
                return "Target route focus is local pawn join. Single-pawn movement recommendations are foundation pieces, not a replacement for the local-player proof.";

            if (string.Equals(proofId, "proof.1p-pawn-movement", StringComparison.Ordinal))
                return "Target route focus is one responsive pawn. Add broader multiplayer, combat, UI, or scene features after this movement proof is honest.";

            PyralisAuthoringFact proof = PyralisAuthoringGrammarRegistry.AllFacts.FirstOrDefault(fact =>
                fact != null && string.Equals(fact.StableId, proofId, StringComparison.Ordinal));
            return proof != null && !string.IsNullOrWhiteSpace(proof.Summary)
                ? proof.Summary
                : "Target route focus follows the selected lane, participant route, and gameplay ingredients.";
        }

        private static string BuildProofFocusSummary(PyralisAuthoringIntentSelection selection)
        {
            string label = BuildProofFocusLabel(selection);
            string detail = BuildProofFocusDetail(selection);
            return string.IsNullOrWhiteSpace(detail)
                ? "Target proof: " + label
                : "Target proof: " + label + ". " + detail;
        }

        private static string ResolveProofFocusId(PyralisAuthoringIntentSelection selection)
        {
            RuntimeCapabilityFamily[] families = BuildIntentRuntimeFamilies(selection);
            bool requiresPawn = families.Any(family => family == RuntimeCapabilityFamily.CharacterPawnGameplay);
            return PyralisProofFamilyVocabulary.GetGenericProofTargetId(
                families,
                requiresPawn,
                GetParticipantTopology(selection?.ParticipantRoute ?? PyralisIntentParticipantRoute.InferFromSetup));
        }

        private static RuntimeCapabilityFamily[] BuildIntentRuntimeFamilies(PyralisAuthoringIntentSelection selection)
        {
            if (selection == null)
                return Array.Empty<RuntimeCapabilityFamily>();

            if (selection.DescriptorIds != null && selection.DescriptorIds.Length > 0)
            {
                RuntimeCapabilityFamily[] descriptorFamilies =
                    PyralisAuthoringCapabilityDescriptorRegistry.BuildRuntimeFamiliesForDescriptors(
                        selection.DescriptorIds,
                        selection.Lane,
                        selection.Axioms);
                if (descriptorFamilies.Length > 0)
                    return descriptorFamilies;
            }

            return PyralisAuthoringCapabilityDescriptorRegistry.BuildRuntimeFamilies(
                selection.Capabilities,
                selection.Lane,
                selection.Axioms);
        }

        private static PyralisParticipantTopology GetParticipantTopology(PyralisIntentParticipantRoute route)
        {
            switch (route)
            {
                case PyralisIntentParticipantRoute.TwoLocalPlayers:
                case PyralisIntentParticipantRoute.ThreeLocalPlayers:
                case PyralisIntentParticipantRoute.FourLocalPlayers:
                    return PyralisParticipantTopology.LocalJoin;
                case PyralisIntentParticipantRoute.HybridLocalNetworked:
                    return PyralisParticipantTopology.HybridLocalNetworked;
                default:
                    return PyralisParticipantTopology.Unknown;
            }
        }

        private static string GetParticipantRouteDisplayName(PyralisIntentParticipantRoute route)
        {
            switch (route)
            {
                case PyralisIntentParticipantRoute.SoloLocal:
                    return "Solo Local";
                case PyralisIntentParticipantRoute.TwoLocalPlayers:
                    return "2 Local Players";
                case PyralisIntentParticipantRoute.ThreeLocalPlayers:
                    return "3 Local Players";
                case PyralisIntentParticipantRoute.FourLocalPlayers:
                    return "4 Local Players";
                case PyralisIntentParticipantRoute.Networked:
                    return "Networked";
                case PyralisIntentParticipantRoute.HybridLocalNetworked:
                    return "Hybrid Local + Network";
                default:
                    return "Infer From Setup";
            }
        }

        private static string BuildReason(PyralisAuthoringIntentSelection selection, PyralisAuthoringFact fact, HashSet<string> relatedStableIds)
        {
            if (relatedStableIds.Contains(fact.StableId))
                return PyralisAuthoringGuidance.RelatedByIntent;

            if (HasCapabilityOverlap(selection, fact) || HasGoalOverlap(selection, fact))
                return PyralisAuthoringGuidance.MatchesCapabilities;

            if (HasLane(fact, selection.Lane))
                return PyralisAuthoringGuidance.MatchesLane;

            return PyralisAuthoringGuidance.GeneralReflectiveFact;
        }

        private static bool IsIntentVisibleKind(PyralisAuthoringFactKind kind)
        {
            return kind == PyralisAuthoringFactKind.RouteIntent
                || kind == PyralisAuthoringFactKind.RuntimeCapability
                || kind == PyralisAuthoringFactKind.FeatureContract
                || kind == PyralisAuthoringFactKind.Proof;
        }

        private static HashSet<string> BuildRelatedStableIdSet(IReadOnlyList<PyralisAuthoringFact> matchingIntents)
        {
            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
            if (matchingIntents == null)
                return ids;

            for (int i = 0; i < matchingIntents.Count; i++)
            {
                PyralisAuthoringFact fact = matchingIntents[i];
                if (fact == null)
                    continue;

                for (int relatedIndex = 0; relatedIndex < fact.RelatedStableIds.Length; relatedIndex++)
                {
                    string relatedId = fact.RelatedStableIds[relatedIndex];
                    if (!string.IsNullOrWhiteSpace(relatedId))
                        ids.Add(relatedId);
                }
            }

            return ids;
        }

        private static HashSet<string> BuildVisibleDescriptorFactIdSet(PyralisAuthoringIntentSelection selection)
        {
            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
            if (selection == null)
                return ids;

            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors =
                PyralisAuthoringCapabilityDescriptorRegistry.BuildIntentDescriptors(selection.Lane, selection.Axioms);
            PyralisAuthoringIntentProjection projection = PyralisAuthoringIntentProjection.Build(selection, descriptors);

            AddDescriptorFacts(projection.SelectedDescriptors);
            AddDescriptorFacts(projection.RouteEssentialGroups
                .SelectMany(group => group.Subgroups)
                .SelectMany(subgroup => subgroup.Descriptors));

            return ids;

            void AddDescriptorFacts(IEnumerable<PyralisAuthoringIntentDescriptorProjection> projectedDescriptors)
            {
                if (projectedDescriptors == null)
                    return;

                foreach (PyralisAuthoringIntentDescriptorProjection projected in projectedDescriptors)
                {
                    PyralisAuthoringCapabilityDescriptor descriptor = projected?.Descriptor;
                    if (descriptor == null)
                        continue;

                    if (!string.IsNullOrWhiteSpace(descriptor.StableId))
                        ids.Add(descriptor.StableId);
                    if (!string.IsNullOrWhiteSpace(descriptor.SourceFact?.StableId))
                        ids.Add(descriptor.SourceFact.StableId);
                    if (descriptor.SourceFact?.RelatedStableIds == null)
                        continue;

                    for (int i = 0; i < descriptor.SourceFact.RelatedStableIds.Length; i++)
                    {
                        string relatedId = descriptor.SourceFact.RelatedStableIds[i];
                        if (!string.IsNullOrWhiteSpace(relatedId))
                            ids.Add(relatedId);
                    }
                }
            }
        }

        private static bool IsRouteRelevantFact(
            PyralisAuthoringIntentSelection selection,
            PyralisAuthoringFact fact,
            int score,
            HashSet<string> relatedStableIds,
            HashSet<string> visibleDescriptorFactIds)
        {
            if (fact == null)
                return false;

            if (fact.Kind == PyralisAuthoringFactKind.RouteIntent)
                return true;

            if (relatedStableIds != null && relatedStableIds.Contains(fact.StableId))
                return true;

            if (visibleDescriptorFactIds != null && visibleDescriptorFactIds.Contains(fact.StableId))
                return true;

            if (fact.Kind == PyralisAuthoringFactKind.Proof)
                return score >= 180 && (HasCapabilityOverlap(selection, fact) || HasGoalOverlap(selection, fact));

            return false;
        }

        private static int CountAxiomOverlap(AuthoringWorldAxiom selection, AuthoringWorldAxiom fact)
        {
            AuthoringWorldAxiom overlap = selection & fact;
            if (overlap == AuthoringWorldAxiom.None) return 0;

            int count = 0;
            ulong value = (ulong)overlap;
            while (value != 0)
            {
                value &= (value - 1);
                count++;
            }
            return count;
        }

        private static int CountCapabilityMatches(AuthoringCapability selection, AuthoringCapability fact)
        {
            AuthoringCapability overlap = selection & fact;
            if (overlap == AuthoringCapability.None) return 0;

            int count = 0;
            ulong value = (ulong)overlap;
            while (value != 0)
            {
                value &= (value - 1);
                count++;
            }
            return count;
        }

        private static bool HasCapabilityOverlap(PyralisAuthoringIntentSelection selection, PyralisAuthoringFact fact)
        {
            return (selection.Capabilities & fact.Capability) != AuthoringCapability.None;
        }

        private static bool IsAxiomContradiction(AuthoringWorldAxiom selection, AuthoringWorldAxiom fact)
        {
            if (HasAxiom(selection, AuthoringWorldAxiom.Dimensions2D) && HasAxiom(fact, AuthoringWorldAxiom.Dimensions3D)) return true;
            if (HasAxiom(selection, AuthoringWorldAxiom.Dimensions3D) && HasAxiom(fact, AuthoringWorldAxiom.Dimensions2D)) return true;
            if (HasAxiom(selection, AuthoringWorldAxiom.Realtime) && HasAxiom(fact, AuthoringWorldAxiom.TurnBased)) return true;
            if (HasAxiom(selection, AuthoringWorldAxiom.TurnBased) && HasAxiom(fact, AuthoringWorldAxiom.Realtime)) return true;
            if (HasAxiom(selection, AuthoringWorldAxiom.GravityNone) && HasAxiom(fact, AuthoringWorldAxiom.GravityVertical)) return true;
            if (HasAxiom(selection, AuthoringWorldAxiom.GravityVertical) && HasAxiom(fact, AuthoringWorldAxiom.GravityNone)) return true;
            
            return false;
        }

        private static bool HasAxiom(AuthoringWorldAxiom flags, AuthoringWorldAxiom target)
        {
            return (flags & target) != 0;
        }

        private static bool HasLane(PyralisAuthoringFact fact, RuntimeCapabilityLaneTag lane)
        {
            return Contains(fact.LaneTags, lane.ToString()) || Contains(fact.LaneTags, ToPresentationModeLaneName(lane));
        }

        private static bool HasUnsupportedLane(PyralisAuthoringFact fact, RuntimeCapabilityLaneTag lane)
        {
            return Contains(fact.UnsupportedLaneTags, lane.ToString()) || Contains(fact.UnsupportedLaneTags, ToPresentationModeLaneName(lane));
        }

        private static string ToPresentationModeLaneName(RuntimeCapabilityLaneTag lane)
        {
            return lane switch
            {
                RuntimeCapabilityLaneTag.Sprite2D => "Sprite2D",
                RuntimeCapabilityLaneTag.Billboard2_5D => "Billboard2_5D",
                RuntimeCapabilityLaneTag.ThirdPerson3D => "Rigged3D",
                _ => lane.ToString()
            };
        }

        private static int CountGoalMatches(PyralisAuthoringIntentSelection selection, PyralisAuthoringFact fact)
        {
            if (fact.GoalTags == null || fact.GoalTags.Length == 0)
                return 0;

            int count = 0;
            if (HasCapabilityGoal(selection, fact, AuthoringCapability.Movement, "Movement"))
                count++;
            if (HasCapabilityGoal(selection, fact, AuthoringCapability.Combat, "Combat", "MeleeFlow", "RangedFlow", "Projectiles"))
                count++;
            if (HasCapabilityGoal(selection, fact, AuthoringCapability.Input, "Input"))
                count++;
            if (HasCapabilityGoal(selection, fact, AuthoringCapability.Animation, "AnimationPresentation"))
                count++;
            if (HasCapabilityGoal(selection, fact, AuthoringCapability.Camera, "Camera"))
                count++;
            if (HasCapabilityGoal(selection, fact, AuthoringCapability.Tabletop, "Tabletop"))
                count++;
            if (HasCapabilityGoal(selection, fact, AuthoringCapability.UI, "UiHud", "UI"))
                count++;
            if (HasCapabilityGoal(selection, fact, AuthoringCapability.Networking, "Networking"))
                count++;

            return count;
        }

        private static bool HasGoalOverlap(PyralisAuthoringIntentSelection selection, PyralisAuthoringFact fact)
        {
            return CountGoalMatches(selection, fact) > 0;
        }

        private static bool HasCapabilityGoal(PyralisAuthoringIntentSelection selection, PyralisAuthoringFact fact, AuthoringCapability capability, params string[] goals)
        {
            if ((selection.Capabilities & capability) == 0 || goals == null)
                return false;

            for (int i = 0; i < goals.Length; i++)
            {
                if (Contains(fact.GoalTags, goals[i]))
                    return true;
            }

            return false;
        }

        private static bool Contains(string[] values, string expected)
{
            if (values == null || string.IsNullOrWhiteSpace(expected))
                return false;

            for (int i = 0; i < values.Length; i++)
            {
                string val = values[i];
                if (string.Equals(val, expected, StringComparison.OrdinalIgnoreCase))
                    return true;

                // Hierarchical match: 
                // - A tag like 'Combat/Reaction' matches a search for 'Combat'
                // - A tag like 'Combat' matches a search for 'Combat/Reaction' (as a parent category)
                if (val != null && (val.StartsWith(expected + "/", StringComparison.OrdinalIgnoreCase) ||
                                   expected.StartsWith(val + "/", StringComparison.OrdinalIgnoreCase)))
                    return true;
            }

            return false;
        }

        private static void SortRows(List<PyralisAuthoringIntentRow> rows)
        {
            rows.Sort((left, right) =>
            {
                int scoreCompare = right.Score.CompareTo(left.Score);
                return scoreCompare != 0
                    ? scoreCompare
                    : string.Compare(left.Fact.DisplayName, right.Fact.DisplayName, StringComparison.Ordinal);
            });
        }

        private static void SortFactsByDisplayName(List<PyralisAuthoringFact> facts)
        {
            facts.Sort((left, right) => string.Compare(left.DisplayName, right.DisplayName, StringComparison.Ordinal));
        }

        private sealed class ScoredIntentFact
        {
            public ScoredIntentFact(PyralisAuthoringFact fact, int score)
            {
                Fact = fact;
                Score = score;
            }

            public PyralisAuthoringFact Fact { get; }
            public int Score { get; }
        }

        private static string JoinFactNames(IReadOnlyList<PyralisAuthoringFact> facts)
        {
            if (facts == null || facts.Count == 0)
                return "a custom route";

            if (facts.Count == 1)
                return facts[0].DisplayName;

            List<string> names = new List<string>();
            for (int i = 0; i < facts.Count; i++)
                names.Add(facts[i].DisplayName);

            return string.Join(" + ", names);
        }
    }
}
