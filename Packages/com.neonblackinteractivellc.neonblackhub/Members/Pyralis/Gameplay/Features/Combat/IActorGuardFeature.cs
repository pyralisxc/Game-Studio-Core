namespace NeonBlack.Gameplay.Features.Combat
{
    public interface IActorGuardFeature
    {
        bool IsGuarding { get; }
        float BlockDamageReduction { get; }
        float BlockFrontalAngle { get; }
        void BeginGuard();
        void EndGuard();
    }
}
