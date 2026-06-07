namespace NeonBlack.Gameplay.Features.Feedback
{
    public interface IActorFeedbackReceiver
    {
        void HandleFeedbackEvent(ActorFeedbackEvent feedbackEvent);
    }
}
