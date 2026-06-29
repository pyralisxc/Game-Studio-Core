namespace NeonBlack.Gameplay.Core.Contracts
{
    public interface IActorCombatMovementState : IFacingDirectionProvider
    {
        bool IsGrounded { get; }
        bool IsAirborne { get; }
        bool IsActing { get; set; }
        void ResetMoveToIdle();
    }
}
