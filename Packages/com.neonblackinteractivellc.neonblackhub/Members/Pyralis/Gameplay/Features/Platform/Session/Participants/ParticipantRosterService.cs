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
    /// Authoritative runtime roster of participants. Also exposes a default
    /// participant through IPlayerProvider for systems that need a single focus handle.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Session,
        Relevance = "Authoritative runtime roster of participants. Exposes a default participant handle only for systems that need a single focus handle.",
        RoleTags = new[] { AuthoringContractRoleTags.IntentRouteEssential, AuthoringContractRoleTags.ParticipantRouteSupport },
        AssignmentFields = new[] { nameof(sessionDefinition) },
        FirstProof = "Enter Play Mode and spawn a pawn. Verify the 'Participants' list reflects the character.",
        NativeSetup = new[] { "Add to GameplaySessionBootstrap child." },
        ExpertAdvice = "Source of truth for all active participants. Bridges Unity's PlayerInput system to the Pyralis participant model. Use the roster for participant iteration and authority-aware lookup instead of assuming player one.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/participants"
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Setup/Participant Roster Service")]
    public class ParticipantRosterService : MonoBehaviour, IParticipantRoster, IPlayerProvider, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (sessionDefinition == null)
                yield return PyralisRuntimeValidationIssue.Required("Session Definition is empty. This is expected when GameplaySessionBootstrap injects it at runtime.");
        }
        [SerializeField] private SessionDefinition sessionDefinition;

        private readonly List<ParticipantHandle> _participants = new List<ParticipantHandle>();
        private int _nextParticipantId = 1;
        private IParticipantAuthorityService _participantAuthorityService;

        public IReadOnlyList<ParticipantHandle> Participants => _participants;

        public event Action<ParticipantHandle> ParticipantRegistered;
        public event Action<ParticipantHandle> ParticipantRemoved;
        public event Action<ParticipantHandle, GameObject> ParticipantPawnAssigned;
        public event Action<ParticipantHandle, GameObject> ParticipantPawnCleared;

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

            int seatIndex = ResolveSeatIndex(playerInput, preferredSeatIndex);
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
                ClearPawn(removed);
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
            {
                ClearPawn(participant);
                ParticipantRemoved?.Invoke(participant);
            }

            return removed;
        }

        public void AttachPawn(ParticipantHandle participant, GameObject pawn)
        {
            if (participant == null)
                return;

            participant.AttachPawn(pawn);
            if (pawn != null)
                ParticipantPawnAssigned?.Invoke(participant, pawn);
        }

        public void ClearPawn(ParticipantHandle participant)
        {
            if (participant == null || participant.PawnInstance == null)
                return;

            GameObject pawn = participant.PawnInstance;
            participant.ClearPawn();
            ParticipantPawnCleared?.Invoke(participant, pawn);
        }

        public bool TryGetPrimaryParticipant(out ParticipantHandle participant)
        {
            participant = _participants.Count > 0 ? _participants[0] : null;
            return participant != null;
        }

        public bool TryGetParticipantBySeat(int seatIndex, out ParticipantHandle participant)
        {
            for (int i = 0; i < _participants.Count; i++)
            {
                ParticipantHandle candidate = _participants[i];
                if (candidate != null && candidate.SeatIndex == seatIndex)
                {
                    participant = candidate;
                    return true;
                }
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
                if (!IsSeatTaken(seat))
                    return seat;

                seat++;
            }
        }

        private int ResolveSeatIndex(PlayerInput playerInput, int preferredSeatIndex)
        {
            if (preferredSeatIndex >= 0 && !IsSeatTaken(preferredSeatIndex))
                return preferredSeatIndex;

            if (playerInput != null && playerInput.playerIndex >= 0 && !IsSeatTaken(playerInput.playerIndex))
                return playerInput.playerIndex;

            return GetNextSeatIndex();
        }

        private bool IsSeatTaken(int seatIndex)
        {
            for (int i = 0; i < _participants.Count; i++)
            {
                if (_participants[i].SeatIndex == seatIndex)
                    return true;
            }

            return false;
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
