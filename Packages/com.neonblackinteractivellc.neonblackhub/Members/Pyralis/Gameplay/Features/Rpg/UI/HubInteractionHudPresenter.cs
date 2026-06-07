using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Features.Composition;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeonBlack.Gameplay.Features.Rpg.UI
{
    [AddComponentMenu("NeonBlack/Gameplay/RPG/UI/Hub Interaction HUD Presenter")]
    public sealed class HubInteractionHudPresenter : MonoBehaviour, IRuntimeValidationProvider
    {
        [Header("Prompt Surface")]
        [SerializeField] private GameObject promptRoot;
        [SerializeField] private TextMeshProUGUI promptLabel;
        [SerializeField] private TextMeshProUGUI promptHintLabel;
        [SerializeField] private GameObject lockedBadgeRoot;

        [Header("Notification Surface")]
        [SerializeField] private GameObject notificationRoot;
        [SerializeField] private TextMeshProUGUI notificationTitleLabel;
        [SerializeField] private TextMeshProUGUI notificationBodyLabel;

        [Header("Result Surface")]
        [SerializeField] private TextMeshProUGUI routeLabel;
        [SerializeField] private TextMeshProUGUI issueLabel;

        [Header("Buttons")]
        [SerializeField] private Button selectButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button previousButton;

        [Header("Copy")]
        [SerializeField] private string emptyPromptText = string.Empty;
        [SerializeField] private string lockedPrefix = "Locked: ";

        private readonly HubHudPromptList _promptList = new HubHudPromptList();
        private readonly HubHudPresentationState _presentationState = new HubHudPresentationState();

        public event Action<HubPromptPayload> PromptConfirmed;
        public event Action<HubInteractionResult> InteractionResultShown;

        public HubHudPromptList PromptList => _promptList;
        public HubHudPresentationState PresentationState => _presentationState;

        private void OnEnable()
        {
            selectButton?.onClick.AddListener(ConfirmSelectedPrompt);
            nextButton?.onClick.AddListener(SelectNextPrompt);
            previousButton?.onClick.AddListener(SelectPreviousPrompt);
            RenderPrompt();
        }

        private void OnDisable()
        {
            selectButton?.onClick.RemoveListener(ConfirmSelectedPrompt);
            nextButton?.onClick.RemoveListener(SelectNextPrompt);
            previousButton?.onClick.RemoveListener(SelectPreviousPrompt);
        }

        public void ShowPrompts(IEnumerable<HubPromptPayload> prompts)
        {
            _promptList.ApplyPrompts(prompts);
            RenderPrompt();
        }

        public void ClearPrompts()
        {
            _promptList.Clear();
            RenderPrompt();
        }

        public void SelectPrompt(string interactableId)
        {
            if (_promptList.SelectPrompt(interactableId))
                RenderPrompt();
        }

        public void SelectNextPrompt()
        {
            _promptList.SelectNext();
            RenderPrompt();
        }

        public void SelectPreviousPrompt()
        {
            _promptList.SelectPrevious();
            RenderPrompt();
        }

        public void ConfirmSelectedPrompt()
        {
            if (!_promptList.HasPrompt)
                return;

            PromptConfirmed?.Invoke(_promptList.SelectedPrompt);
        }

        public void ShowInteractionResult(HubInteractionResult result)
        {
            _presentationState.ApplyResult(result);
            RenderResult(result);
            InteractionResultShown?.Invoke(result);
        }

        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (promptLabel == null)
                yield return "`HubInteractionHudPresenter` should reference a prompt label so available hub actions are visible.";

            if (selectButton == null)
                yield return "`HubInteractionHudPresenter` can show prompts without Select Button, but player confirmation needs a button or input bridge calling ConfirmSelectedPrompt().";

            if (notificationBodyLabel == null && notificationTitleLabel == null && routeLabel == null && issueLabel == null)
                yield return "`HubInteractionHudPresenter` should reference at least one result or notification label for hub interaction feedback.";
        }

        private void RenderPrompt()
        {
            bool hasPrompt = _promptList.HasPrompt;
            if (promptRoot != null)
                promptRoot.SetActive(hasPrompt || !string.IsNullOrEmpty(emptyPromptText));

            if (!hasPrompt)
            {
                if (promptLabel != null)
                    promptLabel.text = emptyPromptText;
                if (promptHintLabel != null)
                    promptHintLabel.text = string.Empty;
                if (lockedBadgeRoot != null)
                    lockedBadgeRoot.SetActive(false);
                SetPromptButtons(false);
                return;
            }

            HubPromptPayload prompt = _promptList.SelectedPrompt;
            if (promptLabel != null)
                promptLabel.text = prompt.Locked ? lockedPrefix + prompt.Text : prompt.Text;
            if (promptHintLabel != null)
                promptHintLabel.text = _promptList.Prompts.Count > 1 ? $"{_promptList.SelectedIndex + 1}/{_promptList.Prompts.Count}" : string.Empty;
            if (lockedBadgeRoot != null)
                lockedBadgeRoot.SetActive(prompt.Locked);

            SetPromptButtons(!prompt.Locked);
        }

        private void RenderResult(HubInteractionResult result)
        {
            if (issueLabel != null)
                issueLabel.text = result.Status == HubInteractionStatus.Invalid || result.Status == HubInteractionStatus.Locked
                    ? result.Issue
                    : string.Empty;

            if (routeLabel != null)
                routeLabel.text = BuildRouteText(result);

            HubNotificationPayload notification = result.Notifications != null && result.Notifications.Length > 0
                ? result.Notifications[0]
                : default;
            bool hasNotification = !string.IsNullOrWhiteSpace(notification.Title) || !string.IsNullOrWhiteSpace(notification.Body);
            if (notificationRoot != null)
                notificationRoot.SetActive(hasNotification);
            if (notificationTitleLabel != null)
                notificationTitleLabel.text = notification.Title;
            if (notificationBodyLabel != null)
                notificationBodyLabel.text = notification.Body;
        }

        private void SetPromptButtons(bool canSelect)
        {
            if (selectButton != null)
                selectButton.interactable = canSelect;

            bool hasMultiple = _promptList.Prompts.Count > 1;
            if (nextButton != null)
                nextButton.interactable = hasMultiple;
            if (previousButton != null)
                previousButton.interactable = hasMultiple;
        }

        private static string BuildRouteText(HubInteractionResult result)
        {
            if (!string.IsNullOrEmpty(result.SceneId))
                return $"Scene: {result.SceneId}";
            if (!string.IsNullOrEmpty(result.DialogueGraphId))
                return string.IsNullOrEmpty(result.NpcId) ? $"Dialogue: {result.DialogueGraphId}" : $"Dialogue: {result.NpcId}";
            if (result.PanelRoute != PlayerPanelRoute.None)
                return $"Panel: {result.PanelRoute}";
            return string.Empty;
        }
    }
}
