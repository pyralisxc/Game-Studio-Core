using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace NeonBlack.Gameplay.Editor
{
    public enum PyralisAuthoringSceneSurfaceKind
    {
        EnvironmentPlayfield,
        CameraBounds,
        UiHudMenus,
        ScoringObjectives,
        BoardActionSelection,
        PickupsHazardsEnemies,
        FallbackTypeName
    }

    public sealed class PyralisAuthoringSceneSurfaceDetectorResult
    {
        public PyralisAuthoringSceneSurfaceDetectorResult(
            string detectorId,
            PyralisAuthoringSceneSurfaceKind surfaceKind,
            Object candidateObject,
            bool linkedToActiveSetup,
            string summary,
            string issueCode = "",
            PyralisAuthoringNativeAction? nativeAction = null)
        {
            DetectorId = detectorId ?? string.Empty;
            SurfaceKind = surfaceKind;
            CandidateObject = candidateObject;
            LinkedToActiveSetup = linkedToActiveSetup;
            Summary = summary ?? string.Empty;
            IssueCode = issueCode ?? string.Empty;
            NativeAction = nativeAction;
        }

        public string DetectorId { get; }
        public PyralisAuthoringSceneSurfaceKind SurfaceKind { get; }
        public Object CandidateObject { get; }
        public bool LinkedToActiveSetup { get; }
        public string Summary { get; }
        public string IssueCode { get; }
        public PyralisAuthoringNativeAction? NativeAction { get; }
    }

    public sealed class PyralisAuthoringSceneSurfaceRow
    {
        public PyralisAuthoringSceneSurfaceRow(
            string surface,
            bool present,
            bool recommended,
            string current,
            string nextFix,
            PyralisAuthoringEvidenceState evidenceState = PyralisAuthoringEvidenceState.NotRelevant,
            string detectorId = "",
            PyralisAuthoringSceneSurfaceKind surfaceKind = PyralisAuthoringSceneSurfaceKind.FallbackTypeName,
            Object candidateObject = null,
            bool linkedToActiveSetup = false,
            bool routeRelevant = false,
            string issueCode = "",
            PyralisAuthoringNativeAction? nativeAction = null)
        {
            Surface = surface;
            Present = present;
            Recommended = recommended;
            Current = current;
            NextFix = nextFix;
            EvidenceState = evidenceState;
            DetectorId = detectorId ?? string.Empty;
            SurfaceKind = surfaceKind;
            CandidateObject = candidateObject;
            LinkedToActiveSetup = linkedToActiveSetup;
            RouteRelevant = routeRelevant;
            IssueCode = issueCode ?? string.Empty;
            NativeAction = nativeAction;
        }

        public string Surface { get; }
        public bool Present { get; }
        public bool Recommended { get; }
        public string Current { get; }
        public string NextFix { get; }
        public PyralisAuthoringEvidenceState EvidenceState { get; }
        public string DetectorId { get; }
        public PyralisAuthoringSceneSurfaceKind SurfaceKind { get; }
        public Object CandidateObject { get; }
        public bool LinkedToActiveSetup { get; }
        public bool RouteRelevant { get; }
        public string IssueCode { get; }
        public PyralisAuthoringNativeAction? NativeAction { get; }
        public bool SupportsFirstProofAttempt => !Recommended || Present;
    }

    public sealed class PyralisAuthoringSceneSurfaceSnapshot
    {
        private readonly List<PyralisAuthoringSceneSurfaceRow> _rows;

        private PyralisAuthoringSceneSurfaceSnapshot(List<PyralisAuthoringSceneSurfaceRow> rows)
        {
            _rows = rows ?? new List<PyralisAuthoringSceneSurfaceRow>();
        }

        public IReadOnlyList<PyralisAuthoringSceneSurfaceRow> Rows => _rows;

        public static PyralisAuthoringSceneSurfaceSnapshot Build(Object activeSetup)
        {
            return Build(activeSetup, null);
        }

        public static PyralisAuthoringSceneSurfaceSnapshot Build(Object activeSetup, PyralisSetupRouteAnalysis routeAnalysis)
        {
            GameplaySessionBootstrap bootstrap = PyralisAuthoringSetupContextResolver.GetSelectedBootstrap(activeSetup);
            PyralisAuthoringRouteDescriptor route = routeAnalysis != null
                ? PyralisAuthoringRouteDescriptor.Build(routeAnalysis)
                : PyralisAuthoringRouteDescriptor.Build(activeSetup);
            PyralisAuthoringSceneEvidence evidence = PyralisAuthoringSceneEvidence.Build(bootstrap);
            List<PyralisAuthoringSceneSurfaceRow> rows = new List<PyralisAuthoringSceneSurfaceRow>();

            bool wantsWorld = PyralisAuthoringSceneSurfaceGuidance.IsRecommended(route, PyralisAuthoringSceneSurfaceGuidance.EnvironmentPlayfield);
            bool wantsCamera = PyralisAuthoringSceneSurfaceGuidance.IsRecommended(route, PyralisAuthoringSceneSurfaceGuidance.CameraBounds);
            bool wantsUi = PyralisAuthoringSceneSurfaceGuidance.IsRecommended(route, PyralisAuthoringSceneSurfaceGuidance.UiHudMenus);
            bool wantsScoring = PyralisAuthoringSceneSurfaceGuidance.IsRecommended(route, PyralisAuthoringSceneSurfaceGuidance.ScoringObjectives);
            bool wantsActionOrTabletop = PyralisAuthoringSceneSurfaceGuidance.IsRecommended(route, PyralisAuthoringSceneSurfaceGuidance.BoardActionSelection);
            bool wantsHazardsOrPickups = PyralisAuthoringSceneSurfaceGuidance.IsRecommended(route, PyralisAuthoringSceneSurfaceGuidance.PickupsHazardsEnemies);

            rows.Add(new PyralisAuthoringSceneSurfaceRow(
                PyralisAuthoringSceneSurfaceGuidance.EnvironmentPlayfield,
                evidence.HasEnvironmentSurface,
                wantsWorld,
                evidence.GetEnvironmentSummary(),
                PyralisAuthoringSceneSurfaceGuidance.GetNextFix(PyralisAuthoringSceneSurfaceGuidance.EnvironmentPlayfield, wantsWorld),
                GetEvidenceState(evidence.HasEnvironmentSurface, wantsWorld, evidence.HasLinkedSurface(PyralisAuthoringSceneSurfaceKind.EnvironmentPlayfield)),
                detectorId: evidence.GetPrimaryDetectorId(PyralisAuthoringSceneSurfaceKind.EnvironmentPlayfield),
                surfaceKind: PyralisAuthoringSceneSurfaceKind.EnvironmentPlayfield,
                candidateObject: evidence.GetPrimaryCandidate(PyralisAuthoringSceneSurfaceKind.EnvironmentPlayfield),
                linkedToActiveSetup: evidence.HasLinkedSurface(PyralisAuthoringSceneSurfaceKind.EnvironmentPlayfield),
                routeRelevant: wantsWorld,
                issueCode: BuildIssueCode(PyralisAuthoringSceneSurfaceKind.EnvironmentPlayfield, evidence.HasEnvironmentSurface, wantsWorld),
                nativeAction: BuildNativeAction(PyralisAuthoringSceneSurfaceGuidance.EnvironmentPlayfield, wantsWorld)));

            rows.Add(new PyralisAuthoringSceneSurfaceRow(
                PyralisAuthoringSceneSurfaceGuidance.CameraBounds,
                evidence.HasCameraSurface,
                wantsCamera,
                evidence.GetCameraSummary(),
                PyralisAuthoringSceneSurfaceGuidance.GetNextFix(PyralisAuthoringSceneSurfaceGuidance.CameraBounds, wantsCamera),
                GetEvidenceState(evidence.HasCameraSurface, wantsCamera, evidence.HasLinkedSurface(PyralisAuthoringSceneSurfaceKind.CameraBounds)),
                detectorId: evidence.GetPrimaryDetectorId(PyralisAuthoringSceneSurfaceKind.CameraBounds),
                surfaceKind: PyralisAuthoringSceneSurfaceKind.CameraBounds,
                candidateObject: evidence.GetPrimaryCandidate(PyralisAuthoringSceneSurfaceKind.CameraBounds),
                linkedToActiveSetup: evidence.HasLinkedSurface(PyralisAuthoringSceneSurfaceKind.CameraBounds),
                routeRelevant: wantsCamera,
                issueCode: BuildIssueCode(PyralisAuthoringSceneSurfaceKind.CameraBounds, evidence.HasCameraSurface, wantsCamera),
                nativeAction: BuildNativeAction(PyralisAuthoringSceneSurfaceGuidance.CameraBounds, wantsCamera)));

            rows.Add(new PyralisAuthoringSceneSurfaceRow(
                PyralisAuthoringSceneSurfaceGuidance.UiHudMenus,
                evidence.HasUiSurface,
                wantsUi,
                evidence.GetUiSummary(),
                PyralisAuthoringSceneSurfaceGuidance.GetNextFix(PyralisAuthoringSceneSurfaceGuidance.UiHudMenus, wantsUi),
                GetEvidenceState(evidence.HasUiSurface, wantsUi),
                detectorId: evidence.GetPrimaryDetectorId(PyralisAuthoringSceneSurfaceKind.UiHudMenus),
                surfaceKind: PyralisAuthoringSceneSurfaceKind.UiHudMenus,
                candidateObject: evidence.GetPrimaryCandidate(PyralisAuthoringSceneSurfaceKind.UiHudMenus),
                routeRelevant: wantsUi,
                issueCode: BuildIssueCode(PyralisAuthoringSceneSurfaceKind.UiHudMenus, evidence.HasUiSurface, wantsUi),
                nativeAction: BuildNativeAction(PyralisAuthoringSceneSurfaceGuidance.UiHudMenus, wantsUi)));

            rows.Add(new PyralisAuthoringSceneSurfaceRow(
                PyralisAuthoringSceneSurfaceGuidance.ScoringObjectives,
                evidence.ScoreServiceCount > 0,
                wantsScoring,
                evidence.ScoreServiceCount > 0 ? $"{evidence.ScoreServiceCount} score service object(s)" : "No score service detected",
                PyralisAuthoringSceneSurfaceGuidance.GetNextFix(PyralisAuthoringSceneSurfaceGuidance.ScoringObjectives, wantsScoring),
                GetEvidenceState(evidence.ScoreServiceCount > 0, wantsScoring),
                detectorId: evidence.GetPrimaryDetectorId(PyralisAuthoringSceneSurfaceKind.ScoringObjectives),
                surfaceKind: PyralisAuthoringSceneSurfaceKind.ScoringObjectives,
                candidateObject: evidence.GetPrimaryCandidate(PyralisAuthoringSceneSurfaceKind.ScoringObjectives),
                routeRelevant: wantsScoring,
                issueCode: BuildIssueCode(PyralisAuthoringSceneSurfaceKind.ScoringObjectives, evidence.ScoreServiceCount > 0, wantsScoring),
                nativeAction: BuildNativeAction(PyralisAuthoringSceneSurfaceGuidance.ScoringObjectives, wantsScoring)));

            rows.Add(new PyralisAuthoringSceneSurfaceRow(
                PyralisAuthoringSceneSurfaceGuidance.BoardActionSelection,
                evidence.HasSelectionSurface,
                wantsActionOrTabletop,
                evidence.GetSelectionSummary(),
                PyralisAuthoringSceneSurfaceGuidance.GetNextFix(PyralisAuthoringSceneSurfaceGuidance.BoardActionSelection, wantsActionOrTabletop),
                GetEvidenceState(evidence.HasSelectionSurface, wantsActionOrTabletop),
                detectorId: evidence.GetPrimaryDetectorId(PyralisAuthoringSceneSurfaceKind.BoardActionSelection),
                surfaceKind: PyralisAuthoringSceneSurfaceKind.BoardActionSelection,
                candidateObject: evidence.GetPrimaryCandidate(PyralisAuthoringSceneSurfaceKind.BoardActionSelection),
                routeRelevant: wantsActionOrTabletop,
                issueCode: BuildIssueCode(PyralisAuthoringSceneSurfaceKind.BoardActionSelection, evidence.HasSelectionSurface, wantsActionOrTabletop),
                nativeAction: BuildNativeAction(PyralisAuthoringSceneSurfaceGuidance.BoardActionSelection, wantsActionOrTabletop)));

            rows.Add(new PyralisAuthoringSceneSurfaceRow(
                PyralisAuthoringSceneSurfaceGuidance.PickupsHazardsEnemies,
                evidence.HasEncounterSurface,
                wantsHazardsOrPickups,
                evidence.GetEncounterSummary(),
                PyralisAuthoringSceneSurfaceGuidance.GetNextFix(PyralisAuthoringSceneSurfaceGuidance.PickupsHazardsEnemies, wantsHazardsOrPickups),
                GetEvidenceState(evidence.HasEncounterSurface, wantsHazardsOrPickups),
                detectorId: evidence.GetPrimaryDetectorId(PyralisAuthoringSceneSurfaceKind.PickupsHazardsEnemies),
                surfaceKind: PyralisAuthoringSceneSurfaceKind.PickupsHazardsEnemies,
                candidateObject: evidence.GetPrimaryCandidate(PyralisAuthoringSceneSurfaceKind.PickupsHazardsEnemies),
                routeRelevant: wantsHazardsOrPickups,
                issueCode: BuildIssueCode(PyralisAuthoringSceneSurfaceKind.PickupsHazardsEnemies, evidence.HasEncounterSurface, wantsHazardsOrPickups),
                nativeAction: BuildNativeAction(PyralisAuthoringSceneSurfaceGuidance.PickupsHazardsEnemies, wantsHazardsOrPickups)));

            IReadOnlyList<PyralisAuthoringSceneSurfaceDetectorResult> fallbackResults = evidence.FallbackTypeNameResults;
            for (int i = 0; i < fallbackResults.Count; i++)
            {
                PyralisAuthoringSceneSurfaceDetectorResult fallback = fallbackResults[i];
                rows.Add(new PyralisAuthoringSceneSurfaceRow(
                    "Fallback Type-Name Scene Surface",
                    true,
                    false,
                    fallback.Summary,
                    "Add a typed scene-surface detector for this component or tag it through an existing contract/component interface.",
                    PyralisAuthoringEvidenceState.CandidateDetected,
                    detectorId: fallback.DetectorId,
                    surfaceKind: fallback.SurfaceKind,
                    candidateObject: fallback.CandidateObject,
                    linkedToActiveSetup: fallback.LinkedToActiveSetup,
                    routeRelevant: false,
                    issueCode: fallback.IssueCode,
                    nativeAction: fallback.NativeAction));
            }

            return new PyralisAuthoringSceneSurfaceSnapshot(rows);
        }

        private static PyralisAuthoringEvidenceState GetEvidenceState(bool present, bool recommended, bool linkedToActiveSetup = false)
        {
            if (!recommended && !present)
                return PyralisAuthoringEvidenceState.NotRelevant;

            if (!present)
                return PyralisAuthoringEvidenceState.Missing;

            return linkedToActiveSetup
                ? PyralisAuthoringEvidenceState.LinkedToActiveSetup
                : PyralisAuthoringEvidenceState.CandidateDetected;
        }

        private static string BuildIssueCode(PyralisAuthoringSceneSurfaceKind kind, bool present, bool recommended)
        {
            if (!recommended)
                return string.Empty;

            return present
                ? "SceneSurface." + kind + ".Detected"
                : "SceneSurface." + kind + ".Missing";
        }

        private static PyralisAuthoringNativeAction? BuildNativeAction(string surface, bool recommended)
        {
            if (!recommended)
                return null;

            return PyralisAuthoringNativeActionFactory.CreateSceneObjectAction(
                surface,
                string.Empty,
                PyralisAuthoringSceneSurfaceGuidance.GetSuccess(surface),
                PyralisAuthoringSceneSurfaceGuidance.GetExpected(surface));
        }
    }
}
