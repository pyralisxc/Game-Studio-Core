namespace NeonBlack.Gameplay.Core.Contracts
{
    public interface IActorMovementModifierReceiver
    {
        void SetStatusMoveSpeedMultiplier(float multiplier);
        void SetStatusActionLock(bool locked);
    }
}
