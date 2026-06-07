using UnityEngine;

namespace NeonBlack.Gameplay.Characters
{
    /// <summary>
    /// Runtime locomotion contract — separate from the profiling-only IPawnMotor.
    /// Implement this interface on any MonoBehaviour that owns movement execution
    /// so external systems (AI, network) can drive movement without knowing the
    /// concrete controller type.
    /// </summary>
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
        void Move(Vector2 input);

        /// <summary>Request a jump. No-op if conditions are not met (not grounded, no jumps remaining, etc.).</summary>
        void Jump();

        /// <summary>Enable or disable movement processing entirely (e.g. during cinematic or stun).</summary>
        void SetMovementEnabled(bool enabled);
    }
}
