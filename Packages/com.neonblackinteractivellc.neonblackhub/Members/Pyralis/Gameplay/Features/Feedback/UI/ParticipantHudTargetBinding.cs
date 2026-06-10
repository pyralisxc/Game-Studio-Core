using NeonBlack.Gameplay.Characters;
using VContainer;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Feedback.UI
{
    public abstract class ParticipantHudTargetBinding : MonoBehaviour
    {
        [Header("Participant Filter")]
        [SerializeField] protected bool usePrimaryParticipant = true;
        [SerializeField] protected int participantSeat = 0;

        private IParticipantRoster _participantRoster;

        [Inject]
        private void Construct(IParticipantRoster participantRoster)
        {
            _participantRoster = participantRoster;
            OnBindingsConstructed();
        }

        protected virtual void OnBindingsConstructed()
        {
        }

        protected bool MatchesParticipant(ParticipantHandle participant)
        {
            if (participant == null)
                return false;

            if (usePrimaryParticipant)
                return participant.SeatIndex == 0;

            return participant.SeatIndex == participantSeat;
        }

        protected bool TryGetTrackedParticipant(out ParticipantHandle participant)
        {
            participant = null;
            if (_participantRoster == null)
                return false;

            if (usePrimaryParticipant)
                return _participantRoster.TryGetPrimaryParticipant(out participant);

            for (int i = 0; i < _participantRoster.Participants.Count; i++)
            {
                ParticipantHandle candidate = _participantRoster.Participants[i];
                if (candidate != null && candidate.SeatIndex == participantSeat)
                {
                    participant = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
