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
using System;
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
        private const string NetworkedSessionStateServiceTypeName = "NeonBlack.Gameplay.Networking.Participants.NetworkedSessionStateService, NeonBlack.Gameplay.Networking";
        private const string NetworkedParticipantRosterServiceTypeName = "NeonBlack.Gameplay.Networking.Participants.NetworkedParticipantRosterService, NeonBlack.Gameplay.Networking";
        private const string NetworkedParticipantSpawnServiceTypeName = "NeonBlack.Gameplay.Networking.Participants.NetworkedParticipantSpawnService, NeonBlack.Gameplay.Networking";
        private const string NetworkedSessionOwnershipServiceTypeName = "NeonBlack.Gameplay.Networking.Runtime.NetworkedSessionOwnershipService, NeonBlack.Gameplay.Networking";
        private const string NetworkedParticipantAuthorityServiceTypeName = "NeonBlack.Gameplay.Networking.Runtime.NetworkedParticipantAuthorityService, NeonBlack.Gameplay.Networking";

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
        [SerializeField, Tooltip("Optional explicit camera bounds provider for specialized scenes. Prefer assigning Camera Rig Controller; CinemachineCameraRigController provides ICameraBoundsProvider for 2D camera-aware systems.")]
        private MonoBehaviour cameraBoundsSource;
        [SerializeField] private SceneLoader sceneLoader;
        [SerializeField] private TimeManager timeManager;
        [SerializeField] private CameraShake cameraShake;

        private void Awake()
        {
            Scene bootstrapScene = gameObject.scene;

            sessionDefinition?.Sanitize();
            GameplayRuntimeContext.SetSession(sessionDefinition);

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);

            // Services are now authored or resolved via hierarchy/DI, removing manual creation logic.
            
            bool useNetcodeServices = sessionDefinition != null && sessionDefinition.networkMode != GameplayNetworkMode.LocalOnly;

            sessionStateService ??= useNetcodeServices
                ? GetOrCreatePersistentService<SessionStateService>("SessionStateService", NetworkedSessionStateServiceTypeName)
                : GetOrCreatePersistentService<SessionStateService>("SessionStateService");
            sessionStateService.SetSessionDefinition(sessionDefinition);

            ParticipantRosterService rosterService = participantRosterService ??= useNetcodeServices
                ? GetOrCreatePersistentService<ParticipantRosterService>("ParticipantRosterService", NetworkedParticipantRosterServiceTypeName)
                : GetOrCreatePersistentService<ParticipantRosterService>("ParticipantRosterService");
            rosterService.SetSessionDefinition(sessionDefinition);

            ParticipantSpawnService spawnService = participantSpawnService ??= useNetcodeServices
                ? GetOrCreatePersistentService<ParticipantSpawnService>("ParticipantSpawnService", NetworkedParticipantSpawnServiceTypeName)
                : GetOrCreatePersistentService<ParticipantSpawnService>("ParticipantSpawnService");
            spawnService.SetRosterService(rosterService);
            spawnService.SetSessionStateService(sessionStateService);
            spawnService.SetSpawnPoints(spawnPoints);
            spawnService.SetCameraBoundsProvider(cameraRigController as ICameraBoundsProvider ?? cameraBoundsSource as ICameraBoundsProvider);

            ParticipantInputRouter inputRouter = participantInputRouter ??= GetOrCreatePersistentService<ParticipantInputRouter>("ParticipantInputRouter");
            inputRouter.SetSessionDefinition(sessionDefinition);
            inputRouter.SetRosterService(rosterService);
            inputRouter.SetPlayerInputManager(playerInputManager);

            // Initialize Static Utilities for systems not yet using DI
            ParticipantQueryUtility.Initialize(rosterService, rosterService);

            ISessionOwnershipService sessionOwnershipService = ResolveOrCreateSessionOwnershipService(useNetcodeServices);
            IParticipantAuthorityService participantAuthorityService = ResolveOrCreateParticipantAuthorityService(useNetcodeServices);

            PyralisGameplayLifetimeScope lifetimeScope = GetOrCreateLifetimeScope();
            lifetimeScope.InjectLoadedScenesOnBuild = injectLoadedScenesOnBuild;
            lifetimeScope.ConfigureRuntime(
                sessionDefinition,
                sessionStateService,
                rosterService,
                spawnService,
                inputRouter,
                sceneLoader,
                timeManager,
                cameraShake,
                cameraRigController,
                sessionOwnershipService,
                participantAuthorityService);
            if (lifetimeScope.Container == null)
                lifetimeScope.Build();

            ConfigurePlayerInputManager();

            if (cameraRigController != null)
                cameraRigController.SetParticipantRoster(rosterService);

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

        private static ISessionOwnershipService ResolveOrCreateSessionOwnershipService(bool useNetcodeServices)
        {
            if (useNetcodeServices && TryCreateServiceInstance(NetworkedSessionOwnershipServiceTypeName, out ISessionOwnershipService networkedService))
            {
                return networkedService;
            }

            return new LocalSessionOwnershipService();
        }

        private static IParticipantAuthorityService ResolveOrCreateParticipantAuthorityService(bool useNetcodeServices)
        {
            if (useNetcodeServices && TryCreateServiceInstance(NetworkedParticipantAuthorityServiceTypeName, out IParticipantAuthorityService networkedService))
            {
                return networkedService;
            }

            return new LocalParticipantAuthorityService();
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
                Debug.Log($"[GameplaySessionBootstrap] Service '{serviceName}' will be auto-created at runtime. To customize, add an authored GameObject named '{serviceName}' with the {typeof(T).Name} component.", this);
            }
        }

        private void ConfigurePlayerInputManager()
        {
            if (playerInputManager == null || sessionDefinition == null)
                return;

            TrySetMember(playerInputManager, "maxPlayerCount", sessionDefinition.GetEffectiveMaxParticipants());
            TrySetMember(playerInputManager, "splitScreen", sessionDefinition.allowSplitScreen && !sessionDefinition.sharedCameraByDefault);
        }

        private T GetOrCreatePersistentService<T>(string serviceName) where T : Component
        {
            return GetOrCreatePersistentService<T>(serviceName, null);
        }

        private T GetOrCreatePersistentService<T>(string serviceName, string preferredTypeName) where T : Component
        {
            GameObject existingChild = transform.Find(serviceName)?.gameObject;
            if (existingChild != null && existingChild.TryGetComponent(out T existingComponent))
                return existingComponent;

            GameObject go = new GameObject(serviceName);
            go.transform.SetParent(transform, false);

            if (!string.IsNullOrWhiteSpace(preferredTypeName))
            {
                Type preferredType = Type.GetType(preferredTypeName);
                if (preferredType != null && typeof(T).IsAssignableFrom(preferredType) && typeof(Component).IsAssignableFrom(preferredType))
                    return (T)go.AddComponent(preferredType);

                Debug.LogWarning($"[GameplaySessionBootstrap] Networked service type `{preferredTypeName}` was not found. Falling back to `{typeof(T).Name}`.", this);
            }

            return go.AddComponent<T>();
        }

        private static bool TryCreateServiceInstance<T>(string typeName, out T service) where T : class
        {
            service = null;
            Type type = Type.GetType(typeName);
            if (type == null || !typeof(T).IsAssignableFrom(type))
                return false;

            service = Activator.CreateInstance(type) as T;
            return service != null;
        }

        private static void TrySetMember(object target, string memberName, object value)
{
            System.Reflection.PropertyInfo property = target.GetType().GetProperty(memberName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (property != null && property.CanWrite)
            {
                property.SetValue(target, value);
                return;
            }

            System.Reflection.FieldInfo field = target.GetType().GetField(memberName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(target, value);
        }
    }
}
