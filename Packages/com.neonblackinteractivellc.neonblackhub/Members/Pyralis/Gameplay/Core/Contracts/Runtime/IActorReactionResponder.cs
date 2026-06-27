namespace NeonBlack.Gameplay.Core.Contracts
{
    public interface IActorReactionResponder
    {
        void ApplyReactionLock(float duration);
        void ClearReactionLock();
    }
}
