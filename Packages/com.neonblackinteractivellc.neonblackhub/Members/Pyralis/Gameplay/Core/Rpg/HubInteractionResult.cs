using System;

namespace NeonBlack.Gameplay.Core.Rpg
{
    public readonly struct HubInteractionResult
    {
        public HubInteractionResult(
            HubInteractionStatus status,
            string issue,
            HubPromptPayload prompt,
            PlayerPanelRoute panelRoute,
            string sceneId,
            string dialogueGraphId,
            string npcId,
            HubNotificationPayload[] notifications)
        {
            Status = status;
            Issue = issue ?? string.Empty;
            Prompt = prompt;
            PanelRoute = panelRoute;
            SceneId = Normalize(sceneId);
            DialogueGraphId = Normalize(dialogueGraphId);
            NpcId = Normalize(npcId);
            Notifications = notifications ?? Array.Empty<HubNotificationPayload>();
        }

        public HubInteractionStatus Status { get; }
        public string Issue { get; }
        public HubPromptPayload Prompt { get; }
        public PlayerPanelRoute PanelRoute { get; }
        public string SceneId { get; }
        public string DialogueGraphId { get; }
        public string NpcId { get; }
        public HubNotificationPayload[] Notifications { get; }

        public static HubInteractionResult Invalid(string issue)
        {
            return new HubInteractionResult(HubInteractionStatus.Invalid, issue, default, PlayerPanelRoute.None, string.Empty, string.Empty, string.Empty, Array.Empty<HubNotificationPayload>());
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
