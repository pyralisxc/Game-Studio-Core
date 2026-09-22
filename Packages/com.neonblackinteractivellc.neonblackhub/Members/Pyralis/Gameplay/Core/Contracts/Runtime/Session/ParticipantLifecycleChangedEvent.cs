using NeonBlack.Gameplay.Core.Participants;

namespace NeonBlack.Gameplay.Core.Contracts
{
    public readonly struct ParticipantLifecycleChangedEvent : IGameplayEvent
    {
        public ParticipantLifecycleChangedEvent(
            ParticipantId participantId,
            int seatIndex,
            ParticipantLifecycleState previousState,
            ParticipantLifecycleState currentState)
        {
            ParticipantId = participantId;
            SeatIndex = seatIndex;
            PreviousState = previousState;
            CurrentState = currentState;
        }

        public ParticipantId ParticipantId { get; }
        public int SeatIndex { get; }
        public ParticipantLifecycleState PreviousState { get; }
        public ParticipantLifecycleState CurrentState { get; }
    }
}
