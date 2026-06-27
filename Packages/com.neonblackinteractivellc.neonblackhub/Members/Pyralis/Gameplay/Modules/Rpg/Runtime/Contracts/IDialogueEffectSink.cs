namespace NeonBlack.Gameplay.Modules.Rpg.Runtime
{
    public interface IDialogueEffectSink
    {
        bool TryApply(RpgOwnerKey owner, DialogueEffect effect, out string issue);
    }
}
