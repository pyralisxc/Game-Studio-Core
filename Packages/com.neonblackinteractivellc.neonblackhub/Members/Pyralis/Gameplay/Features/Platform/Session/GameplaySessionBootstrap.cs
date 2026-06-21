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
        RoleTags = new[] { AuthoringContractRoleTags.IntentRouteEssential, AuthoringContractRoleTags.CoreRouteAnchor },
        Relevance = "Primary entry point for gameplay sessions; wires the authored session, visible runtime services, input join, and camera setup.",
        Axioms = AuthoringWorldAxiom.None,
        NativeSetup = new[]
        {
            "Add GameplaySessionBootstrap to the first scene of your game.",
            "Assign a SessionDefinition asset.",
            "Author core runtime service components under the Gameplay Root or assign explicit overrides.",
            "Configure camera rig controller and core service references."
        },
        AssignmentFields = new[] { nameof(sessionDefinition), nameof(cameraRigController), nameof(playerInputManager) },
        FirstProof = "Enter Play Mode and confirm the session initializes, core services run, and the camera frames the active route.",
        ExpertAdvice = "The Bootstrap is the Unity-facing session entry point. Add the core runtime service components under the Gameplay Root or assign explicit override fields so the LifetimeScope can register authored scene objects instead of creating hidden services. ParticipantSpawnService owns pawn spawn points.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/bootstrap"
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Setup/Gameplay Session Bootstrap")]
    [DefaultExecutionOrder(-1100)]
    [RequireComponent(typeof(PyralisGameplayLifetimeScope))]
    public class GameplaySessionBootstrap : MonoBehaviour
    {
        [Header("Session")]
        [SerializeField] private SessionDefinition sessionDefinition;
        [Header("Behavior")]
        [SerializeField] private bool dontDestroyOnLoad = true;
        [SerializeField] private bool injectLoadedScenesOnBuild = true;

        [Header("Participants")]
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

            PyralisGameplayLifetimeScope lifetimeScope = GetRequiredLifetimeScope();
            if (lifetimeScope == null)
                return;

            lifetimeScope.InjectLoadedScenesOnBuild = injectLoadedScenesOnBuild;
            lifetimeScope.ConfigureRuntime(
                sessionDefinition,
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

        private PyralisGameplayLifetimeScope GetRequiredLifetimeScope()
        {
            PyralisGameplayLifetimeScope lifetimeScope = GetComponent<PyralisGameplayLifetimeScope>();
            if (lifetimeScope == null)
            {
                Debug.LogError("[GameplaySessionBootstrap] Missing PyralisGameplayLifetimeScope. Add it to the Gameplay Root before Play Mode; GameplaySessionBootstrap requires the visible composition root.", this);
                return null;
            }

            lifetimeScope.autoRun = false;
            return lifetimeScope;
        }

        private void ConfigurePlayerInputManager()
        {
            if (playerInputManager == null || sessionDefinition == null)
                return;

            playerInputManager.splitScreen = sessionDefinition.allowSplitScreen && !sessionDefinition.sharedCameraByDefault;
        }

    }
}
