namespace NeonBlack.Gameplay.Core.Rpg
{
    public readonly struct HubPromptPayload
    {
        public HubPromptPayload(RpgOwnerKey owner, string hubId, string interactableId, string text, string iconId, bool locked, int priority = 0)
        {
            Owner = owner;
            HubId = Normalize(hubId);
            InteractableId = Normalize(interactableId);
            Text = text ?? string.Empty;
            IconId = Normalize(iconId);
            Locked = locked;
            Priority = priority;
        }

        public RpgOwnerKey Owner { get; }
        public string HubId { get; }
        public string InteractableId { get; }
        public string Text { get; }
        public string IconId { get; }
        public bool Locked { get; }
        public int Priority { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
