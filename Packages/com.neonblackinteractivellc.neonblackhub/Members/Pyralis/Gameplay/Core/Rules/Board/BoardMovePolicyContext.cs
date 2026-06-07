namespace NeonBlack.Gameplay.Core.Rules.Board
{
    /// <summary>
    /// Runtime data required to evaluate whether a logical board move is legal.
    /// </summary>
    public readonly struct BoardMovePolicyContext
    {
        public BoardMovePolicyContext(
            BoardRuntimeState boardState,
            BoardPieceState movingPiece,
            BoardCoordinate destination,
            BoardPieceState destinationPiece = null)
        {
            BoardState = boardState;
            MovingPiece = movingPiece;
            Destination = destination;
            DestinationPiece = destinationPiece;
        }

        public BoardRuntimeState BoardState { get; }
        public BoardPieceState MovingPiece { get; }
        public BoardCoordinate Destination { get; }
        public BoardPieceState DestinationPiece { get; }
    }
}
