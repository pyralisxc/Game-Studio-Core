namespace NeonBlack.Gameplay.Features.Characters
{
    /// <summary>
    /// Lightweight combat input target for 2D pawns.
    /// </summary>
    public interface IPawnCombatInputReceiver2D
    {
        void HandlePrimaryAttackInput();
        void HandleSecondaryAttackInput();
    }
}
