namespace NeonBlack.Gameplay.Core.Rpg
{
    public interface IDialogueEffectSink
    {
        bool TryApply(RpgOwnerKey owner, DialogueEffect effect, out string issue);
    }
}
