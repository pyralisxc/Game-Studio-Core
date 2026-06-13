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
            PyralisAuthoringGraphEvidenceState evidenceState,
            string message,
            Object target,
            string evidence,
            string nativeActionGuidance = null,
            string workIntentLabel = null)
        {
            Lane = lane;
            Label = label;
            EvidenceState = evidenceState;
            Message = message;
            Target = target;
            Evidence = evidence;
            NativeActionGuidance = nativeActionGuidance ?? string.Empty;
            WorkIntentLabel = workIntentLabel ?? string.Empty;
        }

        public PyralisAuthoringOverviewLane Lane { get; }
        public string Label { get; }
        public PyralisAuthoringGraphEvidenceState EvidenceState { get; }
        public string Message { get; }
        public Object Target { get; }
        public string Evidence { get; }
        public string NativeActionGuidance { get; }
        public string WorkIntentLabel { get; }
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
            string routeName = graph != null && !string.IsNullOrWhiteSpace(graph.RouteName)
                ? graph.RouteName
                : "No setup route selected";
            List<PyralisAuthoringOverviewIssue> doNow = new List<PyralisAuthoringOverviewIssue>();
            List<PyralisAuthoringOverviewIssue> doSoon = new List<PyralisAuthoringOverviewIssue>();
            List<PyralisAuthoringOverviewIssue> later = new List<PyralisAuthoringOverviewIssue>();
            IReadOnlyList<PyralisAuthoringOverviewIssue> overviewIssues =
                PyralisAuthoringSetupGraphProjection.BuildOverviewIssues(graph, activeSetup);
            for (int i = 0; i < overviewIssues.Count; i++)
            {
                PyralisAuthoringOverviewIssue issue = overviewIssues[i];
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

            return new PyralisAuthoringOverviewModel(
                routeName,
                PyralisAuthoringSetupGraphProjection.IsOverviewReadyToPressPlay(graph),
                PyralisAuthoringSetupGraphProjection.BuildOverviewBestNextAction(doNow, doSoon),
                PyralisAuthoringSetupGraphProjection.GetOverviewFirstProofLabel(graph),
                PyralisAuthoringSetupGraphProjection.GetOverviewFirstProofGuidance(graph),
                PyralisAuthoringSetupGraphProjection.GetOverviewFirstProofSetupSurface(graph),
                PyralisAuthoringSetupGraphProjection.GetOverviewFirstProofSuccessCriteria(graph),
                PyralisAuthoringSetupGraphProjection.GetOverviewFirstProofDeferUntilAfter(graph),
                PyralisAuthoringSetupGraphProjection.GetOverviewFirstProofChainSummary(graph),
                new List<PyralisAuthoringPlayModeChecklistItem>(PyralisAuthoringSetupGraphProjection.BuildOverviewPlayModeChecklist(graph)),
                doNow,
                doSoon,
                later);
        }

    }

}
