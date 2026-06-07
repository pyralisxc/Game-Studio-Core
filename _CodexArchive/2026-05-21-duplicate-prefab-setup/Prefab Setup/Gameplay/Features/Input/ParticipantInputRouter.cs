using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Characters;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using VContainer;

namespace NeonBlack.Gameplay.Features.Input
{
    /// <summary>
    /// Watches PlayerInput instances and maps them into the participant roster.
    /// This keeps the session local-first while matching an N-participant model.
    /// </summary>
    public class ParticipantInputRouter : MonoBehaviour
    {
        [SerializeField] private SessionDefinition sessionDefinition;
        [SerializeField] private ParticipantRosterService rosterService;
        [SerializeField] private bool autoRegisterDefaultParticipantsWithoutPlayerInput = true;

        private readonly HashSet<PlayerInput> _knownInputs = new HashSet<PlayerInput>();
        private bool _autoRegisteredDefaults;

        [Inject]
        private void Construct(ParticipantRosterService injectedRosterService = null)
        {
            rosterService ??= injectedRosterService;
        }

        private void Awake()
        {
        }

        private void Update()
        {
            if (rosterService == null)
                return;

            RegisterLivePlayerInputs();
            RemoveMissingPlayerInputs();
            AutoRegisterDefaultsIfNeeded();
        }

        public void SetSessionDefinition(SessionDefinition definition)
        {
            sessionDefinition = definition;
        }

        private void RegisterLivePlayerInputs()
        {
            foreach (PlayerInput playerInput in PlayerInput.all)
            {
                if (playerInput == null || _knownInputs.Contains(playerInput))
                    continue;

                int preferredSeat = playerInput.playerIndex >= 0 ? playerInput.playerIndex : -1;
                ParticipantDefinition participantDefinition = ResolveDefinitionForSeat(preferredSeat >= 0 ? preferredSeat : _knownInputs.Count);
                ParticipantHandle registeredParticipant = rosterService.RegisterParticipant(playerInput, participantDefinition, participantDefinition != null ? participantDefinition.preferredSeatIndex : preferredSeat);
                if (registeredParticipant == null)
                    continue;

                ApplyParticipantInputProfile(playerInput, registeredParticipant);
                _knownInputs.Add(playerInput);
            }
        }

        private void RemoveMissingPlayerInputs()
        {
            List<PlayerInput> missing = null;
            foreach (PlayerInput playerInput in _knownInputs)
            {
                if (playerInput != null && ContainsInput(PlayerInput.all, playerInput))
                    continue;

                missing ??= new List<PlayerInput>();
                missing.Add(playerInput);
            }

            if (missing == null)
                return;

            for (int i = 0; i < missing.Count; i++)
            {
                rosterService.RemoveParticipant(missing[i]);
                _knownInputs.Remove(missing[i]);
            }
        }

        private static bool ContainsInput(ReadOnlyArray<PlayerInput> inputs, PlayerInput target)
        {
            for (int i = 0; i < inputs.Count; i++)
            {
                if (inputs[i] == target)
                    return true;
            }

            return false;
        }

        private void AutoRegisterDefaultsIfNeeded()
        {
            if (_autoRegisteredDefaults || !autoRegisterDefaultParticipantsWithoutPlayerInput || rosterService.Participants.Count > 0)
                return;

            if (PlayerInput.all.Count > 0 || sessionDefinition == null || sessionDefinition.defaultParticipants == null)
                return;

            for (int i = 0; i < sessionDefinition.defaultParticipants.Length; i++)
            {
                ParticipantDefinition definition = sessionDefinition.defaultParticipants[i];
                if (definition == null || !definition.autoJoin)
                    continue;

                if (rosterService.RegisterParticipant(null, definition, definition.preferredSeatIndex) == null)
                    break;
            }

            _autoRegisteredDefaults = true;
        }

        private ParticipantDefinition ResolveDefinitionForSeat(int currentInputCount)
        {
            if (sessionDefinition == null || sessionDefinition.defaultParticipants == null)
                return null;

            if (currentInputCount >= 0 && currentInputCount < sessionDefinition.defaultParticipants.Length)
                return sessionDefinition.defaultParticipants[currentInputCount];

            return null;
        }

        private void ApplyParticipantInputProfile(PlayerInput playerInput, ParticipantHandle participant)
        {
            if (playerInput == null || participant == null)
                return;

            InputProfile inputProfile = ParticipantInputProfileUtility.ResolveEffectiveInputProfile(
                participant.Definition,
                participant.PawnDefinition,
                sessionDefinition != null ? sessionDefinition.defaultInputProfile : null);

            if (inputProfile == null)
                return;

            inputProfile.Sanitize();
            ParticipantInputProfileUtility.ApplyToPlayerInput(playerInput, inputProfile);
        }
    }
}
