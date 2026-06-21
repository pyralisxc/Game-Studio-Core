using UnityEngine.Events;

namespace NeonBlack.Gameplay.Core.Contracts
{
    public interface ISessionScoreAwardSink
    {
        void AddPoints(int amount = 1);
    }

    public interface ISessionScoreService : ISessionScoreAwardSink
    {
        int PointsCollected { get; }
        float SurvivalTime { get; }
        int HighScorePoints { get; }
        float HighScoreTime { get; }
        float HighScoreBestTime { get; }

        void AddPointsChangedListener(UnityAction<int> listener);
        void RemovePointsChangedListener(UnityAction<int> listener);
    }
}
