namespace NeonBlack.Gameplay.Features.Combat
{
    public interface IActorCombatModifierReceiver
    {
        void SetOutgoingDamageMultiplier(float multiplier);
        void SetOutgoingKnockbackMultiplier(float multiplier);
    }
}
