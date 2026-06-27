namespace NeonBlack.Gameplay.Modules.Rpg.Runtime
{
    public interface IHubEffectSink
    {
        bool TryApply(RpgOwnerKey owner, HubInteractionEffect effect, out string issue);
    }
}
