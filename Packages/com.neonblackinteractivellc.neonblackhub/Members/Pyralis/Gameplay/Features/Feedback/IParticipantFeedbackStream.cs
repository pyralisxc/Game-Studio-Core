using System;

namespace NeonBlack.Gameplay.Features.Feedback
{
    public interface IParticipantFeedbackStream
    {
        event Action<ParticipantFeedbackMessage> FeedbackPublished;
    }
}
