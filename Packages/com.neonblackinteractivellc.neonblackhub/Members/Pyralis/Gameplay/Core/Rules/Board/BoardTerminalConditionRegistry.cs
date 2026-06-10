using System;

namespace NeonBlack.Gameplay.Core.Rules.Board
{
    /// <summary>
    /// Registry of standard tabletop terminal conditions.
    /// </summary>
    public static class BoardTerminalConditionRegistry
    {
        public static IBoardTerminalCondition EliminateOpponent(int opponentSeat, int winnerSeat) =>
            new BoardTerminalCondition("condition.eliminateOpponent", BoardTerminalConditionKind.SideEliminated, opponentSeat, winnerSeat);

        public static IBoardTerminalCondition CaptureObjective(BoardCoordinate coordinate, int winningSeat) =>
            new BoardTerminalCondition("condition.captureObjective", BoardTerminalConditionKind.ObjectiveOccupied, -1, winningSeat, coordinate);
            
        public static IBoardTerminalCondition ReachEndZone(int seat, int winningSeat, BoardCoordinate coordinate) =>
            new BoardTerminalCondition("condition.reachEndZone", BoardTerminalConditionKind.ObjectiveOccupied, seat, winningSeat, coordinate);
    }
}