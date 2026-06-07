using NeonBlack.Gameplay.Core.Contracts.Networking;

namespace NeonBlack.Gameplay.Networking.Runtime
{
    /// <summary>
    /// Default local/offline ownership policy used until an online backend overrides it.
    /// </summary>
    public sealed class LocalSessionOwnershipService : ISessionOwnershipService
    {
        public bool IsServerAuthoritative => false;
        public void TryStartSessionHost()
        {
            // Local/offline sessions do not need an explicit host bootstrap.
        }
    }
}
