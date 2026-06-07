namespace NeonBlack.Gameplay.Features.Combat
{
    public interface IActorReactionResponder
    {
        void ApplyReactionLock(float duration);
        void ClearReactionLock();
    }
}
