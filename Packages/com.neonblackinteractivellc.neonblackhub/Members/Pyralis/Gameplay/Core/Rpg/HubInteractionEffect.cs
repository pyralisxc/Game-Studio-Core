namespace NeonBlack.Gameplay.Core.Rpg
{
    public readonly struct HubInteractionEffect
    {
        public HubInteractionEffect(HubEffectKind kind, string targetId, string secondaryTargetId, int quantity, bool boolValue, PlayerPanelRoute panelRoute)
        {
            Kind = kind;
            TargetId = Normalize(targetId);
            SecondaryTargetId = Normalize(secondaryTargetId);
            Quantity = quantity < 1 ? 1 : quantity;
            BoolValue = boolValue;
            PanelRoute = panelRoute;
        }

        public HubEffectKind Kind { get; }
        public string TargetId { get; }
        public string SecondaryTargetId { get; }
        public int Quantity { get; }
        public bool BoolValue { get; }
        public PlayerPanelRoute PanelRoute { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
