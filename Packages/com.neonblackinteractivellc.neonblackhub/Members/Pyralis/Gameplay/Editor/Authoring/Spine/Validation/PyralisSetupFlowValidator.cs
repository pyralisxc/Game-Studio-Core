using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Runtime;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Features.Feedback.UI;
using NeonBlack.Gameplay.Features.GameFlow;
using NeonBlack.Gameplay.Features.Input;
using NeonBlack.Gameplay.Features.Settings;
using NeonBlack.Gameplay.Features.Tabletop;
using NeonBlack.Gameplay.Core.Rules.Board;
using NeonBlack.Gameplay.Presentation.Camera;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    public static class PyralisSetupFlowValidator
    {
        public static PyralisSetupFlowReport BuildReport(GameplaySessionBootstrap bootstrap)
        {
            List<PyralisSetupFlowStep> steps = new List<PyralisSetupFlowStep>();

            if (bootstrap == null)
            {
                steps.Add(new PyralisSetupFlowStep(
                    "Select Gameplay Session Bootstrap",
                    PyralisSetupFlowStepStatus.Missing,
                    "Select a scene object with GameplaySessionBootstrap to inspect setup flow.",
                    stepId: PyralisSetupFlowStepId.SelectGameplaySessionBootstrap));
                return new PyralisSetupFlowReport(steps);
            }

            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            SessionDefinition session = GetObjectReference<SessionDefinition>(serializedBootstrap, "sessionDefinition");
            bool injectLoadedScenesOnBuild = GetBool(serializedBootstrap, "injectLoadedScenesOnBuild");
            int spawnPointCount = GetArraySize(serializedBootstrap, "spawnPoints");
            CinemachineCameraRigController cameraRig = GetObjectReference<CinemachineCameraRigController>(serializedBootstrap, "cameraRigController");
            bool hasCameraRig = cameraRig != null;
            bool hasPlayerInputManager = GetObjectReference<Object>(serializedBootstrap, "playerInputManager") != null;
            bool hasLifetimeScope = bootstrap.GetComponent<PyralisGameplayLifetimeScope>() != null;

            PyralisSetupRouteAnalysis route = PyralisSetupRouteAnalysis.Build(session);
            GameModeDefinition mode = route.Mode;
            GameSetupProfile setupProfile = route.SetupProfile;
            bool hasSelectedCapabilities = route.HasValidPatterns;
            bool requiresPawn = route.RequiresPawn;
            bool hasParticipants = route.HasParticipants;
            bool hasParticipantPawn = route.HasAnyDefaultPawn;
            string participantPawnIssue = route.ParticipantPawnIssue;
            PawnDefinition firstPawn = GetFirstPawnDefinition(session);
            bool hasParticipantInputProfile = HasAnyParticipantInputProfile(session);
            string participantInputProfileIssue = GetParticipantInputIssue(session);
            bool hasUsableParticipantInputProfile = hasParticipantInputProfile && string.IsNullOrWhiteSpace(participantInputProfileIssue);
            bool setupRouteReady = setupProfile != null && hasSelectedCapabilities;
            bool needsCameraRigForFirstProof = setupRouteReady && route.UsesPawnGameplay();
            bool needs2DCameraBounds = setupRouteReady && route.Requires2DCameraBounds();
            bool has2DCameraBounds = !needs2DCameraBounds || HasUsable2DCameraBounds(cameraRig, mode);
            bool hasGameplayStateService = HasSceneService<IGameplayStateReader>(bootstrap, out MonoBehaviour gameplayStateService);
            bool hasCameraBoundsService = HasSceneService<ICameraBoundsProvider>(bootstrap, out MonoBehaviour cameraBoundsService);
            bool hasScoreService = HasSceneService<ISessionScoreService>(bootstrap, out MonoBehaviour scoreService);
            bool hasSettingsManager = HasSceneComponent<SettingsManager>(bootstrap, out SettingsManager settingsManager);
            bool hasProjectileLauncher = HasSceneComponent<ProjectileLauncherBase>(bootstrap, out ProjectileLauncherBase projectileLauncher);
            bool hasTabletopGridPresenter = HasSceneComponent<TabletopBoardGridPresenter>(bootstrap, out TabletopBoardGridPresenter tabletopGridPresenter);
            bool hasTabletopSelectionBridge = HasSceneComponent<TabletopBoardSelectionBridge>(bootstrap, out TabletopBoardSelectionBridge tabletopSelectionBridge);
            bool hasTabletopContract = HasTabletopRuntimeContract(mode, tabletopGridPresenter, out Object tabletopContractReference);
            bool hasTabletopSelectionSurface = hasTabletopGridPresenter || hasTabletopSelectionBridge;
            Object tabletopSelectionReference = tabletopGridPresenter != null
                ? tabletopGridPresenter
                : tabletopSelectionBridge != null
                    ? tabletopSelectionBridge
                    : (Object)setupProfile;
            bool hasCanvas = HasSceneComponent<Canvas>(bootstrap, out Canvas canvas);
            bool hasUiManager = HasSceneComponent<UIManager>(bootstrap, out UIManager uiManager);
            bool hasFeedbackHud = HasSceneComponent<ParticipantFeedbackHudPresenter>(bootstrap, out ParticipantFeedbackHudPresenter feedbackHud);
            bool hasHealthHud = HasSceneComponent<ParticipantHealthHudBinder>(bootstrap, out ParticipantHealthHudBinder healthHud);
            bool hasHudSurface = hasUiManager || hasFeedbackHud || hasHealthHud;
            Object hudReference = uiManager != null
                ? uiManager
                : feedbackHud != null
                    ? feedbackHud
                    : healthHud != null
                        ? healthHud
                        : canvas != null
                            ? canvas
                            : (Object)bootstrap;
            PyralisRuntimeSystemClaimReport runtimeSystemClaimReport = PyralisRuntimeSystemClaimResolver.BuildReport(
                route.RequiredRuntimeSystems,
                new PyralisRuntimeSystemClaimContext(
                    participantPawnIssue,
                    hasProjectileLauncher,
                    hasScoreService,
                    mode != null && mode.enableScore));
            PyralisSceneReadinessReport sceneReadinessReport = PyralisSceneReadinessValidator.BuildReport(bootstrap);

            steps.Add(new PyralisSetupFlowStep(
                "Gameplay Root",
                PyralisSetupFlowStepStatus.Ready,
                "Selected object has GameplaySessionBootstrap.",
                bootstrap,
                stepId: PyralisSetupFlowStepId.GameplayRoot));

            steps.Add(new PyralisSetupFlowStep(
                "Visible Lifetime Scope",
                hasLifetimeScope ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Recommended,
                hasLifetimeScope
                    ? "PyralisGameplayLifetimeScope is visible on this root."
                    : "Runtime can create this automatically, but adding it now makes the supported composition root easier to inspect.",
                hasLifetimeScope ? (Object)bootstrap.GetComponent<PyralisGameplayLifetimeScope>() : bootstrap.gameObject,
                hasLifetimeScope ? PyralisSetupFlowActionKind.SelectObject : PyralisSetupFlowActionKind.AddLifetimeScope,
                stepId: PyralisSetupFlowStepId.VisibleLifetimeScope));

            steps.Add(new PyralisSetupFlowStep(
                "First-Scene Defaults",
                injectLoadedScenesOnBuild ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Recommended,
                injectLoadedScenesOnBuild
                    ? "Bootstrap startup ownership and loaded-scene injection are ready for a first proof."
                    : "For first-scene proofs, keep bootstrap startup ownership on this root and inject loaded scenes unless this intent deliberately uses a custom composition flow.",
                bootstrap,
                injectLoadedScenesOnBuild ? PyralisSetupFlowActionKind.SelectObject : PyralisSetupFlowActionKind.RestoreFirstSceneDefaults,
                stepId: PyralisSetupFlowStepId.FirstSceneDefaults));

            steps.Add(new PyralisSetupFlowStep(
                "Runtime Service Ownership",
                PyralisSetupFlowStepStatus.Ready,
                "GameplaySessionBootstrap builds PyralisGameplayLifetimeScope as the singular source of truth for runtime services. Systems depend on direct VContainer injection, not hidden global lookups.",
                bootstrap,
                PyralisSetupFlowActionKind.SelectObject,
                stepId: PyralisSetupFlowStepId.RuntimeServiceOwnership));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Session Definition",
                session != null ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Missing,
                session != null ? "Bootstrap can read a SessionDefinition." : "Assign the SessionDefinition this scene should start.",
                session,
                stepId: PyralisSetupFlowStepId.AssignSessionDefinition));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Default Game Mode",
                GetDependentStatus(session != null, mode != null),
                session == null
                    ? "Assign Session Definition first."
                    : mode != null ? "Session has a default GameModeDefinition." : "Assign SessionDefinition > Default Game Mode.",
                mode,
                stepId: PyralisSetupFlowStepId.AssignDefaultGameMode));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Setup Profile",
                GetDependentStatus(mode != null, setupProfile != null),
                mode == null
                    ? "Assign Default Game Mode first."
                    : setupProfile != null ? "Game mode has a GameSetupProfile." : "Assign GameModeDefinition > Setup Profile.",
                setupProfile,
                stepId: PyralisSetupFlowStepId.AssignSetupProfile));

            steps.Add(new PyralisSetupFlowStep(
                "Choose Capabilities",
                GetDependentStatus(setupProfile != null, hasSelectedCapabilities),
                setupProfile == null
                    ? "Assign Setup Profile first."
                    : !route.HasAssignedPatterns ? "Open Authoring Window -> Intent, choose DNA axioms and Engine Spine capabilities, then keep the GameSetupProfile active so those choices save to runtime capabilities." : hasSelectedCapabilities ? "Setup profile has selected capability intent." : "Fix setup capability validation issues before continuing.",
                setupProfile,
                stepId: PyralisSetupFlowStepId.AddRuntimePatterns));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Default Participants",
                GetDependentStatus(session != null, hasParticipants),
                session == null
                    ? "Assign Session Definition first."
                    : hasParticipants ? "Session has default participants." : "Assign at least one default participant, seat, hand, faction, AI, or player.",
                session,
                stepId: PyralisSetupFlowStepId.AssignDefaultParticipants));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Participant Pawn",
                GetParticipantPawnStatus(setupRouteReady, requiresPawn, hasParticipantPawn, participantPawnIssue),
                GetParticipantPawnMessage(setupRouteReady, requiresPawn, hasParticipantPawn, participantPawnIssue),
                session,
                stepId: PyralisSetupFlowStepId.AssignParticipantPawn,
                nativeAction: PyralisSetupFlowGuidance.GetPawnNativeAction(route.ParticipantPawnIssueKind)));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Input Profile",
                GetParticipantInputProfileStatus(setupRouteReady, requiresPawn, hasParticipants, hasUsableParticipantInputProfile),
                GetParticipantInputProfileMessage(setupRouteReady, requiresPawn, hasParticipants, session, hasParticipantInputProfile, participantInputProfileIssue),
                GetInputProfileReference(session),
                stepId: PyralisSetupFlowStepId.AssignInputProfile));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Spawn Points",
                GetSpawnPointStatus(setupRouteReady, requiresPawn, spawnPointCount),
                GetSpawnPointMessage(setupRouteReady, requiresPawn, spawnPointCount),
                bootstrap,
                stepId: PyralisSetupFlowStepId.AssignSpawnPoints));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Camera Rig",
                GetCameraRigStatus(setupRouteReady, needsCameraRigForFirstProof, route.UsesCamera(), hasCameraRig, has2DCameraBounds),
                GetCameraRigMessage(setupRouteReady, needsCameraRigForFirstProof, needs2DCameraBounds, route.UsesCamera(), hasCameraRig, has2DCameraBounds),
                cameraRig,
                stepId: PyralisSetupFlowStepId.AssignCameraRig));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Player Input Manager",
                GetRequiredRouteServiceStatus(setupRouteReady, route.LikelyUsesInputManager(), hasPlayerInputManager),
                GetPlayerInputMessage(setupRouteReady, route.LikelyUsesInputManager(), hasPlayerInputManager),
                GetObjectReference<Object>(serializedBootstrap, "playerInputManager"),
                stepId: PyralisSetupFlowStepId.AssignPlayerInputManager));

            steps.Add(new PyralisSetupFlowStep(
                "Tune Camera Framing",
                GetCustomizationStatus(setupRouteReady, route.UsesPawnGameplay() || route.UsesCamera() || route.UsesPlayfield(), hasCameraRig),
                GetCameraCustomizationMessage(setupRouteReady, route.UsesPawnGameplay() || route.UsesCamera() || route.UsesPlayfield(), hasCameraRig),
                cameraRig != null ? (Object)cameraRig : mode != null ? (Object)mode.cameraRigProfile : null,
                stepId: PyralisSetupFlowStepId.TuneCameraFraming));

            steps.Add(new PyralisSetupFlowStep(
                "Tune Pawn Visuals And Collision",
                GetCustomizationStatus(setupRouteReady, route.UsesPawnGameplay(), firstPawn != null && firstPawn.pawnPrefab != null),
                GetPawnCustomizationMessage(setupRouteReady, route.UsesPawnGameplay(), firstPawn),
                firstPawn != null && firstPawn.pawnPrefab != null ? (Object)firstPawn.pawnPrefab : (Object)firstPawn,
                stepId: PyralisSetupFlowStepId.TunePawnVisualsAndCollision));

            steps.Add(new PyralisSetupFlowStep(
                "Tune Movement And Input Feel",
                GetCustomizationStatus(setupRouteReady, route.UsesPawnGameplay(), firstPawn != null),
                GetMovementCustomizationMessage(setupRouteReady, route.UsesPawnGameplay(), firstPawn),
                GetMovementCustomizationReference(firstPawn, session),
                stepId: PyralisSetupFlowStepId.TuneMovementAndInputFeel));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Playfield Profile",
                GetRecommendationStatus(setupRouteReady, route.UsesPlayfield(), mode != null && mode.playfieldProfile != null),
                GetPlayfieldMessage(setupRouteReady, route.UsesPlayfield(), mode != null && mode.playfieldProfile != null),
                mode != null ? mode.playfieldProfile : null,
                stepId: PyralisSetupFlowStepId.AssignPlayfieldProfile));

            steps.Add(new PyralisSetupFlowStep(
                "Enable Scoring Route",
                GetRequiredRouteServiceStatus(setupRouteReady, route.UsesScoring(), mode != null && mode.enableScore),
                GetScoringMessage(setupRouteReady, route.UsesScoring(), mode != null && mode.enableScore),
                mode,
                stepId: PyralisSetupFlowStepId.EnableScoringRoute));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Gameplay State Service",
                GetGameplayStateServiceStatus(
                    setupRouteReady,
                    route.UsesPawnGameplay() || route.UsesScoring(),
                    hasGameplayStateService),
                GetGameplayStateServiceMessage(
                    setupRouteReady,
                    route.UsesPawnGameplay() || route.UsesScoring(),
                    hasGameplayStateService),
                gameplayStateService,
                stepId: PyralisSetupFlowStepId.AssignGameplayStateService));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Camera Bounds Service",
                GetRecommendationStatus(setupRouteReady, route.UsesCamera() || route.UsesPlayfield(), hasCameraBoundsService),
                GetCameraBoundsServiceMessage(setupRouteReady, route.UsesCamera() || route.UsesPlayfield(), hasCameraBoundsService),
                cameraBoundsService,
                stepId: PyralisSetupFlowStepId.AssignCameraBoundsService));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Score Service",
                GetRequiredRouteServiceStatus(setupRouteReady, route.UsesScoring(), hasScoreService),
                GetScoreServiceMessage(setupRouteReady, route.UsesScoring(), hasScoreService),
                scoreService,
                stepId: PyralisSetupFlowStepId.AssignScoreService));

            steps.Add(new PyralisSetupFlowStep(
                "Assign HUD / UI Surface",
                GetHudSurfaceStatus(setupRouteReady, route, hasCanvas, hasHudSurface),
                GetHudSurfaceMessage(setupRouteReady, route, hasCanvas, hasHudSurface),
                hudReference,
                stepId: PyralisSetupFlowStepId.AddHudOrMenuSurface));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Projectile Launcher",
                GetRecommendationStatus(setupRouteReady, route.UsesProjectileCombat(), hasProjectileLauncher),
                GetProjectileLauncherMessage(setupRouteReady, route.UsesProjectileCombat(), hasProjectileLauncher),
                projectileLauncher,
                stepId: PyralisSetupFlowStepId.AddProjectileLauncher));

            steps.Add(new PyralisSetupFlowStep(
                "Tabletop Runtime Contract",
                GetTabletopContractStatus(setupRouteReady, route.UsesTabletopContract(), hasTabletopContract),
                GetTabletopContractMessage(setupRouteReady, route.UsesTabletopContract(), hasTabletopContract),
                tabletopContractReference != null ? tabletopContractReference : setupProfile,
                stepId: PyralisSetupFlowStepId.TabletopRuntimeContract));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Tabletop Selection Surface",
                GetTabletopSelectionSurfaceStatus(setupRouteReady, route.UsesTabletopContract(), hasTabletopSelectionSurface),
                GetTabletopSelectionSurfaceMessage(setupRouteReady, route.UsesTabletopContract(), hasTabletopSelectionSurface),
                tabletopSelectionReference,
                stepId: PyralisSetupFlowStepId.TabletopSelectionSurface));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Settings Manager",
                GetRecommendationStatus(setupRouteReady, route.UsesPawnGameplay() || route.UsesScoring(), hasSettingsManager),
                hasSettingsManager ? "Settings Manager is present in the scene." : "A SettingsManager is recommended for managing audio volume, deadzones, and control swaps.",
                settingsManager,
                stepId: PyralisSetupFlowStepId.AssignSettingsManager));

            // Reflective contracts derived from AuthoringContract attributes
            var reflectiveReport = PyralisReflectiveContractSolver.BuildReport(bootstrap);
            steps.AddRange(reflectiveReport.Steps);

            steps.Add(new PyralisSetupFlowStep(
                "Resolve Runtime System Claims",
                GetRuntimeSystemClaimsStatus(setupRouteReady, runtimeSystemClaimReport),
                GetRuntimeSystemClaimsMessage(setupRouteReady, runtimeSystemClaimReport),
                setupProfile));

            steps.Add(new PyralisSetupFlowStep(
                "Scene And Prefab Readiness",
                GetSceneReadinessStatus(setupRouteReady, sceneReadinessReport),
                GetSceneReadinessMessage(setupRouteReady, sceneReadinessReport),
                sceneReadinessReport != null && !sceneReadinessReport.IsReady ? (Object)bootstrap : (Object)session,
                stepId: PyralisSetupFlowStepId.SceneAndPrefabReadiness));

            return new PyralisSetupFlowReport(steps);
        }

        private static PyralisSetupFlowStepStatus GetDependentStatus(bool dependencyReady, bool ready)
        {
            if (!dependencyReady)
                return PyralisSetupFlowStepStatus.Blocked;

            return ready ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Missing;
        }

        private static PyralisSetupFlowStepStatus GetParticipantPawnStatus(bool setupReady, bool requiresPawn, bool hasParticipantPawn, string participantPawnIssue)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            if (!requiresPawn)
            {
                if (!hasParticipantPawn)
                    return PyralisSetupFlowStepStatus.Optional;

                return string.IsNullOrWhiteSpace(participantPawnIssue)
                    ? PyralisSetupFlowStepStatus.Ready
                    : PyralisSetupFlowStepStatus.Recommended;
            }

            return hasParticipantPawn && string.IsNullOrWhiteSpace(participantPawnIssue)
                ? PyralisSetupFlowStepStatus.Ready
                : PyralisSetupFlowStepStatus.Missing;
        }

        private static PyralisSetupFlowStepStatus GetParticipantInputProfileStatus(
            bool setupReady,
            bool requiresPawn,
            bool hasParticipants,
            bool hasInputProfile)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            if (!requiresPawn)
                return PyralisSetupFlowStepStatus.Optional;

            if (!hasParticipants)
                return PyralisSetupFlowStepStatus.Blocked;

            return hasInputProfile
                ? PyralisSetupFlowStepStatus.Ready
                : PyralisSetupFlowStepStatus.Missing;
        }

        private static PyralisSetupFlowStepStatus GetSpawnPointStatus(bool setupReady, bool requiresPawn, int spawnPointCount)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            if (!requiresPawn)
                return spawnPointCount > 0 ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Optional;

            return spawnPointCount > 0 ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Missing;
        }

        private static PyralisSetupFlowStepStatus GetRecommendationStatus(bool setupReady, bool recommended, bool ready)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            if (!recommended)
                return ready ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Optional;

            return ready ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Recommended;
        }

        private static PyralisSetupFlowStepStatus GetCustomizationStatus(bool setupReady, bool relevant, bool hasTarget)
        {
            if (!setupReady || !relevant)
                return PyralisSetupFlowStepStatus.Optional;

            return hasTarget ? PyralisSetupFlowStepStatus.Recommended : PyralisSetupFlowStepStatus.Optional;
        }

        private static PyralisSetupFlowStepStatus GetCameraRigStatus(bool setupReady, bool requiredForFirstProof, bool recommended, bool ready, bool usable2DBounds)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            if (requiredForFirstProof)
                return ready && usable2DBounds ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Missing;

            if (!recommended)
                return ready ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Optional;

            return ready ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Recommended;
        }

        private static PyralisSetupFlowStepStatus GetRequiredRouteServiceStatus(bool setupReady, bool required, bool ready)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            if (!required)
                return ready ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Optional;

            return ready ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Missing;
        }

        private static PyralisSetupFlowStepStatus GetTabletopContractStatus(bool setupReady, bool usesTabletopContract, bool hasTabletopContract)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            if (!usesTabletopContract)
                return PyralisSetupFlowStepStatus.Optional;

            return hasTabletopContract ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Missing;
        }

        private static PyralisSetupFlowStepStatus GetTabletopSelectionSurfaceStatus(bool setupReady, bool usesTabletopContract, bool hasTabletopSelectionSurface)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            if (!usesTabletopContract)
                return PyralisSetupFlowStepStatus.Optional;

            return hasTabletopSelectionSurface ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Recommended;
        }

        private static PyralisSetupFlowStepStatus GetRuntimeSystemClaimsStatus(bool setupReady, PyralisRuntimeSystemClaimReport report)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            if (report == null || !report.HasDeclaredClaims)
                return PyralisSetupFlowStepStatus.Optional;

            return report.HasUnverifiedClaims
                ? PyralisSetupFlowStepStatus.Recommended
                : PyralisSetupFlowStepStatus.Ready;
        }

        private static string GetRuntimeSystemClaimsMessage(bool setupReady, PyralisRuntimeSystemClaimReport report)
        {
            if (!setupReady)
                return "Choose setup capabilities before resolving declared runtime system claims.";

            if (report == null || !report.HasDeclaredClaims)
                return "No explicit Required Runtime Systems are declared by optional route contracts.";

            if (!report.HasUnverifiedClaims)
                return "Declared Required Runtime Systems are covered by bootstrap services, pawn validation, or concrete scene-service checks.";

            return "These declared Required Runtime Systems still need project verification or deeper prefab checks: " + report.UnverifiedSummary + ".";
        }

        private static PyralisSetupFlowStepStatus GetSceneReadinessStatus(bool setupReady, PyralisSceneReadinessReport report)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            if (report == null || !report.IsReady)
                return PyralisSetupFlowStepStatus.Missing;

            return report.HasRecommendations
                ? PyralisSetupFlowStepStatus.Recommended
                : PyralisSetupFlowStepStatus.Ready;
        }

        private static string GetSceneReadinessMessage(bool setupReady, PyralisSceneReadinessReport report)
        {
            if (!setupReady)
                return "Choose a valid setup profile and capability intent before checking scene and prefab readiness.";

            if (report == null)
                return "Scene and prefab readiness could not be evaluated.";

            if (!report.IsReady)
                return "Do not enter Play Mode yet. Fix required scene/prefab issue: " + report.RequiredSummary + ".";

            if (report.HasRecommendations)
                return "Required scene/prefab checks are clear for a narrow proof. Recommended follow-up: " + report.RecommendedSummary + ".";

            return "Scene and prefab readiness checks are clear. Play Mode can now test the proof instead of revealing missing setup.";
        }

        private static string GetParticipantPawnMessage(bool setupReady, bool requiresPawn, bool hasParticipantPawn, string participantPawnIssue)
        {
            if (!setupReady)
                return "Choose setup capabilities before deciding whether participants need pawns.";

            if (!requiresPawn)
            {
                if (!hasParticipantPawn)
                    return "No participant pawn is required for this setup route.";

                if (!string.IsNullOrWhiteSpace(participantPawnIssue))
                    return participantPawnIssue;

                return hasParticipantPawn
                    ? "A participant has a pawn, which is allowed for this setup."
                    : "No participant pawn is required for this setup route.";
            }

            if (!string.IsNullOrWhiteSpace(participantPawnIssue))
                return participantPawnIssue;

            return hasParticipantPawn
                ? "At least one default participant has a pawn."
                : "Selected setup requires pawn-backed participants. Assign a PawnDefinition to a default participant.";
        }

        private static string GetParticipantInputProfileMessage(
            bool setupReady,
            bool requiresPawn,
            bool hasParticipants,
            SessionDefinition session,
            bool hasInputProfile,
            string inputProfileIssue)
        {
            if (!setupReady)
                return "Choose setup capabilities before deciding whether input profiles are required.";

            if (!hasParticipants)
                return "Assign participants first, then assign input profiles.";

            if (!hasInputProfile)
            {
                return requiresPawn
                    ? "Assign InputProfile on `SessionDefinition.defaultParticipants[0]` (or set `SessionDefinition.defaultInputProfile`) in Inspector before routing movement."
                    : "Input profile is optional for this route unless a built-in player/input surface is used.";
            }

            if (!string.IsNullOrWhiteSpace(inputProfileIssue))
                return inputProfileIssue;

            if (session == null || session.defaultInputProfile == null)
                return "A participant InputProfile is assigned. Pawn/input readers can now bind control signals.";

            return "InputProfile is assigned. Participant values are used before SessionDefinition.defaultInputProfile fallback.";
        }

        private static Object GetInputProfileReference(SessionDefinition session)
        {
            if (session == null)
                return null;

            if (session.defaultInputProfile != null)
                return session.defaultInputProfile;

            if (session.defaultParticipants == null)
                return session;

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                if (participant != null && participant.inputProfile != null)
                    return participant;
            }

            return session;
        }

        private static Object GetMovementCustomizationReference(PawnDefinition pawn, SessionDefinition session)
        {
            if (pawn != null && pawn.movementProfile != null)
                return pawn.movementProfile;

            Object inputProfileReference = GetInputProfileReference(session);
            return inputProfileReference != null ? inputProfileReference : (Object)pawn;
        }

        private static PawnDefinition GetFirstPawnDefinition(SessionDefinition session)
        {
            if (session == null || session.defaultParticipants == null)
                return null;

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                if (participant != null && participant.defaultPawn != null)
                    return participant.defaultPawn;
            }

            return null;
        }

        private static string GetParticipantInputIssue(SessionDefinition session)
        {
            if (session == null || session.defaultParticipants == null || session.defaultParticipants.Length == 0)
                return "Assign default participants first.";

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                if (participant == null)
                    continue;

                PawnDefinition pawn = participant.defaultPawn;
                InputProfile effectiveProfile = ParticipantInputProfileUtility.ResolveEffectiveInputProfile(
                    participant,
                    pawn,
                    session.defaultInputProfile);

                if (effectiveProfile == null)
                    return "Add InputProfile to one participant, its PawnDefinition, or SessionDefinition.defaultInputProfile before trying movement in Play Mode.";

                string bindingIssue = GetInputProfileBindingIssue(effectiveProfile);
                if (!string.IsNullOrWhiteSpace(bindingIssue))
                    return $"Participant `{participant.displayName}` effective InputProfile `{effectiveProfile.name}`: {bindingIssue}";
            }

            return string.Empty;
        }

        private static string GetInputProfileBindingIssue(InputProfile profile)
        {
            if (profile == null)
                return "Assign an InputProfile before trying movement in Play Mode.";

            profile.Sanitize();

            if (profile.actions == null)
                return "assign Actions to the stock Assets/InputSystem_Actions.inputactions asset, or choose a custom Unity Input Action Asset for an advanced input layout.";

            InputActionMap actionMap = ParticipantInputProfileUtility.FindGameplayActionMap(profile.actions, profile);
            if (actionMap == null)
            {
                string mapName = !string.IsNullOrWhiteSpace(profile.primaryActionMap)
                    ? profile.primaryActionMap
                    : "Player";
                return $"Primary Action Map `{mapName}` was not found in Actions.";
            }

            GameplayInputActionBinding moveBinding = profile.FindBinding(GameplayInputActionRole.Move);
            if (moveBinding == null)
                return "add a required Move row to Gameplay Actions.";

            if (string.IsNullOrWhiteSpace(moveBinding.actionName))
                return "set the Move row Unity Action Name to the action that drives movement.";

            InputActionMap moveMap = actionMap;
            string moveMapName = moveBinding.GetActionMap(profile.primaryActionMap);
            if (!string.Equals(moveMapName, actionMap.name, System.StringComparison.OrdinalIgnoreCase))
                moveMap = profile.actions.FindActionMap(moveMapName, throwIfNotFound: false);

            if (moveMap == null)
                return $"Move row Action Map `{moveMapName}` was not found in Actions.";

            if (ParticipantInputProfileUtility.FindAction(moveMap, moveBinding.actionName) == null)
                return $"Move row Unity Action Name `{moveBinding.actionName}` was not found in Action Map `{moveMap.name}`.";

            if (!profile.supportsGamepad && !profile.supportsKeyboardMouse && !profile.touchFriendly)
                return "enable at least one supported input surface such as keyboard/mouse, gamepad, or touch.";

            return string.Empty;
        }

        private static string GetSpawnPointMessage(bool setupReady, bool requiresPawn, int spawnPointCount)
        {
            if (!setupReady)
                return "Choose setup capabilities before deciding whether spawn points are required.";

            if (!requiresPawn)
                return spawnPointCount > 0
                    ? "Spawn points are assigned, which is allowed when this setup spawns actor bodies."
                    : "Spawn points can stay empty for no-pawn board/card/menu/camera routes.";

            return spawnPointCount > 0
                ? "Spawn points are assigned for pawn-backed participants."
                : "Selected setup requires pawns. Add spawn point transforms to the bootstrap.";
        }

        private static string GetCameraRigMessage(bool setupReady, bool requiredForFirstProof, bool requires2DBounds, bool recommended, bool ready, bool usable2DBounds)
        {
            if (!setupReady)
                return "Choose setup capabilities before deciding camera rig wiring.";

            if (requiredForFirstProof)
            {
                if (!ready)
                    return "Pawn movement needs camera bounds before the first Play Mode proof. Keep or create one physical Unity Camera, usually the default Main Camera; do not delete it for the normal Cinemachine route. Create Camera Root, add CinemachineCameraRigController, create or choose a separate Cinemachine Camera for Shared Camera Behaviour, verify the physical Main Camera is tagged MainCamera with Cinemachine Brain, assign that physical camera as Target Camera, then drag Camera Root from Hierarchy into Bootstrap > Camera Rig Controller.";

                if (!requires2DBounds)
                    return "Camera rig is assigned for the pawn movement proof. This route uses a 3D, 2.5D, or non-orthographic pawn lane, so 2D orthographic bounds are not required before Play Mode.";

                return usable2DBounds
                    ? "Camera rig is assigned with usable 2D bounds for the pawn movement proof."
                    : "Camera rig is assigned, but the 2D movement proof still needs orthographic bounds. Select Camera Root and assign an orthographic CameraRigProfile, or select the physical Target Camera and set Camera > Projection to Orthographic. If using a profile, also assign it to GameModeDefinition > Camera Rig Profile.";
            }

            if (!recommended)
                return ready
                    ? "Camera rig is assigned."
                    : "Camera rig is optional for this setup route. Add it later if the player controls a view, cursor, selector, board camera, or follow camera.";

            return ready
                ? "Camera rig is assigned for camera/cursor flow."
                : "Selected setup uses camera/cursor flow. Create or choose a Camera Rig Profile in your project folderbase. In the Hierarchy, keep or create one physical Unity Camera, usually the default Main Camera, then create Camera Root, add CinemachineCameraRigController, and create or choose a separate Cinemachine Camera for Shared Camera Behaviour. Verify the physical Main Camera keeps the MainCamera tag and Cinemachine Brain, then assign Camera Rig Profile, Shared Camera Behaviour, and Target Camera before dragging Camera Root into Bootstrap > Camera Rig Controller. For 2D proofs, set the physical Target Camera Projection to Orthographic or use an orthographic CameraRigProfile, then tune 2D Bounds Framing on the rig.";
        }

        private static string GetCameraCustomizationMessage(bool setupReady, bool relevant, bool hasCameraRig)
        {
            if (!setupReady)
                return "Choose setup capabilities before deciding camera customization.";

            if (!relevant)
                return "Camera framing can wait until this route uses a pawn, camera, cursor, board view, playfield, or follow camera.";

            return hasCameraRig
                ? "Before judging Play Mode, select Camera Root and CameraRigProfile and tune framing for the scene: physical Target Camera assignment, MainCamera tag/Brain on that physical camera, orthographic size, 2D Bounds Framing minimum visible area, Follow Damping (0 means no lag), Follow Offset, View Euler Angles for pitch/yaw/roll, and how much room the player needs around the pawn. In orthographic mode, CameraRigProfile > Orthographic Size controls zoom only until Camera Root > Enforce Minimum Visible Area 2D raises it to fit the authored min world size. Keep Use Profile Transform on for profile-driven framing, or turn it off when you want direct Cinemachine transform authoring. In Play Mode, Cinemachine follows a runtime GameplaySharedCameraFocus driven from participants; prove follow by moving the pawn, then verifying the Game view follows that shared focus."
                : "Tune camera framing after the Camera Root exists. The Authoring Window should keep this visible so the proof is judged against the intended view, not a default camera accident.";
        }

        private static string GetPawnCustomizationMessage(bool setupReady, bool relevant, PawnDefinition pawn)
        {
            if (!setupReady)
                return "Choose setup capabilities before deciding pawn customization.";

            if (!relevant)
                return "Pawn visuals and colliders can wait because this route does not currently need actor bodies.";

            if (pawn == null || pawn.pawnPrefab == null)
                return "Tune pawn visuals and colliders after the PawnDefinition points to a prefab.";

            return "Before judging Play Mode, open the pawn prefab and check the obvious Unity-owned fit: SpriteRenderer/art placement, visual child offset, Collider2D or Collider shape/size, Rigidbody2D settings, sorting, and whether the pivot matches the intended feet/body position.";
        }

        private static string GetMovementCustomizationMessage(bool setupReady, bool relevant, PawnDefinition pawn)
        {
            if (!setupReady)
                return "Choose setup capabilities before deciding movement customization.";

            if (!relevant)
                return "Movement and input tuning can wait because this route does not currently need pawn control.";

            if (pawn == null)
                return "Tune movement and input after a ParticipantDefinition references a PawnDefinition.";

            return "Before judging Play Mode, inspect the PawnMovementProfile, effective InputProfile, and installed FeatureModuleDefinition assets. Use top-down 2D defaults for free X/Y movement, add a top-down hop feature when Jump should lift the visual while staying map-plane grounded, or use side-view 2D settings when Jump should drive Rigidbody2D vertical motion. The InputProfile maps Unity Input Actions into semantic roles; the pawn prefab still needs an input module such as Motor2DInputAdapter to dispatch those roles.";
        }

        private static bool HasUsable2DCameraBounds(CinemachineCameraRigController rig, GameModeDefinition mode)
        {
            if (rig == null)
                return false;

            SerializedObject serializedRig = new SerializedObject(rig);
            CameraRigProfile rigProfile = serializedRig.FindProperty("cameraRigProfile")?.objectReferenceValue as CameraRigProfile;
            if (rigProfile != null)
                return rigProfile.orthographic;

            if (mode != null && mode.cameraRigProfile != null && mode.cameraRigProfile.orthographic)
                return true;

            Camera targetCamera = serializedRig.FindProperty("targetCamera")?.objectReferenceValue as Camera;
            if (targetCamera != null)
                return targetCamera.orthographic;

            Camera childCamera = rig.GetComponentInChildren<Camera>(true);
            return childCamera != null && childCamera.orthographic;
        }

        private static string GetPlayerInputMessage(bool setupReady, bool recommended, bool ready)
        {
            if (!setupReady)
                return "Choose setup capabilities before deciding local join wiring.";

            if (!recommended)
                return ready
                    ? "PlayerInputManager is assigned."
                    : "PlayerInputManager is optional for single-player, AI-only, menu-only, and no-join prototypes. Add it only when multiple local players can join.";

            return ready
                ? "PlayerInputManager is assigned for local join, and ParticipantInputRouter will subscribe to join/leave events."
                : "Selected setup looks like multi-participant local join. For a 1P proof, select/open the SessionDefinition asset and set Max Participants to 1. For local join, create an Input Root, add Unity PlayerInputManager, assign a dedicated PlayerInput prefab, configure Join Behavior/Input Actions, then drag the component into Bootstrap > Player Input Manager.";
        }

        private static string GetPlayfieldMessage(bool setupReady, bool recommended, bool ready)
        {
            if (!setupReady)
                return "Choose setup capabilities before deciding playfield wiring.";

            if (!recommended)
                return ready
                    ? "Playfield profile is assigned."
                    : "Playfield profile is optional until the route needs authored bounds, board spaces, lanes, zones, or generated areas.";

            return ready
                ? "Playfield profile is assigned."
                : "Add a playfield profile when this setup needs bounds, board spaces, lanes, zones, or generated areas. Put the authored playfield reference on GameModeDefinition > Playfield Profile, then create matching scene anchors or presenters under a Playfield Root.";
        }

        private static string GetScoringMessage(bool setupReady, bool recommended, bool ready)
        {
            if (!setupReady)
                return "Choose setup capabilities before deciding scoring wiring.";

            if (!recommended)
                return ready ? "Scoring is enabled." : "Scoring can stay disabled for this setup route.";

            return ready ? "Scoring route is enabled." : "Selected setup uses scoring/objectives. Enable scoring when score systems are part of the first playable loop.";
        }

        private static PyralisSetupFlowStepStatus GetGameplayStateServiceStatus(
            bool setupReady,
            bool required,
            bool ready)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            if (!required)
                return ready ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Optional;

            return PyralisSetupFlowStepStatus.Ready;
        }

        private static string GetGameplayStateServiceMessage(
            bool setupReady,
            bool required,
            bool ready)
        {
            if (!setupReady)
                return "Choose setup capabilities before deciding gameplay state service wiring.";

            if (!required)
            {
                return ready ? "Gameplay state service is present." : "Gameplay state service is optional for this setup route.";
            }

            return ready
                ? "Scene has an IGameplayStateReader for active/dead/game-over aware systems."
                : "GameplaySessionBootstrap provisions SessionStateService through the supported startup path; add a custom IGameplayStateReader only when this intent deliberately owns gameplay state differently.";

        }

        private static string GetCameraBoundsServiceMessage(bool setupReady, bool recommended, bool ready)
        {
            if (!setupReady)
                return "Choose setup capabilities before deciding camera bounds service wiring.";

            if (!recommended)
                return ready ? "Camera bounds provider is present." : "Camera bounds provider is optional until the selected intent uses framing, camera-aware spawning, hazards, pickups, or bounded playfield behavior.";

            return ready
                ? "Scene has an ICameraBoundsProvider for the selected camera/playfield proof."
                : "Selected intent includes camera or bounds behavior. Assign CinemachineCameraRigController to Bootstrap > Camera Rig Controller, or use Camera Bounds Source only for a specialized custom ICameraBoundsProvider.";
        }

        private static string GetScoreServiceMessage(bool setupReady, bool required, bool ready)
        {
            if (!setupReady)
                return "Choose setup capabilities before deciding score service wiring.";

            if (!required)
                return ready ? "Score service is present." : "Score service is optional for this setup route.";

            return ready
                ? "Scene has an ISessionScoreService for score/objective runtime."
                : "Selected setup claims scoring/objectives. Add ParticipantScoreService or another ISessionScoreService before treating this route as playable.";
        }

        private static PyralisSetupFlowStepStatus GetHudSurfaceStatus(bool setupReady, PyralisSetupRouteAnalysis route, bool hasCanvas, bool ready)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            bool recommended = route != null
                && (route.UsesScoring()
                    || route.UsesPawnGameplay()
                    || route.UsesTabletopContract()
                    || route.UsesActionSelection()
                    || route.RequiresRuntimeSystem("HUD")
                    || route.RequiresRuntimeSystem("UI"));

            if (!recommended)
                return ready || hasCanvas ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Optional;

            return ready ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Recommended;
        }

        private static string GetHudSurfaceMessage(bool setupReady, PyralisSetupRouteAnalysis route, bool hasCanvas, bool ready)
        {
            if (!setupReady)
                return "Choose setup capabilities before deciding HUD or menu surfaces.";

            if (ready)
                return "Scene has a Pyralis HUD/UI surface. Verify its Canvas, EventSystem, labels, panels, buttons, and service references in the Inspector.";

            if (hasCanvas)
                return "Scene has a Canvas, but no known Pyralis HUD/menu presenter yet. Add ParticipantHealthHudBinder for pawn health, ParticipantFeedbackHudPresenter for combat/score/status messages, UIManager for score/time/game-over flow, or a project-owned presenter that reads the same services.";

            if (route != null && route.UsesScoring())
                return "Selected setup uses scoring/objectives. Create a UI Root with Canvas and EventSystem, then add UIManager for score/time/game-over flow or ParticipantFeedbackHudPresenter for score feedback. Link score UI to ParticipantScoreService or another ISessionScoreService after score changes work in Play Mode.";

            if (route != null && route.UsesTabletopContract())
                return "Selected setup uses Board/Card/Tabletop flow. Create a UI Root with Canvas and EventSystem for turn prompts, action menus, card hands, board selection, or routed interaction panels; connect presenters to the board/action/turn services the scene owns.";

            if (route != null && route.UsesActionSelection())
                return "Selected setup uses action selection. Create a UI Root with Canvas and EventSystem, then add buttons, panels, or cursor/selection presenters that call the chosen action, menu, turn, card, or command runtime. Start with one selectable action before expanding the whole menu.";

            if (route != null && route.UsesPawnGameplay())
                return "Pawn-backed setups usually need visible health, feedback, or menus. Create a UI Root with Canvas and EventSystem, then add ParticipantHealthHudBinder for health, ParticipantFeedbackHudPresenter for combat/status/score messages, UIManager for game-over flow, or project-owned presenters as needed.";

            return "HUD or menu surfaces are optional for this route. Add a Canvas and EventSystem when the game needs visible state, buttons, prompts, settings, or action selection.";
        }

        private static string GetProjectileLauncherMessage(bool setupReady, bool required, bool ready)
        {
            if (!setupReady)
                return "Choose setup capabilities before deciding projectile launcher wiring.";

            if (!required)
                return ready ? "Projectile launcher is present." : "Projectile launcher is optional for this setup route.";

            return ready
                ? "Scene has a ProjectileLauncherBase implementation for projectile/hitscan runtime."
                : "Projectile combat is selected, but the first movement proof can run before combat wiring. Add ProjectileLauncher2D or ProjectileLauncher3D before treating the full projectile route as wired.";
        }

        private static string GetTabletopContractMessage(bool setupReady, bool usesTabletopContract, bool hasTabletopContract)
        {
            if (!setupReady)
                return "Choose setup capabilities before deciding tabletop runtime contract wiring.";

            if (!usesTabletopContract)
                return "Tabletop runtime contract is optional for this setup route.";

            return hasTabletopContract
                ? "Tabletop route has authored board and turn data. Use the selection surface row to make one visible Play Mode proof."
                : "Create and assign BoardDefinition plus TurnOrderDefinition before calling the no-pawn tabletop route ready. BoardMovePolicyDefinition and BoardPieceDefinition assets make the first proof selectable and readable.";
        }

        private static string GetTabletopSelectionSurfaceMessage(bool setupReady, bool usesTabletopContract, bool hasTabletopSelectionSurface)
        {
            if (!setupReady)
                return "Choose setup capabilities before deciding tabletop selection wiring.";

            if (!usesTabletopContract)
                return "Tabletop selection/input surfaces are optional for this setup route.";

            return hasTabletopSelectionSurface
                ? "Scene has a tabletop selection surface. Enter Play Mode and prove one generic board, card, cursor, or menu selection changes board, turn, score, or UI state."
                : "Add TabletopBoardGridPresenter for a generic board proof, or connect TabletopBoardSelectionBridge to a project-owned selection/input bridge, card-hand presenter, cursor, or menu action surface.";
        }

        private static bool HasAnyParticipantInputProfile(SessionDefinition session)
        {
            if (session == null)
                return false;

            if (session.defaultInputProfile != null)
                return true;

            if (session.defaultParticipants == null)
                return false;

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                if (participant == null)
                    continue;

                if (participant.inputProfile != null)
                    return true;

                if (participant.defaultPawn != null && participant.defaultPawn.defaultInputProfile != null)
                    return true;
            }

            return false;
        }

        private static T GetObjectReference<T>(SerializedObject serializedObject, string propertyName) where T : Object
        {
            return serializedObject.FindProperty(propertyName)?.objectReferenceValue as T;
        }

        private static bool GetBool(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            return property != null && property.boolValue;
        }

        private static int GetArraySize(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            return property != null && property.isArray ? property.arraySize : 0;
        }

        private static bool HasTabletopRuntimeContract(GameModeDefinition mode, TabletopBoardGridPresenter presenter, out Object reference)
        {
            reference = null;
            if (mode != null && mode.boardDefinition != null && mode.turnOrderDefinition != null)
            {
                reference = mode;
                return true;
            }

            if (presenter == null)
                return false;

            SerializedObject serializedPresenter = new SerializedObject(presenter);
            bool hasBoard = GetObjectReference<Object>(serializedPresenter, "boardDefinition") != null;
            bool hasTurnOrder = GetObjectReference<Object>(serializedPresenter, "turnOrderDefinition") != null;
            if (!hasBoard || !hasTurnOrder)
                return false;

            reference = presenter;
            return true;
        }

        private static bool HasSceneService<T>(GameplaySessionBootstrap bootstrap, out MonoBehaviour service) where T : class
        {
            service = null;
            if (bootstrap == null)
                return false;

            UnityEngine.SceneManagement.Scene scene = bootstrap.gameObject.scene;
            MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null && behaviour.gameObject.scene == scene && behaviour is T)
                {
                    service = behaviour;
                    return true;
                }
            }

            return false;
        }

        private static bool HasSceneComponent<T>(GameplaySessionBootstrap bootstrap, out T component) where T : Component
        {
            component = null;
            if (bootstrap == null)
                return false;

            UnityEngine.SceneManagement.Scene scene = bootstrap.gameObject.scene;
            T[] components = Object.FindObjectsByType<T>(FindObjectsInactive.Include);
            for (int i = 0; i < components.Length; i++)
            {
                T candidate = components[i];
                if (candidate != null && candidate.gameObject.scene == scene)
                {
                    component = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
