using System;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Glue.Bootstrap;
using NeonBlack.Gameplay.Glue.InputRouting;
using NeonBlack.Gameplay.Glue.Lifetime;
using NeonBlack.Gameplay.Glue.Participants;
using NeonBlack.Gameplay.Glue.SceneFlow.Arcade2D;
using NeonBlack.Gameplay.Glue.SceneServices;
using NeonBlack.Gameplay.Glue.ServiceRegistration;
using NeonBlack.Gameplay.Glue.Session;
using NeonBlack.Gameplay.Glue.Spawning;
using NeonBlack.Gameplay.Modules.Combat;
using NeonBlack.Gameplay.Modules.Enemies;
using NeonBlack.Gameplay.Modules.Feedback;
using NeonBlack.Gameplay.Modules.Scoring;
using NeonBlack.Gameplay.Presentation.Camera;
using NeonBlack.Gameplay.Presentation.Visuals;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Glue.Wiring.Reporting
{
    public static class GameplayWiringReportBuilder
    {
        public static GameplayWiringReport BuildFrom(GameplaySessionBootstrap bootstrap)
        {
            return Build(
                bootstrap != null ? bootstrap.gameObject : null,
                bootstrap != null ? bootstrap.SessionDefinition : null);
        }

        public static GameplayWiringReport Build(GameObject gameplayRoot, SessionDefinition sessionDefinition = null)
        {
            GameplayWiringReport report = new GameplayWiringReport();
            GameplaySessionBootstrap bootstrap = FindComponent<GameplaySessionBootstrap>(gameplayRoot);
            if (sessionDefinition == null && bootstrap != null)
                sessionDefinition = bootstrap.SessionDefinition;

            AddRootEvidence(report, gameplayRoot);
            AddSessionEvidence(report, sessionDefinition);
            AddCoreServiceEvidence(report, gameplayRoot, sessionDefinition);
            AddRuntimeSceneSearchInventoryEvidence(report);
            AddFeatureActivationEvidence(report, sessionDefinition);
            AddRuntimeServiceReceiverEvidence(report, gameplayRoot, sessionDefinition);
            AddRuntimeValidationEvidence(report, gameplayRoot, sessionDefinition);

            return report;
        }

        private static void AddRootEvidence(GameplayWiringReport report, GameObject gameplayRoot)
        {
            if (gameplayRoot == null)
            {
                report.Add(new GameplayWiringRow(
                    GameplayWiringRowKind.MissingProvider,
                    "GameplayRoot",
                    string.Empty,
                    "Runtime Wiring",
                    string.Empty,
                    "Scene",
                    GameplayWiringScope.Scene,
                    GameplayWiringTiming.Authoring,
                    GameplayWiringRequiredness.Required,
                    GameplayWiringSeverity.Error,
                    "No gameplay root was provided to the wiring report."));
                return;
            }

            report.Add(new GameplayWiringRow(
                GameplayWiringRowKind.Provider,
                "GameplayRoot",
                GetPath(gameplayRoot.transform),
                "Runtime Wiring",
                gameplayRoot.name,
                "Scene",
                GameplayWiringScope.Scene,
                GameplayWiringTiming.Authoring,
                GameplayWiringRequiredness.Required,
                GameplayWiringSeverity.Info,
                "Scene root supplied for wiring inventory."));
        }

        private static void AddSessionEvidence(
            GameplayWiringReport report,
            SessionDefinition sessionDefinition)
        {
            if (sessionDefinition == null)
            {
                report.Add(new GameplayWiringRow(
                    GameplayWiringRowKind.MissingProvider,
                    "SessionDefinition",
                    string.Empty,
                    nameof(GameplaySessionBootstrap),
                    "sessionDefinition",
                    "Data",
                    GameplayWiringScope.Session,
                    GameplayWiringTiming.Authoring,
                    GameplayWiringRequiredness.Required,
                    GameplayWiringSeverity.Error,
                    "Session Definition is unassigned. Action: Assign the SessionDefinition asset on GameplaySessionBootstrap. Success: GameplaySessionBootstrap references the session asset before Play Mode."));
                return;
            }

            report.Add(new GameplayWiringRow(
                GameplayWiringRowKind.DataIntake,
                "SessionDefinition",
                sessionDefinition.name,
                nameof(GameplaySessionBootstrap),
                sessionDefinition.GetType().Name,
                "Data",
                GameplayWiringScope.Session,
                GameplayWiringTiming.Authoring,
                GameplayWiringRequiredness.Required,
                GameplayWiringSeverity.Info,
                "Authored session data is available for runtime startup."));
        }

        private static void AddCoreServiceEvidence(
            GameplayWiringReport report,
            GameObject gameplayRoot,
            SessionDefinition sessionDefinition)
        {
            if (gameplayRoot == null)
                return;

            AddRequiredComponent<GameplaySessionBootstrap>(
                report,
                gameplayRoot,
                "GameplaySessionBootstrap",
                "Session startup",
                GameplayWiringScope.Session,
                GameplayWiringTiming.Startup);

            AddRequiredComponent<GameplayLifetimeScope>(
                report,
                gameplayRoot,
                "GameplayLifetimeScope",
                "Runtime service delivery",
                GameplayWiringScope.Session,
                GameplayWiringTiming.Startup);

            if (sessionDefinition == null)
            {
                report.Add(new GameplayWiringRow(
                    GameplayWiringRowKind.TimingIssue,
                    "CoreServiceRoute",
                    nameof(SessionDefinition),
                    "Runtime service delivery",
                    "Session-scoped service requirements",
                    "Runtime Wiring",
                    GameplayWiringScope.Session,
                    GameplayWiringTiming.Authoring,
                    GameplayWiringRequiredness.Required,
                    GameplayWiringSeverity.Warning,
                    "Session-scoped service requirements are deferred until a SessionDefinition is assigned. The report should not infer participant, spawn, input, camera, or feature service requirements before the authored session exists."));
                return;
            }

            AddRequiredComponent<SessionStateService>(
                report,
                gameplayRoot,
                "SessionStateService",
                "Session state",
                GameplayWiringScope.Session,
                GameplayWiringTiming.Startup);

            if (!HasParticipantRouteData(sessionDefinition))
            {
                report.Add(new GameplayWiringRow(
                    GameplayWiringRowKind.TimingIssue,
                    "ParticipantServiceRoute",
                    nameof(SessionDefinition),
                    "Participant runtime services",
                    "Session participant route requirements",
                    "Runtime Wiring",
                    GameplayWiringScope.Participant,
                    GameplayWiringTiming.Authoring,
                    GameplayWiringRequiredness.Required,
                    GameplayWiringSeverity.Warning,
                    "Participant, spawn, input, camera, and feature service requirements are deferred until SessionDefinition has a default GameModeDefinition and at least one default ParticipantDefinition."));
                return;
            }

            AddRequiredComponent<ParticipantRosterService>(
                report,
                gameplayRoot,
                "ParticipantRosterService",
                "Participant roster",
                GameplayWiringScope.Participant,
                GameplayWiringTiming.Startup);

            AddRequiredComponent<ParticipantSpawnService>(
                report,
                gameplayRoot,
                "ParticipantSpawnService",
                "Participant spawn delivery",
                GameplayWiringScope.Participant,
                GameplayWiringTiming.Spawn);

            AddRequiredComponent<ParticipantInputRouter>(
                report,
                gameplayRoot,
                "ParticipantInputRouter",
                "Participant input route",
                GameplayWiringScope.Participant,
                GameplayWiringTiming.Join);

            AddOptionalComponent<PlayerInputManager>(
                report,
                gameplayRoot,
                "PlayerInputManager",
                "Unity local join source",
                GameplayWiringScope.Participant,
                GameplayWiringTiming.Join);

            AddOptionalComponent<CinemachineCameraRigController>(
                report,
                gameplayRoot,
                "CinemachineCameraRigController",
                "Camera focus delivery",
                GameplayWiringScope.Presentation,
                GameplayWiringTiming.Startup);

            AddOptionalComponent<TimeScaleService>(
                report,
                gameplayRoot,
                "TimeScaleService",
                "Gameplay time scale delivery",
                GameplayWiringScope.Session,
                GameplayWiringTiming.Play);

            AddOptionalComponent<CameraShake>(
                report,
                gameplayRoot,
                "CameraShake",
                "Camera feedback delivery",
                GameplayWiringScope.Presentation,
                GameplayWiringTiming.Play);

            AddOptionalInterface<ISceneNavigator>(
                report,
                gameplayRoot,
                "ISceneNavigator",
                "Scene navigation delivery",
                GameplayWiringScope.Scene,
                GameplayWiringTiming.Play);

            AddParticipantRouteEvidence(report, gameplayRoot, sessionDefinition);
        }

        private static void AddRuntimeServiceReceiverEvidence(
            GameplayWiringReport report,
            GameObject gameplayRoot,
            SessionDefinition sessionDefinition)
        {
            if (gameplayRoot == null || sessionDefinition == null)
                return;

            IGameplayRuntimeServicesReceiver[] gameplayReceivers =
                gameplayRoot.GetComponentsInChildren<IGameplayRuntimeServicesReceiver>(true);
            for (int i = 0; i < gameplayReceivers.Length; i++)
            {
                report.Add(new GameplayWiringRow(
                    GameplayWiringRowKind.Delivery,
                    "GameplayRuntimeServicesContext",
                    nameof(GameplayLifetimeScope),
                    GetObjectLabel(gameplayReceivers[i]),
                    nameof(GameplayRuntimeServicesContext),
                    "Runtime Wiring",
                    GameplayWiringScope.Feature,
                    GameplayWiringTiming.Startup,
                    GameplayWiringRequiredness.AutoDerived,
                    GameplayWiringSeverity.Info,
                    "GameplayLifetimeScope applies shared scene/session runtime services to this receiver after container injection."));
            }

            IPawnRuntimeServicesReceiver[] pawnReceivers =
                gameplayRoot.GetComponentsInChildren<IPawnRuntimeServicesReceiver>(true);
            for (int i = 0; i < pawnReceivers.Length; i++)
            {
                report.Add(new GameplayWiringRow(
                    GameplayWiringRowKind.Delivery,
                    "PawnRuntimeServicesContext",
                    nameof(ParticipantSpawnService),
                    GetObjectLabel(pawnReceivers[i]),
                    nameof(PawnRuntimeServicesContext),
                    "ParticipantSpawnService",
                    GameplayWiringScope.Pawn,
                    GameplayWiringTiming.Spawn,
                    GameplayWiringRequiredness.AutoDerived,
                    GameplayWiringSeverity.Info,
                    "ParticipantSpawnService applies pawn runtime services when a participant pawn is spawned or claimed."));
            }
        }

        private static void AddParticipantRouteEvidence(
            GameplayWiringReport report,
            GameObject gameplayRoot,
            SessionDefinition sessionDefinition)
        {
            if (sessionDefinition == null || sessionDefinition.defaultParticipants == null)
                return;

            PlayerInputManager playerInputManager = FindComponent<PlayerInputManager>(gameplayRoot);
            ParticipantJoinRouteDecision joinRoute = ParticipantJoinRoutePolicy.Evaluate(
                sessionDefinition,
                playerInputManager != null);
            if (joinRoute.ShouldDeferAutoRegistration)
            {
                report.Add(new GameplayWiringRow(
                    GameplayWiringRowKind.TimingIssue,
                    "ParticipantJoinRoute",
                    nameof(PlayerInputManager),
                    nameof(ParticipantInputRouter),
                    "AutoJoin participants",
                    "ParticipantInputRouter",
                    GameplayWiringScope.Participant,
                    GameplayWiringTiming.Join,
                    GameplayWiringRequiredness.Required,
                    GameplayWiringSeverity.Warning,
                    joinRoute.WarningMessage));
            }
        }

        private static void AddFeatureActivationEvidence(
            GameplayWiringReport report,
            SessionDefinition sessionDefinition)
        {
            if (!HasParticipantRouteData(sessionDefinition))
                return;

            RuntimeFeatureServicePolicy policy = RuntimeFeatureServicePolicy.ResolveWithLoadedSceneEvidence(sessionDefinition);

            AddServiceActivation(report, "CombatServices", policy.UsesCombatServices);
            AddServiceActivation(report, "EnemyServices", policy.UsesEnemyServices);
            AddServiceActivation(report, "RpgServices", policy.UsesRpgServices);
            AddServiceActivation(report, "GameFlowServices", policy.UsesGameFlowServices);
            AddServiceActivation(report, "ScoringServices", policy.UsesScoringServices);
            AddServiceActivation(report, "FeedbackServices", policy.UsesFeedbackServices);
        }

        private static void AddServiceActivation(
            GameplayWiringReport report,
            string contract,
            bool active)
        {
            if (!active)
                return;

            report.Add(new GameplayWiringRow(
                GameplayWiringRowKind.ServiceActivation,
                contract,
                "RuntimeFeatureServicePolicy",
                "FeatureServiceInstaller",
                "Feature service family",
                "Glue.ServiceRegistration",
                GameplayWiringScope.Feature,
                GameplayWiringTiming.Startup,
                GameplayWiringRequiredness.AutoDerived,
                GameplayWiringSeverity.Info,
                "Authored data or loaded scene evidence currently asks for this service family."));
        }

        private static void AddRuntimeSceneSearchInventoryEvidence(GameplayWiringReport report)
        {
            AddRuntimeSceneSearchInventoryRow(
                report,
                "CombatServices",
                "PawnCombatBehaviour, PawnCombatBehaviour2D, or CombatFlowController",
                CombatServiceInstaller.ContainsLoadedSceneEvidence());
            AddRuntimeSceneSearchInventoryRow(
                report,
                "EnemyServices",
                nameof(EnemyAI),
                EnemyServiceInstaller.ContainsLoadedSceneEvidence());
            AddRuntimeSceneSearchInventoryRow(
                report,
                "RpgServices",
                "NeonBlack.Gameplay.Modules.Rpg namespace",
                RuntimeSceneSearch.ContainsComponentInNamespace("NeonBlack.Gameplay.Modules.Rpg"));
            AddRuntimeSceneSearchInventoryRow(
                report,
                "GameFlowServices",
                nameof(ArcadeGameFlowController) + " or NeonBlack.Gameplay.Glue.SceneFlow.Arcade2D namespace",
                RuntimeSceneSearch.ContainsComponent<ArcadeGameFlowController>()
                || RuntimeSceneSearch.ContainsComponentInNamespace("NeonBlack.Gameplay.Glue.SceneFlow.Arcade2D"));
            AddRuntimeSceneSearchInventoryRow(
                report,
                "ScoringServices",
                "ParticipantScoreService or StillnessBonus2D",
                ScoringServiceInstaller.ContainsLoadedSceneEvidence());
            AddRuntimeSceneSearchInventoryRow(
                report,
                "FeedbackServices",
                "ParticipantFeedbackService or NeonBlack.Gameplay.Modules.Feedback namespace",
                FeedbackServiceInstaller.ContainsLoadedSceneEvidence());
        }

        private static void AddRuntimeSceneSearchInventoryRow(
            GameplayWiringReport report,
            string serviceFamily,
            string evidenceSource,
            bool found)
        {
            if (!found)
                return;

            report.Add(new GameplayWiringRow(
                GameplayWiringRowKind.Inventory,
                "RuntimeSceneSearch",
                evidenceSource,
                nameof(RuntimeFeatureServicePolicy),
                serviceFamily,
                "Glue.ServiceRegistration",
                GameplayWiringScope.Feature,
                GameplayWiringTiming.Startup,
                GameplayWiringRequiredness.AutoDerived,
                GameplayWiringSeverity.Info,
                "Loaded scene search found feature evidence used by RuntimeFeatureServicePolicy. This row is inventory only; registration behavior is unchanged."));
        }

        private static void AddRuntimeValidationEvidence(
            GameplayWiringReport report,
            GameObject gameplayRoot,
            SessionDefinition sessionDefinition)
        {
            if (sessionDefinition is IRuntimeValidationProvider sessionProvider)
                AddValidationProviderRows(report, sessionProvider, sessionDefinition.name);

            if (gameplayRoot == null)
                return;

            MonoBehaviour[] behaviours = gameplayRoot.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IRuntimeValidationProvider provider)
                    AddValidationProviderRows(report, provider, GetComponentPath(behaviours[i]));
            }
        }

        private static void AddValidationProviderRows(
            GameplayWiringReport report,
            IRuntimeValidationProvider provider,
            string providerPath)
        {
            if (provider == null)
                return;

            RuntimeValidationIssue[] issues;
            try
            {
                issues = ToArray(provider.GetRuntimeValidationIssues());
            }
            catch (Exception exception)
            {
                report.Add(new GameplayWiringRow(
                    GameplayWiringRowKind.ValidationIssue,
                    "RuntimeValidationProvider",
                    provider.GetType().Name,
                    "Runtime Wiring",
                    providerPath,
                    provider.GetType().Name,
                    GameplayWiringScope.Unknown,
                    GameplayWiringTiming.Authoring,
                    GameplayWiringRequiredness.Required,
                    GameplayWiringSeverity.Error,
                    $"Validation provider threw while reporting setup evidence: {exception.Message}"));
                return;
            }

            for (int i = 0; i < issues.Length; i++)
            {
                RuntimeValidationIssue issue = issues[i];
                if (issue == null)
                    continue;
                if (IsCoveredByCanonicalMissingProvider(report, issue))
                    continue;
                if (ShouldDeferRouteDependentValidation(report, issue))
                    continue;

                report.Add(new GameplayWiringRow(
                    GameplayWiringRowKind.ValidationIssue,
                    string.IsNullOrWhiteSpace(issue.IssueCode) ? "RuntimeValidation" : issue.IssueCode,
                    provider.GetType().Name,
                    issue.TargetLabel,
                    issue.FieldPath,
                    provider.GetType().Name,
                    GameplayWiringScope.Unknown,
                    GameplayWiringTiming.Authoring,
                    ToRequiredness(issue.Severity),
                    ToSeverity(issue.Severity),
                    BuildValidationEvidence(issue, providerPath)));
            }
        }

        private static bool IsCoveredByCanonicalMissingProvider(
            GameplayWiringReport report,
            RuntimeValidationIssue issue)
        {
            if (!string.Equals(
                issue.IssueCode,
                "GameplaySessionBootstrap.SessionDefinition.Missing",
                StringComparison.Ordinal))
                return false;

            return HasRow(
                report,
                GameplayWiringRowKind.MissingProvider,
                "SessionDefinition",
                nameof(GameplaySessionBootstrap),
                "sessionDefinition");
        }

        private static bool ShouldDeferRouteDependentValidation(
            GameplayWiringReport report,
            RuntimeValidationIssue issue)
        {
            if (!string.Equals(
                issue.IssueCode,
                "GameplaySessionBootstrap.CameraRig.Optional",
                StringComparison.Ordinal))
                return false;

            return HasRow(
                report,
                GameplayWiringRowKind.MissingProvider,
                "SessionDefinition",
                nameof(GameplaySessionBootstrap),
                "sessionDefinition")
                || HasRow(
                    report,
                    GameplayWiringRowKind.TimingIssue,
                    "ParticipantServiceRoute",
                    "Participant runtime services",
                    "Session participant route requirements");
        }

        private static bool HasParticipantRouteData(SessionDefinition sessionDefinition)
        {
            return sessionDefinition != null
                && sessionDefinition.defaultGameMode != null
                && sessionDefinition.defaultParticipants != null
                && sessionDefinition.defaultParticipants.Length > 0;
        }

        private static bool HasRow(
            GameplayWiringReport report,
            GameplayWiringRowKind kind,
            string contract,
            string receiver,
            string package)
        {
            if (report == null)
                return false;

            for (int i = 0; i < report.Rows.Count; i++)
            {
                GameplayWiringRow row = report.Rows[i];
                if (row.Kind == kind
                    && string.Equals(row.Contract, contract, StringComparison.Ordinal)
                    && string.Equals(row.Receiver, receiver, StringComparison.Ordinal)
                    && string.Equals(row.Package, package, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static RuntimeValidationIssue[] ToArray(System.Collections.Generic.IEnumerable<RuntimeValidationIssue> issues)
        {
            if (issues == null)
                return Array.Empty<RuntimeValidationIssue>();

            System.Collections.Generic.List<RuntimeValidationIssue> result = new System.Collections.Generic.List<RuntimeValidationIssue>();
            foreach (RuntimeValidationIssue issue in issues)
                result.Add(issue);

            return result.ToArray();
        }

        private static string BuildValidationEvidence(RuntimeValidationIssue issue, string providerPath)
        {
            string evidence = issue.Message;
            if (!string.IsNullOrWhiteSpace(issue.NativeAction))
                evidence += " Action: " + issue.NativeAction;
            if (!string.IsNullOrWhiteSpace(issue.SuccessCheck))
                evidence += " Success: " + issue.SuccessCheck;
            if (!string.IsNullOrWhiteSpace(providerPath))
                evidence += " Source: " + providerPath;

            return evidence;
        }

        private static GameplayWiringRequiredness ToRequiredness(RuntimeValidationSeverity severity)
        {
            return severity == RuntimeValidationSeverity.Required
                ? GameplayWiringRequiredness.Required
                : GameplayWiringRequiredness.Optional;
        }

        private static GameplayWiringSeverity ToSeverity(RuntimeValidationSeverity severity)
        {
            switch (severity)
            {
                case RuntimeValidationSeverity.Required:
                    return GameplayWiringSeverity.Error;
                case RuntimeValidationSeverity.Recommended:
                    return GameplayWiringSeverity.Warning;
                default:
                    return GameplayWiringSeverity.Info;
            }
        }

        private static void AddRequiredComponent<T>(
            GameplayWiringReport report,
            GameObject gameplayRoot,
            string contract,
            string receiver,
            GameplayWiringScope scope,
            GameplayWiringTiming timing)
            where T : Component
        {
            T[] components = FindComponents<T>(gameplayRoot);
            if (components.Length == 0)
            {
                report.Add(new GameplayWiringRow(
                    GameplayWiringRowKind.MissingProvider,
                    contract,
                    string.Empty,
                    receiver,
                    typeof(T).Name,
                    receiver,
                    scope,
                    timing,
                    GameplayWiringRequiredness.Required,
                    GameplayWiringSeverity.Error,
                    $"No {typeof(T).Name} was found under the gameplay root."));
                return;
            }

            AddAmbiguousProviderRowIfNeeded(
                report,
                contract,
                receiver,
                typeof(T).Name,
                components.Length,
                scope,
                timing,
                GameplayWiringRequiredness.Required);

            for (int i = 0; i < components.Length; i++)
            {
                report.Add(new GameplayWiringRow(
                    GameplayWiringRowKind.Provider,
                    contract,
                    GetComponentPath(components[i]),
                    receiver,
                    typeof(T).Name,
                    receiver,
                    scope,
                    timing,
                    GameplayWiringRequiredness.Required,
                    GameplayWiringSeverity.Info,
                    $"{typeof(T).Name} is present under the gameplay root."));
            }
        }

        private static void AddOptionalComponent<T>(
            GameplayWiringReport report,
            GameObject gameplayRoot,
            string contract,
            string receiver,
            GameplayWiringScope scope,
            GameplayWiringTiming timing)
            where T : Component
        {
            T[] components = FindComponents<T>(gameplayRoot);
            if (components.Length == 0)
                return;

            AddAmbiguousProviderRowIfNeeded(
                report,
                contract,
                receiver,
                typeof(T).Name,
                components.Length,
                scope,
                timing,
                GameplayWiringRequiredness.Optional);

            for (int i = 0; i < components.Length; i++)
            {
                report.Add(new GameplayWiringRow(
                    GameplayWiringRowKind.Provider,
                    contract,
                    GetComponentPath(components[i]),
                    receiver,
                    typeof(T).Name,
                    receiver,
                    scope,
                    timing,
                    GameplayWiringRequiredness.Optional,
                    GameplayWiringSeverity.Info,
                    $"{typeof(T).Name} is available as optional runtime wiring evidence."));
            }
        }

        private static void AddAmbiguousProviderRowIfNeeded(
            GameplayWiringReport report,
            string contract,
            string receiver,
            string package,
            int count,
            GameplayWiringScope scope,
            GameplayWiringTiming timing,
            GameplayWiringRequiredness requiredness)
        {
            if (count <= 1)
                return;

            report.Add(new GameplayWiringRow(
                GameplayWiringRowKind.AmbiguousProvider,
                contract,
                count + " providers",
                receiver,
                package,
                "Runtime Wiring",
                scope,
                timing,
                requiredness,
                GameplayWiringSeverity.Warning,
                $"Multiple {package} providers were found under the gameplay root. This may be valid for repeated scene services, but singleton setup should choose one explicit provider."));
        }

        private static void AddOptionalInterface<T>(
            GameplayWiringReport report,
            GameObject gameplayRoot,
            string contract,
            string receiver,
            GameplayWiringScope scope,
            GameplayWiringTiming timing)
            where T : class
        {
            T service = gameplayRoot.GetComponentInChildren<T>(true);
            if (service == null)
                return;

            report.Add(new GameplayWiringRow(
                GameplayWiringRowKind.Provider,
                contract,
                GetObjectLabel(service),
                receiver,
                typeof(T).Name,
                receiver,
                scope,
                timing,
                GameplayWiringRequiredness.Optional,
                GameplayWiringSeverity.Info,
                $"{typeof(T).Name} is available as optional runtime wiring evidence."));
        }

        private static T FindComponent<T>(GameObject gameplayRoot)
            where T : Component
        {
            return gameplayRoot != null ? gameplayRoot.GetComponentInChildren<T>(true) : null;
        }

        private static T[] FindComponents<T>(GameObject gameplayRoot)
            where T : Component
        {
            return gameplayRoot != null
                ? gameplayRoot.GetComponentsInChildren<T>(true)
                : Array.Empty<T>();
        }

        private static string GetComponentPath(Component component)
        {
            return component != null ? GetPath(component.transform) + "/" + component.GetType().Name : string.Empty;
        }

        private static string GetObjectLabel(object value)
        {
            if (value is Component component)
                return GetComponentPath(component);

            return value != null ? value.GetType().Name : string.Empty;
        }

        private static string GetPath(Transform transform)
        {
            if (transform == null)
                return string.Empty;

            string path = transform.name;
            Transform parent = transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}
