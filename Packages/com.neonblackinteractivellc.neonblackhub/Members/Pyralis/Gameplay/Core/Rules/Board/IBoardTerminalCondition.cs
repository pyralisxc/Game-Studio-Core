namespace NeonBlack.Gameplay.Core.Rules.Board
{
    /// <summary>
    /// Evaluates whether a board state should end the game or round.
    /// </summary>
    public interface IBoardTerminalCondition
    {
        string ConditionId { get; }
        BoardTerminalEvaluationResult Evaluate(BoardRuntimeState boardState);
    }
}
