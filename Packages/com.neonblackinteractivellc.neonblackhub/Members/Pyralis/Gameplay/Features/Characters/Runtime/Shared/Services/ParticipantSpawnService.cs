using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using VContainer;

namespace NeonBlack.Gameplay.Characters
{
    /// <summary>
    /// Spawns and assigns pawns for registered participants using authored PawnDefinitions.
    /// </summary>
    /// <summary>
    /// Service for spawning participants into the scene at designated spawn points.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Setup | AuthoringCapability.Session,
        Relevance = "Orchestrates participant spawning at designated spawn points during session initialization.",
        Axioms = AuthoringWorldAxiom.None,
        RequiredInterfaces = new[] { typeof(IGameService) },
        FirstProof = "Register a participant and verify their pawn is spawned at the correct spawn point."
    )]
    public class ParticipantSpawnService : MonoBehaviour, IGameService
{
        [SerializeField] private ParticipantRosterService rosterService;
        [SerializeField] private SessionStateService sessionStateService;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private bool spawnOnRegister = true;
        [SerializeField] private bool replaceExistingPawn = true;

        [Inject]
        private void Construct(ParticipantRosterService injectedRosterService = null, SessionStateService injectedSessionStateService = null)
        {
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

        public void SetSpawnPoints(Transform[] points)
        {
            spawnPoints = points;
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
                participant.ClearPawn();
            }

            GameObject joinedPawnInstance = TryResolveJoinedPawnInstance(participant);
            if (joinedPawnInstance != null)
            {
                joinedPawnInstance.transform.position = ResolveSpawnPosition(participant.SeatIndex);
                participant.AttachPawn(joinedPawnInstance);
                InitializePawnInstance(joinedPawnInstance, participant);
                return joinedPawnInstance;
            }

            Vector3 spawnPosition = ResolveSpawnPosition(participant.SeatIndex);
            GameObject instance = Instantiate(participant.PawnDefinition.pawnPrefab, spawnPosition, Quaternion.identity);
            participant.AttachPawn(instance);
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
            participant.ClearPawn();
        }

        private Vector3 ResolveSpawnPosition(int seatIndex)
        {
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                if (seatIndex >= 0 && seatIndex < spawnPoints.Length && spawnPoints[seatIndex] != null)
                    return spawnPoints[seatIndex].position;
            }

            return transform.position + new Vector3(seatIndex * 2f, 0f, 0f);
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
