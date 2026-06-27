using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Modules.Character
{
    public interface IPawnParticipantInitializer
    {
        void InitializeForParticipant(ParticipantHandle participant, GameModeDefinition gameMode);
    }

    public readonly struct PawnRuntimeServicesContext
    {
        public PawnRuntimeServicesContext(
            IGameplayStateReader gameplayStateReader,
            ICameraBoundsProvider cameraBoundsProvider,
            IPlayfieldBoundsProvider playfieldBoundsProvider)
        {
            GameplayStateReader = gameplayStateReader;
            CameraBoundsProvider = cameraBoundsProvider;
            PlayfieldBoundsProvider = playfieldBoundsProvider;
        }

        public IGameplayStateReader GameplayStateReader { get; }
        public ICameraBoundsProvider CameraBoundsProvider { get; }
        public IPlayfieldBoundsProvider PlayfieldBoundsProvider { get; }
    }

    public interface IPawnRuntimeServicesReceiver
    {
        void ApplyRuntimeServices(PawnRuntimeServicesContext context);
    }
}
