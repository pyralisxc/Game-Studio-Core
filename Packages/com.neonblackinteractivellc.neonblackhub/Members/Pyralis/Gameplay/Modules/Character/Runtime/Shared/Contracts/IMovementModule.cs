using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Character
{
    /// <summary>
    /// Runtime locomotion contract — separate from the profiling-only IPawnMotor.
    /// Implement this interface on any MonoBehaviour that owns movement execution
    /// so external systems (AI, network) can drive movement without knowing the
    /// concrete controller type.
    /// </summary>
    [AuthoringContract(
        Category = "Movement",
        Surface = AuthoringSurface.Goal,
        Summary = "Calculates actor translation and velocity based on input and physical rules.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/movement",
        RequiredFields = new[] { nameof(IMovementModule.MoveSpeed), nameof(IMovementModule.IsGrounded) },
        SetupSteps = new[] { "Implement interface in a movement component" },
        SuccessChecks = new[] { "Call Move and verify the character's world position changes." },
        Tags = new[] { "capability:Movement", "axiom:Dimensions2D", "axiom:Dimensions3D" }
    )]
public interface IMovementModule
{
        /// <summary>Current horizontal move speed in world-units per second.</summary>
        float MoveSpeed { get; }

        /// <summary>True when the pawn is in contact with the ground.</summary>
        bool IsGrounded { get; }

        /// <summary>
        /// Drive movement for this frame.
        /// </summary>
        /// <param name="input">Normalised X/Y input (X = left/right, Y = forward/back).</param>
        void Move(Vector2 input, float deltaTime);

        /// <summary>Request a jump. No-op if conditions are not met (not grounded, no jumps remaining, etc.).</summary>
        void Jump(float deltaTime);

        /// <summary>Enable or disable movement processing entirely (e.g. during cinematic or stun).</summary>
        void SetMovementEnabled(bool enabled);
    }
}
