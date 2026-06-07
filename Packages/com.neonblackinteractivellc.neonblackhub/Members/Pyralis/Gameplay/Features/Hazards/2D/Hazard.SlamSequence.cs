using System.Collections;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Hazards
{
public partial class Hazard
{
    // ---------------------------------------------------------------------
    // Slam sequence
    // ---------------------------------------------------------------------

    /// <summary>
    /// Shared approach + warning-flash phase used by both Slam and Bouncy hazards.
    /// Handles the shadow drift, outline pulse, and optional targeting drift during warning.
    /// </summary>
    private IEnumerator SlamWarningRoutine(DifficultyManager.HazardTiming timing)
    {
        DisableAllColliders();
        SetShadowSprite(_data.shadowSprite);
        SetShadowAlpha(_shadowAlpha);
        SetOutlineActive(false);

        float shadowDur = Mathf.Max(0.05f, timing.shadowDuration);
        if (_data.enableTargeting)
        {
            yield return DriftTowardPlayer(shadowDur);
        }
        else
        {
            _cachedWait = new WaitForSeconds(shadowDur);
            yield return _cachedWait;
        }

        SetShadowAlpha(_warningAlpha);
        SetOutlineActive(true);
        SetOutlineSprite(_data.shadowSprite, _data.outlineColor);

        // Outline alpha throttle: update ~20x/sec instead of every frame.
        // Each SpriteRenderer.color write dirties the renderer and breaks sprite
        // batching - on mobile with 6+ hazards this is a significant cost per frame.
        const float outlineAlphaInterval = 0.05f;
        float warnDur     = Mathf.Max(0.05f, timing.warningFlashDuration);
        float warnElapsed = 0f;
        float driftSpeed  = _data.trackingStrength * _data.moveSpeed;
        while (warnElapsed < warnDur)
        {
            warnElapsed        += Time.deltaTime;
            _outlineAlphaTimer += Time.deltaTime;
            if (_outlineAlphaTimer >= outlineAlphaInterval)
            {
                _outlineAlphaTimer = 0f;
                SetOutlineAlpha(Mathf.Abs(Mathf.Sin(warnElapsed * Mathf.PI * _data.warningPulseRate)));
            }
            if (_data.enableTargeting && Player != null && Player.gameObject.activeInHierarchy
                && Vector2.Distance(transform.position, Player.position) > _data.lockOnRadius)
            {
                Vector2 toPlayer = ((Vector2)Player.position - (Vector2)transform.position).normalized;
                transform.position = (Vector2)transform.position + toPlayer * driftSpeed * Time.deltaTime;
            }
            yield return null;
        }

        SetOutlineActive(false);
    }

    /// <summary>
    /// Plays a one-shot clip through the hazard's cached 2D AudioSource (spatialBlend = 0).
    /// Volume is always equal across the screen - correct for an orthographic 2D game.
    /// </summary>
    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || _audioSource == null) return;
        float sfxVol = ResolveSfxVolume();
        _audioSource.PlayOneShot(clip, _data.audioVolume * sfxVol);
    }

    private void StartTravelLoop(AudioClip clip)
    {
        if (clip == null || _audioSource == null) return;
        float sfxVol = ResolveSfxVolume();
        _audioSource.clip   = clip;
        _audioSource.loop   = true;
        _audioSource.volume = _data.audioVolume * sfxVol;
        _audioSource.Play();
    }

    private void StopTravelLoop()
    {
        if (_audioSource == null) return;
        _audioSource.Stop();
        _audioSource.loop = false;
        _audioSource.clip = null;
    }

    private IEnumerator SlamSequenceRoutine(DifficultyManager.HazardTiming timing)
    {
        yield return SlamWarningRoutine(timing);

        SetShadowSprite(_data.fullyFormedSprite);
        ApplyActiveTint();
        SetShadowAlpha(1f);
        EnableHitColliders();
        HandleCollectiblesOnActivate(transform.position);
        _feedbackRuntime?.PlayActivationFeedback();

        // Screen shake + audio on slam impact
        PlayScreenShake();
        PlaySFX(_data.slamImpactClip);

        // OnImpact: fires immediately at slam activation (before the active-phase wait)
        if (_data.enableExplosion && _data.explosionTrigger == HazardData.ExplosionTrigger.OnImpact)
            yield return TriggerExplosionEffect();

        float slam = timing.slamDuration > 0f ? timing.slamDuration : _data.slamDuration;
        yield return new WaitForSeconds(slam);

        DisableAllColliders();

        // OnExit: fires after the slam active phase ends, before retract
        if (_data.enableExplosion && _data.explosionTrigger == HazardData.ExplosionTrigger.OnExit)
            yield return TriggerExplosionEffect();

        float retract = timing.retractDuration > 0f ? timing.retractDuration : _data.retractDuration;
        yield return FadeOutRoutine(retract);
        _feedbackRuntime?.PlayExitFeedback();

        ReturnToPool();
    }
}
}
