namespace NeonBlack.Gameplay.Modules.Feedback
{
    public interface IParticipantFeedbackPublisher
    {
        void Publish(ParticipantFeedbackMessage message);
    }
}
