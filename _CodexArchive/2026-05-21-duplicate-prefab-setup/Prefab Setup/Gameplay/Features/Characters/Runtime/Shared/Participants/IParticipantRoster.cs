using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Characters
{
    public interface IParticipantRoster
    {
        IReadOnlyList<ParticipantHandle> Participants { get; }
        event Action<ParticipantHandle> ParticipantRegistered;
        event Action<ParticipantHandle> ParticipantRemoved;

        ParticipantHandle RegisterParticipant(PlayerInput playerInput, NeonBlack.Gameplay.Data.Definitions.ParticipantDefinition definition = null, int preferredSeatIndex = -1);
        bool RemoveParticipant(PlayerInput playerInput);
        bool RemoveParticipant(ParticipantHandle participant);
        bool TryGetPrimaryParticipant(out ParticipantHandle participant);
    }
}
