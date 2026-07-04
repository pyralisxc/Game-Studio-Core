using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Participants;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Feedback.UI
{
    [AuthoringContract(
        Category = "U I",
        CapabilityPath = "UI/HUD/Participant Health Hud Binder",
        Surface = AuthoringSurface.Goal,
        Summary = "Binds participant health state to UI elements like labels and progress bars.",
        RequiredFields = new[] { nameof(healthPanels) },
        SetupSteps = new[] { "Attach to HUD canvas element", "Assign or child a ParticipantHealthPanel" },
        SuccessChecks = new[] { "The health bar updates when the tracked participant takes damage." },
        Tags = new[] { "capability:UI" }
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Feedback/UI/Participant Health HUD Binder")]
    public class ParticipantHealthHudBinder : ParticipantHudTargetBinding, IRuntimeValidationProvider
    {
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

            bool hasPanelSurface = healthPanels != null && healthPanels.Length > 0;

            if (!hasPanelSurface)
                yield return RuntimeValidationIssue.Required("`ParticipantHealthHudBinder` should reference at least one `ParticipantHealthPanel`.");
        }

        private void CachePanels()
        {
            if (healthPanels == null || healthPanels.Length == 0)
                healthPanels = GetComponentsInChildren<ParticipantHealthPanel>(true);
        }

        private void UpdateHealthUI()
        {
            if (healthPanels == null || healthPanels.Length == 0)
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
        }
    }
}
