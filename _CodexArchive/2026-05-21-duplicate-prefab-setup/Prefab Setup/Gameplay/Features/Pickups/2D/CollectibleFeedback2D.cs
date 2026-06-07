using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Scoring;
using UnityEngine;
using VContainer;

namespace NeonBlack.Gameplay.Features.Pickups
{

/// <summary>
/// Central singleton for collectible audio and particle feedback in the 2D gameplay loop.
/// Called by collectible collect and hazard-removal paths.
///
/// SETUP:
///   1. Attach to a dedicated "FeedbackManager" GameObject in the Game scene (or the Spawners GO).
///   2. Assign audio clips and particle systems in the Inspector.
///   3. Create two world-space ParticleSystem GameObjects in the scene (set Stop Action = Disable):
///      - CollectFX  : sparkle / confetti burst
///      - DestroyFX  : small puff / smoke burst
///      Wire them into _collectFX and _destroyFX.
///      Both systems are repositioned to the collectible's world position before Play() is called.
/// </summary>
[AddComponentMenu("NeonBlack/Gameplay/Pickups/Collectible Feedback 2D")]
public class CollectibleFeedback2D : MonoBehaviour, IPickupAwardSink
{
    public static CollectibleFeedback2D Instance { get; private set; }

    [Header("Collect Feedback")]
    [SerializeField, Tooltip("Sound played when the player collects a collectible.")]
    private AudioClip _collectClip;
    [SerializeField, Tooltip("Particle system burst played at the collectible's position on collection.")]
    private ParticleSystem _collectFX;

    [Header("Destroy Feedback")]
    [SerializeField, Tooltip("Sound played when a hazard destroys a collectible without the player collecting it.")]
    private AudioClip _destroyClip;
    [SerializeField, Tooltip("Particle system burst played at the collectible's position when destroyed by a hazard.")]
    private ParticleSystem _destroyFX;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField, Tooltip("Per-clip volume trim (0\u20131). Overall SFX level is controlled by the SFX mixer group \u2014 set this to fine-tune collect vs destroy relative loudness.")]
    private float _baseVolume = 1f;

    private AudioSource _audioSource;
    private ParticipantScoreService _scoreService;

    [Inject]
    private void Construct(ParticipantScoreService scoreService = null)
    {
        _scoreService = scoreService;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        // Cache a 2D (non-spatial) AudioSource so clips have equal volume everywhere on screen.
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.spatialBlend = 0f;
        _audioSource.playOnAwake  = false;

        // Validate mixer routing. Without an Output Group assigned, SettingsManager's
        // SFX volume slider has no effect on this component's clips.
        if (_audioSource.outputAudioMixerGroup == null)
            Debug.LogError("[CollectibleFeedback2D] AudioSource has no Output AudioMixerGroup assigned. "
                + "Drag your SFX mixer group into the AudioSource's 'Output' field so volume settings apply.", this);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>Play collect sound + particle burst at <paramref name="worldPos"/>.</summary>
    public void PlayCollect(Vector3 worldPos)
    {
        PlaySFX(_collectClip);

        if (_collectFX != null)
        {
            _collectFX.transform.position = worldPos;
            _collectFX.Play();
        }
    }

    /// <summary>Play destroy sound + particle burst at <paramref name="worldPos"/>.</summary>
    public void PlayDestroy(Vector3 worldPos)
    {
        PlaySFX(_destroyClip);

        if (_destroyFX != null)
        {
            _destroyFX.transform.position = worldPos;
            _destroyFX.Play();
        }
    }

    public void ApplyAward(in PickupAwardPayload payload)
    {
        switch (payload.Outcome)
        {
            case PickupAwardOutcome.Collected:
                ApplyCollectedAward(payload);
                break;
            case PickupAwardOutcome.DestroyedWithoutAward:
                PlayDestroy(payload.WorldPosition);
                break;
        }
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || _audioSource == null) return;
        // Overall SFX level is handled by the SFX AudioMixer group on this AudioSource's Output.
        _audioSource.PlayOneShot(clip, _baseVolume);
    }

    private void ApplyCollectedAward(in PickupAwardPayload payload)
    {
        if (_scoreService != null && payload.ScoreValue > 0)
        {
            _scoreService.AddPoints(payload.ScoreValue);
            if (payload.Collector != null
                && ParticipantQueryUtility.TryResolveParticipant(payload.Collector, out ParticipantHandle participant))
            {
                _scoreService.AddScore(participant, payload.ScoreValue);
            }
        }

        PlayCollect(payload.WorldPosition);
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        try { Handheld.Vibrate(); } catch { }
#endif
    }
}

} // namespace NeonBlack.Gameplay.Features.Pickups
