namespace NeonBlack.Gameplay.Data.Interactions
{
    public interface IActorInteractionHandler
    {
        bool TryHandleInteraction(ActorInteractionContext context);
    }
}
