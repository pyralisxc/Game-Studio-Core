using UnityEngine;

namespace NeonBlack.Gameplay.Core.Types.Input
{
    /// <summary>
    /// Per-frame actor command data captured from a player, AI, network packet, or scripted driver.
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

        public int WeaponCycleDelta;
    }
}
