namespace NeonBlack.Gameplay.Core.Contracts
{
    public readonly struct SessionLifecycleChangedEvent : IGameplayEvent
    {
        public SessionLifecycleChangedEvent(SessionLifecycleState previousState, SessionLifecycleState currentState)
        {
            PreviousState = previousState;
            CurrentState = currentState;
        }

        public SessionLifecycleState PreviousState { get; }
        public SessionLifecycleState CurrentState { get; }
    }
}
