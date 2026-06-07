using NeonBlack.Gameplay.Core.Contracts.Networking;
using Unity.Netcode;

namespace NeonBlack.Gameplay.Networking.Runtime
{
    /// <summary>
    /// NGO-backed session ownership policy used by networked Pyralis sessions.
    /// </summary>
    public sealed class NetworkedSessionOwnershipService : ISessionOwnershipService
    {
        public bool IsServerAuthoritative => true;

        public void TryStartSessionHost()
        {
            NetworkManager networkManager = NetworkManager.Singleton;
            if (networkManager == null || networkManager.IsListening)
                return;

            networkManager.StartHost();
        }
    }
}
