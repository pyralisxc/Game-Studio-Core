using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace NeonBlack.Gameplay.Characters
{
    /// <summary>
    /// Spawns and assigns pawns for registered participants using authored PawnDefinitions and service-owned spawn points.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Setup | AuthoringCapability.Session,
        RuntimeFamilies = new[] { RuntimeCapabilityFamily.PlatformCore, RuntimeCapabilityFamily.CharacterPawnGameplay },
        Relevance = "Single owner for participant pawn spawning. It resolves each ParticipantDefinition default pawn, places it at authored spawn points, and reports pawn assignment through the roster.",
        Axioms = AuthoringWorldAxiom.None,
        RoleTags = new[] { AuthoringContractRoleTags.IntentRouteEssential, AuthoringContractRoleTags.ParticipantRouteSupport },
        RequiredInterfaces = new[] { typeof(IGameService) },
        AssignmentFields = new[] { nameof(rosterService), nameof(sessionStateService), nameof(spawnPoints) },
        FirstProof = "Register a participant and confirm ParticipantSpawnService creates or reuses the pawn, attaches it to the roster, and places it at the expected spawn point.",
        NativeSetup = new[] { "Add as a child service under GameplaySessionBootstrap.", "Assign Spawn Points on ParticipantSpawnService for pawn-backed routes." },
        ExpertAdvice = "Keep spawn points here, not on GameplaySessionBootstrap. Non-pawn routes can leave spawn points empty and disable Spawn On Register.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/participants"
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Setup/Participant Spawn Service")]
    public class ParticipantSpawnService : MonoBehaviour, IGameService, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (rosterService == null)
                yield return PyralisRuntimeValidationIssue.Required("Roster Service is empty. This is expected when GameplaySessionBootstrap injects it at runtime.");
            if (sessionStateService == null)
                yield return PyralisRuntimeValidationIssue.Required("Session State Service is empty. This is expected when GameplaySessionBootstrap injects it at runtime.");
            if (spawnOnRegister && (spawnPoints == null || spawnPoints.Length == 0))
                yield return PyralisRuntimeValidationIssue.Required("Spawn Points is empty. Assign spawn points on ParticipantSpawnService for pawn-backed routes, or disable Spawn On Register for non-pawn routes.");
        }
        [SerializeField] private ParticipantRosterService rosterService;
        [SerializeField] private SessionStateService sessionStateService;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private bool spawnOnRegister = true;
        [SerializeField] private bool replaceExistingPawn = true;

        private IObjectResolver _resolver;
        private ICameraBoundsProvider _cameraBoundsProvider;
        private IPlayfieldBoundsProvider _playfieldBoundsProvider;

        [Inject]
        private void Construct(IObjectResolver resolver, ParticipantRosterService injectedRosterService = null, SessionStateService injectedSessionStateService = null)
        {
            _resolver = resolver;
            rosterService ??= injectedRosterService;
            sessionStateService ??= injectedSessionStateService;
        }

        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        public void Initialize()
        {
            if (rosterService == null)
                return;

            rosterService.ParticipantRegistered -= HandleParticipantRegistered;
            rosterService.ParticipantRegistered += HandleParticipantRegistered;
            rosterService.ParticipantRemoved -= HandleParticipantRemoved;
            rosterService.ParticipantRemoved += HandleParticipantRemoved;

            if (!spawnOnRegister)
                return;

            for (int i = 0; i < rosterService.Participants.Count; i++)
            {
                ParticipantHandle participant = rosterService.Participants[i];
                if (participant.PawnInstance == null)
                    SpawnParticipantPawn(participant);
            }
        }

        public void Shutdown()
        {
            if (rosterService == null)
                return;

            rosterService.ParticipantRegistered -= HandleParticipantRegistered;
            rosterService.ParticipantRemoved -= HandleParticipantRemoved;
        }

        public void SetRosterService(ParticipantRosterService service)
        {
            rosterService = service;
        }

        public void SetSessionStateService(SessionStateService service)
        {
            sessionStateService = service;
        }

        public void SetCameraBoundsProvider(ICameraBoundsProvider provider)
        {
            _cameraBoundsProvider = provider;
        }

        public void SetPlayfieldBoundsProvider(IPlayfieldBoundsProvider provider)
        {
            _playfieldBoundsProvider = provider;
        }

        public virtual GameObject SpawnParticipantPawn(ParticipantHandle participant)
        {
            if (participant == null || participant.PawnDefinition == null || participant.PawnDefinition.pawnPrefab == null)
                return null;

            if (participant.PawnInstance != null)
            {
                if (!replaceExistingPawn)
                    return participant.PawnInstance;

                DestroyPawnInstance(participant.PawnInstance);
                ClearParticipantPawn(participant);
            }

            GameObject joinedPawnInstance = TryResolveJoinedPawnInstance(participant);
            if (joinedPawnInstance != null)
            {
                if (!TryResolveSpawnPosition(participant.SeatIndex, out Vector3 joinedSpawnPosition))
                    return null;

                joinedPawnInstance.transform.position = joinedSpawnPosition;
                AttachParticipantPawn(participant, joinedPawnInstance);
                InitializePawnInstance(joinedPawnInstance, participant);
                return joinedPawnInstance;
            }

            if (!TryResolveSpawnPosition(participant.SeatIndex, out Vector3 spawnPosition))
                return null;

            GameObject instance = _resolver != null 
                ? _resolver.Instantiate(participant.PawnDefinition.pawnPrefab, spawnPosition, Quaternion.identity)
                : Instantiate(participant.PawnDefinition.pawnPrefab, spawnPosition, Quaternion.identity);
            AttachParticipantPawn(participant, instance);
            InitializePawnInstance(instance, participant);

            return instance;
        }

        private GameObject TryResolveJoinedPawnInstance(ParticipantHandle participant)
        {
            if (participant?.PlayerInput == null)
                return null;

            GameObject inputObject = participant.PlayerInput.gameObject;
            MonoBehaviour[] behaviours = inputObject.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IPawnParticipantInitializer)
                    return behaviours[i].gameObject;
            }

            return null;
        }

        private void InitializePawnInstance(GameObject instance, ParticipantHandle participant)
        {
            if (instance == null)
                return;

            IPawnParticipantInitializer pawnInitializer = instance.GetComponent<IPawnParticipantInitializer>();
            if (pawnInitializer != null)
                pawnInitializer.InitializeForParticipant(participant, sessionStateService != null ? sessionStateService.ActiveGameMode : null);

            ConfigureSpawnedPawnRuntime(instance);
        }

        private void ConfigureSpawnedPawnRuntime(GameObject instance)
        {
            if (instance == null)
                return;

            MonoBehaviour[] behaviours = instance.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                ApplyPawnRuntimeServices(behaviours[i]);
            }
        }

        private void ApplyPawnRuntimeServices(MonoBehaviour behaviour)
        {
            if (behaviour is not IPawnRuntimeServicesReceiver receiver)
                return;

            receiver.ApplyRuntimeServices(new PawnRuntimeServicesContext(sessionStateService, _cameraBoundsProvider, _playfieldBoundsProvider));
        }

        private void HandleParticipantRegistered(ParticipantHandle participant)
        {
            if (spawnOnRegister)
                SpawnParticipantPawn(participant);
        }

        private void HandleParticipantRemoved(ParticipantHandle participant)
        {
            if (participant == null || participant.PawnInstance == null)
                return;

            DestroyPawnInstance(participant.PawnInstance);
            ClearParticipantPawn(participant);
        }

        private void AttachParticipantPawn(ParticipantHandle participant, GameObject instance)
        {
            if (rosterService != null)
                rosterService.AttachPawn(participant, instance);
            else
                participant.AttachPawn(instance);
        }

        private void ClearParticipantPawn(ParticipantHandle participant)
        {
            if (rosterService != null)
                rosterService.ClearPawn(participant);
            else
                participant.ClearPawn();
        }

        private bool TryResolveSpawnPosition(int seatIndex, out Vector3 position)
        {
            position = default;
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                if (seatIndex >= 0 && seatIndex < spawnPoints.Length && spawnPoints[seatIndex] != null)
                {
                    position = spawnPoints[seatIndex].position;
                    return true;
                }

                for (int i = 0; i < spawnPoints.Length; i++)
                {
                    if (spawnPoints[i] == null)
                        continue;

                    position = spawnPoints[i].position;
                    return true;
                }
            }

            Debug.LogError("[ParticipantSpawnService] Spawn On Register is enabled, but no spawn point is assigned. Add at least one Transform to ParticipantSpawnService > Spawn Points, or disable Spawn On Register for a custom/no-pawn route.", this);
            return false;
        }

        /// <summary>Override in a networked subclass to despawn the pawn from NGO before destroying it.</summary>
        protected virtual void DestroyPawnInstance(GameObject go)
        {
            if (go == null) return;
            if (Application.isPlaying)
                Destroy(go);
            else
                DestroyImmediate(go);
        }
    }
}
