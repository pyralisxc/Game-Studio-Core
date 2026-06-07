using System.Collections;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Hazards
{
public partial class Hazard
{
    // ---------------------------------------------------------------------
    // Bouncy sequence  (slam-style approach + warning ? distance-based segment travel)
    // ---------------------------------------------------------------------

    private IEnumerator BouncySequenceRoutine(DifficultyManager.HazardTiming timing)
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
            yield return new WaitForSeconds(_data.hitLingerDuration);

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
                totalElapsed += Time.deltaTime;
                if (totalElapsed >= maxTime) { hitTimeLimit = true; break; }

                if (_data.enableTargeting) SteerTowardPlayer(ref dir);

                float curveTBouncy = Mathf.Clamp01(1f - (float)hopsLeft / Mathf.Max(1, _data.bounceCount));
                float speedMult    = _data.speedCurve.Evaluate(curveTBouncy);
                float step         = speed * speedMult * Time.deltaTime;

                Vector2 wavyDelta = Vector2.zero;
                if (_data.enableWavyPath)
                {
                    Vector2 perp = new Vector2(-dir.y, dir.x);
                    wavyDelta    = perp * Mathf.Cos(totalElapsed * _data.waveFrequency * Mathf.PI * 2f)
                                   * _data.waveAmplitude * _data.waveFrequency * Mathf.PI * 2f
                                   * Time.deltaTime;
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
