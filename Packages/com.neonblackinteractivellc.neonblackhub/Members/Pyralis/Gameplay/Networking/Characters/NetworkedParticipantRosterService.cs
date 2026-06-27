using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Glue.Participants;
using NeonBlack.Gameplay.Glue.Session;
using NeonBlack.Gameplay.Glue.Spawning;
using NeonBlack.Gameplay.Modules.Character;
using Unity.Netcode;

namespace NeonBlack.Gameplay.Networking.Participants
{
    /// <summary>
    /// Drop-in replacement for <see cref="ParticipantRosterService"/> in online sessions.
    /// Resolves the NGO <see cref="Unity.Netcode.NetworkManager.LocalClientId"/> for participant ownership.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Networking,
        RuntimeFamilies = new[] { RuntimeCapabilityFamily.Networking },
        CapabilityPath = "Networking/Participants/Networked Participant Roster Service",
        Relevance = "Drop-in replacement for ParticipantRosterService in online sessions. Resolves NGO Client IDs.",
        RoleTags = new[] { AuthoringContractRoleTags.IntentRouteEssential, AuthoringContractRoleTags.NetworkRouteSupport },
        Proof = "The participant roster correctly reflects the NetworkManager.LocalClientId for the local player."
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
