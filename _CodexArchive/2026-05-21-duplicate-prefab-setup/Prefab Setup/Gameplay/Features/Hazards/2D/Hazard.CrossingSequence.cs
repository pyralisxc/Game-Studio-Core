using System.Collections;
using NeonBlack.Gameplay.Presentation.Camera;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Hazards
{
public partial class Hazard
{
    // ---------------------------------------------------------------------
    // Crossing master sequence  (Crossing / Wavy)
    // ---------------------------------------------------------------------

    private IEnumerator CrossingSequenceRoutine(DifficultyManager.HazardTiming timing)
    {
        DisableAllColliders();
        SetShadowAlpha(0f);
        SetOutlineActive(false);
        transform.position = CrossingStart;

        // Show the shadow sprite at the entry edge so players can see what�s coming before it launches.
        SetShadowSprite(_data.shadowSprite);
        SetShadowAlpha(_shadowAlpha);

        ShowLaneRenderer();
        SetOutlineActive(true);
        SetOutlineSprite(_data.shadowSprite, _data.outlineColor);

        float warningTime = timing.warningFlashDuration > 0.05f
            ? timing.warningFlashDuration : _data.crossingWarningDuration;

        // Throttle outline alpha to ~20fps � same reason as SlamWarningRoutine.
        const float crossingAlphaInterval = 0.05f;
        float elapsed = 0f;
        float crossingAlphaTimer = 0f;
        while (elapsed < warningTime)
        {
            elapsed            += Time.deltaTime;
            crossingAlphaTimer += Time.deltaTime;
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
        if (_data.enableScreenShake)
            CameraShaker.Instance?.Shake(_data.shakeDuration, _data.shakeMagnitude);
        PlaySFX(_data.crossingEntryClip);

        // Start looped travel audio.
        StartTravelLoop(_data.crossingTravelClip);

        if (_data.rotatesToFaceDirection)
        {
            Vector2 d = (CrossingEnd - CrossingStart).normalized;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
        }

        if (_data.entryDelay > 0.01f)
            yield return new WaitForSeconds(_data.entryDelay);

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
                yield return new WaitForSeconds(_data.hitLingerDuration);
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
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / travelTime);

            if (_data.enableTargeting) SteerTowardPlayer(ref dir);

            float speedMult  = _data.speedCurve.Evaluate(t);
            Vector2 movement = dir * speed * speedMult * Time.deltaTime;

            if (_data.enableWavyPath)
            {
                Vector2 perp = new Vector2(-dir.y, dir.x);
                movement += perp * Mathf.Cos(elapsed * _data.waveFrequency * Mathf.PI * 2f)
                                 * _data.waveAmplitude * _data.waveFrequency * Mathf.PI * 2f
                                 * Time.deltaTime;
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
                    ? speed * speedMult * Time.deltaTime
                    : Time.deltaTime;
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
            Debug.LogWarning($"[Hazard] '{name}' TravelCrossing hit the {maxTravelTime:F1}s safety cap � check moveSpeed, enableTargeting, or crossingAxis setup.", this);
        transform.localScale = baseScale;
    }


    // ---------------------------------------------------------------------
    // Lane renderer
    // ---------------------------------------------------------------------

    private void ShowLaneRenderer()
    {
        if (_laneRenderer == null)
        {
            Debug.LogWarning($"[Hazard] '{name}' ShowLaneRenderer: _laneRenderer is null � lane will not show.", this);
            return;
        }
        if (_laneRenderer.sprite == null)
        {
            Debug.LogWarning($"[Hazard] '{name}' ShowLaneRenderer: _laneRenderer.sprite is null � assign a sprite to the LaneSprite SpriteRenderer in the prefab.", this);
            return;
        }

        Camera cam = CameraAspectController.Main != null ? CameraAspectController.Main : Camera.main;
        if (cam == null)
        {
            Debug.LogWarning($"[Hazard] '{name}' ShowLaneRenderer: no camera found.", this);
            return;
        }

        bool isHorizontal = _data.crossingAxis == HazardData.CrossingAxis.Horizontal;
        bool isDiagonal   = _data.crossingAxis == HazardData.CrossingAxis.Diagonal;

        float screenW  = cam.orthographicSize * cam.aspect * 2f + 2f;
        float screenH  = cam.orthographicSize * 2f + 2f;
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
            finalPos   = new Vector3(cam.transform.position.x, cam.transform.position.y, transform.position.z);
            finalScale = new Vector3(diag / native.x / px, hitSz.y / native.y / py, 1f);
            _laneRenderer.transform.position  = finalPos;
            _laneRenderer.transform.localScale = finalScale;
            Vector2 d = (CrossingEnd - CrossingStart).normalized;
            _laneRenderer.transform.rotation   = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
        }
        else if (isHorizontal)
        {
            finalPos   = new Vector3(cam.transform.position.x, CrossingStart.y, transform.position.z);
            finalScale = new Vector3(screenW / native.x / px, hitSz.y / native.y / py, 1f);
            _laneRenderer.transform.position   = finalPos;
            _laneRenderer.transform.rotation   = Quaternion.identity;
            _laneRenderer.transform.localScale = finalScale;
        }
        else // Vertical
        {
            finalPos   = new Vector3(CrossingStart.x, cam.transform.position.y, transform.position.z);
            finalScale = new Vector3(hitSz.x / native.x / px, screenH / native.y / py, 1f);
            _laneRenderer.transform.position   = finalPos;
            _laneRenderer.transform.rotation   = Quaternion.identity;
            _laneRenderer.transform.localScale = finalScale;
        }

        // Lane color is read directly from the SpriteRenderer's vertex color set in the Inspector.
        // Do not override it here � configure it per-prefab on the LaneSprite child.

        if (_shadowRenderer != null)
        {
            _laneRenderer.sortingLayerName = _shadowRenderer.sortingLayerName;
            _laneRenderer.sortingOrder     = _shadowRenderer.sortingOrder - 1;
        }

        _laneRenderer.gameObject.SetActive(true);
    }

    private void HideLaneRenderer() { if (_laneRenderer != null) _laneRenderer.gameObject.SetActive(false); }

}
}
