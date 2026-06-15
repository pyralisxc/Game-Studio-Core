using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Definitions.Rpg;
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
using NeonBlack.Gameplay.Features.Feedback;
using NeonBlack.Gameplay.Features.GameFlow;
using NeonBlack.Gameplay.Features.Pickups;
using NeonBlack.Gameplay.Features.Scoring;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace NeonBlack.Gameplay.Core.Runtime
{
    /// <summary>
    /// Runtime VContainer scope for the Pyralis gameplay platform.
    /// This is the singular source of truth for service resolution in the session.
    /// </summary>
    [AddComponentMenu("NeonBlack/Gameplay/Setup/Pyralis Gameplay Lifetime Scope")]
    [DisallowMultipleComponent]
    [AuthoringContract(
        Capability = AuthoringCapability.Setup, 
        Relevance = "Inspector Add Component path for the visible Pyralis runtime composition scope.", 
        Axioms = AuthoringWorldAxiom.None,
        AssignmentFields = new[] { nameof(InjectLoadedScenesOnBuild) },
        FirstProof = "Check the VContainer debugger to ensure all gameplay services are correctly registered in the scope.",
        NativeSetup = new[] { "Configure VContainer Resolver" }
    )]
    public class PyralisGameplayLifetimeScope : LifetimeScope
    {
        private const string NetworkedSessionStateServiceTypeName = "NeonBlack.Gameplay.Networking.Participants.NetworkedSessionStateService, NeonBlack.Gameplay.Networking";
        private const string NetworkedParticipantRosterServiceTypeName = "NeonBlack.Gameplay.Networking.Participants.NetworkedParticipantRosterService, NeonBlack.Gameplay.Networking";
        private const string NetworkedParticipantSpawnServiceTypeName = "NeonBlack.Gameplay.Networking.Participants.NetworkedParticipantSpawnService, NeonBlack.Gameplay.Networking";
        private const string NetworkedSessionOwnershipServiceTypeName = "NeonBlack.Gameplay.Networking.Runtime.NetworkedSessionOwnershipService, NeonBlack.Gameplay.Networking";
        private const string NetworkedParticipantAuthorityServiceTypeName = "NeonBlack.Gameplay.Networking.Runtime.NetworkedParticipantAuthorityService, NeonBlack.Gameplay.Networking";

        private bool _isConfigured;
        private SessionDefinition _sessionDefinition;
        private SessionStateService _sessionStateService;
        private ParticipantRosterService _participantRosterService;
        private ParticipantSpawnService _participantSpawnService;
        private ParticipantInputRouter _participantInputRouter;
        private MonoBehaviour _sceneNavigatorSource;
        private TimeManager _timeManager;
        private CameraShake _cameraShake;
        private CinemachineCameraRigController _cameraRigController;
        private ISessionOwnershipService _sessionOwnershipService;
        private IParticipantAuthorityService _participantAuthorityService;
        private PyralisRuntimeFeatureServicePolicy _featureServicePolicy;

        [Header("RPG Definitions")]
        [SerializeField] private ItemCatalogDefinition itemCatalog;
        [SerializeField] private ProgressionCurveDefinition progressionCurve;

        public bool InjectLoadedScenesOnBuild { get; set; } = true;
        public ParticipantRosterService ParticipantRosterService => _participantRosterService;

        public void ConfigureRuntime(
            SessionDefinition sessionDefinition,
            Transform[] spawnPoints,
            PlayerInputManager playerInputManager,
            MonoBehaviour sceneNavigatorSource,
            TimeManager timeManager,
            CameraShake cameraShake,
            CinemachineCameraRigController cameraRigController,
            SessionStateService sessionStateServiceOverride = null,
            ParticipantRosterService participantRosterServiceOverride = null,
            ParticipantSpawnService participantSpawnServiceOverride = null,
            ParticipantInputRouter participantInputRouterOverride = null)
        {
            _sessionDefinition = sessionDefinition;
            _sceneNavigatorSource = sceneNavigatorSource;
            _timeManager = timeManager;
            _cameraShake = cameraShake;
            _cameraRigController = cameraRigController;
            bool useNetcodeServices = sessionDefinition != null && sessionDefinition.networkMode != GameplayNetworkMode.LocalOnly;

            _sessionStateService = ResolveCoreComponent(
                sessionStateServiceOverride,
                "SessionStateService",
                useNetcodeServices ? NetworkedSessionStateServiceTypeName : null);
            _sessionStateService.SetSessionDefinition(sessionDefinition);

            _participantRosterService = ResolveCoreComponent(
                participantRosterServiceOverride,
                "ParticipantRosterService",
                useNetcodeServices ? NetworkedParticipantRosterServiceTypeName : null);
            _participantRosterService.SetSessionDefinition(sessionDefinition);

            _participantSpawnService = ResolveCoreComponent(
                participantSpawnServiceOverride,
                "ParticipantSpawnService",
                useNetcodeServices ? NetworkedParticipantSpawnServiceTypeName : null);
            _participantSpawnService.SetRosterService(_participantRosterService);
            _participantSpawnService.SetSessionStateService(_sessionStateService);
            _participantSpawnService.SetSpawnPoints(spawnPoints);
            _participantSpawnService.SetCameraBoundsProvider(cameraRigController);
            _participantSpawnService.SetPlayfieldBoundsProvider(sessionDefinition?.defaultGameMode?.playfieldProfile);

            _participantInputRouter = ResolveCoreComponent(
                participantInputRouterOverride,
                "ParticipantInputRouter",
                null);
            _participantInputRouter.SetSessionDefinition(sessionDefinition);
            _participantInputRouter.SetRosterService(_participantRosterService);
            _participantInputRouter.SetPlayerInputManager(playerInputManager);

            ParticipantQueryUtility.Initialize(_participantRosterService, _participantRosterService);
            _sessionOwnershipService = ResolveOrCreateSessionOwnershipService(useNetcodeServices);
            _participantAuthorityService = ResolveOrCreateParticipantAuthorityService(useNetcodeServices);
            _featureServicePolicy = PyralisRuntimeFeatureServicePolicy.Resolve(sessionDefinition);
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
            RegisterCoreSessionServices(builder);
            RegisterCoreSceneServices(builder);
            RegisterFeatureServices(builder);
            RegisterOwnershipServices(builder);
            RegisterSettingsServices(builder);

            builder.RegisterBuildCallback(container =>
            {
                if (InjectLoadedScenesOnBuild)
                    InjectLoadedSceneObjects(container);
            });
        }

        private void RegisterCoreSessionServices(IContainerBuilder builder)
        {
            if (_sessionDefinition != null)
                builder.RegisterInstance(_sessionDefinition).AsSelf();

            RegisterComponent(builder, _sessionStateService);
            RegisterComponent(builder, _participantRosterService);
            RegisterComponent(builder, _participantSpawnService);
            RegisterComponent(builder, _participantInputRouter);
        }

        private void RegisterCoreSceneServices(IContainerBuilder builder)
        {
            var timeManager = _timeManager != null ? _timeManager : FindServiceInHierarchy<TimeManager>();
            var cameraShake = _cameraShake != null ? _cameraShake : FindServiceInHierarchy<CameraShake>();

            RegisterSceneNavigator(builder);
            RegisterComponent(builder, timeManager);
            RegisterComponent(builder, cameraShake);
            RegisterComponent(builder, _cameraRigController);
        }

        private void RegisterSceneNavigator(IContainerBuilder builder)
        {
            ISceneNavigator navigator = _sceneNavigatorSource as ISceneNavigator;
            if (navigator == null && _sceneNavigatorSource != null)
                navigator = _sceneNavigatorSource.GetComponent<ISceneNavigator>();
            if (navigator == null)
                navigator = FindServiceInHierarchy<ISceneNavigator>();

            if (navigator is Component component)
                RegisterComponent(builder, component);
        }

        private void RegisterFeatureServices(IContainerBuilder builder)
        {
            bool usesCombatServices = _featureServicePolicy.UsesCombatServices
                || HasLoadedSceneComponent<PawnCombatBehaviour>()
                || HasLoadedSceneComponent<PawnCombatBehaviour2D>();
            bool usesEnemyServices = _featureServicePolicy.UsesEnemyServices
                || HasLoadedSceneComponent<EnemyAI>()
                || HasLoadedSceneComponent<BattleManager>();
            bool usesRpgServices = _featureServicePolicy.UsesRpgServices
                || HasLoadedSceneComponentInNamespace("NeonBlack.Gameplay.Features.Rpg");
            bool usesGameFlowServices = _featureServicePolicy.UsesGameFlowServices
                || HasLoadedSceneComponent<GameManager>()
                || HasLoadedSceneComponentInNamespace("NeonBlack.Gameplay.Features.GameFlow");
            bool usesScoringServices = _featureServicePolicy.UsesScoringServices
                || HasLoadedSceneComponent<ParticipantScoreService>()
                || HasLoadedSceneComponent<LeaderboardManager>()
                || HasLoadedSceneComponent<StillnessBonus2D>()
                || HasLoadedSceneComponent<CollectibleFeedback2D>();
            bool usesFeedbackServices = _featureServicePolicy.UsesFeedbackServices
                || HasLoadedSceneComponent<ParticipantFeedbackService>()
                || HasLoadedSceneComponentInNamespace("NeonBlack.Gameplay.Features.Feedback");

            if (usesCombatServices)
                RegisterCombatServices(builder);

            if (usesEnemyServices)
                RegisterEnemyServices(builder);

            if (usesRpgServices)
                RegisterRpgServices(builder);

            if (usesGameFlowServices)
                RegisterGameFlowServices(builder);

            if (usesScoringServices)
                RegisterScoringServices(builder);

            if (usesFeedbackServices)
                RegisterFeedbackServices(builder);
        }

        private static void RegisterCombatServices(IContainerBuilder builder)
        {
            builder.Register<PawnComboProcessor>(Lifetime.Transient);
            builder.Register<PawnDamageHandler>(Lifetime.Transient);
        }

        private void RegisterEnemyServices(IContainerBuilder builder)
        {
            builder.Register<EnemyDetectionService>(Lifetime.Singleton);
            builder.Register<EnemyCombatProcessor>(Lifetime.Singleton);
            RegisterComponent(builder, FindServiceInHierarchy<BattleManager>());
        }

        private void RegisterRpgServices(IContainerBuilder builder)
        {
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
        }

        private void RegisterGameFlowServices(IContainerBuilder builder)
        {
            RegisterComponent(builder, FindLoadedSceneComponent<GameManager>() ?? FindServiceInHierarchy<GameManager>());
        }

        private void RegisterScoringServices(IContainerBuilder builder)
        {
            RegisterComponent(builder, FindLoadedSceneComponent<ParticipantScoreService>() ?? FindServiceInHierarchy<ParticipantScoreService>());
            RegisterComponent(builder, FindLoadedSceneComponent<LeaderboardManager>() ?? FindServiceInHierarchy<LeaderboardManager>());
        }

        private void RegisterFeedbackServices(IContainerBuilder builder)
        {
            RegisterComponent(builder, FindLoadedSceneComponent<ParticipantFeedbackService>() ?? FindServiceInHierarchy<ParticipantFeedbackService>());
        }

        private void RegisterOwnershipServices(IContainerBuilder builder)
        {
            if (_sessionOwnershipService != null)
                builder.RegisterInstance<ISessionOwnershipService>(_sessionOwnershipService);

            if (_participantAuthorityService != null)
                builder.RegisterInstance<IParticipantAuthorityService>(_participantAuthorityService);
        }

        private void RegisterSettingsServices(IContainerBuilder builder)
        {
            var settingsApplier = FindServiceInHierarchy<IGameplaySettingsApplier>();
            if (settingsApplier != null)
                builder.RegisterInstance<IGameplaySettingsApplier>(settingsApplier);
            else
                builder.RegisterInstance<IGameplaySettingsApplier>(new NullGameplaySettingsApplier());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        private static void RegisterComponent<T>(IContainerBuilder builder, T component)
            where T : Component
        {
            if (component == null)
                return;

            builder.RegisterComponent(component).AsSelf().AsImplementedInterfaces();
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

        private static bool HasLoadedSceneComponent<T>() where T : Component
        {
            return FindLoadedSceneComponent<T>() != null;
        }

        private static T FindLoadedSceneComponent<T>() where T : Component
        {
            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded)
                    continue;

                GameObject[] roots = scene.GetRootGameObjects();
                for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
                {
                    if (roots[rootIndex] == null)
                        continue;

                    T component = roots[rootIndex].GetComponentInChildren<T>(true);
                    if (component != null)
                        return component;
                }
            }

            return null;
        }

        private static bool HasLoadedSceneComponentInNamespace(string namespacePrefix)
        {
            if (string.IsNullOrWhiteSpace(namespacePrefix))
                return false;

            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded)
                    continue;

                GameObject[] roots = scene.GetRootGameObjects();
                for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
                {
                    GameObject root = roots[rootIndex];
                    if (root == null)
                        continue;

                    MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
                    for (int i = 0; i < behaviours.Length; i++)
                    {
                        Type type = behaviours[i] != null ? behaviours[i].GetType() : null;
                        if (type != null
                            && type.Namespace != null
                            && type.Namespace.StartsWith(namespacePrefix, StringComparison.Ordinal))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private T FindServiceInHierarchy<T>() where T : class
        {
            return GetComponentInChildren<T>(true);
        }

        private T ResolveCoreComponent<T>(T overrideComponent, string serviceName, string preferredTypeName)
            where T : Component
        {
            if (overrideComponent != null)
                return overrideComponent;

            T existing = FindServiceInHierarchy<T>();
            if (existing != null)
                return existing;

            GameObject go = new GameObject(serviceName);
            go.transform.SetParent(transform, false);

            if (!string.IsNullOrWhiteSpace(preferredTypeName))
            {
                Type preferredType = Type.GetType(preferredTypeName);
                if (preferredType != null && typeof(T).IsAssignableFrom(preferredType) && typeof(Component).IsAssignableFrom(preferredType))
                    return (T)go.AddComponent(preferredType);

                Debug.LogWarning($"[PyralisGameplayLifetimeScope] Networked service type `{preferredTypeName}` was not found. Falling back to `{typeof(T).Name}`.", this);
            }

            return go.AddComponent<T>();
        }

        private static ISessionOwnershipService ResolveOrCreateSessionOwnershipService(bool useNetcodeServices)
        {
            if (useNetcodeServices && TryCreateServiceInstance(NetworkedSessionOwnershipServiceTypeName, out ISessionOwnershipService networkedService))
                return networkedService;

            return new LocalSessionOwnershipService();
        }

        private static IParticipantAuthorityService ResolveOrCreateParticipantAuthorityService(bool useNetcodeServices)
        {
            if (useNetcodeServices && TryCreateServiceInstance(NetworkedParticipantAuthorityServiceTypeName, out IParticipantAuthorityService networkedService))
                return networkedService;

            return new LocalParticipantAuthorityService();
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
    }
}
