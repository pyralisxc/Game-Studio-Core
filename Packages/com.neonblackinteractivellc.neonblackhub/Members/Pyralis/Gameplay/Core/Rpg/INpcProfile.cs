namespace NeonBlack.Gameplay.Core.Rpg
{
    public interface INpcProfile
    {
        string NpcId { get; }
        string DisplayName { get; }
        string Role { get; }
        string[] Tags { get; }
        string FactionId { get; }
        string ActorLinkId { get; }
    }
}
