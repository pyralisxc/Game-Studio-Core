using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Contracts.Networking;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Core.Runtime
{
    /// <summary>
    /// Default local authority model for offline and same-machine sessions.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Networking,
        Relevance = "Provides the local-only authority model for participants.",
        Axioms = AuthoringWorldAxiom.None,
        RequiredInterfaces = new[] { typeof(IParticipantAuthorityService) },
        FirstProof = "Verify that local participants are treated as locally controlled in an offline or same-machine session.",
        NativeSetup = new[] { "Register through the scene/session composition root when an offline authority service is needed." },
        ExpertAdvice = "The Local Authority service is a pass-through for offline and same-machine play. It identifies participant input as local. Use a networked authority service when ownership comes from an online backend.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/authority"
    )]
public sealed class LocalParticipantAuthorityService : IParticipantAuthorityService
{
        public bool IsLocalParticipant(PlayerInput playerInput, int seatIndex)
        {
            return true;
        }

        public ulong ResolveOwnerClientId(PlayerInput playerInput, int seatIndex)
        {
            return 0UL;
        }
    }
}
