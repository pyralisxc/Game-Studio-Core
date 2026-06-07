namespace NeonBlack.Gameplay.Core.Rules.Board
{
    /// <summary>
    /// Runtime evaluator for common board and tabletop win conditions.
    /// </summary>
    public sealed class BoardTerminalCondition : IBoardTerminalCondition
    {
        public BoardTerminalCondition(
            string conditionId,
            BoardTerminalConditionKind kind,
            int observedSeat,
            int winningSeat,
            BoardCoordinate objectiveCoordinate = default)
        {
            ConditionId = string.IsNullOrWhiteSpace(conditionId) ? "condition.boardTerminal" : conditionId;
            Kind = kind;
            ObservedSeat = observedSeat;
            WinningSeat = winningSeat;
            ObjectiveCoordinate = objectiveCoordinate;
        }

        public string ConditionId { get; }
        public BoardTerminalConditionKind Kind { get; }
        public int ObservedSeat { get; }
        public int WinningSeat { get; }
        public BoardCoordinate ObjectiveCoordinate { get; }

        public BoardTerminalEvaluationResult Evaluate(BoardRuntimeState boardState)
        {
            if (boardState == null)
                return BoardTerminalEvaluationResult.InProgress($"Condition `{ConditionId}` requires board state.");

            switch (Kind)
            {
                case BoardTerminalConditionKind.SideEliminated:
                    return EvaluateSideEliminated(boardState);
                case BoardTerminalConditionKind.ObjectiveOccupied:
                    return EvaluateObjectiveOccupied(boardState);
                default:
                    return BoardTerminalEvaluationResult.InProgress($"Condition `{ConditionId}` has unsupported kind `{Kind}`.");
            }
        }

        private BoardTerminalEvaluationResult EvaluateSideEliminated(BoardRuntimeState boardState)
        {
            if (ObservedSeat < 0 || WinningSeat < 0)
                return BoardTerminalEvaluationResult.InProgress($"Condition `{ConditionId}` requires valid observed and winning seats.");

            foreach (BoardPieceState piece in boardState.Pieces)
            {
                if (!piece.IsCaptured && piece.OwnerSeat == ObservedSeat)
                    return BoardTerminalEvaluationResult.InProgress($"Condition `{ConditionId}` is still in progress.");
            }

            return BoardTerminalEvaluationResult.Terminal(
                WinningSeat,
                $"Condition `{ConditionId}` completed because seat {ObservedSeat} has no active pieces.");
        }

        private BoardTerminalEvaluationResult EvaluateObjectiveOccupied(BoardRuntimeState boardState)
        {
            if (!boardState.TryGetPieceAt(ObjectiveCoordinate, out BoardPieceState piece) || piece.IsCaptured)
                return BoardTerminalEvaluationResult.InProgress($"Condition `{ConditionId}` objective is not occupied.");

            if (ObservedSeat >= 0 && piece.OwnerSeat != ObservedSeat)
                return BoardTerminalEvaluationResult.InProgress($"Condition `{ConditionId}` objective is occupied by seat {piece.OwnerSeat}, not observed seat {ObservedSeat}.");

            int winner = WinningSeat >= 0 ? WinningSeat : piece.OwnerSeat;
            return BoardTerminalEvaluationResult.Terminal(
                winner,
                $"Condition `{ConditionId}` completed because seat {piece.OwnerSeat} occupies objective `{ObjectiveCoordinate}`.");
        }
    }
}
