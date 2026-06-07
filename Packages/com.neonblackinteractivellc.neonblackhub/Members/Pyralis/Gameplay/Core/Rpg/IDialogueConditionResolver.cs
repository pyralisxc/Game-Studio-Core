namespace NeonBlack.Gameplay.Core.Rpg
{
    public interface IDialogueConditionResolver
    {
        bool Evaluate(RpgOwnerKey owner, DialogueCondition condition);
    }
}
