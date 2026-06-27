namespace NeonBlack.Gameplay.Core.Contracts
{
    public interface IActorCombatCommandReceiver
    {
        void UpdateCombatTimers();
        void HandleAttack();
        void HandleKick();
        void HandleBlockStart();
        void HandleBlockEnd();
        void CycleWeapon(int direction);
    }
}
