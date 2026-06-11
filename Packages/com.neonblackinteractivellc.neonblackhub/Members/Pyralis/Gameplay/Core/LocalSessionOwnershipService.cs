using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Contracts.Networking;

namespace NeonBlack.Gameplay.Core.Runtime
{
    /// <summary>
    /// Default local/offline ownership policy used until an online backend overrides it.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Networking,
        Relevance = "Provides the local-only ownership model for game sessions, used in offline modes.",
        Axioms = AuthoringWorldAxiom.None,
        RequiredInterfaces = new[] { typeof(ISessionOwnershipService) },
        FirstProof = "Start a local session and verify the server-authoritative flag is false.",
        NativeSetup = new[] { "Add to Scene Service Registry or Bootstrap." },
        ExpertAdvice = "Enforces that the local machine owns the game world. Use this for single-player or local split-screen prototypes. It effectively bypasses network synchronization checks.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/session"
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
