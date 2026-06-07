using System;

namespace NeonBlack.Gameplay.Core.Rpg
{
    public readonly struct NpcProfile : INpcProfile
    {
        public NpcProfile(string npcId, string displayName, string role, string[] tags, string factionId, string actorLinkId)
        {
            NpcId = Normalize(npcId);
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? NpcId : displayName.Trim();
            Role = Normalize(role);
            Tags = tags ?? Array.Empty<string>();
            FactionId = Normalize(factionId);
            ActorLinkId = Normalize(actorLinkId);
        }

        public string NpcId { get; }
        public string DisplayName { get; }
        public string Role { get; }
        public string[] Tags { get; }
        public string FactionId { get; }
        public string ActorLinkId { get; }
        public bool IsValid => !string.IsNullOrWhiteSpace(NpcId);

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
