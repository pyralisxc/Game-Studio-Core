using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Shared traversal authoring profile for jumps, dodge, and climb-like features.
    /// </summary>
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Pawn Traversal Profile", fileName = "PawnTraversalProfile", order = -50)]
    public class PawnTraversalProfile : ScriptableObject
    {
        public bool allowJump = true;
        public bool allowClimb = false;
        public bool allowHang = false;
        public bool allowDodge = false;
        public bool allowCrouch = true;
        public float jumpHeight = 3f;
        public float gravity = -20f;
        public float dodgeDistance = 3f;
        public float dodgeDuration = 0.4f;
        public float dodgeCooldown = 0.8f;
        public float climbCooldown = 1.2f;

        public void Sanitize()
        {
            jumpHeight = Mathf.Max(0f, jumpHeight);
            dodgeDistance = Mathf.Max(0f, dodgeDistance);
            dodgeDuration = Mathf.Max(0.01f, dodgeDuration);
            dodgeCooldown = Mathf.Max(0f, dodgeCooldown);
            climbCooldown = Mathf.Max(0f, climbCooldown);
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
