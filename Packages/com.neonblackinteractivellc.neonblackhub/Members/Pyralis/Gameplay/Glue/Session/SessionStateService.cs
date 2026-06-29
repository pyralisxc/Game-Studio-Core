using System.Collections.Generic;
using NeonBlack.Gameplay.Glue.Session;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Contracts.Networking;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using VContainer;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Glue.Session
{
    /// <summary>
    /// Shared session state and host-authoritative startup rules for NeonBlack Gameplay.
    /// </summary>
    /// <summary>
    /// Service for tracking and reading the high-level state of the gameplay session (e.g., Playing, Paused).
    /// </summary>
    [AuthoringContract(
        Category = "Session",
        CapabilityPath = "Core Setup/Session/Session State Service",
        Surface = AuthoringSurface.Service,
        Summary = "Global service for tracking high-level gameplay session states (Playing, Paused, Lobby).",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/session",
        RequiredInterfaces = new[] { typeof(IGameService), typeof(IGameplayStateReader) },
        PrerequisiteStableIds = new[] { "bootstrap.root", "session.definition" },
        RouteStage = "Scene Services",
        RouteOrder = 25,
        SetupDomain = "Session",
        ProofTarget = "Session state service receives the active SessionDefinition before Play Mode.",
        NativeActionKind = AuthoringActionKind.AddComponent,
        SetupSteps = new[] { "Add to GameplaySessionBootstrap child." },
        SuccessChecks = new[] { "Verify the session transitions from Boot to Gameplay state upon startup." },
        Tags = new[] { "capability:Session" },
        Selectable = false
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Setup/Session State Service")]
    public class SessionStateService : MonoBehaviour, IGameService, IGameplayStateReader, IRuntimeValidationProvider
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (sessionDefinition == null)
                yield return RuntimeValidationIssue.Required("Session Definition is empty. This is expected when GameplaySessionBootstrap injects the session at runtime.");
        }
        public enum SessionPhase
        {
            Boot,
            Lobby,
            Gameplay,
            Results
        }

        [SerializeField] private SessionDefinition sessionDefinition;
        [SerializeField] private bool autoStartGameplay = true;

        private readonly SessionStateMachine _stateMachine = new SessionStateMachine();
        private ISessionOwnershipService _sessionOwnershipService;
        private IGameplaySettingsApplier _settingsApplier;
        private IGameplayEventChannel _eventChannel;

        public SessionDefinition ActiveSessionDefinition => sessionDefinition;
        public GameModeDefinition ActiveGameMode => sessionDefinition != null ? sessionDefinition.defaultGameMode : null;
        public int EffectiveMaxParticipants => sessionDefinition != null ? sessionDefinition.GetEffectiveMaxParticipants() : 1;
        public SessionLifecycleState CurrentLifecycleState => _stateMachine.CurrentState;
        public SessionPhase CurrentPhase => ToSessionPhase(CurrentLifecycleState);
        public bool IsGameplayActive => CurrentPhase == SessionPhase.Gameplay;

        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        public void Initialize()
        {
            TryStartHostIfNeeded();
            ApplySettingsDefaults();
            ResetLifecycle(SessionLifecycleState.Booting);
            MoveToLifecycleState(autoStartGameplay ? SessionLifecycleState.Playing : SessionLifecycleState.AuthoringReady);
        }

        public void Shutdown()
        {
        }

        public void SetSessionDefinition(SessionDefinition definition)
        {
            sessionDefinition = definition;
            sessionDefinition?.Sanitize();
        }

        public void SetPhase(SessionPhase phase)
        {
            MoveToLifecycleState(ToLifecycleState(phase));
        }

        [Inject]
        private void Construct(
            ISessionOwnershipService sessionOwnershipService = null,
            IGameplaySettingsApplier settingsApplier = null,
            IGameplayEventChannel eventChannel = null)
        {
            _sessionOwnershipService = sessionOwnershipService;
            _settingsApplier = settingsApplier;
            _eventChannel = eventChannel;
        }

        /// <summary>Override in a networked subclass to start the host when <see cref="SessionDefinition.autoStartHost"/> is true.</summary>
        protected virtual void TryStartHostIfNeeded()
        {
            ISessionOwnershipService ownershipService = _sessionOwnershipService;
            if (ownershipService == null)
                return;

            ownershipService.TryStartSessionHost();
        }

        private void ApplySettingsDefaults()
        {
            SettingsProfile settingsProfile = sessionDefinition != null ? sessionDefinition.settingsProfile : null;
            if (settingsProfile == null || _settingsApplier == null)
                return;

            _settingsApplier.SetMusicVolume(settingsProfile.defaultMusicVolume);
            _settingsApplier.SetSFXVolume(settingsProfile.defaultSfxVolume);
            _settingsApplier.SetJoystickDeadzone(settingsProfile.defaultJoystickDeadzone);
            _settingsApplier.SetGamepadDeadzone(settingsProfile.defaultGamepadDeadzone);
            _settingsApplier.SetSwapControls(settingsProfile.defaultSwapControls);
        }

        private void MoveToLifecycleState(SessionLifecycleState targetState)
        {
            if (CurrentLifecycleState == targetState)
                return;

            switch (targetState)
            {
                case SessionLifecycleState.Booting:
                    ResetLifecycle(SessionLifecycleState.Booting);
                    return;
                case SessionLifecycleState.AuthoringReady:
                    TryApplyLifecycleState(SessionLifecycleState.AuthoringReady);
                    return;
                case SessionLifecycleState.Loading:
                    EnsureAuthoringReady();
                    TryApplyLifecycleState(SessionLifecycleState.Loading);
                    return;
                case SessionLifecycleState.Playing:
                    EnsureAuthoringReady();
                    TryApplyLifecycleState(SessionLifecycleState.Loading);
                    TryApplyLifecycleState(SessionLifecycleState.Playing);
                    return;
                case SessionLifecycleState.Paused:
                    EnsurePlaying();
                    TryApplyLifecycleState(SessionLifecycleState.Paused);
                    return;
                case SessionLifecycleState.Results:
                    EnsurePlaying();
                    TryApplyLifecycleState(SessionLifecycleState.Results);
                    return;
                case SessionLifecycleState.Ending:
                    TryApplyLifecycleState(SessionLifecycleState.Ending);
                    return;
            }
        }

        private void EnsureAuthoringReady()
        {
            if (CurrentLifecycleState == SessionLifecycleState.Booting)
                TryApplyLifecycleState(SessionLifecycleState.AuthoringReady);
        }

        private void EnsurePlaying()
        {
            if (CurrentLifecycleState == SessionLifecycleState.Playing)
                return;

            MoveToLifecycleState(SessionLifecycleState.Playing);
        }

        private void ResetLifecycle(SessionLifecycleState state)
        {
            SessionLifecycleState previousState = CurrentLifecycleState;
            _stateMachine.Reset(state);
            PublishLifecycleChanged(previousState, state);
        }

        private bool TryApplyLifecycleState(SessionLifecycleState state)
        {
            SessionLifecycleState previousState = CurrentLifecycleState;
            if (!_stateMachine.TryTransitionTo(state))
                return false;

            PublishLifecycleChanged(previousState, state);
            return true;
        }

        private void PublishLifecycleChanged(SessionLifecycleState previousState, SessionLifecycleState currentState)
        {
            if (previousState == currentState || _eventChannel == null)
                return;

            _eventChannel.Publish(new SessionLifecycleChangedEvent(previousState, currentState));
        }

        private static SessionLifecycleState ToLifecycleState(SessionPhase phase)
        {
            switch (phase)
            {
                case SessionPhase.Boot:
                    return SessionLifecycleState.Booting;
                case SessionPhase.Lobby:
                    return SessionLifecycleState.AuthoringReady;
                case SessionPhase.Gameplay:
                    return SessionLifecycleState.Playing;
                case SessionPhase.Results:
                    return SessionLifecycleState.Results;
                default:
                    return SessionLifecycleState.Booting;
            }
        }

        private static SessionPhase ToSessionPhase(SessionLifecycleState state)
        {
            switch (state)
            {
                case SessionLifecycleState.Booting:
                    return SessionPhase.Boot;
                case SessionLifecycleState.AuthoringReady:
                case SessionLifecycleState.Loading:
                case SessionLifecycleState.Paused:
                    return SessionPhase.Lobby;
                case SessionLifecycleState.Playing:
                    return SessionPhase.Gameplay;
                case SessionLifecycleState.Results:
                case SessionLifecycleState.Ending:
                    return SessionPhase.Results;
                default:
                    return SessionPhase.Boot;
            }
        }
    }
}
