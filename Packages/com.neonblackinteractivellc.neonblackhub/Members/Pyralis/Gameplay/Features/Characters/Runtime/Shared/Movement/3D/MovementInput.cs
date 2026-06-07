using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    /// <summary>
    /// All input and combat-context data <see cref="BrawlerMovementModel"/> needs for one Update frame.
    /// Filled by the MonoBehaviour layer from Unity input, scene camera context, and combat state.
    /// </summary>
    public struct MovementInput
    {
        /// <summary>Raw axis input (x = horizontal, y = depth / vertical).</summary>
        public Vector2 Move;

        /// <summary>Planar world-space movement direction resolved from input and movement camera.</summary>
        public Vector3 MoveWorld;

        public bool SprintHeld;
        public bool CrouchHeld;

        /// <summary>True only on the frame Jump was pressed.</summary>
        public bool JumpPressed;
        /// <summary>True only on the frame Jump was released.</summary>
        public bool JumpReleased;
        /// <summary>True only on the frame Dodge / Roll was pressed.</summary>
        public bool DodgePressed;

        public bool AttackTimerActive;
        public bool KickTimerActive;
        public float AttackMoveMultiplier;
        public float AerialAttackMoveMultiplier;

        /// <summary>
        /// Camera-right direction flattened to XZ, used for screen-relative facing.
        /// Pass <c>Vector3.right</c> when no camera is available.
        /// </summary>
        public Vector3 CameraRight;
    }
}