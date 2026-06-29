namespace NeonBlack.Gameplay.Core.Contracts
{
    public interface IActorCombatModifierReceiver
    {
        void SetOutgoingDamageMultiplier(float multiplier);
        void SetOutgoingKnockbackMultiplier(float multiplier);
    }
}
