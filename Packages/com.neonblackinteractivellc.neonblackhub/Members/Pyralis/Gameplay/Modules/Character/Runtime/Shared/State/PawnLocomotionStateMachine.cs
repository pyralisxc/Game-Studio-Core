namespace NeonBlack.Gameplay.Modules.Character
{
    public sealed class PawnLocomotionStateMachine
    {
        public PawnLocomotionState CurrentState { get; private set; } = PawnLocomotionState.Grounded;

        public bool TryTransitionTo(PawnLocomotionState nextState)
        {
            if (!CanTransitionTo(nextState))
                return false;

            CurrentState = nextState;
            return true;
        }

        public bool CanTransitionTo(PawnLocomotionState nextState)
        {
            if (CurrentState == PawnLocomotionState.Dead && nextState != PawnLocomotionState.Dead)
                return false;

            return true;
        }

        public void Reset(PawnLocomotionState state = PawnLocomotionState.Grounded)
        {
            CurrentState = state;
        }

        public static bool CanStartDodge(MovementState state, bool allowDodge)
        {
            return state != null
                && allowDodge
                && !state.IsDodging
                && state.DodgeTimer <= 0f
                && state.RollTimer <= 0f
                && !state.IsActing
                && state.IsGrounded;
        }

        public static bool CanStartPowerSlide(MovementState state, bool allowPowerSlide)
        {
            return state != null
                && allowPowerSlide
                && !state.IsPowerSliding
                && state.PowerSlideTimer <= 0f
                && !state.IsActing
                && state.IsGrounded
                && state.IsSprinting;
        }

        public static bool CanStartDash(Motor2DState state, bool movementEnabled, bool dashEnabled, bool hasDirection)
        {
            return state != null
                && movementEnabled
                && dashEnabled
                && hasDirection
                && !state.IsDead
                && !state.IsDashing
                && state.DashCooldownTimer <= 0f;
        }

        public PawnLocomotionState ProjectFrom(MovementState state, bool movementEnabled = true, bool isDead = false)
        {
            PawnLocomotionState projected = Resolve(state, movementEnabled, isDead);
            TryTransitionTo(projected);
            return CurrentState;
        }

        public PawnLocomotionState ProjectFrom(Motor2DState state, bool movementEnabled = true, bool isGrounded = true)
        {
            PawnLocomotionState projected = Resolve(state, movementEnabled, isGrounded);
            TryTransitionTo(projected);
            return CurrentState;
        }

        public static PawnLocomotionState Resolve(MovementState state, bool movementEnabled = true, bool isDead = false)
        {
            if (isDead)
                return PawnLocomotionState.Dead;
            if (!movementEnabled)
                return PawnLocomotionState.Disabled;
            if (state == null)
                return PawnLocomotionState.Disabled;
            if (state.IsClimbing)
                return PawnLocomotionState.Climbing;
            if (state.IsHanging)
                return PawnLocomotionState.Hanging;
            if (state.IsDodging || state.IsPowerSliding)
                return PawnLocomotionState.Dashing;
            if (state.IsWallSliding)
                return PawnLocomotionState.WallSliding;
            if (state.IsSliding)
                return PawnLocomotionState.Sliding;

            return state.IsGrounded ? PawnLocomotionState.Grounded : PawnLocomotionState.Airborne;
        }

        public static PawnLocomotionState Resolve(Motor2DState state, bool movementEnabled = true, bool isGrounded = true)
        {
            if (state == null)
                return PawnLocomotionState.Disabled;
            if (state.IsDead)
                return PawnLocomotionState.Dead;
            if (!movementEnabled)
                return PawnLocomotionState.Disabled;
            if (state.IsDashing)
                return PawnLocomotionState.Dashing;

            return isGrounded ? PawnLocomotionState.Grounded : PawnLocomotionState.Airborne;
        }
    }
}
