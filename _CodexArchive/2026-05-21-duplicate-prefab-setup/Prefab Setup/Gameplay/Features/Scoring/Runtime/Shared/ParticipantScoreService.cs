using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Characters;
using UnityEngine;
using UnityEngine.Events;

namespace NeonBlack.Gameplay.Features.Scoring
{
    /// <summary>
    /// Canonical scoring service for NeonBlack gameplay sessions.
    /// Tracks per-participant scores in multi-player scenarios, and session-level
    /// points, survival time, and high-score persistence for single-player scenarios.
    /// Register this service through the Pyralis gameplay composition root and resolve it via DI.
    /// </summary>
    [DefaultExecutionOrder(-30)]
    public class ParticipantScoreService : MonoBehaviour, IGameService
    {
        // â”€â”€ PlayerPrefs keys â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public const string HighScorePointsKey   = "HighScore_Points";
        public const string HighScoreTimeKey     = "HighScore_Time";
        public const string HighScoreBestTimeKey = "HighScore_BestTime";

        // â”€â”€ Per-participant scores â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private readonly Dictionary<int, int> _scores = new Dictionary<int, int>();

        // â”€â”€ Session-level tracking â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private int   _pointsCollected;
        private float _survivalTime;
        private bool  _isTiming;

        private int   _highScorePointsCached;
        private float _highScoreTimeCached;
        private float _highScoreBestTimeCached;

        // â”€â”€ Events â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        /// <summary>Fires with the current session point count whenever points are added.</summary>
        public UnityEvent<int> OnPointsChanged   = new UnityEvent<int>();

        /// <summary>Fires with the new best point count when a high score is beaten.</summary>
        public UnityEvent<int> OnHighScoreBeaten = new UnityEvent<int>();

        // â”€â”€ Properties â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public int   PointsCollected   => _pointsCollected;
        public float SurvivalTime      => _survivalTime;
        public int   HighScorePoints   => _highScorePointsCached;
        public float HighScoreTime     => _highScoreTimeCached;
        public float HighScoreBestTime => _highScoreBestTimeCached;

        // â”€â”€ Unity lifecycle â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private void Awake()
        {
            _highScorePointsCached   = PlayerPrefs.GetInt(HighScorePointsKey, 0);
            _highScoreTimeCached     = PlayerPrefs.GetFloat(HighScoreTimeKey, 0f);
            _highScoreBestTimeCached = PlayerPrefs.GetFloat(HighScoreBestTimeKey, 0f);
        }

        private void Update()
        {
            if (_isTiming)
                _survivalTime += Time.deltaTime;
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        // â”€â”€ IGameService â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public void Initialize()
        {
            _scores.Clear();
        }

        public void Shutdown()
        {
            _scores.Clear();
        }

        // â”€â”€ Session-level score â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        /// <summary>Resets session points and survival time, and starts the timer.</summary>
        public void ResetScore()
        {
            _pointsCollected = 0;
            _survivalTime    = 0f;
            _isTiming        = true;
            OnPointsChanged?.Invoke(_pointsCollected);
        }

        /// <summary>Adds points to the session total and fires <see cref="OnPointsChanged"/>.</summary>
        public void AddPoints(int amount = 1)
        {
            if (amount <= 0)
                return;

            _pointsCollected += amount;
            OnPointsChanged?.Invoke(_pointsCollected);
        }

        /// <summary>Stops the survival timer without resetting it.</summary>
        public void StopTimer()
        {
            _isTiming = false;
        }

        /// <summary>
        /// Persists the session score to PlayerPrefs if it beats the stored record.
        /// Also independently tracks the longest survival time ever recorded.
        /// </summary>
        public void SaveHighScore()
        {
            if (_pointsCollected > _highScorePointsCached)
            {
                PlayerPrefs.SetInt(HighScorePointsKey, _pointsCollected);
                PlayerPrefs.SetFloat(HighScoreTimeKey, _survivalTime);
                _highScorePointsCached = _pointsCollected;
                _highScoreTimeCached   = _survivalTime;
                OnHighScoreBeaten?.Invoke(_pointsCollected);
            }

            if (_survivalTime > _highScoreBestTimeCached)
            {
                PlayerPrefs.SetFloat(HighScoreBestTimeKey, _survivalTime);
                _highScoreBestTimeCached = _survivalTime;
            }

            PlayerPrefs.Save();
        }

        /// <summary>Formats a seconds value as M:SS (for example 75.4f â†’ "1:15").</summary>
        public static string FormatTime(float seconds)
        {
            int minutes      = Mathf.FloorToInt(seconds / 60f);
            int wholeSeconds = Mathf.FloorToInt(seconds % 60f);
            return $"{minutes}:{wholeSeconds:00}";
        }

        // â”€â”€ Per-participant score â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        /// <summary>Adds <paramref name="amount"/> to the named participant's score.</summary>
        public void AddScore(ParticipantHandle participant, int amount)
        {
            if (participant == null)
                return;

            int key = participant.Id.Value;
            _scores.TryGetValue(key, out int current);
            _scores[key] = current + amount;
        }

        /// <summary>Returns the current score for the given participant, or 0 if not found.</summary>
        public int GetScore(ParticipantHandle participant)
        {
            if (participant == null)
                return 0;

            return _scores.TryGetValue(participant.Id.Value, out int score) ? score : 0;
        }
    }
}
