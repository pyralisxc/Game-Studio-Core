using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Enums;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace NeonBlack.Gameplay.Editor
{
    public sealed class PyralisAuthoringSceneSurfaceRow
    {
        public PyralisAuthoringSceneSurfaceRow(
            string surface,
            bool present,
            bool recommended,
            string current,
            string nextFix,
            PyralisAuthoringEvidenceState evidenceState = PyralisAuthoringEvidenceState.NotRelevant)
        {
            Surface = surface;
            Present = present;
            Recommended = recommended;
            Current = current;
            NextFix = nextFix;
            EvidenceState = evidenceState;
        }

        public string Surface { get; }
        public bool Present { get; }
        public bool Recommended { get; }
        public string Current { get; }
        public string NextFix { get; }
        public PyralisAuthoringEvidenceState EvidenceState { get; }
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
            GameplaySessionBootstrap bootstrap = PyralisAuthoringSetupContextResolver.GetSelectedBootstrap(activeSetup);
            PyralisAuthoringRouteDescriptor route = PyralisAuthoringRouteDescriptor.Build(activeSetup);
            PyralisAuthoringSceneEvidence evidence = PyralisAuthoringSceneEvidence.Build(bootstrap);
            List<PyralisAuthoringSceneSurfaceRow> rows = new List<PyralisAuthoringSceneSurfaceRow>();

            bool wantsWorld = PyralisAuthoringSceneSurfaceGuidance.IsRecommended(route, PyralisAuthoringSceneSurfaceGuidance.EnvironmentPlayfield);
            bool wantsCamera = PyralisAuthoringSceneSurfaceGuidance.IsRecommended(route, PyralisAuthoringSceneSurfaceGuidance.CameraBounds);
            bool wantsUi = PyralisAuthoringSceneSurfaceGuidance.IsRecommended(route, PyralisAuthoringSceneSurfaceGuidance.UiHudMenus);
            bool wantsScoring = PyralisAuthoringSceneSurfaceGuidance.IsRecommended(route, PyralisAuthoringSceneSurfaceGuidance.ScoringObjectives);
            bool wantsActionOrTabletop = PyralisAuthoringSceneSurfaceGuidance.IsRecommended(route, PyralisAuthoringSceneSurfaceGuidance.BoardActionSelection);
            bool wantsHazardsOrPickups = PyralisAuthoringSceneSurfaceGuidance.IsRecommended(route, PyralisAuthoringSceneSurfaceGuidance.PickupsHazardsEnemies);

            bool needsWalkableEnvironmentSurface = IsSideView2DMovementProof(bootstrap);
            bool environmentPresent = wantsWorld && needsWalkableEnvironmentSurface
                ? evidence.HasPlayableEnvironmentSurface
                : evidence.HasEnvironmentSurface;

            rows.Add(new PyralisAuthoringSceneSurfaceRow(
                PyralisAuthoringSceneSurfaceGuidance.EnvironmentPlayfield,
                environmentPresent,
                wantsWorld,
                evidence.GetEnvironmentSummary(),
                PyralisAuthoringSceneSurfaceGuidance.GetNextFix(PyralisAuthoringSceneSurfaceGuidance.EnvironmentPlayfield, wantsWorld),
                GetEvidenceState(environmentPresent, wantsWorld, evidence.LinkedSpawnPointCount > 0)));

            rows.Add(new PyralisAuthoringSceneSurfaceRow(
                PyralisAuthoringSceneSurfaceGuidance.CameraBounds,
                evidence.HasCameraSurface,
                wantsCamera,
                evidence.GetCameraSummary(),
                PyralisAuthoringSceneSurfaceGuidance.GetNextFix(PyralisAuthoringSceneSurfaceGuidance.CameraBounds, wantsCamera),
                GetEvidenceState(evidence.HasCameraSurface, wantsCamera, evidence.LinkedCameraRigCount > 0)));

            rows.Add(new PyralisAuthoringSceneSurfaceRow(
                PyralisAuthoringSceneSurfaceGuidance.UiHudMenus,
                evidence.HasUiSurface,
                wantsUi,
                evidence.GetUiSummary(),
                PyralisAuthoringSceneSurfaceGuidance.GetNextFix(PyralisAuthoringSceneSurfaceGuidance.UiHudMenus, wantsUi),
                GetEvidenceState(evidence.HasUiSurface, wantsUi)));

            rows.Add(new PyralisAuthoringSceneSurfaceRow(
                PyralisAuthoringSceneSurfaceGuidance.ScoringObjectives,
                evidence.ScoreServiceCount > 0,
                wantsScoring,
                evidence.ScoreServiceCount > 0 ? $"{evidence.ScoreServiceCount} score service object(s)" : "No score service detected",
                PyralisAuthoringSceneSurfaceGuidance.GetNextFix(PyralisAuthoringSceneSurfaceGuidance.ScoringObjectives, wantsScoring),
                GetEvidenceState(evidence.ScoreServiceCount > 0, wantsScoring)));

            rows.Add(new PyralisAuthoringSceneSurfaceRow(
                PyralisAuthoringSceneSurfaceGuidance.BoardActionSelection,
                evidence.HasSelectionSurface,
                wantsActionOrTabletop,
                evidence.GetSelectionSummary(),
                PyralisAuthoringSceneSurfaceGuidance.GetNextFix(PyralisAuthoringSceneSurfaceGuidance.BoardActionSelection, wantsActionOrTabletop),
                GetEvidenceState(evidence.HasSelectionSurface, wantsActionOrTabletop)));

            rows.Add(new PyralisAuthoringSceneSurfaceRow(
                PyralisAuthoringSceneSurfaceGuidance.PickupsHazardsEnemies,
                evidence.HasEncounterSurface,
                wantsHazardsOrPickups,
                evidence.GetEncounterSummary(),
                PyralisAuthoringSceneSurfaceGuidance.GetNextFix(PyralisAuthoringSceneSurfaceGuidance.PickupsHazardsEnemies, wantsHazardsOrPickups),
                GetEvidenceState(evidence.HasEncounterSurface, wantsHazardsOrPickups)));

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

        private static bool IsSideView2DMovementProof(GameplaySessionBootstrap bootstrap)
        {
            PawnMovementProfile profile = GetFirstPawnMovementProfile(bootstrap);
            return profile != null
                && profile.movementMode == MovementMode.TwoD
                && profile.use2DPhysics
                && profile.allow2DJump;
        }

        private static PawnMovementProfile GetFirstPawnMovementProfile(GameplaySessionBootstrap bootstrap)
        {
            if (bootstrap == null)
                return null;

            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            SessionDefinition session = serializedBootstrap.FindProperty("sessionDefinition")?.objectReferenceValue as SessionDefinition;
            if (session == null || session.defaultParticipants == null)
                return null;

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                PawnDefinition pawn = participant != null ? participant.defaultPawn : null;
                if (pawn != null && pawn.movementProfile != null)
                    return pawn.movementProfile;
            }

            return null;
        }

    }
}
