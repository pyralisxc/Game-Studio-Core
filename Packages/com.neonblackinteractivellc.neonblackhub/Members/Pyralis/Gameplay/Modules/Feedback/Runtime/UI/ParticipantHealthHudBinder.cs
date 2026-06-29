using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Participants;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Feedback.UI
{
    [AuthoringContract(
        Category = "U I",
        CapabilityPath = "UI/HUD/Participant Health Hud Binder",
        Surface = AuthoringSurface.Goal,
        Summary = "Binds participant health state to UI elements like labels and progress bars.",
        RequiredFields = new[] { nameof(healthLabel), nameof(healthFillImage), nameof(healthPanels) },
        SetupSteps = new[] { "Attach to HUD canvas element", "Assign health label or fill image" },
        SuccessChecks = new[] { "The health bar updates when the tracked participant takes damage." },
        Tags = new[] { "capability:UI" }
    )]
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

        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            CachePanels();

            bool hasDirectHudSurface = healthLabel != null || healthFillImage != null;
            bool hasPanelSurface = healthPanels != null && healthPanels.Length > 0;

            if (!hasDirectHudSurface && !hasPanelSurface)
                yield return RuntimeValidationIssue.Required("`ParticipantHealthHudBinder` should reference a health label, fill image, or health panel.");
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
