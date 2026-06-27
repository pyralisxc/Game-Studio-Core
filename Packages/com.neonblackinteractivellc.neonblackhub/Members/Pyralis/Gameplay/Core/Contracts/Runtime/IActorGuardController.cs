namespace NeonBlack.Gameplay.Core.Contracts
{
    public interface IActorGuardController
    {
        bool IsGuarding { get; }
        float BlockDamageReduction { get; }
        float BlockFrontalAngle { get; }
        void BeginGuard();
        void EndGuard();
    }
}
