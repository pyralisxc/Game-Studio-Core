using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Core.Contracts.Networking
{
    /// <summary>
    /// Resolves authority and ownership metadata for participants.
    /// </summary>
    public interface IParticipantAuthorityService
    {
        ulong ResolveOwnerClientId(PlayerInput playerInput, int seatIndex);

        bool IsLocalParticipant(PlayerInput playerInput, int seatIndex);
    }
}
