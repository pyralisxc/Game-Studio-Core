namespace NeonBlack.Gameplay.Core.Contracts
{
    /// <summary>
    /// Basic lifecycle contract for bootstrapped runtime services.
    /// </summary>
    public interface IGameService
    {
        void Initialize();

        void Shutdown();
    }

    /// <summary>
    /// Receives hit-pause requests from combat, hazards, projectiles, and reactions.
    /// </summary>
    public interface IHitPauseSink
    {
        void Freeze(float duration);
    }

    /// <summary>
    /// Receives camera-shake requests without requiring callers to know the camera implementation.
    /// </summary>
    public interface ICameraShakeSink
    {
        void Shake(float intensity, float duration);
    }
}
