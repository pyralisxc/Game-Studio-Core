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
using NeonBlack.Gameplay.Networking.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Characters
{
    /// <summary>
    /// Single supported startup path for NeonBlack Gameplay scenes.
    /// </summary>
    [DefaultExecutionOrder(-1100)]
    public class GameplaySessionBootstrap : MonoBehaviour
    {
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
        [SerializeField] private SceneLoader sceneLoader;
        [SerializeField] private TimeManager timeManager;
        [SerializeField] private CameraShake cameraShake;

        private void Awake()
        {
            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);

            sessionDefinition?.Sanitize();
            GameplayRuntimeContext.SetSession(sessionDefinition);
            GameplayPlatformContext platformContext = GameplayPlatformContext.CreateOrReplace(sessionDefinition);
            RegisterPlatformDefaults(platformContext.Services);

            if (autoCreateCoreServices)
            {
                sceneLoader ??= GetOrCreatePersistentService<SceneLoader>("SceneLoader");
                timeManager ??= GetOrCreatePersistentService<TimeManager>("TimeManager");
                cameraShake ??= GetOrCreatePersistentService<CameraShake>("CameraShake");
            }

            sessionStateService ??= GetOrCreatePersistentService<SessionStateService>("SessionStateService");
            sessionStateService.SetSessionDefinition(sessionDefinition);
            platformContext.Services.Register(sessionStateService);

            ParticipantRosterService rosterService = participantRosterService ??= GetOrCreatePersistentService<ParticipantRosterService>("ParticipantRosterService");
            rosterService.SetSessionDefinition(sessionDefinition);
            platformContext.Services.Register(rosterService);
            platformContext.Services.Register<IParticipantRoster>(rosterService);
            platformContext.Services.Register<IPlayerProvider>(rosterService);

            ParticipantSpawnService spawnService = participantSpawnService ??= GetOrCreatePersistentService<ParticipantSpawnService>("ParticipantSpawnService");
            spawnService.SetRosterService(rosterService);
            spawnService.SetSessionStateService(sessionStateService);
            spawnService.SetSpawnPoints(spawnPoints);
            platformContext.Services.Register(spawnService);

            ParticipantInputRouter inputRouter = participantInputRouter ??= GetOrCreatePersistentService<ParticipantInputRouter>("ParticipantInputRouter");
            inputRouter.SetSessionDefinition(sessionDefinition);
            platformContext.Services.Register(inputRouter);

            ISessionOwnershipService sessionOwnershipService = ResolveOrCreateSessionOwnershipService(platformContext.Services);
            IParticipantAuthorityService participantAuthorityService = ResolveOrCreateParticipantAuthorityService(platformContext.Services);

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

            if (cameraRigController != null && sessionDefinition != null && sessionDefinition.defaultGameMode != null)
                cameraRigController.SetGameMode(sessionDefinition.defaultGameMode);
        }

        private static void RegisterPlatformDefaults(PlatformServiceRegistry services)
        {
            if (!services.TryResolve(out ISessionOwnershipService _))
                services.Register<ISessionOwnershipService>(new LocalSessionOwnershipService());

            if (!services.TryResolve(out IParticipantAuthorityService _))
                services.Register<IParticipantAuthorityService>(new LocalParticipantAuthorityService());
        }

        private PyralisGameplayLifetimeScope GetOrCreateLifetimeScope()
        {
            PyralisGameplayLifetimeScope lifetimeScope = GetComponent<PyralisGameplayLifetimeScope>();
            if (lifetimeScope == null)
                lifetimeScope = gameObject.AddComponent<PyralisGameplayLifetimeScope>();

            lifetimeScope.autoRun = false;
            return lifetimeScope;
        }

        private static ISessionOwnershipService ResolveOrCreateSessionOwnershipService(PlatformServiceRegistry services)
        {
            if (services.TryResolve(out ISessionOwnershipService service) && service != null)
                return service;

            service = new LocalSessionOwnershipService();
            services.Register<ISessionOwnershipService>(service);
            return service;
        }

        private static IParticipantAuthorityService ResolveOrCreateParticipantAuthorityService(PlatformServiceRegistry services)
        {
            if (services.TryResolve(out IParticipantAuthorityService service) && service != null)
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
                Debug.LogWarning("[GameplaySessionBootstrap] Session Definition has no default input profile. Legacy input-driven actors may need per-pawn input setup.", this);
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
            GameObject existingChild = transform.Find(serviceName)?.gameObject;
            if (existingChild != null && existingChild.TryGetComponent(out T existingComponent))
                return existingComponent;

            GameObject go = new GameObject(serviceName);
            go.transform.SetParent(transform, false);
            if (dontDestroyOnLoad)
                DontDestroyOnLoad(go);
            return go.AddComponent<T>();
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
