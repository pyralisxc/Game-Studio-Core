using System.Collections;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Pickups;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Hazards
{
public partial class Hazard
{

    // ---------------------------------------------------------------------
    // Explosion effect  (shared modifier - triggered from any hazard type)
    // ---------------------------------------------------------------------

    private IEnumerator TriggerExplosionEffect()
    {
        DisableAllColliders();

        // Hide the main hazard sprite immediately - from this point on only the
        // explosion effect is visible. Without this, the original sprite and the
        // explosion child both render simultaneously for the full explosionDuration.
        SetShadowAlpha(0f);
        SetOutlineActive(false);

        if (_explosionEffect != null)
        {
            _explosionEffect.transform.localScale = Vector3.one * _data.explosionSpriteScale;
            _explosionEffect.SetActive(true);
            _feedbackRuntime?.PlayExplosionFeedback();

            // Shake + audio at detonation
            PlayScreenShake();
            PlaySFX(_data.explosionClip);

            yield return GetWait(_data.explosionDuration);
            _explosionEffect.SetActive(false);
        }
        else
        {
            // Config warning is already raised at Initialize() - just wait here so
            // the sequence timing stays correct even with a misconfigured prefab.
            yield return GetWait(_data.explosionDuration);
        }
    }


    // ---------------------------------------------------------------------
    // Targeting helpers
    // ---------------------------------------------------------------------

    /// <summary>
    /// Returns true if the active explosion trigger condition is satisfied.
    /// Caller is responsible for setting _explosionTriggered after acting on the result.
    /// </summary>
    private bool CheckExplosionTriggers(float elapsed)
    {
        if (_data.explosionTrigger == HazardData.ExplosionTrigger.OnImpact
            && _pendingImpactExplosion)
            return true;

        if (_data.explosionTrigger == HazardData.ExplosionTrigger.OnProximity
            && Player != null && Player.gameObject.activeInHierarchy
            && Vector2.Distance(transform.position, Player.position) <= _data.explosionProximityRadius)
            return true;

        if (_data.explosionTrigger == HazardData.ExplosionTrigger.OnTimeElapsed
            && elapsed >= _data.explosionTimeDelay)
            return true;

        return false;
    }

    private void SteerTowardPlayer(ref Vector2 currentDir)
    {
        if (Player == null || !Player.gameObject.activeInHierarchy) return;
        if (Vector2.Distance(transform.position, Player.position) <= _data.lockOnRadius) return;
        Vector2 toPlayer    = ((Vector2)Player.position - (Vector2)transform.position).normalized;
        float currentAngle  = Mathf.Atan2(currentDir.y, currentDir.x) * Mathf.Rad2Deg;
        float targetAngle   = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
        float maxDeg        = _data.trackingStrength * 720f * Time.deltaTime;
        float newAngle      = Mathf.MoveTowardsAngle(currentAngle, targetAngle, maxDeg);
        currentDir = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad));
    }

    /// <summary>Coroutine that slowly moves the hazard toward the player (slam-style).</summary>
    private IEnumerator DriftTowardPlayer(float duration)
    {
        float elapsed = 0f;
        float driftSpeed = _data.trackingStrength * _data.moveSpeed;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (Player != null && Player.gameObject.activeInHierarchy
                && Vector2.Distance(transform.position, Player.position) > _data.lockOnRadius)
            {
                Vector2 toPlayer = ((Vector2)Player.position - (Vector2)transform.position).normalized;
                transform.position = (Vector2)transform.position + toPlayer * driftSpeed * Time.deltaTime;
            }
            yield return null;
        }
    }

    // ---------------------------------------------------------------------
    // Jump scale
    // ---------------------------------------------------------------------

    private void ApplyJumpScale(float t, Vector3 baseScale)
    {
        float hopT   = Mathf.PingPong(t * _data.jumpCount, 1f);
        float scaleT = Mathf.Sin(hopT * Mathf.PI);
        float s = scaleT > 0.5f
            ? Mathf.Lerp(1f, _data.jumpPeakScale, (scaleT - 0.5f) * 2f)
            : Mathf.Lerp(_data.jumpLandScale, 1f, scaleT * 2f);
        transform.localScale = baseScale * s;
    }

    // ---------------------------------------------------------------------
    // Shared helpers
    // ---------------------------------------------------------------------

    private void PlayScreenShake()
    {
        if (!_data.enableScreenShake)
            return;

        ResolveCameraShakeSink()?.Shake(
            _data.shakeMagnitude,
            _data.shakeDuration);
    }

    // Wrapper kept for any future callers, but now uses a pooled instance to avoid allocation.
    private WaitForSeconds WaitSec(float seconds)
    {
        return GetWait(seconds);
    }

    private IEnumerator FadeOutRoutine(float duration)
    {
        if (duration <= 0f) { SetShadowAlpha(0f); yield break; }
        // Throttle alpha writes to ~20fps - SpriteRenderer.color is a GPU dirty flag.
        // Writing it every frame at 60fps per hazard fragments the sprite batch.
        const float fadeAlphaInterval = 0.05f;
        float elapsed    = 0f;
        float fadeTimer  = 0f;
        float startAlpha = _shadowRenderer != null ? _shadowRenderer.color.a : 1f;
        while (elapsed < duration)
        {
            elapsed   += Time.deltaTime;
            fadeTimer += Time.deltaTime;
            if (fadeTimer >= fadeAlphaInterval)
            {
                fadeTimer = 0f;
                SetShadowAlpha(Mathf.Lerp(startAlpha, 0f, elapsed / duration));
            }
            yield return null;
        }
        SetShadowAlpha(0f);
    }

    private void HandleCollectiblesOnActivate(Vector2 pos)
    {
        int removed = 0;
        if (_data.destroysNearbyCollectibles) removed = DestroyCollectiblesInRadius();
        if (removed > 0)
            _feedbackRuntime?.PlayCollectibleFeedback(removed);
        if (_data.spawnsCollectibles)
        {
            SpawnCollectiblesAt(pos, _data.collectibleSpawnCount, 0.5f);
            _feedbackRuntime?.PlayCollectibleFeedback(_data.collectibleSpawnCount);
        }
    }

    // Throttled wrapper used during travel loops - limits the Physics2D.OverlapCircle
    // call to once every 0.2s instead of every frame. At 60fps with 6+ hazards that
    // was 360+ overlap queries/second, causing the lag spike at high difficulty.
    private const float CollectibleSweepInterval = 0.2f;
    private void TryTravelCollectibleSweep()
    {
        _collectibleSweepTimer += Time.deltaTime;
        if (_collectibleSweepTimer < CollectibleSweepInterval) return;
        _collectibleSweepTimer = 0f;
        DestroyCollectiblesInRadius();
    }

    private int DestroyCollectiblesInRadius()
    {
        if (_collectibleLayer.value == 0)
        {
            LogMissingCollectibleLayer();
            return 0;
        }

        Vector2 sz   = GetPrimaryHitColliderSize();
        float radius = Mathf.Max(sz.x, sz.y) * 0.5f * _data.collectibleDestroyRadiusScale;
        var filter = new ContactFilter2D { useTriggers = true };
        filter.SetLayerMask(_collectibleLayer);
        int count = Physics2D.OverlapCircle(transform.position, radius, filter, _collectibleBuffer);
        if (count == _collectibleBuffer.Length)
            Debug.LogWarning($"[Hazard] '{name}': collectible buffer full ({_collectibleBuffer.Length} colliders). " +
                             "Some collectibles in range may have been missed. " +
                             "Increase the _collectibleBuffer array size in Hazard.cs or reduce collectible density.");
        int removed = 0;
        for (int i = 0; i < count; i++)
        {
            Collectible2D collectible = _collectibleBuffer[i].GetComponent<Collectible2D>();
            if (collectible != null)
            {
                if (collectible.RemoveWithoutScore())
                    removed++;
            }
        }

        return removed;
    }

    private void LogMissingCollectibleLayer()
    {
        if (_collectibleLayerWarningLogged)
            return;

        _collectibleLayerWarningLogged = true;
        Debug.LogWarning($"[Hazard] '{name}': HazardData '{_data.hazardName}' destroys nearby collectibles, but Collectible Layer is set to Nothing. Assign the collectible physics layer on the Hazard prefab or disable collectible destruction.", this);
    }

    private void SpawnCollectiblesAt(Vector2 position, int count, float radius)
    {
        _pickupBurstSpawnSurface?.SpawnCollectiblesAt(position, count, radius);
    }

    private ICameraShakeSink ResolveCameraShakeSink()
    {
        if (_resolvedCameraShakeSink != null)
            return _resolvedCameraShakeSink;

        _resolvedCameraShakeSink = _cameraShakeSink as ICameraShakeSink;
        return _resolvedCameraShakeSink;
    }

    private float ResolveSfxVolume()
    {
        if (_settings == null && _settingsSource != null)
        {
            _settings = _settingsSource as IGameplaySettingsApplier;
            if (_settings == null)
                _settings = _settingsSource.GetComponent<IGameplaySettingsApplier>();
        }

        return _settings != null ? _settings.SFXVolume : 1f;
    }

}
}
