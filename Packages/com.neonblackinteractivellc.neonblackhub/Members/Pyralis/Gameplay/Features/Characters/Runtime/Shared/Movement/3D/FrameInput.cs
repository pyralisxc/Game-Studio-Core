using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    /// <summary>
    /// All player-facing input values captured for a single Update frame.
    /// Produced by <see cref="Pawn3DInputModule.CollectFrameInput"/> and passed by
    /// <see cref="Motor3D"/> to every sub-module that needs per-frame input data.
    /// Plain data struct with no MonoBehaviour dependency.
    /// </summary>
    public struct FrameInput
    {
        public Vector2 Move;
        public Vector2 Look;

        public bool SprintHeld;

        public bool JumpPressed;
        public bool JumpReleased;
        public bool CrouchPressed;
        public bool CrouchReleased;
        public bool RollPressed;
        public bool AttackPressed;
        public bool KickPressed;
        public bool InteractPressed;
        public bool BlockPressed;
        public bool BlockReleased;
        public bool LookAroundPressed;
        public bool LookAroundReleased;

        /// <summary>Weapon cycle delta: -1 = previous, 0 = no change, +1 = next.</summary>
        public int WeaponCycleDelta;
    }
}
