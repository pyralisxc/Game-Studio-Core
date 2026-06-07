using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Features.Characters
{
    /// <summary>
    /// Minimal motor contract that combat and traversal sub-systems can query
    /// without taking a hard dependency on a specific motor implementation.
    /// Implemented by <see cref="Motor3D"/>.
    /// </summary>
    public interface ICharacterMotorState : IFacingDirectionProvider
    {
        /// <summary>True when the pawn is standing on a ground surface.</summary>
        bool IsGrounded  { get; }

        /// <summary>True when the pawn is airborne: not grounded, or moving upward through a jump.</summary>
        bool IsAirborne  { get; }

        /// <summary>
        /// Shared action lock. Set <c>true</c> by attacks, dodge, and climb while those actions run.
        /// Cleared by the motor once the Animator leaves all combat-tagged states.
        /// </summary>
        bool IsActing    { get; set; }

        /// <summary>
        /// Reset the MoveToIdle animator trigger, if the Animator has that parameter.
        /// Called by <see cref="PawnCombatBehaviour"/> before firing attack triggers.
        /// </summary>
        void ResetMoveToIdle();
    }
}
