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
        [SerializeField] private bool autoCreateCoreServices = true;
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
            GameplayPlatformContext platformContext = GameplayPlatformContext.CreateOrReplace(sessionDefinition);
            RegisterPlatformDefaults(platformContext.Services);
            RegisterSceneServices(platformContext.Services, bootstrapScene);

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);

            if (autoCreateCoreServices)
            {
                sceneLoader ??= GetOrCreatePersistentService<SceneLoader>("SceneLoader");
                timeManager ??= GetOrCreatePersistentService<TimeManager>("TimeManager");
                cameraShake ??= GetOrCreatePersistentService<CameraShake>("CameraShake");
            }

            bool useNetcodeServices = sessionDefinition != null && sessionDefinition.networkMode != GameplayNetworkMode.LocalOnly;

            sessionStateService ??= useNetcodeServices
                ? GetOrCreatePersistentService<SessionStateService>("SessionStateService", NetworkedSessionStateServiceTypeName)
                : GetOrCreatePersistentService<SessionStateService>("SessionStateService");
            sessionStateService.SetSessionDefinition(sessionDefinition);
            platformContext.Services.Register(sessionStateService);
            platformContext.Services.Register<IGameplayStateReader>(sessionStateService);

            ParticipantRosterService rosterService = participantRosterService ??= useNetcodeServices
                ? GetOrCreatePersistentService<ParticipantRosterService>("ParticipantRosterService", NetworkedParticipantRosterServiceTypeName)
                : GetOrCreatePersistentService<ParticipantRosterService>("ParticipantRosterService");
            rosterService.SetSessionDefinition(sessionDefinition);
            platformContext.Services.Register(rosterService);
            platformContext.Services.Register<IParticipantRoster>(rosterService);
            platformContext.Services.Register<IPlayerProvider>(rosterService);

            ParticipantSpawnService spawnService = participantSpawnService ??= useNetcodeServices
                ? GetOrCreatePersistentService<ParticipantSpawnService>("ParticipantSpawnService", NetworkedParticipantSpawnServiceTypeName)
                : GetOrCreatePersistentService<ParticipantSpawnService>("ParticipantSpawnService");
            spawnService.SetRosterService(rosterService);
            spawnService.SetSessionStateService(sessionStateService);
            spawnService.SetSpawnPoints(spawnPoints);
            platformContext.Services.Register(spawnService);

            ParticipantInputRouter inputRouter = participantInputRouter ??= GetOrCreatePersistentService<ParticipantInputRouter>("ParticipantInputRouter");
            inputRouter.SetSessionDefinition(sessionDefinition);
            inputRouter.SetRosterService(rosterService);
            inputRouter.SetPlayerInputManager(playerInputManager);
            platformContext.Services.Register(inputRouter);

            ISessionOwnershipService sessionOwnershipService = ResolveOrCreateSessionOwnershipService(platformContext.Services, useNetcodeServices);
            IParticipantAuthorityService participantAuthorityService = ResolveOrCreateParticipantAuthorityService(platformContext.Services, useNetcodeServices);

            PyralisGameplayLifetimeScope lifetimeScope = GetOrCreateLifetimeScope();
            lifetimeScope.InjectLoadedScenesOnBuild = injectLoadedScenesOnBuild;
            lifetimeScope.ConfigureRuntime(
                platformContext,
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

        private static void RegisterPlatformDefaults(PlatformServiceRegistry services)
        {
            if (!services.TryResolve(out ISessionOwnershipService _))
                services.Register<ISessionOwnershipService>(new LocalSessionOwnershipService());

            if (!services.TryResolve(out IParticipantAuthorityService _))
                services.Register<IParticipantAuthorityService>(new LocalParticipantAuthorityService());

            if (!services.TryResolve(out IGameplaySettingsApplier _))
                services.Register<IGameplaySettingsApplier>(new NullGameplaySettingsApplier());
        }

        private void RegisterSceneServices(PlatformServiceRegistry services, Scene bootstrapScene)
        {
            if (services == null || !bootstrapScene.IsValid())
                return;

            if (!services.TryResolve(out ICameraBoundsProvider _)
                && cameraRigController != null
                && cameraRigController.gameObject.scene == bootstrapScene)
            {
                services.Register(cameraRigController);
                services.Register<ICameraBoundsProvider>(cameraRigController);
            }

            if (!services.TryResolve(out ICameraBoundsProvider _)
                && TryGetAuthoredSceneService(bootstrapScene, out ICameraBoundsProvider cameraBoundsProvider))
            {
                services.Register(cameraBoundsProvider);
                services.Register<ICameraBoundsProvider>(cameraBoundsProvider);
            }
        }

        private bool TryGetAuthoredSceneService<T>(Scene scene, out T service) where T : class
        {
            service = null;
            if (cameraBoundsSource != null && cameraBoundsSource.gameObject.scene == scene && cameraBoundsSource is T candidate)
            {
                service = candidate;
                return true;
            }

            return false;
        }

        private PyralisGameplayLifetimeScope GetOrCreateLifetimeScope()
        {
            PyralisGameplayLifetimeScope lifetimeScope = GetComponent<PyralisGameplayLifetimeScope>();
            if (lifetimeScope == null)
                lifetimeScope = gameObject.AddComponent<PyralisGameplayLifetimeScope>();

            lifetimeScope.autoRun = false;
            return lifetimeScope;
        }

        private static ISessionOwnershipService ResolveOrCreateSessionOwnershipService(PlatformServiceRegistry services, bool useNetcodeServices)
        {
            if (services.TryResolve(out ISessionOwnershipService service) && service != null)
            {
                if (!useNetcodeServices || service.GetType().FullName != typeof(LocalSessionOwnershipService).FullName)
                    return service;
            }

            if (useNetcodeServices && TryCreateServiceInstance(NetworkedSessionOwnershipServiceTypeName, out ISessionOwnershipService networkedService))
            {
                services.Register<ISessionOwnershipService>(networkedService);
                return networkedService;
            }

            if (service != null)
                return service;

            service = new LocalSessionOwnershipService();
            services.Register<ISessionOwnershipService>(service);
            return service;
        }

        private static IParticipantAuthorityService ResolveOrCreateParticipantAuthorityService(PlatformServiceRegistry services, bool useNetcodeServices)
        {
            if (services.TryResolve(out IParticipantAuthorityService service) && service != null)
            {
                if (!useNetcodeServices || service.GetType().FullName != typeof(LocalParticipantAuthorityService).FullName)
                    return service;
            }

            if (useNetcodeServices && TryCreateServiceInstance(NetworkedParticipantAuthorityServiceTypeName, out IParticipantAuthorityService networkedService))
            {
                services.Register<IParticipantAuthorityService>(networkedService);
                return networkedService;
            }

            if (service != null)
                return service;

            service = new LocalParticipantAuthorityService();
            services.Register<IParticipantAuthorityService>(service);
            return service;
        }

        [ContextMenu("Validate Gameplay Setup")]
        private void ValidateSetup()
        {
            if (sessionDefinition == null)
            {
                Debug.LogWarning("[GameplaySessionBootstrap] Session Definition is not assigned.", this);
                return;
            }

            if (sessionDefinition.defaultGameMode == null)
                Debug.LogWarning("[GameplaySessionBootstrap] Session Definition has no Default Game Mode.", this);
            if (sessionDefinition.defaultParticipants == null || sessionDefinition.defaultParticipants.Length == 0)
                Debug.LogWarning("[GameplaySessionBootstrap] Session Definition has no default participants configured.", this);
            if (sessionDefinition.defaultInputProfile == null)
                Debug.LogWarning("[GameplaySessionBootstrap] Session Definition has no default input profile. Compatibility input-driven actors may need per-pawn input setup.", this);
            if (playerInputManager == null)
                Debug.LogWarning("[GameplaySessionBootstrap] PlayerInputManager is not assigned. Local join will require explicit participant registration.", this);
            if (cameraRigController == null)
                Debug.LogWarning("[GameplaySessionBootstrap] CinemachineCameraRigController is not assigned. Shared camera profile control will remain inactive until a rig is provided.", this);
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
