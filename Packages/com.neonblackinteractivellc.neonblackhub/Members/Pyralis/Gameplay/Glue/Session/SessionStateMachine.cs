using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Glue.Session
{
    public sealed class SessionStateMachine
    {
        public SessionLifecycleState CurrentState { get; private set; } = SessionLifecycleState.Booting;

        public void Reset(SessionLifecycleState state = SessionLifecycleState.Booting)
        {
            CurrentState = state;
        }

        public bool TryTransitionTo(SessionLifecycleState nextState)
        {
            if (!CanTransition(CurrentState, nextState))
                return false;

            CurrentState = nextState;
            return true;
        }

        public static bool CanTransition(SessionLifecycleState currentState, SessionLifecycleState nextState)
        {
            if (currentState == nextState)
                return true;

            switch (currentState)
            {
                case SessionLifecycleState.Booting:
                    return nextState == SessionLifecycleState.AuthoringReady;
                case SessionLifecycleState.AuthoringReady:
                    return nextState == SessionLifecycleState.Loading || nextState == SessionLifecycleState.Ending;
                case SessionLifecycleState.Loading:
                    return nextState == SessionLifecycleState.Playing || nextState == SessionLifecycleState.Ending;
                case SessionLifecycleState.Playing:
                    return nextState == SessionLifecycleState.Paused
                        || nextState == SessionLifecycleState.Results
                        || nextState == SessionLifecycleState.Ending;
                case SessionLifecycleState.Paused:
                    return nextState == SessionLifecycleState.Playing
                        || nextState == SessionLifecycleState.Results
                        || nextState == SessionLifecycleState.Ending;
                case SessionLifecycleState.Results:
                    return nextState == SessionLifecycleState.Ending;
                default:
                    return false;
            }
        }
    }
}
