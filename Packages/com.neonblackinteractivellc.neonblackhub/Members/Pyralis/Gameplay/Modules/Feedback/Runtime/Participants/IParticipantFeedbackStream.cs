using System;

namespace NeonBlack.Gameplay.Modules.Feedback
{
    public interface IParticipantFeedbackStream
    {
        event Action<ParticipantFeedbackMessage> FeedbackPublished;
    }
}
