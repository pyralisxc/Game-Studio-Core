using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Modules.Character;
using NeonBlack.Gameplay.Modules.Combat;
using NeonBlack.Gameplay.Glue.Participants;
using NeonBlack.Gameplay.Glue.Session;
using NUnit.Framework;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class RuntimeStateMachineTests
    {
        [Test]
        public void SessionStateMachine_RequiresAuthoringReadyBeforePlaying()
        {
            SessionStateMachine stateMachine = new SessionStateMachine();

            Assert.False(stateMachine.TryTransitionTo(SessionLifecycleState.Playing));
            Assert.AreEqual(SessionLifecycleState.Booting, stateMachine.CurrentState);

            Assert.True(stateMachine.TryTransitionTo(SessionLifecycleState.AuthoringReady));
            Assert.True(stateMachine.TryTransitionTo(SessionLifecycleState.Loading));
            Assert.True(stateMachine.TryTransitionTo(SessionLifecycleState.Playing));

            Assert.AreEqual(SessionLifecycleState.Playing, stateMachine.CurrentState);
        }

        [Test]
        public void ParticipantStateMachine_AllowsJoinSpawnPossessAndRejectsSkip()
        {
            ParticipantStateMachine blocked = new ParticipantStateMachine();
            Assert.False(blocked.TryTransitionTo(ParticipantLifecycleState.PossessingPawn));
            Assert.AreEqual(ParticipantLifecycleState.Unjoined, blocked.CurrentState);

            ParticipantStateMachine stateMachine = new ParticipantStateMachine();
            Assert.True(stateMachine.TryTransitionTo(ParticipantLifecycleState.Joined));
            Assert.True(stateMachine.TryTransitionTo(ParticipantLifecycleState.Spawned));
            Assert.True(stateMachine.TryTransitionTo(ParticipantLifecycleState.PossessingPawn));

            Assert.AreEqual(ParticipantLifecycleState.PossessingPawn, stateMachine.CurrentState);
        }

        [Test]
        public void PawnLocomotionStateMachine_ResolvesMovementAndProtectsDeadState()
        {
            MovementState climbingGrounded = new MovementState
            {
                IsGrounded = true,
                IsClimbing = true,
            };
            Assert.AreEqual(PawnLocomotionState.Climbing, PawnLocomotionStateMachine.Resolve(climbingGrounded));
            Assert.False(PawnLocomotionStateMachine.CanStartDodge(new MovementState { IsGrounded = false }, true));

            PawnLocomotionStateMachine stateMachine = new PawnLocomotionStateMachine();
            Assert.True(stateMachine.TryTransitionTo(PawnLocomotionState.Dead));
            Assert.False(stateMachine.TryTransitionTo(PawnLocomotionState.Grounded));
            Assert.AreEqual(PawnLocomotionState.Dead, stateMachine.CurrentState);

            stateMachine.Reset();
            Assert.AreEqual(PawnLocomotionState.Grounded, stateMachine.CurrentState);
        }

        [Test]
        public void CombatActionStateMachine_AllowsOrderedFlowAndRejectsInvalidStarts()
        {
            CombatActionStateMachine stateMachine = new CombatActionStateMachine();
            Assert.True(stateMachine.TryTransitionTo(CombatActionState.Windup));
            Assert.True(stateMachine.TryTransitionTo(CombatActionState.Active));
            Assert.True(stateMachine.TryTransitionTo(CombatActionState.Recovery));
            Assert.False(stateMachine.TryTransitionTo(CombatActionState.Active));
            Assert.AreEqual(CombatActionState.Recovery, stateMachine.CurrentState);

            Assert.True(stateMachine.TryTransitionTo(CombatActionState.Cooldown));
            Assert.True(stateMachine.TryTransitionTo(CombatActionState.Idle));

            Assert.AreEqual(CombatActionState.Cooldown, CombatActionStateMachine.Resolve(false, 0f, 0.2f));
            Assert.False(CombatActionStateMachine.CanStartActionFrom(false, 0.1f));
        }
    }
}
