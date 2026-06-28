using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Contracts.Networking;
using Unity.Netcode;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Networking.Runtime
{
    /// <summary>
    /// NGO-backed session ownership policy used by networked Pyralis sessions.
    /// </summary>
    [AuthoringContract(
        Category = "Networking",
        CapabilityPath = "Networking/Session/Session Ownership Service",
        Surface = AuthoringSurface.Goal,
        Summary = "NGO-backed session ownership policy used by networked Pyralis sessions.",
        RequiredInterfaceNames = new[] { nameof(ISessionOwnershipService) },
        SetupSteps = new[]
        {
            "Add a Unity Netcode NetworkManager to the scene.",
            "Assign transport and session ownership references through the Inspector.",
            "Enter Play Mode and start host from the networked session surface."
        },
        SuccessChecks = new[] { "StartHost correctly triggers the NGO NetworkManager to begin listening." },
        RoleTags = new[] { "IntentRouteEssential", "NetworkRouteSupport" },
        Tags = new[] { "capability:Networking", "runtime:Networking" }
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
