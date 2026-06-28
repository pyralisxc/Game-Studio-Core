using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Participants
{
    public interface IPawnParticipantInitializer
    {
        void InitializeForParticipant(ParticipantHandle participant, GameModeDefinition gameMode);
    }

    public interface IPawnParticipantStateReader
    {
        ParticipantHandle Participant { get; }
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

    public interface IPawnInputModule
    {
        void ApplyInputProfile(PawnProfileApplicationContext context, InputProfile inputProfile);
    }

    [AuthoringContract(
        Category = "Combat",
        Surface = AuthoringSurface.Goal,
        Summary = "Applies pawn combat profile data to a runtime combat module.",
        SetupSteps = new[] { "Implement interface in a combat module" },
        SuccessChecks = new[] { "Verify that ApplyCombatProfile is called when the pawn is initialized." },
        Tags = new[] { "capability:Combat" }
    )]
    public interface IPawnCombatModule
    {
        void ApplyCombatProfile(PawnProfileApplicationContext context, PawnCombatProfile combatProfile);
    }
}
