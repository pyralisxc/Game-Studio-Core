using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    public partial class WorldHealthBar
    {
        private void LateUpdate()
        {
            if (_canvasRoot == null) return;

            _targetFill = Mathf.Clamp01(_health.HealthPercent);
            TickFollowCamera();
            TickFill();
            TickGhostBar();
            TickPunch();
            TickLowHpFlash();
            TickVisibility();
        }

        private void TickFollowCamera()
        {
            _canvasRoot.position = transform.position + barOffset;
            if (_cam != null) _canvasRoot.rotation = _cam.transform.rotation;
        }

        private void TickFill()
        {
            float currentFill = _fill.rectTransform.sizeDelta.x / CW;
            currentFill = fillAnimSpeed > 0f
                ? Mathf.MoveTowards(currentFill, _targetFill, fillAnimSpeed * Time.deltaTime)
                : _targetFill;
            _fill.rectTransform.sizeDelta = new Vector2(CW * currentFill, 0f);
            RefreshFillColor(currentFill);
        }

        private void TickGhostBar()
        {
            if (_ghost == null)
                return;

            _ghostTimer -= Time.deltaTime;
            if (_ghostTimer > 0f)
                return;

            float ghostFill = _ghost.rectTransform.sizeDelta.x / CW;
            if (ghostFill <= _targetFill)
                return;

            ghostFill = Mathf.MoveTowards(ghostFill, _targetFill, ghostDrainSpeed * Time.deltaTime);
            _ghost.rectTransform.sizeDelta = new Vector2(CW * ghostFill, 0f);
        }

        private void TickPunch()
        {
            if (!_isPunching)
                return;

            _punchTimer -= Time.deltaTime;
            float t = Mathf.Clamp01(1f - _punchTimer / punchDuration);
            _canvasRoot.localScale = _baseScale * Mathf.Lerp(1f, punchScale, Mathf.Sin(t * Mathf.PI));
            if (_punchTimer <= 0f)
            {
                _isPunching = false;
                _canvasRoot.localScale = _baseScale;
            }
        }

        private void TickLowHpFlash()
        {
            if (!flashAtLowHp || _targetFill > flashThreshold)
                return;

            _flashTimer += Time.deltaTime * flashSpeed;
            float pulse = Mathf.Sin(_flashTimer * Mathf.PI * 2f) * 0.5f + 0.5f;
            _fill.color = Color.Lerp(lowHpColor, Color.white, pulse * 0.30f);
        }

        private void TickVisibility()
        {
            if (!alwaysVisible && _visible)
            {
                _hideTimer -= Time.deltaTime;
                if (_hideTimer <= 0f) _visible = false;
            }

            _group.alpha = Mathf.MoveTowards(
                _group.alpha,
                (_visible || alwaysVisible) ? 1f : 0f,
                fadeSpeed * Time.deltaTime);
        }

        private void Show()
        {
            _visible = true;
            _hideTimer = hideDelay;
        }

        private void TriggerPunch()
        {
            _isPunching = true;
            _punchTimer = punchDuration;
        }

        private void RefreshFillColor(float pct)
        {
            if (_fill == null) return;
            if (flashAtLowHp && _targetFill <= flashThreshold) return;

            _fill.color = fillGradient
                ? pct >= 0.5f
                    ? Color.Lerp(midHpColor, fillColor, (pct - 0.5f) * 2f)
                    : Color.Lerp(lowHpColor, midHpColor, pct * 2f)
                : fillColor;
        }
    }
}
