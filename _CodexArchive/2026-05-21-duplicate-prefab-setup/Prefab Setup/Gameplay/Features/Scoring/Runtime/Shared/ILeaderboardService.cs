using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeonBlack.Gameplay.Features.Scoring
{
    public interface ILeaderboardService
    {
        void SubmitScore(int score);
        Task<List<LeaderboardEntry>> GetTopScoresAsync();
        Task<LeaderboardEntry> GetPlayerEntryAsync();
    }
}
