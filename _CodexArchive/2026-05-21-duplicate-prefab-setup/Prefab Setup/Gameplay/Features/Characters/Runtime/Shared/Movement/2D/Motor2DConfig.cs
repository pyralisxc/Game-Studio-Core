namespace NeonBlack.Gameplay.Features.Characters
{
    /// <summary>
    /// Immutable tuning parameters for <see cref="Motor2DModel"/>.
    /// Fill via <c>Motor2D.BuildMotorConfig()</c> and pass to <c>Motor2DModel.Configure()</c>.
    /// Re-configure whenever a PawnMovementProfile is applied at runtime.
    /// </summary>
    public struct Motor2DConfig
    {
        // â”€â”€ Movement â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        public float MoveSpeed;
        public float Acceleration;
        public float Deceleration;
        /// <summary>
        /// Normalised stop-snap threshold (0 = drift, 1 = instant).
        /// Velocity snaps to zero when <c>magnitude &lt; StopThreshold * MoveSpeed</c>.
        /// </summary>
        public float StopThreshold;

        // â”€â”€ Dash â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        public bool  DashEnabled;
        public float DashSpeed;
        public float DashDuration;
        public float DashCooldown;
    }
}
