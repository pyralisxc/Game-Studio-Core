namespace NeonBlack.Gameplay.Data.Definitions
{
    /// <summary>
    /// Describes whether a runtime pattern expects a pawn-backed participant.
    /// </summary>
    public enum ParticipantEmbodimentRequirement
    {
        NoneRequired,
        OptionalPawn,
        RequiredPawn,
        NonPawnSurfaceRequired,
        Custom
    }
}
