using UnityEngine.Events;

namespace NeonBlack.Gameplay.Core.Contracts
{
    public enum GameState
    {
        Playing,
        Dead,
        GameOver
    }

    public interface IGameplaySessionFlow
    {
        GameState CurrentState { get; }

        void AddGameStateChangedListener(UnityAction<GameState> listener);
        void RemoveGameStateChangedListener(UnityAction<GameState> listener);
        void RestartGame();
        void GoToMainMenu();
    }
}
