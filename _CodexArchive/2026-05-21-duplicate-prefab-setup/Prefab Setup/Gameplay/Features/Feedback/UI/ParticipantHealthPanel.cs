using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Composition;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeonBlack.Gameplay.Features.Feedback.UI
{
    [AddComponentMenu("NeonBlack/Gameplay/Feedback/UI/Participant Health Panel")]
    public class ParticipantHealthPanel : MonoBehaviour, IRuntimeValidationProvider
    {
        [SerializeField] private TextMeshProUGUI healthLabel;
        [SerializeField] private Image healthFillImage;
        [SerializeField] private bool tintFillByHealth = false;
        [SerializeField] private Gradient healthFillGradient;

        public void ApplyHealth(IActorHealthState health)
        {
            if (health == null)
                return;

            if (healthFillImage != null)
            {
                float fill = Mathf.Clamp01(health.HealthPercent);
                healthFillImage.fillAmount = fill;
                if (tintFillByHealth && healthFillGradient != null)
                    healthFillImage.color = healthFillGradient.Evaluate(fill);
            }

            if (healthLabel != null)
                healthLabel.text = $"{Mathf.CeilToInt(health.CurrentHealth)}/{Mathf.CeilToInt(health.MaxHealth)}";
        }

        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (healthLabel == null && healthFillImage == null)
                yield return "`ParticipantHealthPanel` should reference a health label, a fill image, or both.";
        }
    }
}
