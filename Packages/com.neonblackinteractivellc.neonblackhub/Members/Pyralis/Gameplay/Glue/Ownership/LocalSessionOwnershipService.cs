using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Contracts.Networking;

namespace NeonBlack.Gameplay.Glue.Ownership
{
    /// <summary>
    /// Default local/offline ownership policy used until an online backend overrides it.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Networking,
        Relevance = "Provides the local-only ownership model for game sessions, used in offline modes.",
        Axioms = AuthoringWorldAxiom.None,
        RequiredInterfaces = new[] { typeof(ISessionOwnershipService) },
        Proof = "Start a local session and verify the server-authoritative flag is false.",
        NativeSetup = new[] { "Register through the scene/session composition root when offline session ownership is needed." },
        ExpertAdvice = "Enforces that the local machine owns the game world for offline and local split-screen routes. Use a networked ownership service when synchronization authority belongs to an online backend.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/session",
        CapabilityPath = "Core Setup/Session/Local Ownership Service",
        Surface = AuthoringContractSurface.Service
    )]
    public sealed class LocalSessionOwnershipService : ISessionOwnershipService
{
        public bool IsServerAuthoritative => false;

        public void TryStartSessionHost()
        {
            // Local/offline sessions do not need an explicit host bootstrap.
        }
    }
}
