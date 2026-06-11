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
        NativeSetup = new[] { "Add to Scene Service Registry or Bootstrap." },
        ExpertAdvice = "The Local Authority service is a 'dumb' pass-through for same-machine play. It identifies all inputs as local. Use the Networked variant for online multiplayer projects.",
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
