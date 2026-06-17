using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Scoring;
using VContainer;

namespace NeonBlack.Gameplay.Features.GameFlow
{
    public partial class GameManager
    {
        private ParticipantRosterService _participantRosterService;
        private ICameraBoundsProvider _cameraBoundsProvider;
        private IGameplayStateReader _gameplayStateReader;
        private SessionStateService _sessionStateService;

        [Inject]
        private void Construct(
            ParticipantRosterService participantRosterService = null,
            ILeaderboardService leaderboardService = null,
            ICameraBoundsProvider cameraBoundsProvider = null,
            ISceneNavigator sceneNavigator = null,
            IGameplaySettingsApplier settings = null,
            IGameplayStateReader gameplayStateReader = null,
            SessionStateService sessionStateService = null)
        {
            _participantRosterService = participantRosterService;
            _leaderboardService = leaderboardService;
            if (cameraBoundsProvider != null)
                _cameraBoundsProvider = cameraBoundsProvider;
            if (sceneNavigator != null)
                _sceneNavigator = sceneNavigator;
            if (settings != null)
                _settings = settings;
            if (gameplayStateReader != null)
                _gameplayStateReader = gameplayStateReader;
            if (sessionStateService != null)
                _sessionStateService = sessionStateService;
        }

        private void ConfigureRuntimeDependencies()
        {
            IGameplayStateReader stateReader = ResolveGameplayStateReader();
            pickupSpawner?.ConfigureRuntime(stateReader, _cameraBoundsProvider);
            hazardSpawner?.ConfigureRuntime(stateReader, _cameraBoundsProvider, this, pickupSpawner);
        }
    }
}
