namespace NeonBlack.Gameplay.Core.Contracts.Networking
{
    /// <summary>
    /// Neutral participant authority request built by Unity-facing input or networking adapters.
    /// </summary>
    public readonly struct ParticipantAuthorityRequest
    {
        public ParticipantAuthorityRequest(int seatIndex, int inputPlayerIndex, bool hasUnityInputOwner)
        {
            SeatIndex = seatIndex;
            InputPlayerIndex = inputPlayerIndex;
            HasUnityInputOwner = hasUnityInputOwner;
        }

        public int SeatIndex { get; }
        public int InputPlayerIndex { get; }
        public bool HasUnityInputOwner { get; }
    }

    /// <summary>
    /// Resolves authority and ownership metadata for participants.
    /// </summary>
    public interface IParticipantAuthorityService
    {
        ulong ResolveOwnerClientId(ParticipantAuthorityRequest request);

        bool IsLocalParticipant(ParticipantAuthorityRequest request);
    }
}
