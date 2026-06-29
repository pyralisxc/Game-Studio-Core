namespace NeonBlack.Gameplay.Modules.Combat
{
    public interface IActorHealthModifierReceiver
    {
        void SetIncomingDamageMultiplier(float multiplier);
        void SetRegenRateMultiplier(float multiplier);
    }
}
