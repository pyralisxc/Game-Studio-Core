using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeonBlack.Gameplay.Modules.Scoring
{
    public interface ILeaderboardService
    {
        void SubmitScore(int score);
        Task<List<LeaderboardEntry>> GetTopScoresAsync();
        Task<LeaderboardEntry> GetPlayerEntryAsync();
    }
}
