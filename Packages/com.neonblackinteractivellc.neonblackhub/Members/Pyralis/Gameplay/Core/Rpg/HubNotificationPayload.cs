namespace NeonBlack.Gameplay.Core.Rpg
{
    public readonly struct HubNotificationPayload
    {
        public HubNotificationPayload(string title, string body, string iconId, string severity, float durationSeconds)
        {
            Title = string.IsNullOrWhiteSpace(title) ? string.Empty : title.Trim();
            Body = body ?? string.Empty;
            IconId = string.IsNullOrWhiteSpace(iconId) ? string.Empty : iconId.Trim();
            Severity = string.IsNullOrWhiteSpace(severity) ? "info" : severity.Trim();
            DurationSeconds = durationSeconds <= 0f ? 2.5f : durationSeconds;
        }

        public string Title { get; }
        public string Body { get; }
        public string IconId { get; }
        public string Severity { get; }
        public float DurationSeconds { get; }
    }
}
