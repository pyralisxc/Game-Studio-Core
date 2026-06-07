using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Core.Contracts.Networking;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace NeonBlack.Gameplay.Characters
{
    /// <summary>
    /// Authoritative local/runtime roster of participants. Also bridges legacy single-player
    /// lookups by exposing the primary participant as an IPlayerProvider.
    /// </summary>
        public class ParticipantRosterService : MonoBehaviour, IParticipantRoster, IPlayerProvider
    {
        [SerializeField] private SessionDefinition sessionDefinition;

        private readonly List<ParticipantHandle> _participants = new List<ParticipantHandle>();
        private int _nextParticipantId = 1;
        private IParticipantAuthorityService _participantAuthorityService;

        public IReadOnlyList<ParticipantHandle> Participants => _participants;

        public event Action<ParticipantHandle> ParticipantRegistered;
        public event Action<ParticipantHandle> ParticipantRemoved;

        public void Initialize() { }
        public void Shutdown() { }

        [Inject]
        private void Construct(IParticipantAuthorityService participantAuthorityService = null)
        {
            _participantAuthorityService = participantAuthorityService;
        }

        /// <summary>Override in a networked subclass to return the NGO client ID for the local player.</summary>
        protected virtual ulong ResolveOwnerClientId() => 0UL;

        public void SetSessionDefinition(SessionDefinition definition)
        {
            sessionDefinition = definition;
            sessionDefinition?.Sanitize();
        }

        public ParticipantHandle RegisterParticipant(PlayerInput playerInput, ParticipantDefinition definition = null, int preferredSeatIndex = -1)
        {
            if (playerInput != null)
            {
                for (int i = 0; i < _participants.Count; i++)
                    if (_participants[i].PlayerInput == playerInput)
                        return _participants[i];
            }

            if (!CanRegisterAdditionalParticipant())
                return null;

            int seatIndex = preferredSeatIndex >= 0
                ? preferredSeatIndex
                : playerInput != null && playerInput.playerIndex >= 0
                    ? playerInput.playerIndex
                    : GetNextSeatIndex();
            ParticipantDefinition resolvedDefinition = definition ?? ResolveDefaultDefinitionForSeat(seatIndex);
            ulong ownerClientId = ResolveOwnerClientId(playerInput, seatIndex);
            bool isLocal = ResolveIsLocalParticipant(playerInput, seatIndex);

            string displayName = resolvedDefinition != null && !string.IsNullOrWhiteSpace(resolvedDefinition.displayName)
                ? resolvedDefinition.displayName
                : $"Participant {seatIndex + 1}";

            ParticipantHandle participant = new ParticipantHandle(
                new ParticipantId(_nextParticipantId++),
                seatIndex,
                resolvedDefinition != null ? resolvedDefinition.teamIndex : 0,
                ownerClientId,
                local: isLocal,
                name: displayName,
                playerInput: playerInput,
                definition: resolvedDefinition);

            _participants.Add(participant);
            ParticipantRegistered?.Invoke(participant);
            return participant;
        }

        public bool RemoveParticipant(PlayerInput playerInput)
        {
            for (int i = 0; i < _participants.Count; i++)
            {
                if (_participants[i].PlayerInput != playerInput)
                    continue;

                ParticipantHandle removed = _participants[i];
                _participants.RemoveAt(i);
                ParticipantRemoved?.Invoke(removed);
                return true;
            }

            return false;
        }

        public bool RemoveParticipant(ParticipantHandle participant)
        {
            if (participant == null)
                return false;

            bool removed = _participants.Remove(participant);
            if (removed)
                ParticipantRemoved?.Invoke(participant);

            return removed;
        }

        public bool TryGetPrimaryParticipant(out ParticipantHandle participant)
        {
            if (_participants.Count > 0)
            {
                participant = _participants[0];
                return true;
            }

            participant = null;
            return false;
        }

        public Transform GetPlayerTransform()
        {
            return TryGetPrimaryParticipant(out ParticipantHandle participant) && participant.PawnInstance != null
                ? participant.PawnInstance.transform
                : null;
        }

        public GameObject GetPlayerGameObject()
        {
            return TryGetPrimaryParticipant(out ParticipantHandle participant)
                ? participant.PawnInstance
                : null;
        }

        private int GetNextSeatIndex()
        {
            int seat = 0;
            while (true)
            {
                bool taken = false;
                for (int i = 0; i < _participants.Count; i++)
                {
                    if (_participants[i].SeatIndex == seat)
                    {
                        taken = true;
                        break;
                    }
                }

                if (!taken)
                    return seat;

                seat++;
            }
        }

        private bool CanRegisterAdditionalParticipant()
        {
            if (sessionDefinition == null)
                return true;

            return _participants.Count < sessionDefinition.GetEffectiveMaxParticipants();
        }

        private ParticipantDefinition ResolveDefaultDefinitionForSeat(int seatIndex)
        {
            if (sessionDefinition == null || sessionDefinition.defaultParticipants == null)
                return null;

            for (int i = 0; i < sessionDefinition.defaultParticipants.Length; i++)
            {
                ParticipantDefinition definition = sessionDefinition.defaultParticipants[i];
                if (definition == null)
                    continue;

                if (definition.preferredSeatIndex == seatIndex)
                    return definition;
            }

            return seatIndex >= 0 && seatIndex < sessionDefinition.defaultParticipants.Length
                ? sessionDefinition.defaultParticipants[seatIndex]
                : null;
        }

        private ulong ResolveOwnerClientId(PlayerInput playerInput, int seatIndex)
        {
            IParticipantAuthorityService authorityService = _participantAuthorityService;
            if (authorityService != null)
                return authorityService.ResolveOwnerClientId(playerInput, seatIndex);

            return ResolveOwnerClientId();
        }

        private bool ResolveIsLocalParticipant(PlayerInput playerInput, int seatIndex)
        {
            IParticipantAuthorityService authorityService = _participantAuthorityService;
            if (authorityService != null)
                return authorityService.IsLocalParticipant(playerInput, seatIndex);

            return true;
        }
    }
}
