using NeonBlack.Gameplay.Core.Contracts.Networking;
using Unity.Netcode;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Networking.Runtime
{
    /// <summary>
    /// Resolves participant authority from the active NGO local client.
    /// </summary>
    public sealed class NetworkedParticipantAuthorityService : IParticipantAuthorityService
    {
        public ulong ResolveOwnerClientId(PlayerInput playerInput, int seatIndex)
        {
            NetworkManager networkManager = NetworkManager.Singleton;
            return networkManager != null && networkManager.IsListening
                ? networkManager.LocalClientId
                : NetworkManager.ServerClientId;
        }

        public bool IsLocalParticipant(PlayerInput playerInput, int seatIndex)
        {
            NetworkManager networkManager = NetworkManager.Singleton;
            if (networkManager == null || !networkManager.IsListening)
                return true;

            ulong ownerClientId = ResolveOwnerClientId(playerInput, seatIndex);
            return ownerClientId == networkManager.LocalClientId;
        }
    }
}
