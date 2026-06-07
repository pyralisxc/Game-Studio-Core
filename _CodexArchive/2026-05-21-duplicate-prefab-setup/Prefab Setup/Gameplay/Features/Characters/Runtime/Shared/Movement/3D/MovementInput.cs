using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    /// <summary>
    /// All input and combat-context data <see cref="BrawlerMovementModel"/> needs for one Update frame.
    /// Filled by the <see cref="Pawn3DInputModule"/> from Unity InputSystem and PawnCombatBehaviour.
    /// Plain-data struct â€” no MonoBehaviour dependency.
    /// </summary>
    public struct MovementInput
    {
        /// <summary>Raw axis input (x = horizontal, y = depth / vertical).</summary>
        public Vector2 Move;
        public bool    SprintHeld;
        public bool    CrouchHeld;

        /// <summary>True only on the frame Jump was pressed (performed callback).</summary>
        public bool JumpPressed;
        /// <summary>True only on the frame Jump was released (canceled callback).</summary>
        public bool JumpReleased;
        /// <summary>True only on the frame Dodge / Roll was pressed.</summary>
        public bool DodgePressed;

        // â”€â”€ Combat slowdown context (from PawnCombatBehaviour) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        public bool  AttackTimerActive;
        public bool  KickTimerActive;
        public float AttackMoveMultiplier;
        public float AerialAttackMoveMultiplier;

        /// <summary>
        /// Camera-right direction flattened to XZ, used for screen-relative facing.
        /// Pass <c>Vector3.right</c> when no camera is available.
        /// </summary>
        public Vector3 CameraRight;
    }
}
