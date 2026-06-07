using System;
using System.Collections.Generic;

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

    public enum PyralisAuthoringWorldIntent
    {
        SideView2DGravity,
        TopDown2DPlane,
        Lane2_5D,
        Space3D,
        BoardGridTabletop,
        CardTableSurface,
        UiMenuFirst,
        HybridUnsure
    }

    public enum PyralisAuthoringControlIntent
    {
        PawnActor,
        CursorSelector,
        BoardSeat,
        CardHand,
        Camera,
        MenuCommand,
        FactionTeam,
        Mixed
    }

    public sealed class PyralisAuthoringIntentSelection
    {
        public PyralisAuthoringIntentSelection(RuntimeCapabilityLaneTag lane, RuntimeCapabilityGoalTag[] goals)
            : this(PyralisAuthoringWorldIntent.SideView2DGravity, PyralisAuthoringControlIntent.PawnActor, lane, goals)
        {
        }

        public PyralisAuthoringIntentSelection(
            PyralisAuthoringWorldIntent world,
            PyralisAuthoringControlIntent control,
            RuntimeCapabilityLaneTag lane,
            RuntimeCapabilityGoalTag[] goals)
        {
            World = world;
            Control = control;
            Lane = lane;
            Goals = goals ?? Array.Empty<RuntimeCapabilityGoalTag>();
        }

        public PyralisAuthoringWorldIntent World { get; }
        public PyralisAuthoringControlIntent Control { get; }
        public RuntimeCapabilityLaneTag Lane { get; }
        public RuntimeCapabilityGoalTag[] Goals { get; }
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
            IReadOnlyList<PyralisAuthoringFact> matchingIntents)
        {
            Summary = summary ?? string.Empty;
            Recommendations = recommendations ?? Array.Empty<PyralisAuthoringIntentRow>();
            Cautions = cautions ?? Array.Empty<PyralisAuthoringIntentRow>();
            MatchingIntents = matchingIntents ?? Array.Empty<PyralisAuthoringFact>();
        }

        public string Summary { get; }
        public IReadOnlyList<PyralisAuthoringIntentRow> Recommendations { get; }
        public IReadOnlyList<PyralisAuthoringIntentRow> Cautions { get; }
        public IReadOnlyList<PyralisAuthoringFact> MatchingIntents { get; }
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
                PyralisAuthoringWorldIntent.SideView2DGravity,
                PyralisAuthoringControlIntent.PawnActor,
                RuntimeCapabilityLaneTag.Sprite2D,
                Array.Empty<RuntimeCapabilityGoalTag>());
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
                if (score <= 0 && !unsupported)
                    continue;

                if (unsupported && (score > 0 || HasGoalOverlap(selection, fact)))
                {
                    cautions.Add(new PyralisAuthoringIntentRow(
                        fact,
                        score,
                        PyralisAuthoringIntentRowState.Caution,
                        $"Useful context, but this fact cautions against {selection.Lane}.",
                        PyralisAuthoringIntentGuideTier.Caution));
                    continue;
                }

                if (score <= 0)
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

                bool matchesWorld = HasWorldOverlap(selection, fact);
                bool matchesControl = HasControlOverlap(selection, fact);
                bool isHybridSelection = selection.World == PyralisAuthoringWorldIntent.HybridUnsure
                    || selection.Control == PyralisAuthoringControlIntent.Mixed;

                if (!isHybridSelection && (!matchesWorld || !matchesControl))
                    continue;

                int score = ScoreFact(selection, fact, new HashSet<string>(StringComparer.Ordinal), false);
                if (score >= 60)
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

            if (HasWorldOverlap(selection, fact))
                score += 24;

            if (HasControlOverlap(selection, fact))
                score += 20;

            int goalMatches = CountGoalMatches(selection, fact);
            score += goalMatches * 18;

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

            if (HasGoalOverlap(selection, fact) || score >= 55)
                return PyralisAuthoringIntentGuideTier.SuggestedNext;

            return PyralisAuthoringIntentGuideTier.OptionalEnhancer;
        }

        private static string BuildSummary(PyralisAuthoringIntentSelection selection, IReadOnlyList<PyralisAuthoringFact> matchingIntents)
        {
            string projectShape = $"{GetWorldLabel(selection.World)} with {GetControlLabel(selection.Control)}";
            if (matchingIntents != null && matchingIntents.Count > 0)
                return $"Project intent reads like {projectShape}. Active focus currently resembles {JoinFactNames(matchingIntents)} for {selection.Lane}.";

            return $"Project intent reads like {projectShape}. Toggle capabilities to shape guidance without applying a preset.";
        }

        private static string BuildReason(PyralisAuthoringIntentSelection selection, PyralisAuthoringFact fact, HashSet<string> relatedStableIds)
        {
            if (relatedStableIds.Contains(fact.StableId))
                return "Related by the selected route intent.";

            if (HasWorldOverlap(selection, fact) && HasControlOverlap(selection, fact) && HasGoalOverlap(selection, fact))
                return "Matches the project world, control shape, and selected capability goals.";

            if (HasWorldOverlap(selection, fact) && HasControlOverlap(selection, fact))
                return "Matches the project world and control shape.";

            if (HasLane(fact, selection.Lane) && HasGoalOverlap(selection, fact))
                return "Matches the selected lane and game goal.";

            if (HasLane(fact, selection.Lane))
                return "Matches the selected lane.";

            if (HasGoalOverlap(selection, fact))
                return "Matches the selected game goal.";

            return "Relevant reflective authoring fact.";
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
            if (selection.Goals == null || selection.Goals.Length == 0)
                return 0;

            int count = 0;
            for (int i = 0; i < selection.Goals.Length; i++)
            {
                if (Contains(fact.GoalTags, selection.Goals[i].ToString()))
                    count++;
            }

            return count;
        }

        private static bool HasGoalOverlap(PyralisAuthoringIntentSelection selection, PyralisAuthoringFact fact)
        {
            return CountGoalMatches(selection, fact) > 0;
        }

        private static bool HasWorldOverlap(PyralisAuthoringIntentSelection selection, PyralisAuthoringFact fact)
        {
            return ContainsAny(GetFactSearchText(fact), GetWorldKeywords(selection.World));
        }

        private static bool HasControlOverlap(PyralisAuthoringIntentSelection selection, PyralisAuthoringFact fact)
        {
            return ContainsAny(GetFactSearchText(fact), GetControlKeywords(selection.Control));
        }

        private static string GetFactSearchText(PyralisAuthoringFact fact)
        {
            return (
                fact.StableId + " " +
                fact.DisplayName + " " +
                fact.Summary + " " +
                fact.RouteRelevance + " " +
                fact.FirstProof + " " +
                string.Join(" ", fact.GoalTags ?? Array.Empty<string>()) + " " +
                string.Join(" ", fact.LaneTags ?? Array.Empty<string>()) + " " +
                string.Join(" ", fact.AssignmentFields ?? Array.Empty<string>()) + " " +
                string.Join(" ", fact.CustomizationMoments ?? Array.Empty<string>()) + " " +
                string.Join(" ", fact.RelatedStableIds ?? Array.Empty<string>())).ToLowerInvariant();
        }

        private static bool ContainsAny(string haystack, string[] needles)
        {
            if (string.IsNullOrWhiteSpace(haystack) || needles == null)
                return false;

            for (int i = 0; i < needles.Length; i++)
            {
                string needle = needles[i];
                if (!string.IsNullOrWhiteSpace(needle) && haystack.Contains(needle.ToLowerInvariant()))
                    return true;
            }

            return false;
        }

        private static string[] GetWorldKeywords(PyralisAuthoringWorldIntent world)
        {
            return world switch
            {
                PyralisAuthoringWorldIntent.SideView2DGravity => new[] { "side-view", "side view", "gravity", "platform", "brawler", "runner", "2d-side-view" },
                PyralisAuthoringWorldIntent.TopDown2DPlane => new[] { "top-down", "top down", "free movement", "plane", "no gravity", "topdown" },
                PyralisAuthoringWorldIntent.Lane2_5D => new[] { "2.5d", "lane", "depth", "arena", "billboard" },
                PyralisAuthoringWorldIntent.Space3D => new[] { "3d", "rigged", "space", "rigged3d" },
                PyralisAuthoringWorldIntent.BoardGridTabletop => new[] { "board", "grid", "tabletop", "tile", "turn" },
                PyralisAuthoringWorldIntent.CardTableSurface => new[] { "card", "hand", "deck", "table surface" },
                PyralisAuthoringWorldIntent.UiMenuFirst => new[] { "ui", "menu", "hud", "canvas", "command" },
                _ => new[] { "hybrid", "custom", "mixed" }
            };
        }

        private static string[] GetControlKeywords(PyralisAuthoringControlIntent control)
        {
            return control switch
            {
                PyralisAuthoringControlIntent.PawnActor => new[] { "pawn", "actor", "participant", "movement" },
                PyralisAuthoringControlIntent.CursorSelector => new[] { "cursor", "selector", "selection" },
                PyralisAuthoringControlIntent.BoardSeat => new[] { "seat", "board", "tabletop", "faction" },
                PyralisAuthoringControlIntent.CardHand => new[] { "card", "hand", "deck" },
                PyralisAuthoringControlIntent.Camera => new[] { "camera", "framing", "bounds" },
                PyralisAuthoringControlIntent.MenuCommand => new[] { "menu", "command", "action selection", "ui" },
                PyralisAuthoringControlIntent.FactionTeam => new[] { "faction", "team", "seat", "participant" },
                _ => new[] { "mixed", "hybrid", "custom" }
            };
        }

        public static string GetWorldLabel(PyralisAuthoringWorldIntent world)
        {
            return world switch
            {
                PyralisAuthoringWorldIntent.SideView2DGravity => "2D side-view gravity world",
                PyralisAuthoringWorldIntent.TopDown2DPlane => "2D top-down/free-movement plane",
                PyralisAuthoringWorldIntent.Lane2_5D => "2.5D lane or arena space",
                PyralisAuthoringWorldIntent.Space3D => "3D space",
                PyralisAuthoringWorldIntent.BoardGridTabletop => "board/grid/tabletop world",
                PyralisAuthoringWorldIntent.CardTableSurface => "card/table surface",
                PyralisAuthoringWorldIntent.UiMenuFirst => "UI/menu-first surface",
                _ => "hybrid or unsure world"
            };
        }

        public static string GetControlLabel(PyralisAuthoringControlIntent control)
        {
            return control switch
            {
                PyralisAuthoringControlIntent.PawnActor => "pawn/actor control",
                PyralisAuthoringControlIntent.CursorSelector => "cursor or selector control",
                PyralisAuthoringControlIntent.BoardSeat => "board seat control",
                PyralisAuthoringControlIntent.CardHand => "card hand control",
                PyralisAuthoringControlIntent.Camera => "camera control",
                PyralisAuthoringControlIntent.MenuCommand => "menu command control",
                PyralisAuthoringControlIntent.FactionTeam => "faction/team control",
                _ => "mixed control"
            };
        }

        private static bool Contains(string[] values, string expected)
        {
            if (values == null || string.IsNullOrWhiteSpace(expected))
                return false;

            for (int i = 0; i < values.Length; i++)
            {
                if (string.Equals(values[i], expected, StringComparison.Ordinal))
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
