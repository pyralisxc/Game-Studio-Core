namespace NeonBlack.Gameplay.Core.Contracts
{
    public interface IActorInteractionRequestReceiver
    {
        bool TryHandleInteraction();
    }
}
