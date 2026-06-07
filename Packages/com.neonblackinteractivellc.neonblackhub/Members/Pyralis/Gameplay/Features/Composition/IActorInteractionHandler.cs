namespace NeonBlack.Gameplay.Features.Composition
{
    public interface IActorInteractionHandler
    {
        bool TryHandleInteraction(ActorFeatureContext context);
    }
}
