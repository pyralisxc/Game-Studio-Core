using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Presentation.Camera;
using NeonBlack.Gameplay.Presentation.Visuals;
using NeonBlack.Gameplay.Core.Contracts.Networking;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Runtime;
using NeonBlack.Gameplay.Features.Input;
using NeonBlack.Gameplay.Characters;
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
    [DisallowMultipleComponent]
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
            RegisterComponent(builder, _participantRosterService);
            RegisterComponent(builder, _participantSpawnService);
            RegisterComponent(builder, _participantInputRouter);
            RegisterComponent(builder, _sceneLoader);
            RegisterComponent(builder, _timeManager);
            RegisterComponent(builder, _cameraShake);
            RegisterComponent(builder, _cameraRigController);

            if (_participantRosterService != null)
            {
                builder.RegisterInstance<IParticipantRoster>(_participantRosterService);
                builder.RegisterInstance<IPlayerProvider>(_participantRosterService);
            }

            if (_sessionOwnershipService != null)
                builder.RegisterInstance<ISessionOwnershipService>(_sessionOwnershipService);

            if (_participantAuthorityService != null)
                builder.RegisterInstance<IParticipantAuthorityService>(_participantAuthorityService);

            builder.RegisterBuildCallback(container =>
            {
                if (_platformContext == null)
                    return;

                _platformContext.Services.Register<IObjectResolver>(container);
                _platformContext.Services.Register(container);
                if (InjectLoadedScenesOnBuild)
                    InjectLoadedSceneObjects(container);
            });
        }

        private static void RegisterComponent<T>(IContainerBuilder builder, T component)
            where T : Component
        {
            if (component == null)
                return;

            builder.RegisterComponent(component).AsSelf();
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
