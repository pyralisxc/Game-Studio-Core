namespace NeonBlack.Gameplay.Core.Rules.Board
{
    /// <summary>
    /// Result of evaluating whether a board game has reached a terminal state.
    /// </summary>
    public readonly struct BoardTerminalEvaluationResult
    {
        private BoardTerminalEvaluationResult(bool isTerminal, int winningSeat, string message)
        {
            IsTerminal = isTerminal;
            WinningSeat = winningSeat;
            Message = message ?? string.Empty;
        }

        public bool IsTerminal { get; }
        public int WinningSeat { get; }
        public string Message { get; }

        public static BoardTerminalEvaluationResult InProgress(string message = "")
        {
            return new BoardTerminalEvaluationResult(false, -1, message);
        }

        public static BoardTerminalEvaluationResult Terminal(int winningSeat, string message)
        {
            return new BoardTerminalEvaluationResult(true, winningSeat, message);
        }
    }
}
