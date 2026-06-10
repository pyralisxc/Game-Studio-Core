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
        FirstProof = "Verify that IsLocalParticipant returns true in a single-player session.",
        NativeSetup = new[] { "Implement interface in a service component" }
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
