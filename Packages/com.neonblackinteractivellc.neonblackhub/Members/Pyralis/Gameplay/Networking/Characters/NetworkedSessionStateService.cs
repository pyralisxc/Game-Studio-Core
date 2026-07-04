using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Glue.Session;
using NeonBlack.Gameplay.Glue.Spawning;
using NeonBlack.Gameplay.Modules.Character;
using Unity.Netcode;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Networking.Participants
{
    /// <summary>
    /// Drop-in replacement for <see cref="SessionStateService"/> in online sessions.
    /// Starts the configured NGO role when <see cref="NeonBlack.Gameplay.Data.Definitions.SessionDefinition.autoStartHost"/> is true.
    /// </summary>
    [AuthoringContract(
        Category = "Networking",
        CapabilityPath = "Networking/Session/Networked Session State Service",
        Surface = AuthoringSurface.Goal,
        Summary = "Drop-in replacement for SessionStateService in online sessions. Handles NGO role startup.",
        SuccessChecks = new[] { "Entering the scene correctly triggers the NGO role (Host/Client/Server) defined in the SessionDefinition." },
        RoleTags = new[] { "IntentRouteEssential", "NetworkRouteSupport" },
        Tags = new[] { "capability:Networking", "runtime:Networking" }
    )]
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
