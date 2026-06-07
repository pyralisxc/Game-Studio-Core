namespace NeonBlack.Gameplay.Core.Rpg
{
    public interface IHubConditionResolver
    {
        bool Evaluate(RpgOwnerKey owner, HubInteractionCondition condition);
    }
}
