using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Contracts.Networking;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Glue.Ownership
{
    /// <summary>
    /// Default local/offline ownership policy used until an online backend overrides it.
    /// </summary>
    [AuthoringContract(
        Category = "Networking",
        CapabilityPath = "Core Setup/Session/Local Ownership Service",
        Surface = AuthoringSurface.Service,
        Summary = "Provides the local-only ownership model for game sessions, used in offline modes.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/session",
        RequiredInterfaces = new[] { typeof(ISessionOwnershipService) },
        SetupSteps = new[] { "Register through the scene/session composition root when offline session ownership is needed." },
        SuccessChecks = new[] { "Start a local session and verify the server-authoritative flag is false." },
        Tags = new[] { "capability:Networking" },
        Selectable = false
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
