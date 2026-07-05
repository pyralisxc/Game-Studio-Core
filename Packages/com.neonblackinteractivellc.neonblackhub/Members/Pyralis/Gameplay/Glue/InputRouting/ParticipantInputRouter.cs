using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Glue.Participants;
using NeonBlack.Gameplay.Glue.Session;
using NeonBlack.Gameplay.Glue.Spawning;
using NeonBlack.Gameplay.Modules.Character;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Glue.InputRouting
{
    /// <summary>
    /// Watches PlayerInput instances and maps them into the participant roster.
    /// This keeps the session local-first while matching an N-participant model.
    /// </summary>
    [AuthoringContract(
        StableId = "participant.input-router",
        Category = "Input, Setup",
        CapabilityPath = "Core Setup/Input/Participant Input Router",
        Surface = AuthoringSurface.RequiredSetup,
        Summary = "Routes physical input device events to the correct participant; participant definitions own the InputProfile that pawn and non-pawn control surfaces consume.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/input",
        PrerequisiteStableIds = new[] { "participant.default", "input.profile" },
        RouteStage = "Scene Services",
        RouteOrder = 60,
        SetupDomain = "Input",
        ProofTarget = "Participant input routing registers the intended local participant before Play Mode proof.",
        NativeActionKind = AuthoringActionKind.AddComponent,
        SetupSteps = new[] 
        { 
            "Add ParticipantInputRouter to the Gameplay Root or a Gameplay Root child.",
            "Ensure it is wired to the ParticipantRosterService."
        },
        SuccessChecks = new[] { "Join a new player and verify they are correctly assigned to a participant seat in the roster." },
        RoleTags = new[] { "IntentRouteEssential", "InputRouteSupport" },
        Tags = new[] { "capability:Input", "capability:Setup", "runtime:PlatformCore" },
        Selectable = false
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Setup/Participant Input Router")]
    public class ParticipantInputRouter : MonoBehaviour, IRuntimeValidationProvider
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (sessionDefinition == null)
            {
                yield return RuntimeValidationIssue.Recommended(
                    "Session Definition is empty. This is expected when GameplaySessionBootstrap injects it at runtime.",
                    nameof(sessionDefinition),
                    nameof(ParticipantInputRouter),
                    "Assign ParticipantInputRouter.sessionDefinition only for a standalone scene object; otherwise let GameplaySessionBootstrap inject it.",
                    "ParticipantInputRouter has a SessionDefinition before routing input in Play Mode.",
                    "ParticipantInputRouter.SessionDefinition.Injected");
            }

            if (rosterService == null)
            {
                yield return RuntimeValidationIssue.Recommended(
                    "Roster Service is empty. This is expected when GameplaySessionBootstrap injects it at runtime.",
                    nameof(rosterService),
                    nameof(ParticipantInputRouter),
                    "Assign ParticipantInputRouter.rosterService only for a standalone scene object; otherwise let GameplaySessionBootstrap inject it.",
                    "ParticipantInputRouter has a ParticipantRosterService before routing input in Play Mode.",
                    "ParticipantInputRouter.RosterService.Injected");
            }
        }
        [SerializeField] private SessionDefinition sessionDefinition;
        [SerializeField] private ParticipantRosterService rosterService;
        [SerializeField] private PlayerInputManager playerInputManager;
        [SerializeField] private bool autoRegisterDefaultParticipantsWithoutPlayerInput = true;

        private readonly HashSet<PlayerInput> _knownInputs = new HashSet<PlayerInput>();
        private bool _autoRegisteredDefaults;
        private bool _subscribedToPlayerInputManager;

        [Inject]
        private void Construct(ParticipantRosterService injectedRosterService = null)
        {
            rosterService ??= injectedRosterService;
        }

        private void Awake()
        {
            ResolvePlayerInputManager();
        }

        private void OnEnable()
        {
            SubscribeToPlayerInputManager();
            SynchronizeExistingPlayerInputs();
            AutoRegisterDefaultsIfNeeded();
        }

        private void Start()
        {
            SynchronizeExistingPlayerInputs();
            AutoRegisterDefaultsIfNeeded();
        }

        private void OnDisable()
        {
            UnsubscribeFromPlayerInputManager();
        }

        public void SetSessionDefinition(SessionDefinition definition)
        {
            sessionDefinition = definition;
            TryInitializeRoute();
        }

        public void SetRosterService(ParticipantRosterService service)
        {
            rosterService = service;
            TryInitializeRoute();
        }

        public void SetPlayerInputManager(PlayerInputManager manager)
        {
            if (playerInputManager == manager)
                return;

            UnsubscribeFromPlayerInputManager();
            playerInputManager = manager;
            SubscribeToPlayerInputManager();
            TryInitializeRoute();
        }

        public void RegisterPlayerInput(PlayerInput playerInput)
        {
            if (rosterService == null || playerInput == null || _knownInputs.Contains(playerInput))
                return;

            int preferredSeat = playerInput.playerIndex >= 0 ? playerInput.playerIndex : -1;
            int resolvedSeat = ResolveAvailableSeatForJoin(preferredSeat);
            ParticipantDefinition participantDefinition = ResolveDefinitionForSeat(resolvedSeat);
            ParticipantHandle registeredParticipant = rosterService.RegisterParticipant(playerInput, participantDefinition, resolvedSeat);
            if (registeredParticipant == null)
                return;

            ApplyParticipantInputProfile(playerInput, registeredParticipant);
            _knownInputs.Add(playerInput);
        }

        public void UnregisterPlayerInput(PlayerInput playerInput)
        {
            if (playerInput == null)
                return;

            if (rosterService != null)
                rosterService.RemoveParticipant(playerInput);

            _knownInputs.Remove(playerInput);
        }

        private void SynchronizeExistingPlayerInputs()
        {
            if (rosterService == null)
                return;

            foreach (PlayerInput playerInput in PlayerInput.all)
                RegisterPlayerInput(playerInput);
        }

        private void TryInitializeRoute()
        {
            if (!isActiveAndEnabled)
                return;

            SynchronizeExistingPlayerInputs();
            AutoRegisterDefaultsIfNeeded();
        }

        private void AutoRegisterDefaultsIfNeeded()
        {
            if (rosterService == null
                || _autoRegisteredDefaults
                || !autoRegisterDefaultParticipantsWithoutPlayerInput
                || rosterService.Participants.Count > 0)
            {
                return;
            }

            if (sessionDefinition == null || sessionDefinition.defaultParticipants == null)
                return;

            ParticipantJoinRouteDecision joinRoute = ParticipantJoinRoutePolicy.Evaluate(
                sessionDefinition,
                playerInputManager != null);
            if (joinRoute.ShouldDeferAutoRegistration)
            {
                Debug.LogWarning("[ParticipantInputRouter] " + joinRoute.WarningMessage, this);
                _autoRegisteredDefaults = true;
                return;
            }

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

        private void SubscribeToPlayerInputManager()
        {
            if (_subscribedToPlayerInputManager)
                return;

            ResolvePlayerInputManager();
            if (playerInputManager == null)
                return;

            playerInputManager.onPlayerJoined += RegisterPlayerInput;
            playerInputManager.onPlayerLeft += UnregisterPlayerInput;
            _subscribedToPlayerInputManager = true;
        }

        private void UnsubscribeFromPlayerInputManager()
        {
            if (!_subscribedToPlayerInputManager || playerInputManager == null)
                return;

            playerInputManager.onPlayerJoined -= RegisterPlayerInput;
            playerInputManager.onPlayerLeft -= UnregisterPlayerInput;
            _subscribedToPlayerInputManager = false;
        }

        private void ResolvePlayerInputManager()
        {
            if (playerInputManager != null)
                return;

            playerInputManager = GetComponentInParent<PlayerInputManager>();
        }

        private ParticipantDefinition ResolveDefinitionForSeat(int currentInputCount)
        {
            if (sessionDefinition == null || sessionDefinition.defaultParticipants == null)
                return null;

            if (currentInputCount >= 0 && currentInputCount < sessionDefinition.defaultParticipants.Length)
                return sessionDefinition.defaultParticipants[currentInputCount];

            return null;
        }

        private int ResolveAvailableSeatForJoin(int preferredSeat)
        {
            if (preferredSeat >= 0 && !IsSeatTaken(preferredSeat))
                return preferredSeat;

            int fallbackSeat = _knownInputs.Count;
            if (fallbackSeat >= 0 && !IsSeatTaken(fallbackSeat))
                return fallbackSeat;

            int seat = 0;
            while (IsSeatTaken(seat))
                seat++;

            return seat;
        }

        private bool IsSeatTaken(int seatIndex)
        {
            if (rosterService == null)
                return false;

            for (int i = 0; i < rosterService.Participants.Count; i++)
            {
                ParticipantHandle participant = rosterService.Participants[i];
                if (participant != null && participant.SeatIndex == seatIndex)
                    return true;
            }

            return false;
        }

        private void ApplyParticipantInputProfile(PlayerInput playerInput, ParticipantHandle participant)
        {
            if (playerInput == null || participant == null)
                return;

            InputProfile inputProfile = ParticipantInputProfileUtility.ResolveEffectiveInputProfile(participant.Definition);

            if (inputProfile == null)
                return;

            inputProfile.Sanitize();
            ParticipantInputProfileUtility.ApplyToPlayerInput(playerInput, inputProfile);
        }
    }
}
