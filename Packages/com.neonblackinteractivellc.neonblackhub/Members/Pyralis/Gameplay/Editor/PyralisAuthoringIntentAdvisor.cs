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

    public sealed class PyralisAuthoringIntentSelection
    {
        public PyralisAuthoringIntentSelection(RuntimeCapabilityLaneTag lane, AuthoringCapability capabilities, AuthoringWorldAxiom axioms)
        {
            Lane = lane;
            Capabilities = capabilities;
            Axioms = axioms;
        }

        public RuntimeCapabilityLaneTag Lane { get; }
        public AuthoringCapability Capabilities { get; }
        public AuthoringWorldAxiom Axioms { get; }
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
            IReadOnlyList<PyralisAuthoringIntentRow> recommendations,
            IReadOnlyList<PyralisAuthoringIntentRow> cautions,
            IReadOnlyList<PyralisAuthoringFact> matchingIntents,
            IReadOnlyList<PyralisAuthoringIssue> hygieneIssues = null)
        {
            Summary = summary ?? string.Empty;
            Recommendations = recommendations ?? Array.Empty<PyralisAuthoringIntentRow>();
            Cautions = cautions ?? Array.Empty<PyralisAuthoringIntentRow>();
            MatchingIntents = matchingIntents ?? Array.Empty<PyralisAuthoringFact>();
            HygieneIssues = hygieneIssues ?? Array.Empty<PyralisAuthoringIssue>();
        }

        public string Summary { get; }
        public IReadOnlyList<PyralisAuthoringIntentRow> Recommendations { get; }
        public IReadOnlyList<PyralisAuthoringIntentRow> Cautions { get; }
        public IReadOnlyList<PyralisAuthoringFact> MatchingIntents { get; }
        public IReadOnlyList<PyralisAuthoringIssue> HygieneIssues { get; }
    }

    public static class PyralisAuthoringGuidance
    {
        public const string RelatedByIntent = "Related by the selected route intent.";
        public const string MatchesCapabilities = "Matches the selected Spine capabilities.";
        public const string MatchesLane = "Matches the selected lane.";
        public const string GeneralReflectiveFact = "Relevant reflective authoring fact.";
        public const string CautionAgainstLane = "Useful context, but this fact cautions against {0}.";
        public const string MatchingIntentSummary = "Active focus currently resembles {0} for {1}. DNA Axioms provide {2} grounding.";
        public const string AxiomFoundationSummary = "Project DNA is defined by {0}. Engine Spine capabilities: {1}.";
    }

    public static class PyralisAuthoringIntentAdvisor
    {
        public static PyralisAuthoringIntentModel Build(PyralisAuthoringIntentSelection selection)
        {
            return Build(selection, PyralisAuthoringFactRegistry.AllFacts);
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
            List<PyralisAuthoringIntentRow> recommendations = new List<PyralisAuthoringIntentRow>();
            List<PyralisAuthoringIntentRow> cautions = new List<PyralisAuthoringIntentRow>();

            for (int i = 0; i < facts.Count; i++)
            {
                PyralisAuthoringFact fact = facts[i];
                if (fact == null || !IsIntentVisibleKind(fact.Kind))
                    continue;

                bool unsupported = HasUnsupportedLane(fact, selection.Lane);
                int score = ScoreFact(selection, fact, relatedStableIds, unsupported);
                
                if (score <= 0 && !unsupported && !HasCapabilityOverlap(selection, fact))
                    continue;

                if (unsupported && (score > 0 || HasCapabilityOverlap(selection, fact)))
                {
                    cautions.Add(new PyralisAuthoringIntentRow(
                        fact,
                        score,
                        PyralisAuthoringIntentRowState.Caution,
                        string.Format(PyralisAuthoringGuidance.CautionAgainstLane, selection.Lane),
                        PyralisAuthoringIntentGuideTier.Caution));
                    continue;
                }

                if (score <= 0 && !HasCapabilityOverlap(selection, fact))
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

            List<PyralisAuthoringIssue> hygieneIssues = ValidateHygiene(selection, facts, recommendations);

            return new PyralisAuthoringIntentModel(
                BuildSummary(selection, matchingIntents),
                recommendations,
                cautions,
                matchingIntents,
                hygieneIssues);
        }

        private static List<PyralisAuthoringIssue> ValidateHygiene(
            PyralisAuthoringIntentSelection selection,
            IReadOnlyList<PyralisAuthoringFact> allFacts,
            List<PyralisAuthoringIntentRow> recommendations)
        {
            List<PyralisAuthoringIssue> issues = new List<PyralisAuthoringIssue>();

            // 1. Check for Missing Primary Providers for selected capabilities
            foreach (AuthoringCapability cap in AuthoringCapabilityRegistry.GetAllIndividualCapabilities())
            {
                if ((selection.Capabilities & cap) == 0) continue;

                bool hasProvider = recommendations.Any(r => (r.Fact.Capability & cap) != 0);
                if (!hasProvider)
                {
                    issues.Add(new PyralisAuthoringIssue(
                        "HYG001",
                        PyralisAuthoringIssueSeverity.Required,
                        cap.ToString(),
                        PyralisAuthoringEvidenceState.Missing,
                        "Project",
                        "Spine",
                        null,
                        $"Capability '{AuthoringCapabilityRegistry.GetDisplayName(cap)}' is selected but no providing scripts were discovered in the project. {AuthoringCapabilityRegistry.GetHygieneAdvice(cap)}"));
                }
            }

            // 2. Check for Capability Conflicts (Multiple High-Priority Providers)
            var groups = recommendations
                .Where(r => r.Fact.Capability != AuthoringCapability.None)
                .GroupBy(r => r.Fact.Capability);

            foreach (var group in groups)
            {
                var facts = group.ToList();
                if (facts.Count <= 1) continue;

                int maxPriority = facts.Max(f => f.Fact.Priority);
                var topProviders = facts.Where(f => f.Fact.Priority == maxPriority).ToList();

                if (topProviders.Count > 1)
                {
                    string names = string.Join(", ", topProviders.Select(p => p.Fact.DisplayName));
                    issues.Add(new PyralisAuthoringIssue(
                        "HYG002",
                        PyralisAuthoringIssueSeverity.Bug,
                        group.Key.ToString(),
                        PyralisAuthoringEvidenceState.Conflict,
                        "Code",
                        "Duplication",
                        null,
                        $"Multiple primary providers for '{group.Key}' detected: {names}. This indicates potential code duplication or a need to refine Priority in the [AuthoringContract]. Refine Priority to resolve conflict."));
                }
            }

            // 3. Check for Documentation/Doc Truth Hygiene
            foreach (var rec in recommendations)
            {
                if (string.IsNullOrEmpty(rec.Fact.Summary) || rec.Fact.Summary == rec.Fact.DisplayName)
                {
                    issues.Add(new PyralisAuthoringIssue(
                        "HYG003",
                        PyralisAuthoringIssueSeverity.Optional,
                        rec.Fact.DisplayName,
                        PyralisAuthoringEvidenceState.CandidateDetected,
                        "Documentation",
                        "Content",
                        null,
                        $"Contract '{rec.Fact.DisplayName}' is missing a meaningful Summary. Update the [AuthoringContract] attribute with 'Relevance' or 'Summary' text."));
                }

                if (string.IsNullOrEmpty(rec.Fact.DocumentationURL))
                {
                    issues.Add(new PyralisAuthoringIssue(
                        "HYG004",
                        PyralisAuthoringIssueSeverity.Info,
                        rec.Fact.DisplayName,
                        PyralisAuthoringEvidenceState.CandidateDetected,
                        "Documentation",
                        "Source",
                        null,
                        $"Contract '{rec.Fact.DisplayName}' has no Documentation URL. Consider adding a link to the technical wiki in [AuthoringContract]."));
                }

                if (string.IsNullOrEmpty(rec.Fact.ExpertAdvice))
                {
                    issues.Add(new PyralisAuthoringIssue(
                        "HYG005",
                        PyralisAuthoringIssueSeverity.Recommended,
                        rec.Fact.DisplayName,
                        PyralisAuthoringEvidenceState.Missing,
                        "Authoring",
                        "Content",
                        null,
                        $"Contract '{rec.Fact.DisplayName}' is missing Expert Advice. Provide a pro-tip in the [AuthoringContract] to help developers use this feature effectively."));
                }

                if (string.IsNullOrEmpty(rec.Fact.FirstProof))
                {
                    issues.Add(new PyralisAuthoringIssue(
                        "HYG006",
                        PyralisAuthoringIssueSeverity.Recommended,
                        rec.Fact.DisplayName,
                        PyralisAuthoringEvidenceState.Missing,
                        "Authoring",
                        "Validation",
                        null,
                        $"Contract '{rec.Fact.DisplayName}' has no First Proof defined. Describe a simple setup step to verify this feature works in the [AuthoringContract]."));
                }
            }

            return issues;
        }

        private static List<PyralisAuthoringFact> FindMatchingIntentFacts(PyralisAuthoringIntentSelection selection, IReadOnlyList<PyralisAuthoringFact> facts)
        {
            List<ScoredIntentFact> matches = new List<ScoredIntentFact>();
            for (int i = 0; i < facts.Count; i++)
            {
                PyralisAuthoringFact fact = facts[i];
                if (fact == null || fact.Kind != PyralisAuthoringFactKind.RouteIntent)
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
            int score = 0;
            if (fact.Kind == PyralisAuthoringFactKind.RouteIntent)
                score += 20;
            else if (fact.Kind == PyralisAuthoringFactKind.RuntimeCapability)
                score += 18;
            else if (fact.Kind == PyralisAuthoringFactKind.FeatureContract)
                score += 10;
            else if (fact.Kind == PyralisAuthoringFactKind.Proof)
                score += 8;

            if (HasLane(fact, selection.Lane))
                score += 35;
            else if (fact.LaneTags.Length == 0)
                score += 4;
            else
                score -= 8;

            // Typed Capability Scoring
            if (selection.Capabilities != AuthoringCapability.None)
            {
                int capabilityOverlap = CountCapabilityMatches(selection.Capabilities, fact.Capability);
                score += capabilityOverlap * 50; // High weight for Spine alignment
            }

            if (selection.Axioms != AuthoringWorldAxiom.None && fact.Axioms != AuthoringWorldAxiom.None)
            {
                int axiomOverlap = CountAxiomOverlap(selection.Axioms, fact.Axioms);
                score += axiomOverlap * 25; 
                
                if (IsAxiomContradiction(selection.Axioms, fact.Axioms))
                {
                    score -= 50;
                }
            }
            else if (selection.Axioms != AuthoringWorldAxiom.None && fact.Axioms == AuthoringWorldAxiom.None)
            {
                score += 5; 
            }

            // Priority boost for "Primary Providers"
            score += fact.Priority * 15;

            if (relatedStableIds.Contains(fact.StableId))
                score += 28;

            if (unsupported)
                score -= 25;

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

            if (HasCapabilityOverlap(selection, fact) || score >= 55)
                return PyralisAuthoringIntentGuideTier.SuggestedNext;

            return PyralisAuthoringIntentGuideTier.OptionalEnhancer;
        }

        private static string BuildSummary(PyralisAuthoringIntentSelection selection, IReadOnlyList<PyralisAuthoringFact> matchingIntents)
        {
            if (matchingIntents != null && matchingIntents.Count > 0)
                return string.Format(PyralisAuthoringGuidance.MatchingIntentSummary, JoinFactNames(matchingIntents), selection.Lane, selection.Axioms);

            return string.Format(PyralisAuthoringGuidance.AxiomFoundationSummary, selection.Axioms, selection.Capabilities);
        }

        private static string BuildReason(PyralisAuthoringIntentSelection selection, PyralisAuthoringFact fact, HashSet<string> relatedStableIds)
        {
            if (relatedStableIds.Contains(fact.StableId))
                return PyralisAuthoringGuidance.RelatedByIntent;

            if (HasCapabilityOverlap(selection, fact))
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

        private static int CountAxiomOverlap(AuthoringWorldAxiom selection, AuthoringWorldAxiom fact)
        {
            AuthoringWorldAxiom overlap = selection & fact;
            if (overlap == AuthoringWorldAxiom.None) return 0;

            int count = 0;
            uint value = (uint)overlap;
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
            uint value = (uint)overlap;
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
                RuntimeCapabilityLaneTag.Rigged3D => "Rigged3D",
                _ => lane.ToString()
            };
        }

        private static int CountGoalMatches(PyralisAuthoringIntentSelection selection, PyralisAuthoringFact fact)
        {
            if (fact.GoalTags == null || fact.GoalTags.Length == 0)
                return 0;

            // This is a legacy fallback for facts without typed Capabilities
            return 0; 
        }

        private static bool HasGoalOverlap(PyralisAuthoringIntentSelection selection, PyralisAuthoringFact fact)
        {
            return CountGoalMatches(selection, fact) > 0;
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
