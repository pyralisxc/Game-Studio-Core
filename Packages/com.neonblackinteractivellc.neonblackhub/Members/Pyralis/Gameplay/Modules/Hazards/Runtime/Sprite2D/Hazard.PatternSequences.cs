using System.Collections;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Hazards
{
public partial class Hazard
{
    private interface IHazardPatternRunner
    {
        IEnumerator Execute(Hazard hazard, HazardDifficultyController.HazardTiming timing);
    }

    private static readonly IHazardPatternRunner SlamPatternRunner = new SlamHazardPatternRunner();
    private static readonly IHazardPatternRunner CrossingPatternRunner = new CrossingHazardPatternRunner();
    private static readonly IHazardPatternRunner BouncyPatternRunner = new BouncyHazardPatternRunner();

    private static IHazardPatternRunner GetPatternRunner(HazardData.HazardType hazardType)
    {
        switch (hazardType)
        {
            case HazardData.HazardType.Crossing:
                return CrossingPatternRunner;
            case HazardData.HazardType.Bouncy:
                return BouncyPatternRunner;
            default:
                return SlamPatternRunner;
        }
    }

    private sealed class SlamHazardPatternRunner : IHazardPatternRunner
    {
        public IEnumerator Execute(Hazard hazard, HazardDifficultyController.HazardTiming timing)
        {
            return hazard.SlamSequenceRoutine(timing);
        }
    }

    private sealed class CrossingHazardPatternRunner : IHazardPatternRunner
    {
        public IEnumerator Execute(Hazard hazard, HazardDifficultyController.HazardTiming timing)
        {
            return hazard.CrossingSequenceRoutine(timing);
        }
    }

    private sealed class BouncyHazardPatternRunner : IHazardPatternRunner
    {
        public IEnumerator Execute(Hazard hazard, HazardDifficultyController.HazardTiming timing)
        {
            return hazard.BouncySequenceRoutine(timing);
        }
    }

    // ---------------------------------------------------------------------
    // Slam sequence
    // ---------------------------------------------------------------------

    /// <summary>
    /// Shared approach + warning-flash phase used by both Slam and Bouncy hazards.
    /// Handles the shadow drift, outline pulse, and optional targeting drift during warning.
    /// </summary>
    private IEnumerator SlamWarningRoutine(HazardDifficultyController.HazardTiming timing)
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
            yield return GetWait(shadowDur);
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
            warnElapsed        += GameplayDeltaTime;
            _outlineAlphaTimer += GameplayDeltaTime;
            if (_outlineAlphaTimer >= outlineAlphaInterval)
            {
                _outlineAlphaTimer = 0f;
                SetOutlineAlpha(Mathf.Abs(Mathf.Sin(warnElapsed * Mathf.PI * _data.warningPulseRate)));
            }
            if (_data.enableTargeting && Player != null && Player.gameObject.activeInHierarchy
                && Vector2.Distance(transform.position, Player.position) > _data.lockOnRadius)
            {
                Vector2 toPlayer = ((Vector2)Player.position - (Vector2)transform.position).normalized;
                transform.position = (Vector2)transform.position + toPlayer * driftSpeed * GameplayDeltaTime;
            }
            yield return null;
        }

        SetOutlineActive(false);
    }

    /// <summary>
    /// Plays a one-shot clip through the hazard's authored 2D AudioSource (spatialBlend = 0).
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

    private IEnumerator SlamSequenceRoutine(HazardDifficultyController.HazardTiming timing)
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
        yield return GetWait(slam);

        DisableAllColliders();

        // OnExit: fires after the slam active phase ends, before retract
        if (_data.enableExplosion && _data.explosionTrigger == HazardData.ExplosionTrigger.OnExit)
            yield return TriggerExplosionEffect();

        float retract = timing.retractDuration > 0f ? timing.retractDuration : _data.retractDuration;
        yield return FadeOutRoutine(retract);
        _feedbackRuntime?.PlayExitFeedback();

        ReturnToPool();
    }

    // ---------------------------------------------------------------------
    // Crossing master sequence  (Crossing / Wavy)
    // ---------------------------------------------------------------------

    private IEnumerator CrossingSequenceRoutine(HazardDifficultyController.HazardTiming timing)
    {
        DisableAllColliders();
        SetShadowAlpha(0f);
        SetOutlineActive(false);
        transform.position = CrossingStart;

        // Show the shadow sprite at the entry edge so players can see what's coming before it launches.
        SetShadowSprite(_data.shadowSprite);
        SetShadowAlpha(_shadowAlpha);

        ShowLaneRenderer();
        SetOutlineActive(true);
        SetOutlineSprite(_data.shadowSprite, _data.outlineColor);

        float warningTime = timing.warningFlashDuration > 0.05f
            ? timing.warningFlashDuration : _data.crossingWarningDuration;

        // Throttle outline alpha to ~20fps - same reason as SlamWarningRoutine.
        const float crossingAlphaInterval = 0.05f;
        float elapsed = 0f;
        float crossingAlphaTimer = 0f;
        while (elapsed < warningTime)
        {
            elapsed            += GameplayDeltaTime;
            crossingAlphaTimer += GameplayDeltaTime;
            if (crossingAlphaTimer >= crossingAlphaInterval)
            {
                crossingAlphaTimer = 0f;
                SetOutlineAlpha(Mathf.Abs(Mathf.Sin(elapsed * Mathf.PI * _data.warningPulseRate)));
            }
            yield return null;
        }

        SetOutlineActive(false);
        HideLaneRenderer();
        SetShadowSprite(_data.fullyFormedSprite);
        ApplyActiveTint();
        SetShadowAlpha(1f);
        EnableHitColliders();
        _feedbackRuntime?.PlayActivationFeedback();

        // Entry: screen shake + one-shot audio as it launches.
        PlayScreenShake();
        PlaySFX(_data.crossingEntryClip);

        // Start looped travel audio.
        StartTravelLoop(_data.crossingTravelClip);

        if (_data.rotatesToFaceDirection)
        {
            Vector2 d = (CrossingEnd - CrossingStart).normalized;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
        }

        if (_data.entryDelay > 0.01f)
            yield return GetWait(_data.entryDelay);

        _explosionTriggered = false;
        _pendingImpactExplosion = false;

        yield return TravelCrossing();

        // Stop travel loop and play exit one-shot.
        StopTravelLoop();
        PlaySFX(_data.crossingExitClip);

        if (!_explosionTriggered)
        {
            if (_data.enableExplosion && _data.explosionTrigger == HazardData.ExplosionTrigger.OnExit)
                yield return TriggerExplosionEffect();
            if (_data.hitLingerDuration > 0.01f)
                yield return GetWait(_data.hitLingerDuration);
            if (_data.spawnsCollectibles)
            {
                SpawnCollectiblesAt(transform.position, _data.collectibleSpawnCount, 0.5f);
                _feedbackRuntime?.PlayCollectibleFeedback(_data.collectibleSpawnCount);
            }
        }

        _feedbackRuntime?.PlayExitFeedback();
        ReturnToPool();
    }

    // ---------------------------------------------------------------------
    // Travel implementations
    // ---------------------------------------------------------------------

    /// <summary>
    /// Moves the hazard from CrossingStart toward CrossingEnd.
    /// Handles straight travel, wavy oscillation (enableWavyPath), speed curve, targeting,
    /// jump-scale variant, collectible sweeping, and explosion triggers in a single loop.
    /// Replaces the former TravelStraight + TravelWavy pair.
    /// </summary>
    private IEnumerator TravelCrossing()
    {
        float speed      = Mathf.Max(0.1f, _data.moveSpeed);
        float total      = Vector2.Distance(CrossingStart, CrossingEnd);
        float travelTime = total / speed;
        float elapsed    = 0f;
        Vector2 dir      = (CrossingEnd - CrossingStart).normalized;
        Vector3 baseScale = transform.localScale;
        float crumbAccum  = 0f; // accumulates distance (PerDistance) or time (PerSecond) for collectible spawning

        float maxTravelTime = travelTime * 3f;
        while (elapsed < maxTravelTime)
        {
            elapsed += GameplayDeltaTime;
            float t = Mathf.Clamp01(elapsed / travelTime);

            if (_data.enableTargeting) SteerTowardPlayer(ref dir);

            float speedMult  = _data.speedCurve.Evaluate(t);
            Vector2 movement = dir * speed * speedMult * GameplayDeltaTime;

            if (_data.enableWavyPath)
            {
                Vector2 perp = new Vector2(-dir.y, dir.x);
                movement += perp * Mathf.Cos(elapsed * _data.waveFrequency * Mathf.PI * 2f)
                                 * _data.waveAmplitude * _data.waveFrequency * Mathf.PI * 2f
                                 * GameplayDeltaTime;
            }

            transform.position = (Vector2)transform.position + movement;

            if (_data.rotatesToFaceDirection)
                transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

            if (_data.crossingVariant == HazardData.CrossingVariant.Jump)
                ApplyJumpScale(t, baseScale);

            if (_data.destroysNearbyCollectibles) TryTravelCollectibleSweep();

            // Travel collectible spawning: accumulate distance or time and burst on threshold.
            if (_data.crossingCollectibleMode != HazardData.CrossingCollectibleMode.None)
            {
                crumbAccum += _data.crossingCollectibleMode == HazardData.CrossingCollectibleMode.PerDistance
                    ? speed * speedMult * GameplayDeltaTime
                    : GameplayDeltaTime;
                if (crumbAccum >= _data.collectibleSpawnInterval)
                {
                    crumbAccum -= _data.collectibleSpawnInterval;
                    SpawnCollectiblesAt(transform.position, _data.collectibleSpawnCount, 0.5f);
                    _feedbackRuntime?.PlayCollectibleFeedback(_data.collectibleSpawnCount);
                }
            }

            if (_data.enableExplosion && CheckExplosionTriggers(elapsed))
            { yield return TriggerExplosionEffect(); _explosionTriggered = true; yield break; }

            if (Vector2.Distance(transform.position, CrossingEnd) < 0.2f) break;

            yield return null;
        }
        if (elapsed >= maxTravelTime)
            Debug.LogWarning($"[Hazard] '{name}' TravelCrossing hit the {maxTravelTime:F1}s safety cap - check moveSpeed, enableTargeting, or crossingAxis setup.", this);
        transform.localScale = baseScale;
    }


    // ---------------------------------------------------------------------
    // Lane renderer
    // ---------------------------------------------------------------------

    private void ShowLaneRenderer()
    {
        if (_laneRenderer == null)
        {
            Debug.LogWarning($"[Hazard] '{name}' ShowLaneRenderer: _laneRenderer is null - lane will not show.", this);
            return;
        }
        if (_laneRenderer.sprite == null)
        {
            Debug.LogWarning($"[Hazard] '{name}' ShowLaneRenderer: _laneRenderer.sprite is null - assign a sprite to the LaneSprite SpriteRenderer in the prefab.", this);
            return;
        }

        if (_cameraBoundsProvider == null || !_cameraBoundsProvider.TryGetCameraBounds2D(-1f, out CameraBounds2D bounds))
        {
            Debug.LogWarning($"[Hazard] '{name}' ShowLaneRenderer: no camera bounds provider configured.", this);
            return;
        }

        bool isHorizontal = _data.crossingAxis == HazardData.CrossingAxis.Horizontal;
        bool isDiagonal   = _data.crossingAxis == HazardData.CrossingAxis.Diagonal;

        float screenW  = bounds.HalfWidth * 2f;
        float screenH  = bounds.HalfHeight * 2f;
        Vector2 hitSz  = _cachedHitSz;

        Vector2 native = _laneRenderer.sprite.bounds.size;
        native.x = Mathf.Max(native.x, 0.001f);
        native.y = Mathf.Max(native.y, 0.001f);

        Vector3 ps = _laneRenderer.transform.parent != null
            ? _laneRenderer.transform.parent.lossyScale : Vector3.one;
        float px = Mathf.Abs(ps.x) > 0.001f ? Mathf.Abs(ps.x) : 1f;
        float py = Mathf.Abs(ps.y) > 0.001f ? Mathf.Abs(ps.y) : 1f;

        Vector3 finalScale;
        Vector3 finalPos;

        if (isDiagonal)
        {
            float diag = Mathf.Sqrt(screenW * screenW + screenH * screenH);
            finalPos   = new Vector3(bounds.Center.x, bounds.Center.y, transform.position.z);
            finalScale = new Vector3(diag / native.x / px, hitSz.y / native.y / py, 1f);
            _laneRenderer.transform.position  = finalPos;
            _laneRenderer.transform.localScale = finalScale;
            Vector2 d = (CrossingEnd - CrossingStart).normalized;
            _laneRenderer.transform.rotation   = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
        }
        else if (isHorizontal)
        {
            finalPos   = new Vector3(bounds.Center.x, CrossingStart.y, transform.position.z);
            finalScale = new Vector3(screenW / native.x / px, hitSz.y / native.y / py, 1f);
            _laneRenderer.transform.position   = finalPos;
            _laneRenderer.transform.rotation   = Quaternion.identity;
            _laneRenderer.transform.localScale = finalScale;
        }
        else // Vertical
        {
            finalPos   = new Vector3(CrossingStart.x, bounds.Center.y, transform.position.z);
            finalScale = new Vector3(hitSz.x / native.x / px, screenH / native.y / py, 1f);
            _laneRenderer.transform.position   = finalPos;
            _laneRenderer.transform.rotation   = Quaternion.identity;
            _laneRenderer.transform.localScale = finalScale;
        }

        // Lane color is read directly from the SpriteRenderer's vertex color set in the Inspector.
        // Do not override it here - configure it per-prefab on the LaneSprite child.

        if (_shadowRenderer != null)
        {
            _laneRenderer.sortingLayerName = _shadowRenderer.sortingLayerName;
            _laneRenderer.sortingOrder     = _shadowRenderer.sortingOrder - 1;
        }

        _laneRenderer.gameObject.SetActive(true);
    }

    private void HideLaneRenderer() { if (_laneRenderer != null) _laneRenderer.gameObject.SetActive(false); }


    // ---------------------------------------------------------------------
    // Bouncy sequence  (slam-style approach + warning ? distance-based segment travel)
    // ---------------------------------------------------------------------

    private IEnumerator BouncySequenceRoutine(HazardDifficultyController.HazardTiming timing)
    {
        yield return SlamWarningRoutine(timing);

        SetShadowSprite(_data.fullyFormedSprite);
        ApplyActiveTint();
        SetShadowAlpha(1f);
        EnableHitColliders();
        HandleCollectiblesOnActivate(transform.position);
        _feedbackRuntime?.PlayActivationFeedback();

        PlayScreenShake();
        PlaySFX(_data.slamImpactClip);

        yield return TravelBouncy();

        if (_data.hitLingerDuration > 0f)
            yield return GetWait(_data.hitLingerDuration);

        if (_data.enableExplosion && _data.explosionTrigger == HazardData.ExplosionTrigger.OnExit
            && !_explosionTriggered)
            yield return TriggerExplosionEffect();

        _feedbackRuntime?.PlayExitFeedback();
        ReturnToPool();
    }


    private IEnumerator TravelBouncy()
    {
        HazardData.BouncePatternType pattern = _data.PickBouncePattern();

        // Per-activation state for stateful patterns - randomised fresh each spawn.
        _zigzagFlipNext = Random.value < 0.5f; // which side the first zigzag turn goes
        _orbitClockwise = Random.value < 0.5f; // CW or CCW orbit for this activation

        // Use direction override if set by SpawnBounceChildren (split children),
        // otherwise pick the initial direction based on the chosen pattern.
        Vector2 dir = _bouncyDirOverride.HasValue
            ? _bouncyDirOverride.Value
            : PickInitialBouncyDirection(pattern);
        _bouncyDirOverride = null;

        Vector3 baseScale   = transform.localScale;
        float   speed       = Mathf.Max(0.1f, _data.moveSpeed);
        float   segDist     = Mathf.Max(0.1f, _data.bounceDistance);
        int     hopsLeft    = _data.bounceCount;

        const float maxTime  = 30f;
        float totalElapsed   = 0f;
        bool  hitTimeLimit   = false;
        bool  splitTriggered = false;

        while (hopsLeft >= 0 && !hitTimeLimit)
        {
            // -- Travel one bounce segment (bounceDistance world units) -----
            float segTraveled = 0f;
            while (segTraveled < segDist)
            {
                totalElapsed += GameplayDeltaTime;
                if (totalElapsed >= maxTime) { hitTimeLimit = true; break; }

                if (_data.enableTargeting) SteerTowardPlayer(ref dir);

                float curveTBouncy = Mathf.Clamp01(1f - (float)hopsLeft / Mathf.Max(1, _data.bounceCount));
                float speedMult    = _data.speedCurve.Evaluate(curveTBouncy);
                float step         = speed * speedMult * GameplayDeltaTime;

                Vector2 wavyDelta = Vector2.zero;
                if (_data.enableWavyPath)
                {
                    Vector2 perp = new Vector2(-dir.y, dir.x);
                    wavyDelta    = perp * Mathf.Cos(totalElapsed * _data.waveFrequency * Mathf.PI * 2f)
                                   * _data.waveAmplitude * _data.waveFrequency * Mathf.PI * 2f
                                   * GameplayDeltaTime;
                }

                transform.position = (Vector2)transform.position + dir * step + wavyDelta;
                segTraveled += step;

                // Sine-pulse scale: small ? big ? small across each segment (arc illusion)
                float segT    = Mathf.Clamp01(segTraveled / segDist);
                bool grounded = segT <= _data.groundedWindow || segT >= (1f - _data.groundedWindow);

                if (_data.bounceScalePeak > 1f)
                {
                    float scaleMult = 1f + (_data.bounceScalePeak - 1f) * Mathf.Sin(segT * Mathf.PI);
                    Vector3 newScale = baseScale * scaleMult;
                    // Only write localScale when the value actually changes - avoids
                    // dirtying the transform every frame when scaleMult is near 1.
                    if (newScale != transform.localScale)
                        transform.localScale = newScale;
                }

                // Ground-only hitbox: active only in the landing windows at each end of the segment.
                if (_data.hitOnlyWhenGrounded)
                {
                    if (grounded) EnableHitColliders(); else DisableAllColliders();
                }

                if (_data.rotatesToFaceDirection)
                    transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

                if (_data.crossingVariant == HazardData.CrossingVariant.Jump)
                    ApplyJumpScale(curveTBouncy, baseScale);

                // Crumb sweep: only destroy crumbs in landing windows when hitOnlyWhenGrounded is on.
                if (_data.destroysNearbyCollectibles && (!_data.hitOnlyWhenGrounded || grounded))
                    TryTravelCollectibleSweep();

                if (_data.enableExplosion && CheckExplosionTriggers(totalElapsed))
                { yield return TriggerExplosionEffect(); _explosionTriggered = true; yield break; }

                yield return null;
            }

            if (hitTimeLimit) break;

            // -- Direction change (bounce) ----------------------------------
            hopsLeft--;
            if (hopsLeft < 0) break;

            // Split on first direction change
            if (_data.splitOnFirstBounce && !splitTriggered && !_isSplitChild && _spawner != null)
            {
                splitTriggered = true;
                _spawner.SpawnBounceChildren(this, transform.position, dir);
                _explosionTriggered = true;
                yield break;
            }

            PlaySFX(_data.bounceClip);
            _feedbackRuntime?.PlayBounceFeedback();

            // Crumb spawn at bounce events.
            if (_data.bouncyCollectibleMode != HazardData.BouncyCollectibleMode.None)
            {
                if (_data.bouncyCollectibleMode == HazardData.BouncyCollectibleMode.OnEachBounce ||
                    (_data.bouncyCollectibleMode == HazardData.BouncyCollectibleMode.OnLastBounce && hopsLeft == 0))
                {
                    SpawnCollectiblesAt(transform.position, _data.collectibleSpawnCount, 0.5f);
                    _feedbackRuntime?.PlayCollectibleFeedback(_data.collectibleSpawnCount);
                }
            }

            if (_data.enableExplosion &&
                _data.explosionTrigger == HazardData.ExplosionTrigger.OnLastBounce &&
                hopsLeft == 0)
            { yield return TriggerExplosionEffect(); _explosionTriggered = true; yield break; }

            dir = PickNextBouncyDirection(pattern, dir);
        }

        if (hitTimeLimit)
            Debug.LogWarning($"[Hazard] '{name}' TravelBouncy hit the {maxTime}s safety cap - check bounceCount, bounceDistance, and moveSpeed.", this);
        transform.localScale = baseScale;
    }

    private Vector2 PickInitialBouncyDirection(HazardData.BouncePatternType pattern)
    {
        switch (pattern)
        {
            case HazardData.BouncePatternType.AimedAtPlayer:
                // Launch directly toward the player's current position.
                // NOTE: this only re-aims at bounce POINTS - enable Targeting for continuous homing.
                if (Player != null && Player.gameObject.activeInHierarchy)
                    return ((Vector2)Player.position - (Vector2)transform.position).normalized;
                return Random.insideUnitCircle.normalized;

            case HazardData.BouncePatternType.FleeFromPlayer:
                // Launch directly AWAY from the player.
                if (Player != null && Player.gameObject.activeInHierarchy)
                    return ((Vector2)transform.position - (Vector2)Player.position).normalized;
                return Random.insideUnitCircle.normalized;

            case HazardData.BouncePatternType.Diagonal:
                // One of the 4 cardinal 45 degrees diagonals.
                int d = Random.Range(0, 4);
                return new Vector2(d < 2 ? 1f : -1f, d % 2 == 0 ? 1f : -1f).normalized;

            case HazardData.BouncePatternType.Zigzag:
                // Start aimed at the player (or random), then zigzag left/right from there.
                if (Player != null && Player.gameObject.activeInHierarchy)
                    return ((Vector2)Player.position - (Vector2)transform.position).normalized;
                return Random.insideUnitCircle.normalized;

            case HazardData.BouncePatternType.Orbit:
                // Start perpendicular to the player direction - begins circling immediately.
                if (Player != null && Player.gameObject.activeInHierarchy)
                {
                    Vector2 toPlayer = ((Vector2)Player.position - (Vector2)transform.position).normalized;
                    // Perpendicular: rotate 90 degrees (CW or CCW based on _orbitClockwise chosen at TravelBouncy start)
                    return _orbitClockwise
                        ? new Vector2(toPlayer.y, -toPlayer.x)
                        : new Vector2(-toPlayer.y, toPlayer.x);
                }
                return Random.insideUnitCircle.normalized;

            default: // FullyRandom, Ricochet
                return Random.insideUnitCircle.normalized;
        }
    }

    private Vector2 PickNextBouncyDirection(HazardData.BouncePatternType pattern, Vector2 currentDir)
    {
        switch (pattern)
        {
            case HazardData.BouncePatternType.AimedAtPlayer:
                // Re-aim toward the player from the current bounce position.
                if (Player != null && Player.gameObject.activeInHierarchy)
                    return ((Vector2)Player.position - (Vector2)transform.position).normalized;
                return Random.insideUnitCircle.normalized;

            case HazardData.BouncePatternType.FleeFromPlayer:
                // Re-aim away from the player from the current bounce position.
                if (Player != null && Player.gameObject.activeInHierarchy)
                    return ((Vector2)transform.position - (Vector2)Player.position).normalized;
                return Random.insideUnitCircle.normalized;

            case HazardData.BouncePatternType.Diagonal:
                // Billiard-style: randomly flip one axis to change to another 45 degrees diagonal.
                return (Random.value < 0.5f
                    ? new Vector2(-currentDir.x,  currentDir.y)
                    : new Vector2( currentDir.x, -currentDir.y)).normalized;

            case HazardData.BouncePatternType.Ricochet:
                // Deflects ~90 degrees left or right of the current direction - clean wall-ricochet feel.
                // Random variation of +/-20 degrees around perpendicular keeps it slightly unpredictable.
                float ricAngle = Random.value < 0.5f
                    ? Random.Range(70f, 110f)   // ~left perpendicular
                    : Random.Range(250f, 290f);  // ~right perpendicular
                return (Quaternion.Euler(0f, 0f, ricAngle) * currentDir).normalized;

            case HazardData.BouncePatternType.Zigzag:
                // Alternates a hard left (120 degrees) or right (-120 degrees) turn each bounce.
                float zigAngle = _zigzagFlipNext ? 120f : -120f;
                _zigzagFlipNext = !_zigzagFlipNext; // flip for next bounce
                return (Quaternion.Euler(0f, 0f, zigAngle) * currentDir).normalized;

            case HazardData.BouncePatternType.Orbit:
                // Consistent 90 degrees clockwise or counterclockwise turn - circles the arena.
                float orbitAngle = _orbitClockwise ? -90f : 90f;
                return (Quaternion.Euler(0f, 0f, orbitAngle) * currentDir).normalized;

            default: // FullyRandom
                return Random.insideUnitCircle.normalized;
        }
    }
}
}
