namespace NeonBlack.Gameplay.Modules.Rpg.Runtime
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
