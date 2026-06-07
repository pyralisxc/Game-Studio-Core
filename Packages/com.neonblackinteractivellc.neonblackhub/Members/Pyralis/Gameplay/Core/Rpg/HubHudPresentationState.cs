using System;

namespace NeonBlack.Gameplay.Core.Rpg
{
    public sealed class HubHudPresentationState
    {
        public HubInteractionStatus LastStatus { get; private set; } = HubInteractionStatus.Invalid;
        public string LastIssue { get; private set; } = string.Empty;
        public PlayerPanelRoute ActivePanelRoute { get; private set; } = PlayerPanelRoute.None;
        public string RequestedSceneId { get; private set; } = string.Empty;
        public string RequestedDialogueGraphId { get; private set; } = string.Empty;
        public string RequestedNpcId { get; private set; } = string.Empty;
        public HubNotificationPayload[] Notifications { get; private set; } = Array.Empty<HubNotificationPayload>();

        public void ApplyResult(HubInteractionResult result)
        {
            LastStatus = result.Status;
            LastIssue = result.Issue ?? string.Empty;
            ActivePanelRoute = result.PanelRoute;
            RequestedSceneId = result.SceneId ?? string.Empty;
            RequestedDialogueGraphId = result.DialogueGraphId ?? string.Empty;
            RequestedNpcId = result.NpcId ?? string.Empty;
            Notifications = result.Notifications ?? Array.Empty<HubNotificationPayload>();
        }

        public void Clear()
        {
            LastStatus = HubInteractionStatus.Invalid;
            LastIssue = string.Empty;
            ActivePanelRoute = PlayerPanelRoute.None;
            RequestedSceneId = string.Empty;
            RequestedDialogueGraphId = string.Empty;
            RequestedNpcId = string.Empty;
            Notifications = Array.Empty<HubNotificationPayload>();
        }
    }
}
