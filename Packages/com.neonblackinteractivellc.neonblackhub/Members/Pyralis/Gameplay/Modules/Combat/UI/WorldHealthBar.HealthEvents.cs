using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Combat
{
    public partial class WorldHealthBar
    {
        private void OnDamaged(float amount)
        {
            _targetFill = Mathf.Clamp01(_health.HealthPercent);
            _ghostTimer = ghostDelay;
            Show();
            UpdateHpLabel();
            if (punchOnDamage) TriggerPunch();
            if (showDamageNumbers)
                ResolveDamageNumberSink()?.Spawn(amount, transform.position + numberSpawnOffset);
        }

        private void OnHealed(float amount)
        {
            _targetFill = Mathf.Clamp01(_health.HealthPercent);
            if (_ghost != null) _ghost.rectTransform.sizeDelta = new Vector2(CW * _targetFill, 0f);
            if (!alwaysVisible) Show();
            UpdateHpLabel();
            if (showHealNumbers)
                ResolveDamageNumberSink()?.SpawnHeal(amount, transform.position + numberSpawnOffset);
        }
    }
}
