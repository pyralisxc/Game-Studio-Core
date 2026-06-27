using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Data.Participants
{
    public interface IParticipantRoster
    {
        IReadOnlyList<ParticipantHandle> Participants { get; }
        event Action<ParticipantHandle> ParticipantRegistered;
        event Action<ParticipantHandle> ParticipantRemoved;
        event Action<ParticipantHandle, GameObject> ParticipantPawnAssigned;
        event Action<ParticipantHandle, GameObject> ParticipantPawnCleared;

        ParticipantHandle RegisterParticipant(PlayerInput playerInput, NeonBlack.Gameplay.Data.Definitions.ParticipantDefinition definition = null, int preferredSeatIndex = -1);
        bool RemoveParticipant(PlayerInput playerInput);
        bool RemoveParticipant(ParticipantHandle participant);
        bool TryGetPrimaryParticipant(out ParticipantHandle participant);
        bool TryGetParticipantBySeat(int seatIndex, out ParticipantHandle participant);
    }
}
