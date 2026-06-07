using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Scoring
{
/// <summary>
/// Compile-safe leaderboard bridge.
/// When Unity Gaming Services leaderboard packages are not installed, this remains a
/// no-op service so the rest of the gameplay package can still compile and run.
/// </summary>
[AddComponentMenu("NeonBlack/Gameplay/Scoring/Leaderboard Manager")]
[DefaultExecutionOrder(-50)]
public class LeaderboardManager : MonoBehaviour, ILeaderboardService
{
    public static LeaderboardManager Instance { get; private set; }

    [SerializeField, Tooltip("Must match the Leaderboard ID in your backend service when leaderboard integration is enabled.")]
    private string _leaderboardId = "main_leaderboard";

    [SerializeField, Range(1, 100), Tooltip("Maximum number of rows fetched for the top scores display when leaderboard integration is enabled.")]
    private int _topScoresFetchLimit = 10;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SubmitScore(int score)
    {
        if (score <= 0)
            return;

        Debug.LogWarning($"[LeaderboardManager] Leaderboard services are not installed. Score '{score}' for '{_leaderboardId}' was not submitted.", this);
    }

    public Task<List<LeaderboardEntry>> GetTopScoresAsync()
    {
        Debug.LogWarning($"[LeaderboardManager] Leaderboard services are not installed. Returning no scores for '{_leaderboardId}'.", this);
        return Task.FromResult(new List<LeaderboardEntry>(_topScoresFetchLimit));
    }

    public Task<LeaderboardEntry> GetPlayerEntryAsync()
    {
        Debug.LogWarning($"[LeaderboardManager] Leaderboard services are not installed. No player rank is available for '{_leaderboardId}'.", this);
        return Task.FromResult<LeaderboardEntry>(null);
    }
}
}
