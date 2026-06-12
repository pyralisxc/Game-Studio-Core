using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
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

        public static PyralisAuthoringOverviewModel Build(Object activeSetup, PyralisAuthoringRouteReport routeReport)
        {
            string routeName = routeReport != null ? routeReport.RouteName : "No setup route selected";
            GameplaySessionBootstrap bootstrap = PyralisAuthoringWindow.GetSelectedBootstrap(activeSetup);
            List<PyralisAuthoringOverviewIssue> doNow = new List<PyralisAuthoringOverviewIssue>();
            List<PyralisAuthoringOverviewIssue> doSoon = new List<PyralisAuthoringOverviewIssue>();
            List<PyralisAuthoringOverviewIssue> later = new List<PyralisAuthoringOverviewIssue>();

            if (bootstrap == null)
            {
                AddNoActiveSetupIssues(activeSetup, routeReport, doNow);
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

            PyralisAuthoringRouteDescriptor route = PyralisAuthoringRouteDescriptor.Build(activeSetup);
            PyralisSetupFlowReport setupFlowReport = PyralisSetupFlowValidator.BuildReport(bootstrap);
            PyralisSceneReadinessReport readinessReport = PyralisSceneReadinessValidator.BuildReport(bootstrap);
            for (int i = 0; i < setupFlowReport.Steps.Count; i++)
            {
                PyralisSetupFlowStep step = setupFlowReport.Steps[i];
                PyralisAuthoringOverviewIssue issue = BuildIssue(step);
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

            bool requiredSetupClear = setupFlowReport.RequiredIssueCount == 0;

            return new PyralisAuthoringOverviewModel(
                routeName,
                requiredSetupClear,
                GetBestNextAction(doNow, doSoon),
                GetFirstProofLabel(route),
                GetFirstProofGuidance(route),
                GetFirstProofSetupSurface(route),
                GetFirstProofSuccessCriteria(route),
                GetFirstProofDeferUntilAfter(route),
                GetFirstProofChainSummary(route),
                BuildPlayModeChecklist(setupFlowReport, readinessReport, route),
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
            PyralisSetupFlowReport setupFlowReport,
            PyralisSceneReadinessReport readinessReport,
            PyralisAuthoringRouteDescriptor route)
        {
            List<PyralisAuthoringPlayModeChecklistItem> items = new List<PyralisAuthoringPlayModeChecklistItem>();
            bool requiredClear = setupFlowReport != null && setupFlowReport.RequiredIssueCount == 0;
            items.Add(new PyralisAuthoringPlayModeChecklistItem(
                "Required setup",
                requiredClear,
                requiredClear ? "Do Now is clear." : setupFlowReport?.FirstBlockingStep?.Message ?? "Clear the selected intent's Do Now setup before Play Mode."));

            items.Add(new PyralisAuthoringPlayModeChecklistItem(
                "First proof target",
                route != null,
                route != null ? PyralisAuthoringRouteProof.Build(route).SuccessCriteria : "Select a route so Pyralis can name the smallest proof."));

            AddReadinessChecklistItem(
                items,
                "Scene visibility",
                readinessReport,
                PyralisSceneReadinessCategory.CameraAudio,
                "Camera/audio checks are clear enough for a narrow visual proof.");

            AddReadinessChecklistItem(
                items,
                "Input route",
                readinessReport,
                PyralisSceneReadinessCategory.Input,
                "InputProfile, action map, Move action, and UI input module checks are clear.");

            AddReadinessChecklistItem(
                items,
                "Presentation",
                readinessReport,
                PyralisSceneReadinessCategory.Presentation,
                "Visible sprites/renderers and presentation-route checks are clear.");

            AddReadinessChecklistItem(
                items,
                "Physics feel",
                readinessReport,
                PyralisSceneReadinessCategory.Physics,
                "Physics lane and collider checks are clear enough to judge movement feel.");

            return items;
        }

        private static void AddReadinessChecklistItem(
            List<PyralisAuthoringPlayModeChecklistItem> items,
            string label,
            PyralisSceneReadinessReport readinessReport,
            PyralisSceneReadinessCategory category,
            string readyDetail)
        {
            PyralisSceneReadinessIssue issue = FindFirstReadinessIssue(readinessReport, category);
            items.Add(new PyralisAuthoringPlayModeChecklistItem(
                label,
                issue == null || issue.Severity != PyralisSceneReadinessSeverity.RequiredBeforePlay,
                issue == null ? readyDetail : issue.Message));
        }

        private static PyralisSceneReadinessIssue FindFirstReadinessIssue(
            PyralisSceneReadinessReport readinessReport,
            PyralisSceneReadinessCategory category)
        {
            if (readinessReport == null)
                return null;

            PyralisSceneReadinessIssue fallback = null;
            for (int i = 0; i < readinessReport.Issues.Count; i++)
            {
                PyralisSceneReadinessIssue issue = readinessReport.Issues[i];
                if (issue == null || issue.Category != category)
                    continue;

                if (issue.Severity == PyralisSceneReadinessSeverity.RequiredBeforePlay)
                    return issue;

                fallback ??= issue;
            }

            return fallback;
        }

        private static void AddNoActiveSetupIssues(
            Object activeSetup,
            PyralisAuthoringRouteReport routeReport,
            List<PyralisAuthoringOverviewIssue> doNow)
        {
            if (routeReport != null && routeReport.ValidationIssues.Count > 0)
            {
                for (int i = 0; i < routeReport.ValidationIssues.Count; i++)
                {
                    string issue = routeReport.ValidationIssues[i];
                    if (string.IsNullOrWhiteSpace(issue))
                        continue;

                    doNow.Add(new PyralisAuthoringOverviewIssue(
                        PyralisAuthoringOverviewLane.DoNow,
                        "Selected Asset Validation",
                        PyralisSetupFlowStepStatus.Missing,
                        issue,
                        activeSetup,
                        "Validation comes from the selected asset because no GameplaySessionBootstrap is active.",
                        GetNativeActionGuidance("Selected Asset Validation", issue)));
                }

                return;
            }

            string message = routeReport != null
                ? routeReport.NextStep
                : "Create a Gameplay Root scene object with GameplaySessionBootstrap, then create and assign a SessionDefinition.";
            doNow.Add(new PyralisAuthoringOverviewIssue(
                PyralisAuthoringOverviewLane.DoNow,
                "Create Gameplay Root",
                PyralisSetupFlowStepStatus.Missing,
                message,
                activeSetup,
                "Overview needs a GameplaySessionBootstrap route before it can judge scene readiness.",
                GetNativeActionGuidance("Create Gameplay Root", message)));
        }

        private static PyralisAuthoringOverviewIssue BuildIssue(PyralisSetupFlowStep step)
        {
            if (step == null || step.Status == PyralisSetupFlowStepStatus.Ready)
                return null;

            PyralisAuthoringOverviewLane lane = GetLane(step.Status, step.WorkIntent);
            return new PyralisAuthoringOverviewIssue(
                lane,
                step.Label,
                step.Status,
                step.Message,
                step.ReferencedObject,
                GetEvidence(step),
                GetNativeActionGuidance(step),
                step.WorkIntent);
        }

        private static string GetNativeActionGuidance(PyralisSetupFlowStep step)
        {
            if (step == null)
                return string.Empty;

            if (step.NativeAction.HasValue)
                return step.NativeAction.Value.ToGuidanceSentence();

            return string.Empty;
        }

        private static PyralisAuthoringOverviewLane GetLane(PyralisSetupFlowStepStatus status, PyralisSetupFlowWorkIntent workIntent)
        {
            switch (status)
            {
                case PyralisSetupFlowStepStatus.Missing:
                case PyralisSetupFlowStepStatus.Blocked:
                    return PyralisAuthoringOverviewLane.DoNow;
                case PyralisSetupFlowStepStatus.Recommended:
                    return workIntent == PyralisSetupFlowWorkIntent.FeatureCard
                        ? PyralisAuthoringOverviewLane.Later
                        : PyralisAuthoringOverviewLane.DoSoon;
                case PyralisSetupFlowStepStatus.Optional:
                default:
                    return PyralisAuthoringOverviewLane.Later;
            }
        }

        private static string GetEvidence(PyralisSetupFlowStep step)
        {
            if (step.ReferencedObject != null)
                return "Evidence: " + step.ReferencedObject.name + " (" + step.ReferencedObject.GetType().Name + ")";

            return "Evidence: no referenced object is currently assigned for this step.";
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

        private static string GetFirstProofLabel(PyralisAuthoringRouteDescriptor route)
        {
            return PyralisAuthoringRouteProof.Build(route).Label;
        }

        private static string GetFirstProofGuidance(PyralisAuthoringRouteDescriptor route)
        {
            return PyralisAuthoringRouteProof.Build(route).Guidance;
        }

        private static string GetFirstProofSetupSurface(PyralisAuthoringRouteDescriptor route)
        {
            return PyralisAuthoringRouteProof.Build(route).SetupSurface;
        }

        private static string GetFirstProofSuccessCriteria(PyralisAuthoringRouteDescriptor route)
        {
            return PyralisAuthoringRouteProof.Build(route).SuccessCriteria;
        }

        private static string GetFirstProofDeferUntilAfter(PyralisAuthoringRouteDescriptor route)
        {
            return PyralisAuthoringRouteProof.Build(route).DeferUntilAfter;
        }

        private static string GetFirstProofChainSummary(PyralisAuthoringRouteDescriptor route)
        {
            return PyralisAuthoringRouteProof.Build(route).ProofChainSummary;
        }
    }

    public sealed class PyralisAuthoringOverviewSnapshot
    {
        private PyralisAuthoringOverviewSnapshot(
            string routeName,
            bool readyToPressPlay,
            int requiredMissingCount,
            int recommendedNextCount,
            int optionalLaterCount,
            int readyCount,
            string requiredMissingLabel,
            string recommendedNextLabel,
            string optionalLaterLabel)
        {
            RouteName = routeName;
            ReadyToPressPlay = readyToPressPlay;
            RequiredMissingCount = requiredMissingCount;
            RecommendedNextCount = recommendedNextCount;
            OptionalLaterCount = optionalLaterCount;
            ReadyCount = readyCount;
            RequiredMissingLabel = requiredMissingLabel;
            RecommendedNextLabel = recommendedNextLabel;
            OptionalLaterLabel = optionalLaterLabel;
        }

        public string RouteName { get; }
        public bool ReadyToPressPlay { get; }
        public int RequiredMissingCount { get; }
        public int RecommendedNextCount { get; }
        public int OptionalLaterCount { get; }
        public int ReadyCount { get; }
        public string RequiredMissingLabel { get; }
        public string RecommendedNextLabel { get; }
        public string OptionalLaterLabel { get; }

        public static PyralisAuthoringOverviewSnapshot Build(Object activeSetup, PyralisAuthoringRouteReport routeReport)
        {
            GameplaySessionBootstrap bootstrap = PyralisAuthoringWindow.GetSelectedBootstrap(activeSetup);
            if (bootstrap == null)
            {
                int validationCount = routeReport != null ? routeReport.ValidationIssues.Count : 0;
                return new PyralisAuthoringOverviewSnapshot(
                    routeReport != null ? routeReport.RouteName : "No setup route selected",
                    false,
                    validationCount,
                    0,
                    0,
                    0,
                    validationCount > 0 ? "Resolve selected asset validation before scene readiness can be checked." : "Select or pin a GameplaySessionBootstrap to check scene readiness.",
                    "No scene-level recommendations until a bootstrap is active.",
                    "Optional scene roots are shown after a bootstrap is active.");
            }

            PyralisSetupFlowReport setupFlowReport = PyralisSetupFlowValidator.BuildReport(bootstrap);
            PyralisSetupFlowStep firstRequired = setupFlowReport.FirstBlockingStep;
            PyralisSetupFlowStep firstRecommended = FindFirstRecommendedStep(setupFlowReport);
            PyralisSetupFlowStep firstOptional = FindFirstStep(setupFlowReport, PyralisSetupFlowStepStatus.Optional);

            return new PyralisAuthoringOverviewSnapshot(
                routeReport != null ? routeReport.RouteName : "No setup route selected",
                setupFlowReport.RequiredIssueCount == 0,
                setupFlowReport.RequiredIssueCount,
                setupFlowReport.RecommendedIssueCount,
                setupFlowReport.OptionalCount,
                setupFlowReport.ReadyCount,
                firstRequired != null ? firstRequired.Label + ": " + firstRequired.Message : "Required setup is clear.",
                firstRecommended != null ? firstRecommended.Label + ": " + firstRecommended.Message : "No recommended next item.",
                firstOptional != null ? firstOptional.Label + ": " + firstOptional.Message : "No optional later item.");
        }

        private static PyralisSetupFlowStep FindFirstStep(PyralisSetupFlowReport report, PyralisSetupFlowStepStatus status)
        {
            if (report == null)
                return null;

            for (int i = 0; i < report.Steps.Count; i++)
            {
                if (report.Steps[i].Status == status)
                    return report.Steps[i];
            }

            return null;
        }

        private static PyralisSetupFlowStep FindFirstRecommendedStep(PyralisSetupFlowReport report)
        {
            if (report == null)
                return null;

            for (int i = 0; i < report.Steps.Count; i++)
            {
                PyralisSetupFlowStep step = report.Steps[i];
                if (step.Status == PyralisSetupFlowStepStatus.Recommended && IsRouteSpecificRecommendation(step))
                    return step;
            }

            return FindFirstStep(report, PyralisSetupFlowStepStatus.Recommended);
        }

        private static bool IsRouteSpecificRecommendation(PyralisSetupFlowStep step)
        {
            if (step == null)
                return false;

            return step.WorkIntent == PyralisSetupFlowWorkIntent.ProofEnhancer
                || step.WorkIntent == PyralisSetupFlowWorkIntent.FeatureCard;
        }
    }
}
