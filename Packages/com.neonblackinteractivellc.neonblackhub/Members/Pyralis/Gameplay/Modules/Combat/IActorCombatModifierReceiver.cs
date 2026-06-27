namespace NeonBlack.Gameplay.Modules.Combat
{
    public interface IActorCombatModifierReceiver
    {
        void SetOutgoingDamageMultiplier(float multiplier);
        void SetOutgoingKnockbackMultiplier(float multiplier);
    }
}
