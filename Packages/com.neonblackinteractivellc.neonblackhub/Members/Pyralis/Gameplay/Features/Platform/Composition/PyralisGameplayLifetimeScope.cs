using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NeonBlack.Gameplay.Presentation.Camera;
using NeonBlack.Gameplay.Presentation.Visuals;
using NeonBlack.Gameplay.Core.Contracts.Networking;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Runtime;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Features.Input;
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
        RoleTags = new[] { AuthoringContractRoleTags.IntentRouteEssential, AuthoringContractRoleTags.CoreRouteAnchor },
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
            _sessionStateService?.SetSessionDefinition(sessionDefinition);

            _participantRosterService = ResolveCoreComponent(
                participantRosterServiceOverride,
                "ParticipantRosterService",
                useNetcodeServices ? NetworkedParticipantRosterServiceTypeName : null);
            _participantRosterService?.SetSessionDefinition(sessionDefinition);

            _participantSpawnService = ResolveCoreComponent(
                participantSpawnServiceOverride,
                "ParticipantSpawnService",
                useNetcodeServices ? NetworkedParticipantSpawnServiceTypeName : null);

            if (_participantSpawnService != null)
            {
                _participantSpawnService.SetRosterService(_participantRosterService);
                _participantSpawnService.SetSessionStateService(_sessionStateService);
                _participantSpawnService.SetCameraBoundsProvider(cameraRigController);
                _participantSpawnService.SetPlayfieldBoundsProvider(sessionDefinition?.defaultGameMode?.playfieldProfile);
            }

            _participantInputRouter = ResolveCoreComponent(
                participantInputRouterOverride,
                "ParticipantInputRouter",
                null);

            if (_participantInputRouter != null)
            {
                _participantInputRouter.SetSessionDefinition(sessionDefinition);
                _participantInputRouter.SetRosterService(_participantRosterService);
                _participantInputRouter.SetPlayerInputManager(playerInputManager);
            }

            ParticipantQueryUtility.Initialize(_participantRosterService, _participantRosterService);
            _sessionOwnershipService = ResolveSessionOwnershipService(useNetcodeServices);
            _participantAuthorityService = ResolveParticipantAuthorityService(useNetcodeServices);
            _featureServicePolicy = PyralisRuntimeFeatureServicePolicy.ResolveWithLoadedSceneEvidence(sessionDefinition);
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
            PyralisFeatureServiceInstaller.RegisterFeatureServices(
                builder,
                _featureServicePolicy,
                this,
                itemCatalog,
                progressionCurve);
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
            {
                if (!MatchesPreferredCoreServiceType(existing, preferredTypeName, out string preferredDisplayName))
                {
                    Debug.LogError($"[PyralisGameplayLifetimeScope] Core service `{serviceName}` is authored as `{existing.GetType().Name}`, but this route expects `{preferredDisplayName}`. Replace the authored component or assign the correct Bootstrap override before Play Mode.", this);
                    return null;
                }

                return existing;
            }

            Debug.LogError($"[PyralisGameplayLifetimeScope] Critical core service `{serviceName}` is missing from the scene. Add an authored child GameObject under the Gameplay Root with `{typeof(T).Name}`, or assign the Bootstrap override field before Play Mode.", this);
            return null;
        }

        private static bool MatchesPreferredCoreServiceType<T>(T existing, string preferredTypeName, out string preferredDisplayName)
            where T : Component
        {
            preferredDisplayName = typeof(T).Name;
            if (existing == null || string.IsNullOrWhiteSpace(preferredTypeName))
                return true;

            Type preferredType = Type.GetType(preferredTypeName);
            if (preferredType != null)
            {
                preferredDisplayName = preferredType.Name;
                return preferredType.IsInstanceOfType(existing);
            }

            int assemblySeparator = preferredTypeName.IndexOf(',');
            string preferredFullName = assemblySeparator >= 0
                ? preferredTypeName.Substring(0, assemblySeparator).Trim()
                : preferredTypeName.Trim();
            preferredDisplayName = string.IsNullOrWhiteSpace(preferredFullName)
                ? typeof(T).Name
                : preferredFullName.Substring(preferredFullName.LastIndexOf('.') + 1);

            return string.Equals(existing.GetType().FullName, preferredFullName, StringComparison.Ordinal);
        }

        private static ISessionOwnershipService ResolveSessionOwnershipService(bool useNetcodeServices)
        {
            if (useNetcodeServices && TryCreateServiceInstance(NetworkedSessionOwnershipServiceTypeName, out ISessionOwnershipService networkedService))
                return networkedService;

            return new LocalSessionOwnershipService();
        }

        private static IParticipantAuthorityService ResolveParticipantAuthorityService(bool useNetcodeServices)
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
