using NeonBlack.Gameplay.Characters;

namespace NeonBlack.Gameplay.Features.Feedback
{
    public readonly struct ParticipantFeedbackMessage
    {
        public ParticipantFeedbackMessage(
            ParticipantHandle participant,
            ParticipantFeedbackKind kind,
            int intValue = 0,
            float floatValue = 0f,
            string textValue = null)
        {
            Participant = participant;
            Kind = kind;
            IntValue = intValue;
            FloatValue = floatValue;
            TextValue = textValue;
        }

        public ParticipantHandle Participant { get; }
        public ParticipantFeedbackKind Kind { get; }
        public int IntValue { get; }
        public float FloatValue { get; }
        public string TextValue { get; }
    }
}
