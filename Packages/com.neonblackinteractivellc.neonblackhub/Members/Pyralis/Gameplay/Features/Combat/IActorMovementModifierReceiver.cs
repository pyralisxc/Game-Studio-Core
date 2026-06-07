namespace NeonBlack.Gameplay.Features.Combat
{
    public interface IActorMovementModifierReceiver
    {
        void SetStatusMoveSpeedMultiplier(float multiplier);
        void SetStatusActionLock(bool locked);
    }
}
