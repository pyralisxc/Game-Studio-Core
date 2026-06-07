using System;

namespace NeonBlack.Gameplay.Core.Rpg
{
    public readonly struct HubInteractable
    {
        public HubInteractable(
            string interactableId,
            string displayName,
            string promptText,
            string lockedPromptText,
            string iconId,
            HubInteractionKind kind,
            HubInteractionAvailability availability,
            PlayerPanelRoute panelRoute,
            string sceneId,
            string dialogueGraphId,
            string npcId,
            HubInteractionCondition[] conditions,
            HubInteractionEffect[] effects,
            int priority,
            string notificationText)
        {
            InteractableId = Normalize(interactableId);
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? InteractableId : displayName.Trim();
            PromptText = string.IsNullOrWhiteSpace(promptText) ? DisplayName : promptText.Trim();
            LockedPromptText = string.IsNullOrWhiteSpace(lockedPromptText) ? PromptText : lockedPromptText.Trim();
            IconId = Normalize(iconId);
            Kind = kind;
            Availability = availability;
            PanelRoute = panelRoute;
            SceneId = Normalize(sceneId);
            DialogueGraphId = Normalize(dialogueGraphId);
            NpcId = Normalize(npcId);
            Conditions = conditions ?? Array.Empty<HubInteractionCondition>();
            Effects = effects ?? Array.Empty<HubInteractionEffect>();
            Priority = priority;
            NotificationText = notificationText ?? string.Empty;
        }

        public string InteractableId { get; }
        public string DisplayName { get; }
        public string PromptText { get; }
        public string LockedPromptText { get; }
        public string IconId { get; }
        public HubInteractionKind Kind { get; }
        public HubInteractionAvailability Availability { get; }
        public PlayerPanelRoute PanelRoute { get; }
        public string SceneId { get; }
        public string DialogueGraphId { get; }
        public string NpcId { get; }
        public HubInteractionCondition[] Conditions { get; }
        public HubInteractionEffect[] Effects { get; }
        public int Priority { get; }
        public string NotificationText { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
