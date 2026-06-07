namespace NeonBlack.Gameplay.Core.Rules.Board
{
    /// <summary>
    /// Service boundary for systems that need logical board state without owning board rules.
    /// </summary>
    public interface IBoardStateService
    {
        BoardRuntimeState BoardState { get; }
        bool TryGetPieceAt(BoardCoordinate coordinate, out BoardPieceState piece);
        bool TryMovePiece(string pieceId, BoardCoordinate destination, out string issue);
        bool TryCapturePiece(string pieceId, out string issue);
    }
}
