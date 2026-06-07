namespace NeonBlack.Gameplay.Core.Rpg
{
    public readonly struct DialogueSessionState
    {
        public DialogueSessionState(RpgOwnerKey owner, string npcId, string graphId, string currentNodeId, bool ended)
        {
            Owner = owner;
            NpcId = Normalize(npcId);
            GraphId = Normalize(graphId);
            CurrentNodeId = Normalize(currentNodeId);
            Ended = ended;
        }

        public RpgOwnerKey Owner { get; }
        public string NpcId { get; }
        public string GraphId { get; }
        public string CurrentNodeId { get; }
        public bool Ended { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
