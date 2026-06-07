using System;
using NeonBlack.Gameplay.Data.Definitions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Characters
{
    /// <summary>
    /// Runtime participant state separated from pawn identity and scene object ownership.
    /// </summary>
    [Serializable]
    public class ParticipantHandle
    {
        [SerializeField] private int seatIndex;
        [SerializeField] private int teamIndex;
        [SerializeField] private ulong ownerClientId;
        [SerializeField] private bool isLocal;
        [SerializeField] private string displayName;

        public ParticipantId Id { get; private set; }
        public int SeatIndex => seatIndex;
        public int TeamIndex => teamIndex;
        public ulong OwnerClientId => ownerClientId;
        public bool IsLocal => isLocal;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? Id.ToString() : displayName;

        public PlayerInput PlayerInput { get; private set; }
        public ParticipantDefinition Definition { get; private set; }
        public PawnDefinition PawnDefinition { get; private set; }
        public GameObject PawnInstance { get; private set; }
        public ParticipantHandle(ParticipantId id, int seat, int team, ulong clientId, bool local, string name, PlayerInput playerInput, ParticipantDefinition definition)
        {
            Id = id;
            seatIndex = seat;
            teamIndex = team;
            ownerClientId = clientId;
            isLocal = local;
            displayName = name;
            PlayerInput = playerInput;
            Definition = definition;
            PawnDefinition = definition != null ? definition.defaultPawn : null;
        }

        public void SetDefinition(ParticipantDefinition definition)
        {
            Definition = definition;
            if (definition != null)
            {
                teamIndex = definition.teamIndex;
                if (!string.IsNullOrWhiteSpace(definition.displayName))
                    displayName = definition.displayName;
                if (definition.defaultPawn != null)
                    PawnDefinition = definition.defaultPawn;
            }
        }

        public void SetPawnDefinition(PawnDefinition definition)
        {
            PawnDefinition = definition;
        }

        public void AttachPawn(GameObject instance)
        {
            PawnInstance = instance;
        }

        public void ClearPawn()
        {
            PawnInstance = null;
        }
    }
}
