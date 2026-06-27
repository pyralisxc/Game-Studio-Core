using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Glue.Participants;
using NeonBlack.Gameplay.Glue.Session;
using NUnit.Framework;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class SessionParticipantLifecycleTests
    {
        [Test]
        public void SessionStateMachine_RejectsBootingToPlayingSkip()
        {
            SessionStateMachine stateMachine = new SessionStateMachine();

            bool transitioned = stateMachine.TryTransitionTo(SessionLifecycleState.Playing);

            Assert.False(transitioned);
            Assert.AreEqual(SessionLifecycleState.Booting, stateMachine.CurrentState);
        }

        [Test]
        public void SessionStateMachine_AllowsLoadingToPlaying()
        {
            SessionStateMachine stateMachine = new SessionStateMachine();
            Assert.True(stateMachine.TryTransitionTo(SessionLifecycleState.AuthoringReady));
            Assert.True(stateMachine.TryTransitionTo(SessionLifecycleState.Loading));

            bool transitioned = stateMachine.TryTransitionTo(SessionLifecycleState.Playing);

            Assert.True(transitioned);
            Assert.AreEqual(SessionLifecycleState.Playing, stateMachine.CurrentState);
        }

        [Test]
        public void ParticipantStateMachine_AllowsJoinSpawnPossessPath()
        {
            ParticipantStateMachine stateMachine = new ParticipantStateMachine();

            Assert.True(stateMachine.TryTransitionTo(ParticipantLifecycleState.Joined));
            Assert.True(stateMachine.TryTransitionTo(ParticipantLifecycleState.Spawned));
            Assert.True(stateMachine.TryTransitionTo(ParticipantLifecycleState.PossessingPawn));

            Assert.AreEqual(ParticipantLifecycleState.PossessingPawn, stateMachine.CurrentState);
        }

        [Test]
        public void ParticipantStateMachine_RejectsUnjoinedToPossessingPawnSkip()
        {
            ParticipantStateMachine stateMachine = new ParticipantStateMachine();

            bool transitioned = stateMachine.TryTransitionTo(ParticipantLifecycleState.PossessingPawn);

            Assert.False(transitioned);
            Assert.AreEqual(ParticipantLifecycleState.Unjoined, stateMachine.CurrentState);
        }
    }
}
