using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Glue.Participants;
using NeonBlack.Gameplay.Glue.Session;
using NeonBlack.Gameplay.Glue.Spawning;
using NeonBlack.Gameplay.Modules.Character;
using Unity.Netcode;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Networking.Participants
{
    /// <summary>
    /// Drop-in replacement for <see cref="ParticipantRosterService"/> in online sessions.
    /// Resolves the NGO <see cref="Unity.Netcode.NetworkManager.LocalClientId"/> for participant ownership.
    /// </summary>
    [AuthoringContract(
        Category = "Networking",
        CapabilityPath = "Networking/Participants/Networked Participant Roster Service",
        Surface = AuthoringSurface.Goal,
        Summary = "Drop-in replacement for ParticipantRosterService in online sessions. Resolves NGO Client IDs.",
        SuccessChecks = new[] { "The participant roster correctly reflects the NetworkManager.LocalClientId for the local player." },
        RoleTags = new[] { "IntentRouteEssential", "NetworkRouteSupport" },
        Tags = new[] { "capability:Networking", "runtime:Networking" }
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
