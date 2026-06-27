using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Glue.Participants
{
    public sealed class ParticipantStateMachine
    {
        public ParticipantLifecycleState CurrentState { get; private set; } = ParticipantLifecycleState.Unjoined;

        public bool TryTransitionTo(ParticipantLifecycleState nextState)
        {
            if (!CanTransition(CurrentState, nextState))
                return false;

            CurrentState = nextState;
            return true;
        }

        public static bool CanTransition(ParticipantLifecycleState currentState, ParticipantLifecycleState nextState)
        {
            if (currentState == nextState)
                return true;

            switch (currentState)
            {
                case ParticipantLifecycleState.Unjoined:
                    return nextState == ParticipantLifecycleState.Joined;
                case ParticipantLifecycleState.Joined:
                    return nextState == ParticipantLifecycleState.Spawned
                        || nextState == ParticipantLifecycleState.Eliminated
                        || nextState == ParticipantLifecycleState.Left;
                case ParticipantLifecycleState.Spawned:
                    return nextState == ParticipantLifecycleState.PossessingPawn
                        || nextState == ParticipantLifecycleState.Eliminated
                        || nextState == ParticipantLifecycleState.Left;
                case ParticipantLifecycleState.PossessingPawn:
                    return nextState == ParticipantLifecycleState.Joined
                        || nextState == ParticipantLifecycleState.Eliminated
                        || nextState == ParticipantLifecycleState.Left;
                case ParticipantLifecycleState.Eliminated:
                    return nextState == ParticipantLifecycleState.Left;
                default:
                    return false;
            }
        }
    }
}
