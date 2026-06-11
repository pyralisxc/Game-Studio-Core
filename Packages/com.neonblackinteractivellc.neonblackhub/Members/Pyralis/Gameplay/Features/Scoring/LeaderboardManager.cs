using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Scoring
{
/// <summary>
/// Compile-safe leaderboard bridge.
/// When Unity Gaming Services leaderboard packages are not installed, this remains a
/// no-op service so the rest of the gameplay package can still compile and run.
/// </summary>
[AuthoringContract(
    Capability = AuthoringCapability.Session,
    Relevance = "No-op bridge for leaderboard services. Replace with a package-specific manager for online scores.",
    NativeSetup = new[] { "Place in the menu or bootstrap scene.", "Set Leaderboard ID." },
    AssignmentFields = new[] { nameof(_leaderboardId), nameof(_topScoresFetchLimit) },
    FirstProof = "Verify 'Leaderboard services not installed' warning appears in console when submitting score.",
    ExpertAdvice = "Use this bridge to keep code compiling without backend dependencies. Ensure the Leaderboard ID matches your online configuration."
)]
[AddComponentMenu("NeonBlack/Gameplay/Scoring/Leaderboard Manager")]
[DefaultExecutionOrder(-50)]
public class LeaderboardManager : MonoBehaviour, ILeaderboardService, IRuntimeValidationProvider
{
    public IEnumerable<string> GetRuntimeValidationIssues()
    {
        if (string.IsNullOrWhiteSpace(_leaderboardId)) yield return "Leaderboard ID cannot be blank.";
        if (_topScoresFetchLimit <= 0) yield return "Fetch limit must be positive.";
    }
    [SerializeField, Tooltip("Must match the Leaderboard ID in your backend service when leaderboard integration is enabled.")]
    private string _leaderboardId = "main_leaderboard";

    [SerializeField, Range(1, 100), Tooltip("Maximum number of rows fetched for the top scores display when leaderboard integration is enabled.")]
    private int _topScoresFetchLimit = 10;

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
