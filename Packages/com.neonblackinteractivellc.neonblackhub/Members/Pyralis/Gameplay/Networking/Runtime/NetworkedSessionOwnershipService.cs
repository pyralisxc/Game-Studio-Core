using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Contracts.Networking;
using Unity.Netcode;

namespace NeonBlack.Gameplay.Networking.Runtime
{
    /// <summary>
    /// NGO-backed session ownership policy used by networked Pyralis sessions.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Networking,
        Relevance = "NGO-backed session ownership policy used by networked Pyralis sessions.",
        FirstProof = "StartHost correctly triggers the NGO NetworkManager to begin listening.",
        ExpertAdvice = "This service enforces server-authoritative logic for the session lifecycle."
    )]
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
