using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Contracts.Networking;
using Unity.Netcode;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Networking.Runtime
{
    /// <summary>
    /// Resolves participant authority from the active NGO local client.
    /// </summary>
    [AuthoringContract(
        Category = "Networking",
        CapabilityPath = "Networking/Participants/Participant Authority Service",
        Surface = AuthoringSurface.Goal,
        Summary = "Resolves participant authority from the active Netcode for GameObjects (NGO) local client.",
        RequiredInterfaceNames = new[] { nameof(IParticipantAuthorityService) },
        SetupSteps = new[] { "Register as the participant authority service for networked sessions.", "Use with NetworkManager and Unity PlayerInput seating." },
        SuccessChecks = new[] { "The local client is correctly identified as the owner in a networked session." },
        RoleTags = new[] { "IntentRouteEssential", "NetworkRouteSupport" },
        Tags = new[] { "capability:Networking", "runtime:Networking" }
    )]
    public sealed class NetworkedParticipantAuthorityService : IParticipantAuthorityService
    {
        public ulong ResolveOwnerClientId(ParticipantAuthorityRequest request)
        {
            NetworkManager networkManager = NetworkManager.Singleton;
            return networkManager != null && networkManager.IsListening
                ? networkManager.LocalClientId
                : NetworkManager.ServerClientId;
        }

        public bool IsLocalParticipant(ParticipantAuthorityRequest request)
        {
            NetworkManager networkManager = NetworkManager.Singleton;
            if (networkManager == null || !networkManager.IsListening)
                return true;

            ulong ownerClientId = ResolveOwnerClientId(request);
            return ownerClientId == networkManager.LocalClientId;
        }
    }
}
