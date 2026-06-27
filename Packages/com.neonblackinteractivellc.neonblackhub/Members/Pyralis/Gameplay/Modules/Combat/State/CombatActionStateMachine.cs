namespace NeonBlack.Gameplay.Modules.Combat
{
    public sealed class CombatActionStateMachine
    {
        public CombatActionState CurrentState { get; private set; } = CombatActionState.Idle;

        public bool TryTransitionTo(CombatActionState nextState)
        {
            if (!CanTransitionTo(nextState))
                return false;

            CurrentState = nextState;
            return true;
        }

        public bool CanTransitionTo(CombatActionState nextState)
        {
            if (CurrentState == CombatActionState.Idle)
                return true;
            if (nextState == CombatActionState.Idle)
                return true;
            if (nextState == CurrentState)
                return true;

            return CurrentState switch
            {
                CombatActionState.Windup => nextState == CombatActionState.Active || nextState == CombatActionState.Cooldown,
                CombatActionState.Active => nextState == CombatActionState.Recovery || nextState == CombatActionState.Cooldown,
                CombatActionState.Recovery => nextState == CombatActionState.Cooldown,
                CombatActionState.Cooldown => nextState == CombatActionState.Idle,
                _ => false,
            };
        }

        public bool CanStartAction => CurrentState == CombatActionState.Idle;

        public static bool CanStartActionFrom(bool isActionLocked, float cooldownTimer)
        {
            return !isActionLocked && cooldownTimer <= 0f;
        }

        public void Reset()
        {
            CurrentState = CombatActionState.Idle;
        }

        public CombatActionState ProjectFrom(bool isActionLocked, float activeTimer, float cooldownTimer)
        {
            CombatActionState projected = Resolve(isActionLocked, activeTimer, cooldownTimer);
            TryTransitionTo(projected);
            return CurrentState;
        }

        public static CombatActionState Resolve(bool isActionLocked, float activeTimer, float cooldownTimer)
        {
            if (isActionLocked)
                return CombatActionState.Active;
            if (activeTimer > 0f)
                return CombatActionState.Recovery;
            if (cooldownTimer > 0f)
                return CombatActionState.Cooldown;

            return CombatActionState.Idle;
        }
    }
}
