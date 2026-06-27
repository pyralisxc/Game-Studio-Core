namespace NeonBlack.Gameplay.Modules.Character
{
    public interface IPawnCombatMovementContext
    {
        float AttackTimer { get; }
        float KickTimer { get; }
        float AttackMoveMultiplier { get; }
        float AerialAttackMoveMultiplier { get; }
    }
}
