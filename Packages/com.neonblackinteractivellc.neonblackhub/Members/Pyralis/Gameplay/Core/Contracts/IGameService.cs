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
    /// Pure contract for time management. Deprecates static TimeManager access.
    /// </summary>
    public interface ITimeManager : IHitPauseSink
    {
    }

    /// <summary>
    /// Pure contract for camera shake. Deprecates static CameraShake access.
    /// </summary>
    public interface ICameraShake : ICameraShakeSink
    {
    }

    /// <summary>
    /// Receives camera-shake requests without requiring callers to know the camera implementation.
    /// </summary>
    public interface ICameraShakeSink
    {
        void Shake(float intensity, float duration);
    }
}
