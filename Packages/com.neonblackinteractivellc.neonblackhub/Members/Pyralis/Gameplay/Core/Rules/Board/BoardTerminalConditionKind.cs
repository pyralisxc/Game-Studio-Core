namespace NeonBlack.Gameplay.Core.Rules.Board
{
    /// <summary>
    /// Baseline board-game terminal conditions that can be authored without custom code.
    /// </summary>
    public enum BoardTerminalConditionKind
    {
        SideEliminated = 0,
        ObjectiveOccupied = 1
    }
}
