using System.Collections.Generic;
using NeonBlack.Gameplay.Features.Composition;
using TMPro;
using UnityEngine;
using VContainer;

namespace NeonBlack.Gameplay.Features.Feedback.UI
{
    [AddComponentMenu("NeonBlack/Gameplay/Feedback/UI/Participant Feedback HUD Presenter")]
    public class ParticipantFeedbackHudPresenter : ParticipantHudTargetBinding, IRuntimeValidationProvider
    {
        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI comboLabel;
        [SerializeField] private TextMeshProUGUI statusLabel;
        [SerializeField] private TextMeshProUGUI scorePopupLabel;
        [SerializeField] private TextMeshProUGUI combatAlertLabel;

        [Header("Reusable Panels")]
        [SerializeField] private ParticipantTimedTextPanel[] comboPanels;
        [SerializeField] private ParticipantTimedTextPanel[] statusPanels;
        [SerializeField] private ParticipantTimedTextPanel[] scorePanels;
        [SerializeField] private ParticipantTimedTextPanel[] combatAlertPanels;

        [Header("Timing")]
        [SerializeField] private float comboDisplayTime = 0.8f;
        [SerializeField] private float statusDisplayTime = 1f;
        [SerializeField] private float scoreDisplayTime = 0.7f;
        [SerializeField] private float combatAlertDisplayTime = 0.9f;

        private IParticipantFeedbackStream _feedbackStream;
        private float _comboTimer;
        private float _statusTimer;
        private float _scoreTimer;
        private float _combatAlertTimer;

        [Inject]
        private void Construct(IParticipantFeedbackStream feedbackStream = null)
        {
            _feedbackStream = feedbackStream;
        }

        private void Start()
        {
            CachePanels();
            if (_feedbackStream == null)
                return;

            _feedbackStream.FeedbackPublished += HandleFeedbackMessage;
        }

        private void Update()
        {
            TickLabel(comboLabel, ref _comboTimer);
            TickLabel(statusLabel, ref _statusTimer);
            TickLabel(scorePopupLabel, ref _scoreTimer);
            TickLabel(combatAlertLabel, ref _combatAlertTimer);
        }

        private void OnDestroy()
        {
            if (_feedbackStream == null)
                return;

            _feedbackStream.FeedbackPublished -= HandleFeedbackMessage;
        }

        private void HandleFeedbackMessage(ParticipantFeedbackMessage message)
        {
            if (!MatchesParticipant(message.Participant))
                return;

            switch (message.Kind)
            {
                case ParticipantFeedbackKind.Combo:
                    ShowPanels(comboPanels, $"Combo {message.IntValue}", comboDisplayTime);
                    ShowLegacyLabel(comboLabel, $"Combo {message.IntValue}", ref _comboTimer, comboDisplayTime);
                    break;
                case ParticipantFeedbackKind.Status:
                    ShowPanels(statusPanels, message.TextValue, statusDisplayTime);
                    ShowLegacyLabel(statusLabel, message.TextValue, ref _statusTimer, statusDisplayTime);
                    break;
                case ParticipantFeedbackKind.Score:
                    string scoreText = $"+{message.IntValue}";
                    ShowPanels(scorePanels, scoreText, scoreDisplayTime);
                    ShowLegacyLabel(scorePopupLabel, scoreText, ref _scoreTimer, scoreDisplayTime);
                    break;
                case ParticipantFeedbackKind.CombatAlert:
                    string alertText = message.TextValue == "Finisher"
                        ? $"Finisher {message.IntValue}"
                        : message.TextValue == "GuardBreak"
                            ? "Guard Break"
                            : message.TextValue;
                    ShowPanels(combatAlertPanels, alertText, combatAlertDisplayTime);
                    ShowLegacyLabel(combatAlertLabel, alertText, ref _combatAlertTimer, combatAlertDisplayTime);
                    break;
            }
        }

        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            CachePanels();

            bool hasLegacySurface = comboLabel != null
                || statusLabel != null
                || scorePopupLabel != null
                || combatAlertLabel != null;
            bool hasPanelSurface = (comboPanels != null && comboPanels.Length > 0)
                || (statusPanels != null && statusPanels.Length > 0)
                || (scorePanels != null && scorePanels.Length > 0)
                || (combatAlertPanels != null && combatAlertPanels.Length > 0);

            if (!hasLegacySurface && !hasPanelSurface)
                yield return "`ParticipantFeedbackHudPresenter` should reference at least one feedback label or timed text panel.";
        }

        private void CachePanels()
        {
            if (comboPanels == null || comboPanels.Length == 0)
                comboPanels = GetPanelsWithNameHint("combo");
            if (statusPanels == null || statusPanels.Length == 0)
                statusPanels = GetPanelsWithNameHint("status");
            if (scorePanels == null || scorePanels.Length == 0)
                scorePanels = GetPanelsWithNameHint("score");
            if (combatAlertPanels == null || combatAlertPanels.Length == 0)
                combatAlertPanels = GetPanelsWithNameHint("combat");
        }

        private ParticipantTimedTextPanel[] GetPanelsWithNameHint(string nameHint)
        {
            ParticipantTimedTextPanel[] allPanels = GetComponentsInChildren<ParticipantTimedTextPanel>(true);
            List<ParticipantTimedTextPanel> matched = new List<ParticipantTimedTextPanel>();
            for (int i = 0; i < allPanels.Length; i++)
            {
                if (allPanels[i] != null && allPanels[i].name.ToLowerInvariant().Contains(nameHint))
                    matched.Add(allPanels[i]);
            }

            return matched.ToArray();
        }

        private static void TickLabel(TextMeshProUGUI label, ref float timer)
        {
            if (label == null || timer <= 0f)
                return;

            timer -= Time.deltaTime;
            if (timer <= 0f)
                label.gameObject.SetActive(false);
        }

        private static void ShowPanels(ParticipantTimedTextPanel[] panels, string text, float duration)
        {
            if (panels == null)
                return;

            for (int i = 0; i < panels.Length; i++)
                panels[i]?.ShowText(text, duration);
        }

        private static void ShowLegacyLabel(TextMeshProUGUI label, string text, ref float timer, float duration)
        {
            if (label == null)
                return;

            label.text = text;
            label.gameObject.SetActive(true);
            timer = duration;
        }
    }
}
