namespace NeonBlack.Gameplay.Core.Contracts
{
    public interface IActorCombatMovementInfluence
    {
        float AttackTimer { get; }
        float KickTimer { get; }
        float AttackMoveMultiplier { get; }
        float AerialAttackMoveMultiplier { get; }
    }
}
