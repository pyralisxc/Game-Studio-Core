namespace NeonBlack.Gameplay.Core.Contracts
{
    /// <summary>
    /// Runtime service handoff supplied by Glue to authored scene and prefab components.
    /// </summary>
    public readonly struct GameplayRuntimeServicesContext
    {
        public GameplayRuntimeServicesContext(
            IGameplayStateReader gameplayStateReader,
            IGameplaySessionFlow gameplaySessionFlow,
            ICameraBoundsProvider cameraBoundsProvider,
            IPlayfieldBoundsProvider playfieldBoundsProvider,
            IInputSettingsRegistrar inputSettingsRegistrar,
            ISessionScoreService sessionScoreService,
            ISessionScoreAwardSink sessionScoreAwardSink,
            IGameplayEventChannel eventChannel)
        {
            GameplayStateReader = gameplayStateReader;
            GameplaySessionFlow = gameplaySessionFlow;
            CameraBoundsProvider = cameraBoundsProvider;
            PlayfieldBoundsProvider = playfieldBoundsProvider;
            InputSettingsRegistrar = inputSettingsRegistrar;
            SessionScoreService = sessionScoreService;
            SessionScoreAwardSink = sessionScoreAwardSink;
            EventChannel = eventChannel;
        }

        public IGameplayStateReader GameplayStateReader { get; }
        public IGameplaySessionFlow GameplaySessionFlow { get; }
        public ICameraBoundsProvider CameraBoundsProvider { get; }
        public IPlayfieldBoundsProvider PlayfieldBoundsProvider { get; }
        public IInputSettingsRegistrar InputSettingsRegistrar { get; }
        public ISessionScoreService SessionScoreService { get; }
        public ISessionScoreAwardSink SessionScoreAwardSink { get; }
        public IGameplayEventChannel EventChannel { get; }
    }

    public interface IGameplayRuntimeServicesReceiver
    {
        void ApplyRuntimeServices(GameplayRuntimeServicesContext context);
    }
}
