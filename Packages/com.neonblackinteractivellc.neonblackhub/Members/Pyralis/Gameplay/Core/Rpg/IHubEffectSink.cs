namespace NeonBlack.Gameplay.Core.Rpg
{
    public interface IHubEffectSink
    {
        bool TryApply(RpgOwnerKey owner, HubInteractionEffect effect, out string issue);
    }
}
