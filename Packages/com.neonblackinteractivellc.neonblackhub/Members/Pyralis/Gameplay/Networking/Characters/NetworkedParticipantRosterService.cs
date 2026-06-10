using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Characters;
using Unity.Netcode;

namespace NeonBlack.Gameplay.Networking.Participants
{
    /// <summary>
    /// Drop-in replacement for <see cref="ParticipantRosterService"/> in online sessions.
    /// Resolves the NGO <see cref="Unity.Netcode.NetworkManager.LocalClientId"/> for participant ownership.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Networking,
        Relevance = "Drop-in replacement for ParticipantRosterService in online sessions. Resolves NGO Client IDs.",
        FirstProof = "The participant roster correctly reflects the NetworkManager.LocalClientId for the local player."
    )]
    public class NetworkedParticipantRosterService : ParticipantRosterService
    {
        protected override ulong ResolveOwnerClientId()
        {
            return NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening
                ? NetworkManager.Singleton.LocalClientId
                : 0UL;
        }
    }
}
