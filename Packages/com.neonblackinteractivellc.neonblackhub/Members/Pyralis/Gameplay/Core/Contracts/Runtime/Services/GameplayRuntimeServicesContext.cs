namespace NeonBlack.Gameplay.Core.Contracts
{
    /// <summary>
    /// Runtime service handoff supplied by Glue to authored scene and prefab components.
    /// </summary>
    public readonly struct GameplayRuntimeServicesContext
    {
        public GameplayRuntimeServicesContext(
            IGameplayStateReader gameplayStateReader,
            ICameraBoundsProvider cameraBoundsProvider,
            IPlayfieldBoundsProvider playfieldBoundsProvider,
            IInputSettingsRegistrar inputSettingsRegistrar,
            ISessionScoreAwardSink sessionScoreAwardSink,
            IGameplayEventChannel eventChannel)
        {
            GameplayStateReader = gameplayStateReader;
            CameraBoundsProvider = cameraBoundsProvider;
            PlayfieldBoundsProvider = playfieldBoundsProvider;
            InputSettingsRegistrar = inputSettingsRegistrar;
            SessionScoreAwardSink = sessionScoreAwardSink;
            EventChannel = eventChannel;
        }

        public IGameplayStateReader GameplayStateReader { get; }
        public ICameraBoundsProvider CameraBoundsProvider { get; }
        public IPlayfieldBoundsProvider PlayfieldBoundsProvider { get; }
        public IInputSettingsRegistrar InputSettingsRegistrar { get; }
        public ISessionScoreAwardSink SessionScoreAwardSink { get; }
        public IGameplayEventChannel EventChannel { get; }
    }

    public interface IGameplayRuntimeServicesReceiver
    {
        void ApplyRuntimeServices(GameplayRuntimeServicesContext context);
    }
}
