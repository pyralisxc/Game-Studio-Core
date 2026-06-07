using NeonBlack.Gameplay.Core.Contracts.Networking;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Networking.Runtime
{
    /// <summary>
    /// Default local authority model for offline and same-machine sessions.
    /// </summary>
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
