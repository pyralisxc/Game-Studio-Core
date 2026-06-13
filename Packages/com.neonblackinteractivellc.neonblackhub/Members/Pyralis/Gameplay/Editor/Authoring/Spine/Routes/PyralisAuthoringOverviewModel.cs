using System.Collections.Generic;
using System.Linq;
using NeonBlack.Gameplay.Editor.Inspectors;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    public enum PyralisAuthoringOverviewLane
    {
        DoNow,
        DoSoon,
        Later
    }

    public sealed class PyralisAuthoringOverviewIssue
    {
        public PyralisAuthoringOverviewIssue(
            PyralisAuthoringOverviewLane lane,
            string label,
            PyralisSetupFlowStepStatus status,
            string message,
            Object target,
            string evidence,
            string nativeActionGuidance = null,
            PyralisSetupFlowWorkIntent workIntent = PyralisSetupFlowWorkIntent.RequiredSetup)
        {
            Lane = lane;
            Label = label;
            Status = status;
            Message = message;
            Target = target;
            Evidence = evidence;
            NativeActionGuidance = nativeActionGuidance ?? string.Empty;
            WorkIntent = workIntent;
        }

        public PyralisAuthoringOverviewLane Lane { get; }
        public string Label { get; }
        public PyralisSetupFlowStepStatus Status { get; }
        public string Message { get; }
        public Object Target { get; }
        public string Evidence { get; }
        public string NativeActionGuidance { get; }
        public PyralisSetupFlowWorkIntent WorkIntent { get; }
    }

    public sealed class PyralisAuthoringPlayModeChecklistItem
    {
        public PyralisAuthoringPlayModeChecklistItem(string label, bool ready, string detail)
        {
            Label = label ?? string.Empty;
            Ready = ready;
            Detail = detail ?? string.Empty;
        }

        public string Label { get; }
        public bool Ready { get; }
        public string Detail { get; }
    }

    public sealed class PyralisAuthoringOverviewModel
    {
        private readonly List<PyralisAuthoringOverviewIssue> _doNow;
        private readonly List<PyralisAuthoringOverviewIssue> _doSoon;
        private readonly List<PyralisAuthoringOverviewIssue> _later;
        private readonly List<PyralisAuthoringPlayModeChecklistItem> _playModeChecklist;

        private PyralisAuthoringOverviewModel(
            string routeName,
            bool readyToPressPlay,
            string bestNextAction,
            string firstProofLabel,
            string firstProofGuidance,
            string firstProofSetupSurface,
            string firstProofSuccessCriteria,
            string firstProofDeferUntilAfter,
            string firstProofChainSummary,
            List<PyralisAuthoringPlayModeChecklistItem> playModeChecklist,
            List<PyralisAuthoringOverviewIssue> doNow,
            List<PyralisAuthoringOverviewIssue> doSoon,
            List<PyralisAuthoringOverviewIssue> later)
        {
            RouteName = routeName;
            ReadyToPressPlay = readyToPressPlay;
            BestNextAction = bestNextAction;
            FirstProofLabel = firstProofLabel;
            FirstProofGuidance = firstProofGuidance;
            FirstProofSetupSurface = firstProofSetupSurface;
            FirstProofSuccessCriteria = firstProofSuccessCriteria;
            FirstProofDeferUntilAfter = firstProofDeferUntilAfter;
            FirstProofChainSummary = firstProofChainSummary;
            _playModeChecklist = playModeChecklist ?? new List<PyralisAuthoringPlayModeChecklistItem>();
            _doNow = doNow ?? new List<PyralisAuthoringOverviewIssue>();
            _doSoon = doSoon ?? new List<PyralisAuthoringOverviewIssue>();
            _later = later ?? new List<PyralisAuthoringOverviewIssue>();
        }

        public string RouteName { get; }
        public bool ReadyToPressPlay { get; }
        public string BestNextAction { get; }
        public string FirstProofLabel { get; }
        public string FirstProofGuidance { get; }
        public string FirstProofSetupSurface { get; }
        public string FirstProofSuccessCriteria { get; }
        public string FirstProofDeferUntilAfter { get; }
        public string FirstProofChainSummary { get; }
        public IReadOnlyList<PyralisAuthoringPlayModeChecklistItem> PlayModeChecklist => _playModeChecklist;
        public IReadOnlyList<PyralisAuthoringOverviewIssue> DoNow => _doNow;
        public IReadOnlyList<PyralisAuthoringOverviewIssue> DoSoon => _doSoon;
        public IReadOnlyList<PyralisAuthoringOverviewIssue> Later => _later;

        public static PyralisAuthoringOverviewModel Build(
            Object activeSetup,
            PyralisAuthoringSetupGraph graph)
        {
            PyralisAuthoringRouteDescriptor route = PyralisAuthoringRouteDescriptor.Build(graph?.RouteAnalysis);
            string routeName = route != null && !string.IsNullOrWhiteSpace(route.RouteName)
                ? route.RouteName
                : "No setup route selected";
            List<PyralisAuthoringOverviewIssue> doNow = new List<PyralisAuthoringOverviewIssue>();
            List<PyralisAuthoringOverviewIssue> doSoon = new List<PyralisAuthoringOverviewIssue>();
            List<PyralisAuthoringOverviewIssue> later = new List<PyralisAuthoringOverviewIssue>();

            if (graph == null || graph.Source == null)
            {
                AddNoActiveSetupIssues(activeSetup, graph, doNow);
                return new PyralisAuthoringOverviewModel(
                    routeName,
                    false,
                    GetBestNextAction(doNow, doSoon),
                    "Create Setup Foundation",
                    "Create a Gameplay Root scene object with GameplaySessionBootstrap, then create and assign the first SessionDefinition asset.",
                    "Hierarchy object plus Project asset foundation.",
                    "Overview can inspect the bootstrap route and name the first playable proof.",
                    "Defer pawn, combat, UI, camera, and NPC expansion until the bootstrap has a SessionDefinition route.",
                    "Foundation first: GameplaySessionBootstrap -> SessionDefinition -> GameModeDefinition -> GameSetupProfile.",
                    BuildNoSetupChecklist(),
                    doNow,
                    doSoon,
                    later);
            }

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                PyralisAuthoringGraphNode node = graph.Nodes[i];
                PyralisAuthoringOverviewIssue issue = BuildIssue(node);
                if (issue == null)
                    continue;

                switch (issue.Lane)
                {
                    case PyralisAuthoringOverviewLane.DoNow:
                        doNow.Add(issue);
                        break;
                    case PyralisAuthoringOverviewLane.DoSoon:
                        doSoon.Add(issue);
                        break;
                    case PyralisAuthoringOverviewLane.Later:
                        later.Add(issue);
                        break;
                }
            }

            bool requiredSetupClear = CountDoNowBlockers(graph) == 0;

            return new PyralisAuthoringOverviewModel(
                routeName,
                requiredSetupClear,
                GetBestNextAction(doNow, doSoon),
                GetFirstProofLabel(graph),
                GetFirstProofGuidance(graph),
                GetFirstProofSetupSurface(graph),
                GetFirstProofSuccessCriteria(graph),
                GetFirstProofDeferUntilAfter(graph),
                GetFirstProofChainSummary(graph),
                BuildPlayModeChecklist(graph),
                doNow,
                doSoon,
                later);
        }

        private static List<PyralisAuthoringPlayModeChecklistItem> BuildNoSetupChecklist()
        {
            return new List<PyralisAuthoringPlayModeChecklistItem>
            {
                new PyralisAuthoringPlayModeChecklistItem(
                    "Setup foundation",
                    false,
                    "Create a Gameplay Root scene object with GameplaySessionBootstrap, then assign a SessionDefinition before Play Mode guidance can describe a proof.")
            };
        }

        private static List<PyralisAuthoringPlayModeChecklistItem> BuildPlayModeChecklist(
            PyralisAuthoringSetupGraph graph)
        {
            List<PyralisAuthoringPlayModeChecklistItem> items = new List<PyralisAuthoringPlayModeChecklistItem>();
            bool requiredClear = CountDoNowBlockers(graph) == 0;
            PyralisAuthoringGraphNode firstRequired = FindFirstDoNowBlocker(graph);
            PyralisAuthoringGraphNode proofNode = PyralisAuthoringSetupGraphProjection.FindCurrentProofNode(graph);
            items.Add(new PyralisAuthoringPlayModeChecklistItem(
                "Required setup",
                requiredClear,
                requiredClear ? "Do Now is clear." : firstRequired?.Guidance ?? "Clear the selected intent's Do Now setup before Play Mode."));

            items.Add(new PyralisAuthoringPlayModeChecklistItem(
                "First proof target",
                proofNode != null,
                proofNode != null && !string.IsNullOrWhiteSpace(proofNode.BlockingReason)
                    ? proofNode.BlockingReason
                    : "Select a route so Pyralis can name the smallest proof."));

            AddReadinessChecklistItem(
                items,
                "Scene visibility",
                graph,
                "CameraAudio",
                "Camera/audio checks are clear enough for a narrow visual proof.");

            AddReadinessChecklistItem(
                items,
                "Input route",
                graph,
                "Input",
                "InputProfile, action map, Move action, and UI input module checks are clear.");

            AddReadinessChecklistItem(
                items,
                "Presentation",
                graph,
                "Presentation",
                "Visible sprites/renderers and presentation-route checks are clear.");

            AddReadinessChecklistItem(
                items,
                "Physics feel",
                graph,
                "Physics",
                "Physics lane and collider checks are clear enough to judge movement feel.");

            return items;
        }

        private static void AddReadinessChecklistItem(
            List<PyralisAuthoringPlayModeChecklistItem> items,
            string label,
            PyralisAuthoringSetupGraph graph,
            string category,
            string readyDetail)
        {
            PyralisAuthoringGraphNode issue = FindFirstSceneReadinessNode(graph, category);
            items.Add(new PyralisAuthoringPlayModeChecklistItem(
                label,
                issue == null || issue.EvidenceState != PyralisAuthoringGraphEvidenceState.Blocked,
                issue == null ? readyDetail : issue.Guidance));
        }

        private static PyralisAuthoringGraphNode FindFirstSceneReadinessNode(
            PyralisAuthoringSetupGraph graph,
            string category)
        {
            if (graph == null || string.IsNullOrWhiteSpace(category))
                return null;

            PyralisAuthoringGraphNode fallback = null;
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                PyralisAuthoringGraphNode issue = graph.Nodes[i];
                if (issue == null || issue.SourceKind != PyralisAuthoringGraphSourceKind.SceneReadiness)
                    continue;

                if (!string.Equals(issue.Label, category, System.StringComparison.Ordinal))
                    continue;

                if (issue.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked)
                    return issue;

                fallback ??= issue;
            }

            return fallback;
        }

        private static void AddNoActiveSetupIssues(
            Object activeSetup,
            PyralisAuthoringSetupGraph graph,
            List<PyralisAuthoringOverviewIssue> doNow)
        {
            PyralisAuthoringGraphNode bootstrapNode = null;
            graph?.TryFindNode("bootstrap.root", out bootstrapNode);
            string message = bootstrapNode != null && !string.IsNullOrWhiteSpace(bootstrapNode.Guidance)
                ? bootstrapNode.Guidance
                : "Create a Gameplay Root scene object with GameplaySessionBootstrap, then create and assign a SessionDefinition.";
            doNow.Add(new PyralisAuthoringOverviewIssue(
                PyralisAuthoringOverviewLane.DoNow,
                "Create Gameplay Root",
                PyralisSetupFlowStepStatus.Missing,
                message,
                activeSetup,
                "Overview needs a GameplaySessionBootstrap route before it can judge scene readiness.",
                bootstrapNode != null && bootstrapNode.NativeAction.HasValue
                    ? bootstrapNode.NativeAction.Value.ToGuidanceSentence()
                    : GetNativeActionGuidance("Create Gameplay Root", message)));
        }

        private static PyralisAuthoringOverviewIssue BuildIssue(PyralisAuthoringGraphNode node)
        {
            if (node == null || node.EvidenceState == PyralisAuthoringGraphEvidenceState.Ready)
                return null;

            if (!PyralisAuthoringSetupGraphProjection.IsReadinessNode(node))
                return null;

            PyralisAuthoringOverviewLane lane = GetLane(node);
            return new PyralisAuthoringOverviewIssue(
                lane,
                node.Label,
                GetStatus(node.EvidenceState),
                node.Guidance,
                node.SourceObject,
                GetEvidence(node),
                node.NativeAction.HasValue ? node.NativeAction.Value.ToGuidanceSentence() : GetFirstNativeSetup(node),
                GetWorkIntent(lane));
        }

        private static PyralisAuthoringOverviewLane GetLane(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return PyralisAuthoringOverviewLane.Later;

            if (node.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked)
                return PyralisAuthoringOverviewLane.DoNow;

            if (node.EvidenceState == PyralisAuthoringGraphEvidenceState.Missing)
            {
                if (node.Kind == PyralisAuthoringGraphNodeKind.SetupChain
                    || node.Kind == PyralisAuthoringGraphNodeKind.UnitySurfaceRequirement
                    || node.SourceKind == PyralisAuthoringGraphSourceKind.SetupFlow
                    || string.Equals(node.StableId, "capability.selected", System.StringComparison.Ordinal))
                {
                    return PyralisAuthoringOverviewLane.DoNow;
                }

                return PyralisAuthoringOverviewLane.DoSoon;
            }

            if (node.EvidenceState == PyralisAuthoringGraphEvidenceState.CandidateDetected)
                return PyralisAuthoringOverviewLane.DoSoon;

            return PyralisAuthoringOverviewLane.Later;
        }

        private static string GetEvidence(PyralisAuthoringGraphNode node)
        {
            if (node.SourceObject != null)
                return "Evidence: " + node.SourceObject.name + " (" + node.SourceObject.GetType().Name + ")";

            return "Evidence: " + node.SourceKind + " / " + node.StableId;
        }

        private static int CountDoNowBlockers(PyralisAuthoringSetupGraph graph)
        {
            int count = 0;
            if (graph == null)
                return count;

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                if (IsDoNowBlocker(graph.Nodes[i]))
                    count++;
            }

            return count;
        }

        private static PyralisAuthoringGraphNode FindFirstDoNowBlocker(PyralisAuthoringSetupGraph graph)
        {
            if (graph == null)
                return null;

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                if (IsDoNowBlocker(graph.Nodes[i]))
                    return graph.Nodes[i];
            }

            return null;
        }

        private static bool IsDoNowBlocker(PyralisAuthoringGraphNode node)
        {
            if (node == null)
                return false;

            if (node.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked)
                return true;

            if (node.EvidenceState != PyralisAuthoringGraphEvidenceState.Missing)
                return false;

            return node.Kind == PyralisAuthoringGraphNodeKind.SetupChain
                || node.Kind == PyralisAuthoringGraphNodeKind.UnitySurfaceRequirement
                || node.SourceKind == PyralisAuthoringGraphSourceKind.SetupFlow
                || string.Equals(node.StableId, "capability.selected", System.StringComparison.Ordinal);
        }

        private static PyralisSetupFlowStepStatus GetStatus(PyralisAuthoringGraphEvidenceState evidenceState)
        {
            switch (evidenceState)
            {
                case PyralisAuthoringGraphEvidenceState.Blocked:
                    return PyralisSetupFlowStepStatus.Blocked;
                case PyralisAuthoringGraphEvidenceState.Missing:
                    return PyralisSetupFlowStepStatus.Missing;
                case PyralisAuthoringGraphEvidenceState.CandidateDetected:
                    return PyralisSetupFlowStepStatus.Recommended;
                case PyralisAuthoringGraphEvidenceState.Optional:
                    return PyralisSetupFlowStepStatus.Optional;
                default:
                    return PyralisSetupFlowStepStatus.Ready;
            }
        }

        private static PyralisSetupFlowWorkIntent GetWorkIntent(PyralisAuthoringOverviewLane lane)
        {
            switch (lane)
            {
                case PyralisAuthoringOverviewLane.DoNow:
                    return PyralisSetupFlowWorkIntent.RequiredSetup;
                case PyralisAuthoringOverviewLane.DoSoon:
                    return PyralisSetupFlowWorkIntent.ProofEnhancer;
                default:
                    return PyralisSetupFlowWorkIntent.FeatureCard;
            }
        }

        private static string GetFirstNativeSetup(PyralisAuthoringGraphNode node)
        {
            if (node == null || node.NativeSetup.Length == 0)
                return string.Empty;

            return node.NativeSetup[0];
        }

        private static string GetBestNextAction(
            IReadOnlyList<PyralisAuthoringOverviewIssue> doNow,
            IReadOnlyList<PyralisAuthoringOverviewIssue> doSoon)
        {
            if (doNow != null && doNow.Count > 0)
                return FormatBestNextAction(doNow[0]);

            if (doSoon != null && doSoon.Count > 0)
                return "Optional proof enhancer: " + FormatBestNextAction(doSoon[0]);

            return "Required setup is clear. Run the minimal route proof in Play mode first, then add one feature at a time.";
        }

        private static string FormatBestNextAction(PyralisAuthoringOverviewIssue issue)
        {
            if (issue == null)
                return "Select a setup item and follow its native Unity action.";

            if (!string.IsNullOrWhiteSpace(issue.NativeActionGuidance))
                return issue.Label + ": " + issue.NativeActionGuidance;

            return issue.Label + ": " + issue.Message;
        }

        private static string GetNativeActionGuidance(string label, string message)
        {
            string normalized = (label ?? string.Empty) + " " + (message ?? string.Empty);

            if (normalized.Contains("Create Gameplay Root")
                || normalized.Contains("Select Active Setup")
                || normalized.Contains("Select Gameplay Session Bootstrap"))
            {
                return new PyralisAuthoringNativeAction(
                    "Create or select",
                    PyralisAuthoringActionSurface.Hierarchy,
                    "Gameplay Root",
                    "right-click -> Create Empty, name it Gameplay Root, then use Inspector -> Add Component -> GameplaySessionBootstrap",
                    "Overview shows Gameplay Root as the active setup").ToGuidanceSentence();
            }

            return string.Empty;
        }

        private static string GetFirstProofLabel(PyralisAuthoringSetupGraph graph)
        {
            return PyralisAuthoringSetupGraphProjection.FindCurrentProofNode(graph)?.Label ?? "Create Setup Foundation";
        }

        private static string GetFirstProofGuidance(PyralisAuthoringSetupGraph graph)
        {
            return PyralisAuthoringSetupGraphProjection.FindCurrentProofNode(graph)?.Guidance
                ?? "Create a Gameplay Root scene object with GameplaySessionBootstrap, then create and assign the first SessionDefinition asset.";
        }

        private static string GetFirstProofSetupSurface(PyralisAuthoringSetupGraph graph)
        {
            PyralisAuthoringGraphNode proofNode = PyralisAuthoringSetupGraphProjection.FindCurrentProofNode(graph);
            return proofNode != null && proofNode.NativeSetup.Length > 0
                ? proofNode.NativeSetup[0]
                : "Hierarchy object plus Project asset foundation.";
        }

        private static string GetFirstProofSuccessCriteria(PyralisAuthoringSetupGraph graph)
        {
            return PyralisAuthoringSetupGraphProjection.FindCurrentProofNode(graph)?.BlockingReason
                ?? "Overview can inspect the bootstrap route and name the first playable proof.";
        }

        private static string GetFirstProofDeferUntilAfter(PyralisAuthoringSetupGraph graph)
        {
            PyralisAuthoringGraphNode unresolved = PyralisAuthoringSetupGraphProjection.FindFirstUnresolvedNode(graph);
            return unresolved != null
                ? "Defer expansion until this graph node is clear: " + unresolved.Label
                : "Defer broad polish until the graph-backed first proof runs in Play Mode.";
        }

        private static string GetFirstProofChainSummary(PyralisAuthoringSetupGraph graph)
        {
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> rows = PyralisAuthoringSetupGraphProjection.BuildProofSupportRows(graph);
            if (rows.Count == 0)
                return "Graph proof chain: setup route -> active proof target.";

            return "Graph proof chain: " + string.Join(
                " -> ",
                rows.Select(row => row.FromLabel).Distinct().Concat(new[] { rows[0].ToLabel }));
        }
    }

}
