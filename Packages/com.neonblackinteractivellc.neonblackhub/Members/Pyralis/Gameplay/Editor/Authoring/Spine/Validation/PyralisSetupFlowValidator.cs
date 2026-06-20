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
        private const string NetworkedSessionStateServiceFullName = "NeonBlack.Gameplay.Networking.Participants.NetworkedSessionStateService";
        private const string NetworkedParticipantRosterServiceFullName = "NeonBlack.Gameplay.Networking.Participants.NetworkedParticipantRosterService";
        private const string NetworkedParticipantSpawnServiceFullName = "NeonBlack.Gameplay.Networking.Participants.NetworkedParticipantSpawnService";

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
            ParticipantSpawnService participantSpawnService = GetParticipantSpawnService(bootstrap, serializedBootstrap);
            int spawnPointCount = CountSpawnPoints(participantSpawnService);
            CinemachineCameraRigController cameraRig = GetObjectReference<CinemachineCameraRigController>(serializedBootstrap, "cameraRigController");
            bool hasCameraRig = cameraRig != null;
            PlayerInputManager playerInputManager = GetObjectReference<PlayerInputManager>(serializedBootstrap, "playerInputManager");
            bool hasPlayerInputManager = playerInputManager != null;
            bool hasLifetimeScope = bootstrap.GetComponent<PyralisGameplayLifetimeScope>() != null;

            PyralisSetupRouteAnalysis route = PyralisSetupRouteAnalysis.Build(bootstrap);
            bool usesNetworkedCoreServices = session != null && session.networkMode != GameplayNetworkMode.LocalOnly;
            List<string> coreServiceIssues = BuildCoreServiceIssues(bootstrap, serializedBootstrap, usesNetworkedCoreServices);
            GameModeDefinition mode = route.Mode;
            bool hasSelectedCapabilities = route.HasSelectedCapabilities;
            bool requiresPawn = route.RequiresPawn;
            bool hasParticipants = route.HasParticipants;
            int assignedParticipantCount = CountAssignedParticipants(session);
            bool localMultiplayerRoute = route.LikelyUsesInputManager();
            string playerInputManagerIssue = localMultiplayerRoute
                ? route.PlayerInputManagerIssue
                : GetPlayerInputManagerIssue(playerInputManager, localMultiplayerRoute);
            bool hasUsablePlayerInputManager = string.IsNullOrWhiteSpace(playerInputManagerIssue);
            bool hasParticipantPawn = route.HasAnyDefaultPawn;
            string participantPawnIssue = route.ParticipantPawnIssue;
            PawnDefinition firstPawn = GetFirstPawnDefinition(session);
            bool hasParticipantInputProfile = HasAnyParticipantInputProfile(session);
            string participantInputProfileIssue = GetParticipantInputIssue(route);
            bool hasUsableParticipantInputProfile = hasParticipantInputProfile && string.IsNullOrWhiteSpace(participantInputProfileIssue);
            bool setupRouteReady = hasSelectedCapabilities;
            bool needsCameraRigForFirstProof = setupRouteReady && route.UsesPawnGameplay();
            bool needs2DCameraBounds = setupRouteReady && route.Requires2DCameraBounds();
            bool has2DCameraBounds = !needs2DCameraBounds || HasUsable2DCameraBounds(cameraRig, mode);
            string cameraTopologyIssue = GetCameraTopologyIssue(cameraRig, route, assignedParticipantCount);
            bool hasUsableCameraRig = hasCameraRig && string.IsNullOrWhiteSpace(cameraTopologyIssue);
            PyralisAuthoringSceneEvidence sceneEvidence = PyralisAuthoringSceneEvidence.Build(bootstrap);
            bool hasGameplayStateService = sceneEvidence.HasGameplayStateService;
            MonoBehaviour gameplayStateService = sceneEvidence.GameplayStateService as MonoBehaviour;
            bool hasCameraBoundsService = sceneEvidence.HasCameraBoundsService;
            MonoBehaviour cameraBoundsService = sceneEvidence.CameraBoundsService as MonoBehaviour;
            bool hasScoreService = sceneEvidence.HasScoreService;
            MonoBehaviour scoreService = sceneEvidence.ScoreService as MonoBehaviour;
            bool hasSettingsManager = sceneEvidence.HasSettingsManager;
            SettingsManager settingsManager = sceneEvidence.SettingsManager;
            bool hasProjectileLauncher = sceneEvidence.HasProjectileLauncher;
            ProjectileLauncherBase projectileLauncher = sceneEvidence.ProjectileLauncher;
            bool hasTabletopGridPresenter = sceneEvidence.HasTabletopGridPresenter;
            TabletopBoardGridPresenter tabletopGridPresenter = sceneEvidence.TabletopGridPresenter;
            bool hasTabletopSelectionBridge = sceneEvidence.HasTabletopSelectionBridge;
            TabletopBoardSelectionBridge tabletopSelectionBridge = sceneEvidence.TabletopSelectionBridge;
            bool hasTabletopContract = HasTabletopRuntimeContract(mode, tabletopGridPresenter, out Object tabletopContractReference);
            bool hasTabletopSelectionSurface = hasTabletopGridPresenter || hasTabletopSelectionBridge;
            Object tabletopSelectionReference = tabletopGridPresenter != null
                ? tabletopGridPresenter
                : tabletopSelectionBridge != null
                    ? tabletopSelectionBridge
                    : (Object)mode;
            bool hasCanvas = sceneEvidence.HasCanvas;
            Canvas canvas = sceneEvidence.Canvas;
            UIManager uiManager = sceneEvidence.UiManager;
            ParticipantFeedbackHudPresenter feedbackHud = sceneEvidence.FeedbackHud;
            ParticipantHealthHudBinder healthHud = sceneEvidence.HealthHud;
            bool hasHudSurface = sceneEvidence.HasHudSurface;
            Object hudReference = uiManager != null
                ? uiManager
                : feedbackHud != null
                    ? feedbackHud
                    : healthHud != null
                        ? healthHud
                        : canvas != null
                            ? canvas
                            : (Object)bootstrap;
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
                    : "Add PyralisGameplayLifetimeScope to the Gameplay Root so the supported composition root is visible before Play Mode.",
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
                GetRuntimeServiceOwnershipStatus(session, coreServiceIssues),
                GetRuntimeServiceOwnershipMessage(session, coreServiceIssues, usesNetworkedCoreServices),
                bootstrap,
                PyralisSetupFlowActionKind.SelectObject,
                stepId: PyralisSetupFlowStepId.RuntimeServiceOwnership,
                nativeAction: GetRuntimeServiceOwnershipAction(usesNetworkedCoreServices)));

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
                "Resolve Route Capabilities",
                GetDependentStatus(mode != null, hasSelectedCapabilities),
                mode == null
                    ? "Assign Default Game Mode first."
                    : hasSelectedCapabilities ? "The graph reflected route capabilities from authored gameplay setup, feature modules, participants, mode flags, or contracts." : "Open Intent to filter the guide, then create or wire gameplay assets so contracts and serialized references expose route capabilities.",
                mode,
                stepId: PyralisSetupFlowStepId.ResolveRouteCapabilities));

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
                "Resolve Participant Join Policy",
                GetParticipantJoinPolicyStatus(setupRouteReady, route),
                GetParticipantJoinPolicyMessage(setupRouteReady, route),
                bootstrap.GetComponentInChildren<ParticipantInputRouter>(true) != null
                    ? (Object)bootstrap.GetComponentInChildren<ParticipantInputRouter>(true)
                    : bootstrap,
                stepId: PyralisSetupFlowStepId.ResolveParticipantJoinPolicy));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Spawn Points",
                GetSpawnPointStatus(setupRouteReady, requiresPawn, spawnPointCount, assignedParticipantCount),
                GetSpawnPointMessage(setupRouteReady, requiresPawn, spawnPointCount, assignedParticipantCount),
                bootstrap,
                stepId: PyralisSetupFlowStepId.AssignSpawnPoints));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Camera Rig",
                GetCameraRigStatus(setupRouteReady, needsCameraRigForFirstProof, route.UsesCamera(), hasUsableCameraRig, has2DCameraBounds),
                GetCameraRigMessage(setupRouteReady, needsCameraRigForFirstProof, needs2DCameraBounds, route.UsesCamera(), hasCameraRig, has2DCameraBounds, cameraTopologyIssue),
                cameraRig,
                stepId: PyralisSetupFlowStepId.AssignCameraRig));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Player Input Manager",
                GetPlayerInputManagerStatus(setupRouteReady, localMultiplayerRoute, hasPlayerInputManager, hasUsablePlayerInputManager),
                GetPlayerInputMessage(setupRouteReady, localMultiplayerRoute, hasPlayerInputManager, hasUsablePlayerInputManager, playerInputManagerIssue, assignedParticipantCount),
                playerInputManager,
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
                tabletopContractReference != null ? tabletopContractReference : mode,
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

        private static PyralisSetupFlowStepStatus GetRuntimeServiceOwnershipStatus(
            SessionDefinition session,
            IReadOnlyList<string> coreServiceIssues)
        {
            if (session == null)
                return PyralisSetupFlowStepStatus.Ready;

            return coreServiceIssues == null || coreServiceIssues.Count == 0
                ? PyralisSetupFlowStepStatus.Ready
                : PyralisSetupFlowStepStatus.Missing;
        }

        private static string GetRuntimeServiceOwnershipMessage(
            SessionDefinition session,
            IReadOnlyList<string> coreServiceIssues,
            bool usesNetworkedCoreServices)
        {
            if (session == null)
            {
                return "GameplaySessionBootstrap and PyralisGameplayLifetimeScope are the runtime composition path. Assign SessionDefinition before checking authored core services.";
            }

            if (coreServiceIssues == null || coreServiceIssues.Count == 0)
            {
                return usesNetworkedCoreServices
                    ? "Networked core services are authored and ready for PyralisGameplayLifetimeScope registration."
                    : "Core runtime services are authored and ready for PyralisGameplayLifetimeScope registration.";
            }

            return "Runtime no longer creates hidden core service GameObjects. Add or assign these authored services under the Gameplay Root before Play Mode: "
                + string.Join("; ", coreServiceIssues);
        }

        private static PyralisAuthoringNativeAction GetRuntimeServiceOwnershipAction(bool usesNetworkedCoreServices)
        {
            string serviceList = usesNetworkedCoreServices
                ? "NetworkedSessionStateService, NetworkedParticipantRosterService, NetworkedParticipantSpawnService, and ParticipantInputRouter"
                : "SessionStateService, ParticipantRosterService, ParticipantSpawnService, and ParticipantInputRouter";

            return new PyralisAuthoringNativeAction(
                "Add or assign",
                PyralisAuthoringActionSurface.Inspector,
                "Gameplay Root",
                serviceList + " as child components or Bootstrap override fields",
                "Runtime Service Ownership is ready and no core service is runtime-created");
        }

        private static List<string> BuildCoreServiceIssues(
            GameplaySessionBootstrap bootstrap,
            SerializedObject serializedBootstrap,
            bool usesNetworkedCoreServices)
        {
            List<string> issues = new List<string>();
            AppendCoreServiceIssue<SessionStateService>(
                bootstrap,
                serializedBootstrap,
                "sessionStateService",
                "SessionStateService",
                usesNetworkedCoreServices ? NetworkedSessionStateServiceFullName : string.Empty,
                issues);
            AppendCoreServiceIssue<ParticipantRosterService>(
                bootstrap,
                serializedBootstrap,
                "participantRosterService",
                "ParticipantRosterService",
                usesNetworkedCoreServices ? NetworkedParticipantRosterServiceFullName : string.Empty,
                issues);
            AppendCoreServiceIssue<ParticipantSpawnService>(
                bootstrap,
                serializedBootstrap,
                "participantSpawnService",
                "ParticipantSpawnService",
                usesNetworkedCoreServices ? NetworkedParticipantSpawnServiceFullName : string.Empty,
                issues);
            AppendCoreServiceIssue<ParticipantInputRouter>(
                bootstrap,
                serializedBootstrap,
                "participantInputRouter",
                "ParticipantInputRouter",
                string.Empty,
                issues);
            return issues;
        }

        private static void AppendCoreServiceIssue<T>(
            GameplaySessionBootstrap bootstrap,
            SerializedObject serializedBootstrap,
            string propertyName,
            string serviceName,
            string preferredFullTypeName,
            List<string> issues) where T : Component
        {
            T service = GetObjectReference<T>(serializedBootstrap, propertyName);
            service ??= bootstrap != null ? bootstrap.GetComponentInChildren<T>(true) : null;
            if (service == null)
            {
                issues.Add(serviceName);
                return;
            }

            if (!string.IsNullOrWhiteSpace(preferredFullTypeName)
                && !string.Equals(service.GetType().FullName, preferredFullTypeName, StringComparison.Ordinal))
            {
                issues.Add($"{serviceName} should use {GetTypeDisplayName(preferredFullTypeName)} for this networked route, but found {service.GetType().Name}");
            }
        }

        private static string GetTypeDisplayName(string fullTypeName)
        {
            if (string.IsNullOrWhiteSpace(fullTypeName))
                return string.Empty;

            int lastDot = fullTypeName.LastIndexOf('.');
            return lastDot >= 0 && lastDot < fullTypeName.Length - 1
                ? fullTypeName.Substring(lastDot + 1)
                : fullTypeName;
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

        private static PyralisSetupFlowStepStatus GetSpawnPointStatus(
            bool setupReady,
            bool requiresPawn,
            int spawnPointCount,
            int assignedParticipantCount)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            if (!requiresPawn)
                return spawnPointCount > 0 ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Optional;

            if (spawnPointCount <= 0)
                return PyralisSetupFlowStepStatus.Missing;

            int requiredSpawnPoints = Math.Max(1, assignedParticipantCount);
            return spawnPointCount >= requiredSpawnPoints
                ? PyralisSetupFlowStepStatus.Ready
                : PyralisSetupFlowStepStatus.Missing;
        }

        private static PyralisSetupFlowStepStatus GetPlayerInputManagerStatus(
            bool setupReady,
            bool recommended,
            bool hasPlayerInputManager,
            bool hasUsablePlayerInputManager)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            if (hasPlayerInputManager && !hasUsablePlayerInputManager)
                return PyralisSetupFlowStepStatus.Missing;

            if (!recommended)
                return hasPlayerInputManager && hasUsablePlayerInputManager
                    ? PyralisSetupFlowStepStatus.Ready
                    : PyralisSetupFlowStepStatus.Optional;

            return hasPlayerInputManager && hasUsablePlayerInputManager
                ? PyralisSetupFlowStepStatus.Ready
                : PyralisSetupFlowStepStatus.Missing;
        }

        private static PyralisSetupFlowStepStatus GetParticipantJoinPolicyStatus(
            bool setupReady,
            PyralisSetupRouteAnalysis route)
        {
            if (!setupReady || route == null)
                return PyralisSetupFlowStepStatus.Blocked;

            if (!route.HasParticipants)
                return PyralisSetupFlowStepStatus.Blocked;

            if (route.HasLocalJoinPolicyConflict())
                return PyralisSetupFlowStepStatus.Missing;

            return PyralisSetupFlowStepStatus.Ready;
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

        private static PyralisSetupFlowStepStatus GetCameraRigStatus(bool setupReady, bool proofRequiresCameraRig, bool recommended, bool ready, bool usable2DBounds)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            if (proofRequiresCameraRig)
                return ready ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Missing;

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
                return "Choose a valid capability intent before checking scene and prefab readiness.";

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
                    ? "Assign InputProfile on the controlling `ParticipantDefinition.inputProfile` before routing movement."
                    : "Input profile is optional for this route unless a built-in player/input surface is used.";
            }

            if (!string.IsNullOrWhiteSpace(inputProfileIssue))
                return inputProfileIssue;

            return "A participant InputProfile is assigned. Pawn/input readers can now bind control signals.";
        }

        private static string GetParticipantJoinPolicyMessage(bool setupReady, PyralisSetupRouteAnalysis route)
        {
            if (!setupReady || route == null)
                return "Choose setup capabilities before deciding participant join policy.";

            if (!route.HasParticipants)
                return "Assign default participants before deciding whether they auto-start, join locally, or wait for network/manual authority.";

            if (route.HasLocalJoinPolicyConflict())
            {
                return $"This route looks like local join: {route.AssignedParticipantCount} local pawn participants are assigned, but ParticipantInputRouter is set to auto-register {route.AutoJoinParticipantCount} default participant(s) without PlayerInput. Disable auto-register defaults for local co-op so Unity PlayerInputManager joins create each participant/controller pair.";
            }

            switch (route.ParticipantTopology)
            {
                case PyralisParticipantTopology.LocalJoin:
                    return route.HasPlayerInputManager
                        ? $"Local join topology is selected for {route.AssignedParticipantCount} pawn participants. PlayerInputManager owns controller pairing; ParticipantSpawnService can spawn when each joined participant registers."
                        : $"Local join topology is selected for {route.AssignedParticipantCount} pawn participants. Add Unity PlayerInputManager so each controller joins one participant instead of all participants auto-starting.";
                case PyralisParticipantTopology.SoloLocal:
                    return route.AutoRegisterDefaultsWithoutPlayerInput
                        ? "Solo local topology can auto-register the default participant without PlayerInputManager. Use PlayerInputManager only if this proof should wait for a join button/device."
                        : "Solo local topology is configured to wait for PlayerInput join or custom registration instead of auto-starting.";
                case PyralisParticipantTopology.Networked:
                    return "Networked topology should let the networking authority path register participants. Local PlayerInputManager is not the transport owner.";
                case PyralisParticipantTopology.HybridLocalNetworked:
                    return "Hybrid topology has multiple participants on a networked session. Keep local device pairing separate from network authority and validate both paths before Play Mode.";
                case PyralisParticipantTopology.NoParticipants:
                    return "No participants are assigned yet.";
                default:
                    return "Participant topology could not be inferred yet.";
            }
        }

        private static Object GetInputProfileReference(SessionDefinition session)
        {
            if (session == null)
                return null;

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

        private static int CountAssignedParticipants(SessionDefinition session)
        {
            if (session == null || session.defaultParticipants == null)
                return 0;

            int count = 0;
            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                if (session.defaultParticipants[i] != null)
                    count++;
            }

            return count;
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

        private static string GetParticipantInputIssue(PyralisSetupRouteAnalysis route)
        {
            if (route != null && route.ParticipantSeats != null && route.ParticipantSeats.Length > 0)
            {
                for (int i = 0; i < route.ParticipantSeats.Length; i++)
                {
                    PyralisParticipantSeatReadiness seat = route.ParticipantSeats[i];
                    if (seat == null || !seat.RequiresPawn)
                        continue;

                    if (!string.IsNullOrWhiteSpace(seat.InputIssue))
                        return seat.InputIssue;

                    if (seat.InputProfile == null)
                        return $"Participant slot {seat.SlotIndex} needs ParticipantDefinition.inputProfile before trying movement in Play Mode.";

                    string bindingIssue = GetInputProfileBindingIssue(seat.InputProfile);
                    if (!string.IsNullOrWhiteSpace(bindingIssue))
                        return $"Participant `{seat.DisplayName}` effective InputProfile `{seat.InputProfile.name}`: {bindingIssue}";
                }

                return string.Empty;
            }

            SessionDefinition session = route?.Session;
            if (session == null || session.defaultParticipants == null || session.defaultParticipants.Length == 0)
                return "Assign default participants first.";

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                if (participant == null)
                    continue;

                InputProfile effectiveProfile = ParticipantInputProfileUtility.ResolveEffectiveInputProfile(participant);

                if (effectiveProfile == null)
                    return "Add InputProfile to the controlling ParticipantDefinition before trying movement in Play Mode.";

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

        private static string GetSpawnPointMessage(
            bool setupReady,
            bool requiresPawn,
            int spawnPointCount,
            int assignedParticipantCount)
        {
            if (!setupReady)
                return "Choose setup capabilities before deciding whether spawn points are required.";

            if (!requiresPawn)
                return spawnPointCount > 0
                    ? "Spawn points are assigned, which is allowed when this setup spawns actor bodies."
                    : "Spawn points can stay empty for no-pawn board/card/menu/camera routes.";

            if (spawnPointCount <= 0)
                return "Selected setup requires pawns. Add spawn point transforms to ParticipantSpawnService.";

            int requiredSpawnPoints = Math.Max(1, assignedParticipantCount);
            if (spawnPointCount < requiredSpawnPoints)
                return $"Selected setup has {spawnPointCount} assigned spawn point(s) for {requiredSpawnPoints} default participant(s). Add one spawn point per starting participant, or set the session to a clean 1P proof before Play Mode.";

            return "Spawn points are assigned for pawn-backed participants.";
        }

        private static string GetCameraRigMessage(
            bool setupReady,
            bool proofRequiresCameraRig,
            bool requires2DBounds,
            bool recommended,
            bool ready,
            bool usable2DBounds,
            string cameraTopologyIssue)
        {
            if (!setupReady)
                return "Choose setup capabilities before deciding camera rig wiring.";

            if (proofRequiresCameraRig)
            {
                if (!ready)
                    return "Pawn movement needs a scene camera route before the first Play Mode proof. Keep or create one physical Unity Camera, usually the default Main Camera; do not delete it for the normal Cinemachine route. Create Camera Root, add CinemachineCameraRigController, create or choose a separate Cinemachine Camera for Shared Camera Behaviour, verify the physical Main Camera is tagged MainCamera with Cinemachine Brain, assign that physical camera as Target Camera, then drag Camera Root from Hierarchy into Bootstrap > Camera Rig Controller.";

                if (!string.IsNullOrWhiteSpace(cameraTopologyIssue))
                    return cameraTopologyIssue;

                if (!requires2DBounds)
                    return "Camera rig is assigned for the pawn movement proof. Cinemachine follows the camera focus mode selected by CameraRigProfile; add PawnCameraTarget to the pawn prefab when the follow/look-at socket should be explicit.";

                return usable2DBounds
                    ? "Camera rig is assigned with an orthographic 2D framing path. Cinemachine follow still comes from CameraRigProfile focus mode and the resolved pawn/playfield target."
                    : "Camera rig is assigned. Before judging 2D camera feel, select Camera Root and assign an orthographic CameraRigProfile, or select the physical Target Camera and set Camera > Projection to Orthographic. If using a profile, also assign it to GameModeDefinition > Camera Rig Profile.";
            }

            if (!recommended)
                return ready
                    ? "Camera rig is assigned."
                    : "Camera rig is optional for this setup route. Add it later if the player controls a view, cursor, selector, board camera, or follow camera.";

            if (!string.IsNullOrWhiteSpace(cameraTopologyIssue))
                return cameraTopologyIssue;

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
                ? "Before judging Play Mode, select Camera Root and CameraRigProfile. Choose Focus Mode first: Participant Group for a shared pawn camera, Participant Pawns for per-participant cameras, Playfield Center for board/menu/playfield views, Explicit Scene Target for a scene anchor, or Manual Cinemachine when you want to wire Follow/LookAt directly. Then tune physical Target Camera assignment, MainCamera tag/Brain, orthographic size, 2D Bounds Framing minimum visible area, Follow Damping, Follow Offset, View Euler Angles, and player room around the target. Add PawnCameraTarget to the pawn prefab when the camera should follow a visible socket instead of the pawn root fallback."
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

            return "Before judging Play Mode, inspect the PawnMovementProfile, effective InputProfile, and installed FeatureModuleDefinition assets. Use PawnMovementProfile > Movement Style = TopDownNoGravity for free X/Y movement, add a top-down hop feature when Jump should lift the visual while staying map-plane grounded, or set Movement Style = SideViewGravity when Jump should drive Rigidbody2D vertical motion. The InputProfile maps Unity Input Actions into semantic roles; the pawn prefab still needs an input module such as Motor2DInputAdapter to dispatch those roles.";
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

        private static string GetCameraTopologyIssue(
            CinemachineCameraRigController rig,
            PyralisSetupRouteAnalysis route,
            int assignedParticipantCount)
        {
            if (rig == null || route == null || route.Mode == null || route.Mode.cameraRigProfile == null)
                return string.Empty;

            CameraRigProfile profile = route.Mode.cameraRigProfile;
            SerializedObject serializedRig = new SerializedObject(rig);
            Camera targetCamera = serializedRig.FindProperty("targetCamera")?.objectReferenceValue as Camera;
            MonoBehaviour sharedCameraBehaviour = serializedRig.FindProperty("sharedCameraBehaviour")?.objectReferenceValue as MonoBehaviour;
            Transform explicitFocusTarget = serializedRig.FindProperty("explicitFocusTarget")?.objectReferenceValue as Transform;
            int splitScreenCameraCount = CountObjectReferences(serializedRig.FindProperty("splitScreenCameraBehaviours"));

            if (targetCamera == null)
                return "CameraRigController needs Target Camera assigned to the physical Unity Camera that has Cinemachine Brain.";

            if (profile.focusMode == CameraRigProfile.CameraFocusMode.ManualCinemachine)
                return string.Empty;

            if (profile.focusMode == CameraRigProfile.CameraFocusMode.ExplicitSceneTarget && explicitFocusTarget == null)
                return "CameraRigProfile uses Explicit Scene Target. Assign CinemachineCameraRigController.explicitFocusTarget to the scene anchor, menu, board, or cursor target the camera should frame.";

            if (profile.presentationMode == CameraRigProfile.CameraPresentationMode.SplitScreen
                || profile.focusMode == CameraRigProfile.CameraFocusMode.ParticipantPawns
                    && route.ParticipantTopology == PyralisParticipantTopology.LocalJoin
                    && assignedParticipantCount > 1)
            {
                int requiredCameras = Math.Max(1, assignedParticipantCount);
                if (splitScreenCameraCount < requiredCameras)
                {
                    return $"CameraRigProfile is set up for participant pawn split/per-player focus, but CameraRigController has {splitScreenCameraCount} split-screen camera behaviour(s) for {requiredCameras} participant(s). Assign one Cinemachine camera per local participant, or switch CameraRigProfile to Shared + Participant Group for one shared camera.";
                }
            }

            if (profile.focusMode == CameraRigProfile.CameraFocusMode.ParticipantGroup
                || profile.focusMode == CameraRigProfile.CameraFocusMode.ParticipantPawns
                || profile.focusMode == CameraRigProfile.CameraFocusMode.PlayfieldCenter)
            {
                if (sharedCameraBehaviour == null
                    && profile.presentationMode == CameraRigProfile.CameraPresentationMode.Shared)
                {
                    return "CameraRigController needs Shared Camera Behaviour assigned to the Cinemachine Camera used by the shared route.";
                }
            }

            return string.Empty;
        }

        private static int CountObjectReferences(SerializedProperty property)
        {
            if (property == null || !property.isArray)
                return 0;

            int count = 0;
            for (int i = 0; i < property.arraySize; i++)
            {
                if (property.GetArrayElementAtIndex(i).objectReferenceValue != null)
                    count++;
            }

            return count;
        }

        private static string GetPlayerInputMessage(
            bool setupReady,
            bool recommended,
            bool hasPlayerInputManager,
            bool hasUsablePlayerInputManager,
            string playerInputManagerIssue,
            int assignedParticipantCount)
        {
            if (!setupReady)
                return "Choose setup capabilities before deciding local join wiring.";

            if (!string.IsNullOrWhiteSpace(playerInputManagerIssue))
                return playerInputManagerIssue;

            if (!recommended)
                return hasPlayerInputManager
                    ? "PlayerInputManager is assigned."
                    : "PlayerInputManager is optional for single-player, AI-only, menu-only, and no-join prototypes. Add it when the session has multiple local player participants.";

            return hasPlayerInputManager
                ? $"PlayerInputManager is assigned for {assignedParticipantCount} local participants, and ParticipantInputRouter will subscribe to join/leave events."
                : $"Selected setup has {assignedParticipantCount} local pawn participants. For local co-op, create an Input Root, add Unity PlayerInputManager, assign a player prefab that contains PlayerInput and PawnRoot, configure Join Behavior/Input Actions, then drag the component into Bootstrap > Player Input Manager.";
        }

        private static string GetPlayerInputManagerIssue(PlayerInputManager playerInputManager, bool localMultiplayerRoute)
        {
            if (playerInputManager == null)
                return localMultiplayerRoute
                    ? "Selected setup has multiple local pawn participants. Add Unity PlayerInputManager and assign a player prefab with PlayerInput and PawnRoot so each controller owns one pawn."
                    : string.Empty;

            if (playerInputManager.playerPrefab == null)
                return "Configure PlayerInputManager > Player Prefab before Play Mode. Unity PlayerInputManager logs runtime errors when join is enabled without a Player Prefab.";

            GameObject playerPrefab = playerInputManager.playerPrefab;
            if (playerPrefab.GetComponent<PlayerInput>() == null)
                return "PlayerInputManager.playerPrefab must contain a PlayerInput component so Unity can pair each controller with one participant.";

            if (!PrefabContainsPawnInitializer(playerPrefab))
                return "PlayerInputManager.playerPrefab should be the pawn prefab, or contain PawnRoot/IPawnParticipantInitializer, so the joined PlayerInput controls that participant's pawn instead of a shared action asset.";

            return string.Empty;
        }

        private static bool PrefabContainsPawnInitializer(GameObject prefab)
        {
            if (prefab == null)
                return false;

            MonoBehaviour[] behaviours = prefab.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IPawnParticipantInitializer)
                    return true;
            }

            return false;
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
                : "Author SessionStateService under the Gameplay Root or assign a custom IGameplayStateReader only when this intent deliberately owns gameplay state differently.";

        }

        private static string GetCameraBoundsServiceMessage(bool setupReady, bool recommended, bool ready)
        {
            if (!setupReady)
                return "Choose setup capabilities before deciding camera bounds service wiring.";

            if (!recommended)
                return ready ? "Camera bounds provider is present." : "Camera bounds provider is optional until the selected intent uses framing, camera-aware spawning, hazards, pickups, or bounded playfield behavior.";

            return ready
                ? "Scene has an ICameraBoundsProvider for camera-aware runtime systems. This does not by itself prove camera follow; follow comes from CameraRigProfile focus mode and the resolved target."
                : "Selected intent includes camera-aware bounds behavior. Assign CinemachineCameraRigController to GameplaySessionBootstrap > Camera Rig Controller when spawners, pickups, hazards, or screen-edge movement need visible camera bounds. Pawn camera follow is handled separately by CameraRigProfile focus mode.";
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
                    || route.UsesActionSelection());

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

            if (session.defaultParticipants == null)
                return false;

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                if (participant == null)
                    continue;

                if (participant.inputProfile != null)
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

        private static ParticipantSpawnService GetParticipantSpawnService(GameplaySessionBootstrap bootstrap, SerializedObject serializedBootstrap)
        {
            ParticipantSpawnService service = GetObjectReference<ParticipantSpawnService>(serializedBootstrap, "participantSpawnService");
            if (service != null || bootstrap == null)
                return service;

            return bootstrap.GetComponentInChildren<ParticipantSpawnService>(true);
        }

        private static int CountSpawnPoints(ParticipantSpawnService service)
        {
            if (service == null)
                return 0;

            SerializedObject serializedService = new SerializedObject(service);
            SerializedProperty spawnPoints = serializedService.FindProperty("spawnPoints");
            if (spawnPoints == null || !spawnPoints.isArray)
                return 0;

            int count = 0;
            for (int i = 0; i < spawnPoints.arraySize; i++)
            {
                if (spawnPoints.GetArrayElementAtIndex(i).objectReferenceValue != null)
                    count++;
            }

            return count;
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

    }
}
