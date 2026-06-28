using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Glue.Participants;
using NeonBlack.Gameplay.Glue.Spawning;
using NeonBlack.Gameplay.Glue.Session;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Modules.Character;
using NeonBlack.Gameplay.Presentation.Camera;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Presentation.Visuals;
using NeonBlack.Gameplay.Glue.Lifetime;
using NeonBlack.Gameplay.Glue.InputRouting;
using NeonBlack.Gameplay.Core.Contracts.Networking;
using NeonBlack.Gameplay.Glue.SceneServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Glue.Bootstrap
{
    /// <summary>
    /// Single supported startup path for NeonBlack Gameplay scenes.
    /// </summary>
    [AuthoringContract(
        StableId = "bootstrap.root",
        Category = "Setup",
        CapabilityPath = "Core Setup/Session/Gameplay Session Bootstrap",
        Surface = AuthoringSurface.Goal,
        Summary = "Primary entry point for gameplay sessions; wires the authored session, visible runtime services, input join, and camera setup.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/bootstrap",
        RequiredFields = new[] { nameof(sessionDefinition) },
        PrerequisiteStableIds = new[] { "lifetime.scope" },
        RouteStage = "Scene Root",
        RouteOrder = 10,
        SetupDomain = "Session",
        ProofTarget = "Gameplay session starts from the authored scene root.",
        NativeActionKind = AuthoringActionKind.AddComponent,
        SetupSteps = new[]
        {
            "Add GameplaySessionBootstrap to the first scene of your game.",
            "Assign a SessionDefinition asset.",
            "Author core runtime service components under the Gameplay Root or assign explicit overrides.",
            "Configure camera rig controller and core service references."
        },
        SuccessChecks = new[] { "Enter Play Mode and confirm the session initializes, core services run, and the camera frames the active route." },
        RoleTags = new[] { "IntentRouteEssential", "CoreRouteAnchor" },
        Tags = new[] { "capability:Setup", "runtime:PlatformCore", "priority:Primary" }
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Setup/Gameplay Session Bootstrap")]
    [DefaultExecutionOrder(-1100)]
    [RequireComponent(typeof(PyralisGameplayLifetimeScope))]
    public class GameplaySessionBootstrap : MonoBehaviour, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (sessionDefinition == null)
            {
                yield return PyralisRuntimeValidationIssue.Required(
                    "Session Definition is unassigned.",
                    nameof(sessionDefinition),
                    nameof(GameplaySessionBootstrap),
                    "Assign the SessionDefinition asset on GameplaySessionBootstrap.",
                    "GameplaySessionBootstrap references the session asset before Play Mode.",
                    "GameplaySessionBootstrap.SessionDefinition.Missing");
            }

            if (cameraRigController == null)
            {
                yield return PyralisRuntimeValidationIssue.Recommended(
                    "Camera Rig Controller is unassigned. This is fine for non-camera proofs, but pawn proofs should author camera focus.",
                    nameof(cameraRigController),
                    nameof(GameplaySessionBootstrap),
                    "Assign the scene CinemachineCameraRigController when the selected route needs camera follow.",
                    "GameplaySessionBootstrap can hand the participant roster and game mode to the camera rig.",
                    "GameplaySessionBootstrap.CameraRig.Optional");
            }
        }

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
