using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NeonBlack.Gameplay.Features.Rpg.Samples;
using NeonBlack.Gameplay.Presentation.Camera;
using NeonBlack.Gameplay.Presentation.Visuals;
using NeonBlack.Gameplay.Core.Contracts.Networking;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Runtime;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Features.Input;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Enemies;
using NeonBlack.Gameplay.Features.Characters;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace NeonBlack.Gameplay.Core.Runtime
{
    /// <summary>
    /// Runtime VContainer scope for the Pyralis gameplay platform.
    /// This wraps the existing service graph so the package can migrate
    /// away from static resolution without breaking active systems.
    /// </summary>
    [AddComponentMenu("NeonBlack/Gameplay/Setup/Pyralis Gameplay Lifetime Scope")]
    [DisallowMultipleComponent]
    [AuthoringContract(
        Capability = AuthoringCapability.Setup, 
        Relevance = "Inspector Add Component path for the visible Pyralis runtime composition scope.", 
        Axioms = AuthoringWorldAxiom.None,
        AssignmentFields = new[] { nameof(InjectLoadedScenesOnBuild) },
        FirstProof = "Check the VContainer debugger to ensure all gameplay services are correctly registered in the scope.",
        NativeSetup = new[] { "Add Component", "Configure VContainer Resolver" }
    )]
    public class PyralisGameplayLifetimeScope : LifetimeScope
    {
        private bool _isConfigured;
        private GameplayPlatformContext _platformContext;
        private SessionDefinition _sessionDefinition;
        private SessionStateService _sessionStateService;
        private ParticipantRosterService _participantRosterService;
        private ParticipantSpawnService _participantSpawnService;
        private ParticipantInputRouter _participantInputRouter;
        private SceneLoader _sceneLoader;
        private TimeManager _timeManager;
        private CameraShake _cameraShake;
        private CinemachineCameraRigController _cameraRigController;
        private ISessionOwnershipService _sessionOwnershipService;
        private IParticipantAuthorityService _participantAuthorityService;

        [Header("RPG Definitions")]
        [SerializeField] private ItemCatalogDefinition itemCatalog;
        [SerializeField] private ProgressionCurveDefinition progressionCurve;
        [SerializeField] private bool includeGoldenSample;

        public bool InjectLoadedScenesOnBuild { get; set; } = true;

        public void ConfigureRuntime(
            GameplayPlatformContext platformContext,
            SessionDefinition sessionDefinition,
            SessionStateService sessionStateService,
            ParticipantRosterService participantRosterService,
            ParticipantSpawnService participantSpawnService,
            ParticipantInputRouter participantInputRouter,
            SceneLoader sceneLoader,
            TimeManager timeManager,
            CameraShake cameraShake,
            CinemachineCameraRigController cameraRigController,
            ISessionOwnershipService sessionOwnershipService,
            IParticipantAuthorityService participantAuthorityService)
        {
            _platformContext = platformContext;
            _sessionDefinition = sessionDefinition;
            _sessionStateService = sessionStateService;
            _participantRosterService = participantRosterService;
            _participantSpawnService = participantSpawnService;
            _participantInputRouter = participantInputRouter;
            _sceneLoader = sceneLoader;
            _timeManager = timeManager;
            _cameraShake = cameraShake;
            _cameraRigController = cameraRigController;
            _sessionOwnershipService = sessionOwnershipService;
            _participantAuthorityService = participantAuthorityService;
            _isConfigured = true;
        }

        protected override void Awake()
        {
            if (!_isConfigured)
                autoRun = false;

            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            if (_platformContext != null)
            {
                builder.RegisterInstance(_platformContext).AsSelf();
                builder.RegisterInstance(_platformContext.Services).AsSelf();
            }

            if (_sessionDefinition != null)
                builder.RegisterInstance(_sessionDefinition).AsSelf();

            RegisterComponent(builder, _sessionStateService);
            if (_sessionStateService != null)
                builder.RegisterInstance<IGameplayStateReader>(_sessionStateService);

            RegisterComponent(builder, _participantRosterService);
            if (_participantRosterService != null)
            {
                builder.RegisterInstance<IParticipantRoster>(_participantRosterService);
                builder.RegisterInstance<IPlayerProvider>(_participantRosterService);
            }

            RegisterComponent(builder, _participantSpawnService);
            RegisterComponent(builder, _participantInputRouter);
RegisterComponent(builder, _sceneLoader);
            RegisterComponent(builder, _timeManager);
            RegisterComponent(builder, _cameraShake);
            RegisterComponent(builder, _cameraRigController);

            builder.Register<PawnComboProcessor>(Lifetime.Transient);
            builder.Register<PawnDamageHandler>(Lifetime.Transient);

            builder.Register<EnemyDetectionService>(Lifetime.Singleton);
            builder.Register<EnemyCombatProcessor>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<BattleManager>().AsSelf();

            builder.Register<LocalRpgPersistenceService>(Lifetime.Singleton).As<IRpgPersistenceService>();

            if (itemCatalog != null)
                builder.RegisterInstance<IItemCatalog>(itemCatalog);

            if (progressionCurve != null)
                builder.RegisterInstance<IProgressionCurve>(progressionCurve);

            builder.Register<InventoryService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<ProgressionService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<QuestService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<EquipmentService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<SkillTreeService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<DialogueService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<VendorService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<RpgOpenZoneService>(Lifetime.Singleton).AsSelf();
            builder.Register<HubInteractionService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

            if (includeGoldenSample)
                RpgGoldenSampleFactory.RegisterTo(builder);

            if (_sessionOwnershipService != null)
builder.RegisterInstance<ISessionOwnershipService>(_sessionOwnershipService);

            if (_participantAuthorityService != null)
                builder.RegisterInstance<IParticipantAuthorityService>(_participantAuthorityService);

            if (_platformContext != null && _platformContext.Services.TryResolve(out IGameplaySettingsApplier settingsApplier) && settingsApplier != null)
                builder.RegisterInstance<IGameplaySettingsApplier>(settingsApplier);
            else
                builder.RegisterInstance<IGameplaySettingsApplier>(new NullGameplaySettingsApplier());

            if (_platformContext != null)
                RegisterPlatformRegistryServices(builder, _platformContext.Services);

            builder.RegisterBuildCallback(container =>
            {
                if (_platformContext == null)
                    return;

                _platformContext.Services.SetFallbackResolver(type => container.Resolve(type));
                if (InjectLoadedScenesOnBuild)
                    InjectLoadedSceneObjects(container);
            });
}

        protected override void OnDestroy()
        {
            if (_platformContext != null)
            {
                if (!GameplayPlatformContext.ClearCurrentIf(_platformContext))
                    _platformContext.Services.Unregister<IObjectResolver>();
            }

            base.OnDestroy();
        }

        private static void RegisterComponent<T>(IContainerBuilder builder, T component)
            where T : Component
        {
            if (component == null)
                return;

            builder.RegisterComponent(component).AsSelf().AsImplementedInterfaces();
        }

        private static void RegisterPlatformRegistryServices(IContainerBuilder builder, PlatformServiceRegistry services)
        {
            foreach (KeyValuePair<System.Type, object> entry in services.Entries)
            {
                if (entry.Value == null || entry.Key == typeof(IObjectResolver))
                    continue;

                if (builder.Exists(entry.Key, includeInterfaceTypes: true, findParentScopes: true))
                    continue;

                builder.RegisterInstance(entry.Value, entry.Key);
            }
        }

        private void InjectLoadedSceneObjects(IObjectResolver container)
        {
            HashSet<GameObject> injectedRoots = new HashSet<GameObject>();

            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded)
                    continue;

                GameObject[] roots = scene.GetRootGameObjects();
                for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
                {
                    GameObject root = roots[rootIndex];
                    if (root == null || !injectedRoots.Add(root))
                        continue;

                    container.InjectGameObject(root);
                }
            }
        }
    }
}
