using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Presentation.Camera;
using NeonBlack.Gameplay.Core.Config;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Presentation.Visuals;
using NeonBlack.Gameplay.Core.Runtime;
using NeonBlack.Gameplay.Features.Input;
using NeonBlack.Gameplay.Core.Contracts.Networking;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace NeonBlack.Gameplay.Characters
{
    /// <summary>
    /// Single supported startup path for NeonBlack Gameplay scenes.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Setup,
        Priority = AuthoringPriority.Primary,
        SetupNodeId = "bootstrap.root",
        Relevance = "Primary entry point for gameplay sessions; orchestrates participant spawn, camera setup, and core services.",
        Axioms = AuthoringWorldAxiom.None,
        NativeSetup = new[]
        {
            "Add GameplaySessionBootstrap to the first scene of your game.",
            "Assign a SessionDefinition asset.",
            "Wire spawn points for participants.",
            "Configure camera rig controller and core service references."
        },
        AssignmentFields = new[] { nameof(sessionDefinition), nameof(spawnPoints), nameof(cameraRigController), nameof(playerInputManager) },
        FirstProof = "Enter Play Mode and confirm the session initializes. Verify participant pawns spawn at designated points and the camera frames the action correctly.",
        ExpertAdvice = "The Bootstrap is the heart of the Pyralis session. Ensure your SessionDefinition has at least one Participant defined. The Bootstrap auto-creates required services (Roster, Spawn, State) if they are missing from its children.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/bootstrap"
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Setup/Gameplay Session Bootstrap")]
    [DefaultExecutionOrder(-1100)]
    public class GameplaySessionBootstrap : MonoBehaviour
    {
        [Header("Session")]
        [SerializeField] private SessionDefinition sessionDefinition;
        [Header("Behavior")]
        [SerializeField] private bool dontDestroyOnLoad = true;
        [SerializeField] private bool injectLoadedScenesOnBuild = true;

        [Header("Participants")]
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private PlayerInputManager playerInputManager;
        [SerializeField] private SessionStateService sessionStateService;
        [SerializeField] private ParticipantRosterService participantRosterService;
        [SerializeField] private ParticipantSpawnService participantSpawnService;
        [SerializeField] private ParticipantInputRouter participantInputRouter;

        [Header("Camera")]
        [SerializeField] private CinemachineCameraRigController cameraRigController;
        [SerializeField, Tooltip("Optional scene transition service. Assign SceneFader, SceneLoader, or another component implementing ISceneNavigator.")]
        private MonoBehaviour sceneNavigatorSource;
        [SerializeField] private TimeManager timeManager;
        [SerializeField] private CameraShake cameraShake;

        private void Awake()
        {
            Scene bootstrapScene = gameObject.scene;

            sessionDefinition?.Sanitize();
            GameplayRuntimeContext.SetSession(sessionDefinition);

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);

            PyralisGameplayLifetimeScope lifetimeScope = GetOrCreateLifetimeScope();
            lifetimeScope.InjectLoadedScenesOnBuild = injectLoadedScenesOnBuild;
            lifetimeScope.ConfigureRuntime(
                sessionDefinition,
                spawnPoints,
                playerInputManager,
                sceneNavigatorSource,
                timeManager,
                cameraShake,
                cameraRigController,
                sessionStateService,
                participantRosterService,
                participantSpawnService,
                participantInputRouter);
            if (lifetimeScope.Container == null)
                lifetimeScope.Build();

            ConfigurePlayerInputManager();

            if (cameraRigController != null)
                cameraRigController.SetParticipantRoster(lifetimeScope.ParticipantRosterService);

            if (cameraRigController != null && sessionDefinition != null && sessionDefinition.defaultGameMode != null)
                cameraRigController.SetGameMode(sessionDefinition.defaultGameMode);
        }

        private PyralisGameplayLifetimeScope GetOrCreateLifetimeScope()
        {
            PyralisGameplayLifetimeScope lifetimeScope = GetComponent<PyralisGameplayLifetimeScope>();
            if (lifetimeScope == null)
                lifetimeScope = gameObject.AddComponent<PyralisGameplayLifetimeScope>();

            lifetimeScope.autoRun = false;
            return lifetimeScope;
        }

        [ContextMenu("Validate Gameplay Setup")]
        private void ValidateSetup()
        {
            if (sessionDefinition == null)
            {
                Debug.LogError("[GameplaySessionBootstrap] CRITICAL: Session Definition is missing. The session cannot initialize.", this);
                return;
            }

            // Core modularity checks
            if (sessionDefinition.defaultGameMode == null)
                Debug.LogWarning("[GameplaySessionBootstrap] Session Definition has no Default Game Mode. Ensure your game logic is handled by a custom Feature Module.", this);
            
            if (sessionDefinition.defaultParticipants == null || sessionDefinition.defaultParticipants.Length == 0)
                Debug.LogWarning("[GameplaySessionBootstrap] Session Definition has no default participants. No actors will spawn automatically.", this);
            
            if (playerInputManager == null)
                Debug.LogWarning("[GameplaySessionBootstrap] PlayerInputManager is missing. Local player join will not be automated.", this);
            
            if (cameraRigController == null)
                Debug.LogWarning("[GameplaySessionBootstrap] CinemachineCameraRigController is missing. Dynamic camera framing will be disabled.", this);

            // Check for potential service name collisions or missing authored services
            CheckPersistentService<SessionStateService>("SessionStateService");
            CheckPersistentService<ParticipantRosterService>("ParticipantRosterService");
            CheckPersistentService<ParticipantSpawnService>("ParticipantSpawnService");
            CheckPersistentService<ParticipantInputRouter>("ParticipantInputRouter");
        }

        private void CheckPersistentService<T>(string serviceName) where T : Component
        {
            Transform existing = transform.Find(serviceName);
            if (existing == null)
            {
                Debug.Log($"[GameplaySessionBootstrap] LifetimeScope will create service '{serviceName}' at runtime. To customize, add an authored GameObject named '{serviceName}' with the {typeof(T).Name} component under the Gameplay Root.", this);
            }
        }

        private void ConfigurePlayerInputManager()
        {
            if (playerInputManager == null || sessionDefinition == null)
                return;

            playerInputManager.splitScreen = sessionDefinition.allowSplitScreen && !sessionDefinition.sharedCameraByDefault;
        }

    }
}
