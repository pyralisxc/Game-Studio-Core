using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Scoring;
using UnityEngine;
using VContainer;

namespace NeonBlack.Gameplay.Features.Pickups
{

/// <summary>
/// Scene-authored pickup award sink for collectible audio, particles, and score awards.
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
[AuthoringContract(
    Capability = AuthoringCapability.Audio | AuthoringCapability.VFX,
    Relevance = "Manages audio and visual feedback (particles/sounds) for collectible actions.",
    NativeSetup = new[] 
    { 
        "Attach to a FeedbackManager or Spawner GameObject.",
        "Assign AudioClips and ParticleSystems.",
        "Ensure AudioSource is routed to the SFX mixer group."
    },
    AssignmentFields = new[] { nameof(_collectClip), nameof(_collectFX), nameof(_destroyClip), nameof(_destroyFX), nameof(_scoreAwardSource) },
    FirstProof = "Collect a pickup and verify the sparkle particles play and the collection sound triggers.",
    ExpertAdvice = "Set SFX spatial blend to 0 (2D) for consistent UI-style feedback. Ensure Particle Systems have 'Stop Action' set to 'Disable' or 'None' for pooling."
)]
[AddComponentMenu("NeonBlack/Gameplay/Pickups/Collectible Feedback 2D")]
public class CollectibleFeedback2D : MonoBehaviour, IPickupAwardSink, IRuntimeValidationProvider
{
    public IEnumerable<string> GetRuntimeValidationIssues()
    {
        if (_collectClip == null) yield return "Collect Clip is unassigned.";
        if (_collectFX == null) yield return "Collect FX particle system is unassigned.";
        if (_scoreAwardSource == null) yield return "Score Award Source is unassigned. No points will be awarded.";

        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null && audio.outputAudioMixerGroup == null)
            yield return "AudioSource is missing an Output Mixer Group. Volume settings will not apply.";
    }
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

    [Header("Runtime Services")]
    [SerializeField, Tooltip("Score award target used when collected pickups should add points. ParticipantScoreService implements ISessionScoreAwardSink, or assign a custom score/resource service.")]
    private MonoBehaviour _scoreAwardSource;

    private AudioSource _audioSource;
    private ISessionScoreAwardSink _scoreAwardSink;
    private ParticipantScoreService _participantScoreService;
    private bool _loggedMissingScoreAwardSink;

    [Inject]
    private void Construct(ISessionScoreAwardSink scoreAwardSink = null, ParticipantScoreService participantScoreService = null)
    {
        if (scoreAwardSink != null)
            _scoreAwardSink = scoreAwardSink;
        if (participantScoreService != null)
        {
            _participantScoreService = participantScoreService;
            _scoreAwardSink ??= participantScoreService;
        }
    }

    private void Awake()
    {
        // Cache a 2D (non-spatial) AudioSource so clips have equal volume everywhere on screen.
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.spatialBlend = 0f;
        _audioSource.playOnAwake  = false;

        // Validate mixer routing only when this component is actually playing clips.
        // Score-only pickup proofs should not need audio mixer setup.
        if ((_collectClip != null || _destroyClip != null) && _audioSource.outputAudioMixerGroup == null)
            Debug.LogError("[CollectibleFeedback2D] AudioSource has no Output AudioMixerGroup assigned. "
                + "Drag your SFX mixer group into the AudioSource's 'Output' field so volume settings apply.", this);

        ResolveScoreAwardSink();
    }

    public void ConfigureRuntime(ISessionScoreAwardSink scoreAwardSink)
    {
        if (scoreAwardSink != null)
        {
            _scoreAwardSink = scoreAwardSink;
            _participantScoreService ??= scoreAwardSink as ParticipantScoreService;
        }
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
        ISessionScoreAwardSink scoreAwardSink = ResolveScoreAwardSink();
        if (payload.ScoreValue > 0)
        {
            if (scoreAwardSink != null)
                scoreAwardSink.AddPoints(payload.ScoreValue);
            else
                LogMissingScoreAwardSink();

            ParticipantScoreService participantScoreService = ResolveParticipantScoreService(scoreAwardSink);
            if (payload.Collector != null
                && ParticipantQueryUtility.TryResolveParticipant(payload.Collector, out ParticipantHandle participant))
            {
                participantScoreService?.AddScore(participant, payload.ScoreValue);
            }
        }

        PlayCollect(payload.WorldPosition);
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        try { Handheld.Vibrate(); } catch { }
#endif
    }

    private ISessionScoreAwardSink ResolveScoreAwardSink()
    {
        if (_scoreAwardSink != null)
            return _scoreAwardSink;

        if (_scoreAwardSource != null)
        {
            _scoreAwardSink = _scoreAwardSource as ISessionScoreAwardSink;
            if (_scoreAwardSink == null)
                _scoreAwardSink = _scoreAwardSource.GetComponent<ISessionScoreAwardSink>();

            _participantScoreService ??= _scoreAwardSource as ParticipantScoreService;
            if (_participantScoreService == null)
                _participantScoreService = _scoreAwardSource.GetComponent<ParticipantScoreService>();
        }

        if (_scoreAwardSink == null)
        {
            _participantScoreService ??= GetComponent<ParticipantScoreService>();
            _scoreAwardSink = _participantScoreService;
        }

        return _scoreAwardSink;
    }

    private ParticipantScoreService ResolveParticipantScoreService(ISessionScoreAwardSink scoreAwardSink)
    {
        if (_participantScoreService != null)
            return _participantScoreService;

        _participantScoreService = scoreAwardSink as ParticipantScoreService;
        if (_participantScoreService == null && _scoreAwardSource != null)
            _participantScoreService = _scoreAwardSource.GetComponent<ParticipantScoreService>();

        return _participantScoreService;
    }

    private void LogMissingScoreAwardSink()
    {
        if (_loggedMissingScoreAwardSink)
            return;

        _loggedMissingScoreAwardSink = true;
        Debug.LogError("[CollectibleFeedback2D] Score Award Source is not configured. Assign ParticipantScoreService or another ISessionScoreAwardSink when collected pickups should add points.", this);
    }
}

} // namespace NeonBlack.Gameplay.Features.Pickups
