using NeonBlack.Gameplay.Modules.Character;
using NeonBlack.Gameplay.Modules.Combat;
using NUnit.Framework;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class PawnAndCombatStateMachineTests
    {
        [Test]
        public void PawnLocomotionStateMachine_PrioritizesClimbOverGroundContact()
        {
            MovementState movementState = new MovementState
            {
                IsGrounded = true,
                IsClimbing = true,
            };

            PawnLocomotionState projected = PawnLocomotionStateMachine.Resolve(movementState);

            Assert.AreEqual(PawnLocomotionState.Climbing, projected);
        }

        [Test]
        public void PawnLocomotionStateMachine_DoesNotLeaveDeadWithoutReset()
        {
            PawnLocomotionStateMachine stateMachine = new PawnLocomotionStateMachine();
            Assert.True(stateMachine.TryTransitionTo(PawnLocomotionState.Dead));

            bool transitioned = stateMachine.TryTransitionTo(PawnLocomotionState.Grounded);

            Assert.False(transitioned);
            Assert.AreEqual(PawnLocomotionState.Dead, stateMachine.CurrentState);
        }

        [Test]
        public void PawnLocomotionStateMachine_ResetReturnsToGrounded()
        {
            PawnLocomotionStateMachine stateMachine = new PawnLocomotionStateMachine();
            Assert.True(stateMachine.TryTransitionTo(PawnLocomotionState.Dead));

            stateMachine.Reset();

            Assert.AreEqual(PawnLocomotionState.Grounded, stateMachine.CurrentState);
        }

        [Test]
        public void CombatActionStateMachine_AllowsOrderedActionFlow()
        {
            CombatActionStateMachine stateMachine = new CombatActionStateMachine();

            Assert.True(stateMachine.TryTransitionTo(CombatActionState.Windup));
            Assert.True(stateMachine.TryTransitionTo(CombatActionState.Active));
            Assert.True(stateMachine.TryTransitionTo(CombatActionState.Recovery));
            Assert.True(stateMachine.TryTransitionTo(CombatActionState.Cooldown));
            Assert.True(stateMachine.TryTransitionTo(CombatActionState.Idle));

            Assert.AreEqual(CombatActionState.Idle, stateMachine.CurrentState);
        }

        [Test]
        public void CombatActionStateMachine_RejectsRecoveryToActive()
        {
            CombatActionStateMachine stateMachine = new CombatActionStateMachine();
            Assert.True(stateMachine.TryTransitionTo(CombatActionState.Windup));
            Assert.True(stateMachine.TryTransitionTo(CombatActionState.Active));
            Assert.True(stateMachine.TryTransitionTo(CombatActionState.Recovery));

            bool transitioned = stateMachine.TryTransitionTo(CombatActionState.Active);

            Assert.False(transitioned);
            Assert.AreEqual(CombatActionState.Recovery, stateMachine.CurrentState);
        }

        [Test]
        public void CombatActionStateMachine_ProjectsCooldownFromTimers()
        {
            CombatActionState projected = CombatActionStateMachine.Resolve(false, 0f, 0.2f);

            Assert.AreEqual(CombatActionState.Cooldown, projected);
        }

        [Test]
        public void PawnLocomotionStateMachine_RequiresGroundedActionFreeDodge()
        {
            MovementState movementState = new MovementState
            {
                IsGrounded = false,
                IsActing = false,
            };

            bool canDodge = PawnLocomotionStateMachine.CanStartDodge(movementState, true);

            Assert.False(canDodge);
        }

        [Test]
        public void CombatActionStateMachine_RejectsActionStartDuringCooldown()
        {
            bool canStart = CombatActionStateMachine.CanStartActionFrom(false, 0.1f);

            Assert.False(canStart);
        }
    }
}
