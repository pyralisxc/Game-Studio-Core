using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Glue.Session;
using NeonBlack.Gameplay.Glue.Spawning;
using NeonBlack.Gameplay.Modules.Character;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Glue.SceneFlow.Arcade2D
{
    public partial class ArcadeGameFlowController
    {
        private readonly ArcadeFlowStateReader _standaloneStateReader = new ArcadeFlowStateReader();

        private void SetState(GameState state)
        {
            _currentState = state;
            ApplySessionPhase(state);
            OnGameStateChanged?.Invoke(state);
        }

        private void ApplySessionPhase(GameState state)
        {
            if (_sessionStateService == null)
                return;

            _sessionStateService.SetPhase(state == GameState.Playing
                ? SessionStateService.SessionPhase.Gameplay
                : SessionStateService.SessionPhase.Results);
        }

        private IGameplayStateReader ResolveGameplayStateReader()
        {
            if (_gameplayStateReader != null)
                return _gameplayStateReader;

            if (_sessionStateService != null)
                return _sessionStateService;

            _standaloneStateReader.Owner = this;
            return _standaloneStateReader;
        }

        private sealed class ArcadeFlowStateReader : IGameplayStateReader
        {
            public ArcadeGameFlowController Owner { get; set; }
            public bool IsGameplayActive => Owner != null && Owner.CurrentState == GameState.Playing;
        }
    }
}
