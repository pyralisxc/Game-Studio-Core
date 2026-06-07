using NeonBlack.Gameplay.Characters;
using Unity.Netcode;

namespace NeonBlack.Gameplay.Networking.Participants
{
    /// <summary>
    /// Drop-in replacement for <see cref="SessionStateService"/> in online sessions.
    /// Starts the configured NGO role when <see cref="NeonBlack.Gameplay.Data.Definitions.SessionDefinition.autoStartHost"/> is true.
    /// </summary>
    public class NetworkedSessionStateService : SessionStateService
    {
        protected override void TryStartHostIfNeeded()
        {
            if (ActiveSessionDefinition == null || !ActiveSessionDefinition.autoStartHost)
                return;
            if (NetworkManager.Singleton == null || NetworkManager.Singleton.IsListening)
                return;

            switch (ActiveSessionDefinition.networkMode)
            {
                case Data.Definitions.GameplayNetworkMode.NetcodeClient:
                    NetworkManager.Singleton.StartClient();
                    break;
                case Data.Definitions.GameplayNetworkMode.NetcodeServer:
                    NetworkManager.Singleton.StartServer();
                    break;
                case Data.Definitions.GameplayNetworkMode.NetcodeHost:
                    NetworkManager.Singleton.StartHost();
                    break;
            }
        }
    }
}
