namespace NeonBlack.Gameplay.Modules.Feedback
{
    public interface IActorFeedbackReceiver
    {
        void HandleFeedbackEvent(ActorFeedbackEvent feedbackEvent);
    }
}
