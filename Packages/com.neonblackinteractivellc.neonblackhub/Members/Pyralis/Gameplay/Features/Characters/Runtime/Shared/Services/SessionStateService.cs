using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Contracts.Networking;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using VContainer;

namespace NeonBlack.Gameplay.Characters
{
    /// <summary>
    /// Shared session state and host-authoritative startup rules for NeonBlack Gameplay.
    /// </summary>
    public class SessionStateService : MonoBehaviour, IGameService, IGameplayStateReader
    {
        public enum SessionPhase
        {
            Boot,
            Lobby,
            Gameplay,
            Results
        }

        [SerializeField] private SessionDefinition sessionDefinition;
        [SerializeField] private bool autoStartGameplay = true;

        private ISessionOwnershipService _sessionOwnershipService;
        private IGameplaySettingsApplier _settingsApplier;

        public SessionDefinition ActiveSessionDefinition => sessionDefinition;
        public GameModeDefinition ActiveGameMode => sessionDefinition != null ? sessionDefinition.defaultGameMode : null;
        public int EffectiveMaxParticipants => sessionDefinition != null ? sessionDefinition.GetEffectiveMaxParticipants() : 1;
        public SessionPhase CurrentPhase { get; private set; } = SessionPhase.Boot;
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
            CurrentPhase = autoStartGameplay ? SessionPhase.Gameplay : SessionPhase.Lobby;
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
            CurrentPhase = phase;
        }

        [Inject]
        private void Construct(
            ISessionOwnershipService sessionOwnershipService = null,
            IGameplaySettingsApplier settingsApplier = null)
        {
            _sessionOwnershipService = sessionOwnershipService;
            _settingsApplier = settingsApplier;
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
    }
}
