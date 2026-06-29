using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Core.Contracts.Networking;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Glue.Participants
{
    /// <summary>
    /// Authoritative runtime roster of participants. Also exposes a default
    /// participant through IPlayerProvider for systems that need a single focus handle.
    /// </summary>
    [AuthoringContract(
        Category = "Session",
        CapabilityPath = "Core Setup/Participants/Participant Roster Service",
        Surface = AuthoringSurface.Goal,
        Summary = "Authoritative runtime roster of participants. Exposes a default participant handle only for systems that need a single focus handle.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/participants",
        PrerequisiteStableIds = new[] { "session.definition" },
        RouteStage = "Scene Services",
        RouteOrder = 45,
        SetupDomain = "Participants",
        ProofTarget = "Participant roster registers the authored participants for the session.",
        NativeActionKind = AuthoringActionKind.AddComponent,
        SetupSteps = new[] { "Add to GameplaySessionBootstrap child." },
        SuccessChecks = new[] { "Enter Play Mode and spawn a pawn. Verify the 'Participants' list reflects the character." },
        RoleTags = new[] { "IntentRouteEssential", "ParticipantRouteSupport" },
        Tags = new[] { "capability:Session", "runtime:PlatformCore" }
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Setup/Participant Roster Service")]
    public class ParticipantRosterService : MonoBehaviour, IParticipantRoster, IPlayerProvider, IRuntimeValidationProvider
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (sessionDefinition == null)
                yield return RuntimeValidationIssue.Required("Session Definition is empty. This is expected when GameplaySessionBootstrap injects it at runtime.");
        }
        [SerializeField] private SessionDefinition sessionDefinition;

        private readonly List<ParticipantHandle> _participants = new List<ParticipantHandle>();
        private readonly Dictionary<ParticipantHandle, ParticipantStateMachine> _participantStateMachines = new Dictionary<ParticipantHandle, ParticipantStateMachine>();
        private int _nextParticipantId = 1;
        private IParticipantAuthorityService _participantAuthorityService;
        private IGameplayEventChannel _eventChannel;

        public IReadOnlyList<ParticipantHandle> Participants => _participants;

        public event Action<ParticipantHandle> ParticipantRegistered;
        public event Action<ParticipantHandle> ParticipantRemoved;
        public event Action<ParticipantHandle, GameObject> ParticipantPawnAssigned;
        public event Action<ParticipantHandle, GameObject> ParticipantPawnCleared;

        public void Initialize() { }
        public void Shutdown() { }

        [Inject]
        private void Construct(
            IParticipantAuthorityService participantAuthorityService = null,
            IGameplayEventChannel eventChannel = null)
        {
            _participantAuthorityService = participantAuthorityService;
            _eventChannel = eventChannel;
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
            _participantStateMachines.Add(participant, new ParticipantStateMachine());
            ApplyParticipantLifecycle(participant, ParticipantLifecycleState.Joined);
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
                ApplyParticipantLifecycle(removed, ParticipantLifecycleState.Left);
                _participantStateMachines.Remove(removed);
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
                ApplyParticipantLifecycle(participant, ParticipantLifecycleState.Left);
                _participantStateMachines.Remove(participant);
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
            {
                ApplyParticipantLifecycle(participant, ParticipantLifecycleState.Spawned);
                ApplyParticipantLifecycle(participant, ParticipantLifecycleState.PossessingPawn);
                ParticipantPawnAssigned?.Invoke(participant, pawn);
            }
        }

        public void ClearPawn(ParticipantHandle participant)
        {
            if (participant == null || participant.PawnInstance == null)
                return;

            GameObject pawn = participant.PawnInstance;
            participant.ClearPawn();
            ApplyParticipantLifecycle(participant, ParticipantLifecycleState.Joined);
            ParticipantPawnCleared?.Invoke(participant, pawn);
        }

        public bool TryGetLifecycleState(ParticipantHandle participant, out ParticipantLifecycleState state)
        {
            if (participant != null && _participantStateMachines.TryGetValue(participant, out ParticipantStateMachine stateMachine))
            {
                state = stateMachine.CurrentState;
                return true;
            }

            state = ParticipantLifecycleState.Unjoined;
            return false;
        }

        public ParticipantLifecycleState GetLifecycleState(ParticipantHandle participant)
        {
            return TryGetLifecycleState(participant, out ParticipantLifecycleState state)
                ? state
                : ParticipantLifecycleState.Unjoined;
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
                return authorityService.ResolveOwnerClientId(BuildAuthorityRequest(playerInput, seatIndex));

            return ResolveOwnerClientId();
        }

        private bool ResolveIsLocalParticipant(PlayerInput playerInput, int seatIndex)
        {
            IParticipantAuthorityService authorityService = _participantAuthorityService;
            if (authorityService != null)
                return authorityService.IsLocalParticipant(BuildAuthorityRequest(playerInput, seatIndex));

            return true;
        }

        private static ParticipantAuthorityRequest BuildAuthorityRequest(PlayerInput playerInput, int seatIndex)
        {
            return new ParticipantAuthorityRequest(
                seatIndex,
                playerInput != null ? playerInput.playerIndex : -1,
                playerInput != null);
        }

        private bool ApplyParticipantLifecycle(ParticipantHandle participant, ParticipantLifecycleState state)
        {
            if (participant == null)
                return false;

            if (!_participantStateMachines.TryGetValue(participant, out ParticipantStateMachine stateMachine))
                return false;

            ParticipantLifecycleState previousState = stateMachine.CurrentState;
            if (!stateMachine.TryTransitionTo(state))
                return false;

            PublishParticipantLifecycleChanged(participant, previousState, state);
            return true;
        }

        private void PublishParticipantLifecycleChanged(
            ParticipantHandle participant,
            ParticipantLifecycleState previousState,
            ParticipantLifecycleState currentState)
        {
            if (previousState == currentState || _eventChannel == null)
                return;

            _eventChannel.Publish(new ParticipantLifecycleChangedEvent(
                participant.Id.Value,
                participant.SeatIndex,
                previousState,
                currentState));
        }
    }
}
