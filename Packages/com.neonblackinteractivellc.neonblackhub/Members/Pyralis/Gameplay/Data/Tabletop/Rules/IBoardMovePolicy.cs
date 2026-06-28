namespace NeonBlack.Gameplay.Data.Tabletop
{
    /// <summary>
    /// Evaluates whether a piece can move to a destination on the current board.
    /// </summary>
    public interface IBoardMovePolicy
    {
        string PolicyId { get; }
        bool AllowsCapture { get; }
        bool IsMoveAllowed(BoardMovePolicyContext context, out string issue);
    }
}
