using NeonBlack.Gameplay.Characters;
using Unity.Netcode;

namespace NeonBlack.Gameplay.Networking.Participants
{
    /// <summary>
    /// Drop-in replacement for <see cref="ParticipantRosterService"/> in online sessions.
    /// Resolves the NGO <see cref="Unity.Netcode.NetworkManager.LocalClientId"/> for participant ownership.
    /// </summary>
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
