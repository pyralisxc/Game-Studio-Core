namespace NeonBlack.Gameplay.Features.Feedback
{
    public interface IParticipantFeedbackPublisher
    {
        void Publish(ParticipantFeedbackMessage message);
    }
}
