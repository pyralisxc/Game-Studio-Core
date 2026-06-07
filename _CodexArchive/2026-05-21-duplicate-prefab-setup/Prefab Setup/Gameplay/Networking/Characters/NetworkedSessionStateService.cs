using NeonBlack.Gameplay.Characters;
using Unity.Netcode;

namespace NeonBlack.Gameplay.Networking.Participants
{
    /// <summary>
    /// Drop-in replacement for <see cref="SessionStateService"/> in online sessions.
    /// Starts the NGO host when <see cref="NeonBlack.Gameplay.Data.Definitions.SessionDefinition.autoStartHost"/> is true.
    /// </summary>
    public class NetworkedSessionStateService : SessionStateService
    {
        protected override void TryStartHostIfNeeded()
        {
            if (ActiveSessionDefinition == null || !ActiveSessionDefinition.autoStartHost)
                return;
            if (NetworkManager.Singleton == null || NetworkManager.Singleton.IsListening)
                return;

            NetworkManager.Singleton.StartHost();
        }
    }
}
