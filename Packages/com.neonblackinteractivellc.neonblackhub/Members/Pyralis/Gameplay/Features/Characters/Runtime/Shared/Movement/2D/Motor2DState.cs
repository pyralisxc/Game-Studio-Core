using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    /// <summary>
    /// Mutable runtime state for <see cref="Motor2DModel"/>.
    /// Read by the MonoBehaviour layer to drive rendering, audio, and physics.
    /// Never written to directly from outside the model; use the model's API methods.
    /// </summary>
    public sealed class Motor2DState
    {
        /// <summary>Smoothed world-space velocity returned by the last <c>Tick</c>.</summary>
        public Vector2 CurrentVelocity = Vector2.zero;
        public bool    IsDashing         = false;
        public Vector2 DashDir           = Vector2.zero;
        public float   DashTimer         = 0f;
        public float   DashCooldownTimer = 0f;

        public bool IsDead = false;

        /// <summary>True on the frame a dash is successfully started.</summary>
        public bool TriggerDashStarted = false;
    }
}
