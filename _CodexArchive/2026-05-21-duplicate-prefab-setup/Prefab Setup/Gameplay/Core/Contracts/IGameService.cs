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
}
