namespace NeonBlack.Gameplay.Core.Contracts
{
    public interface IEnemyActorState
    {
        bool IsPatrolling { get; }
        bool IsChasing { get; }
        bool IsAttacking { get; }
    }
}
