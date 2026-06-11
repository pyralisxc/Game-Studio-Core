using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Enums;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Shared movement authoring profile for pawn composition.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Movement, 
        Priority = AuthoringPriority.AuxiliaryDefault,
        Lane = "Movement",
        Relevance = "Defines the movement feel, speed, acceleration, and damping for a pawn archetype.",
        AssignmentFields = new[] { nameof(walkSpeed), nameof(acceleration), nameof(dashSpeed), nameof(movementMode), nameof(useCharacterController) },
        FirstProof = "Move the pawn in play mode and verify speed feel.",
        ExpertAdvice = "The movement profile is your 'steering wheel'. It defines the responsiveness and agility of your actor. For 2D games, set 'Use 2D Physics' to enable Rigidbody2D interaction.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/movement",
        NativeSetup = new[] { "Create asset in Project window.", "Assign to a PawnDefinition." }
    )]
[CreateAssetMenu(menuName = "NeonBlack/Profiles/Pawn Movement Profile", fileName = "PawnMovementProfile", order = -60)]
    public class PawnMovementProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (walkSpeed < 0f) yield return "Walk Speed cannot be negative.";
            if (acceleration < 0f) yield return "Acceleration cannot be negative.";
        }

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

        [Header("2D Dash")]
        [Tooltip("Whether 2D pawn movement can dash when a runtime input source or script calls TryDash. The InputProfile still decides which hardware action, if any, triggers it.")]
        public bool allow2DDash = true;
        public float dashSpeed = 12f;
        [Range(0.05f, 0.5f)] public float dashDuration = 0.15f;
        [Range(0.1f, 3f)] public float dashCooldown = 0.8f;

        [Header("2D Side View Jump")]
        [Tooltip("Enable for side-view/platformer 2D pawns that should move horizontally and jump with Rigidbody2D gravity. Leave off for top-down 2D pawns.")]
        public bool allow2DJump = false;
        [Tooltip("Initial upward velocity applied when a grounded 2D side-view pawn jumps.")]
        public float jumpVelocity2D = 8f;
        [Tooltip("Gravity scale applied to Rigidbody2D while 2D side-view jump is enabled.")]
        public float gravityScale2D = 3f;

        public void Sanitize()
        {
            walkSpeed = Mathf.Max(0f, walkSpeed);
            sprintSpeed = Mathf.Max(0f, sprintSpeed);
            crouchSpeed = Mathf.Max(0f, crouchSpeed);
            acceleration = Mathf.Max(0f, acceleration);
            deceleration = Mathf.Max(0f, deceleration);
            depthSpeedMultiplier = Mathf.Clamp(depthSpeedMultiplier, 0.1f, 1f);
            dashSpeed = Mathf.Max(0f, dashSpeed);
            dashDuration = Mathf.Clamp(dashDuration, 0.05f, 0.5f);
            dashCooldown = Mathf.Clamp(dashCooldown, 0.1f, 3f);
            jumpVelocity2D = Mathf.Max(0f, jumpVelocity2D);
            gravityScale2D = Mathf.Max(0f, gravityScale2D);
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
