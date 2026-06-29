namespace NeonBlack.Gameplay.Core.Contracts
{
    public readonly struct ParticipantLifecycleChangedEvent : IGameplayEvent
    {
        public ParticipantLifecycleChangedEvent(
            int participantId,
            int seatIndex,
            ParticipantLifecycleState previousState,
            ParticipantLifecycleState currentState)
        {
            ParticipantId = participantId;
            SeatIndex = seatIndex;
            PreviousState = previousState;
            CurrentState = currentState;
        }

        public int ParticipantId { get; }
        public int SeatIndex { get; }
        public ParticipantLifecycleState PreviousState { get; }
        public ParticipantLifecycleState CurrentState { get; }
    }
}
