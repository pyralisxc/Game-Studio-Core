using NeonBlack.Gameplay.Data.Definitions;

namespace NeonBlack.Gameplay.Characters
{
    public interface IPawnParticipantInitializer
    {
        void InitializeForParticipant(ParticipantHandle participant, GameModeDefinition gameMode);
    }
}
