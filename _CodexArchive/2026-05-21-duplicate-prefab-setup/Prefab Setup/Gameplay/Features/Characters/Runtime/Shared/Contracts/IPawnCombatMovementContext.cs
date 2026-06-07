namespace NeonBlack.Gameplay.Characters
{
    public interface IPawnCombatMovementContext
    {
        float AttackTimer { get; }
        float KickTimer { get; }
        float AttackMoveMultiplier { get; }
        float AerialAttackMoveMultiplier { get; }
    }
}
