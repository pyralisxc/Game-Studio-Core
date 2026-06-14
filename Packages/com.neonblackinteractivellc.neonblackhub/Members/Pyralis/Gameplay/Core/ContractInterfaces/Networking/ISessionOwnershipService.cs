namespace NeonBlack.Gameplay.Core.Contracts.Networking
{
    /// <summary>
    /// Session ownership hook used by bootstrappers and runtime services.
    /// </summary>
    public interface ISessionOwnershipService
    {
        bool IsServerAuthoritative { get; }
        void TryStartSessionHost();
    }
}
