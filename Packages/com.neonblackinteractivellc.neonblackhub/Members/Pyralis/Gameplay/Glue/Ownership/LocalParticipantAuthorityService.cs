using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Contracts.Networking;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Glue.Ownership
{
    /// <summary>
    /// Default local authority model for offline and same-machine sessions.
    /// </summary>
    [AuthoringContract(
        Category = "Networking",
        CapabilityPath = "Core Setup/Participants/Local Authority Service",
        Surface = AuthoringSurface.Service,
        Summary = "Provides the local-only authority model for participants.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/authority",
        RequiredInterfaces = new[] { typeof(IParticipantAuthorityService) },
        SetupSteps = new[] { "Register through the scene/session composition root when an offline authority service is needed." },
        SuccessChecks = new[] { "Verify that local participants are treated as locally controlled in an offline or same-machine session." },
        Tags = new[] { "capability:Networking" },
        Selectable = false
    )]
    public sealed class LocalParticipantAuthorityService : IParticipantAuthorityService
    {
        public bool IsLocalParticipant(ParticipantAuthorityRequest request)
        {
            return true;
        }

        public ulong ResolveOwnerClientId(ParticipantAuthorityRequest request)
        {
            return 0UL;
        }
    }
}
