namespace NeonBlack.Gameplay.Features.Combat
{
    public interface IActorHealthModifierReceiver
    {
        void SetIncomingDamageMultiplier(float multiplier);
        void SetRegenRateMultiplier(float multiplier);
    }
}
