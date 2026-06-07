using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using VContainer;

namespace NeonBlack.Gameplay.Features.Scoring
{
/// <summary>
/// 2D scoring bonus that awards points for staying still.
/// Every <see cref="_stillnessInterval"/> seconds of consecutive stillness,
/// <see cref="_collectiblesPerBonus"/> points are added to the score.
/// The timer resets the moment the player moves above the velocity threshold.
///
/// Setup:
///   1. Attach to the Player GameObject alongside Motor2D.
///   2. Wire _motor in the Inspector (or leave empty â€” auto-fetched in Awake).
///   3. Optionally assign _bonusClip for an audio cue on reward.
///   4. Tune _collectiblesPerBonus and _stillnessInterval in the Inspector.
/// </summary>
[RequireComponent(typeof(Motor2D))]
[AddComponentMenu("NeonBlack/Gameplay/Features/Scoring/Stillness Bonus 2D")]
public class StillnessBonus2D : MonoBehaviour
{
    [Header("Reward Settings")]
    [SerializeField, Tooltip("Points added to the score each time the stillness interval completes.")]
    [Min(1)]
    [UnityEngine.Serialization.FormerlySerializedAs("_crumbsPerBonus")]
    private int _collectiblesPerBonus = 1;

    [SerializeField, Tooltip("Seconds of consecutive stillness required to earn the reward.")]
    [Min(0.5f)]
    private float _stillnessInterval = 3f;

    [SerializeField, Tooltip("Velocity magnitude (world units/sec) below which the player counts as still.")]
    [Range(0f, 0.5f)]
    private float _stillnessThreshold = 0.05f;

    [Header("Audio")]
    [SerializeField, Tooltip("Optional sound played when a stillness bonus is awarded.")]
    private AudioClip _bonusClip;

    [SerializeField, Tooltip("Volume of the bonus clip, scaled by SettingsManager.SFXVolume.")]
    [Range(0f, 1f)]
    private float _baseVolume = 0.8f;

    // â”€â”€ References â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private Motor2D _motor;
    private AudioSource _audioSource;
    private ParticipantScoreService _scoreService;
    private IGameplayStateReader _gameplayStateReader;

    // â”€â”€ Runtime â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private float _stillTimer;

    /// <summary>Seconds accumulated without moving. Resets to 0 on movement.</summary>
    public float StillTime => _stillTimer;

    /// <summary>Normalised 0â€“1 progress toward the next reward (useful for a UI fill bar).</summary>
    public float StillProgress => _stillnessInterval > 0f ? _stillTimer / _stillnessInterval : 0f;

    private void Awake()
    {
        _motor = GetComponent<Motor2D>();

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.spatialBlend = 0f;
        _audioSource.playOnAwake  = false;

        // Validate mixer routing. Without an Output Group assigned, SettingsManager's
        // SFX volume slider has no effect on this component's clips.
        if (_audioSource.outputAudioMixerGroup == null)
            Debug.LogError("[StillnessBonus2D] AudioSource has no Output AudioMixerGroup assigned. "
                + "Drag your SFX mixer group into the AudioSource's 'Output' field so volume settings apply.", this);
    }

    [Inject]
    private void Construct(ParticipantScoreService scoreService = null, IGameplayStateReader gameplayStateReader = null)
    {
        _scoreService = scoreService;
        _gameplayStateReader = gameplayStateReader;
    }

    private void Update()
    {
        // Only tick during active gameplay.
        if (_gameplayStateReader == null || !_gameplayStateReader.IsGameplayActive)
        {
            _stillTimer = 0f;
            return;
        }

        if (_motor == null || _motor.IsDead)
        {
            _stillTimer = 0f;
            return;
        }

        bool isStill = _motor.CurrentVelocity.magnitude < _stillnessThreshold;

        if (!isStill)
        {
            // Any movement resets the streak.
            _stillTimer = 0f;
            return;
        }

        _stillTimer += Time.deltaTime;

        if (_stillTimer >= _stillnessInterval)
        {
            _stillTimer -= _stillnessInterval; // carry overflow so very long pauses can multi-trigger
            AwardBonus();
        }
    }

    private void AwardBonus()
    {
        _scoreService?.AddPoints(_collectiblesPerBonus);

        if (_bonusClip != null && _audioSource != null)
            _audioSource.PlayOneShot(_bonusClip, _baseVolume); // SFX volume handled by AudioMixer group
    }
}
}
