namespace NeonBlack.Gameplay.Modules.Rpg.Runtime
{
    public interface IHubConditionResolver
    {
        bool Evaluate(RpgOwnerKey owner, HubInteractionCondition condition);
    }
}
