using NeonBlack.Gameplay.Core.Enums;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Shared movement authoring profile for pawn composition.
    /// </summary>
    [CreateAssetMenu(menuName = "NeonBlack/Gameplay/Profiles/Pawn Movement Profile", fileName = "PawnMovementProfile")]
    public class PawnMovementProfile : ScriptableObject
    {
        public MovementMode movementMode = MovementMode.ThreeD;
        public float walkSpeed = 5f;
        public float sprintSpeed = 10f;
        public float crouchSpeed = 2.5f;
        public float acceleration = 20f;
        public float deceleration = 25f;
        public bool useCharacterController = true;
        public bool use2DPhysics = false;
        public bool allowDepthMovement = true;
        public bool allowScreenWrap = false;
        public float depthSpeedMultiplier = 0.6f;

        public void Sanitize()
        {
            walkSpeed = Mathf.Max(0f, walkSpeed);
            sprintSpeed = Mathf.Max(0f, sprintSpeed);
            crouchSpeed = Mathf.Max(0f, crouchSpeed);
            acceleration = Mathf.Max(0f, acceleration);
            deceleration = Mathf.Max(0f, deceleration);
            depthSpeedMultiplier = Mathf.Clamp(depthSpeedMultiplier, 0.1f, 1f);
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
