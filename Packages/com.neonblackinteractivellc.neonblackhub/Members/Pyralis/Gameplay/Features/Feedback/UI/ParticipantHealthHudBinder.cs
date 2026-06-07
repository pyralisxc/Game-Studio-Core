using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Features.Composition;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeonBlack.Gameplay.Features.Feedback.UI
{
    [AddComponentMenu("NeonBlack/Gameplay/Feedback/UI/Participant Health HUD Binder")]
    public class ParticipantHealthHudBinder : ParticipantHudTargetBinding, IRuntimeValidationProvider
    {
        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI healthLabel;
        [SerializeField] private Image healthFillImage;

        [Header("Reusable Panels")]
        [SerializeField] private ParticipantHealthPanel[] healthPanels;

        private void Start()
        {
            CachePanels();
        }

        private void Update()
        {
            UpdateHealthUI();
        }

        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            CachePanels();

            bool hasDirectHudSurface = healthLabel != null || healthFillImage != null;
            bool hasPanelSurface = healthPanels != null && healthPanels.Length > 0;

            if (!hasDirectHudSurface && !hasPanelSurface)
                yield return "`ParticipantHealthHudBinder` should reference a health label, fill image, or health panel.";
        }

        private void CachePanels()
        {
            if (healthPanels == null || healthPanels.Length == 0)
                healthPanels = GetComponentsInChildren<ParticipantHealthPanel>(true);
        }

        private void UpdateHealthUI()
        {
            if (healthFillImage == null && healthLabel == null && (healthPanels == null || healthPanels.Length == 0))
                return;

            if (!TryGetTrackedParticipant(out ParticipantHandle participant) || participant?.PawnInstance == null)
                return;

            IActorHealthState health = participant.PawnInstance.GetComponent<IActorHealthState>();
            if (health == null)
                return;

            if (healthPanels != null)
            {
                for (int i = 0; i < healthPanels.Length; i++)
                    healthPanels[i]?.ApplyHealth(health);
            }

            if (healthFillImage != null)
                healthFillImage.fillAmount = Mathf.Clamp01(health.HealthPercent);

            if (healthLabel != null)
                healthLabel.text = $"{Mathf.CeilToInt(health.CurrentHealth)}/{Mathf.CeilToInt(health.MaxHealth)}";
        }
    }
}
