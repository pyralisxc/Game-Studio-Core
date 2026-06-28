using NeonBlack.Gameplay.Data.Rpg;
namespace NeonBlack.Gameplay.Modules.Rpg.Runtime
{
    public interface IDialogueConditionResolver
    {
        bool Evaluate(RpgOwnerKey owner, DialogueCondition condition);
    }
}
