namespace NeonBlack.Gameplay.Core.Rules.Board
{
    /// <summary>
    /// Common grid movement shapes used by authorable board move policies.
    /// </summary>
    public enum BoardMoveShape
    {
        Any = 0,
        Orthogonal = 1,
        Diagonal = 2,
        OrthogonalOrDiagonal = 3,
        Adjacent = 4,
        Offset = 5
    }
}
